﻿using System.Reflection;
using ASureBus.Abstractions;
using ASureBus.Core;
using ASureBus.Services.StorageAccount;

namespace ASureBus.Accessories.Heavy;

internal static class HeavyIo
{
    private static IAzureDataStorageService _storage = null!;

    internal static void ConfigureStorage(IAzureDataStorageService storage)
    {
        _storage = storage;
    }

    public static bool IsHeavyConfigured()
    {
        return AsbConfiguration.UseHeavyProperties;
    }

    private static void GuardAgainstNotConfigured()
    {
        if (!IsHeavyConfigured()) throw new Exception("Heavies not configured");
    }

    public static async Task<IReadOnlyList<HeavyReference>> Unload<TMessage>(
        TMessage message, Guid messageId,
        CancellationToken cancellationToken = default)
        where TMessage : IAmAMessage
    {
        GuardAgainstNotConfigured();

        var heavyProps = GetMessageHeavies(message);

        var heaviesRef = new List<HeavyReference>();
        if (heavyProps.Length == 0) return heaviesRef;

        foreach (var heavyProp in heavyProps)
        {
            var heavy = heavyProp.GetValue(message);
            var heavyId = (heavy as Abstractions.Heavy)!.Ref;

            await _storage.Save(heavy,
                    AsbConfiguration.HeavyProps?.DataStorageContainer!,
                    GetBlobName(messageId, heavyId),
                    false,
                    cancellationToken)
                .ConfigureAwait(false);

            heavyProp.SetValue(message, null);

            heaviesRef.Add(new HeavyReference
            {
                PropertyName = heavyProp.Name,
                Ref = heavyId
            });
        }

        return heaviesRef;
    }

    private static PropertyInfo[] GetMessageHeavies<TMessage>(TMessage message)
        where TMessage : IAmAMessage
    {
        return message
            .GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(prop =>
                prop.PropertyType.IsGenericType &&
                prop.PropertyType.GetGenericTypeDefinition() ==
                typeof(Heavy<>))
            .ToArray();
    }

    public static async Task Load(object message,
        IReadOnlyList<HeavyReference> heavies,
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        GuardAgainstNotConfigured();

        foreach (var heavyRef in heavies)
        {
            var prop = message
                .GetType()
                .GetProperties()
                .FirstOrDefault(prop =>
                    prop.Name.Equals(heavyRef.PropertyName));

            var propType = prop?.PropertyType.GetGenericArguments()
                .First();

            var heavyGenericType = typeof(Heavy<>).MakeGenericType(propType!);

            var value = await _storage.Get(
                    AsbConfiguration.HeavyProps?.DataStorageContainer!,
                    GetBlobName(messageId, heavyRef.Ref),
                    heavyGenericType,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            prop?.SetValue(message, value);
        }
    }

    public static async Task Delete(Guid messageId, HeavyReference heavyReference,
        CancellationToken cancellationToken = default)
    {
        GuardAgainstNotConfigured();

        await _storage.Delete(
                AsbConfiguration.HeavyProps?.DataStorageContainer!,
                GetBlobName(messageId, heavyReference.Ref),
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static string GetBlobName(Guid messageId, Guid heavyId)
    {
        return string.Join('-', messageId, heavyId);
    }
}
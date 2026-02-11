using System.Reflection;
using ASureBus.Abstractions;
using ASureBus.Accessories.Heavies;
using ASureBus.Accessories.Heavies.Entities;
using ASureBus.IO.StorageAccount;

namespace ASureBus.IO.Heavies;

internal sealed class HeavyIO(IAzureDataStorageService storage, IExpirableHeaviesObserver expirableHeaviesObserver) : IHeavyIO
{
    public bool IsHeavyConfigured => AsbConfiguration.UseHeavyProperties;
    
    private void GuardAgainstNotConfigured()
    {
        if (!IsHeavyConfigured) throw new Exception("Heavies not configured");
    }

    public async Task<IReadOnlyList<HeavyReference>> Unload<TMessage>(
        TMessage message, Guid messageId,
        CancellationToken cancellationToken = default)
        where TMessage : IAmAMessage
    { 
        GuardAgainstNotConfigured();

        var heavyProps = GetHeaviesInMessage(message);

        var heaviesRef = new List<HeavyReference>();
        if (heavyProps.Length == 0) return heaviesRef;

        foreach (var heavyProp in heavyProps)
        {
            var value = heavyProp.GetValue(message);
            var heavy = value as Heavy;
            var heavyId = heavy!.Ref;

            await storage.Save(value,
                    AsbConfiguration.HeavyProps?.Container!,
                    GetBlobName(messageId, heavyId),
                    false,
                    cancellationToken)
                .ConfigureAwait(false);

            if (heavy.HasExpiration)
            {
                expirableHeaviesObserver.DeleteOnExpiration(heavy, messageId, this, cancellationToken);
            }

            heavyProp.SetValue(message, null);

            heaviesRef.Add(new HeavyReference
            {
                PropertyName = heavyProp.Name,
                Ref = heavyId
            });
        }

        return heaviesRef;
    }

    private static PropertyInfo[] GetHeaviesInMessage<TMessage>(TMessage message)
        where TMessage : IAmAMessage
    {
        return message
            .GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(prop =>
                prop.PropertyType.IsGenericType &&
                prop.PropertyType.GetGenericTypeDefinition() == typeof(Heavy<>))
            .ToArray();
    }

    public async Task Load(object message,
        IReadOnlyList<HeavyReference> heavies,
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        GuardAgainstNotConfigured();

        foreach (var heavyRef in heavies)
        {
            var prop = GetReferencedProperty(message, heavyRef);

            var propType = prop?.PropertyType.GetGenericArguments().First();

            var heavyGenericType = typeof(Heavy<>).MakeGenericType(propType!);

            var value = await storage.Get(
                    AsbConfiguration.HeavyProps?.Container!,
                    GetBlobName(messageId, heavyRef.Ref),
                    heavyGenericType,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            prop?.SetValue(message, value);
        }
    }

    private static PropertyInfo? GetReferencedProperty(object message, HeavyReference heavyRef)
    {
        return message
            .GetType()
            .GetProperties()
            .FirstOrDefault(prop =>
                prop.PropertyType.IsGenericType
                && 
                prop.PropertyType.GetGenericTypeDefinition() == typeof(Heavy<>)
                &&
                prop.Name.Equals(heavyRef.PropertyName));
    }

    public async Task Delete(Guid messageId, Guid heavyReference,
        CancellationToken cancellationToken = default)
    {
        GuardAgainstNotConfigured();

        await storage.Delete(
                AsbConfiguration.HeavyProps?.Container!,
                GetBlobName(messageId, heavyReference),
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static string GetBlobName(Guid messageId, Guid heavyId)
    {
        return string.Join('-', messageId, heavyId);
    }
}
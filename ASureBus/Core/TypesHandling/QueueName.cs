namespace ASureBus.Core.TypesHandling;

public static class QueueName
{
    public static string Resolve(Type type)
    {
        var isGeneric = type.IsGenericType;

        return isGeneric
            ? type.Name.Split('`')[0] + "_" + string.Join('_', type.GenericTypeArguments.Select(x => x.Name))
            : type.Name;
    }
}
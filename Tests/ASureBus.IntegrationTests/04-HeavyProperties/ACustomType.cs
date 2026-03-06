namespace ASureBus.IntegrationTests._04_HeavyProperties;

internal sealed class ACustomType(string prop1, int prop2, bool prop3)
{
    public string Prop1 { get; init; } = prop1;
    public int Prop2 { get; init; } = prop2;
    public bool Prop3 { get; init; } = prop3;

    public override bool Equals(object? obj)
    {
        if (obj is not ACustomType other) return false;

        return GetHashCode() == other.GetHashCode();
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Prop1, Prop2, Prop3);
    }
}
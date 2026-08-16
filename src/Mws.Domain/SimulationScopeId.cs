namespace Mws.Domain;

public readonly record struct SimulationScopeId
{
    public SimulationScopeId(ulong value)
    {
        if (value == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Simulation scope ID must be non-zero.");
        }

        Value = value;
    }

    public ulong Value { get; }

    public static SimulationScopeId Root { get; } = new(1);
}

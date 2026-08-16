namespace Mws.Domain;

public readonly record struct SimulationScopeId(ulong Value)
{
    public static SimulationScopeId Root { get; } = new(1);

    public SimulationScopeId
    {
        if (Value == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Value), Value, "Simulation scope ID must be non-zero.");
        }
    }
}

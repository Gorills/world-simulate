namespace Mws.Domain;

public readonly record struct SimulationScopeId(ulong Value)
{
    public static SimulationScopeId Root { get; } = new(1);
}

namespace Mws.Domain;

public readonly record struct SimulationTime(long Milliseconds)
{
    public SimulationTime AddMilliseconds(long milliseconds) => new(checked(Milliseconds + milliseconds));
}

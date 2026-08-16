namespace Mws.Domain;

public readonly record struct WorldTick(long Value)
{
    public WorldTick Next() => new(checked(Value + 1));
}

using Godot;
using Mws.Client.Godot.Session;

namespace Mws.Client.Godot.App;

public partial class PlaytestClock : Node
{
    private GameSession? _session;
    private double _elapsedSeconds;
    private long _observedTimeMilliseconds;

    internal void Bind(GameSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _elapsedSeconds = 0.0;
        _observedTimeMilliseconds = session.Time.Milliseconds;
    }

    public override void _Process(double delta)
    {
        if (_session is null)
        {
            return;
        }

        if (_session.Time.Milliseconds != _observedTimeMilliseconds)
        {
            _observedTimeMilliseconds = _session.Time.Milliseconds;
            _elapsedSeconds = 0.0;
        }

        _elapsedSeconds += delta;
        if (_elapsedSeconds < PlaytestTimeProfile.RealSecondsPerGameHour)
        {
            return;
        }

        var elapsedHours = Math.Max(
            1,
            (int)(_elapsedSeconds / PlaytestTimeProfile.RealSecondsPerGameHour));
        _elapsedSeconds -= elapsedHours * PlaytestTimeProfile.RealSecondsPerGameHour;
        _session.AdvanceHours(elapsedHours);
        _observedTimeMilliseconds = _session.Time.Milliseconds;
    }
}

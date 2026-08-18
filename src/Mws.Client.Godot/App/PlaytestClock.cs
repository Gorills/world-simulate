using Godot;
using Mws.Client.Godot.Session;

namespace Mws.Client.Godot.App;

public partial class PlaytestClock : Node
{
    private const double ActiveTravelSampleSeconds = 0.25;
    private const long ActiveTravelSampleMilliseconds = 15_000;

    private GameWorldSession? _session;
    private double _elapsedSeconds;
    private long _observedTimeMilliseconds;
    private bool _activeTravelSampling;

    internal void Bind(GameWorldSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _elapsedSeconds = 0.0;
        _observedTimeMilliseconds = session.Time.Milliseconds;
        _activeTravelSampling = false;
    }

    internal void BeginActiveTravelSampling()
    {
        if (_session is null)
        {
            throw new InvalidOperationException("Playtest clock is not bound to a session.");
        }

        _elapsedSeconds = 0.0;
        _observedTimeMilliseconds = _session.Time.Milliseconds;
        _activeTravelSampling = true;
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

        if (_session.TravelPlaytestPending)
        {
            return;
        }

        _elapsedSeconds += delta;
        if (_activeTravelSampling)
        {
            while (_elapsedSeconds >= ActiveTravelSampleSeconds)
            {
                _elapsedSeconds -= ActiveTravelSampleSeconds;
                _activeTravelSampling = _session.AdvanceActiveTravelSample(
                    ActiveTravelSampleMilliseconds);
                _observedTimeMilliseconds = _session.Time.Milliseconds;
                if (!_activeTravelSampling)
                {
                    _elapsedSeconds = 0.0;
                    break;
                }
            }

            return;
        }

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

using System.Diagnostics;
using Akka.Actor;
using Akka.Persistence.Snapshot;

namespace Akka.Persistence.MongoDb.Benchmark;

public interface ICommand { }

public sealed class WriteSnapshot: ICommand
{
    public static readonly WriteSnapshot Instance = new ();
    private WriteSnapshot() { }
}

public sealed class ReadSnapshot: ICommand
{
    public static readonly ReadSnapshot Instance = new ();
    private ReadSnapshot() { }
}

public sealed class Result
{
    public Exception? Cause { get; set; }
    public TimeSpan Duration { get; set; } = TimeSpan.Zero;
}

public class BenchmarkActor: UntypedPersistentActor
{
    private readonly Random _rnd;
    private readonly Stopwatch _stopwatch;
    private readonly IActorRef _store;
    private readonly LoadSnapshot _loadRequest;
    private readonly byte[] _staticPayload;
    private readonly bool _useStaticPayload;
    private IActorRef? _sender;
    
    public BenchmarkActor(byte[] payload, bool useStaticPayload)
    {
        _rnd = new Random();
        _stopwatch = new Stopwatch();
        _store = Persistence.Instance.Apply(Context.System).SnapshotStoreFor(null);
        _loadRequest = new LoadSnapshot("benchmark", SnapshotSelectionCriteria.Latest, long.MaxValue);
        _staticPayload = payload;
        _useStaticPayload = useStaticPayload;
    }
    
    public override string PersistenceId => "benchmark";
    
    protected override void OnCommand(object message)
    {
        switch (message)
        {
            case WriteSnapshot:
                _sender = Sender;

                if (_useStaticPayload)
                {
                    _stopwatch.Restart();
                    SaveSnapshot(_staticPayload);
                }
                else
                {
                    var payload = new byte[16_384_000];

                    for (var i = 0; i < 16_384_000; i++)
                    {
                        payload[i] = (byte)(_rnd.Next(0, 255));
                    }
                    _stopwatch.Restart();
                    SaveSnapshot(payload);
                }
                break;
            
            case SaveSnapshotSuccess:
                _stopwatch.Stop();
                _sender.Tell(new Result
                {
                    Duration = _stopwatch.Elapsed,
                });
                _sender = null;
                break;
            
            case SaveSnapshotFailure fail:
                _stopwatch.Stop();
                _sender.Tell(new Result
                {
                    Cause = fail.Cause,
                });
                _sender = null;
                break;
            
            case ReadSnapshot:
                _sender = Sender;
                _stopwatch.Restart();
                _store.Tell(_loadRequest, Self);
                break;
            
            case LoadSnapshotResult:
                _stopwatch.Stop();
                _sender.Tell(new Result
                {
                    Duration = _stopwatch.Elapsed,
                });
                _sender = null;
                break;
            
            case LoadSnapshotFailed fail:
                _stopwatch.Stop();
                _sender.Tell(new Result
                {
                    Cause = fail.Cause,
                });
                _sender = null;
                break;
            
            default:
                Unhandled(message);
                break;
        }
    }

    protected override void OnRecover(object message)
    {
        // no-op
    }
}
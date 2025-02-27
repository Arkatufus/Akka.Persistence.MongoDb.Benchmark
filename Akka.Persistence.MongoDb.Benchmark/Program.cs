using System.Text;
using Akka.Actor;
using Akka.Hosting;
using Akka.Persistence.Hosting;
using Akka.Persistence.MongoDb.Benchmark;
using Akka.Persistence.MongoDb.Hosting;
using MathNet.Numerics.Statistics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mongo2Go;

const bool useTransactions = false;
const bool useDynamicPayload = false;
const int opCount = 1000;

var payload = new byte[16_384_000];

for (var i = 0; i < 16_384_000; i++)
{
    payload[i] = (byte)(i % 256);
}

var mongo = MongoDbRunner.Start(singleNodeReplSet: true);
var s = mongo.ConnectionString.Split('?');
var connectionString = s[0] + "akkanet?" + s[1];

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(builder =>
    {
        builder.ClearProviders();
        builder.AddConsole();
    })
    .ConfigureServices(services =>
    {
        services.AddAkka("Benchmark", builder =>
        {
            var mongoOptions = new MongoDbSnapshotOptions(true)
            {
                ConnectionString = connectionString, 
                UseWriteTransaction = useTransactions
            };
            
            builder.WithSnapshot(mongoOptions);
        });
    }).Build();

await host.StartAsync();

var sys = host.Services.GetRequiredService<ActorSystem>();
var benchActor = sys.ActorOf(Props.Create(() => new BenchmarkActor(payload, useDynamicPayload)), "benchmark-actor");

var writeResults = new Result[opCount];
for (var i = 0; i < opCount; i++)
{
    if(i%100 == 0)
        Console.WriteLine($"Write {i} messages");
    var result = await benchActor.Ask<Result>(WriteSnapshot.Instance);
    writeResults[i] = result;
}

var readResults = new Result[opCount];
for (var i = 0; i < opCount; i++)
{
    if(i%100 == 0)
        Console.WriteLine($"Read {i} messages");
    var result = await benchActor.Ask<Result>(ReadSnapshot.Instance);
    readResults[i] = result;
}

var sb = new StringBuilder();
sb.AppendLine("| Operation | Count | Mean | Median | Standard Deviation | Rejected Outliers | Failures |");
sb.AppendLine("|-- | --| --| --| --| --| --|");
sb.AppendLine(Measure("SaveSnapshot", writeResults));
sb.AppendLine(Measure("LoadSnapshot", readResults));

Console.WriteLine(sb.ToString());

await host.StopAsync();
host.Dispose();
mongo.Dispose();

return 0;

string Measure(string operation, Result[] results)
{
    var measurements = results.Where(r => r.Cause is null).Select(r => r.Duration.TotalMilliseconds).ToArray();
    var (rejected, times) = RejectOutliers(measurements, 2.0);
    var fails = writeResults.Where(r => r.Cause is not null).ToArray();
    var mean = times.Average();
    var stdDev = times.PopulationStandardDeviation();
    // var min = times.Minimum();
    // var q1 = times.LowerQuartile();
    var median = times.Median();
    // var q3 = times.UpperQuartile();
    // var max = times.Maximum();

    return $"| {operation} | {opCount} | {mean:F3} ms | {median:F3} ms | {stdDev:F3} ms | {rejected.Count} | {fails.Length} |";
}

(IReadOnlyList<double> Rejected, IReadOnlyList<double> Measurements) RejectOutliers(IReadOnlyList<double> measurements, double sigma)
{
    var mean = measurements.Average();
    var stdDev = measurements.PopulationStandardDeviation();
    var threshold = sigma * stdDev;
    var minThreshold = mean - threshold;
    var maxThreshold = mean + threshold;
    var rejected = measurements.Where(m => m < minThreshold || m > maxThreshold);
    var accepted = measurements.Where(m => m >= minThreshold && m <= maxThreshold);
    return (rejected.ToArray(), accepted.ToArray());
}

using Externalscaler;
using Grpc.Net.Client;

Console.WriteLine("Hello, World! Hit enter to continue.");
Console.ReadLine();
Console.WriteLine("Hit Ctrl-C to exit.");

using var channel = GrpcChannel.ForAddress("http://localhost:5003");
var client = new ExternalScaler.ExternalScalerClient(channel);
var metricName = "grainsPerSilo";
var scaledObjectRef = new ScaledObjectRef
{
    Name = metricName,
    Namespace = "default",
    ScalerMetadata = {
        { "graintype", "customergrain" },
        { "upperbound", "100" },
        { "siloNameFilter", "api" }
    }
};

while (true)
{
    var getMetricSpecResponse = await client.GetMetricSpecAsync(scaledObjectRef);
    Console.WriteLine("GetMetricSpecAsync: " + getMetricSpecResponse.MetricSpecs);

    var getMetricsResponse = await client.GetMetricsAsync(new GetMetricsRequest
    {
        MetricName = metricName,
        ScaledObjectRef = scaledObjectRef
    });
    Console.WriteLine("GetMetricsAsync: " + getMetricsResponse.MetricValues);

    var isActiveResponse = await client.IsActiveAsync(scaledObjectRef);
    Console.WriteLine("IsActiveAsync: " + isActiveResponse.Result);

    await Task.Delay(10000);
}
using Externalscaler;
using Grpc.Core;

namespace Scaler.Services
{
    public class ExternalScalerService : ExternalScaler.ExternalScalerBase
    {
        private readonly ILogger<ExternalScalerService> _logger;
        private readonly IManagementGrain _managementGrain;
        private readonly IClusterClient _clusterClient;
        private const string MetricName = "grainsPerSilo";
        private const string GraintypeKey = "graintype";
        private const string UpperboundKey = "upperbound";
        private const string SiloNameFilterKey = "siloNameFilter";

        public ExternalScalerService(IClusterClient clusterClient, ILogger<ExternalScalerService> logger)
        {
            _clusterClient = clusterClient;
            _managementGrain = clusterClient.GetGrain<IManagementGrain>(0);
            _logger = logger;
        }

        public override Task<GetMetricSpecResponse> GetMetricSpec(ScaledObjectRef request, ServerCallContext context)
        {
            _logger.LogInformation("GetMetricSpec: Entering");
            var upperbound = request.ScalerMetadata[UpperboundKey];

            _logger.LogInformation("GetMetricSpec: Returning MetricName: {MetricName}, TargetSize: {Upperbound}.", MetricName, upperbound);
            var resp = new GetMetricSpecResponse();
            resp.MetricSpecs.Add(new MetricSpec
            {
                MetricName = MetricName,
                TargetSize = Convert.ToInt32(upperbound)
            });

            _logger.LogInformation("GetMetricSpec: Exiting");

            return Task.FromResult(resp);
        }

        public override async Task<GetMetricsResponse> GetMetrics(GetMetricsRequest request, ServerCallContext context)
        {
            _logger.LogInformation("GetMetrics: Entering");

            CheckRequestMetadata(request.ScaledObjectRef);

            var grainType = request.ScaledObjectRef.ScalerMetadata[GraintypeKey];
            var siloNameFilter = request.ScaledObjectRef.ScalerMetadata[SiloNameFilterKey];
            var summary = await GetGrainCountInCluster(grainType, siloNameFilter);
            var value = (summary.GrainCount > 0 && summary.SiloCount > 0) ? (summary.GrainCount / summary.SiloCount) : 0;

            _logger.LogInformation("GetMetrics: Returning MetricName: {MetricName}, MetricValue: {Value}.", MetricName, value);

            var response = new GetMetricsResponse();
            response.MetricValues.Add(new MetricValue
            {
                MetricName = MetricName,
                MetricValue_ = value
            });

            _logger.LogInformation("GetMetrics: Exiting");

            return response;
        }

        public override async Task StreamIsActive(ScaledObjectRef request, IServerStreamWriter<IsActiveResponse> responseStream, ServerCallContext context)
        {
            _logger.LogInformation("StreamIsActive: Entering");

            CheckRequestMetadata(request);

            var grainType = request.ScalerMetadata[GraintypeKey];
            var upperbound = request.ScalerMetadata[UpperboundKey];

            _logger.LogInformation("StreamIsActive: Processing with graintype: {Graintype}, upperbound: {Upperbound}.", grainType, upperbound);

            while (!context.CancellationToken.IsCancellationRequested)
            {
                var result = await AreTooManyGrainsInTheCluster(request);
                _logger.LogInformation("StreamIsActive: Writing IsActiveResponse to stream with Result = {Result}.", result);

                await responseStream.WriteAsync(new IsActiveResponse
                {
                    Result = result
                });

                await Task.Delay(TimeSpan.FromSeconds(5));
            }

            _logger.LogInformation("StreamIsActive: Exiting");
        }

        public override async Task<IsActiveResponse> IsActive(ScaledObjectRef request, ServerCallContext context)
        {
            _logger.LogInformation("IsActive: Entering");

            CheckRequestMetadata(request);

            var grainType = request.ScalerMetadata[GraintypeKey];
            var upperbound = request.ScalerMetadata[UpperboundKey];

            _logger.LogInformation("IsActive: Processing with graintype: {Graintype}, upperbound: {Upperbound}.", grainType, upperbound);

            var result = await AreTooManyGrainsInTheCluster(request);

            _logger.LogInformation("IsActive: Returning {Result} from IsActive.", result);
            _logger.LogInformation("IsActive: Exiting");

            return new IsActiveResponse
            {
                Result = result
            };
        }

        private static void CheckRequestMetadata(ScaledObjectRef request)
        {
            if (!request.ScalerMetadata.ContainsKey(GraintypeKey)
                || !request.ScalerMetadata.ContainsKey(UpperboundKey)
                || !request.ScalerMetadata.ContainsKey(SiloNameFilterKey))
            {
                throw new ArgumentException($"{GraintypeKey}, {SiloNameFilterKey}, and {UpperboundKey} must be specified");
            }
        }

        private async Task<bool> AreTooManyGrainsInTheCluster(ScaledObjectRef request)
        {
            var grainType = request.ScalerMetadata[GraintypeKey];
            var upperbound = request.ScalerMetadata[UpperboundKey];
            var siloNameFilter = request.ScalerMetadata[SiloNameFilterKey];
            var counts = await GetGrainCountInCluster(grainType, siloNameFilter);

            if (counts.GrainCount == 0 || counts.SiloCount == 0) return false;

            var result = counts.GrainCount / counts.SiloCount;
            var isTooMany = Convert.ToInt32(upperbound) <= result;

            _logger.LogInformation("Returning {IsTooMany} from AreTooManyGrainsInTheCluster as there are {GrainCount} grains and {SiloCount} silos (average of {Result} per silo).",
                isTooMany, counts.GrainCount, counts.SiloCount, result);

            return isTooMany;
        }

        private async Task<GrainSaturationSummary> GetGrainCountInCluster(string grainType, string siloNameFilter)
        {
            var statistics = await _managementGrain.GetDetailedGrainStatistics();
            var activeGrainsInCluster = statistics.Select(_ => new GrainInfo(_.GrainType, _.GrainId.ToString(), _.SiloAddress.ToGatewayUri().AbsoluteUri));
            var activeGrainsOfSpecifiedType = activeGrainsInCluster.Where(_ => _.Type.ToLower().Contains(grainType.ToLower()));
            var detailedHosts = await _managementGrain.GetDetailedHosts();
            var silos = detailedHosts
                            .Where(x => x.Status == SiloStatus.Active)
                            .Select(_ => new SiloInfo(_.SiloName, _.SiloAddress.ToGatewayUri().AbsoluteUri));
            var activeSiloCount = silos.Count(_ => _.SiloName.ToLower().Contains(siloNameFilter.ToLower()));

            _logger.LogInformation("Found {GrainCount} instances of {GrainType} in cluster, with {ActiveSiloCount} '{SiloNameFilter}' silos in the cluster hosting {GrainType} grains.",
                activeGrainsOfSpecifiedType.Count(), grainType, activeSiloCount, siloNameFilter, grainType);

            return new GrainSaturationSummary(activeGrainsOfSpecifiedType.Count(), activeSiloCount);
        }
    }

    public record GrainInfo(string Type, string PrimaryKey, string SiloName);
    public record GrainSaturationSummary(long GrainCount, long SiloCount);
    public record SiloInfo(string SiloName, string SiloAddress);
}

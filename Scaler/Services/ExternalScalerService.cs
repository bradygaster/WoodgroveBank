
using Grpc.Core;
using Externalscaler;
using Orleans;
using Orleans.Runtime;

namespace Scaler.Services
{
    public class ExternalScalerService : Externalscaler.ExternalScaler.ExternalScalerBase
    {
        ILogger<ExternalScalerService> _logger;
        IManagementGrain _managementGrain;
        IGrainFactory _grainFactory;
        string _metricName = "grainThreshold";

        public ExternalScalerService(IGrainFactory grainFactory, ILogger<ExternalScalerService> logger)
        {
            _grainFactory = grainFactory;
            _managementGrain = _grainFactory.GetGrain<IManagementGrain>(0);
            _logger = logger;
        }

        public override async Task<GetMetricsResponse> GetMetrics(GetMetricsRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"Entering GetMetrics");

            CheckRequestMetadata(request.ScaledObjectRef);

            var response = new GetMetricsResponse();
            var grainType = request.ScaledObjectRef.ScalerMetadata["graintype"];

            var fnd = await GetGrainCountInCluster(grainType);

            response.MetricValues.Add(new MetricValue
            {
                MetricName = _metricName,
                MetricValue_ = fnd
            });

            _logger.LogInformation($"Exiting GetMetrics");
            return response;
        }

        public override Task<GetMetricSpecResponse> GetMetricSpec(ScaledObjectRef request, ServerCallContext context)
        {
            _logger.LogInformation($"Entering GetMetricSpec");
            CheckRequestMetadata(request);

            var resp = new GetMetricSpecResponse();

            resp.MetricSpecs.Add(new MetricSpec
            {
                MetricName = _metricName,
                TargetSize = 10
            });

            _logger.LogInformation($"Exiting GetMetricSpec");
            return Task.FromResult(resp);
        }

        public override async Task StreamIsActive(ScaledObjectRef request, IServerStreamWriter<IsActiveResponse> responseStream, ServerCallContext context)
        {
            _logger.LogInformation($"Entering StreamIsActive");
            CheckRequestMetadata(request);
            var grainType = request.ScalerMetadata["graintype"];
            var upperbound = request.ScalerMetadata["upperbound"];

            while (!context.CancellationToken.IsCancellationRequested)
            {
                var grainCount = await GetGrainCountInCluster(grainType);
                if (grainCount > long.Parse(upperbound))
                {
                    _logger.LogInformation($"Writing IsActiveResopnse to stream with Result = true.");
                    await responseStream.WriteAsync(new IsActiveResponse
                    {
                        Result = true
                    });
                }

                _logger.LogInformation($"Waiting inside StreamIsActive");
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }

        public override async Task<IsActiveResponse> IsActive(ScaledObjectRef request, ServerCallContext context)
        {
            _logger.LogInformation($"Entering IsActive");
            CheckRequestMetadata(request);

            _logger.LogInformation($"Exiting IsActive");
            return new IsActiveResponse
            {
                Result = await AreTooManyGrainsInTheCluster(request)
            };
        }

        private static void CheckRequestMetadata(ScaledObjectRef request)
        {
            if (!request.ScalerMetadata.ContainsKey("graintype") || !request.ScalerMetadata.ContainsKey("upperbound"))
            {
                throw new ArgumentException("graintype and upperbound must be specified");
            }
        }

        private async Task<bool> AreTooManyGrainsInTheCluster(ScaledObjectRef request)
        {
            _logger.LogInformation($"Entering AreTooManyGrainsInTheCluster");
            var grainType = request.ScalerMetadata["graintype"];
            var upperbound = request.ScalerMetadata["upperbound"];
            _logger.LogInformation($"Entering GetMetricSpec");
            var tooMany = Convert.ToInt32(upperbound) <= await GetGrainCountInCluster(grainType);
            _logger.LogInformation($"Exiting AreTooManyGrainsInTheCluster");
            return tooMany;
        }

        private async Task<long> GetGrainCountInCluster(string grainType)
        {
            _logger.LogInformation($"Entering GetGrainCountInCluster");
            _logger.LogInformation($"Checking cluster for count of instances of {grainType}.");
            var statistics = await _managementGrain.GetDetailedGrainStatistics();
            var activeGrainsInCluster = statistics.Select(_ => new GrainInfo(_.GrainType, _.GrainIdentity.IdentityString, _.SiloAddress.ToGatewayUri().AbsoluteUri));
            var activeGrainsOfSpecifiedType = activeGrainsInCluster.Where(_ => _.Type.ToLower().Contains(grainType)).Count();
            _logger.LogInformation($"Found {activeGrainsOfSpecifiedType} instances of {grainType} in cluster.");
            _logger.LogInformation($"Exiting GetGrainCountInCluster");
            return activeGrainsOfSpecifiedType;
        }
    }

    public record GrainInfo(string Type, string PrimaryKey, string SiloName);
}

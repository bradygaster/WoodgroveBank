
using Grpc.Core;
using KEDA;
using Orleans;
using Orleans.Runtime;

namespace Scaler.Services
{
    public class ExternalScalerService : KEDA.ExternalScaler.ExternalScalerBase
    {
        IManagementGrain _managementGrain;
        IGrainFactory _grainFactory;

        public ExternalScalerService(IGrainFactory grainFactory)
        {
            _grainFactory = grainFactory;
            _managementGrain = _grainFactory.GetGrain<IManagementGrain>(0);
        }

        public override Task<GetMetricsResponse> GetMetrics(GetMetricsRequest request, ServerCallContext context)
        {
            return base.GetMetrics(request, context);
        }

        public override Task<GetMetricSpecResponse> GetMetricSpec(ScaledObjectRef request, ServerCallContext context)
        {
            var resp = new GetMetricSpecResponse();

            resp.MetricSpecs.Add(new MetricSpec
            {
                MetricName = "grainThreshold",
                TargetSize = 10
            });

            return Task.FromResult(resp);
        }

        public override Task StreamIsActive(ScaledObjectRef request, IServerStreamWriter<IsActiveResponse> responseStream, ServerCallContext context)
        {
            return base.StreamIsActive(request, responseStream, context);
        }

        public override async Task<IsActiveResponse> IsActive(ScaledObjectRef request, ServerCallContext context)
        {
            CheckRequestMetadata(request);

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
            var graintype = request.ScalerMetadata["graintype"];
            var upperbound = request.ScalerMetadata["upperbound"];

            var statistics = await _managementGrain.GetDetailedGrainStatistics();
            var activeGrainsInCluster = statistics.Select(_ => new GrainInfo(_.GrainType, _.GrainIdentity.IdentityString, _.SiloAddress.ToGatewayUri().AbsoluteUri));
            var activeGrainsOfSpecifiedType = activeGrainsInCluster.Where(_ => _.Type.ToLower().Contains(graintype)).Count();

            return Convert.ToInt32(upperbound) <= activeGrainsOfSpecifiedType;
        }
    }

    public record GrainInfo(string Type, string PrimaryKey, string SiloName);
}

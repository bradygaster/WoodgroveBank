using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Builder;

public static class SystemEndpointExtensions
{
    public static WebApplication MapSystemEndpoints(this WebApplication app)
    {
        /// <summary>
        /// Get high-level details about the Orleans cluster.
        /// </summary>
        app.MapGet("/system/grains", async ([FromServices] IGrainFactory grainFactory) =>
        {
            var managementGrain = grainFactory.GetGrain<IManagementGrain>(0);
            var statistics = await managementGrain.GetDetailedGrainStatistics();
            var detailedHosts = await managementGrain.GetDetailedHosts();
            var silos = detailedHosts.Select(_ => new SiloInfo(_.SiloName, _.SiloAddress.ToGatewayUri().AbsoluteUri)).Distinct();
            var result = statistics.Select(_ => new GrainInfo(_.GrainType, _.GrainId.ToString(), silos.First(silo => silo.SiloAddress == _.SiloAddress.ToGatewayUri().AbsoluteUri).SiloName));
            return Results.Ok(result);
        })
        .WithTags("System")
        .WithName("GrainDetails")
        .Produces<GrainInfo[]>(StatusCodes.Status200OK);

        /// <summary>
        /// Get high-level details about the Orleans silos.
        /// </summary>
        app.MapGet("/system/silos", async ([FromServices] IGrainFactory grainFactory) =>
        {
            var managementGrain = grainFactory.GetGrain<IManagementGrain>(0);
            var statistics = await managementGrain.GetDetailedGrainStatistics();
            var detailedHosts = await managementGrain.GetDetailedHosts();
            var silos = detailedHosts
                            .Where(x => x.Status == SiloStatus.Active)
                            .Select(_ => new SiloInfo(_.SiloName, _.SiloAddress.ToGatewayUri().AbsoluteUri));

            silos = silos.Distinct();
            return Results.Ok(silos);
        })
        .WithTags("System")
        .WithName("SiloDetails")
        .Produces<SiloInfo[]>(StatusCodes.Status200OK);

        return app;
    }
}

#pragma warning disable AZPROVISION001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

using AppHost;
using Azure.Provisioning.AppContainers;

var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("storage")
                     .RunAsEmulator();

var clustering = storage.AddTables("clustering");
var grainState = storage.AddBlobs("grainState");

var orleans = builder.AddOrleans("orleans")
                     .WithClustering(clustering)
                     .WithGrainStorage(grainState);

var bank = builder.AddProject<Projects.Bank>("bank")
       .WithReference(orleans)
       .WaitFor(storage)
       .WaitFor(clustering)
       .WaitFor(grainState)
       .WithExternalHttpEndpoints()
       .PublishAsAzureContainerApp((module, containerApp) =>
       {
           containerApp.Template.Value!.Scale.Value!.MaxReplicas = 1;
           containerApp.Template.Value!.Scale.Value!.MinReplicas = 1;
           containerApp.Template.Value!.Scale.Value!.Rules = [];
       });

var scaler = builder.AddProject<Projects.Scaler>("scaler")
       .WithReference(orleans.AsClient())
       .WaitFor(storage)
       .WaitFor(clustering)
       .WaitFor(grainState)
       .WaitFor(bank)
       .AsHttp2Service()
       .PublishAsAzureContainerApp((module, containerApp) =>
       {
           containerApp.Configuration.Value!.Ingress.Value!.AllowInsecure = true;
           containerApp.Template.Value!.Scale.Value!.MaxReplicas = 1;
           containerApp.Template.Value!.Scale.Value!.MinReplicas = 1;
           containerApp.Template.Value!.Scale.Value!.Rules = [];
       });

var api = builder.AddProject<Projects.API>("api")
                 .WithReference(orleans)
                 .WaitFor(storage)
                 .WaitFor(clustering)
                 .WaitFor(grainState)
                 .WaitFor(bank)
                 .WaitFor(scaler)
                 .PublishAsAzureContainerApp((module, app) =>
                 {
                     var scalerEndpoint = scaler.GetEndpoint("http").AsProvisioningParameter(module);

                     app.Configuration.Value!.Ingress.Value!.AllowInsecure = true;
                     app.Template.Value!.Scale.Value!.MinReplicas = 1;
                     app.Template.Value!.Scale.Value!.MaxReplicas = 5;
                     app.Template.Value!.Scale.Value!.Rules = [
                       new ContainerAppScaleRule()
                       {
                           Name = "orleans",
                           Custom = new ContainerAppCustomScaleRule()
                           {
                               CustomScaleRuleType = "external",
                               Metadata = {
                                   { "scalerAddress", scalerEndpoint },
                                   { "upperbound", "100" },
                                   { "graintype", "CustomerGrain" },
                                   { "siloNameFilter", "api" }
                               }
                           }
                       }
                    ];
                 });

builder.AddProject<Projects.Simulations>("simulations")
       .WithReference(orleans.AsClient())
       .WithReference(api)
       .WaitFor(storage)
       .WaitFor(clustering)
       .WaitFor(grainState)
       .WaitFor(api)
       .WaitFor(bank)
       .WaitFor(scaler)
       .PublishAsAzureContainerApp((module, containerApp) =>
       {
           containerApp.Template.Value!.Scale.Value!.MaxReplicas = 1;
           containerApp.Template.Value!.Scale.Value!.MinReplicas = 1;
           containerApp.Template.Value!.Scale.Value!.Rules = [];
       });

builder.Build().Run();


#pragma warning restore AZPROVISION001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

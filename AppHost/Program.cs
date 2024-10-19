#pragma warning disable AZPROVISION001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("storage")
                     .RunAsEmulator();

var clustering = storage.AddTables("clustering");
var grainState = storage.AddBlobs("grainState");
var pubSubStorage = storage.AddBlobs("PubSubStore");

var orleans = builder.AddOrleans("orleans")
                     .WithClustering(clustering)
                     .WithGrainStorage(grainState)
                     .WithGrainStorage(pubSubStorage);

var api = builder.AddProject<Projects.API>("api")
                 .WithReference(orleans)
                 .PublishAsAzureContainerApp((module, containerApp) =>
                 {
                     containerApp.Template.Value!.Scale.Value!.MaxReplicas = 3;
                     containerApp.Template.Value!.Scale.Value!.MinReplicas = 3;
                 });

var bank = builder.AddProject<Projects.Bank>("bank")
       .WithReference(orleans)
       .WithExternalHttpEndpoints()
       .PublishAsAzureContainerApp((module, containerApp) =>
       {
           containerApp.Template.Value!.Scale.Value!.MaxReplicas = 1;
           containerApp.Template.Value!.Scale.Value!.MinReplicas = 1;
       });

builder.AddProject<Projects.Scaler>("scaler")
       .WithReference(orleans)
       .WaitFor(api)
       .WaitFor(bank)
       .AsHttp2Service()
       .PublishAsAzureContainerApp((module, containerApp) =>
       {
           containerApp.Template.Value!.Scale.Value!.MaxReplicas = 1;
           containerApp.Template.Value!.Scale.Value!.MinReplicas = 1;
       });

builder.AddProject<Projects.Simulations>("simulations")
       .WithReference(orleans)
       .WithReference(api)
       .WaitFor(api)
       .WaitFor(bank)
       .PublishAsAzureContainerApp((module, containerApp) =>
       {
           containerApp.Template.Value!.Scale.Value!.MaxReplicas = 1;
           containerApp.Template.Value!.Scale.Value!.MinReplicas = 1;
       });

builder.Build().Run();


#pragma warning restore AZPROVISION001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

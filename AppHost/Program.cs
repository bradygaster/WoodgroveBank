#pragma warning disable AZPROVISION001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

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
           containerApp.Template.Scale.MaxReplicas = 1;
           containerApp.Template.Scale.MinReplicas = 1;
           containerApp.Template.Scale.Rules = [];
       });

var scaler = builder.AddProject<Projects.Scaler>("scaler")
       .WithReference(orleans.AsClient())
       .WaitFor(bank)
       .AsHttp2Service()
       .PublishAsAzureContainerApp((module, containerApp) =>
       {
           containerApp.Configuration.Ingress.AllowInsecure = true;
           containerApp.Template.Scale.MaxReplicas = 1;
           containerApp.Template.Scale.MinReplicas = 1;
           containerApp.Template.Scale.Rules = [];
       });

var transactionsilo = builder.AddProject<Projects.TransactionSilo>("transactionsilo")
                 .WithReference(orleans)
                 .WaitFor(bank)
                 .WaitFor(scaler)
                 .PublishAsAzureContainerApp((module, app) =>
                 {
                     var endpoint = scaler.GetEndpoint("http");
                     var scalerEndpoint = ReferenceExpression
                                          .Create($"{endpoint.Property(EndpointProperty.Host)}:{endpoint.Property(EndpointProperty.Port)}")
                                          .AsProvisioningParameter(module, "scalerEndpoint");

                     app.Configuration.Ingress.AllowInsecure = true;
                     app.Template.Scale.MinReplicas = 1;
                     app.Template.Scale.MaxReplicas = 10;
                     app.Template.Scale.Rules = [
                        new ContainerAppScaleRule()
                        {
                            Name = "orleans",
                            Custom = new ContainerAppCustomScaleRule()
                            {
                                CustomScaleRuleType = "external",
                                Metadata = {
                                    { "scalerAddress", scalerEndpoint },
                                    { "upperbound", "500" },
                                    { "graintype", "TransactionGrain" },
                                    { "siloNameFilter", "transactions" }
                                }
                            }
                        }
                    ];
                 });

var customersilo = builder.AddProject<Projects.CustomerSilo>("customersilo")
       .WithReference(orleans)
       .WaitFor(bank)
       .WaitFor(scaler)
       .PublishAsAzureContainerApp((module, app) =>
       {
           var endpoint = scaler.GetEndpoint("http");
           var scalerEndpoint = ReferenceExpression
                                .Create($"{endpoint.Property(EndpointProperty.Host)}:{endpoint.Property(EndpointProperty.Port)}")
                                .AsProvisioningParameter(module, "scalerEndpoint");

           app.Configuration.Ingress.AllowInsecure = true;
           app.Template.Scale.MinReplicas = 1;
           app.Template.Scale.MaxReplicas = 10;
           app.Template.Scale.Rules = [
               new ContainerAppScaleRule()
               {
                   Name = "orleans",
                   Custom = new ContainerAppCustomScaleRule()
                   {
                       CustomScaleRuleType = "external",
                       Metadata = {
                           { "scalerAddress", scalerEndpoint },
                           { "upperbound", "200" },
                           { "graintype", "CustomerGrain" },
                           { "siloNameFilter", "customersilo" }
                       }
                   }
               }
           ];
       });

var accountsilo = builder.AddProject<Projects.AccountSilo>("accountsilo")
       .WithReference(orleans)
       .WaitFor(bank)
       .WaitFor(scaler)
       .WaitFor(transactionsilo)
       .PublishAsAzureContainerApp((module, app) =>
       {
           var endpoint = scaler.GetEndpoint("http");
           var scalerEndpoint = ReferenceExpression
                                .Create($"{endpoint.Property(EndpointProperty.Host)}:{endpoint.Property(EndpointProperty.Port)}")
                                .AsProvisioningParameter(module, "scalerEndpoint");

           app.Configuration.Ingress.AllowInsecure = true;
           app.Template.Scale.MinReplicas = 1;
           app.Template.Scale.MaxReplicas = 10;
           app.Template.Scale.Rules = [
               new ContainerAppScaleRule()
               {
                   Name = "orleans",
                   Custom = new ContainerAppCustomScaleRule()
                   {
                       CustomScaleRuleType = "external",
                       Metadata = {
                           { "scalerAddress", scalerEndpoint },
                           { "upperbound", "500" },
                           { "graintype", "AccountGrain" },
                           { "siloNameFilter", "accountsilo" }
                       }
                   }
               }
           ];
       });

var api = builder.AddProject<Projects.API>("api")
                 .WithReference(orleans)
                 .WaitFor(bank)
                 .WaitFor(scaler)
                 .WaitFor(accountsilo)
                 .WaitFor(customersilo)
                 .WaitFor(transactionsilo)
                 .PublishAsAzureContainerApp((module, app) =>
                 {
                     var endpoint = scaler.GetEndpoint("http");
                     var scalerEndpoint = ReferenceExpression
                                          .Create($"{endpoint.Property(EndpointProperty.Host)}:{endpoint.Property(EndpointProperty.Port)}")
                                          .AsProvisioningParameter(module, "scalerEndpoint");

                     app.Configuration.Ingress.AllowInsecure = true;
                     app.Template.Scale.MinReplicas = 1;
                     app.Template.Scale.MaxReplicas = 10;
                     app.Template.Scale.Rules = [
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
       .WaitFor(api)
       .WaitFor(accountsilo)
       .WaitFor(customersilo)
       .WaitFor(transactionsilo)
       .PublishAsAzureContainerApp((module, containerApp) =>
       {
           containerApp.Template.Scale.MaxReplicas = 1;
           containerApp.Template.Scale.MinReplicas = 1;
           containerApp.Template.Scale.Rules = [];
       });

builder.Build().Run();


#pragma warning restore AZPROVISION001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

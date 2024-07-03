var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.KestrelMetrics_ApiService>("apiservice");

builder.AddProject<Projects.KestrelMetrics_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();

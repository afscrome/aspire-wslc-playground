var builder = DistributedApplication.CreateBuilder(args);

// Aspire reads this IConfiguration key when selecting the container runtime.
builder.Configuration["ASPIRE_CONTAINER_RUNTIME"] = "wslc";

var cache = builder.AddRedis("cache");

var apiService = builder.AddProject<Projects.AspireSample_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.AspireSample_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();

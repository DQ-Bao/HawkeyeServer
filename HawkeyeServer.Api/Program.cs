using System.Text.Json;
using HawkeyeServer.Api.Endpoints;
using HawkeyeServer.Api.Utils;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddDataAccesses(builder.Configuration.GetConnectionString("Default")!);
builder.Services.AddJwtAuth(opts => opts.Key = builder.Configuration["Jwt:Key"]!);
builder.Services.AddSingleton<GoogleTokenVerifier>();
builder.Services.ConfigureHttpJsonOptions(opts =>
    opts.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase
);
builder.Services.AddSignalR(opts =>
{
    opts.EnableDetailedErrors = true;
    opts.MaximumReceiveMessageSize = 1024 * 1024 * 10;
});

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(opts =>
        opts.WithTitle("Hawkeye Server").WithTheme(ScalarTheme.Kepler)
    );
}

app.MapGet("/api/v1/test", () => "Hello World");
app.MapHub<TripHub>("/hub/v1/trip");
app.MapAuthEndpoints();

app.Run();

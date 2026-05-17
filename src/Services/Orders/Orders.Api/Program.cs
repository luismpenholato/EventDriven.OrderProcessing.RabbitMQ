using BuildingBlocks.Observability.Correlation;
using BuildingBlocks.Observability.HealthChecks;
using BuildingBlocks.Observability.Logging;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Orders.Api.Middleware;
using Orders.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilogLogging("Orders.Api");

builder.Services.AddCorrelationContext();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddOrdersInfrastructure(builder.Configuration, builder.Environment);

builder.Services.AddHealthChecks()
    .AddPostgresHealthCheck(builder.Configuration)
    .AddRabbitMqHealthCheck(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
    await app.Services.MigrateOrdersDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCorrelationId();
app.UseExceptionHandler();
app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();

public partial class Program;

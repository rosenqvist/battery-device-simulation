using Device.Api;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DeviceSimulator>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter());
});

var app = builder.Build();

app.MapGet("/device/status", (DeviceSimulator device) =>
{
    return Results.Ok(device.GetStatus());
});

app.MapPost("/simulator/connect", (DeviceSimulator device) =>
{
    return Results.Ok(device.Connect());
});

app.Run();

public partial class Program
{
}
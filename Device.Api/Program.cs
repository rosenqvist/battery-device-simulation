using System.Text.Json.Serialization;

using Device.Api;

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

app.MapPost("/simulator/start", (DeviceSimulator device) =>
{
    try
    {
        return Results.Ok(device.Start());
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(ex.Message);
    }
});

app.MapPost("/simulator/stop", (DeviceSimulator device) =>
{
    try
    {
        return Results.Ok(device.Stop());
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(ex.Message);
    }
});

app.MapPost("/simulator/disconnect", (DeviceSimulator device) =>
{
    return Results.Ok(device.Disconnect());
});

app.MapPost("/simulator/battery/{percentage:int}", (
    int percentage,
    DeviceSimulator device) =>
{
    try
    {
        return Results.Ok(device.SetBattery(percentage));
    }
    catch (ArgumentOutOfRangeException)
    {
        return Results.BadRequest(
            "Battery percentage must be between 0 and 100.");
    }
});

app.Run();

public partial class Program
{
}
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

app.MapGet("/device/status", (DeviceSimulator device) => Results.Ok(device.GetStatus()));

var simulator = app.MapGroup("/simulator");

simulator.MapPost("/connect", (DeviceSimulator device) => Execute(device.Connect));

simulator.MapPost("/start", (DeviceSimulator device) => Execute(device.Start));

simulator.MapPost("/stop", (DeviceSimulator device) => Execute(device.Stop));

simulator.MapPost("/fault", (DeviceSimulator device) => Execute(device.Fault));

simulator.MapPost("/reset", (DeviceSimulator device) => Execute(device.Reset));

simulator.MapPost("/disconnect", (DeviceSimulator device) => Execute(device.Disconnect));

simulator.MapPost("/battery/{percentage:int}", (int percentage, DeviceSimulator device) =>
    Execute(() => device.SetBattery(percentage)));

app.Run();

static IResult Execute(Func<DeviceStatus> action)
{
    try
    {
        return Results.Ok(action());
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(ex.Message);
    }
    catch (ArgumentOutOfRangeException ex)
    {
        return Results.BadRequest(ex.Message);
    }
}

public partial class Program
{
}
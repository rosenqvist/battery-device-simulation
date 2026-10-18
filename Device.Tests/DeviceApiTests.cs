using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using Device.Api;

using Microsoft.AspNetCore.Mvc.Testing;

namespace Device.Tests;

public sealed class DeviceApiTests : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public DeviceApiTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    // Initial state

    [Fact]
    public async Task NewDevice_IsOfflineWith50PercentBattery()
    {
        var status = await GetDeviceStatus();

        Assert.Equal(DeviceMode.Offline, status.Mode);
        Assert.Equal(50, status.BatteryPercentage);
        Assert.False(status.Connected);
    }

    // Connect

    [Fact]
    public async Task Connect_WhenOffline_ChangesDeviceToIdle()
    {
        var status = await PostOk("/simulator/connect");

        Assert.Equal(DeviceMode.Idle, status.Mode);
        Assert.True(status.Connected);
    }

    [Fact]
    public async Task Connect_WhenFaulted_ReturnsConflict()
    {
        await PostOk("/simulator/connect");
        await PostOk("/simulator/fault");

        var statusCode = await PostForStatusCode("/simulator/connect");

        Assert.Equal(HttpStatusCode.Conflict, statusCode);
    }

    [Fact]
    public async Task Connect_WhenRunning_KeepsRunning()
    {
        await PostOk("/simulator/connect");
        await PostOk("/simulator/start");
        await PostOk("/simulator/connect");

        var status = await GetDeviceStatus();

        Assert.Equal(DeviceMode.Running, status.Mode);
    }

    // Start

    [Fact]
    public async Task Start_WhenIdle_ChangesDeviceToRunning()
    {
        await PostOk("/simulator/connect");

        var status = await PostOk("/simulator/start");

        Assert.Equal(DeviceMode.Running, status.Mode);
        Assert.True(status.Connected);
    }

    [Fact]
    public async Task Start_WhenOffline_ReturnsConflict()
    {
        var statusCode = await PostForStatusCode("/simulator/start");

        Assert.Equal(HttpStatusCode.Conflict, statusCode);
    }

    [Fact]
    public async Task Start_WhenBatteryBelow5Percent_ReturnsConflict()
    {
        await PostOk("/simulator/connect");
        await PostOk("/simulator/battery/4");

        var statusCode = await PostForStatusCode("/simulator/start");

        Assert.Equal(HttpStatusCode.Conflict, statusCode);
    }

    [Fact]
    public async Task Start_With5PercentBattery_Succeeds()
    {
        await PostOk("/simulator/connect");
        await PostOk("/simulator/battery/5");

        var status = await PostOk("/simulator/start");

        Assert.Equal(DeviceMode.Running, status.Mode);
        Assert.Equal(5, status.BatteryPercentage);
    }

    // Stop

    [Fact]
    public async Task Stop_WhenRunning_ChangesDeviceToIdle()
    {
        await PostOk("/simulator/connect");
        await PostOk("/simulator/start");

        var status = await PostOk("/simulator/stop");

        Assert.Equal(DeviceMode.Idle, status.Mode);
        Assert.True(status.Connected);
    }

    [Fact]
    public async Task Stop_WhenIdle_ReturnsConflict()
    {
        await PostOk("/simulator/connect");

        var statusCode = await PostForStatusCode("/simulator/stop");

        Assert.Equal(HttpStatusCode.Conflict, statusCode);
    }

    [Fact]
    public async Task Stop_WhenOffline_ReturnsConflict()
    {
        var statusCode = await PostForStatusCode("/simulator/stop");

        Assert.Equal(HttpStatusCode.Conflict, statusCode);
    }

    // Disconnect

    [Fact]
    public async Task Disconnect_WhenConnected_ChangesDeviceToOffline()
    {
        await PostOk("/simulator/connect");

        var status = await PostOk("/simulator/disconnect");

        Assert.Equal(DeviceMode.Offline, status.Mode);
        Assert.False(status.Connected);
    }

    [Fact]
    public async Task Disconnect_WhenRunning_ChangesDeviceToOffline()
    {
        await PostOk("/simulator/connect");
        await PostOk("/simulator/start");

        var status = await PostOk("/simulator/disconnect");

        Assert.Equal(DeviceMode.Offline, status.Mode);
        Assert.False(status.Connected);
    }

    [Fact]
    public async Task Disconnect_WhenFaulted_PowerCyclesDevice()
    {
        await PostOk("/simulator/connect");
        await PostOk("/simulator/fault");

        var disconnected = await PostOk("/simulator/disconnect");

        Assert.Equal(DeviceMode.Offline, disconnected.Mode);
        Assert.False(disconnected.Connected);

        var reconnected = await PostOk("/simulator/connect");

        Assert.Equal(DeviceMode.Idle, reconnected.Mode);
    }

    // Battery

    [Theory]
    [InlineData(0)]
    [InlineData(75)]
    [InlineData(100)]
    public async Task SetBattery_WithValidPercentage_UpdatesBattery(int percentage)
    {
        var status = await PostOk($"/simulator/battery/{percentage}");

        Assert.Equal(percentage, status.BatteryPercentage);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task SetBattery_OutOfRange_ReturnsBadRequest(int percentage)
    {
        var statusCode = await PostForStatusCode($"/simulator/battery/{percentage}");

        Assert.Equal(HttpStatusCode.BadRequest, statusCode);
    }

    // Helpers

    private async Task<DeviceStatus> PostOk(string url)
    {
        var response = await _client.PostAsync(url, content: null);

        response.EnsureSuccessStatusCode();

        var status = await response.Content
            .ReadFromJsonAsync<DeviceStatus>(JsonOptions);

        Assert.NotNull(status);

        return status;
    }

    private async Task<HttpStatusCode> PostForStatusCode(string url)
    {
        var response = await _client.PostAsync(url, content: null);

        return response.StatusCode;
    }

    private async Task<DeviceStatus> GetDeviceStatus()
    {
        var status = await _client.GetFromJsonAsync<DeviceStatus>(
            "/device/status",
            JsonOptions);

        Assert.NotNull(status);

        return status;
    }
}
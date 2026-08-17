using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using Device.Api;

using Microsoft.AspNetCore.Mvc.Testing;

namespace Device.Tests;

public sealed class DeviceApiTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    // Initial state

    [Fact]
    public async Task NewDevice_IsOfflineWith50PercentBattery()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var status = await client.GetFromJsonAsync<DeviceStatus>(
            "/device/status",
            JsonOptions);

        Assert.NotNull(status);
        Assert.Equal(DeviceMode.Offline, status.Mode);
        Assert.Equal(50, status.BatteryPercentage);
        Assert.False(status.Connected);
    }

    // Connect

    [Fact]
    public async Task Connect_WhenOffline_ChangesDeviceToIdle()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.PostAsync(
            "/simulator/connect",
            content: null);

        response.EnsureSuccessStatusCode();

        var status = await response.Content
            .ReadFromJsonAsync<DeviceStatus>(JsonOptions);

        Assert.NotNull(status);
        Assert.Equal(DeviceMode.Idle, status.Mode);
        Assert.True(status.Connected);
    }

    // Start

    [Fact]
    public async Task Start_WhenIdle_ChangesDeviceToRunning()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var connectResponse = await client.PostAsync(
            "/simulator/connect",
            content: null);

        connectResponse.EnsureSuccessStatusCode();

        var response = await client.PostAsync(
            "/simulator/start",
            content: null);

        response.EnsureSuccessStatusCode();

        var status = await response.Content
            .ReadFromJsonAsync<DeviceStatus>(JsonOptions);

        Assert.NotNull(status);
        Assert.Equal(DeviceMode.Running, status.Mode);
        Assert.True(status.Connected);
    }

    [Fact]
    public async Task Start_WhenOffline_ReturnsConflict()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.PostAsync(
            "/simulator/start",
            content: null);

        Assert.Equal(
            HttpStatusCode.Conflict,
            response.StatusCode);
    }

    [Fact]
    public async Task Start_WhenBatteryBelow5Percent_ReturnsConflict()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var connectResponse = await client.PostAsync(
            "/simulator/connect",
            content: null);

        connectResponse.EnsureSuccessStatusCode();

        var batteryResponse = await client.PostAsync(
            "/simulator/battery/4",
            content: null);

        batteryResponse.EnsureSuccessStatusCode();

        var response = await client.PostAsync(
            "/simulator/start",
            content: null);

        Assert.Equal(
            HttpStatusCode.Conflict,
            response.StatusCode);
    }

    [Fact]
    public async Task Start_With5PercentBattery_Succeeds()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var connectResponse = await client.PostAsync(
            "/simulator/connect",
            content: null);

        connectResponse.EnsureSuccessStatusCode();

        var batteryResponse = await client.PostAsync(
            "/simulator/battery/5",
            content: null);

        batteryResponse.EnsureSuccessStatusCode();

        var response = await client.PostAsync(
            "/simulator/start",
            content: null);

        response.EnsureSuccessStatusCode();

        var status = await response.Content
            .ReadFromJsonAsync<DeviceStatus>(JsonOptions);

        Assert.NotNull(status);
        Assert.Equal(DeviceMode.Running, status.Mode);
        Assert.Equal(5, status.BatteryPercentage);
    }

    // Stop

    [Fact]
    public async Task Stop_WhenRunning_ChangesDeviceToIdle()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var connectResponse = await client.PostAsync(
            "/simulator/connect",
            content: null);

        connectResponse.EnsureSuccessStatusCode();

        var startResponse = await client.PostAsync(
            "/simulator/start",
            content: null);

        startResponse.EnsureSuccessStatusCode();

        var response = await client.PostAsync(
            "/simulator/stop",
            content: null);

        response.EnsureSuccessStatusCode();

        var status = await response.Content
            .ReadFromJsonAsync<DeviceStatus>(JsonOptions);

        Assert.NotNull(status);
        Assert.Equal(DeviceMode.Idle, status.Mode);
        Assert.True(status.Connected);
    }

    [Fact]
    public async Task Stop_WhenIdle_ReturnsConflict()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var connectResponse = await client.PostAsync(
            "/simulator/connect",
            content: null);

        connectResponse.EnsureSuccessStatusCode();

        var response = await client.PostAsync(
            "/simulator/stop",
            content: null);

        Assert.Equal(
            HttpStatusCode.Conflict,
            response.StatusCode);
    }

    [Fact]
    public async Task Stop_WhenOffline_ReturnsConflict()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.PostAsync(
            "/simulator/stop",
            content: null);

        Assert.Equal(
            HttpStatusCode.Conflict,
            response.StatusCode);
    }

    // Disconnect

    [Fact]
    public async Task Disconnect_WhenConnected_ChangesDeviceToOffline()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var connectResponse = await client.PostAsync(
            "/simulator/connect",
            content: null);

        connectResponse.EnsureSuccessStatusCode();

        var response = await client.PostAsync(
            "/simulator/disconnect",
            content: null);

        response.EnsureSuccessStatusCode();

        var status = await response.Content
            .ReadFromJsonAsync<DeviceStatus>(JsonOptions);

        Assert.NotNull(status);
        Assert.Equal(DeviceMode.Offline, status.Mode);
        Assert.False(status.Connected);
    }

    [Fact]
    public async Task Disconnect_WhenRunning_ChangesDeviceToOffline()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var connectResponse = await client.PostAsync(
            "/simulator/connect",
            content: null);

        connectResponse.EnsureSuccessStatusCode();

        var startResponse = await client.PostAsync(
            "/simulator/start",
            content: null);

        startResponse.EnsureSuccessStatusCode();

        var response = await client.PostAsync(
            "/simulator/disconnect",
            content: null);

        response.EnsureSuccessStatusCode();

        var status = await response.Content
            .ReadFromJsonAsync<DeviceStatus>(JsonOptions);

        Assert.NotNull(status);
        Assert.Equal(DeviceMode.Offline, status.Mode);
        Assert.False(status.Connected);
    }

    // Battery

    [Fact]
    public async Task SetBattery_WithValidPercentage_UpdatesBattery()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.PostAsync(
            "/simulator/battery/75",
            content: null);

        response.EnsureSuccessStatusCode();

        var status = await response.Content
            .ReadFromJsonAsync<DeviceStatus>(JsonOptions);

        Assert.NotNull(status);
        Assert.Equal(75, status.BatteryPercentage);
    }

    [Fact]
    public async Task SetBattery_With0Percent_UpdatesBattery()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.PostAsync(
            "/simulator/battery/0",
            content: null);

        response.EnsureSuccessStatusCode();

        var status = await response.Content
            .ReadFromJsonAsync<DeviceStatus>(JsonOptions);

        Assert.NotNull(status);
        Assert.Equal(0, status.BatteryPercentage);
    }

    [Fact]
    public async Task SetBattery_With100Percent_UpdatesBattery()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.PostAsync(
            "/simulator/battery/100",
            content: null);

        response.EnsureSuccessStatusCode();

        var status = await response.Content
            .ReadFromJsonAsync<DeviceStatus>(JsonOptions);

        Assert.NotNull(status);
        Assert.Equal(100, status.BatteryPercentage);
    }

    [Fact]
    public async Task SetBattery_Below0_ReturnsBadRequest()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.PostAsync(
            "/simulator/battery/-1",
            content: null);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task SetBattery_Above100_ReturnsBadRequest()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.PostAsync(
            "/simulator/battery/101",
            content: null);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
}
using Device.Api;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

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
}
namespace Device.Api;

public enum DeviceMode
{
    Offline,
    Idle,
    Running,
    Faulted
}

public sealed record DeviceStatus(
    DeviceMode Mode,
    int BatteryPercentage)
{
    public bool Connected => Mode != DeviceMode.Offline;
}

public sealed class DeviceSimulator
{
    public DeviceMode Mode { get; private set; } = DeviceMode.Offline;
    public int BatteryPercent { get; private set; } = 50;

    public DeviceStatus GetStatus()
    {
        return new DeviceStatus(Mode, BatteryPercent);
    }

    public DeviceStatus Connect()
    {

        Mode = DeviceMode.Idle;

        return GetStatus();
    }

    public DeviceStatus Disconnect()
    {
        Mode = DeviceMode.Offline;

        return GetStatus();
    }

}
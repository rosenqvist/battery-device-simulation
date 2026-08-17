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
    public int BatteryPercentage { get; private set; } = 50;

    public DeviceStatus GetStatus()
    {
        return new DeviceStatus(Mode, BatteryPercentage);
    }

    public DeviceStatus Connect()
    {

        Mode = DeviceMode.Idle;

        return GetStatus();
    }

    public DeviceStatus Start()
    {
        if (Mode != DeviceMode.Idle)
        {
            throw new InvalidOperationException(
                "Device must be idle before starting.");
        }

        if (BatteryPercentage < 5) 
        {
            throw new InvalidOperationException(
                "Battery must be at least 5% to start."); 
        }

        Mode = DeviceMode.Running;

        return GetStatus();
    }

    public DeviceStatus Stop()
    {
        if (Mode != DeviceMode.Running)
        {
            throw new InvalidOperationException(
                "Device must be running before stopping.");
        }

        Mode = DeviceMode.Idle;

        return GetStatus();
    }
    public DeviceStatus Disconnect()
    {
        if (Mode == DeviceMode.Running)
        {
            Stop();
        }

        Mode = DeviceMode.Offline;

        return GetStatus();
    }

    public DeviceStatus SetBattery(int batteryPercentage)
    { 
        if (batteryPercentage < 0 || batteryPercentage > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(batteryPercentage), "Battery percentage must be between 0 and 100.");
        }
        
        BatteryPercentage = batteryPercentage;

        return GetStatus();
    }
}
namespace Nandaka.Core.Device
{
    public interface IDeviceUpdatePolicy
    {
        int MillisecondsTimeout { get; }
        RegisterDevice GetNextDevice(MasterDevice masterDevice);
        DeviceErrorHandlerResult OnErrorOccured(RegisterDevice device, DeviceError error);
    }
}

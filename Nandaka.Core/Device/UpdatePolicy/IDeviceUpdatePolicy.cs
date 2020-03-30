using System;

namespace Nandaka.Core.Device
{
    public interface IDeviceUpdatePolicy
    {
        TimeSpan WaitTimeout { get; }
        RegisterDevice GetNextDevice(MasterDevice masterDevice);
        DeviceErrorHandlerResult OnErrorOccured(RegisterDevice device, DeviceError error);
        void OnUnexpectedDeviceResponse(int expectedDeviceAddress, int responseDeviceAddress);
    }
}

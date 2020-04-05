using System;

namespace Nandaka.Core.Device
{
    public interface IDeviceUpdatePolicy
    {
        TimeSpan WaitTimeout { get; }
        NandakaDevice GetNextDevice();
        void OnMessageReceived(NandakaDevice device);
        void OnErrorOccured(NandakaDevice device, DeviceError error);
        void OnUnexpectedDeviceResponse(int expectedDeviceAddress, int responseDeviceAddress);
    }
}

using System;

namespace Nandaka.Core.Device
{
    public interface IDeviceUpdatePolicy
    {
        TimeSpan RequestTimeout { get; }
        TimeSpan UpdateTimeout { get; }
        NandakaDevice GetNextDevice(MasterDeviceManager manager, ILog log, out bool isUpdateCycleCompleted);
        void OnMessageReceived(NandakaDevice device, ILog log);
        void OnErrorOccured(NandakaDevice device, DeviceError error, ILog log);
        void OnUnexpectedDeviceResponse(MasterDeviceManager manager, NandakaDevice expectedDevice, int responseDeviceAddress, ILog log);
    }
}

using System;
using System.Collections.Generic;
using System.Threading;

namespace Nandaka.Core.Device
{
    internal class MasterDeviceDispatcher
    {
        private readonly IDeviceUpdatePolicy _updatePolicy;
        private readonly ILog _log;

        public IReadOnlyCollection<NandakaDevice> SlaveDevices { get; }

        public TimeSpan RequestTimeout => _updatePolicy.RequestTimeout;
        
        private MasterDeviceDispatcher(IReadOnlyCollection<NandakaDevice> slaveDevices, IDeviceUpdatePolicy updatePolicy, ILog log)
        {
            SlaveDevices = slaveDevices;
            _updatePolicy = updatePolicy;
            _log = log;
        }

        public static MasterDeviceDispatcher Create(IReadOnlyCollection<NandakaDevice> slaveDevices, IDeviceUpdatePolicy updatePolicy, ILog log)
        {
            var updaterLog = new PrefixLog(log, "[Dispatcher]");
            return new MasterDeviceDispatcher(slaveDevices, updatePolicy, updaterLog);
        }
        
        public NandakaDevice GetNextDevice()
        {
            NandakaDevice nextDevice = _updatePolicy.GetNextDevice(SlaveDevices, _log, out bool isUpdateCycleCompleted);
            
            if (isUpdateCycleCompleted)
                Thread.Sleep(_updatePolicy.UpdateTimeout);

            return nextDevice;
        }

        public void OnMessageReceived(NandakaDevice device)
        {
            _updatePolicy.OnMessageReceived(device, _log);
        }

        public void OnErrorOccured(NandakaDevice device, DeviceError error)
        {
            _updatePolicy.OnErrorOccured(device, error, _log);
        }

        public void OnUnexpectedDeviceResponse(NandakaDevice expectedDevice, int responseDeviceAddress)
        {
            _updatePolicy.OnUnexpectedDeviceResponse(SlaveDevices, expectedDevice, responseDeviceAddress, _log);
        }
    }
}
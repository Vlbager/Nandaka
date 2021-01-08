using System;
using System.Collections.Generic;
using System.Threading;

namespace Nandaka.Core.Device
{
    internal class MasterDeviceDispatcher
    {
        private readonly IDeviceUpdatePolicy _updatePolicy;
        private readonly ILog _log;

        public IReadOnlyCollection<ForeignDeviceCtx> SlaveDevices { get; }

        public TimeSpan RequestTimeout => _updatePolicy.RequestTimeout;
        
        private MasterDeviceDispatcher(IReadOnlyCollection<ForeignDeviceCtx> slaveDevices, IDeviceUpdatePolicy updatePolicy, ILog log)
        {
            SlaveDevices = slaveDevices;
            _updatePolicy = updatePolicy;
            _log = log;
        }

        public static MasterDeviceDispatcher Create(IReadOnlyCollection<ForeignDeviceCtx> slaveDevices, IDeviceUpdatePolicy updatePolicy, ILog log)
        {
            var updaterLog = new PrefixLog(log, "[Dispatcher]");
            return new MasterDeviceDispatcher(slaveDevices, updatePolicy, updaterLog);
        }
        
        public ForeignDeviceCtx GetNextDevice()
        {
            ForeignDeviceCtx nextDeviceCtx = _updatePolicy.GetNextDevice(SlaveDevices, _log, out bool isUpdateCycleCompleted);
            
            if (isUpdateCycleCompleted)
                Thread.Sleep(_updatePolicy.UpdateTimeout);

            return nextDeviceCtx;
        }

        public void OnMessageReceived(ForeignDeviceCtx deviceCtx)
        {
            _updatePolicy.OnMessageReceived(deviceCtx, _log);
        }

        public void OnErrorOccured(ForeignDeviceCtx deviceCtx, DeviceError error)
        {
            _updatePolicy.OnErrorOccured(deviceCtx, error, _log);
        }

        public void OnUnexpectedDeviceResponse(ForeignDeviceCtx expectedDeviceCtx, int responseDeviceAddress)
        {
            _updatePolicy.OnUnexpectedDeviceResponse(SlaveDevices, expectedDeviceCtx, responseDeviceAddress, _log);
        }
    }
}
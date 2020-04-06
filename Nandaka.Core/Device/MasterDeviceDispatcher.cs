using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Nandaka.Core.Session;

namespace Nandaka.Core.Device
{
    public class MasterDeviceDispatcher
    {
        private readonly MasterDeviceManager _manager;
        private readonly IDeviceUpdatePolicy _updatePolicy;
        private readonly ILog _log;

        public TimeSpan RequestTimeout => _updatePolicy.RequestTimeout;
        
        private MasterDeviceDispatcher(MasterDeviceManager manager, IDeviceUpdatePolicy updatePolicy, ILog log)
        {
            _manager = manager;
            _updatePolicy = updatePolicy;
            _log = log;
        }

        public static MasterDeviceDispatcher Create(MasterDeviceManager manager, IDeviceUpdatePolicy updatePolicy, ILog log)
        {
            var updaterLog = new PrefixLog(log, "[Dispatcher]");
            return new MasterDeviceDispatcher(manager, updatePolicy, updaterLog);
        }
        
        public NandakaDevice GetNextDevice()
        {
            NandakaDevice nextDevice = _updatePolicy.GetNextDevice(_manager, _log, out bool isUpdateCycleCompleted);
            
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
            _updatePolicy.OnUnexpectedDeviceResponse(_manager, expectedDevice, responseDeviceAddress, _log);
        }
    }
}
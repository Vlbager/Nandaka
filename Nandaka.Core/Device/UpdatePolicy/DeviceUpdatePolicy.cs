using System;
using System.Collections.Generic;

namespace Nandaka.Core.Device
{
    public abstract class DeviceUpdatePolicy
    {
        public IReadOnlyCollection<ForeignDevice> SlaveDevices { get; }

        protected DeviceUpdatePolicy(IReadOnlyCollection<ForeignDevice> slaveDevices)
        {
            SlaveDevices = slaveDevices;
        }
        
        /// <summary>
        /// Timeout between request and response.
        /// </summary>
        public abstract TimeSpan RequestTimeout { get; }

        public abstract ForeignDevice WaitForNextDevice();
        public abstract void OnMessageReceived(ForeignDevice device);
        public abstract void OnErrorOccured(ForeignDevice device, DeviceError error);
    }
}

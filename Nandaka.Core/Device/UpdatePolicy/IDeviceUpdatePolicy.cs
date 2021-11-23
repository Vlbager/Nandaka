using System;
using Microsoft.Extensions.Logging;

namespace Nandaka.Core.Device
{
    public interface IDeviceUpdatePolicy
    {
        /// <summary>
        /// Timeout between request and response.
        /// </summary>
        public TimeSpan RequestTimeout { get; }
        
        /// <summary>
        /// Timeout between each update cycle.
        /// </summary>
        public TimeSpan UpdateTimeout { get; }

        public bool IsDeviceShouldBeProcessed(ForeignDevice device, ILogger logger);
        public void OnMessageReceived(ForeignDevice device, ILogger logger);
        public void OnErrorOccured(ForeignDevice device, DeviceError error, ILogger logger);
    }
}

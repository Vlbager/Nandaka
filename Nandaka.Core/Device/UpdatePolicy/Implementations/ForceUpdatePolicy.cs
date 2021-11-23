using System;
using Microsoft.Extensions.Logging;

namespace Nandaka.Core.Device
{
    /// <summary>
    /// Update Policy to ignore slave state and all errors.
    /// </summary>
    public sealed class ForceUpdatePolicy : IDeviceUpdatePolicy
    {
        /// <summary>
        /// <inheritdoc cref="IDeviceUpdatePolicy.UpdateTimeout"/>
        /// </summary>
        public TimeSpan UpdateTimeout { get; }
        
        /// <summary>
        /// <inheritdoc cref="IDeviceUpdatePolicy.RequestTimeout"/>
        /// </summary>
        public TimeSpan RequestTimeout { get; }
        
        /// <param name="requestTimeout">Timeout between request and response</param>
        /// <param name="updateTimeout">Timeout between each update cycle</param>
        public ForceUpdatePolicy(TimeSpan requestTimeout, TimeSpan updateTimeout)
        {
            RequestTimeout = requestTimeout;
            UpdateTimeout = updateTimeout;
        }

        public bool IsDeviceShouldBeProcessed(ForeignDevice device, ILogger logger)
        {
            return true;
        }

        public void OnMessageReceived(ForeignDevice device, ILogger logger)
        {
        }

        public void OnErrorOccured(ForeignDevice device, DeviceError error, ILogger logger)
        {
            logger.LogError("Error occured with {0}. Reason: {1}", device, error);
        }
    }
}
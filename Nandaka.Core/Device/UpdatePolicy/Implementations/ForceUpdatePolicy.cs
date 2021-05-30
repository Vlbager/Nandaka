using System;
using Nandaka.Core.Logging;

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

        public bool IsDeviceShouldBeProcessed(ForeignDevice device)
        {
            return true;
        }

        public void OnMessageReceived(ForeignDevice device)
        {
        }

        public void OnErrorOccured(ForeignDevice device, DeviceError error)
        {
            Log.AppendWarning($"Error occured with {device}. Reason: {error}");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Nandaka.Core.Logging;

namespace Nandaka.Core.Device
{
    /// <summary>
    /// Update Policy to ignore all slave devices errors.
    /// </summary>
    public sealed class ForceUpdatePolicy : DeviceUpdatePolicy
    {
        private readonly IEnumerator<ForeignDevice> _enumerator;
        private readonly TimeSpan _updateTimeout;

        public override TimeSpan RequestTimeout { get; }
        
        /// <param name="devices">Devices to update</param>
        /// <param name="requestTimeout">Timeout between request and response</param>
        /// <param name="updateTimeout">Timeout between each update cycle</param>
        public ForceUpdatePolicy(IReadOnlyCollection<ForeignDevice> devices, TimeSpan requestTimeout, TimeSpan updateTimeout)
            : base(devices)
        {
            RequestTimeout = requestTimeout;
            _updateTimeout = updateTimeout;
            _enumerator = GetEnumerator(devices);
        }

        public override ForeignDevice WaitForNextDevice()
        {
            while (true)
            {
                if (!_enumerator.MoveNext())
                {
                    Thread.Sleep(_updateTimeout);
                    _enumerator.Reset();
                    if (!_enumerator.MoveNext())
                    {
                        Log.AppendWarning("Nothing to update. All devices are disconnected");
                        continue;
                    }
                }

                ForeignDevice nextDevice = _enumerator.Current;

                return nextDevice;
            }
        }

        public override void OnMessageReceived(ForeignDevice device)
        {
        }

        public override void OnErrorOccured(ForeignDevice device, DeviceError error)
        {
            Log.AppendWarning($"Error occured with {device}. Reason: {error}");
        }

        private static IEnumerator<ForeignDevice> GetEnumerator(IReadOnlyCollection<ForeignDevice> slaveDevices)
        {
            IReadOnlyCollection<ForeignDevice> devicesToUpdate = slaveDevices
                .Where(device => device.State == DeviceState.Connected)
                .ToArray();
            
            return devicesToUpdate.GetEnumerator();
        }
    }
}
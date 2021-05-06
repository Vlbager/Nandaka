using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Nandaka.Core.Logging;

namespace Nandaka.Core.Device
{
    /// <summary>
    /// Default policy that stops updating the device if it fails
    /// </summary>
    public sealed class DefaultUpdatePolicy : DeviceUpdatePolicy
    {
        private readonly int _maxErrorInRowCount;
        private readonly TimeSpan _updateTimeout;
        private readonly IEnumerator<ForeignDevice> _enumerator;

        public override TimeSpan RequestTimeout { get; }
        
        /// <param name="devices">Devices to update</param>
        /// <param name="requestTimeout">Timeout between request and response</param>
        /// <param name="updateTimeout">Timeout between each update cycle</param>
        /// <param name="maxErrorInRowCount">The number of errors in a row, which is enough to disable the device</param>
        public DefaultUpdatePolicy(IReadOnlyCollection<ForeignDevice> devices, TimeSpan requestTimeout, TimeSpan updateTimeout, int maxErrorInRowCount)
            : base(devices)
        {
            RequestTimeout = requestTimeout;
            _updateTimeout = updateTimeout;
            _maxErrorInRowCount = maxErrorInRowCount;
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
            device.ErrorCounter.Clear();
        }

        public override void OnErrorOccured(ForeignDevice device, DeviceError error)
        {
            Log.AppendWarning($"Error occured with {device}. Reason: {error}");
            
            if (!IsDeviceShouldBeStopped(device, error))
                return;
            
            switch (error)
            {
                case DeviceError.ErrorReceived:
                case DeviceError.WrongPacketData:
                    device.State = DeviceState.Corrupted;
                    break;

                case DeviceError.NotResponding:
                    device.State = DeviceState.NotResponding;
                    break;

                default:
                    device.State = DeviceState.Disconnected;
                    break;
            }
            
            Log.AppendWarning($"Device has reached the max number of errors. {device} will be disconnected");
        }

        private bool IsDeviceShouldBeStopped(ForeignDevice device, DeviceError newError)
        {
            int errorCount = device.ErrorCounter.TryAdd(newError, 1) 
                           ? 1 
                           : device.ErrorCounter[newError];
            
            device.ErrorCounter[newError] = errorCount + 1;

            return errorCount > _maxErrorInRowCount;
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
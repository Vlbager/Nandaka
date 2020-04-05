using System.Collections.Generic;

namespace Nandaka.Core.Device
{
    public class DeviceInfo
    {
        private readonly Dictionary<DeviceError, int> _errorCounter;
        public NandakaDevice Device { get; }

        public DeviceInfo(NandakaDevice device)
        {
            Device = device;
            _errorCounter = new Dictionary<DeviceError, int>();
        }

        public bool IsDeviceShouldBeStopped(DeviceError newError, int maxErrorCount)
        {
            int errorCounter = _errorCounter[newError];
            _errorCounter[newError] = errorCounter + 1;

            return errorCounter > maxErrorCount;
        }

        public void ClearErrorCounter()
            => _errorCounter.Clear();

        public bool IsDeviceSkipPreviousMessage()
            => _errorCounter.ContainsKey(DeviceError.NotResponding);
    }
}
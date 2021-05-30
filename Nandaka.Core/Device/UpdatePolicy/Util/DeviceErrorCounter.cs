using System.Collections.Generic;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Device
{
    // Thread unsafe
    internal sealed class DeviceErrorCounter
    {
        private readonly Dictionary<DeviceError, int> _errorCounter;

        public DeviceErrorCounter()
        {
            _errorCounter = new Dictionary<DeviceError, int>();
        }

        public int Increment(DeviceError error)
        {
            if (_errorCounter.TryAdd(error, 1))
                return 1;
            
            int newErrorCountValue = _errorCounter[error] + 1;
            
            _errorCounter[error] = newErrorCountValue;

            return newErrorCountValue;
        }

        public void Clear()
        {
            if (_errorCounter.IsEmpty())
                return;
            
            _errorCounter.Clear();
        }
    }
}
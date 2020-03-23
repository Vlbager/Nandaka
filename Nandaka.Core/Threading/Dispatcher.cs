using System;
using System.Collections.Generic;
using Nandaka.Core.Device;

namespace Nandaka.Core.Threading
{
    internal static class Dispatcher
    {
        private static readonly IList<MasterThread> _masterThreads;
        private static readonly IList<SlaveThread> _slaveThreads;

        static Dispatcher()
        {
            _masterThreads = new List<MasterThread>();
            _slaveThreads = new List<SlaveThread>();
        }

        public static MasterThread StartDeviceSession(MasterDevice device, IDeviceUpdatePolicy updatePolicy)
        {
            throw new NotImplementedException();
        }

        public static SlaveThread StartDeviceSession(SlaveDevice device)
        {
            throw new NotImplementedException();
        }
    }
}

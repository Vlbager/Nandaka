using System;
using System.Collections.Generic;
using System.Text;
using Nandaka.Core.Device;

namespace Nandaka.Core.Dispatching
{
    internal static class Dispatcher
    {
        private static readonly IList<MasterThread> _threads;

        static Dispatcher()
        {
            _threads = new List<MasterThread>();
        }

        public static void StartDeviceSession(MasterDevice device)
        {

        }

        public static void StartDeviceSession(SlaveDevice device)
        {
            throw new NotImplementedException();
        }
    }
}

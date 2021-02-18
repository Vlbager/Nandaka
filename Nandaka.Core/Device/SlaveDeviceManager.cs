using System;
using Nandaka.Core.Logging;
using Nandaka.Core.Protocol;
using Nandaka.Core.Threading;

namespace Nandaka.Core.Device
{
    public sealed class SlaveDeviceManager : IDisposable
    {
        private readonly SlaveThread _thread;
        
        public ForeignDevice Device { get; }

        private SlaveDeviceManager(IProtocol protocol, ForeignDevice device)
        {
            Device = device;
            _thread = new SlaveThread(device, protocol);
            _thread.Start();
        }

        public static SlaveDeviceManager Start(IProtocol protocol, ForeignDevice device)
        {
            return new SlaveDeviceManager(protocol, device);
        }

        public void Dispose()
        {
            _thread.Dispose();
        }
    }
}

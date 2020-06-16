using System;
using Nandaka.Core.Protocol;
using Nandaka.Core.Threading;

namespace Nandaka.Core.Device
{
    public sealed class SlaveDeviceManager : IDisposable
    {
        private readonly ILog _log;

        private SlaveThread _thread;
        
        public NandakaDevice Device { get; }

        public SlaveDeviceManager(NandakaDevice device)
        {
            _log = Log.Instance;
            Device = device;
        }

        public void Start(IProtocol protocol)
        {
            _log.AppendMessage(LogMessageType.Info, $"Starting {Device.Name} slave thread");
            _thread = SlaveThread.Create(Device, protocol, _log);
            _thread.Start();
        }

        public void Dispose()
        {
            _thread?.Dispose();
        }
    }
}

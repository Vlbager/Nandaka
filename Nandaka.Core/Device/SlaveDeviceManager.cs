using Nandaka.Core.Protocol;
using Nandaka.Core.Threading;

namespace Nandaka.Core.Device
{
    public abstract class SlaveDeviceManager
    {
        private readonly ILog _log;

        private SlaveThread _thread;
        
        public NandakaDevice Device { get; }

        protected SlaveDeviceManager(NandakaDevice device, ILog log)
        {
            _log = log;
            Device = device;
        }

        public void Start(IProtocol protocol)
        {
            _log.AppendMessage(LogMessageType.Info, "Starting thread");
            _thread = SlaveThread.Create(Device, protocol, _log);
            _thread.Start();
        }
    }
}

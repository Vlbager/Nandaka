using System;
using System.Threading;
using Nandaka.Core.Device;
using Nandaka.Core.Protocol;
using Nandaka.Core.Session;

namespace Nandaka.Core.Threading
{
    internal class SlaveThread : IDisposable
    {
        private readonly SlaveSession _session;
        private readonly ILog _log;
        
        private readonly Thread _thread;
        private bool _isStopped;
        
        private SlaveThread(SlaveSession session, ILog log)
        {
            _session = session;
            _log = log;
            _thread = new Thread(Routine) { IsBackground = true };
        }
        
        public static SlaveThread Create(NandakaDevice device, IProtocol protocol, ILog log)
        {
            var threadLog = new PrefixLog(log, $"[{device.Name} Slave]");
            var session = SlaveSession.Create(device, protocol, threadLog);
            return new SlaveThread(session, threadLog);
        }

        public void Start() => _thread.Start();

        // todo: logger
        private void Routine()
        {
            try
            {
                while (true)
                {
                    if (_isStopped)
                        break;

                    _session.ProcessNextMessage();
                }
            }
            catch (Exception exception)
            {
                _log.AppendMessage(LogMessageType.Error, "Unexpected error occured");
                _log.AppendMessage(LogMessageType.Error, exception.ToString());
                Dispose();
            }
            
            _log.AppendMessage(LogMessageType.Warning, $"Slave thread has been stopped");
        }

        public void Dispose()
        {
            _session?.Dispose();
            _isStopped = true;
        }
    }
}

using System;
using System.Threading;
using Nandaka.Core.Device;
using Nandaka.Core.Logging;
using Nandaka.Core.Protocol;
using Nandaka.Core.Registers;
using Nandaka.Core.Session;
using Nandaka.Core.Util;

namespace Nandaka.Core.Threading
{
    internal sealed class SlaveThread : IDisposable
    {
        private readonly ISessionHandler _sessionHandler;
        private readonly NandakaDevice _device;
        private readonly DisposableList _disposable;
        private readonly Thread _thread;
        
        private bool _isStopped;
        
        public SlaveThread(NandakaDevice device, IProtocol protocol)
        {
            _disposable = new DisposableList();
            _sessionHandler = _disposable.Add(GetSessionHandler(protocol, device));
            _device = device;
            _thread = new Thread(Routine) { IsBackground = true };
        }

        public void Start() => _thread.Start();
        
        public void Dispose()
        {
            _isStopped = true;
            _disposable.Dispose();
        }
        
        private void Routine()
        {
            try
            {
                InitializeLog();
                
                while (true)
                {
                    if (_isStopped)
                        break;

                    _sessionHandler.ProcessNext();
                }
            }
            catch (Exception exception)
            {
                Log.AppendException(exception,"Unexpected error occured");
            }
            
            Log.AppendWarning("Slave thread has been stopped");
        }

        private void InitializeLog()
        {
            _disposable.Add(Log.InitializeLog($"{_device.Name}.Slave.log"));
            
            Log.AppendMessage(LogLevel.Low, "Starting slave thread." + Environment.NewLine + _device.ToLogLine());
        }

        private static ISessionHandler GetSessionHandler(IProtocol protocol, NandakaDevice device)
        {
            var session = new SlaveSyncSession(protocol, device);
            return new ResponseSessionHandler<IRegisterMessage>(session, protocol, device);
        }
    }
}

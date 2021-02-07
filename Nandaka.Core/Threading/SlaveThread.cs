using System;
using System.Threading;
using Nandaka.Core.Device;
using Nandaka.Core.Logging;
using Nandaka.Core.Protocol;
using Nandaka.Core.Session;
using Nandaka.Core.Util;

namespace Nandaka.Core.Threading
{
    internal sealed class SlaveThread : IDisposable
    {
        private readonly ISession _session;
        private readonly ForeignDevice _device;
        private readonly DisposableList _disposable;
        private readonly Thread _thread;
        
        private bool _isStopped;
        
        private SlaveThread(ForeignDevice device, IProtocol protocol)
        {
            _disposable = new DisposableList();
            _session = _disposable.Add(new SlaveSyncSession(protocol, device));
            _device = device;
            _thread = new Thread(Routine) { IsBackground = true };
        }
        
        public static SlaveThread Create(ForeignDevice device, IProtocol protocol)
        {
            return new SlaveThread(device, protocol);
        }

        public void Start() => _thread.Start();
        
        private void Routine()
        {
            try
            {
                InitializeLog();
                
                while (true)
                {
                    if (_isStopped)
                        break;

                    _session.ProcessNext();
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

        public void Dispose()
        {
            _isStopped = true;
            _disposable.Dispose();
        }
    }
}

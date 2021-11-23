using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Nandaka.Core.Device;
using Nandaka.Core.Protocol;
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
        private readonly ILogger _logger;
        
        private bool _isStopped;
        
        public SlaveThread(NandakaDevice device, IProtocol protocol)
        {
            _logger = InitializeLog(device);
            _disposable = new DisposableList();
            _sessionHandler = _disposable.Add(GetSessionHandler(protocol, device, _logger));
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
                while (true)
                {
                    if (_isStopped)
                        break;

                    _sessionHandler.ProcessNext();
                }
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception,"Unexpected error occured");
            }
            
            _logger.LogWarning("Slave thread has been stopped");
        }

        private static ILogger InitializeLog(NandakaDevice device)
        {
            ILogger logger = NandakaConfiguration.Log.Factory.CreateLogger(device.Name);
            
            logger.LogInformation($"Starting slave thread.{Environment.NewLine}{0}", device);

            return logger;
        }

        private static ISessionHandler GetSessionHandler(IProtocol protocol, NandakaDevice device, ILogger logger)
        {
            var session = new SlaveSyncSession(protocol, device, logger);
            return new ResponseSessionHandler<IRegisterMessage>(session, protocol, device, logger);
        }
    }
}

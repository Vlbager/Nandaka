using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Nandaka.Core.Device;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Protocol;
using Nandaka.Core.Session;

namespace Nandaka.Core.Threading
{
    internal class MasterThread : IDisposable
    {
        private readonly Dictionary<int, MasterSession> _deviceSessions;
        private readonly MasterDeviceDispatcher _dispatcher;
        private readonly ILog _log;

        private readonly Thread _thread;
        private bool _isStopped;

        private MasterThread(MasterDeviceDispatcher dispatcher, IProtocol protocol, ILog log)
        {
            _dispatcher = dispatcher;
            _log = log;
            _deviceSessions = dispatcher.SlaveDevices.ToDictionary(device => device.Address,
                    device => new MasterSession(protocol, device, dispatcher, _log));
            
            _thread = new Thread(Routine) { IsBackground = true };
        }

        public static MasterThread Create(IReadOnlyCollection<NandakaDevice> slaveDevices, IProtocol protocol, IDeviceUpdatePolicy updatePolicy, ILog log)
        {
            var threadLog = new PrefixLog(log, "[Master]");
            var dispatcher = MasterDeviceDispatcher.Create(slaveDevices, updatePolicy, threadLog);
            return new MasterThread(dispatcher, protocol, threadLog);
        }

        public void StartRoutine() => _thread.Start();

        private void Routine()
        {
            try
            {
                while (true)
                {
                    if (_isStopped)
                        break;

                    NandakaDevice device = _dispatcher.GetNextDevice();

                    _log.AppendMessage(LogMessageType.Info, $"Set current device: {device}");

                    SendNextMessage(device);
                }
            }
            catch (Exception exception)
            {
                _log.AppendMessage(LogMessageType.Error, "Unexpected error occured");
                _log.AppendMessage(LogMessageType.Error, exception.ToString());
                Dispose();
            }
            
            _log.AppendMessage(LogMessageType.Warning, "Master thread has been stopped");
        }

        private void SendNextMessage(NandakaDevice device)
        {
            MasterSession session = _deviceSessions[device.Address];

            ISpecificMessage specificMessage = default;
            try
            {
                if (device.TryGetSpecific(out specificMessage))
                    session.ProcessSpecificMessage(specificMessage);
                else
                    session.ProcessNextMessage();

                _dispatcher.OnMessageReceived(device);
            }
            catch (DeviceNotRespondException deviceNotRespondException)
            {
                _log.AppendMessage(LogMessageType.Warning, deviceNotRespondException.Message);
                _dispatcher.OnErrorOccured(device, DeviceError.NotResponding);
            }
            catch (InvalidAddressReceivedException invalidAddressException)
            {
                _log.AppendMessage(LogMessageType.Error, invalidAddressException.Message);
                _dispatcher.OnUnexpectedDeviceResponse(device, invalidAddressException.ReceivedAddress);
            }
            catch (InvalidMessageReceivedException invalidMessageException)
            {
                _log.AppendMessage(LogMessageType.Error, invalidMessageException.ToString());
                _dispatcher.OnErrorOccured(device, DeviceError.WrongPacketData);
            }
        }

        public void Dispose()
        {
            _isStopped = true;
        }
    }
}

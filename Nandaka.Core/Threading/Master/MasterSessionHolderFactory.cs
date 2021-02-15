using System;
using System.Collections.Generic;
using Nandaka.Core.Device;
using Nandaka.Core.Protocol;
using Nandaka.Core.Session;

namespace Nandaka.Core.Threading
{
    internal static class MasterSessionHolderFactory
    {
        public static IMasterSessionsHolder Create(IProtocol protocol, MasterDeviceDispatcher dispatcher, string masterName)
        {
            IReadOnlyList<DeviceSessionCollection> deviceSessions = GetDeviceSessions(dispatcher, protocol);
            if (protocol.IsAsyncRequestsAllowed)
                return new MasterAsyncSessionsHolder(dispatcher, deviceSessions, masterName);

            var sessionMap = new MasterDeviceSessionMap(deviceSessions, dispatcher);
            return new MasterSingleThreadHolder(dispatcher, protocol, sessionMap, masterName);
        }
        
        private static IReadOnlyList<DeviceSessionCollection> GetDeviceSessions(MasterDeviceDispatcher dispatcher, IProtocol protocol)
        {
            var sessions = new List<DeviceSessionCollection>();

            foreach (ForeignDevice slaveDevice in dispatcher.SlaveDevices)
            {
                var sessionList = new List<ISession>();
                
                IErrorMessageHandler errorHandler = slaveDevice.ErrorMessageHandlerField ?? new DefaultErrorMessageHandler(dispatcher, slaveDevice);
                
                if (protocol.Info.IsSpecificMessageSupported)
                    sessionList.Add(new SpecificRequestSession(protocol, dispatcher.RequestTimeout, slaveDevice, errorHandler));

                if (protocol.IsAsyncRequestsAllowed && protocol.Info.IsHighPriorityMessageSupported)
                    throw new NotImplementedException("High priority message session");
                
                sessionList.Add(new MasterSyncSession(protocol, dispatcher.RequestTimeout, slaveDevice, errorHandler));

                sessions.Add(new DeviceSessionCollection(slaveDevice, sessionList));
            }

            return sessions;
        }
    }
}
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
                var handlerList = new List<ISessionHandler>();
                
                IErrorMessageHandler errorHandler = slaveDevice.ErrorMessageHandlerField ?? new DefaultErrorMessageHandler(dispatcher, slaveDevice);
                
                if (protocol.Info.IsSpecificMessageSupported)
                    handlerList.Add(GetSessionHandler(new SpecificRequestSession(), protocol, slaveDevice, dispatcher.RequestTimeout, errorHandler));

                if (protocol.IsAsyncRequestsAllowed && protocol.Info.IsHighPriorityMessageSupported)
                    throw new NotImplementedException("High priority message session");
                
                handlerList.Add(GetSessionHandler(new MasterSyncSession(protocol, slaveDevice), protocol, slaveDevice, dispatcher.RequestTimeout, errorHandler));

                sessions.Add(new DeviceSessionCollection(slaveDevice, handlerList));
            }

            return sessions;
        }

        private static ISessionHandler GetSessionHandler<TRequestMessage, TSentResult>(IRequestSession<TRequestMessage, TSentResult> session, 
                                                                                       IProtocol protocol, NandakaDevice device, TimeSpan requestTimeout, 
                                                                                       IErrorMessageHandler errorHandler)
            where TRequestMessage: IMessage
            where TSentResult: ISentResult
        {
            return new RequestSessionHandler<TRequestMessage, TSentResult>(session, protocol, device, requestTimeout, errorHandler);
        }
    }
}
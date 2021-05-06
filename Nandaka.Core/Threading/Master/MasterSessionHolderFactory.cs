using System;
using System.Collections.Generic;
using Nandaka.Core.Device;
using Nandaka.Core.Protocol;
using Nandaka.Core.Session;

namespace Nandaka.Core.Threading
{
    internal static class MasterSessionHolderFactory
    {
        public static IMasterSessionsHolder Create(IProtocol protocol, DeviceUpdatePolicy updatePolicy, string masterName)
        {
            IReadOnlyList<DeviceSessionCollection> deviceSessions = GetDeviceSessions(updatePolicy, protocol);
            if (protocol.IsAsyncRequestsAllowed)
                return new MasterAsyncSessionsHolder(updatePolicy, deviceSessions, masterName);

            var sessionMap = new MasterDeviceSessionMap(deviceSessions, updatePolicy);
            return new MasterSingleThreadHolder(updatePolicy, protocol, sessionMap, masterName);
        }
        
        private static IReadOnlyList<DeviceSessionCollection> GetDeviceSessions(DeviceUpdatePolicy updatePolicy, IProtocol protocol)
        {
            var sessions = new List<DeviceSessionCollection>();

            foreach (ForeignDevice slaveDevice in updatePolicy.SlaveDevices)
            {
                var handlerList = new List<ISessionHandler>();
                
                IErrorMessageHandler errorHandler = slaveDevice.ErrorMessageHandlerField ?? new DefaultErrorMessageHandler(updatePolicy, slaveDevice);
                
                if (protocol.Info.IsSpecificMessageSupported)
                    handlerList.Add(GetSessionHandler(new SpecificRequestSession(), protocol, slaveDevice, updatePolicy.RequestTimeout, errorHandler));

                if (protocol.IsAsyncRequestsAllowed && protocol.Info.IsHighPriorityMessageSupported)
                    throw new NotImplementedException("High priority message session");
                
                var syncSession = new MasterSyncSession(protocol, slaveDevice);
                
                handlerList.Add(GetSessionHandler(syncSession , protocol, slaveDevice, updatePolicy.RequestTimeout, errorHandler));

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
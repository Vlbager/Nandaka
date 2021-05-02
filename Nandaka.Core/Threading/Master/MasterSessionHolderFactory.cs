using System;
using System.Collections.Generic;
using Nandaka.Core.Device;
using Nandaka.Core.Protocol;
using Nandaka.Core.Session;

namespace Nandaka.Core.Threading
{
    internal static class MasterSessionHolderFactory
    {
        public static IMasterSessionsHolder Create(IProtocol protocol, DeviceUpdatePolicyWrapper updatePolicyWrapper, string masterName)
        {
            IReadOnlyList<DeviceSessionCollection> deviceSessions = GetDeviceSessions(updatePolicyWrapper, protocol);
            if (protocol.IsAsyncRequestsAllowed)
                return new MasterAsyncSessionsHolder(updatePolicyWrapper, deviceSessions, masterName);

            var sessionMap = new MasterDeviceSessionMap(deviceSessions, updatePolicyWrapper);
            return new MasterSingleThreadHolder(updatePolicyWrapper, protocol, sessionMap, masterName);
        }
        
        private static IReadOnlyList<DeviceSessionCollection> GetDeviceSessions(DeviceUpdatePolicyWrapper updatePolicyWrapper, IProtocol protocol)
        {
            var sessions = new List<DeviceSessionCollection>();

            foreach (ForeignDevice slaveDevice in updatePolicyWrapper.SlaveDevices)
            {
                var handlerList = new List<ISessionHandler>();
                
                IErrorMessageHandler errorHandler = slaveDevice.ErrorMessageHandlerField ?? new DefaultErrorMessageHandler(updatePolicyWrapper, slaveDevice);
                
                if (protocol.Info.IsSpecificMessageSupported)
                    handlerList.Add(GetSessionHandler(new SpecificRequestSession(), protocol, slaveDevice, updatePolicyWrapper.RequestTimeout, errorHandler));

                if (protocol.IsAsyncRequestsAllowed && protocol.Info.IsHighPriorityMessageSupported)
                    throw new NotImplementedException("High priority message session");
                
                var syncSession = new MasterSyncSession(protocol, slaveDevice);
                
                handlerList.Add(GetSessionHandler(syncSession , protocol, slaveDevice, updatePolicyWrapper.RequestTimeout, errorHandler));

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
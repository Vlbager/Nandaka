using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Nandaka.Core.Device;
using Nandaka.Core.Protocol;
using Nandaka.Core.Session;

namespace Nandaka.Core.Threading
{
    internal static class MasterSessionHolderFactory
    {
        public static IMasterSessionsHolder Create(IProtocol protocol, IDeviceUpdatePolicy updatePolicy, 
                                                   IReadOnlyCollection<ForeignDevice> devices, string masterName)
        {
            var candidatesHolder = new UpdateCandidatesHolder(devices, updatePolicy);

            ILogger dealerLogger = CreateDealerLogger(masterName);
            
            IReadOnlyList<DeviceSessionCollection> deviceSessions = GetDeviceSessions(candidatesHolder, updatePolicy, protocol, masterName, dealerLogger);
            
            if (protocol.IsAsyncRequestsAllowed)
                return new MasterAsyncSessionsHolder(updatePolicy, deviceSessions, dealerLogger);

            var sessionMap = new MasterDeviceSessionMap(deviceSessions);
            return new MasterSingleThreadHolder(updatePolicy, candidatesHolder, protocol, sessionMap, dealerLogger);
        }
        
        private static IReadOnlyList<DeviceSessionCollection> GetDeviceSessions(UpdateCandidatesHolder updateCandidatesHolder, 
                                                                                IDeviceUpdatePolicy updatePolicy, 
                                                                                IProtocol protocol,
                                                                                string masterName,
                                                                                ILogger dealerLogger)
        {
            var sessions = new List<DeviceSessionCollection>();

            foreach (ForeignDevice slaveDevice in updateCandidatesHolder.GetDevicesForProcessing(dealerLogger))
            {
                var handlerList = new List<ISessionHandler>();

                ILogger deviceLogger = CreateDeviceSessionLogger(slaveDevice, masterName);
                
                IErrorMessageHandler errorHandler = slaveDevice.ErrorMessageHandlerField ?? new DefaultErrorMessageHandler(updatePolicy, slaveDevice);

                if (protocol.Info.IsSpecificMessageSupported)
                {
                    var session = new SpecificRequestSession();
                    
                    ISessionHandler sessionHandler = GetSessionHandler(session, protocol, slaveDevice, updatePolicy.RequestTimeout, errorHandler, deviceLogger);
                    
                    handlerList.Add(sessionHandler);
                }

                if (protocol.IsAsyncRequestsAllowed && protocol.Info.IsHighPriorityMessageSupported)
                    throw new NotImplementedException("High priority message session");
                
                var syncSession = new MasterSyncSession(protocol, slaveDevice, deviceLogger);
                
                handlerList.Add(GetSessionHandler(syncSession , protocol, slaveDevice, updatePolicy.RequestTimeout, errorHandler, deviceLogger));

                sessions.Add(new DeviceSessionCollection(slaveDevice, handlerList));
            }

            return sessions;
        }

        private static ISessionHandler GetSessionHandler<TRequestMessage, TSentResult>(IRequestSession<TRequestMessage, TSentResult> session, 
                                                                                       IProtocol protocol, 
                                                                                       NandakaDevice device, 
                                                                                       TimeSpan requestTimeout, 
                                                                                       IErrorMessageHandler errorHandler,
                                                                                       ILogger logger)
            where TRequestMessage: IMessage
            where TSentResult: ISentResult
        {
            return new RequestSessionHandler<TRequestMessage, TSentResult>(session, protocol, device, requestTimeout, errorHandler, logger);
        }

        private static ILogger CreateDeviceSessionLogger(ForeignDevice device, string masterName)
        {
            ILogger logger = NandakaConfiguration.Log.Factory.CreateLogger(device.Name);
            
            logger.LogInformation("Initializing slave sessions log for dealer '{0}' and slave '{1}'", masterName, device.Name);

            return logger;
        }

        private static ILogger CreateDealerLogger(string masterName)
        {
            ILogger logger = NandakaConfiguration.Log.Factory.CreateLogger(masterName);
            
            logger.LogInformation("Initializing logger for dealer '{0}'", masterName);

            return logger;
        }
    }
}
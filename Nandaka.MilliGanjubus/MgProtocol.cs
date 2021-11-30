using System;
using System.Collections.Generic;
using Nandaka.Core.Device;
using Nandaka.Core.Helpers;
using Nandaka.Core.Network;
using Nandaka.Core.Protocol;
using Nandaka.Core.Registers;
using Nandaka.Core.Session;
using Nandaka.MilliGanjubus.Components;
using Nandaka.MilliGanjubus.Models;
using Nandaka.Model.Registers;

namespace Nandaka.MilliGanjubus
{
    public sealed class MgProtocol : IProtocol
    {
        private readonly IDataPortProvider<byte[]> _dataPortProvider;
        private readonly MgComposer _composer;
        private readonly MgRegisterConverter _registerConverter;
        private readonly MgInfo _info;

        public IProtocolInfo Info => _info;
        public bool IsResponseMayBeSkipped => false;
        public bool IsAsyncRequestsAllowed => false;

        public event EventHandler<MessageReceivedEventArgs>? MessageReceived;

        private MgProtocol(IDataPortProvider<byte[]> dataPortProvider, MgRegisterConverter registerConverter)
        {
            _dataPortProvider = dataPortProvider;
            _registerConverter = registerConverter;
            _composer = new MgComposer();
            var parser = new MgApplicationParser();
            _info = MgInfo.Instance;
            _dataPortProvider.OnDataReceived += (_, data) => parser.Parse(data);
            parser.MessageParsed += OnMessageParsed;
        }

        public static MgProtocol Create(IDataPortProvider<byte[]> dataPortProvider, params ForeignDevice[] devices)
        {
            var tableMap = MgRegisterConverter.Create(devices);
            return new MgProtocol(dataPortProvider, tableMap);
        }

        public SentMessageResult SendMessage(IMessage message)
        {
            if (message is IRegisterMessage registerMessage)
                return SendMessage(registerMessage);

            byte[] packet = _composer.Compose(message);
            _dataPortProvider.Write(packet);
            
            return SentMessageResult.CreateSuccessResult();
        }

        private SentMessageResult SendMessage(IRegisterMessage message)
        {
            IReadOnlyList<IRegister> registersToSend = GetPacketRegisters(message);

            return SendRegisterMessage(message, registersToSend);
        }
        
        private SentMessageResult SendRegisterMessage(IRegisterMessage message, IReadOnlyList<IRegister> registersToSend)
        {
            IRegister<byte>[] byteRegistersToSend = _registerConverter.ConvertToMgRegisters(message.SlaveDeviceAddress, registersToSend);

            MgRegisterMessage mgMessage = MgRegisterMessage.Convert(message, byteRegistersToSend);

            byte[] packet = _composer.Compose(mgMessage);

            _dataPortProvider.Write(packet);

            return SentMessageResult.CreateSuccessResult(registersToSend);
        }
        
        private void OnMessageParsed(object? sender, MessageReceivedEventArgs eventArgs)
        {
            if (!eventArgs.IsException() || eventArgs.ReceivedMessage is not MgRegisterMessage mgRegisterMessage)
            {
                MessageReceived?.Invoke(sender, eventArgs);
                return;
            }

            IRegister[] userRegisters = _registerConverter.ConvertToUserRegisters(mgRegisterMessage.SlaveDeviceAddress, mgRegisterMessage.MgRegisters);
            
            MessageReceived?.Invoke(sender, new MessageReceivedEventArgs(mgRegisterMessage.ConvertToCommon(userRegisters)));
        }

        private IReadOnlyList<IRegister> GetPacketRegisters(IRegisterMessage message)
        {
            if (RegisterConverter.IsRange(message.Registers, _info))
                return GetRangePacketRegisters(message.Registers);

            return GetSeriesPacketRegisters(message.Registers);
        }

        private IReadOnlyList<IRegister> GetRangePacketRegisters(IEnumerable<IRegister> allRegisters)
        {
            const int rangeAddressHeaderSize = 2;
            const int maxRangeByteRegistersCount = 8;

            var result = new List<IRegister>(maxRangeByteRegistersCount);
            int byteCounter = rangeAddressHeaderSize;
            
            foreach (IRegister register in allRegisters)
            {
                byteCounter += register.DataSize;
                
                if (byteCounter > _info.MaxDataLength)
                    break;

                result.Add(register);
            }

            return result;
        }

        private IReadOnlyList<IRegister> GetSeriesPacketRegisters(IEnumerable<IRegister> allRegisters)
        {
            const int maxSeriesByteRegistersCount = 5;

            var result = new List<IRegister>(maxSeriesByteRegistersCount);
            int byteCounter = 0;

            foreach (IRegister register in allRegisters)
            {
                byteCounter += register.DataSize + _info.AddressSize;

                if (byteCounter > _info.MaxDataLength)
                    break;
                
                result.Add(register);
            }

            return result;
        }
    }
}

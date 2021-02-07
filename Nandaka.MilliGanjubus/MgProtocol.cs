using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Nandaka.Core.Device;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Helpers;
using Nandaka.Core.Network;
using Nandaka.Core.Protocol;
using Nandaka.Core.Registers;
using Nandaka.Core.Session;
using Nandaka.MilliGanjubus.Components;
using Nandaka.MilliGanjubus.Models;

namespace Nandaka.MilliGanjubus
{
    public sealed class MgProtocol : IProtocol
    {
        private readonly IDataPortProvider<byte[]> _dataPortProvider;
        private readonly MgComposer _composer;
        private readonly MgApplicationParser _parser;
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
            _parser = new MgApplicationParser();
            _info = MgInfo.Instance;
            _dataPortProvider.OnDataReceived += (_, data) => _parser.Parse(data);
            _parser.MessageParsed += OnMessageParsed;
        }

        public static MgProtocol Create(IDataPortProvider<byte[]> dataPortProvider, params ForeignDevice[] devices)
        {
            var tableMap = MgRegisterConverter.Create(devices);
            return new MgProtocol(dataPortProvider, tableMap);
        }

        public void SendAsPossible(IRegisterMessage message, out IReadOnlyList<int> sentRegisterAddresses)
        {
            IReadOnlyList<IRegister> registersToSend = GetPacketRegisters(message);
            SendRegisterMessage(message, registersToSend, out sentRegisterAddresses);
        }

        public void SendMessage(IMessage message)
        {
            if (message is IRegisterMessage registerMessage)
            {
                SendMessage(registerMessage, out IReadOnlyList<int> _);
                return;
            }
            
            byte[] packet = _composer.Compose(message, out IReadOnlyList<int> _);
            _dataPortProvider.Write(packet);
        }

        private void SendMessage(IRegisterMessage message, out IReadOnlyList<int> sentRegisterAddresses)
        {
            IReadOnlyList<IRegister> registersToSend = GetPacketRegisters(message);
            if (registersToSend.Count != message.Registers.Count)
                throw new TooMuchDataRequestedException("Can't send all registers");
            
            SendRegisterMessage(message, registersToSend, out sentRegisterAddresses);
        }
        
        private void SendRegisterMessage(IRegisterMessage message, IReadOnlyList<IRegister> registersToSend, out IReadOnlyList<int> sentRegisterAddresses)
        {
            IRegister<byte>[] byteRegistersToSend = _registerConverter.ConvertToMgRegisters(message.SlaveDeviceAddress, registersToSend);

            MgRegisterMessage mgMessage = MgRegisterMessage.Convert(message, byteRegistersToSend);

            byte[] packet = _composer.Compose(mgMessage, out IReadOnlyList<int> composedByteRegisterAddresses);

            Debug.Assert(composedByteRegisterAddresses.Count != byteRegistersToSend.Length, "Fatal logic error");

            _dataPortProvider.Write(packet);

            sentRegisterAddresses = registersToSend.Select(mgRegister => mgRegister.Address).ToArray();
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

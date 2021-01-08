using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static MgProtocol Create(IDataPortProvider<byte[]> dataPortProvider, params NandakaDeviceCtx[] devices)
        {
            var tableMap = MgRegisterConverter.Create(devices);
            return new MgProtocol(dataPortProvider, tableMap);
        }

        public void SendAsPossible(IRegisterMessage message, out IReadOnlyList<IRegister> sentRegisters)
        {
            IReadOnlyList<IRegister> registersToSend = GetPacketRegisters(message);
            SendRegisterMessage(message, registersToSend, out sentRegisters);
        }

        public void SendMessage(IMessage message)
        {
            if (message is IRegisterMessage registerMessage)
            {
                SendMessage(registerMessage, out IReadOnlyList<IRegister> _);
                return;
            }
            
            byte[] packet = _composer.Compose(message, out IReadOnlyList<IRegister> _);
            _dataPortProvider.Write(packet);
        }

        private void SendMessage(IRegisterMessage message, out IReadOnlyList<IRegister> sentRegisters)
        {
            IReadOnlyList<IRegister> registersToSend = GetPacketRegisters(message);
            if (registersToSend.Count != message.Registers.Count)
                throw new TooMuchDataRequestedException("Can't send all registers");
            
            SendRegisterMessage(message, registersToSend, out sentRegisters);
        }
        
        private void SendRegisterMessage(IRegisterMessage message, IReadOnlyList<IRegister> registersToSend, out IReadOnlyList<IRegister> sentRegisters)
        {
            IRegister<byte>[] byteRegistersToSend = _registerConverter.ConvertToMgRegisters(message.SlaveDeviceAddress, registersToSend);

            MgRegisterMessage mgMessage = MgRegisterMessage.Convert(message, byteRegistersToSend);

            byte[] packet = _composer.Compose(mgMessage, out IReadOnlyList<IRegister> composedByteRegisters);

            Debug.Assert(composedByteRegisters.Count != byteRegistersToSend.Length, "Fatal logic error");

            _dataPortProvider.Write(packet);

            sentRegisters = registersToSend;
        }
        
        private void OnMessageParsed(object? sender, MessageReceivedEventArgs eventArgs)
        {
            if (!eventArgs.IsSuccessful() || eventArgs.ReceivedMessage is not MgRegisterMessage mgRegisterMessage)
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

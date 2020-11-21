using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Helpers;
using Nandaka.Core.Protocol;
using Nandaka.Core.Session;
using Nandaka.Core.Table;
using Xunit;

namespace Nandaka.Tests.Common
{
    public class ParserComposerCommonTests : IParserComposerTests
    {
        private readonly MessageGenerator _messageGenerator;
        private readonly IProtocolInfo _protocolInfo;
        private readonly IParser<byte[], MessageReceivedEventArgs> _parser;
        private readonly IComposer<IMessage, byte[]> _composer;
        private readonly AutoResetEvent _messageParsedResetEvent;
        
        private int _messageCounter;
        private IMessage _parsedMessage;

        public ParserComposerCommonTests(IParser<byte[], MessageReceivedEventArgs> parser, IComposer<IMessage, byte[]> composer,
                                         RegisterGenerator standardRegisterGenerator, IProtocolInfo protocolInfo)
        {
            _messageGenerator = new MessageGenerator(standardRegisterGenerator);
            _composer = composer;
            _protocolInfo = protocolInfo;
            _parser = parser;
            _parser.MessageParsed += OnMessageParsed;
            _messageParsedResetEvent = new AutoResetEvent(initialState: false);
        }
        
        public void AllValidSingleRegisterMessages()
        {
            IEnumerable<int> addresses = GetAllValidAddresses();

            foreach (IRegisterMessage message in _messageGenerator.GenerateSingleRegister(addresses, 1.ToEnumerable()))
                AssertRegisterMessage(message);
        }
        
        public void AllValidSizedSeriesRegisterMessages()
        {
            int maxRegisterSize = _messageGenerator.RegisterValueSize + _protocolInfo.AddressSize;
            int maxRegistersInMessage = (_protocolInfo.MaxDataLength - _protocolInfo.DataHeaderSize) / maxRegisterSize;

            // Registers should not be in range.
            int[] addressPool = GetAllValidAddresses().SkipEvery(1)
                                                      .Take(maxRegistersInMessage)
                                                      .ToArray();
            
            IEnumerable<int> messageSizes = Enumerable.Range(1, maxRegistersInMessage);
            
            foreach (IRegisterMessage message in _messageGenerator.Generate(messageSizes, addressPool, 1.ToEnumerable()))
                AssertRegisterMessage(message);
        }

        public void AllValidSizedRangeRegisterMessages()
        {
            int headerSize = 2 * _protocolInfo.AddressSize + _protocolInfo.DataHeaderSize;
            int maxRegisterInMessage = (_protocolInfo.MaxDataLength - headerSize) / _messageGenerator.RegisterValueSize;

            int[] addressPool = GetAllValidAddresses().Take(maxRegisterInMessage)
                                                      .ToArray();

            IEnumerable<int> messageSizes = Enumerable.Range(1, maxRegisterInMessage);
            
            foreach (IRegisterMessage message in _messageGenerator.Generate(messageSizes, addressPool, 1.ToEnumerable()))
                AssertRegisterMessage(message);
        }
        
        public void InvalidSizedSeriesMessages()
        {
            int maxRegisterSize = _messageGenerator.RegisterValueSize + _protocolInfo.AddressSize;
            int invalidRegistersCount = _protocolInfo.MaxDataLength / maxRegisterSize + 1;
            
            // Registers should not be in range.
            int[] addresses = GetAllValidAddresses().Take(invalidRegistersCount)
                                                    .SkipEvery(1)
                                                    .ToArray();

            IEnumerable<IRegisterMessage> messages = _messageGenerator.Generate(invalidRegistersCount.ToEnumerable(), 
                                                                                addresses, 1.ToEnumerable());

            foreach (IRegisterMessage message in messages)
            {
                _composer.Compose(message, out IReadOnlyCollection<IRegisterGroup> composedGroups);
                Assert.True(composedGroups.Count < message.RegisterGroups.Count);
            }
        }

        public void InvalidSizedRangeMessages()
        {
            int headerSize = 2 * _protocolInfo.AddressSize;
            int invalidRegistersCount = (_protocolInfo.MaxDataLength - headerSize) / _messageGenerator.RegisterValueSize + 1;

            int[] addresses = GetAllValidAddresses().Take(invalidRegistersCount)
                                                    .ToArray();

            IEnumerable<IRegisterMessage> messages = _messageGenerator.Generate(invalidRegistersCount.ToEnumerable(),
                                                                                addresses, 1.ToEnumerable());

            foreach (IRegisterMessage message in messages)
            {
                _composer.Compose(message, out IReadOnlyCollection<IRegisterGroup> composedGroups);
                Assert.True(composedGroups.Count < message.RegisterGroups.Count);
            }
        }
        
        public void ZeroSizeMessage()
        {
            var message = new CommonMessage(1, MessageType.Request, OperationType.Read, Array.Empty<IRegisterGroup>());
            Assert.Throws<InvalidRegistersException>(() => _composer.Compose(message, out IReadOnlyCollection<IRegisterGroup> _));
        }

        public void ValidErrorMessages(IEnumerable<ErrorType> validErrorTypes, IEnumerable<int> validErrorCodes)
        {
            IEnumerable<IErrorMessage> messages = _messageGenerator.GenerateCommonErrorMessages(validErrorTypes, 1.ToEnumerable(), true);

            messages = messages.Concat(_messageGenerator.GenerateProtocolErrorMessages(validErrorCodes, 1.ToEnumerable(), true));

            foreach (IErrorMessage message in messages)
                AssertErrorMessage(message);
        }

        public void InvalidErrorMessages(IEnumerable<ErrorType> invalidErrorTypes, IEnumerable<int> invalidErrorCodes)
        {
            IEnumerable<IErrorMessage> messages = _messageGenerator.GenerateCommonErrorMessages(invalidErrorTypes, 1.ToEnumerable(), true);

            messages = messages.Concat(_messageGenerator.GenerateProtocolErrorMessages(invalidErrorCodes, 1.ToEnumerable(), true));

            foreach (IErrorMessage message in messages)
                Assert.ThrowsAny<Exception>(() => _composer.Compose(message, out _));
            
        }

        private void AssertErrorMessage(IErrorMessage message)
        {
            byte[] composed = _composer.Compose(message, out _);
            
            Assert.InRange(composed.Length, _protocolInfo.MinPacketLength, _protocolInfo.MaxPacketLength);

            int currentCounterValue = _messageCounter;
            _parser.Parse(composed);
            Assert.True(_messageParsedResetEvent.WaitOne(TimeSpan.FromSeconds(1)));
            Assert.True(_messageCounter - currentCounterValue == 1);

            var parsedMessage = _parsedMessage as IErrorMessage;
            Assert.NotNull(parsedMessage);
            
            Assert.Equal(message.Type, parsedMessage.Type);
            Assert.Equal(message.ErrorType, parsedMessage.ErrorType);

            if (message is ProtocolSpecifiedErrorMessage protocolSpecificMessage)
            {
                var parsedProtocolSpecificMessage = parsedMessage as ProtocolSpecifiedErrorMessage;
                Assert.NotNull(parsedProtocolSpecificMessage);
                Assert.Equal(protocolSpecificMessage.ErrorCode, parsedProtocolSpecificMessage.ErrorCode);
            }
        }

        private void AssertRegisterMessage(IRegisterMessage message)
        {
            byte[] composed = _composer.Compose(message, out IReadOnlyCollection<IRegisterGroup> composedGroups);
                
            Assert.Equal(message.RegisterGroups.Count, composedGroups.Count);

            Assert.InRange(composed.Length, _protocolInfo.MinPacketLength, _protocolInfo.MaxPacketLength);

            int currentCounterValue = _messageCounter;
            
            _parser.Parse(composed);

            Assert.True(_messageParsedResetEvent.WaitOne(TimeSpan.FromSeconds(1)));

            Assert.True(_messageCounter - currentCounterValue == 1);
            
            var parsedMessage = _parsedMessage as IReceivedMessage;
            Assert.NotNull(parsedMessage);
            Assert.Equal(message.OperationType, parsedMessage.OperationType);
            Assert.Equal(message.Type, parsedMessage.Type);
            Assert.Equal(message.SlaveDeviceAddress, parsedMessage.SlaveDeviceAddress);

            IRegister[] expectedRawRegisters = message.RegisterGroups
                                                      .SelectMany(group => group.GetRawRegisters())
                                                      .ToArray();

            Assert.Equal(expectedRawRegisters.Length, parsedMessage.Registers.Count);

            for (int i = 0; i < expectedRawRegisters.Length; i++)
            {
                IRegister expectedRegister = expectedRawRegisters[i];
                IRegister actualRegister = parsedMessage.Registers[i];
                
                Assert.Equal(expectedRegister.Address, actualRegister.Address);
                Assert.True(expectedRegister.ToBytes().SequenceEqual(actualRegister.ToBytes()));
            }
        }
        
        private IEnumerable<int> GetAllValidAddresses()
        {
            int messagesCount = (_protocolInfo.MaxRegisterAddress - _protocolInfo.MinRegisterAddress) / _messageGenerator.RegisterValueSize;

            return Enumerable.Range(_protocolInfo.MinRegisterAddress, messagesCount);
        }
        
        private void OnMessageParsed(object sender, MessageReceivedEventArgs e)
        {
            _messageCounter++;
            _parsedMessage = e.ReceivedMessage;
            _messageParsedResetEvent.Set();
        }
    }
}
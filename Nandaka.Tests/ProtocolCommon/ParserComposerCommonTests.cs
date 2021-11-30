using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Helpers;
using Nandaka.Core.Protocol;
using Nandaka.Core.Registers;
using Nandaka.Core.Session;
using Nandaka.Model.Registers;
using Nandaka.Tests.Util;
using Xunit;
using Xunit.Sdk;
using ErrorMessage = Nandaka.Core.Session.ErrorMessage;

namespace Nandaka.Tests.ProtocolCommon
{
    public class ParserComposerCommonTests<TRegisterValue> : IParserComposerTests
        where TRegisterValue: struct
    {
        private readonly MessageGenerator<TRegisterValue> _messageGenerator;
        private readonly IProtocolInfo _protocolInfo;
        private readonly IParser<byte[], MessageReceivedEventArgs> _parser;
        private readonly IComposer<IMessage, byte[]> _composer;
        private readonly AutoResetEvent _messageParsedResetEvent;
        
        private int _messageCounter;
        private IMessage? _parsedMessage;

        public ParserComposerCommonTests(IParser<byte[], MessageReceivedEventArgs> parser, IComposer<IMessage, byte[]> composer, IProtocolInfo protocolInfo)
        {
            _messageGenerator = new MessageGenerator<TRegisterValue>();
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
            int maxRegisterSize = MessageGenerator<TRegisterValue>.RegisterValueSize + _protocolInfo.AddressSize;
            int maxRegistersInMessage = (_protocolInfo.MaxDataLength) / maxRegisterSize;

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
            int headerSize = 2 * _protocolInfo.AddressSize;
            int maxRegisterInMessage = (_protocolInfo.MaxDataLength - headerSize) / MessageGenerator<TRegisterValue>.RegisterValueSize;

            int[] addressPool = GetAllValidAddresses().Take(maxRegisterInMessage)
                                                      .ToArray();

            IEnumerable<int> messageSizes = Enumerable.Range(1, maxRegisterInMessage);
            
            foreach (IRegisterMessage message in _messageGenerator.Generate(messageSizes, addressPool, 1.ToEnumerable()))
                AssertRegisterMessage(message);
        }
        
        public void InvalidSizedSeriesMessages()
        {
            int maxRegisterSize = MessageGenerator<TRegisterValue>.RegisterValueSize + _protocolInfo.AddressSize;
            int invalidRegistersCount = _protocolInfo.MaxDataLength / maxRegisterSize + 1;
            
            // Registers should not be in range.
            int[] addresses = GetAllValidAddresses().Take(invalidRegistersCount)
                                                    .SkipEvery(1)
                                                    .ToArray();

            IEnumerable<IRegisterMessage> messages = _messageGenerator.Generate(invalidRegistersCount.ToEnumerable(), 
                                                                                addresses, 1.ToEnumerable());

            foreach (IRegisterMessage message in messages)
            {
                _composer.Invoking(composer => composer.Compose(message))
                      .Should().Throw<TooMuchDataRequestedException>();
            }
        }

        public void InvalidSizedRangeMessages()
        {
            int headerSize = 2 * _protocolInfo.AddressSize;
            int invalidRegistersCount = (_protocolInfo.MaxDataLength - headerSize) / MessageGenerator<TRegisterValue>.RegisterValueSize + 1;

            int[] addresses = GetAllValidAddresses().Take(invalidRegistersCount)
                                                    .ToArray();

            IEnumerable<IRegisterMessage> messages = _messageGenerator.Generate(invalidRegistersCount.ToEnumerable(),
                                                                                addresses, 1.ToEnumerable());

            foreach (IRegisterMessage message in messages)
            {
                _composer.Invoking(composer => composer.Compose(message))
                         .Should().Throw<TooMuchDataRequestedException>();
            }
        }
        
        public void ZeroSizeMessage()
        {
            var message = new CommonMessage(1, MessageType.Request, OperationType.Read, Array.Empty<IRegister>());
            
            _composer.Invoking(composer => composer.Compose(message))
                     .Should().Throw<NandakaBaseException>();
        }

        public void ValidCommonErrorMessages(IEnumerable<ErrorType> validErrorTypes)
        {
            IEnumerable<ErrorMessage> messages = MessageGenerator.GenerateCommonErrorMessages(validErrorTypes, 1.ToEnumerable(), true);

            foreach (ErrorMessage message in messages)
                AssertErrorMessage(message);
        }

        public void InvalidCommonErrorMessages(IEnumerable<ErrorType> invalidErrorTypes)
        {
            IEnumerable<ErrorMessage> messages = MessageGenerator.GenerateCommonErrorMessages(invalidErrorTypes, 1.ToEnumerable(), true);

            foreach (ErrorMessage message in messages)
            {
                _composer.Invoking(composer => composer.Compose(message))
                         .Should().Throw<NandakaBaseException>();
            }
            
        }

        internal void AssertErrorMessage(ErrorMessage message)
        {
            byte[] composed = _composer.Compose(message);
            
            Assert.InRange(composed.Length, _protocolInfo.MinPacketLength, _protocolInfo.MaxPacketLength);

            int currentCounterValue = _messageCounter;
            _parser.Parse(composed);
            Assert.True(_messageParsedResetEvent.WaitOne(TimeSpan.FromSeconds(1)));
            Assert.True(_messageCounter - currentCounterValue == 1);
            
            if (_parsedMessage is not ErrorMessage parsedMessage)
                throw new NotNullException();
            
            Assert.Equal(message.MessageType, parsedMessage.MessageType);

            if (message.ProtocolSpecifiedErrorMessage == null)
            {
                Assert.Equal(message.ErrorType, parsedMessage.ErrorType);
                return;
            }

            ProtocolSpecifiedErrorMessage? parsedProtocolSpecifiedErrorMessage = parsedMessage.ProtocolSpecifiedErrorMessage;
            if (parsedProtocolSpecifiedErrorMessage == null)
                throw new NotNullException();
            
            Assert.Equal(message.ProtocolSpecifiedErrorMessage.ErrorCode, parsedProtocolSpecifiedErrorMessage.ErrorCode);
        }

        internal void AssertRegisterMessage(IRegisterMessage message)
        {
            byte[] composed = _composer.Compose(message);

            Assert.InRange(composed.Length, _protocolInfo.MinPacketLength, _protocolInfo.MaxPacketLength);

            int currentCounterValue = _messageCounter;
            
            _parser.Parse(composed);

            Assert.True(_messageParsedResetEvent.WaitOne(TimeSpan.FromSeconds(1)));

            Assert.True(_messageCounter - currentCounterValue == 1);
            
            if (_parsedMessage is not IRegisterMessage parsedMessage)
                throw new NotNullException();
            
            Assert.Equal(message.OperationType, parsedMessage.OperationType);
            Assert.Equal(message.MessageType, parsedMessage.MessageType);
            Assert.Equal(message.SlaveDeviceAddress, parsedMessage.SlaveDeviceAddress);

            Assert.Equal(message.Registers.Count, parsedMessage.Registers.Count);

            for (var i = 0; i < message.Registers.Count; i++)
            {
                IRegister expectedRegister = message.Registers[i];
                IRegister actualRegister = parsedMessage.Registers[i];
                
                Assert.Equal(expectedRegister.Address, actualRegister.Address);
                Assert.True(expectedRegister.ToBytes().SequenceEqual(actualRegister.ToBytes()));
            }
        }
        
        private IEnumerable<int> GetAllValidAddresses()
        {
            int messagesCount = (_protocolInfo.MaxRegisterAddress - _protocolInfo.MinRegisterAddress) / MessageGenerator<TRegisterValue>.RegisterValueSize;

            return Enumerable.Range(_protocolInfo.MinRegisterAddress, messagesCount);
        }
        
        private void OnMessageParsed(object? sender, MessageReceivedEventArgs e)
        {
            _messageCounter++;
            _parsedMessage = e.ReceivedMessage;
            _messageParsedResetEvent.Set();
        }
    }
}
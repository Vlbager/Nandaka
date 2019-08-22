using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka
{
    public abstract class ApplicationParserBase<TIn> : IParser<TIn, IProtocolMessage>
    {
        private readonly DataLinkParserBase<TIn> _dataLinkParser;

        protected ApplicationParserBase(DataLinkParserBase<TIn> dataLinkParser)
        {
            _dataLinkParser = dataLinkParser;
            _dataLinkParser.MessageParsed += 
                (sender, checkedMessage) => MessageParsed(sender, ApplicationParse(checkedMessage));
        }

        public int AwaitingReplyAddress
        {
            get => _dataLinkParser.AwaitingReplyAddress;
            set => _dataLinkParser.AwaitingReplyAddress = value;
        }

        public event EventHandler<IProtocolMessage> MessageParsed;

        public void Parse(TIn data)
        {
            _dataLinkParser.Parse(data);
        }

        protected abstract IProtocolMessage ApplicationParse(byte[] data);
    }
}

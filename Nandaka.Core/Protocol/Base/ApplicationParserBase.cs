using System;
using Nandaka.Core.Session;
using Nandaka.Core.Table;

namespace Nandaka.Core.Protocol
{
    public abstract class ApplicationParserBase<TIn> : IParser<TIn, IRegisterMessage>
    {
        private readonly DataLinkParserBase<TIn> _dataLinkParser;

        protected ApplicationParserBase(DataLinkParserBase<TIn> dataLinkParser)
        {
            _dataLinkParser = dataLinkParser;
            _dataLinkParser.MessageParsed += 
                (sender, checkedMessage) => MessageParsed?.Invoke(sender, ApplicationParse(checkedMessage));
        }

        public int AwaitingReplyAddress
        {
            get => _dataLinkParser.AwaitingReplyAddress;
            set => _dataLinkParser.AwaitingReplyAddress = value;
        }

        public event EventHandler<IRegisterMessage> MessageParsed;

        public void Parse(TIn data)
        {
            _dataLinkParser.Parse(data);
        }

        protected abstract IRegisterMessage ApplicationParse(byte[] data);
    }
}

using System.Collections.Generic;
using Nandaka.Core.Session;

namespace Nandaka.Tests.ProtocolCommon
{
    public interface IParserComposerTests
    {
        void AllValidSingleRegisterMessages();
        void AllValidSizedSeriesRegisterMessages();
        void AllValidSizedRangeRegisterMessages();
        void InvalidSizedSeriesMessages();
        void InvalidSizedRangeMessages();
        void ZeroSizeMessage();
        void ValidCommonErrorMessages(IEnumerable<ErrorType> validErrorTypes);
        void InvalidCommonErrorMessages(IEnumerable<ErrorType> invalidErrorTypes);
    }
}
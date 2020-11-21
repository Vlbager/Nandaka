using System.Collections.Generic;
using Nandaka.Core.Session;

namespace Nandaka.Tests.Common
{
    public interface IParserComposerTests
    {
        void AllValidSingleRegisterMessages();
        void AllValidSizedSeriesRegisterMessages();
        void AllValidSizedRangeRegisterMessages();
        void InvalidSizedSeriesMessages();
        void InvalidSizedRangeMessages();
        void ZeroSizeMessage();
        void ValidErrorMessages(IEnumerable<ErrorType> validErrorTypes, IEnumerable<int> validErrorCodes);
        void InvalidErrorMessages(IEnumerable<ErrorType> invalidErrorTypes, IEnumerable<int> invalidErrorCodes);
    }
}
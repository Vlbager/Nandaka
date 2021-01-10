using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Helpers;
using Nandaka.Core.Session;
using Nandaka.MilliGanjubus.Components;
using Nandaka.MilliGanjubus.Models;
using Nandaka.Tests.Common;
using Xunit;

namespace Nandaka.Tests.MilliGanjubus
{
    public class ParserComposerMilliGanjubusTests : IParserComposerTests
    {
        private static readonly ParserComposerCommonTests<byte> CommonTests;

        private static readonly ErrorType[] ValidErrorTypes =
        {
            ErrorType.InvalidAddress,
            ErrorType.InvalidMetaData,
            ErrorType.TooMuchDataRequested
        };

        private static readonly int[] ValidErrorCodes = Enum.GetValues<MgErrorType>()
                                                            .Cast<int>()
                                                            .ToArray();

        static ParserComposerMilliGanjubusTests()
        {
            CommonTests = new ParserComposerCommonTests<byte>(new MgApplicationParser(), new MgComposer(), MgInfo.Instance);
        }

        [Fact]
        [Trait("ShouldParseCompose", "All")]
        public void AllValidSingleRegisterMessages()
        {
            CommonTests.AllValidSingleRegisterMessages();
        }

        [Fact]
        [Trait("ShouldParseCompose", "All")]
        public void AllValidSizedSeriesRegisterMessages()
        {
            CommonTests.AllValidSizedSeriesRegisterMessages();
        }

        [Fact]
        [Trait("ShouldParseCompose", "All")]
        public void AllValidSizedRangeRegisterMessages()
        {
            CommonTests.AllValidSizedRangeRegisterMessages();
        }

        [Fact]
        [Trait("ShouldFail", "All")]
        public void InvalidSizedSeriesMessages()
        {
            CommonTests.InvalidSizedSeriesMessages();
        }

        [Fact]
        [Trait("ShouldFail", "All")]
        public void InvalidSizedRangeMessages()
        {
            CommonTests.InvalidSizedRangeMessages();
        }

        [Fact]
        [Trait("ShouldFail", "Once")]
        public void ZeroSizeMessage()
        {
            CommonTests.ZeroSizeMessage();
        }
        
        public static readonly IEnumerable<object[]> ValidErrorMessagesParams = new[] { new object[] { ValidErrorTypes } };

        [Theory]
        [MemberData(nameof(ValidErrorMessagesParams))]
        [Trait("ShouldParseCompose", "All")]
        public void ValidCommonErrorMessages(IEnumerable<ErrorType> validErrorTypes)
        {
            CommonTests.ValidCommonErrorMessages(validErrorTypes);
        }

        public static readonly IEnumerable<object[]> InvalidErrorMessagesParams = new[]
        {
            new object[] { Enum.GetValues<ErrorType>().Except(ValidErrorTypes).ToArray() }
        };

        [Theory]
        [MemberData(nameof(InvalidErrorMessagesParams))]
        [Trait("ShouldFail", "All")]
        public void InvalidCommonErrorMessages(IEnumerable<ErrorType> invalidErrorTypes)
        {
            CommonTests.InvalidCommonErrorMessages(invalidErrorTypes);
        }

        [Fact]
        [Trait("ShouldParseCompose", "All")]
        public void ValidProtocolErrorMessages()
        {
            IEnumerable<ErrorMessage> messages = MessageGenerator.GenerateProtocolErrorMessages(ValidErrorCodes, 1.ToEnumerable(),
                                                                                                true);

            foreach (ErrorMessage errorMessage in messages)
                CommonTests.AssertErrorMessage(errorMessage);
        }
        
        [Fact]
        [Trait("ShouldFail", "All")]
        public void InvalidProtocolErrorMessages()
        {
            IEnumerable<ErrorMessage> messages = MessageGenerator.GenerateProtocolErrorMessages(GetInvalidErrorCodes(), 
                                                                                                1.ToEnumerable(), true);

            foreach (ErrorMessage errorMessage in messages)
                Assert.ThrowsAny<NandakaBaseException>(() => CommonTests.AssertErrorMessage(errorMessage));
        }

        private static int[] GetInvalidErrorCodes()
        {
            const int minCornerCaseErrorCode = 0;
            int maxCornerCaseErrorCode = ValidErrorCodes.Max() + 1;

            return new[] { minCornerCaseErrorCode, maxCornerCaseErrorCode };
        }
    }
}
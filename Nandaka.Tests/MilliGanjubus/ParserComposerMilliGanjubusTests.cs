using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Session;
using Nandaka.Core.Table;
using Nandaka.MilliGanjubus.Components;
using Nandaka.MilliGanjubus.Models;
using Nandaka.Tests.Common;
using Xunit;

namespace Nandaka.Tests.MilliGanjubus
{
    public class ParserComposerMilliGanjubusTests : IParserComposerTests
    {
        private static readonly MilliGanjubusInfo ProtocolInfo;
        private static readonly ParserComposerCommonTests CommonTests;

        private static readonly RegisterGenerator ByteRegisterGenerator;

        private static readonly ErrorType[] ValidErrorTypes =
        {
            ErrorType.InvalidAddress,
            ErrorType.InvalidMetaData,
            ErrorType.TooMuchDataRequested
        };

        private static readonly int[] ValidSpecificErrorCodes =
        {
            (int) MilliGanjubusErrorType.UnableToExecuteCommand
        };

        static ParserComposerMilliGanjubusTests()
        {
            ProtocolInfo = new MilliGanjubusInfo();
            ByteRegisterGenerator = new RegisterGenerator(UInt8RegisterGroup.CreateNew, sizeof(byte));
            CommonTests = new ParserComposerCommonTests(new MilliGanjubusApplicationParser(ProtocolInfo), new MilliGanjubusComposer(ProtocolInfo),
                                                        ByteRegisterGenerator, ProtocolInfo);
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
        
        public static readonly IEnumerable<object[]> ValidErrorMessagesParams = new[] { new object[] { ValidErrorTypes, ValidSpecificErrorCodes } };

        [Theory]
        [MemberData(nameof(ValidErrorMessagesParams))]
        [Trait("ShouldParseCompose", "All")]
        public void ValidErrorMessages(IEnumerable<ErrorType> validErrorTypes, IEnumerable<int> validErrorCodes)
        {
            CommonTests.ValidErrorMessages(validErrorTypes, validErrorCodes);
        }

        public static readonly IEnumerable<object[]> InvalidErrorMessagesParams = new[]
        {
            new object[]
            {
                Enum.GetValues(typeof(ErrorType)).Cast<ErrorType>().Except(ValidErrorTypes).ToArray(),
                GetInvalidErrorCodes()
            }
        };

        private static int[] GetInvalidErrorCodes()
        {
            const int minCornerCaseErrorCode = 0;
            int maxCornerCaseErrorCode = ValidSpecificErrorCodes.Max() + 1;

            return new[] { minCornerCaseErrorCode, maxCornerCaseErrorCode };
        }

        [Theory]
        [MemberData(nameof(InvalidErrorMessagesParams))]
        [Trait("ShouldFail", "All")]
        public void InvalidErrorMessages(IEnumerable<ErrorType> invalidErrorTypes, IEnumerable<int> invalidErrorCodes)
        {
            CommonTests.InvalidErrorMessages(invalidErrorTypes, invalidErrorCodes);
        }
    }
}
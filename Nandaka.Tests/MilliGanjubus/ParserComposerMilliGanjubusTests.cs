using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Helpers;
using Nandaka.Core.Protocol;
using Nandaka.Core.Session;
using Nandaka.Core.Table;
using Nandaka.MilliGanjubus.Components;
using Nandaka.Tests.Common;
using Xunit;

namespace Nandaka.Tests.MilliGanjubus
{
    public class ParserComposerMilliGanjubusTests : IParserComposerTests
    {
        private static readonly MilliGanjubusInfo ProtocolInfo;
        private static readonly RegisterGenerator ByteRegisterGenerator;
        private static readonly ParserComposerCommonTests CommonTests;

        static ParserComposerMilliGanjubusTests()
        {
            ProtocolInfo = new MilliGanjubusInfo();
            ByteRegisterGenerator = new RegisterGenerator(UInt8RegisterGroup.CreateNew, sizeof(byte));
            CommonTests = new ParserComposerCommonTests(new MilliGanjubusApplicationParser(ProtocolInfo), new MilliGanjubusComposer(ProtocolInfo),
                ByteRegisterGenerator, ProtocolInfo);
        }

        public ParserComposerMilliGanjubusTests()
        {
            
        }

        [Fact]
        [Trait("ShouldParseCompose", "All")]
        public void AllValidSingleRegisterMessages()
        {
            CommonTests.AllValidSingleRegisterMessages();
        }

        [Fact]
        [Trait("ShouldParseCompose", "All")]
        public void AllValidSizedRegisterMessages()
        {
            CommonTests.AllValidSizedRegisterMessages();
        }

        [Fact]
        public void Test()
        {
            int[] ints = Enumerable.Range(1, 5).ToArray();
            IEnumerable<IEnumerable<int>> batches = Enumerable.Range(1, 5)
                                                              .Select(batchSize => ints.GetCircular(batchSize));
            foreach (IEnumerable<int> batch in batches)
            {
                foreach (int n in batch)
                    Console.WriteLine(n.ToString());
            }
        }
    }
}
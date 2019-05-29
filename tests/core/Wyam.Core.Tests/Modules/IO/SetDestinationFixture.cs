using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Core.Modules.IO;
using Wyam.Testing;
using Wyam.Testing.Documents;

namespace Wyam.Core.Tests.Modules.IO
{
    [TestFixture]
    public class SetDestinationFixture : BaseFixture
    {
        public class ExecuteTests : SetDestinationFixture
        {
            [Test]
            public async Task ExtensionWithDot()
            {
                // Given
                TestDocument input = new TestDocument(
                    new FilePath("/input/Subfolder/write-test.abc"),
                    new FilePath("Subfolder/write-test.abc"));
                SetDestination setDestination = new SetDestination(".txt");

                // When
                TestDocument result = await ExecuteAsync(input, setDestination).SingleAsync();

                // Then
                result.Destination.ShouldBe("Subfolder/write-test.txt");
            }
        }
    }
}

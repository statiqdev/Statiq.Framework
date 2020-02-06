using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Core.Tests.Scripting
{
    [TestFixture]
    public class ReflectionHelperFixture : BaseFixture
    {
        public class GetCallSignatureTests : ReflectionHelperFixture
        {
            [Test]
            public void GetsClassCallSignatures()
            {
                // Given, When
                string[] callSignatures = ReflectionHelper.GetCallSignatures(typeof(TestClass), "Through");

                // Then
                callSignatures.ShouldBe(
                    new[]
                    {
                        "public double Dubs() => Through.Dubs();",
                        "public System.Single Floats(int f) => Through.Floats(f);",
                        "public string BBB => Through.BBB;"
                    },
                    true);
            }

            [Test]
            public void GetsInterfaceCallSignatures()
            {
                // Given, When
                string[] callSignatures = ReflectionHelper.GetCallSignatures(typeof(IFoo), "Through");

                // Then
                callSignatures.ShouldBe(
                    new[]
                    {
                        "public double Dubs() => Through.Dubs();",
                        "public System.Single Floats(int f) => Through.Floats(f);",
                        "public string Bar(int foo) => Through.Bar(foo);",
                        "public int Foo(string bar, int qwerty = 2) => Through.Foo(bar, qwerty);",
                        "public System.DateTime GetDate(int buzz) => Through.GetDate(buzz);",
                        "public int AAA => Through.AAA;",
                        "public string BBB => Through.BBB;"
                    },
                    true);
            }
        }

        public class TestBase
        {
            public string BBB => "bbb";
        }

        public class TestClass : TestBase
        {
            public double Dubs() => 0;

            public float Floats(int f) => 1.1f;

            private string Str() => "str";

#pragma warning disable SA1400 // Access modifier should be declared
            int AAA { get; set; }
#pragma warning restore SA1400 // Access modifier should be declared

            private bool CCC => false;
        }

        public interface IBase
        {
            double Dubs();

            float Floats(int f) => 1.1f;

            int AAA { get; }

            public string BBB => "bbb";
        }

        public partial interface IFoo
        {
            public string Bar(int foo) => Foo(foo.ToString()).ToString();
        }

        public partial interface IFoo : IBase
        {
            int Foo(string bar, int qwerty = 2);

            DateTime GetDate(int buzz);
        }
    }
}

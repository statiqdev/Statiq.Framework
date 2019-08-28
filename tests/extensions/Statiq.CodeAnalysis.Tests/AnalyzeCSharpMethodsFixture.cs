using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.CodeAnalysis.Tests
{
    [TestFixture]
    public class AnalyzeCSharpMethodsFixture : AnalyzeCSharpBaseFixture
    {
        public class ExecuteTests : AnalyzeCSharpMethodsFixture
        {
            [Test]
            public async Task ClassMembersContainsMethods()
            {
                try
                {
                    // Given
                    const string code = @"
                        namespace Foo
                        {
                            public class Blue
                            {
                                void Green()
                                {
                                }

                                void Red()
                                {
                                }
                            }
                        }
                    ";
                    TestDocument document = GetDocument(code);
                    TestExecutionContext context = GetContext();
                    IModule module = new AnalyzeCSharp();

                    // When
                    IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, module);

                    // Then
                    CollectionAssert.AreEquivalent(
                        new[] { "Green", "Red", "ToString", "Equals", "Equals", "ReferenceEquals", "GetHashCode", "GetType", "Finalize", "MemberwiseClone" },
                        GetResult(results, "Blue").Get<IReadOnlyList<IDocument>>("Members").Select(x => x["Name"]));
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Console.WriteLine("Loader exceptions: " + Environment.NewLine + string.Join(Environment.NewLine, ex.LoaderExceptions.Select(x => x.Message)));
                    throw;
                }
            }

            [Test]
            public async Task ClassOperatorsContainsOperators()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public class Blue
                        {
	                        public static Blue operator +(Blue a1, Blue a2)
	                        {
		                        return null;
	                        }
                            public static explicit operator string(Blue b)
	                        {
	                            return null;
	                        }
                        }
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, module);

                // Then
                CollectionAssert.AreEquivalent(
                    new[] { "op_Addition", "op_Explicit" },
                    GetResult(results, "Blue").Get<IReadOnlyList<IDocument>>("Operators").Select(x => x["Name"]));
            }

            [Test]
            public async Task ContainingTypeIsCorrect()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public class Blue
                        {
                            void Green()
                            {
                            }
                        }
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, module);

                // Then
                Assert.AreEqual("Blue", GetMember(results, "Blue", "Green").Get<IDocument>("ContainingType")["Name"]);
            }

            [Test]
            public async Task DestinationPathIsCorrect()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Red
                        {
                            void X()
                            {
                            }
                        }
                    }

                    class Yellow
                    {
                        void Y<T>()
                        {
                        }
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, module);

                // Then
                CollectionAssert.AreEquivalent(
                    new[] { "global/Yellow/66F23CDD.html", "Foo/Red/A94FD382.html" },
                    results.Where(x => x["Kind"].Equals("Method")).Select(x => x.Destination.FullPath));
            }

            [Test]
            public async Task DisplayNameIsCorrect()
            {
                // Given
                const string code = @"
                    class Yellow
                    {
                        public void X()
                        {
                        }

                        void Y<T>(T a, int b)
                        {
                        }

                        void Z(bool a)
                        {
                        }
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, module);

                // Then
                Assert.AreEqual("X()", GetMember(results, "Yellow", "X")["DisplayName"]);
                Assert.AreEqual("Y<T>(T, int)", GetMember(results, "Yellow", "Y")["DisplayName"]);
                Assert.AreEqual("Z(bool)", GetMember(results, "Yellow", "Z")["DisplayName"]);
            }

            [Test]
            public async Task ReturnTypeIsCorrect()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public class Blue
                        {
                            int Green()
                            {
                                return 0;
                            }

                            Red GetRed()
                            {
                                return new Red();
                            }

                            TFoo Bar<TFoo>()
                            {
                                return default(TFoo);
                            }
                        }

                        public class Red
                        {
                        }
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, module);

                // Then
                Assert.AreEqual("int", GetMember(results, "Blue", "Green").Get<IDocument>("ReturnType")["DisplayName"]);
                Assert.AreEqual("Red", GetMember(results, "Blue", "GetRed").Get<IDocument>("ReturnType")["DisplayName"]);
                Assert.AreEqual("TFoo", GetMember(results, "Blue", "Bar").Get<IDocument>("ReturnType")["DisplayName"]);
            }

            [Test]
            public async Task ReturnTypeParamReferencesClass()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Red<T>
                        {
                            public T Blue()
                            {
                                return default(T);
                            }
                        }
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, module);

                // Then
                Assert.AreEqual("Red", GetMember(results, "Red", "Blue").Get<IDocument>("ReturnType").Get<IDocument>("DeclaringType")["Name"]);
            }

            [Test]
            public async Task ClassMemberExcludedByPredicate()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public class Blue
                        {
                            void Green()
                            {
                            }

                            void Red()
                            {
                            }
                        }
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp().WhereSymbol(x => x.Name != "Green");

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, module);

                // Then
                CollectionAssert.AreEquivalent(new[] { string.Empty, "Foo", "Blue", "Red" }, results.Select(x => x["Name"]));
                CollectionAssert.AreEquivalent(
                    new[] { "Red" },
                    GetResult(results, "Blue").Get<IReadOnlyList<IDocument>>("Members").Where(x => x.GetBool("IsResult")).Select(x => x["Name"]));
                CollectionAssert.AreEquivalent(
                    new[] { "Red", "Green", "ToString", "Equals", "Equals", "ReferenceEquals", "GetHashCode", "GetType", "Finalize", "MemberwiseClone" },
                    GetResult(results, "Blue").Get<IReadOnlyList<IDocument>>("Members").Select(x => x["Name"]));
            }

            [Test]
            public async Task ParameterType()
            {
                // Given
                const string code = @"
                    class Yellow
                    {
                        public void X(int z)
                        {
                        }
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, module);

                // Then
                Assert.AreEqual("Int32", ((IDocument)GetParameter(results, "Yellow", "X", "z")["Type"])["Name"]);
            }

            [Test]
            public async Task ParameterParamsType()
            {
                // Given
                const string code = @"
                    class Yellow
                    {
                        public void X(params int[] z)
                        {
                        }
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, module);

                // Then
                IDocument x = GetParameter(results, "Yellow", "X", "z");
                IDocument y = (IDocument)x["Type"];
                object z = y["Name"];
                Assert.AreEqual("int[]", ((IDocument)GetParameter(results, "Yellow", "X", "z")["Type"])["Name"]);
            }
        }
    }
}

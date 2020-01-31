using NUnit.Framework;
using Shouldly;
using Statiq.CodeAnalysis.Scripting;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.CodeAnalysis.Tests.Scripting
{
    [TestFixture]
    public class ScriptHelperFixture : BaseFixture
    {
        public class ParseTests : ScriptHelperFixture
        {
            [Test]
            public void AddsReturnStatement()
            {
                // Given
                TestDocument document = new TestDocument();
                TestExecutionContext context = new TestExecutionContext()
                {
                    Namespaces = new TestNamespacesCollection(new[] { "Foo.Bar" })
                };
                string code = "int x = 0;";
                string expectedStart =
@"using Foo.Bar;
using Statiq.CodeAnalysis.Scripting;
using System.Collections;

public class Script : ScriptBase, IExecutionState, IMetadata
{
public Script(IMetadata metadata, IExecutionState executionState) : base(metadata, executionState) { }
public override async Task<object> EvaluateAsync()
{
await Task.CompletedTask;

#line 1
int x = 0;
return null;
}

public object Source => Metadata.Get(""Source"");
public object Destination => Metadata.Get(""Destination"");
public object ContentProvider => Metadata.Get(""ContentProvider"");";

                // When
                string actual = ScriptHelper.Parse(code, document.Keys, context);

                // Then
                actual.Substring(0, expectedStart.Length).ShouldBe(expectedStart, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void ConvertsExpressionToReturnStatement()
            {
                // Given
                TestDocument document = new TestDocument();
                TestExecutionContext context = new TestExecutionContext()
                {
                    Namespaces = new TestNamespacesCollection(new[] { "Foo.Bar" })
                };
                string code = "1 + 2";
                string expectedStart =
@"using Foo.Bar;
using Statiq.CodeAnalysis.Scripting;
using System.Collections;

public class Script : ScriptBase, IExecutionState, IMetadata
{
public Script(IMetadata metadata, IExecutionState executionState) : base(metadata, executionState) { }
public override async Task<object> EvaluateAsync()
{
await Task.CompletedTask;
return
#line 1
1 + 2
;
}

public object Source => Metadata.Get(""Source"");
public object Destination => Metadata.Get(""Destination"");
public object ContentProvider => Metadata.Get(""ContentProvider"");";

                // When
                string actual = ScriptHelper.Parse(code, document.Keys, context);

                // Then
                actual.Substring(0, expectedStart.Length).ShouldBe(expectedStart, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void AddsReturnForExpressionWithSemi()
            {
                // Given
                TestDocument document = new TestDocument();
                TestExecutionContext context = new TestExecutionContext()
                {
                    Namespaces = new TestNamespacesCollection(new[] { "Foo.Bar" })
                };
                string code = "1 + 2;";
                string expectedStart =
@"using Foo.Bar;
using Statiq.CodeAnalysis.Scripting;
using System.Collections;

public class Script : ScriptBase, IExecutionState, IMetadata
{
public Script(IMetadata metadata, IExecutionState executionState) : base(metadata, executionState) { }
public override async Task<object> EvaluateAsync()
{
await Task.CompletedTask;

#line 1
1 + 2;
return null;
}

public object Source => Metadata.Get(""Source"");
public object Destination => Metadata.Get(""Destination"");
public object ContentProvider => Metadata.Get(""ContentProvider"");";

                // When
                string actual = ScriptHelper.Parse(code, document.Keys, context);

                // Then
                actual.Substring(0, expectedStart.Length).ShouldBe(expectedStart, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void EmitsReturnStatement()
            {
                // Given
                TestDocument document = new TestDocument();
                TestExecutionContext context = new TestExecutionContext()
                {
                    Namespaces = new TestNamespacesCollection(new[] { "Foo.Bar" })
                };
                string code = "return 0;";
                string expectedStart =
@"using Foo.Bar;
using Statiq.CodeAnalysis.Scripting;
using System.Collections;

public class Script : ScriptBase, IExecutionState, IMetadata
{
public Script(IMetadata metadata, IExecutionState executionState) : base(metadata, executionState) { }
public override async Task<object> EvaluateAsync()
{
await Task.CompletedTask;

#line 1
return 0;
return null;
}

public object Source => Metadata.Get(""Source"");
public object Destination => Metadata.Get(""Destination"");
public object ContentProvider => Metadata.Get(""ContentProvider"");";

                // When
                string actual = ScriptHelper.Parse(code, document.Keys, context);

                // Then
                actual.Substring(0, expectedStart.Length).ShouldBe(expectedStart, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void LiftsClassDeclarations()
            {
                // Given
                TestDocument document = new TestDocument();
                TestExecutionContext context = new TestExecutionContext()
                {
                    Namespaces = new TestNamespacesCollection(new[] { "Foo.Bar" })
                };
                string code =
@"public class Foo
{
    int X { get; set; }
}
public class Bar : Foo
{
    public string Y()
    {
        return X.ToString();
    }
}
int x = 1 + 2;
Pipelines.Add(Content());
public class Baz
{
}";
                string expectedStart =
@"using Foo.Bar;
using Statiq.CodeAnalysis.Scripting;
using System.Collections;

public class Script : ScriptBase, IExecutionState, IMetadata
{
public Script(IMetadata metadata, IExecutionState executionState) : base(metadata, executionState) { }
public override async Task<object> EvaluateAsync()
{
await Task.CompletedTask;

#line 12
int x = 1 + 2;
Pipelines.Add(Content());

return null;
}

public object Source => Metadata.Get(""Source"");
public object Destination => Metadata.Get(""Destination"");
public object ContentProvider => Metadata.Get(""ContentProvider"");";
                string expectedEnd =
@"#line 1
public class Foo
{
    int X { get; set; }
}
public class Bar : Foo
{
    public string Y()
    {
        return X.ToString();
    }
}
#line 14
public class Baz
{
}
public static class ScriptExtensionMethods
{

}";

                // When
                string actual = ScriptHelper.Parse(code, document.Keys, context);

                // Then
                actual.Substring(0, expectedStart.Length).ShouldBe(expectedStart, StringCompareShould.IgnoreLineEndings);
                actual.Substring(actual.Length - expectedEnd.Length).ShouldBe(expectedEnd, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void LiftsUsingDirectives()
            {
                // Given
                TestDocument document = new TestDocument();
                TestExecutionContext context = new TestExecutionContext()
                {
                    Namespaces = new TestNamespacesCollection(new[] { "Foo.Bar" })
                };
                string code =
@"using Red.Blue;
using Yellow;
public static class Foo
{
    public static string Bar(this string x) => x;
}
Pipelines.Add(Content());
public string Self(string x)
{
    return x.ToLower();
}";
                string expectedStart =
@"using Foo.Bar;
using Statiq.CodeAnalysis.Scripting;
using System.Collections;
using Red.Blue;
using Yellow;

public class Script : ScriptBase, IExecutionState, IMetadata
{
public Script(IMetadata metadata, IExecutionState executionState) : base(metadata, executionState) { }
public override async Task<object> EvaluateAsync()
{
await Task.CompletedTask;

#line 7
Pipelines.Add(Content());

return null;
}
#line 8
public string Self(string x)
{
    return x.ToLower();
}
public object Source => Metadata.Get(""Source"");
public object Destination => Metadata.Get(""Destination"");
public object ContentProvider => Metadata.Get(""ContentProvider"");";
                string expectedEnd =
@"#line 3
public static class Foo
{
    public static string Bar(this string x) => x;
}

public static class ScriptExtensionMethods
{

}";

                // When
                string actual = ScriptHelper.Parse(code, document.Keys, context);

                // Then
                actual.Substring(0, expectedStart.Length).ShouldBe(expectedStart, StringCompareShould.IgnoreLineEndings);
                actual.Substring(actual.Length - expectedEnd.Length).ShouldBe(expectedEnd, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void LiftsMethodDeclarations()
            {
                // Given
                TestDocument document = new TestDocument();
                TestExecutionContext context = new TestExecutionContext()
                {
                    Namespaces = new TestNamespacesCollection(new[] { "Foo.Bar" })
                };
                string code =
@"public static class Foo
{
    public static string Bar(this string x) => x;
}
Pipelines.Add(Content());
public string Self(string x)
{
    return x.ToLower();
}";
                string expectedStart =
@"using Foo.Bar;
using Statiq.CodeAnalysis.Scripting;
using System.Collections;

public class Script : ScriptBase, IExecutionState, IMetadata
{
public Script(IMetadata metadata, IExecutionState executionState) : base(metadata, executionState) { }
public override async Task<object> EvaluateAsync()
{
await Task.CompletedTask;

#line 5
Pipelines.Add(Content());

return null;
}
#line 6
public string Self(string x)
{
    return x.ToLower();
}
public object Source => Metadata.Get(""Source"");
public object Destination => Metadata.Get(""Destination"");
public object ContentProvider => Metadata.Get(""ContentProvider"");";
                string expectedEnd =
@"#line 1
public static class Foo
{
    public static string Bar(this string x) => x;
}

public static class ScriptExtensionMethods
{

}";

                // When
                string actual = ScriptHelper.Parse(code, document.Keys, context);

                // Then
                actual.Substring(0, expectedStart.Length).ShouldBe(expectedStart, StringCompareShould.IgnoreLineEndings);
                actual.Substring(actual.Length - expectedEnd.Length).ShouldBe(expectedEnd, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void LiftsExtensionMethodDeclarations()
            {
                // Given
                TestDocument document = new TestDocument();
                TestExecutionContext context = new TestExecutionContext()
                {
                    Namespaces = new TestNamespacesCollection(new[] { "Foo.Bar" })
                };
                string code =
@"public static class Foo
{
    public static string Bar(this string x) => x;
}
Pipelines.Add(Content());
public static string Self(this string x)
{
    return x.ToLower();
}";
                string expectedStart =
@"using Foo.Bar;
using Statiq.CodeAnalysis.Scripting;
using System.Collections;

public class Script : ScriptBase, IExecutionState, IMetadata
{
public Script(IMetadata metadata, IExecutionState executionState) : base(metadata, executionState) { }
public override async Task<object> EvaluateAsync()
{
await Task.CompletedTask;

#line 5
Pipelines.Add(Content());

return null;
}

public object Source => Metadata.Get(""Source"");
public object Destination => Metadata.Get(""Destination"");
public object ContentProvider => Metadata.Get(""ContentProvider"");";
                string expectedEnd =
@"#line 1
public static class Foo
{
    public static string Bar(this string x) => x;
}

public static class ScriptExtensionMethods
{
#line 6
public static string Self(this string x)
{
    return x.ToLower();
}
}";

                // When
                string actual = ScriptHelper.Parse(code, document.Keys, context);

                // Then
                actual.Substring(0, expectedStart.Length).ShouldBe(expectedStart, StringCompareShould.IgnoreLineEndings);
                actual.Substring(actual.Length - expectedEnd.Length).ShouldBe(expectedEnd, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void LiftsCommentsWithDeclarations()
            {
                // Given
                TestDocument document = new TestDocument();
                TestExecutionContext context = new TestExecutionContext()
                {
                    Namespaces = new TestNamespacesCollection(new[] { "Foo.Bar" })
                };
                string code =
@"// XYZ
public class Foo
{
    // ABC
    public string Bar(this string x) => x;
}
// 123
Pipelines.Add(Content());
// QWE
public string Self(string x)
{
    // RTY
    return x.ToLower();
}";
                string expectedStart =
@"using Foo.Bar;
using Statiq.CodeAnalysis.Scripting;
using System.Collections;

public class Script : ScriptBase, IExecutionState, IMetadata
{
public Script(IMetadata metadata, IExecutionState executionState) : base(metadata, executionState) { }
public override async Task<object> EvaluateAsync()
{
await Task.CompletedTask;

#line 7
// 123
Pipelines.Add(Content());

return null;
}
#line 9
// QWE
public string Self(string x)
{
    // RTY
    return x.ToLower();
}
public object Source => Metadata.Get(""Source"");
public object Destination => Metadata.Get(""Destination"");
public object ContentProvider => Metadata.Get(""ContentProvider"");";
                string expectedEnd =
@"#line 1
// XYZ
public class Foo
{
    // ABC
    public string Bar(this string x) => x;
}

public static class ScriptExtensionMethods
{

}";

                // When
                string actual = ScriptHelper.Parse(code, document.Keys, context);

                // Then
                actual.Substring(0, expectedStart.Length).ShouldBe(expectedStart, StringCompareShould.IgnoreLineEndings);
                actual.Substring(actual.Length - expectedEnd.Length).ShouldBe(expectedEnd, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void ContainsMetadataExtensions()
            {
                // Given
                TestDocument document = new TestDocument();
                TestExecutionContext context = new TestExecutionContext()
                {
                    Namespaces = new TestNamespacesCollection(new[] { "Foo.Bar" })
                };
                string code = "int x = 0;";

                // When
                string actual = ScriptHelper.Parse(code, document.Keys, context);

                // Then
                actual.ShouldContain("public string GetString(string key, string defaultValue = default) => Metadata.GetString(key, defaultValue);");
            }
        }
    }
}

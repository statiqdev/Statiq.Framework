using NUnit.Framework;
using Shouldly;
using Statiq.CodeAnalysis.Scripting;
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
                    Namespaces = new[] { "Foo.Bar" }
                };
                string code = "int x = 0;";
                string expected =
@"using Foo.Bar;
using Statiq.CodeAnalysis.Scripting;

public class Script : ScriptBase
{
public Script(IDocument document, IExecutionContext context) : base(document, context) { }
public override async Task<object> EvaluateAsync()
{
await Task.CompletedTask;
#line 1
int x = 0;
return null;
}

public object Id => Document.Get(""Id"");
public object Source => Document.Get(""Source"");
public object Destination => Document.Get(""Destination"");
public object ContentProvider => Document.Get(""ContentProvider"");
public object HasContent => Document.Get(""HasContent"");
}

public static class ScriptExtensionMethods
{

}";

                // When
                string actual = ScriptHelper.Parse(code, document, context);

                // Then
                actual.ShouldBe(expected, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void OmitsReturnStatement()
            {
                // Given
                TestDocument document = new TestDocument();
                TestExecutionContext context = new TestExecutionContext()
                {
                    Namespaces = new[] { "Foo.Bar" }
                };
                string code = "return 0;";
                string expected =
@"using Foo.Bar;
using Statiq.CodeAnalysis.Scripting;

public class Script : ScriptBase
{
public Script(IDocument document, IExecutionContext context) : base(document, context) { }
public override async Task<object> EvaluateAsync()
{
await Task.CompletedTask;
#line 1
return 0;
return null;
}

public object Id => Document.Get(""Id"");
public object Source => Document.Get(""Source"");
public object Destination => Document.Get(""Destination"");
public object ContentProvider => Document.Get(""ContentProvider"");
public object HasContent => Document.Get(""HasContent"");
}

public static class ScriptExtensionMethods
{

}";

                // When
                string actual = ScriptHelper.Parse(code, document, context);

                // Then
                actual.ShouldBe(expected, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void LiftsClassDeclarations()
            {
                // Given
                TestDocument document = new TestDocument();
                TestExecutionContext context = new TestExecutionContext()
                {
                    Namespaces = new[] { "Foo.Bar" }
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
                string expected =
@"using Foo.Bar;
using Statiq.CodeAnalysis.Scripting;

public class Script : ScriptBase
{
public Script(IDocument document, IExecutionContext context) : base(document, context) { }
public override async Task<object> EvaluateAsync()
{
await Task.CompletedTask;
#line 12
int x = 1 + 2;
Pipelines.Add(Content());

return null;
}

public object Id => Document.Get(""Id"");
public object Source => Document.Get(""Source"");
public object Destination => Document.Get(""Destination"");
public object ContentProvider => Document.Get(""ContentProvider"");
public object HasContent => Document.Get(""HasContent"");
}
#line 1
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
                string actual = ScriptHelper.Parse(code, document, context);

                // Then
                actual.ShouldBe(expected, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void LiftsUsingDirectives()
            {
                // Given
                TestDocument document = new TestDocument();
                TestExecutionContext context = new TestExecutionContext()
                {
                    Namespaces = new[] { "Foo.Bar" }
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
                string expected =
@"using Foo.Bar;
using Statiq.CodeAnalysis.Scripting;
using Red.Blue;
using Yellow;

public class Script : ScriptBase
{
public Script(IDocument document, IExecutionContext context) : base(document, context) { }
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
public object Id => Document.Get(""Id"");
public object Source => Document.Get(""Source"");
public object Destination => Document.Get(""Destination"");
public object ContentProvider => Document.Get(""ContentProvider"");
public object HasContent => Document.Get(""HasContent"");
}
#line 3
public static class Foo
{
    public static string Bar(this string x) => x;
}

public static class ScriptExtensionMethods
{

}";

                // When
                string actual = ScriptHelper.Parse(code, document, context);

                // Then
                actual.ShouldBe(expected, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void LiftsMethodDeclarations()
            {
                // Given
                TestDocument document = new TestDocument();
                TestExecutionContext context = new TestExecutionContext()
                {
                    Namespaces = new[] { "Foo.Bar" }
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
                string expected =
@"using Foo.Bar;
using Statiq.CodeAnalysis.Scripting;

public class Script : ScriptBase
{
public Script(IDocument document, IExecutionContext context) : base(document, context) { }
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
public object Id => Document.Get(""Id"");
public object Source => Document.Get(""Source"");
public object Destination => Document.Get(""Destination"");
public object ContentProvider => Document.Get(""ContentProvider"");
public object HasContent => Document.Get(""HasContent"");
}
#line 1
public static class Foo
{
    public static string Bar(this string x) => x;
}

public static class ScriptExtensionMethods
{

}";

                // When
                string actual = ScriptHelper.Parse(code, document, context);

                // Then
                actual.ShouldBe(expected, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void LiftsExtensionMethodDeclarations()
            {
                // Given
                TestDocument document = new TestDocument();
                TestExecutionContext context = new TestExecutionContext()
                {
                    Namespaces = new[] { "Foo.Bar" }
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
                string expected =
@"using Foo.Bar;
using Statiq.CodeAnalysis.Scripting;

public class Script : ScriptBase
{
public Script(IDocument document, IExecutionContext context) : base(document, context) { }
public override async Task<object> EvaluateAsync()
{
await Task.CompletedTask;
#line 5
Pipelines.Add(Content());

return null;
}

public object Id => Document.Get(""Id"");
public object Source => Document.Get(""Source"");
public object Destination => Document.Get(""Destination"");
public object ContentProvider => Document.Get(""ContentProvider"");
public object HasContent => Document.Get(""HasContent"");
}
#line 1
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
                string actual = ScriptHelper.Parse(code, document, context);

                // Then
                actual.ShouldBe(expected, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void LiftsCommentsWithDeclarations()
            {
                // Given
                TestDocument document = new TestDocument();
                TestExecutionContext context = new TestExecutionContext()
                {
                    Namespaces = new[] { "Foo.Bar" }
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
                string expected =
@"using Foo.Bar;
using Statiq.CodeAnalysis.Scripting;

public class Script : ScriptBase
{
public Script(IDocument document, IExecutionContext context) : base(document, context) { }
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
public object Id => Document.Get(""Id"");
public object Source => Document.Get(""Source"");
public object Destination => Document.Get(""Destination"");
public object ContentProvider => Document.Get(""ContentProvider"");
public object HasContent => Document.Get(""HasContent"");
}
#line 1
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
                string actual = ScriptHelper.Parse(code, document, context);

                // Then
                actual.ShouldBe(expected, StringCompareShould.IgnoreLineEndings);
            }
        }
    }
}

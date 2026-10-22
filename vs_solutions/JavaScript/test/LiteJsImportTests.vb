Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime
Imports Xunit

''' <summary>
''' The <c>import</c> statement: CLR namespace imports (reflection into an assembly)
''' and relative JavaScript module imports.
''' <para>
''' The CLR tests use the sample project that the test project already references
''' (<c>VisualStudio.NET5.vbproj</c>), so the dll sits in the test output directory:
''' <c>import ...VisualStudio.IL as msil from '...dll'</c> then
''' <c>msil.MethodDecompiler.Decompile(m)</c>.
''' </para>
''' <para>
''' Module tests write throw-away .js files in a temp folder and point the engine at
''' them through <see cref="Interpreter.SourceFile"/>, which is what anchors relative
''' specifiers.
''' </para>
''' </summary>
Public Class LiteJsImportTests

    Private Const IlNamespace As String =
        "Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.IL"
    Private Const VsAssembly As String =
        "Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.dll"

    ' ======================================================== helpers

    Private Shared Function RunJs(source As String,
                                  ParamArray globals As (Name As String, Value As Object)()) As List(Of String)
        Return RunEngine(source, Nothing, globals)
    End Function

    ''' <summary>Run a script that lives in <paramref name="scriptDir"/> (module base).</summary>
    Private Shared Function RunJsIn(scriptDir As String, source As String,
                                    ParamArray globals As (Name As String, Value As Object)()) As List(Of String)
        Return RunEngine(source, scriptDir, globals)
    End Function

    Private Shared Function RunEngine(source As String, scriptDir As String,
                                      globals As (Name As String, Value As Object)()) As List(Of String)
        Dim io As New LogTextIO
        Dim engine As New Interpreter(io)
        If scriptDir IsNot Nothing Then
            engine.SourceFile = System.IO.Path.Combine(scriptDir, "main.js")
        End If
        For Each g In globals
            engine.DefineGlobal(g.Name, g.Value)
        Next
        engine.Run(Parser.Parse(source))
        Return io.Lines
    End Function

    Private Shared Function OneLine(source As String,
                                    ParamArray globals As (Name As String, Value As Object)()) As String
        Return RunJs(source, globals)(0)
    End Function

    Private Shared Function Generate(source As String) As String
        Return New CodeGenerator().Generate(Parser.Parse(source))
    End Function

    ''' <summary>Write a temp folder of modules; the caller deletes it.</summary>
    Private Shared Function CreateModules(ParamArray files As (Name As String, Source As String)()) As String
        Dim dir = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                                         "litejs-import-" & Guid.NewGuid().ToString("N"))
        System.IO.Directory.CreateDirectory(dir)
        For Each f In files
            Dim target = System.IO.Path.Combine(dir, f.Name)
            Dim parent = System.IO.Path.GetDirectoryName(target)
            If Not System.IO.Directory.Exists(parent) Then System.IO.Directory.CreateDirectory(parent)
            System.IO.File.WriteAllText(target, f.Source)
        Next
        Return dir
    End Function

    Private Shared Sub RemoveModules(dir As String)
        Try
            System.IO.Directory.Delete(dir, True)
        Catch ex As Exception
        End Try
    End Sub

    ' ======================================================== parsing

    <Fact>
    Public Sub Import_ParsesEveryForm()
        Dim clr = DirectCast(Parser.Parse("import Foo.Bar as ns from 'a.dll';").Body(0), ImportStmt)
        Assert.Equal(ImportKind.ClrNamespace, clr.Kind)
        Assert.Equal("Foo.Bar", clr.NamespaceName)
        Assert.Equal("ns", clr.AliasName)
        Assert.Equal("a.dll", clr.Specifier)

        ' no alias: the last namespace segment is the binding
        Dim noAlias = DirectCast(Parser.Parse("import Foo.Bar from 'a.dll';").Body(0), ImportStmt)
        Assert.Equal(ImportKind.ClrNamespace, noAlias.Kind)
        Assert.Equal("Bar", noAlias.AliasName)

        Dim star = DirectCast(Parser.Parse("import * as u from './u.js';").Body(0), ImportStmt)
        Assert.Equal(ImportKind.NamespaceObject, star.Kind)
        Assert.Equal("u", star.AliasName)

        Dim named = DirectCast(Parser.Parse("import {a, b as c} from './u.js';").Body(0), ImportStmt)
        Assert.Equal(ImportKind.NamedBindings, named.Kind)
        Assert.Equal(2, named.Bindings.Count)
        Assert.Equal("a", named.Bindings(0).Local)
        Assert.Equal("b", named.Bindings(1).Imported)
        Assert.Equal("c", named.Bindings(1).Local)

        Dim defaultAlias = DirectCast(Parser.Parse("import u from './u.js';").Body(0), ImportStmt)
        Assert.Equal(ImportKind.DefaultAlias, defaultAlias.Kind)
        Assert.Equal("u", defaultAlias.AliasName)

        Dim sideEffect = DirectCast(Parser.Parse("import './u.js';").Body(0), ImportStmt)
        Assert.Equal(ImportKind.SideEffect, sideEffect.Kind)
    End Sub

    <Fact>
    Public Sub Import_IsRejectedOutsideTheTopLevel()
        Assert.Throws(Of ParseException)(
            Function() Parser.Parse("function f() { import A as b from 'x.dll'; }"))
        Assert.Throws(Of ParseException)(
            Function() Parser.Parse("if (true) { import a from './m.js'; }"))
    End Sub

    <Fact>
    Public Sub Import_RejectsMalformedDeclarations()
        ' missing 'from'
        Assert.Throws(Of ParseException)(Function() Parser.Parse("import a 'm.js';"))
        ' missing specifier string
        Assert.Throws(Of ParseException)(Function() Parser.Parse("import a from m.js;"))
        ' dotted name / bare 'as' only makes sense for an assembly source
        Assert.Throws(Of ParseException)(Function() Parser.Parse("import Foo.Bar from './m.js';"))
        Assert.Throws(Of ParseException)(Function() Parser.Parse("import a as b from './m.js';"))
        ' a CLR namespace import cannot use '*' or a member list
        Assert.Throws(Of ParseException)(Function() Parser.Parse("import * as ns from 'x.dll';"))
        Assert.Throws(Of ParseException)(Function() Parser.Parse("import {a} from 'x.dll';"))
    End Sub

    ' ======================================================== CLR assembly imports

    <Fact>
    Public Sub Import_ClrNamespaceExposesItsTypes()
        Dim lines = RunJs(
            $"import {IlNamespace} as msil from '{VsAssembly}';" &
            "console.log(typeof msil, typeof msil.MethodDecompiler, Object.keys(msil).indexOf('MethodDecompiler') >= 0);")
        Assert.Equal("object function true", lines(0))
    End Sub

    <Fact>
    Public Sub Import_ClrNamespaceIsFlat()
        ' the host asked for direct types only: no dotted sub-namespace keys
        Assert.Equal("true true", OneLine(
            $"import {IlNamespace} as msil from '{VsAssembly}';" &
            "var keys = Object.keys(msil);" &
            "console.log(keys.length > 1, keys.every(k => k.indexOf('.') < 0));"))
    End Sub

    <Fact>
    Public Sub Import_ClrNamespaceCallsStaticMembers()
        ' the headline case: a static VB Module function with an optional parameter
        Dim target = GetType(ClrHostFixture).GetMethod("StaticSum",
                                                       {GetType(Integer), GetType(Integer)})
        Assert.NotNull(target)

        Dim lines = RunJs(
            $"import {IlNamespace} as msil from '{VsAssembly}';" &
            "var syntax = msil.MethodDecompiler.Decompile(m);" &
            "console.log(typeof syntax, syntax !== null);",
            ("m", target))
        Assert.Equal("object true", lines(0))
    End Sub

    <Fact>
    Public Sub Import_ExplicitDllPathIsAccepted()
        Dim dll = System.IO.Path.Combine(AppContext.BaseDirectory, VsAssembly)
        Assert.True(System.IO.File.Exists(dll), "the sample assembly must be copied to the test output")
        Dim spec = dll.Replace("\"c, "/"c)
        Assert.Equal("function", OneLine(
            $"import {IlNamespace} as msil from '{spec}'; console.log(typeof msil.MethodDecompiler);"))
    End Sub

    <Fact>
    Public Sub Import_MissingAssemblyIsCatchable()
        Dim ex = Assert.Throws(Of JsRuntimeException)(
            Function() RunJs($"import {IlNamespace} as msil from 'no-such-assembly-xyz.dll';"))
        Assert.Contains("cannot load assembly", ex.Message)
    End Sub

    <Fact>
    Public Sub Import_MissingNamespaceIsCatchable()
        Dim ex = Assert.Throws(Of JsRuntimeException)(
            Function() RunJs($"import No.Such.Namespace as ns from '{VsAssembly}';"))
        Assert.Contains("has no public types", ex.Message)
    End Sub

    <Fact>
    Public Sub Import_FallsBackToTheHostResolverCallback()
        Dim asked As String = Nothing
        ClrInterop.AssemblyResolver =
            Function(spec As String) As Reflection.Assembly
                asked = spec
                Return GetType(ClrHostFixture).Assembly
            End Function
        Try
            ' the specifier is not a loaded assembly, not a file and not loadable by name
            Assert.Equal("function", OneLine(
                "import LiteJs.Tests as t from 'host-provided-name.dll'; console.log(typeof t.ClrHostFixture);"))
            Assert.Equal("host-provided-name.dll", asked)
        Finally
            ClrInterop.AssemblyResolver = Nothing
        End Try
    End Sub

    <Fact>
    Public Sub Import_SearchPathApiIsSane()
        ClrInterop.ClearAssemblySearchPaths()
        Try
            ClrInterop.AddAssemblySearchPath(AppContext.BaseDirectory)
            ClrInterop.AddAssemblySearchPath(AppContext.BaseDirectory)      ' duplicate ignored
            Assert.Equal(1, ClrInterop.AssemblySearchPaths.Count)
            ClrInterop.ClearAssemblySearchPaths()
            Assert.Empty(ClrInterop.AssemblySearchPaths)
        Finally
            ClrInterop.ClearAssemblySearchPaths()
        End Try
    End Sub

    ' ======================================================== module imports

    <Fact>
    Public Sub Import_ModuleNamespaceObject()
        Dim dir = CreateModules(("utils.js", "function helper() { return 'helped'; } var answer = 42;"))
        Try
            Assert.Equal("helped 42", RunJsIn(dir,
                "import * as utils from './utils.js'; console.log(utils.helper(), utils.answer);")(0))
            ' the extension may be omitted
            Assert.Equal("helped", RunJsIn(dir,
                "import * as utils from './utils'; console.log(utils.helper());")(0))
        Finally
            RemoveModules(dir)
        End Try
    End Sub

    <Fact>
    Public Sub Import_ModuleNamedBindings()
        Dim dir = CreateModules(("utils.js", "function helper() { return 'helped'; } var answer = 42;"))
        Try
            Assert.Equal("42 helped", RunJsIn(dir,
                "import {answer as a, helper} from './utils.js'; console.log(a, helper());")(0))
        Finally
            RemoveModules(dir)
        End Try
    End Sub

    <Fact>
    Public Sub Import_ModuleDefaultAliasIsTheModuleObject()
        ' no 'export' statement: the default binding is the module surface
        Dim dir = CreateModules(("utils.js", "function helper() { return 'helped'; } var answer = 42;"))
        Try
            Assert.Equal("function 42", RunJsIn(dir,
                "import u from './utils.js'; console.log(typeof u.helper, u.answer);")(0))
        Finally
            RemoveModules(dir)
        End Try
    End Sub

    <Fact>
    Public Sub Import_ModuleSideEffectRunsOnce()
        Dim dir = CreateModules(("once.js", "console.log('once');"))
        Try
            Dim lines = RunJsIn(dir, "import './once.js'; import './once.js'; import * as again from './once.js';")
            Assert.Equal(1, lines.Count)          ' cached by absolute path
            Assert.Equal("once", lines(0))
        Finally
            RemoveModules(dir)
        End Try
    End Sub

    <Fact>
    Public Sub Import_ModuleInsideModuleResolvesRelativeToItsOwnFolder()
        Dim dir = CreateModules(
            ("a.js", "var shallow = 'root';"),
            ("sub/a.js", "var deep = 'deep';"),
            ("sub/b.js", "import * as a from './a.js'; function get() { return a.deep; }"))
        Try
            Assert.Equal("deep", RunJsIn(dir,
                "import * as b from './sub/b.js'; console.log(b.get());")(0))
        Finally
            RemoveModules(dir)
        End Try
    End Sub

    <Fact>
    Public Sub Import_ModuleSeesTheGlobalScopeNotTheImporterLocals()
        Dim dir = CreateModules(
            ("probe.js",
             "function readVar() { return typeof sharedValue; }" &
             "function readLet() { try { return hiddenValue; } catch (e) { return 'no-' + e.name; } }"))
        Try
            ' `var` is function-scoped to the script root — it IS a global, like a classic
            ' script include; `let`/`const` stay in the script's block scope
            Assert.Equal("number no-ReferenceError", RunJsIn(dir,
                "var sharedValue = 1; let hiddenValue = 2;" &
                "import * as probe from './probe.js';" &
                "console.log(probe.readVar(), probe.readLet());")(0))
        Finally
            RemoveModules(dir)
        End Try
    End Sub

    <Fact>
    Public Sub Import_MissingModuleIsCatchable()
        Dim ex = Assert.Throws(Of JsRuntimeException)(
            Function() RunJs("import * as m from './definitely-not-here.js';"))
        Assert.Contains("cannot find module", ex.Message)
    End Sub

    <Fact>
    Public Sub Import_MissingModuleBindingIsCatchable()
        Dim dir = CreateModules(("u.js", "var a = 1;"))
        Try
            Dim ex = Assert.Throws(Of JsRuntimeException)(
                Function() RunJsIn(dir, "import {nope} from './u.js';"))
            Assert.Contains("no top-level binding", ex.Message)
        Finally
            RemoveModules(dir)
        End Try
    End Sub

    <Fact>
    Public Sub Import_CircularModulesAreReported()
        Dim dir = CreateModules(
            ("c1.js", "import * as c2 from './c2.js'; var one = 1;"),
            ("c2.js", "import * as c1 from './c1.js'; var two = 2;"))
        Try
            Dim ex = Assert.Throws(Of JsRuntimeException)(
                Function() RunJsIn(dir, "import * as c1 from './c1.js';"))
            Assert.Contains("circular import", ex.Message)
        Finally
            RemoveModules(dir)
        End Try
    End Sub

    ' ======================================================== code generator

    <Fact>
    Public Sub CodeGen_EmitsTheAssemblyImport()
        Dim code = Generate(
            $"import {IlNamespace} as msil from '{VsAssembly}'; console.log(typeof msil.MethodDecompiler);")
        Assert.Contains("JsRuntime.ImportClrNamespace(", code)
        Assert.Contains($"Dim msil As JsValue = JsRuntime.ImportClrNamespace(""{IlNamespace}"", ""{VsAssembly}"")", code)
        ' the member access itself stays dynamic (resolved by the runtime)
        Assert.Contains("""MethodDecompiler""", code)
    End Sub

    <Fact>
    Public Sub CodeGen_EmitsModuleImports()
        Dim code = Generate(
            "import * as u from './u.js'; import {a as b} from './u.js'; import './u.js'; import d from './u.js';")
        Assert.Contains("JsRuntime.ImportModuleNamespace(""./u.js"")", code)
        Assert.Contains("JsRuntime.ImportModuleBinding(""./u.js"", ""a"")", code)
        Assert.Contains("JsRuntime.ImportModuleSideEffect(""./u.js"")", code)
        Assert.Contains("Dim b As JsValue = ", code)
        Assert.Contains("Dim d As JsValue = JsRuntime.ImportModuleNamespace(", code)
    End Sub

    <Fact>
    Public Sub CodeGen_DoesNotEmitImportStatementsInTheBody()
        Dim code = Generate("import * as u from './u.js'; console.log(u);")
        ' the import is emitted exactly once (its prologue declaration), not as a statement
        Assert.Equal(1, CountOccurrences(code, "ImportModuleNamespace("))
    End Sub

    Private Shared Function CountOccurrences(text As String, token As String) As Integer
        Dim count = 0
        Dim i = text.IndexOf(token, StringComparison.Ordinal)
        While i >= 0
            count += 1
            i = text.IndexOf(token, i + token.Length, StringComparison.Ordinal)
        End While
        Return count
    End Function

End Class

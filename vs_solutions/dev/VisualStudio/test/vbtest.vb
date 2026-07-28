Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Xml.Linq
Imports VBLang.Syntax

Module Program

    Sub Main()
        Try
            TestProject()
        Catch ex As Exception
            Console.WriteLine("[WARN] VBProject.Load test failed: " & ex.Message)
        End Try

        Try
            TestReflection()
        Catch ex As Exception
            Console.WriteLine("[WARN] VBProject.LoadAssembly test failed: " & ex.Message)
            Console.WriteLine(ex.ToString())
        End Try

        Dim src As String = "
Imports System

''' <summary>demo namespace</summary>
Namespace DemoApp

    <Serializable>
    Public Delegate Function Transformer(Of T)(input As T) As T

    ''' <summary>demo class</summary>
    <ExportAPI>
    Public Class DemoClass(Of T As Class)
        Inherits BaseClass
        Implements IDisposable, IComparable

        Public Property Name As String
        Private _value As Integer

        Public Sub New()
            _value = 0
        End Sub

        Public Function Compute(x As Integer, _
                                y As T) As Integer
            Dim a As Integer = x
            Dim b, c As Double
            Const max As Long = 100L
            Return a
        End Function

        Public Shared Operator +(left As DemoClass(Of T), right As DemoClass(Of T)) As DemoClass(Of T)
            Return left
        End Operator

        Private Enum InnerEnum As Byte
            First
            Second = 5
        End Enum
    End Class

End Namespace
"

        Dim root As TypeContainerSymbol = VBParser.Parse(src)
        Dump(root, 0)

        Dim failures As New List(Of String)
        RunAsserts(root, failures)

        If failures.Count = 0 Then
            Console.WriteLine(vbCrLf & "ALL TESTS PASSED")
        Else
            Console.WriteLine(vbCrLf & "FAILURES:")
            For Each f In failures
                Console.WriteLine("  - " & f)
            Next
            Environment.Exit(1)
        End If
    End Sub

    Sub RunAsserts(root As TypeContainerSymbol, failures As List(Of String))
        Dim ns = CType(root.InternalNested("DemoApp"), TypeContainerSymbol)
        Assert(ns IsNot Nothing AndAlso ns.Type = SymbolType.[Namespace], "namespace DemoApp", failures)

        Dim del As LanguageSymbolType = Nothing
        If ns.Members.TryGetValue("Transformer", del) Then
            Dim delT = CType(del, DelegateSymbol)
            Assert(delT.Parameters IsNot Nothing AndAlso delT.Parameters.ContainsKey("input"), "delegate parameter input", failures)
            Assert(delT.ValueType IsNot Nothing AndAlso delT.ValueType.fullName = "T", "delegate return T", failures)
        Else
            Assert(False, "delegate Transformer present", failures)
        End If

        Dim cls = CType(ns.InternalNested("DemoClass"), TypeContainerSymbol)
        Assert(cls IsNot Nothing AndAlso cls.Type = SymbolType.[Class], "class DemoClass", failures)
        Assert(cls.GenericTypeArguments IsNot Nothing AndAlso cls.GenericTypeArguments.Length = 1, "class generic T", failures)
        Assert(cls.InheritsType IsNot Nothing AndAlso cls.InheritsType.fullName = "BaseClass", "Inherits BaseClass", failures)
        Assert(cls.ImplementsInterfaces IsNot Nothing AndAlso cls.ImplementsInterfaces.Length = 2, "Implements 2 interfaces", failures)
        Assert(cls.Attributes IsNot Nothing AndAlso cls.Attributes.Contains("ExportAPI"), "attribute ExportAPI", failures)

        Dim prop = cls.Members("Name")
        Assert(prop IsNot Nothing AndAlso TypeOf prop Is PropertySymbol, "property Name", failures)

        Dim ctor = cls.Members("New")
        Assert(ctor IsNot Nothing AndAlso CType(ctor, MethodSymbol).Type = SymbolType.[New], "Sub New", failures)

        Dim fn = CType(cls.Members("Compute"), MethodSymbol)
        Assert(fn IsNot Nothing, "function Compute", failures)
        Assert(fn.Parameters.ContainsKey("x") AndAlso fn.Parameters("x").fullName = "Integer", "param x As Integer", failures)
        Assert(fn.Parameters.ContainsKey("y") AndAlso fn.Parameters("y").fullName = "T", "param y As T", failures)
        Assert(fn.ReturnType IsNot Nothing AndAlso fn.ReturnType.fullName = "Integer", "return Integer", failures)
        Assert(fn.Locals.ContainsKey("a") AndAlso CType(fn.Locals("a"), VariableSymbol).ValueType.fullName = "Integer", "local a", failures)
        Assert(fn.Locals.ContainsKey("b") AndAlso CType(fn.Locals("b"), VariableSymbol).ValueType.fullName = "Double", "local b", failures)
        Assert(fn.Locals.ContainsKey("c") AndAlso CType(fn.Locals("c"), VariableSymbol).ValueType.fullName = "Double", "local c (shared type)", failures)
        Assert(fn.Locals.ContainsKey("max") AndAlso CType(fn.Locals("max"), VariableSymbol).ValueType.fullName = "Long", "local max", failures)

        Dim op = cls.Members("+")
        Assert(op IsNot Nothing AndAlso CType(op, MethodSymbol).Type = SymbolType.[Operator], "operator +", failures)
        Assert(CType(op, MethodSymbol).ReturnType.fullName = "DemoClass(Of T)", "operator return type", failures)

        Assert(cls.Members.ContainsKey("_value"), "field _value", failures)

        Dim en = CType(cls.InternalNested("InnerEnum"), EnumSymbol)
        Assert(en IsNot Nothing AndAlso en.Type = SymbolType.[Enum], "nested enum", failures)
        Assert(en.EnumBaseType IsNot Nothing AndAlso en.EnumBaseType.fullName = "Byte", "enum base Byte", failures)
    End Sub

    Sub TestProject()
        Dim baseDir = AppContext.BaseDirectory
        Dim dir = baseDir
        Dim vbprojPath As String = Nothing
        While dir IsNot Nothing
            Dim cand = Path.Combine(dir, "VBLang", "VBLang.vbproj")
            If File.Exists(cand) Then
                vbprojPath = cand
                Exit While
            End If
            dir = Path.GetDirectoryName(dir)
        End While

        If vbprojPath Is Nothing Then
            Console.WriteLine("[SKIP] VBProject.Load test: VBLang.vbproj not found")
            Return
        End If

        Console.WriteLine(vbCrLf & "--- VBProject.Load ---")
        Console.WriteLine("vbproj: " & vbprojPath)

        Dim proj As VBProject = VBProject.Load(vbprojPath)
        Console.WriteLine("RootNamespace : " & proj.RootNamespace)
        Console.WriteLine("AssemblyName  : " & proj.AssemblyName)
        Console.WriteLine("OutputType    : " & proj.OutputType)
        Console.WriteLine("Compile files : " & If(proj.CompileFiles, New VBDocument() {}).Length)

        For Each doc In proj.CompileFiles
            Console.WriteLine("  " & doc.FileName & "  ->  " & doc.Types.Count & " top-level types, " & doc.Imports.Length & " imports")
        Next

        Console.WriteLine(vbCrLf & "--- VBProject metadata / references ---")
        Console.WriteLine("Sdk              : " & If(proj.Sdk, ""))
        Console.WriteLine("TargetFramework  : " & If(proj.Metadata?.TargetFramework, ""))
        Console.WriteLine("TargetFrameworks : " & If(proj.Metadata?.TargetFrameworks, ""))
        Console.WriteLine("Platforms        : " & If(proj.Metadata?.Platforms, ""))
        Console.WriteLine("Configurations   : " & If(proj.Configurations, New VBBuildConfiguration() {}).Length)
        For Each cfg In If(proj.Configurations, New VBBuildConfiguration() {})
            Console.WriteLine("  [" & If(cfg.Configuration, "") & "|" & If(cfg.Platform, "") & "] DefineConstants=" & If(cfg.DefineConstants, "") & " Optimize=" & cfg.Optimize & " OutputPath=" & If(cfg.OutputPath, ""))
        Next
        Console.WriteLine("ProjectReferences: " & If(proj.ProjectReferences, New VBProjectReference() {}).Length)
        For Each pr In If(proj.ProjectReferences, New VBProjectReference() {})
            Console.WriteLine("  -> " & pr.Include)
        Next
        Console.WriteLine("PackageReferences: " & If(proj.PackageReferences, New VBPackageReference() {}).Length)
        For Each pkg In If(proj.PackageReferences, New VBPackageReference() {})
            Console.WriteLine("  -> " & pkg.Id & " v" & If(pkg.Version, ""))
        Next
        Console.WriteLine("CompileExcludes  : " & If(proj.CompileExcludes, New String() {}).Length)

        Console.WriteLine(vbCrLf & "--- VBProject.Generate ---")
        Dim gen As XDocument = proj.Generate()
        Dim xml As String = gen.ToString()
        Console.WriteLine(xml)
        Dim check = Sub(cond As Boolean, label As String)
                        Console.WriteLine((If(cond, "[OK]   ", "[MISS] ")) & label)
                    End Sub
        check(xml.Contains("<Project"), "Generate: Project root")
        check(xml.Contains("Sdk="), "Generate: Sdk attribute")
        check(xml.Contains("<Compile"), "Generate: Compile items")
        check(If(proj.ProjectReferences, New VBProjectReference() {}).Length = 0 OrElse xml.Contains("<ProjectReference"), "Generate: ProjectReference items")
        check(If(proj.PackageReferences, New VBPackageReference() {}).Length = 0 OrElse xml.Contains("<PackageReference"), "Generate: PackageReference items")

        Console.WriteLine(vbCrLf & "--- synthetic config + nuget round-trip ---")
        Dim synthetic As New VBProject()
        synthetic.Sdk = "Microsoft.NET.Sdk"
        synthetic.RootNamespace = "Demo"
        synthetic.AssemblyName = "Demo"
        synthetic.OutputType = "Exe"
        synthetic.Metadata = New VBProjectMetadata() With {.TargetFramework = "net8.0", .Nullable = "enable", .Other = New Dictionary(Of String, String)()}
        synthetic.NuGet = New VBNuGetMetadata() With {.PackageId = "Demo", .Version = "1.2.3", .Authors = "Me", .Other = New Dictionary(Of String, String)()}
        Dim cfgDebug As New VBBuildConfiguration() With {
            .Condition = "'$(Configuration)|$(Platform)' == 'Debug|AnyCPU'",
            .Configuration = "Debug", .Platform = "AnyCPU",
            .DefineConstants = "TRACE;DEBUG", .Optimize = False,
            .Extra = New Dictionary(Of String, String)()}
        Dim cfgRelease As New VBBuildConfiguration() With {
            .Condition = "'$(Configuration)|$(Platform)' == 'Release|AnyCPU'",
            .Configuration = "Release", .Platform = "AnyCPU",
            .DefineConstants = "TRACE", .Optimize = True,
            .Extra = New Dictionary(Of String, String)()}
        synthetic.Configurations = New VBBuildConfiguration() {cfgDebug, cfgRelease}
        Dim synXml As String = synthetic.Generate().ToString()
        Console.WriteLine(synXml)
        check(synXml.Contains("'$(Configuration)|$(Platform)' == 'Debug|AnyCPU'"), "synthetic: Debug conditional group")
        check(synXml.Contains("'$(Configuration)|$(Platform)' == 'Release|AnyCPU'"), "synthetic: Release conditional group")
        check(synXml.Contains("<PackageId>Demo</PackageId>"), "synthetic: nuget PackageId")
        check(synXml.Contains("<Optimize>true</Optimize>"), "synthetic: Release Optimize=true")

        Console.WriteLine(vbCrLf & "--- VBProject.GetType ---")
        Dim probes As String() = {
            "VBLang.VBDocument",
            "VBLang.VBProject",
            "VBLang.Syntax.VBParser",
            "VBLang.LanguageSymbolType",
            "VBLang.Syntax.TokenKind",
            "VBLang.Syntax.VBScanner",
            "VBLang.VBDocument(Of T)",
            "VBParser",
            "NotExist.Type"
        }
        For Each p In probes
            Dim sym = proj.GetType(p)
            If sym Is Nothing Then
                Console.WriteLine("  " & p & " -> NOT FOUND")
            Else
                Console.WriteLine("  " & p & " -> " & sym.Type.ToString() & " " & sym.Name)
            End If
        Next
    End Sub

    Sub TestReflection()
        ' Load the VBLang assembly itself (copied next to the test exe) via
        ' read-only reflection and verify the symbol tree & queries.
        Dim dllPath As String = Path.Combine(AppContext.BaseDirectory, "VBLang.dll")
        If Not File.Exists(dllPath) Then
            Console.WriteLine("[SKIP] Reflection test: " & dllPath & " not found")
            Return
        End If

        Console.WriteLine(vbCrLf & "--- VBProject.LoadAssembly (reflection) ---")
        Console.WriteLine("dll: " & dllPath)

        Dim proj As VBProject = VBProject.LoadAssembly(dllPath)
        Console.WriteLine("AssemblyName  : " & proj.AssemblyName)
        Console.WriteLine("OutputType    : " & proj.OutputType)
        Console.WriteLine("Compile files : " & If(proj.CompileFiles, New VBDocument() {}).Length)

        Dim doc = proj.CompileFiles(0)
        Console.WriteLine("Virtual doc   : " & doc.FileName)
        Console.WriteLine("Top namespaces/types: " & doc.Types.Count)

        ' Dump the tree through a synthetic root so we reuse Dump().
        Dim root As New NamespaceSymbol()
        root.Name = ""
        root.InternalNested = New Dictionary(Of String, LanguageSymbolType)(doc.Types)
        Dump(root, 0)

        Dim failures As New List(Of String)

        Dim probes As String() = {
            "VBLang.VBProject",
            "VBLang.VBDocument",
            "VBLang.LanguageSymbolType",
            "VBLang.TypeContainerSymbol",
            "VBLang.SymbolType",
            "VBLang.Reflection.AssemblySymbolLoader",
            "VBLang.EventSymbol"
        }
        For Each p In probes
            Assert(proj.GetType(p) IsNot Nothing, "reflection GetType: " & p, failures)
        Next

        ' VBProject should be a Class carrying the loader + source Load members.
        Dim vbproj = proj.GetType("VBLang.VBProject")
        Assert(vbproj IsNot Nothing AndAlso vbproj.Type = SymbolType.Class, "VBProject is a Class", failures)
        Dim vpCt = CType(vbproj, TypeContainerSymbol)
        Assert(vpCt.Members IsNot Nothing AndAlso vpCt.Members.ContainsKey("LoadAssembly"), "VBProject has LoadAssembly member", failures)
        Assert(vpCt.Members.ContainsKey("Load"), "VBProject has Load member", failures)

        ' Tree shape: namespaces hold their types (not flattened).
        Dim vblangNs = proj.GetType("VBLang")
        Assert(vblangNs IsNot Nothing AndAlso vblangNs.Type = SymbolType.Namespace, "VBLang namespace exists (tree)", failures)
        Dim nsCt = CType(vblangNs, TypeContainerSymbol)
        Assert(nsCt.InternalNested IsNot Nothing AndAlso nsCt.InternalNested.ContainsKey("VBProject"), "VBProject nested under VBLang namespace (not flat)", failures)
        Assert(nsCt.InternalNested.ContainsKey("Reflection"), "Reflection namespace nested under VBLang (not flat)", failures)

        If failures.Count = 0 Then
            Console.WriteLine(vbCrLf & "REFLECTION TESTS PASSED")
        Else
            Console.WriteLine(vbCrLf & "REFLECTION FAILURES:")
            For Each f In failures
                Console.WriteLine("  - " & f)
            Next
            Environment.Exit(1)
        End If
    End Sub

    Sub Assert(cond As Boolean, label As String, failures As List(Of String))
        If Not cond Then
            failures.Add(label)
        End If
        Console.WriteLine((If(cond, "[OK]   ", "[FAIL] ")) & label)
    End Sub

    Sub Dump(c As TypeContainerSymbol, indent As Integer)
        Dim pad As String = New String(" "c, indent * 2)
        Console.WriteLine($"{pad}{c.Type} {c.Name} (generic={If(c.GenericTypeArguments Is Nothing, 0, c.GenericTypeArguments.Length)})")
        If c.InheritsType IsNot Nothing Then Console.WriteLine($"{pad}  Inherits {c.InheritsType.fullName}")
        If c.ImplementsInterfaces IsNot Nothing Then
            For Each i In c.ImplementsInterfaces
                Console.WriteLine($"{pad}  Implements {i.fullName}")
            Next
        End If
        If c.InternalNested IsNot Nothing Then
            For Each kv In c.InternalNested
                If TypeOf kv.Value Is TypeContainerSymbol Then Dump(CType(kv.Value, TypeContainerSymbol), indent + 1)
            Next
        End If
        If c.Members IsNot Nothing Then
            For Each kv In c.Members
                Dim m = kv.Value
                If TypeOf m Is TypeContainerSymbol Then
                    Dump(CType(m, TypeContainerSymbol), indent + 1)
                ElseIf TypeOf m Is MethodSymbol Then
                    Dim inv = CType(m, MethodSymbol)
                    Console.WriteLine($"{pad}  {inv.Type} {inv.Name} As {If(inv.ReturnType Is Nothing, "-", inv.ReturnType.fullName)}")
                    If inv.Locals IsNot Nothing Then
                        For Each lv In inv.Locals
                            Console.WriteLine($"{pad}    var {lv.Value.Name} As {If(lv.Value.ValueType Is Nothing, "-", lv.Value.ValueType.fullName)}")
                        Next
                    End If
                ElseIf TypeOf m Is PropertySymbol Then
                    Dim p = CType(m, PropertySymbol)
                    Console.WriteLine($"{pad}  {p.Type} {p.Name} As {If(p.ReturnType Is Nothing, "-", p.ReturnType.fullName)}")
                ElseIf TypeOf m Is DelegateSymbol Then
                    Dim d = CType(m, DelegateSymbol)
                    Console.WriteLine($"{pad}  Delegate {d.Name} As {If(d.ValueType Is Nothing, "-", d.ValueType.fullName)}")
                ElseIf TypeOf m Is VariableSymbol Then
                    Dim v = CType(m, VariableSymbol)
                    Console.WriteLine($"{pad}  var {v.Name} As {If(v.ValueType Is Nothing, "-", v.ValueType.fullName)}")
                Else
                    Console.WriteLine($"{pad}  {m.Type} {m.Name}")
                End If
            Next
        End If
    End Sub

End Module

Imports System.IO
Imports System.Text.RegularExpressions
Imports VBLang.Reflection
Imports VBLang.Syntax

Public Class VBDocument

    ''' <summary>
    ''' relative path to the vbproj file
    ''' </summary>
    ''' <returns></returns>
    Public Property FileName As String
    ''' <summary>
    ''' namespace imports list
    ''' </summary>
    ''' <returns></returns>
    Public Property [Imports] As String()
    ''' <summary>
    ''' language symbols that parsed from current vb.net source file document text
    ''' </summary>
    ''' <returns></returns>
    Public Property Types As Dictionary(Of String, LanguageSymbolType)

End Class

Public Class [Imports]

    ''' <summary>
    ''' Imports XXX
    ''' </summary>
    ''' <returns></returns>
    Public Property [Namespace] As String
    ''' <summary>
    ''' Imports X = XXX
    ''' </summary>
    ''' <returns></returns>
    Public Property [Alias] As String

    Public Overrides Function ToString() As String
        If [Alias].StringEmpty(, True) Then
            Return $"Imports {[Namespace]}"
        Else
            Return $"Imports {[Alias]} = {[Namespace]}"
        End If
    End Function

End Class

''' <summary>
''' external project reference (ProjectReference item)
''' </summary>
Public Class VBProjectReference

    ''' <summary>relative or absolute path to the referenced vbproj</summary>
    Public Property [Include] As String
    Public Property Condition As String
    Public Property Aliases As String
    Public Property [Private] As String
    Public Property SetTargetFramework As String
    Public Property ReferenceOutputAssembly As String

End Class

''' <summary>
''' nuget package reference (PackageReference item)
''' </summary>
Public Class VBPackageReference

    ''' <summary>package id (the Include attribute)</summary>
    Public Property Id As String
    Public Property Version As String
    Public Property Condition As String
    Public Property IncludeAssets As String
    Public Property ExcludeAssets As String
    Public Property PrivateAssets As String

End Class

''' <summary>
''' a single build configuration derived from a conditional PropertyGroup
''' (e.g. '$(Configuration)|$(Platform)' == 'Debug|AnyCPU').
''' </summary>
Public Class VBBuildConfiguration

    ''' <summary>the raw Condition string of the source PropertyGroup</summary>
    Public Property Condition As String
    ''' <summary>Debug / Release / ...</summary>
    Public Property Configuration As String
    ''' <summary>AnyCPU / x64 / ...</summary>
    Public Property Platform As String
    Public Property DefineConstants As String
    Public Property Optimize As Boolean
    Public Property DebugType As String
    Public Property OutputPath As String
    ''' <summary>any other properties declared inside the conditional PropertyGroup</summary>
    Public Property Extra As Dictionary(Of String, String)

End Class

''' <summary>
''' generic, non-packaging project metadata parsed from PropertyGroup elements.
''' </summary>
Public Class VBProjectMetadata

    Public Property TargetFramework As String
    Public Property TargetFrameworks As String
    Public Property Platforms As String
    Public Property Nullable As String
    Public Property LangVersion As String
    Public Property ImplicitUsings As String
    Public Property EnableDefaultCompileItems As String
    Public Property SignAssembly As String
    Public Property AssemblyOriginatorKeyFile As String
    ''' <summary>any unrecognized project properties</summary>
    Public Property Other As Dictionary(Of String, String)

End Class

''' <summary>
''' nuget / packaging metadata parsed from PropertyGroup elements.
''' </summary>
Public Class VBNuGetMetadata

    Public Property PackageId As String
    Public Property Version As String
    Public Property Authors As String
    Public Property Company As String
    Public Property Product As String
    Public Property Description As String
    Public Property Copyright As String
    Public Property PackageLicenseExpression As String
    Public Property PackageProjectUrl As String
    Public Property PackageTags As String
    Public Property PackageReadmeFile As String
    Public Property RepositoryUrl As String
    Public Property RepositoryType As String
    Public Property GeneratePackageOnBuild As String
    Public Property RestoreSources As String
    ''' <summary>any other unrecognized packaging properties</summary>
    Public Property Other As Dictionary(Of String, String)

End Class

''' <summary>
''' vbproj file model
''' </summary>
Public Class VBProject

    Public Property RootNamespace As String
    Public Property AssemblyName As String
    Public Property OutputType As String
    Public Property CompileFiles As VBDocument()

    ''' <summary>the Sdk attribute of the root Project element (e.g. Microsoft.NET.Sdk)</summary>
    Public Property Sdk As String
    ''' <summary>generic project metadata (TargetFramework, Platforms, LangVersion, ...)</summary>
    Public Property Metadata As VBProjectMetadata
    ''' <summary>packaging / nuget metadata</summary>
    Public Property NuGet As VBNuGetMetadata
    ''' <summary>conditional build configurations (Debug/Release x Platform)</summary>
    Public Property Configurations As VBBuildConfiguration()
    ''' <summary>external project references</summary>
    Public Property ProjectReferences As VBProjectReference()
    ''' <summary>nuget package references</summary>
    Public Property PackageReferences As VBPackageReference()
    ''' <summary>Compile Remove patterns collected from the vbproj</summary>
    Public Property CompileExcludes As String()

    ''' <summary>
    ''' Get symbol via fullname
    ''' </summary>
    ''' <param name="fullName">namespace + type symbol name</param>
    ''' <returns></returns>
    Public Overloads Function [GetType](fullName As String) As LanguageSymbolType
        If String.IsNullOrWhiteSpace(fullName) OrElse CompileFiles Is Nothing Then
            Return Nothing
        End If

        Dim clean As String = StripGenerics(fullName).Trim()

        ' Candidate full names to try: the name as-is and, when a project
        ' RootNamespace is declared, the name with the RootNamespace prefix
        ' stripped or added. The real VB full name equals
        ' RootNamespace + namespace + type name.
        Dim candidates As New List(Of String) From {clean}
        If Not String.IsNullOrWhiteSpace(RootNamespace) Then
            Dim sep As String = RootNamespace & "."
            If clean.StartsWith(sep, StringComparison.OrdinalIgnoreCase) Then
                candidates.Add(clean.Substring(sep.Length))
            Else
                candidates.Add(sep & clean)
            End If
        End If

        For Each cand In candidates
            Dim segs = cand.Split(New Char() {"."c}, StringSplitOptions.RemoveEmptyEntries)
            If segs.Length = 0 Then Continue For
            For Each doc In CompileFiles
                If doc IsNot Nothing AndAlso doc.Types IsNot Nothing Then
                    Dim hit = FindInContainer(doc.Types, segs, 0)
                    If hit IsNot Nothing Then Return hit
                End If
            Next
        Next

        ' Fallback: match by the trailing type name anywhere in the tree.
        Dim lastName As String = clean.Split(New Char() {"."c}, StringSplitOptions.RemoveEmptyEntries).Last()
        For Each doc In CompileFiles
            If doc IsNot Nothing AndAlso doc.Types IsNot Nothing Then
                Dim hit = FindByLastName(doc.Types, lastName)
                If hit IsNot Nothing Then Return hit
            End If
        Next

        Return Nothing
    End Function

    ' Walk a container's symbol dictionary segment by segment (case
    ' insensitive, generics stripped) looking for the requested type.
    Private Shared Function FindInContainer(children As Dictionary(Of String, LanguageSymbolType), segs As String(), index As Integer) As LanguageSymbolType
        If children Is Nothing Then Return Nothing
        Dim key As String = CleanName(segs(index))
        Dim sym As LanguageSymbolType = Nothing
        For Each kv In children
            If String.Equals(CleanName(kv.Key), key, StringComparison.OrdinalIgnoreCase) Then
                sym = kv.Value
                Exit For
            End If
        Next
        If sym Is Nothing Then Return Nothing

        If index = segs.Length - 1 Then Return sym

        Dim ct = TryCast(sym, TypeContainerSymbol)
        If ct Is Nothing Then Return Nothing

        Dim deeper = FindInContainer(ct.InternalNested, segs, index + 1)
        If deeper IsNot Nothing Then Return deeper
        Return FindInContainer(ct.Members, segs, index + 1)
    End Function

    ' Depth-first search the whole tree for the first symbol whose simple
    ' name (generics stripped) equals lastName.
    Private Shared Function FindByLastName(children As Dictionary(Of String, LanguageSymbolType), lastName As String) As LanguageSymbolType
        If children Is Nothing Then Return Nothing
        For Each kv In children
            Dim sym = kv.Value
            If String.Equals(CleanName(sym.Name), lastName, StringComparison.OrdinalIgnoreCase) Then
                Return sym
            End If
            Dim ct = TryCast(sym, TypeContainerSymbol)
            If ct IsNot Nothing Then
                Dim deeper = FindByLastName(ct.InternalNested, lastName)
                If deeper IsNot Nothing Then Return deeper
                deeper = FindByLastName(ct.Members, lastName)
                If deeper IsNot Nothing Then Return deeper
            End If
        Next
        Return Nothing
    End Function

    Private Shared Function CleanName(name As String) As String
        If name Is Nothing Then Return ""
        Dim m As Match = Regex.Match(name, "\(Of[^)]*\)", RegexOptions.IgnoreCase)
        If m.Success Then name = name.Remove(m.Index, m.Length)
        Return name.Trim()
    End Function

    Private Shared Function StripGenerics(fullName As String) As String
        If fullName Is Nothing Then Return ""
        Return Regex.Replace(fullName, "\(Of[^)]*\)", "", RegexOptions.IgnoreCase).Trim()
    End Function

    ''' <summary>
    ''' Load a .NET assembly (dll) via reflection and map its symbols into a
    ''' virtual VBProject. The assembly metadata is read only (no execution).
    ''' </summary>
    ''' <param name="dllPath">path to the target dll file.</param>
    Public Shared Function LoadAssembly(dllPath As String) As VBProject
        Return AssemblySymbolLoader.LoadAssembly(dllPath)
    End Function

    ''' <summary>
    ''' Parse vbproj xml file and the vb source files
    ''' </summary>
    ''' <param name="vbproj"></param>
    ''' <returns></returns>
    Public Shared Function Load(vbproj As String) As VBProject
        Dim projDir As String = Path.GetDirectoryName(Path.GetFullPath(vbproj))

        Dim doc As XDocument = XDocument.Load(vbproj)
        Dim ns As XNamespace = If(doc.Root Is Nothing, "", doc.Root.Name.Namespace)

        Dim proj As New VBProject()
        proj.Sdk = If(doc.Root IsNot Nothing, If(doc.Root.Attribute("Sdk")?.Value, ""), "")
        ParseProperties(doc, ns, proj)

        Dim files As String() = CollectCompileFiles(doc, ns, projDir)
        Dim docs As New List(Of VBDocument)

        For Each rel In files
            Dim full As String = Path.Combine(projDir, rel)

            If Not File.Exists(full) Then
                Continue For
            End If

            Dim code As String = Nothing
            Try
                code = File.ReadAllText(full)
            Catch
                Continue For
            End Try

            Dim vbdoc As New VBDocument()
            vbdoc.FileName = rel
            vbdoc.Imports = ExtractImports(code)

            Try
                Dim root As TypeContainerSymbol = VBParser.Parse(code)
                If root.InternalNested IsNot Nothing Then
                    vbdoc.Types = New Dictionary(Of String, LanguageSymbolType)(root.InternalNested)
                Else
                    vbdoc.Types = New Dictionary(Of String, LanguageSymbolType)()
                End If
            Catch
                vbdoc.Types = New Dictionary(Of String, LanguageSymbolType)()
            End Try

            docs.Add(vbdoc)
        Next

        ParseItemGroups(doc, ns, proj)
        proj.CompileFiles = docs.ToArray()
        Return proj
    End Function

    Private Shared Function ReadProperty(doc As XDocument, ns As XNamespace, name As String) As String
        If doc.Root Is Nothing Then Return ""
        For Each pg In doc.Root.Elements(ns + "PropertyGroup")
            Dim el = pg.Element(ns + name)
            If el IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(el.Value) Then
                Return el.Value.Trim()
            End If
        Next
        Return ""
    End Function

    Private Shared Function CollectCompileFiles(doc As XDocument, ns As XNamespace, projDir As String) As String()
        Dim includes As New List(Of String)
        Dim removes As New List(Of String)

        If doc.Root IsNot Nothing Then
            For Each ig In doc.Root.Elements(ns + "ItemGroup")
                For Each c In ig.Elements(ns + "Compile")
                    Dim inc = c.Attribute("Include")?.Value
                    If inc IsNot Nothing Then includes.Add(NormalizePath(inc))
                    Dim remAttr = c.Attribute("Remove")?.Value
                    If remAttr IsNot Nothing Then removes.Add(NormalizePath(remAttr))
                Next
            Next
        End If

        Dim defaultDisabled As Boolean = ReadProperty(doc, ns, "EnableDefaultCompileItems").Equals("false", StringComparison.OrdinalIgnoreCase)

        Dim result As New List(Of String)

        If includes.Count = 0 AndAlso Not defaultDisabled Then
            If Directory.Exists(projDir) Then
                Try
                    For Each f In Directory.GetFiles(projDir, "*.vb", SearchOption.AllDirectories)
                        Dim rel = GetRelativePath(projDir, f)
                        If Not IsExcludedByDefault(rel) Then
                            result.Add(rel)
                        End If
                    Next
                Catch
                End Try
            End If
        Else
            result.AddRange(includes)
        End If

        If removes.Count > 0 Then
            result.RemoveAll(Function(p) removes.Any(Function(r) GlobMatch(r, p)))
        End If

        Return result.ToArray()
    End Function

    Private Shared Function NormalizePath(p As String) As String
        Dim s = p.Trim()
        While s.StartsWith(".\") OrElse s.StartsWith("./")
            s = s.Substring(2)
        End While
        Return s.Replace("/", "\")
    End Function

    Private Shared Function GetRelativePath(baseDir As String, file As String) As String
        Dim b = Path.GetFullPath(baseDir).TrimEnd("\"c, "/"c) & "\"
        Dim f = Path.GetFullPath(file)
        Dim uriB = New Uri(b)
        Dim uriF = New Uri(f)
        Dim rel = Uri.UnescapeDataString(uriB.MakeRelativeUri(uriF).ToString())
        Return rel.Replace("/", "\")
    End Function

    Private Shared Function IsExcludedByDefault(rel As String) As Boolean
        Dim lower = rel.Replace("\", "/").ToLowerInvariant()
        Return lower.Contains("/obj/") OrElse lower.Contains("/bin/") OrElse lower.StartsWith("obj/") OrElse lower.StartsWith("bin/")
    End Function

    Private Shared Function GlobMatch(pattern As String, path As String) As Boolean
        Dim p = pattern.Replace("\", "/").ToLowerInvariant()
        Dim s = path.Replace("\", "/").ToLowerInvariant()
        Dim rx As String = "^"
        Dim i As Integer = 0
        While i < p.Length
            Dim c As Char = p(i)
            If c = "*"c Then
                If i + 1 < p.Length AndAlso p(i + 1) = "*"c Then
                    rx &= ".*"
                    i += 1
                    If i + 1 < p.Length AndAlso p(i + 1) = "/"c Then i += 1
                Else
                    rx &= "[^/]*"
                End If
            ElseIf c = "?"c Then
                rx &= "."
            Else
                rx &= Regex.Escape(c.ToString())
            End If
            i += 1
        End While
        rx &= "$"
        Return Regex.IsMatch(s, rx)
    End Function

    ' Extract Imports statements that VBParser.Parse silently ignores.
    Private Shared Function ExtractImports(source As String) As String()
        Dim scanner As New VBScanner()
        Dim stmts = scanner.Scan(source)
        Dim list As New List(Of String)

        For Each stmt In stmts
            If stmt.Tokens.Count = 0 Then Continue For
            If Not stmt.Tokens(0).Text.Equals("imports", StringComparison.OrdinalIgnoreCase) Then Continue For

            Dim rest As New List(Of Token)
            For k As Integer = 1 To stmt.Tokens.Count - 1
                rest.Add(stmt.Tokens(k))
            Next

            ' skip xml namespace imports : Imports <xmlns:...>
            If rest.Count > 0 AndAlso rest(0).Text = "<"c Then Continue For

            For Each seg In SplitImports(rest)
                Dim txt = String.Join("", seg.[Select](Function(t) t.Text).ToArray()).Trim()
                If txt.Length > 0 Then list.Add(txt)
            Next
        Next

        Return list.ToArray()
    End Function

    Private Shared Function SplitImports(tokens As List(Of Token)) As List(Of List(Of Token))
        Dim result As New List(Of List(Of Token))
        Dim cur As New List(Of Token)
        Dim depth As Integer = 0

        For Each t In tokens
            If t.Text = "("c Then
                depth += 1
                cur.Add(t)
            ElseIf t.Text = ")"c Then
                depth -= 1
                cur.Add(t)
            ElseIf t.Text = ","c AndAlso depth = 0 Then
                result.Add(cur)
                cur = New List(Of Token)()
            Else
                cur.Add(t)
            End If
        Next

        result.Add(cur)
        Return result
    End Function

    ''' <summary>
    ''' Parse every PropertyGroup (conditional or not) into the strongly typed
    ''' metadata / nuget / build-configuration models.
    ''' </summary>
    Private Shared Sub ParseProperties(doc As XDocument, ns As XNamespace, proj As VBProject)
        proj.Metadata = New VBProjectMetadata()
        proj.Metadata.Other = New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        proj.NuGet = New VBNuGetMetadata()
        proj.NuGet.Other = New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

        Dim configs As New Dictionary(Of String, VBBuildConfiguration)(StringComparer.OrdinalIgnoreCase)

        If doc.Root Is Nothing Then
            proj.Configurations = New VBBuildConfiguration() {}
            Return
        End If

        For Each pg In doc.Root.Elements(ns + "PropertyGroup")
            Dim condAttr = If(pg.Attribute("Condition")?.Value, "").Trim()
            Dim hasCond = condAttr.Length > 0

            For Each el In pg.Elements()
                Dim name = el.Name.LocalName
                Dim value = If(el.Value, "").Trim()
                If value.Length = 0 Then Continue For

                If hasCond Then
                    Dim cfg As VBBuildConfiguration = Nothing
                    If Not configs.TryGetValue(condAttr, cfg) Then
                        cfg = New VBBuildConfiguration()
                        cfg.Condition = condAttr
                        Dim cp = ParseConfigCondition(condAttr)
                        cfg.Configuration = cp.Configuration
                        cfg.Platform = cp.Platform
                        cfg.Extra = New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
                        configs(condAttr) = cfg
                    End If
                    Select Case name
                        Case "DefineConstants" : cfg.DefineConstants = value
                        Case "Optimize" : cfg.Optimize = value.Equals("true", StringComparison.OrdinalIgnoreCase)
                        Case "DebugType" : cfg.DebugType = value
                        Case "OutputPath" : cfg.OutputPath = value
                        Case Else : cfg.Extra(name) = value
                    End Select
                ElseIf IsNuGetProperty(name) Then
                    SetNuGetProperty(proj.NuGet, name, value)
                Else
                    SetMetadataProperty(proj.Metadata, proj, name, value)
                End If
            Next
        Next

        proj.Configurations = configs.Values.ToArray()
    End Sub

    Private Shared Sub SetMetadataProperty(meta As VBProjectMetadata, proj As VBProject, name As String, value As String)
        Select Case name
            Case "TargetFramework" : meta.TargetFramework = value
            Case "TargetFrameworks" : meta.TargetFrameworks = value
            Case "Platforms" : meta.Platforms = value
            Case "Nullable" : meta.Nullable = value
            Case "LangVersion" : meta.LangVersion = value
            Case "ImplicitUsings" : meta.ImplicitUsings = value
            Case "EnableDefaultCompileItems" : meta.EnableDefaultCompileItems = value
            Case "SignAssembly" : meta.SignAssembly = value
            Case "AssemblyOriginatorKeyFile" : meta.AssemblyOriginatorKeyFile = value
            Case "RootNamespace" : proj.RootNamespace = value
            Case "AssemblyName" : proj.AssemblyName = value
            Case "OutputType" : proj.OutputType = value
            Case Else : meta.Other(name) = value
        End Select
    End Sub

    Private Shared Function IsNuGetProperty(name As String) As Boolean
        Select Case name
            Case "PackageId", "Version", "Authors", "Company", "Product", "Description", _
                 "Copyright", "PackageLicenseExpression", "PackageProjectUrl", "PackageTags", _
                 "PackageReadmeFile", "PackageReleaseNotes", "PackageOutputPath", _
                 "RepositoryUrl", "RepositoryType", "GeneratePackageOnBuild", _
                 "RestoreSources", "RestoreAdditionalProjectSources", "NoPackageAnalysis", _
                 "IncludeSymbols", "SymbolPackageFormat", "DevelopmentDependency"
                Return True
            Case Else
                Return name.StartsWith("Package", StringComparison.OrdinalIgnoreCase) OrElse _
                       name.StartsWith("Repository", StringComparison.OrdinalIgnoreCase)
        End Select
    End Function

    Private Shared Sub SetNuGetProperty(nuget As VBNuGetMetadata, name As String, value As String)
        Select Case name
            Case "PackageId" : nuget.PackageId = value
            Case "Version" : nuget.Version = value
            Case "Authors" : nuget.Authors = value
            Case "Company" : nuget.Company = value
            Case "Product" : nuget.Product = value
            Case "Description" : nuget.Description = value
            Case "Copyright" : nuget.Copyright = value
            Case "PackageLicenseExpression" : nuget.PackageLicenseExpression = value
            Case "PackageProjectUrl" : nuget.PackageProjectUrl = value
            Case "PackageTags" : nuget.PackageTags = value
            Case "PackageReadmeFile" : nuget.PackageReadmeFile = value
            Case "RepositoryUrl" : nuget.RepositoryUrl = value
            Case "RepositoryType" : nuget.RepositoryType = value
            Case "GeneratePackageOnBuild" : nuget.GeneratePackageOnBuild = value
            Case "RestoreSources" : nuget.RestoreSources = value
            Case Else : nuget.Other(name) = value
        End Select
    End Sub

    ''' <summary>
    ''' Extract Configuration / Platform from a conditional PropertyGroup
    ''' Condition such as '$(Configuration)|$(Platform)' == 'Debug|AnyCPU'.
    ''' </summary>
    Private Shared Function ParseConfigCondition(cond As String) As (Configuration As String, Platform As String)
        Dim configuration As String = ""
        Dim platform As String = ""
        Dim m As Match = Regex.Match(cond, "==\s*'([^']*)'")
        If m.Success Then
            Dim val As String = m.Groups(1).Value
            If val.Contains("|"c) Then
                Dim parts = val.Split(New Char() {"|"c}, 2)
                configuration = parts(0).Trim()
                If parts.Length > 1 Then platform = parts(1).Trim()
            Else
                configuration = val.Trim()
            End If
        End If
        Return (configuration, platform)
    End Function

    ''' <summary>
    ''' Parse ItemGroup elements: ProjectReference, PackageReference and Compile
    ''' Remove patterns.
    ''' </summary>
    Private Shared Sub ParseItemGroups(doc As XDocument, ns As XNamespace, proj As VBProject)
        Dim projRefs As New List(Of VBProjectReference)
        Dim pkgRefs As New List(Of VBPackageReference)
        Dim compileRemoves As New List(Of String)

        If doc.Root IsNot Nothing Then
            For Each ig In doc.Root.Elements(ns + "ItemGroup")
                For Each c In ig.Elements()
                    Dim local = c.Name.LocalName
                    Dim inc = c.Attribute("Include")?.Value
                    Dim remAttr = c.Attribute("Remove")?.Value

                    Select Case local
                        Case "ProjectReference"
                            If inc IsNot Nothing Then
                                Dim pr As New VBProjectReference()
                                pr.Include = NormalizePath(inc)
                                pr.Condition = If(c.Attribute("Condition")?.Value, "")
                                pr.Aliases = ElementValue(c, ns, "Aliases")
                                pr.Private = ElementValue(c, ns, "Private")
                                pr.SetTargetFramework = ElementValue(c, ns, "SetTargetFramework")
                                pr.ReferenceOutputAssembly = ElementValue(c, ns, "ReferenceOutputAssembly")
                                projRefs.Add(pr)
                            End If
                        Case "PackageReference"
                            If inc IsNot Nothing Then
                                Dim pkg As New VBPackageReference()
                                pkg.Id = inc.Trim()
                                pkg.Version = If(c.Attribute("Version")?.Value, ElementValue(c, ns, "Version"))
                                pkg.Condition = If(c.Attribute("Condition")?.Value, "")
                                pkg.IncludeAssets = ElementValue(c, ns, "IncludeAssets")
                                pkg.ExcludeAssets = ElementValue(c, ns, "ExcludeAssets")
                                pkg.PrivateAssets = ElementValue(c, ns, "PrivateAssets")
                                pkgRefs.Add(pkg)
                            End If
                        Case "Compile"
                            If remAttr IsNot Nothing Then compileRemoves.Add(NormalizePath(remAttr))
                    End Select
                Next
            Next
        End If

        proj.ProjectReferences = projRefs.ToArray()
        proj.PackageReferences = pkgRefs.ToArray()
        proj.CompileExcludes = compileRemoves.ToArray()
    End Sub

    Private Shared Function ElementValue(parent As XElement, ns As XNamespace, name As String) As String
        Dim el = parent.Element(ns + name)
        If el Is Nothing Then Return ""
        Return If(el.Value, "").Trim()
    End Function

    ''' <summary>
    ''' Generate a clean, canonical SDK-style vbproj document from this model.
    ''' </summary>
    Public Function Generate() As XDocument
        Dim root As New XElement("Project")
        If Not String.IsNullOrWhiteSpace(Sdk) Then
            root.SetAttributeValue("Sdk", Sdk)
        Else
            root.SetAttributeValue("Sdk", "Microsoft.NET.Sdk")
        End If

        ' Main property group: known scalar + project metadata.
        Dim mainPg As New XElement("PropertyGroup")
        AddIf(mainPg, "TargetFramework", Metadata?.TargetFramework)
        AddIf(mainPg, "TargetFrameworks", Metadata?.TargetFrameworks)
        AddIf(mainPg, "RootNamespace", RootNamespace)
        AddIf(mainPg, "AssemblyName", AssemblyName)
        AddIf(mainPg, "OutputType", OutputType)
        AddIf(mainPg, "Platforms", Metadata?.Platforms)
        AddIf(mainPg, "Nullable", Metadata?.Nullable)
        AddIf(mainPg, "LangVersion", Metadata?.LangVersion)
        AddIf(mainPg, "ImplicitUsings", Metadata?.ImplicitUsings)
        AddIf(mainPg, "EnableDefaultCompileItems", Metadata?.EnableDefaultCompileItems)
        AddIf(mainPg, "SignAssembly", Metadata?.SignAssembly)
        AddIf(mainPg, "AssemblyOriginatorKeyFile", Metadata?.AssemblyOriginatorKeyFile)
        If Metadata?.Other IsNot Nothing Then
            For Each kv In Metadata.Other
                AddIf(mainPg, kv.Key, kv.Value)
            Next
        End If
        If mainPg.Elements().Any() Then root.Add(mainPg)

        ' NuGet / packaging property group.
        Dim nugetPg As New XElement("PropertyGroup")
        AddIf(nugetPg, "PackageId", NuGet?.PackageId)
        AddIf(nugetPg, "Version", NuGet?.Version)
        AddIf(nugetPg, "Authors", NuGet?.Authors)
        AddIf(nugetPg, "Company", NuGet?.Company)
        AddIf(nugetPg, "Product", NuGet?.Product)
        AddIf(nugetPg, "Description", NuGet?.Description)
        AddIf(nugetPg, "Copyright", NuGet?.Copyright)
        AddIf(nugetPg, "PackageLicenseExpression", NuGet?.PackageLicenseExpression)
        AddIf(nugetPg, "PackageProjectUrl", NuGet?.PackageProjectUrl)
        AddIf(nugetPg, "PackageTags", NuGet?.PackageTags)
        AddIf(nugetPg, "PackageReadmeFile", NuGet?.PackageReadmeFile)
        AddIf(nugetPg, "RepositoryUrl", NuGet?.RepositoryUrl)
        AddIf(nugetPg, "RepositoryType", NuGet?.RepositoryType)
        AddIf(nugetPg, "GeneratePackageOnBuild", NuGet?.GeneratePackageOnBuild)
        AddIf(nugetPg, "RestoreSources", NuGet?.RestoreSources)
        If NuGet?.Other IsNot Nothing Then
            For Each kv In NuGet.Other
                AddIf(nugetPg, kv.Key, kv.Value)
            Next
        End If
        If nugetPg.Elements().Any() Then root.Add(nugetPg)

        ' Conditional build configuration property groups.
        If Configurations IsNot Nothing Then
            For Each cfg In Configurations
                Dim pg As New XElement("PropertyGroup")
                If Not String.IsNullOrWhiteSpace(cfg.Condition) Then
                    pg.SetAttributeValue("Condition", cfg.Condition)
                End If
                AddIf(pg, "DefineConstants", cfg.DefineConstants)
                AddIf(pg, "Optimize", If(cfg.Optimize, "true", ""))
                AddIf(pg, "DebugType", cfg.DebugType)
                AddIf(pg, "OutputPath", cfg.OutputPath)
                If cfg.Extra IsNot Nothing Then
                    For Each kv In cfg.Extra
                        AddIf(pg, kv.Key, kv.Value)
                    Next
                End If
                If pg.Elements().Any() Then root.Add(pg)
            Next
        End If

        ' Compile items (Remove first, then Include).
        Dim hasCompile As Boolean = (CompileFiles IsNot Nothing AndAlso CompileFiles.Length > 0) OrElse
                                    (CompileExcludes IsNot Nothing AndAlso CompileExcludes.Length > 0)
        If hasCompile Then
            Dim ig As New XElement("ItemGroup")
            If CompileExcludes IsNot Nothing Then
                For Each ex In CompileExcludes
                    ig.Add(New XElement("Compile", New XAttribute("Remove", ex)))
                Next
            End If
            If CompileFiles IsNot Nothing Then
                For Each doc In CompileFiles
                    ig.Add(New XElement("Compile", New XAttribute("Include", doc.FileName)))
                Next
            End If
            root.Add(ig)
        End If

        ' Project / package references.
        Dim hasRefs As Boolean = (ProjectReferences IsNot Nothing AndAlso ProjectReferences.Length > 0) OrElse
                                (PackageReferences IsNot Nothing AndAlso PackageReferences.Length > 0)
        If hasRefs Then
            Dim ig As New XElement("ItemGroup")
            If ProjectReferences IsNot Nothing Then
                For Each pr In ProjectReferences
                    Dim el As New XElement("ProjectReference", New XAttribute("Include", pr.Include))
                    AddAttrIf(el, "Condition", pr.Condition)
                    AddIf(el, "Aliases", pr.Aliases)
                    AddIf(el, "Private", pr.Private)
                    AddIf(el, "SetTargetFramework", pr.SetTargetFramework)
                    AddIf(el, "ReferenceOutputAssembly", pr.ReferenceOutputAssembly)
                    ig.Add(el)
                Next
            End If
            If PackageReferences IsNot Nothing Then
                For Each pkg In PackageReferences
                    Dim el As New XElement("PackageReference", New XAttribute("Include", pkg.Id))
                    AddAttrIf(el, "Version", pkg.Version)
                    AddAttrIf(el, "Condition", pkg.Condition)
                    AddIf(el, "IncludeAssets", pkg.IncludeAssets)
                    AddIf(el, "ExcludeAssets", pkg.ExcludeAssets)
                    AddIf(el, "PrivateAssets", pkg.PrivateAssets)
                    ig.Add(el)
                Next
            End If
            root.Add(ig)
        End If

        Return New XDocument(root)
    End Function

    ''' <summary>
    ''' Serialize this model back to a clean SDK-style vbproj file on disk.
    ''' </summary>
    Public Sub Save(path As String)
        Dim doc = Generate()
        doc.Save(path)
    End Sub

    Private Shared Sub AddIf(parent As XElement, name As String, value As String)
        If Not String.IsNullOrWhiteSpace(value) Then
            parent.Add(New XElement(parent.Name.Namespace + name, value))
        End If
    End Sub

    Private Shared Sub AddAttrIf(el As XElement, name As String, value As String)
        If Not String.IsNullOrWhiteSpace(value) Then
            el.SetAttributeValue(name, value)
        End If
    End Sub


End Class
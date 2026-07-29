#Region "Microsoft.VisualBasic::67989829e32dedb0ffbd9439821f1bf0, vs_solutions\dev\VisualStudio\VBProject\Project\VBProject.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 630
    '    Code Lines: 497 (78.89%)
    ' Comment Lines: 64 (10.16%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 69 (10.95%)
    '     File Size: 30.57 KB


    '     Class VBProject
    ' 
    '         Properties: AssemblyName, AssemblyVersion, CompileExcludes, CompileFiles, Configurations
    '                     FilePath, IsDotNetCoreSDK, Metadata, MimeType, NuGet
    '                     OutputType, PackageReferences, ProjectReferences, RootNamespace, Sdk
    ' 
    '         Function: [GetType], CleanName, ElementValue, ExtractImports, FindByLastName
    '                   FindInContainer, Generate, IsNuGetProperty, Load, LoadAssembly
    '                   LoadProjectXml, ParseConfigCondition, SplitImports, StripGenerics
    ' 
    '         Sub: AddAttrIf, AddIf, ParseItemGroups, ParseProperties, Save
    '              SetMetadataProperty, SetNuGetProperty
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj.ProjectXml
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj.Reflection
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj.Syntax
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports Microsoft.VisualBasic.Scripting.Expressions

Namespace VBProj

    ''' <summary>
    ''' vbproj file model
    ''' </summary>
    Public Class VBProject : Implements IFileReference

        Public Property RootNamespace As String
        Public Property AssemblyName As String
        Public Property AssemblyVersion As String
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

        Public ReadOnly Property IsDotNetCoreSDK As Boolean
            Get
                Return Sdk = "Microsoft.NET.Sdk"
            End Get
        End Property

        Private Property FilePath As String Implements IFileReference.FilePath
        Private ReadOnly Property MimeType As ContentType() Implements IFileReference.MimeType
            Get
                Return {
                    New ContentType With {
                        .Details = "VisualStudio Project",
                        .FileExt = ".vbproj",
                        .MIMEType = "visualstudio/xml-project",
                        .Name = "Project"
                    }
                }
            End Get
        End Property

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
            Dim proj As VBProject = LoadProjectXml(vbproj)
            Dim projDir As String = DirectCast(proj, IFileReference).FilePath.ParentPath.GetFullPath
            Dim doc As XDocument = XDocument.Load(vbproj)
            Dim ns As XNamespace = If(doc.Root Is Nothing, "", doc.Root.Name.Namespace)
            Dim files As String() = CollectCompileFiles(doc, ns, projDir)
            Dim docs As New List(Of VBDocument)

            For Each rel As String In files
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

            proj.CompileFiles = docs.ToArray()

            Return proj
        End Function

        ''' <summary>
        ''' Just read the vbproj xml file
        ''' </summary>
        ''' <param name="vbproj"></param>
        ''' <returns></returns>
        Public Shared Function LoadProjectXml(vbproj As String) As VBProject
            Dim projDir As String = Path.GetDirectoryName(Path.GetFullPath(vbproj))
            Dim doc As XDocument = XDocument.Load(vbproj)
            Dim ns As XNamespace = If(doc.Root Is Nothing, "", doc.Root.Name.Namespace)

            Dim proj As New VBProject() With {
                .Sdk = If(doc.Root IsNot Nothing, If(doc.Root.Attribute("Sdk")?.Value, ""), ""),
                .FilePath = vbproj.GetFullPath
            }

            ParseProperties(doc, ns, proj)
            ParseItemGroups(doc, ns, proj)

            Return proj
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
                Case "PackageId", "Version", "Authors", "Company", "Product", "Description",
                 "Copyright", "PackageLicenseExpression", "PackageProjectUrl", "PackageTags",
                 "PackageReadmeFile", "PackageReleaseNotes", "PackageOutputPath",
                 "RepositoryUrl", "RepositoryType", "GeneratePackageOnBuild",
                 "RestoreSources", "RestoreAdditionalProjectSources", "NoPackageAnalysis",
                 "IncludeSymbols", "SymbolPackageFormat", "DevelopmentDependency"
                    Return True
                Case Else
                    Return name.StartsWith("Package", StringComparison.OrdinalIgnoreCase) OrElse
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
End Namespace

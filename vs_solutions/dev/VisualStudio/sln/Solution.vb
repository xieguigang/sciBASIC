#Region "Microsoft.VisualBasic::0110500a1a13881ab3099e5d683e5f1d, vs_solutions\dev\VisualStudio\sln\Solution.vb"

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

'   Total Lines: 37
'    Code Lines: 20 (54.05%)
' Comment Lines: 11 (29.73%)
'    - Xml Docs: 100.00%
' 
'   Blank Lines: 6 (16.22%)
'     File Size: 1.14 KB


'     Class Solution
' 
'         Properties: FormatVersion, MinimumVisualStudioVersion, Projects, VisualStudioVersion
' 
'     Enum TypeId
' 
' 
'  
' 
' 
' 
'     Class Project
' 
'         Properties: Guid, Name, NodeType, TreePath
' 
' 
' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Text
Imports System.Xml

Namespace sln

    ''' <summary>
    ''' Microsoft Visual Studio Solution File, works for both classic .sln (text)
    ''' and the new .slnx (XML) solution formats.
    ''' </summary>
    Public Class Solution

        Public Property FormatVersion As String
        Public Property VisualStudioVersion As String
        Public Property MinimumVisualStudioVersion As String

        ''' <summary>
        ''' The projects and solution folders declared in the solution.
        ''' </summary>
        Public Property Projects As New List(Of Project)

        ''' <summary>
        ''' Solution level build configurations / platforms, e.g. ``Debug|AnyCPU``.
        ''' </summary>
        Public Property Configurations As New List(Of SolutionConfiguration)

        ''' <summary>
        ''' Global section key/value pairs (e.g. ``SolutionGuid``).
        ''' </summary>
        Public Property [Global] As New [Global]

        ''' <summary>
        ''' The file path of the solution that this model was parsed from.
        ''' </summary>
        Public Property FilePath As String

        ''' <summary>
        ''' True when the source file was a ``.slnx`` (XML) solution.
        ''' </summary>
        Public Property IsXmlFormat As Boolean

        ''' <summary>
        ''' The solution GUID, mirrored from <see cref="[Global].SolutionGuid"/>.
        ''' Setting this also updates the underlying global property.
        ''' </summary>
        Public Property SolutionGuid As String
            Get
                Return [Global].SolutionGuid
            End Get
            Set(value As String)
                [Global].SolutionGuid = value

                If Not String.IsNullOrEmpty(value) Then
                    [Global].Properties("SolutionGuid") = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' Resolve the full path of a project relative to the solution file.
        ''' </summary>
        Public Function GetProjectFullPath(p As Project) As String
            If String.IsNullOrEmpty(p.RelativePath) Then
                Return String.Empty
            End If

            If IO.Path.IsPathRooted(p.RelativePath) Then
                Return p.RelativePath
            End If

            If String.IsNullOrEmpty(FilePath) Then
                Return p.RelativePath
            End If

            Return IO.Path.GetFullPath(IO.Path.Combine(IO.Path.GetDirectoryName(FilePath), p.RelativePath))
        End Function

        ''' <summary>
        ''' Find a project or solution folder by its GUID (case-insensitive).
        ''' </summary>
        Public Function GetProject(guid As String) As Project
            If String.IsNullOrEmpty(guid) Then
                Return Nothing
            End If

            Dim g As String = guid.ToUpperInvariant()

            For Each p In Projects
                If p.Guid IsNot Nothing AndAlso p.Guid.ToUpperInvariant() = g Then
                    Return p
                End If
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' Get all top-level nodes (no parent solution folder).
        ''' </summary>
        Public Function GetRootProjects() As List(Of Project)
            Dim roots As New List(Of Project)

            For Each p In Projects
                If String.IsNullOrEmpty(p.ParentGuid) Then
                    roots.Add(p)
                End If
            Next

            Return roots
        End Function

        ''' <summary>
        ''' Get the direct children of the given parent solution folder GUID.
        ''' </summary>
        Public Function GetChildProjects(parentGuid As String) As List(Of Project)
            Dim children As New List(Of Project)
            Dim g As String = If(parentGuid Is Nothing, "", parentGuid.ToUpperInvariant())

            For Each p In Projects
                If p.ParentGuid IsNot Nothing AndAlso p.ParentGuid.ToUpperInvariant() = g Then
                    children.Add(p)
                End If
            Next

            Return children
        End Function

        Public Shared Function Load(sln As String) As Solution
            Return Parser.Parse(path:=sln)
        End Function

        ''' <summary>
        ''' Save this solution model as a ``.slnx`` (XML) file. The output is
        ''' symmetric with <see cref="Parser.ParseSlnx"/> so it can be re-read.
        ''' </summary>
        ''' <param name="path">
        ''' The target ``.slnx`` file path. When omitted, <see cref="FilePath"/> is used.
        ''' </param>
        Public Sub Save(Optional path As String = Nothing)
            Dim target As String = If(path, FilePath)

            If String.IsNullOrEmpty(target) Then
                Throw New ArgumentNullException(NameOf(path), "A target path must be supplied (FilePath is empty).")
            End If

            FilePath = target
            IsXmlFormat = True

            Dim doc As New XmlDocument()
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", Nothing))

            Dim root As XmlElement = doc.CreateElement("Solution")
            doc.AppendChild(root)

            If Not String.IsNullOrEmpty(FormatVersion) Then
                root.SetAttribute("Version", FormatVersion)
            End If

            If Not String.IsNullOrEmpty(VisualStudioVersion) Then
                root.SetAttribute("VisualStudioVersion", VisualStudioVersion)
            End If

            If Not String.IsNullOrEmpty(MinimumVisualStudioVersion) Then
                root.SetAttribute("MinimumVisualStudioVersion", MinimumVisualStudioVersion)
            End If

            If Not String.IsNullOrEmpty([Global].SolutionGuid) Then
                root.SetAttribute("Id", [Global].SolutionGuid)
            End If

            ' Emit project tree recursively from roots down.
            For Each root_p In GetRootProjects()
                WriteProjectNode(root, root_p, doc)
            Next

            ' Solution level configurations.
            For Each cfg In Configurations
                Dim cfgEl As XmlElement = doc.CreateElement("Configuration")
                cfgEl.SetAttribute("Name", cfg.Name)
                root.AppendChild(cfgEl)
            Next

            ' Remaining global properties (skip SolutionGuid, already on root).
            For Each kv In [Global].Properties
                If String.Equals(kv.Key, "SolutionGuid", StringComparison.OrdinalIgnoreCase) Then
                    Continue For
                End If

                ' Configuration_* entries are produced by the parser's round-trip,
                ' not genuine global properties, so they are skipped on save.
                If kv.Key.StartsWith("Configuration_", StringComparison.OrdinalIgnoreCase) Then
                    Continue For
                End If

                Dim propEl As XmlElement = doc.CreateElement("Property")
                propEl.SetAttribute("Name", kv.Key)
                propEl.SetAttribute("Value", kv.Value)
                root.AppendChild(propEl)
            Next

            IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(IO.Path.GetFullPath(target)))

            Using writer As New XmlTextWriter(target, New UTF8Encoding(encoderShouldEmitUTF8Identifier:=True))
                writer.Formatting = Formatting.Indented
                writer.Indentation = 2
                doc.WriteTo(writer)
            End Using
        End Sub

        ''' <summary>
        ''' Recursively write a project / folder node and its children into the
        ''' slnx XML tree. Folders become ``&lt;Folder&gt;`` elements, projects
        ''' become ``&lt;Project&gt;`` elements.
        ''' </summary>
        Private Sub WriteProjectNode(parent As XmlNode, p As Project, doc As XmlDocument)
            Dim el As XmlElement

            If p.IsFolder Then
                el = doc.CreateElement("Folder")
                el.SetAttribute("Name", If(p.Name, ""))

                If Not String.IsNullOrEmpty(p.Guid) Then
                    el.SetAttribute("Guid", p.Guid)
                End If
            Else
                el = doc.CreateElement("Project")
                el.SetAttribute("Path", If(p.RelativePath, ""))
                el.SetAttribute("Name", If(p.Name, ""))

                If Not String.IsNullOrEmpty(p.Guid) Then
                    el.SetAttribute("Guid", p.Guid)
                End If

                If Not String.IsNullOrEmpty(p.TypeGuid) Then
                    el.SetAttribute("Type", p.TypeGuid)
                ElseIf p.NodeType <> TypeId.Unknown Then
                    el.SetAttribute("Type", GetTypeGuid(p.NodeType))
                End If
            End If

            parent.AppendChild(el)

            ' Recurse into children so hierarchy is preserved as nesting.
            For Each child In GetChildProjects(p.Guid)
                WriteProjectNode(el, child, doc)
            Next
        End Sub

        ''' <summary>
        ''' Resolve the project type GUID for an enum value via its
        ''' <see cref="DescriptionAttribute"/> (inverse of <c>Parser.ResolveType</c>).
        ''' </summary>
        Private Function GetTypeGuid(type As TypeId) As String
            Dim field = GetType(TypeId).GetField(type.ToString())
            Dim attr = CType(Attribute.GetCustomAttribute(field, GetType(DescriptionAttribute)), DescriptionAttribute)

            If attr IsNot Nothing Then
                Return attr.Description
            End If

            Return String.Empty
        End Function
    End Class

    ''' <summary>
    ''' A solution level build configuration / platform pair, e.g. ``Debug|AnyCPU``.
    ''' </summary>
    Public Class SolutionConfiguration
        ''' <summary>
        ''' The combined name, e.g. ``Debug|AnyCPU``.
        ''' </summary>
        Public Property Name As String
        ''' <summary>
        ''' The configuration part, e.g. ``Debug``.
        ''' </summary>
        Public Property Configuration As String
        ''' <summary>
        ''' The platform part, e.g. ``AnyCPU``.
        ''' </summary>
        Public Property Platform As String

        Public Sub New()
        End Sub

        Public Sub New(name As String)
            Me.Name = name

            If name IsNot Nothing Then
                Dim parts = name.Split({"|"c}, 2)
                Configuration = parts(0)

                If parts.Length > 1 Then
                    Platform = parts(1)
                End If
            End If
        End Sub
    End Class


End Namespace

#Region "Microsoft.VisualBasic::60cf4d51dfcde12abf7b12a673a1bd68, vs_solutions\dev\VisualStudio\sln\File\SlnxWriter.vb"

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

    '   Total Lines: 129
    '    Code Lines: 88 (68.22%)
    ' Comment Lines: 16 (12.40%)
    '    - Xml Docs: 56.25%
    ' 
    '   Blank Lines: 25 (19.38%)
    '     File Size: 5.21 KB


    '     Module SlnxWriter
    ' 
    '         Function: GetTypeGuid
    ' 
    '         Sub: SaveSlnx, WriteProjectNode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Xml

Namespace sln.File

    Module SlnxWriter

        <Extension>
        Public Sub SaveSlnx(sln As Solution, target As String)
            Dim doc As New XmlDocument()
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", Nothing))

            Dim root As XmlElement = doc.CreateElement("Solution")
            doc.AppendChild(root)

            If Not String.IsNullOrEmpty(sln.FormatVersion) Then
                root.SetAttribute("Version", sln.FormatVersion)
            End If

            If Not String.IsNullOrEmpty(sln.VisualStudioVersion) Then
                root.SetAttribute("VisualStudioVersion", sln.VisualStudioVersion)
            End If

            If Not String.IsNullOrEmpty(sln.MinimumVisualStudioVersion) Then
                root.SetAttribute("MinimumVisualStudioVersion", sln.MinimumVisualStudioVersion)
            End If

            If Not String.IsNullOrEmpty(sln.[Global].SolutionGuid) Then
                root.SetAttribute("Id", sln.[Global].SolutionGuid)
            End If

            ' Emit project tree recursively from roots down.
            For Each root_p As Project In sln.GetRootProjects()
                Call sln.WriteProjectNode(root, root_p, doc)
            Next

            ' Solution level configurations.
            For Each cfg As SolutionConfiguration In sln.Configurations
                Dim cfgEl As XmlElement = doc.CreateElement("Configuration")
                cfgEl.SetAttribute("Name", cfg.Name)
                root.AppendChild(cfgEl)
            Next

            ' Remaining global properties (skip SolutionGuid, already on root).
            For Each kv As KeyValuePair(Of String, String) In sln.[Global].Properties
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
        ''' 
        <Extension>
        Private Sub WriteProjectNode(sln As Solution, parent As XmlNode, p As Project, doc As XmlDocument)
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
            For Each child In sln.GetChildProjects(p.Guid)
                Call sln.WriteProjectNode(el, child, doc)
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
    End Module
End Namespace

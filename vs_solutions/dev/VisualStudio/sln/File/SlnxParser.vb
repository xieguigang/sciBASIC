#Region "Microsoft.VisualBasic::7900abd0869855d7fe3509a3ec7395af, vs_solutions\dev\VisualStudio\sln\File\SlnxParser.vb"

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

    '   Total Lines: 134
    '    Code Lines: 92 (68.66%)
    ' Comment Lines: 12 (8.96%)
    '    - Xml Docs: 25.00%
    ' 
    '   Blank Lines: 30 (22.39%)
    '     File Size: 5.30 KB


    '     Module SlnxParser
    ' 
    '         Function: GetAttr, ParseSlnx
    ' 
    '         Sub: ParseSlnxProjects
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Text
Imports System.Xml

Namespace sln.File

    Module SlnxParser

        ' ------------------------------------------------------------------
        ' New .slnx (XML) parsing
        ' ------------------------------------------------------------------

        ''' <summary>
        ''' Parse a Visual Studio ``.slnx`` XML solution file (VS2022 17.10+).
        ''' </summary>
        Public Function ParseSlnx(path As String) As Solution
            If Not path.FileExists() Then
                Return Nothing
            End If

            Dim solution As New Solution With {
                .FilePath = path,
                .IsXmlFormat = True
            }

            Dim doc As New XmlDocument()
            doc.Load(path)

            Dim ns As XmlNamespaceManager = New XmlNamespaceManager(doc.NameTable)

            Dim root As XmlElement = doc.DocumentElement
            solution.FormatVersion = GetAttr(root, "Version")
            solution.VisualStudioVersion = GetAttr(root, "VisualStudioVersion")
            solution.MinimumVisualStudioVersion = GetAttr(root, "MinimumVisualStudioVersion")

            ' <Project> nodes (may be nested under <Folder> for hierarchy)
            ParseSlnxProjects(root, solution, parentGuid:="", ns:=ns)

            ' <Configuration> / solution configurations
            For Each cfgNode As XmlNode In root.SelectNodes("Configuration | SolutionConfiguration", ns)
                Dim name As String = GetAttr(cfgNode, "Name")

                If String.IsNullOrEmpty(name) Then
                    name = cfgNode.InnerText
                End If

                If Not String.IsNullOrEmpty(name) Then
                    solution.Configurations.Add(New SolutionConfiguration(name))
                End If
            Next

            ' Global solution properties
            For Each propNode As XmlNode In root.SelectNodes("Property | Properties/Property", ns)
                Dim key As String = GetAttr(propNode, "Name")
                Dim value As String = GetAttr(propNode, "Value")

                If String.IsNullOrEmpty(value) Then
                    value = propNode.InnerText
                End If

                If Not String.IsNullOrEmpty(key) Then
                    solution.Global.Properties(key) = value

                    If String.Equals(key, "SolutionGuid", StringComparison.OrdinalIgnoreCase) Then
                        solution.Global.SolutionGuid = value
                    End If
                End If
            Next

            ' Resolve full paths.
            For Each p In solution.Projects
                p.FullPath = solution.GetProjectFullPath(p)
            Next

            Return solution
        End Function

        Private Sub ParseSlnxProjects(node As XmlNode, solution As Solution, parentGuid As String, ns As XmlNamespaceManager)
            For Each child As XmlNode In node.ChildNodes
                If child.NodeType <> XmlNodeType.Element Then
                    Continue For
                End If

                Dim el As XmlElement = CType(child, XmlElement)

                Select Case el.LocalName
                    Case "Project"
                        Dim p As New Project()
                        p.Guid = NormalizeGuid(GetAttr(el, "Guid"))
                        p.Name = GetAttr(el, "Name")
                        p.TypeGuid = NormalizeGuid(GetAttr(el, "Type"))
                        p.NodeType = ResolveType(p.TypeGuid)

                        If String.IsNullOrEmpty(p.Name) Then
                            p.Name = GetAttr(el, "Path")
                        End If

                        p.RelativePath = GetAttr(el, "Path")
                        p.TreePath = p.RelativePath
                        p.ParentGuid = parentGuid

                        solution.Projects.Add(p)

                        ' A project does not nest further in slnx.
                    Case "Folder"
                        Dim folderGuid As String = NormalizeGuid(GetAttr(el, "Guid"))

                        If String.IsNullOrEmpty(folderGuid) Then
                            folderGuid = "{" & Guid.NewGuid().ToString().ToUpperInvariant() & "}"
                        End If

                        Dim folder As New Project()
                        folder.Guid = folderGuid
                        folder.Name = GetAttr(el, "Name")
                        folder.TypeGuid = "2150E333-8FDC-42A3-9474-1A3956D46DE8"
                        folder.NodeType = TypeId.FolderGroup
                        folder.ParentGuid = parentGuid
                        solution.Projects.Add(folder)

                        ' recurse into nested projects/folders
                        ParseSlnxProjects(el, solution, folderGuid, ns)
                End Select
            Next
        End Sub

        Private Function GetAttr(el As XmlElement, name As String) As String
            If el Is Nothing OrElse el.Attributes(name) Is Nothing Then
                Return String.Empty
            End If

            Return el.Attributes(name).Value
        End Function
    End Module
End Namespace

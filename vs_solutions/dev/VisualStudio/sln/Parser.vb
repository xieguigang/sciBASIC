#Region "Microsoft.VisualBasic::5c82bda0bb990af0990b08d170b115d4, vs_solutions\dev\VisualStudio\sln\Parser.vb"

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

    '   Total Lines: 434
    '    Code Lines: 292 (67.28%)
    ' Comment Lines: 48 (11.06%)
    '    - Xml Docs: 52.08%
    ' 
    '   Blank Lines: 94 (21.66%)
    '     File Size: 16.82 KB


    '     Module Parser
    ' 
    '         Function: ExtractSectionName, GetAttr, NormalizeGuid, Parse, ParseKeyValueSection
    '                   ParseProjectDeclaration, ParseSln, ParseSlnx, ParseSolutionConfigurations, ResolveType
    '                   SkipSection, SplitKeyValue, SplitQuoted, StripQuotes
    ' 
    '         Sub: ParseSlnxProjects
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.IO
Imports System.Text
Imports System.Xml

Namespace sln

    ''' <summary>
    ''' Parses Visual Studio solution files, both the classic ``.sln`` (text) format
    ''' and the new ``.slnx`` (XML) format, into a unified <see cref="Solution"/> model.
    ''' </summary>
    Module Parser

        ''' <summary>
        ''' Parse a solution file, automatically dispatching on its extension.
        ''' </summary>
        Public Function Parse(path As String) As Solution
            If String.IsNullOrEmpty(path) Then
                Return Nothing
            End If

            Select Case System.IO.Path.GetExtension(path).ToLowerInvariant()
                Case ".slnx"
                    Return ParseSlnx(path)
                Case Else
                    Return ParseSln(path)
            End Select
        End Function

        ' ------------------------------------------------------------------
        ' Classic .sln (text) parsing
        ' ------------------------------------------------------------------

        ''' <summary>
        ''' Parse a classic Visual Studio ``.sln`` text solution file.
        ''' </summary>
        Public Function ParseSln(path As String) As Solution
            If Not File.Exists(path) Then
                Return Nothing
            End If

            Dim solution As New Solution With {
                .FilePath = path,
                .IsXmlFormat = False
            }

            Dim lines As String() = File.ReadAllLines(path, Encoding.UTF8)
            Dim nested As New Dictionary(Of String, String)
            Dim i As Integer = 0

            Do While i < lines.Length
                Dim line As String = lines(i).Trim()

                If line.StartsWith("Microsoft Visual Studio Solution File") Then
                    ' header, format version is parsed below
                ElseIf line.StartsWith("Format Version") Then
                    solution.FormatVersion = line.Substring(line.IndexOf("Format Version") + "Format Version".Length).Trim()
                ElseIf line.StartsWith("# Visual Studio") Then
                    solution.VisualStudioVersion = line.Substring(line.IndexOf("# Visual Studio") + "# Visual Studio".Length).Trim()
                ElseIf line.StartsWith("VisualStudioVersion") Then
                    solution.VisualStudioVersion = SplitKeyValue(line).Value
                ElseIf line.StartsWith("MinimumVisualStudioVersion") Then
                    solution.MinimumVisualStudioVersion = SplitKeyValue(line).Value
                ElseIf line.StartsWith("Project(") Then
                    Dim p As Project = ParseProjectDeclaration(line)
                    solution.Projects.Add(p)
                ElseIf line.StartsWith("GlobalSection") Then
                    Dim sectionName As String = ExtractSectionName(line)

                    If sectionName = "SolutionConfigurationPlatforms" Then
                        i = ParseSolutionConfigurations(lines, i, solution)
                    ElseIf sectionName = "NestedProjects" Then
                        i = ParseKeyValueSection(lines, i, nested)
                    ElseIf sectionName = "SolutionProperties" OrElse sectionName = "ExtensibilityGlobals" Then
                        Dim props As New Dictionary(Of String, String)
                        i = ParseKeyValueSection(lines, i, props)

                        For Each kv In props
                            solution.Global.Properties(kv.Key) = kv.Value

                            If String.Equals(kv.Key, "SolutionGuid", StringComparison.OrdinalIgnoreCase) Then
                                solution.Global.SolutionGuid = kv.Value
                            End If
                        Next
                    Else
                        ' Unknown / unhandled global section: skip to its EndGlobalSection.
                        i = SkipSection(lines, i)
                    End If
                End If

                i += 1
            Loop

            ' Apply nested project (parent) relations.
            For Each p In solution.Projects
                If nested.ContainsKey(p.Guid) Then
                    p.ParentGuid = nested(p.Guid)
                End If

                p.FullPath = solution.GetProjectFullPath(p)
            Next

            Return solution
        End Function

        Private Function ParseProjectDeclaration(line As String) As Project
            ' Project("{typeGuid}") = "Name", "RelativePath", "{projectGuid}"
            Dim p As New Project()
            Dim inner As String = line.Substring(line.IndexOf("("c) + 1)
            Dim typeGuid As String = inner.Substring(0, inner.IndexOf(")"c)).Trim()
            p.TypeGuid = NormalizeGuid(typeGuid)
            p.NodeType = ResolveType(typeGuid)

            ' content after the first '='
            Dim eqIndex As Integer = line.IndexOf("="c)
            Dim rest As String = line.Substring(eqIndex + 1).Trim()

            ' split by comma, respecting quotes
            Dim parts As String() = SplitQuoted(rest)
            p.Name = StripQuotes(parts(0))

            If parts.Length > 1 Then
                p.RelativePath = StripQuotes(parts(1))
                p.TreePath = p.RelativePath
            End If

            If parts.Length > 2 Then
                p.Guid = NormalizeGuid(StripQuotes(parts(2)))
            End If

            Return p
        End Function

        ''' <summary>
        ''' Parse a ``GlobalSection(SolutionConfigurationPlatforms)`` block.
        ''' Returns the index of the last consumed line.
        ''' </summary>
        Private Function ParseSolutionConfigurations(lines As String(), startIndex As Integer, solution As Solution) As Integer
            Dim i As Integer = startIndex + 1

            Do While i < lines.Length
                Dim line As String = lines(i).Trim()

                If line = "EndGlobalSection" Then
                    Return i
                End If

                If line <> "" AndAlso Not line.StartsWith("GlobalSection") Then
                    Dim kv = SplitKeyValue(line)

                    If kv.Key <> "" Then
                        solution.Configurations.Add(New SolutionConfiguration(kv.Key))
                        ' store the raw entry too in case of trailing metadata
                        solution.Global.Properties("Configuration_" & kv.Key) = kv.Value
                    End If
                End If

                i += 1
            Loop

            Return i - 1
        End Function

        ''' <summary>
        ''' Parse a generic key = value global section into the supplied dictionary.
        ''' Returns the index of the last consumed line.
        ''' </summary>
        Private Function ParseKeyValueSection(lines As String(), startIndex As Integer, store As Dictionary(Of String, String)) As Integer
            Dim i As Integer = startIndex + 1

            Do While i < lines.Length
                Dim line As String = lines(i).Trim()

                If line = "EndGlobalSection" Then
                    Return i
                End If

                If line <> "" AndAlso Not line.StartsWith("GlobalSection") Then
                    Dim kv = SplitKeyValue(line)
                    store(kv.Key) = kv.Value
                End If

                i += 1
            Loop

            Return i - 1
        End Function

        Private Function SkipSection(lines As String(), startIndex As Integer) As Integer
            Dim i As Integer = startIndex + 1

            Do While i < lines.Length
                If lines(i).Trim() = "EndGlobalSection" Then
                    Return i
                End If

                i += 1
            Loop

            Return i - 1
        End Function

        ' ------------------------------------------------------------------
        ' New .slnx (XML) parsing
        ' ------------------------------------------------------------------

        ''' <summary>
        ''' Parse a Visual Studio ``.slnx`` XML solution file (VS2022 17.10+).
        ''' </summary>
        Public Function ParseSlnx(path As String) As Solution
            If Not File.Exists(path) Then
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

        ' ------------------------------------------------------------------
        ' Shared helpers
        ' ------------------------------------------------------------------

        Private Function SplitKeyValue(line As String) As (Key As String, Value As String)
            Dim eq As Integer = line.IndexOf("="c)

            If eq < 0 Then
                Return (line.Trim(), "")
            End If

            Return (line.Substring(0, eq).Trim(), line.Substring(eq + 1).Trim())
        End Function

        ''' <summary>
        ''' Split a comma separated list, honoring double quotes so that
        ''' paths containing commas are not broken.
        ''' </summary>
        Private Function SplitQuoted(text As String) As String()
            Dim result As New List(Of String)
            Dim current As New StringBuilder()
            Dim inQuotes As Boolean = False
            Dim i As Integer = 0

            Do While i < text.Length
                Dim c As Char = text(i)

                If c = """"c Then
                    inQuotes = Not inQuotes
                ElseIf c = ","c AndAlso Not inQuotes Then
                    result.Add(current.ToString())
                    current.Clear()
                Else
                    current.Append(c)
                End If

                i += 1
            Loop

            result.Add(current.ToString())
            Return result.ToArray()
        End Function

        Private Function StripQuotes(text As String) As String
            If String.IsNullOrEmpty(text) Then
                Return text
            End If

            text = text.Trim()

            If text.Length >= 2 AndAlso text(0) = """"c AndAlso text(text.Length - 1) = """"c Then
                Return text.Substring(1, text.Length - 2)
            End If

            Return text
        End Function

        Private Function ExtractSectionName(line As String) As String
            ' GlobalSection(SolutionConfigurationPlatforms) = preSolution
            Dim open As Integer = line.IndexOf("("c)
            Dim close As Integer = line.IndexOf(")"c)

            If open >= 0 AndAlso close > open Then
                Return line.Substring(open + 1, close - open - 1).Trim()
            End If

            Return line
        End Function

        Private Function NormalizeGuid(guid As String) As String
            If String.IsNullOrEmpty(guid) Then
                Return guid
            End If

            guid = guid.Trim()

            If Not guid.StartsWith("{"c) Then
                guid = "{" & guid
            End If

            If Not guid.EndsWith("}"c) Then
                guid = guid & "}"
            End If

            Return guid.ToUpperInvariant()
        End Function

        Private Function ResolveType(typeGuid As String) As TypeId
            If String.IsNullOrEmpty(typeGuid) Then
                Return TypeId.Unknown
            End If

            Dim guid As String = NormalizeGuid(typeGuid)
            Dim descriptions = CType([Enum].GetValues(GetType(TypeId)), TypeId())

            For Each value As TypeId In descriptions
                Dim attr = CType(Attribute.GetCustomAttribute(GetType(TypeId).GetField(value.ToString()), GetType(DescriptionAttribute)), DescriptionAttribute)

                If attr IsNot Nothing AndAlso String.Equals(attr.Description, guid, StringComparison.OrdinalIgnoreCase) Then
                    Return value
                End If
            Next

            Return TypeId.Unknown
        End Function

        Private Function GetAttr(el As XmlElement, name As String) As String
            If el Is Nothing OrElse el.Attributes(name) Is Nothing Then
                Return String.Empty
            End If

            Return el.Attributes(name).Value
        End Function
    End Module
End Namespace

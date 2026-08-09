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
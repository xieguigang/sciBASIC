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
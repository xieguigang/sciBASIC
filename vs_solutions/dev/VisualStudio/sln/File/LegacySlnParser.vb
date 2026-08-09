Imports System.Text

Namespace sln.File

    Module LegacySlnParser

        ' ------------------------------------------------------------------
        ' Classic .sln (text) parsing
        ' ------------------------------------------------------------------

        ''' <summary>
        ''' Parse a classic Visual Studio ``.sln`` text solution file.
        ''' </summary>
        Public Function ParseSln(path As String) As Solution
            If Not path.FileExists Then
                Return Nothing
            End If

            Dim solution As New Solution With {
                .FilePath = path,
                .IsXmlFormat = False
            }

            Dim lines As String() = path.ReadAllLines(Encoding.UTF8)
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
    End Module
End Namespace
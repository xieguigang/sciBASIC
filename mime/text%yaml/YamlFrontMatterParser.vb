Imports System.Text.RegularExpressions

''' <summary>
''' A lightweight YAML front-matter parser specialized for SKILL.md files.
''' 
''' This parser is intentionally minimal: it only understands the subset
''' of YAML used in skill metadata blocks (simple key-value pairs and
''' inline/bulleted lists). It avoids pulling in a full YAML dependency
''' so the module remains self-contained and easy to deploy.
''' 
''' A valid front-matter block is delimited by "---" lines at the very
''' top of the markdown file:
''' 
''' ---
''' name: pdf_extract_text
''' description: Extract text content from PDF documents
''' version: 1.0.0
''' tags:
'''   - pdf
'''   - text-extraction
''' ---
''' 
''' </summary>
Public Module YamlFrontMatterParser

    Private ReadOnly FrontMatterRegex As New Regex(
        "^\s*---\s*\r?\n(.*?)\r?\n---\s*(\r?\n|$)",
        RegexOptions.Singleline Or RegexOptions.Compiled)

    ''' <summary>
    ''' Parse the YAML front-matter block from the raw markdown content
    ''' of a SKILL.md file and return a dictionary of key/value pairs.
    ''' List values are returned as comma-joined strings for simplicity.
    ''' </summary>
    ''' <param name="markdownContent">The full raw text of SKILL.md.</param>
    ''' <returns>A dictionary of metadata keys to string values. Returns an empty dictionary if no front-matter block is found.</returns>
    Public Function Parse(markdownContent As String) As Dictionary(Of String, String)
        Dim result As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

        If String.IsNullOrWhiteSpace(markdownContent) Then
            Return result
        End If

        Dim match As Match = FrontMatterRegex.Match(markdownContent)
        If Not match.Success Then
            Return result
        End If

        Dim yamlBlock As String = match.Groups(1).Value
        Dim lines() As String = yamlBlock.Split(New String() {vbCrLf, vbLf}, StringSplitOptions.None)

        Dim currentKey As String = Nothing
        Dim listBuffer As New List(Of String)

        For Each rawLine As String In lines
            Dim line As String = rawLine.TrimEnd()

            ' Skip blank lines and comments
            If String.IsNullOrWhiteSpace(line) OrElse line.TrimStart().StartsWith("#") Then
                Continue For
            End If

            ' Bulleted list item under the current key
            ' Note: list items are typically indented (e.g. "  - pdf"),
            ' so we must TrimStart before checking the prefix.
            Dim trimmedLine As String = line.TrimStart()
            If trimmedLine.StartsWith("- ") OrElse trimmedLine.StartsWith("-" & vbTab) Then
                Dim itemValue As String = trimmedLine.Substring(1).Trim().Trim(""""c).Trim("'"c)
                listBuffer.Add(itemValue)
                Continue For
            End If

            ' Flush any pending list into the previous key before starting a new key
            If currentKey IsNot Nothing AndAlso listBuffer.Count > 0 Then
                result(currentKey) = String.Join(", ", listBuffer)
                listBuffer.Clear()
            End If

            ' Key: value pair
            Dim colonIdx As Integer = line.IndexOf(":"c)
            If colonIdx > 0 Then
                Dim key As String = line.Substring(0, colonIdx).Trim()
                Dim valuePart As String = line.Substring(colonIdx + 1).Trim()

                currentKey = key

                ' Inline list like: tags: [a, b, c]
                If valuePart.StartsWith("[") AndAlso valuePart.EndsWith("]") Then
                    Dim inner As String = valuePart.Substring(1, valuePart.Length - 2)
                    Dim items() As String = inner.Split(New String() {","}, StringSplitOptions.RemoveEmptyEntries)
                    Dim cleaned = items.Select(Function(s) s.Trim().Trim(""""c).Trim("'"c)).ToList()
                    result(key) = String.Join(", ", cleaned)
                    currentKey = Nothing
                ElseIf valuePart.Length > 0 Then
                    ' Scalar value, strip surrounding quotes
                    result(key) = valuePart.Trim(""""c).Trim("'"c)
                Else
                    ' Value is empty - might be a multi-line list following below
                    ' Leave currentKey set so bulleted items can attach
                End If
            End If
        Next

        ' Flush trailing list
        If currentKey IsNot Nothing AndAlso listBuffer.Count > 0 Then
            result(currentKey) = String.Join(", ", listBuffer)
        End If

        Return result
    End Function

    ''' <summary>
    ''' Strip the YAML front-matter block from markdown content and return
    ''' only the body (the actual markdown instructions). This is used by
    ''' Layer 2 to obtain the full instruction text without re-parsing
    ''' the metadata block.
    ''' </summary>
    Public Function StripFrontMatter(markdownContent As String) As String
        If String.IsNullOrWhiteSpace(markdownContent) Then
            Return ""
        End If

        Dim match As Match = FrontMatterRegex.Match(markdownContent)
        If Not match.Success Then
            Return markdownContent
        End If

        Return markdownContent.Substring(match.Length).TrimStart()
    End Function

End Module

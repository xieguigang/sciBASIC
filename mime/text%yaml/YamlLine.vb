''' <summary>
''' Represents a single pre-processed line of YAML text with its indentation level.
''' </summary>
Friend Class YamlLine

    ''' <summary>Indentation level (number of leading spaces).</summary>
    Public Property Indent As Integer
    ''' <summary>Content of the line (with leading whitespace stripped).</summary>
    Public Property Content As String
    ''' <summary>Original line number in the source text (for error reporting).</summary>
    Public Property LineNumber As Integer

    Public ReadOnly Property IsListItem As Boolean
        Get
            If Content Is Nothing Then
                Return False
            Else
                Return Content.TrimStart.StartsWith("-")
            End If
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"[{LineNumber}] {Content}"
    End Function
End Class
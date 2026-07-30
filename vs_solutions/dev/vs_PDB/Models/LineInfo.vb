Namespace Models

    ''' <summary>
    ''' A line-number / sequence-point mapping between a method and a source document.
    ''' </summary>
    Public Class LineInfo

        ''' <summary>
        ''' The source document this line range belongs to.
        ''' </summary>
        Public Property Document As SourceDocument

        ''' <summary>
        ''' IL / native offset at which this line range begins (best-effort, 0 when not available).
        ''' </summary>
        Public Property Offset As Long

        ''' <summary>
        ''' Method or function name this line range is part of (best-effort; may be empty for
        ''' classic PDBs that do not carry method names in the line stream).
        ''' </summary>
        Public Property MethodName As String

        ''' <summary>
        ''' 1-based start line in the source document.
        ''' </summary>
        Public Property StartLine As Integer

        ''' <summary>
        ''' 1-based end line in the source document.
        ''' </summary>
        Public Property EndLine As Integer

        ''' <summary>
        ''' Start column (0-based within the line), or 0 when not available.
        ''' </summary>
        Public Property StartColumn As Integer

        ''' <summary>
        ''' End column (0-based within the line), or 0 when not available.
        ''' </summary>
        Public Property EndColumn As Integer

        Public Overrides Function ToString() As String
            Return $"{If(Document?.FilePath, "?")}({StartLine},{StartColumn})-({EndLine},{EndColumn}) {If(MethodName, "")}"
        End Function
    End Class
End Namespace
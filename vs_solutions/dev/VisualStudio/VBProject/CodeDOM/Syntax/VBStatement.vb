Namespace VBProj.CodeDOM.Syntax

    ''' <summary>
    ''' a logical (already line-continued) source line together with the
    ''' xml documentation comment that immediately precedes it.
    ''' </summary>
    Public Class VBStatement

        ''' <summary>
        ''' the physical line (1-based, as shown in an editor) where the
        ''' declaration keyword of this statement starts.
        ''' </summary>
        Public Line As Integer

        ''' <summary>
        ''' the last physical line (1-based) covered by this logical statement
        ''' after line continuation (trailing underscore) merging. Equal to
        ''' <see cref="Line"/> when the statement is a single physical line.
        ''' </summary>
        Public EndLine As Integer

        ''' <summary>
        ''' the earliest physical line (1-based) of the xml documentation
        ''' comment (''') / standalone attribute block (&lt;...&gt;) that
        ''' immediately precedes this statement. Falls back to <see cref="Line"/>
        ''' when there is no leading comment or attribute.
        ''' </summary>
        Public LeadingLine As Integer

        Public Tokens As List(Of Token)
        Public XmlDoc As String
        Public Attributes As New List(Of String)

    End Class

End Namespace
Namespace VBProj.Syntax

    ''' <summary>
    ''' a logical (already line-continued) source line together with the
    ''' xml documentation comment that immediately precedes it.
    ''' </summary>
    Public Class VBStatement

        Public Line As Integer
        Public Tokens As List(Of Token)
        Public XmlDoc As String
        Public Attributes As New List(Of String)

    End Class

End Namespace
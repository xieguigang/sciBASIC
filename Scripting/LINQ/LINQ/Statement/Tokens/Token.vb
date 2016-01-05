Namespace Statements.Tokens

    Public MustInherit Class Token
        Friend Statement As LINQ.Statements.LINQStatement
        Friend _OriginalCommand As String

        Public Overrides Function ToString() As String
            Return _OriginalCommand
        End Function
    End Class
End Namespace
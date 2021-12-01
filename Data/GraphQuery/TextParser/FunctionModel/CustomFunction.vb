Imports Microsoft.VisualBasic.MIME.Html.Document

Namespace TextParser

    Public Class CustomFunction : Inherits ParserFunction

        ReadOnly parse As Func(Of InnerPlantText, InnerPlantText)

        Sub New(parse As Func(Of InnerPlantText, InnerPlantText))
            Me.parse = parse
        End Sub

        Public Overrides Function GetToken(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
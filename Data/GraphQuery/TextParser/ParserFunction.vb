
Imports Microsoft.VisualBasic.MIME.Markup.HTML

Namespace TextParser

    Public MustInherit Class ParserFunction

        Public MustOverride Function GetToken(document As InnerPlantText) As InnerPlantText

    End Class

    Public Class InternalInvoke : Inherits ParserFunction

        Public Property name As String

        Public Overrides Function GetToken(document As InnerPlantText) As InnerPlantText
            Throw New NotImplementedException()
        End Function
    End Class

    Public Class CustomFunction : Inherits ParserFunction

        Dim parse As Func(Of InnerPlantText, InnerPlantText)

        Sub New(parse As Func(Of InnerPlantText, InnerPlantText))
            Me.parse = parse
        End Sub

        Public Overrides Function GetToken(document As InnerPlantText) As InnerPlantText
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
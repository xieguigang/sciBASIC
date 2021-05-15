Imports Microsoft.VisualBasic.MIME.Markup.HTML

Public Class Parser

    Public Property func As String
    Public Property parameters As String()
    Public Property pipeNext As Parser

    Public Overrides Function ToString() As String
        Return $"{func}({parameters.JoinBy(", ")})"
    End Function

End Class

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
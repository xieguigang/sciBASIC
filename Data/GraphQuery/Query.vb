Imports Microsoft.VisualBasic.MIME.Markup.HTML

''' <summary>
''' the object model of a query
''' </summary>
Public Class Query

    Public Property members As Dictionary(Of String, Query)
    Public Property parser As Parse

End Class

Public Class Parse

    Public Property func As ParserFunction

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
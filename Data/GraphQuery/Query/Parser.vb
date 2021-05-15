Imports Microsoft.VisualBasic.MIME.Markup.HTML

Public Class Parser

    Public Property func As String
    Public Property parameters As String()
    Public Property pipeNext As Parser

    Public Function Parse(document As InnerPlantText, env As Engine) As InnerPlantText
        Throw New NotImplementedException
    End Function

    Public Overrides Function ToString() As String
        Dim thisText As String = $"{func}({parameters.JoinBy(", ")})"

        If Not pipeNext Is Nothing Then
            Return $"{thisText} -> {pipeNext}"
        Else
            Return thisText
        End If
    End Function

End Class

Imports Microsoft.VisualBasic.MIME.application.xml
Imports Microsoft.VisualBasic.MIME.Markup.HTML

Public MustInherit Class Parser

    Public Property func As String
    Public Property parameters As String()
    Public Property pipeNext As Parser

    Sub New()
    End Sub

    Sub New(func As String, parameters As String())
        Me.func = func
        Me.parameters = parameters
    End Sub

    Public Function Parse(document As InnerPlantText, isArray As Boolean, env As Engine) As InnerPlantText
        Dim value As InnerPlantText = ParseImpl(document, isArray, env)

        If Not pipeNext Is Nothing Then
            value = pipeNext.Parse(value, isArray, env)
        End If

        Return value
    End Function

    Protected MustOverride Function ParseImpl(document As InnerPlantText, isArray As Boolean, env As Engine) As InnerPlantText

    Public Overrides Function ToString() As String
        Dim thisText As String = $"{func}({parameters.JoinBy(", ")})"

        If Not pipeNext Is Nothing Then
            Return $"{thisText} -> {pipeNext}"
        Else
            Return thisText
        End If
    End Function

    Public Shared Operator &(left As Parser, [next] As Parser) As Parser
        If left.pipeNext Is Nothing Then
            left.pipeNext = [next]
        Else
#Disable Warning BC42004 ' Expression recursively calls the containing Operator
            left.pipeNext = left.pipeNext & [next]
#Enable Warning BC42004 ' Expression recursively calls the containing Operator
        End If

        Return left
    End Operator

End Class

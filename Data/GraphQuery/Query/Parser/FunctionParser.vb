Imports Microsoft.VisualBasic.MIME.Html.Document

Public Class FunctionParser : Inherits Parser

    Sub New(func As String, parameters As String())
        Call MyBase.New(func, parameters)
    End Sub

    Protected Overrides Function ParseImpl(document As InnerPlantText, isArray As Boolean, env As Engine) As InnerPlantText
        Return env.Execute(document, func, parameters, isArray)
    End Function
End Class

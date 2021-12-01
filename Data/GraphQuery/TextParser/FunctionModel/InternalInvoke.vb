Imports System.Reflection
Imports Microsoft.VisualBasic.MIME.Html.Document

Namespace TextParser

    Public Class InternalInvoke : Inherits ParserFunction

        Public Property name As String
        Public Property method As MethodInfo

        Public Overrides Function GetToken(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText
            Return method.Invoke(Nothing, {document, parameters, isArray})
        End Function

        Public Overrides Function ToString() As String
            Return name
        End Function
    End Class
End Namespace
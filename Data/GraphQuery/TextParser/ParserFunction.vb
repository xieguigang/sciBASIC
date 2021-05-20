
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.Html.Document

Namespace TextParser

    Public Delegate Function IParserPipeline(document As InnerPlantText) As InnerPlantText

    Public MustInherit Class ParserFunction

        Public MustOverride Function GetToken(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText

        Public Shared Function ParseDocument(document As InnerPlantText,
                                             pip As IParserPipeline,
                                             isArray As Boolean,
                                             <CallerMemberName>
                                             Optional callFunc As String = Nothing) As InnerPlantText
            If Not isArray Then
                Return pip(document)
            End If

            Dim array As New HtmlElement With {.TagName = callFunc}

            If TypeOf document Is HtmlElement Then
                For Each element In DirectCast(document, HtmlElement).HtmlElements
                    array.Add(pip(element))
                Next
            Else
                array.Add(pip(document))
            End If

            Return array
        End Function

    End Class

    Public Class InternalInvoke : Inherits ParserFunction

        Public Property name As String
        Public Property method As MethodInfo

        Public Overrides Function GetToken(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText
            Return method.Invoke(Nothing, {document, parameters, isArray})
        End Function
    End Class

    Public Class CustomFunction : Inherits ParserFunction

        Dim parse As Func(Of InnerPlantText, InnerPlantText)

        Sub New(parse As Func(Of InnerPlantText, InnerPlantText))
            Me.parse = parse
        End Sub

        Public Overrides Function GetToken(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
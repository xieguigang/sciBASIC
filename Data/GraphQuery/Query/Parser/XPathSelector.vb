Imports Microsoft.VisualBasic.MIME.application.xml
Imports Microsoft.VisualBasic.MIME.Markup.HTML

Public Class XPathSelector : Inherits Parser

    Sub New(func As String, parameters As String())
        Call MyBase.New(func, parameters)
    End Sub

    Protected Overrides Function ParseImpl(document As InnerPlantText, isArray As Boolean, env As Engine) As InnerPlantText
        Dim xpath As XPath = XPathParser.Parse(parameters(Scan0))
        Dim engine As New XPathQuery(xpath)
        Dim query As InnerPlantText

        If isArray Then
            query = New HtmlElement With {
                .TagName = parameters(Scan0),
                .HtmlElements = engine _
                    .QueryAll(document) _
                    .Select(Function(n)
                                Return DirectCast(DirectCast(n, HtmlElement), InnerPlantText)
                            End Function) _
                    .ToArray
            }
        Else
            query = engine.QuerySingle(document)
        End If

        Return query
    End Function
End Class

Imports Microsoft.VisualBasic.MIME.Markup.HTML

Public Class CSSSelector : Inherits Parser

    Sub New(func As String, parameters As String())
        Call MyBase.New(func, parameters)
    End Sub

    Protected Overrides Function ParseImpl(document As InnerPlantText, isArray As Boolean, env As Engine) As InnerPlantText
        Dim query As String = parameters(Scan0)
        Dim n As String = parameters.ElementAtOrDefault(1)

        If query.First = "#"c Then
            ' get element by id
            Return DirectCast(document, HtmlElement).getElementById(query.Substring(1))
        ElseIf query.First = "."c Then
            Dim list As HtmlElement() = DirectCast(document, HtmlElement).getElementsByClassName(query.Substring(1))

            If isArray AndAlso parameters.Length = 1 Then
                ' get elements by class name
                Return New HtmlElement With {
                    .TagName = query,
                    .HtmlElements = list
                }
            Else
                Return list(CInt(Val(n)))
            End If
        Else
            Dim list As HtmlElement() = DirectCast(document, HtmlElement).getElementsByTagName(query)

            If isArray AndAlso parameters.Length = 1 Then
                ' get elements by tag name
                Return New HtmlElement With {
                    .TagName = query,
                    .HtmlElements = list
                }
            Else
                Return list(CInt(Val(n)))
            End If
        End If
    End Function
End Class

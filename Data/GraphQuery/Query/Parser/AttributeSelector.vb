Imports Microsoft.VisualBasic.MIME.Markup.HTML

Public Class AttributeSelector : Inherits Parser

    Sub New(func As String, parameters As String())
        Call MyBase.New(func, parameters)
    End Sub

    Protected Overrides Function ParseImpl(document As InnerPlantText, isArray As Boolean, env As Engine) As InnerPlantText
        If isArray Then
            Return New HtmlElement With {
                .HtmlElements = DirectCast(document, HtmlElement)(parameters(Scan0)).Values _
                    .Select(Function(a)
                                Return New InnerPlantText With {
                                    .InnerText = a
                                }
                            End Function) _
                    .ToArray
            }
        Else
            Return New InnerPlantText With {
                .InnerText = DirectCast(document, HtmlElement)(parameters(Scan0)).Value
            }
        End If
    End Function
End Class

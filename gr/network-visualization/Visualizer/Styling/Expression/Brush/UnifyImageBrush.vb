Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.MIME.Html.Language.CSS

Namespace Styling.FillBrushes

    ''' <summary>
    ''' 全部都使用统一的图案
    ''' </summary>
    Public Class UnifyImageBrush : Implements IGetBrush

        ReadOnly brush As TextureBrush

        Sub New(expression As String)
            Dim image As Image = UrlEvaluator.EvaluateAsImage(expression)
            Dim brush As New TextureBrush(image)

            Me.brush = brush
        End Sub

        Public Iterator Function GetBrush(nodes As IEnumerable(Of Node)) As IEnumerable(Of Map(Of Node, Brush)) Implements IGetBrush.GetBrush
            For Each n As Node In nodes
                Yield New Map(Of Node, Brush) With {
                    .Key = n,
                    .Maps = brush
                }
            Next
        End Function
    End Class
End Namespace
Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging

Namespace Styling.FillBrushes

    ''' <summary>
    ''' 全部都使用统一的颜色进行填充
    ''' </summary>
    Public Class UnifyColorBrush : Implements IGetBrush

        ReadOnly brush As SolidBrush

        Sub New(expression As String)
            brush = New SolidBrush(expression.TranslateColor)
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
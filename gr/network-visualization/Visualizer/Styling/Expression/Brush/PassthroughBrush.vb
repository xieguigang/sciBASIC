Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Styling.FillBrushes

    ''' <summary>
    ''' 只能够映射颜色
    ''' </summary>
    Public Class PassthroughBrush : Implements IGetBrush

        ReadOnly selector As Func(Of Node, Object)

        Sub New(expression As String)
            selector = expression.SelectNodeValue
        End Sub

        Public Iterator Function GetBrush(nodes As IEnumerable(Of Node)) As IEnumerable(Of Map(Of Node, Brush)) Implements IGetBrush.GetBrush
            Dim color As Color
            Dim brush As Brush

            ' 使用单词进行直接映射
            For Each n As Node In nodes
                color = CStrSafe(selector(n)).TranslateColor
                brush = New SolidBrush(color)

                Yield New Map(Of Node, Brush) With {
                    .Key = n,
                    .Maps = brush
                }
            Next
        End Function
    End Class
End Namespace
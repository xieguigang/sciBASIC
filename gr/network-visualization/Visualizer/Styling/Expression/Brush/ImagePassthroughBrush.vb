
Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging

Namespace Styling.FillBrushes

    ''' <summary>
    ''' 属性名作为文件名，从指定的文件夹之中读取图片文件的passthrough映射
    ''' </summary>
    Public Class ImagePassthroughBrush : Implements IGetBrush

        ReadOnly directory$
        ReadOnly extensionName$
        ReadOnly selector As Func(Of Node, Object)

        Sub New(map As MapExpression)
            directory = map.values(Scan0).GetStackValue("(", ")").Trim(" "c, "'"c)
            extensionName = map.values(1).Trim(" "c, "."c, "*"c)
            selector = map.propertyName.SelectNodeValue
        End Sub

        Public Iterator Function GetBrush(nodes As IEnumerable(Of Node)) As IEnumerable(Of Map(Of Node, Brush)) Implements IGetBrush.GetBrush
            Dim filePath As String
            Dim brush As Brush

            For Each n As Node In nodes
                filePath = $"{directory}/{selector(n)}.{extensionName}"
                brush = New TextureBrush(filePath.LoadImage)

                Yield New Map(Of Node, Brush) With {
                    .Key = n,
                    .Maps = brush
                }
            Next
        End Function
    End Class
End Namespace
Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors

Namespace Styling.FillBrushes

    ''' <summary>
    ''' 区间映射，也可能是category映射
    ''' </summary>
    Public Class ColorRangeBrush : Implements IGetBrush

        Dim patternName$
        Dim n%
        Dim colors As Brush()
        Dim range As DoubleRange = {0, n - 1}
        Dim selector As Func(Of Node, Object)
        Dim getValue As Func(Of Node, Double)

        Sub New(map As MapExpression)
            patternName = map.values(0)
            n = map.values(1)
            colors = Designer _
                .GetColors(patternName, n) _
                .Select(Function(c) DirectCast(New SolidBrush(c), Brush)) _
                .ToArray
            selector = map.propertyName.SelectNodeValue
            getValue = Function(node As Node) Val(selector(node))
        End Sub

        Public Iterator Function GetBrush(nodes As IEnumerable(Of Node)) As IEnumerable(Of Map(Of Node, Brush)) Implements IGetBrush.GetBrush
            ' {value, index}
            Dim nodesArray = nodes.ToArray
            Dim nodeValues = nodesArray.RangeTransform(getValue, range)
            Dim index%
            Dim brush As Brush

            For i As Integer = 0 To nodesArray.Length - 1
                index = CInt(nodeValues(i).Maps)
                brush = colors(index)

                Yield New Map(Of Node, Brush) With {
                    .Key = nodesArray(i),
                    .Maps = brush
                }
            Next
        End Function
    End Class
End Namespace
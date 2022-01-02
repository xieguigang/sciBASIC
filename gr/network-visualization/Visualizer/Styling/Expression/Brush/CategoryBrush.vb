Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Styling.FillBrushes

    ''' <summary>
    ''' 差不多相当于离散映射的一种变种
    ''' </summary>
    Public Class CategoryBrush : Implements IGetBrush

        ReadOnly selector As Func(Of Node, Object)
        ReadOnly schema$

        ''' <summary>
        ''' map(propertyName, [patternName, category])
        ''' </summary>
        ''' <param name="map"></param>
        Sub New(map As MapExpression)
            schema = map.values(0)
            selector = map.propertyName.SelectNodeValue
        End Sub

        Public Iterator Function GetBrush(nodes As IEnumerable(Of Node)) As IEnumerable(Of Map(Of Node, Brush)) Implements IGetBrush.GetBrush
            Dim data As Node() = nodes.ToArray
            Dim key As String
            Dim brush As Brush
            ' 与节点集合之中的元素一一对应的
            Dim nodeTypes$() = data _
                           .Select(Function(o) CStrSafe(selector(o))) _
                           .ToArray
            ' 去重之后用于生成字典的键名的
            Dim types$() = nodeTypes _
                           .Distinct _
                           .ToArray
            Dim colors As Dictionary(Of String, SolidBrush) = Designer _
                           .GetColors(term:=schema, n:=types.Length) _
                           .SeqIterator _
                           .ToDictionary(Function(i) types(i),
                                         Function(color) New SolidBrush(color.value))

            For i As Integer = 0 To data.Length - 1
                key = nodeTypes(i)
                brush = colors.TryGetValue(key, [default]:=Brushes.Black)

                Yield New Map(Of Node, Brush) With {
                    .Key = data(i),
                    .Maps = brush
                }
            Next
        End Function
    End Class
End Namespace
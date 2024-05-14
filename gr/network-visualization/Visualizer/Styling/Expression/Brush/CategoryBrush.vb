#Region "Microsoft.VisualBasic::da791dd64d4ee2458e9f95c4b28e349c, gr\network-visualization\Visualizer\Styling\Expression\Brush\CategoryBrush.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 56
    '    Code Lines: 40
    ' Comment Lines: 9
    '   Blank Lines: 7
    '     File Size: 2.19 KB


    '     Class CategoryBrush
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetBrush
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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

#Region "Microsoft.VisualBasic::a61e94051835200f85416bd25f120cbd, gr\network-visualization\Visualizer\Styling\Expression\ShapeExpression.vb"

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

    '   Total Lines: 80
    '    Code Lines: 62 (77.50%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 18 (22.50%)
    '     File Size: 2.84 KB


    '     Interface IGetShape
    ' 
    '         Function: GetShapes
    ' 
    '     Module ShapeExpression
    ' 
    '         Function: Evaluate, getShapeMapping
    ' 
    '     Class DiscreteShape
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetShapes
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging.d3js.scale
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Styling

    Public Interface IGetShape

        Function GetShapes(nodes As IEnumerable(Of Node)) As IEnumerable(Of Map(Of Node, LegendStyles))

    End Interface

    Module ShapeExpression

        Public Function Evaluate(expression As String) As IGetShape
            If IsMapExpression(expression) Then
                Return getShapeMapping(expression)
            Else
                Throw New NotImplementedException
            End If
        End Function

        Private Function getShapeMapping(expression As String) As IGetShape
            Dim model As MapExpression = expression.MapExpressionParser

            If model.type = MapperTypes.Discrete Then
                Dim data = model.values

                If data.Length = 1 AndAlso Not data(Scan0).Contains("="c) Then
                    Throw New NotImplementedException
                Else
                    Return New DiscreteShape(model)
                End If
            Else
                Throw New NotImplementedException
            End If
        End Function
    End Module

    Public Class DiscreteShape : Implements IGetShape

        ReadOnly shapeList As New Dictionary(Of String, LegendStyles)
        ReadOnly selector As Func(Of Node, Object)

        Sub New(map As MapExpression)
            Dim shape As LegendStyles

            selector = map.propertyName.SelectNodeValue

            For Each p As NamedValue(Of String) In map _
                .values _
                .Select(Function(s)
                            Return s.GetTagValue("=", trim:=True)
                        End Function)

                shape = [Enum].Parse(GetType(LegendStyles), p.Value, ignoreCase:=True)
                shapeList.Add(p.Name, shape)
            Next
        End Sub

        Public Iterator Function GetShapes(nodes As IEnumerable(Of Node)) As IEnumerable(Of Map(Of Node, LegendStyles)) Implements IGetShape.GetShapes
            Dim key As String
            Dim shape As LegendStyles

            For Each node As Node In nodes
                key = CStrSafe(selector(node))
                shape = shapeList.TryGetValue(key, default:=LegendStyles.Circle)

                Yield New Map(Of Node, LegendStyles) With {
                    .Key = node,
                    .Maps = shape
                }
            Next
        End Function
    End Class
End Namespace

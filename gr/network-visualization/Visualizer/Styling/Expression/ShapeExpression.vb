Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
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
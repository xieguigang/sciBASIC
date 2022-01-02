
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Namespace Styling.Numeric

    Public Class ContinuousNumber : Implements IGetSize

        ReadOnly getValue As Func(Of Node, Double)
        ReadOnly range As DoubleRange

        Sub New(map As MapExpression)
            Dim selector = map.propertyName.SelectNodeValue
            Dim values As Double() = map.values _
                .Select(AddressOf Val) _
                .ToArray

            range = New DoubleRange(values(0), values(1))
            getValue = Function(node As Node) Val(selector(node))
        End Sub

        Public Function GetSize(nodes As IEnumerable(Of Node)) As IEnumerable(Of Map(Of Node, Single)) Implements IGetSize.GetSize
            Return nodes _
                .RangeTransform(getValue, range) _
                .Select(Function(map)
                            Return New Map(Of Node, Single)(map.Key, map.Maps)
                        End Function)
        End Function
    End Class
End Namespace
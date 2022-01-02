Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Namespace Styling.Numeric

    Public Class DiscreteNumber : Implements IGetSize

        ReadOnly selector As Func(Of Node, Object)
        ReadOnly sizeList As Dictionary(Of String, Single)

        Sub New(map As MapExpression)
            sizeList = map.values _
                .Select(Function(s) s.GetTagValue("=", trim:=True)) _
                .ToDictionary(Function(p) p.Name,
                                Function(p)
                                    Return CSng(Val(p.Value))
                                End Function)
            selector = map.propertyName.SelectNodeValue
        End Sub

        Public Iterator Function GetSize(nodes As IEnumerable(Of Node)) As IEnumerable(Of Map(Of Node, Single)) Implements IGetSize.GetSize
            Dim key$
            Dim value#

            For Each n As Node In nodes
                key = selector(n)
                value = sizeList.TryGetValue(key)

                Yield New Map(Of Node, Single) With {
                    .Key = n,
                    .Maps = value
                }
            Next
        End Function
    End Class
End Namespace
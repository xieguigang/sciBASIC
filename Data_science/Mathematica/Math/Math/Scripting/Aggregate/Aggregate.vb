Namespace Scripting

    Public Delegate Function IAggregate(data As IEnumerable(Of Double)) As Double

    Public Class Aggregate

        Public Shared Function GetAggregater(name As String) As IAggregate
            Select Case LCase(name).Trim
                Case "min" : Return AddressOf Enumerable.Min
                Case "max" : Return AddressOf Enumerable.Max
                Case "mean" : Return AddressOf Enumerable.Average
                Case "sum" : Return AddressOf Enumerable.Sum
                Case Else
                    Throw New InvalidCastException(name)
            End Select
        End Function
    End Class
End Namespace
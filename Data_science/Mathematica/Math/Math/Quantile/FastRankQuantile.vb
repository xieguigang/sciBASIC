Namespace Quantile

    Public Class FastRankQuantile : Implements QuantileQuery

        ReadOnly data As Double()
        ReadOnly max As Double
        ReadOnly min As Double

        Sub New(data As IEnumerable(Of Double))
            Me.data = data.OrderBy(Function(a) a).ToArray
            Me.max = data.Last
            Me.min = data.First
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="q">[0, 1]</param>
        ''' <returns></returns>
        Public Function Query(q As Double) As Double Implements QuantileQuery.Query
            Dim i As Integer = CInt(data.Length * q)

            If i >= data.Length Then
                Return max
            Else
                Return data(i)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return Me.debugView
        End Function
    End Class
End Namespace
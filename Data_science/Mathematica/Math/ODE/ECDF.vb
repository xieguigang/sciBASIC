Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports stdNum = System.Math

Public Class ECDF

    ReadOnly range As (range As DoubleRange, y As DoubleRange, sign As Integer)()

    Sub New(v As IEnumerable(Of Double), range As Integer())
        Me.range = indexing(v.ToArray, range.Select(Function(i) CDbl(i)).ToArray).ToArray
    End Sub

    Sub New(y As Double(), x As Double())
        Me.range = indexing(y, x).ToArray
    End Sub

    Private Shared Iterator Function indexing(y As Double(), x As Double()) As IEnumerable(Of (range As DoubleRange, y As DoubleRange, Integer))
        For i As Integer = 1 To x.Length - 1
            Dim xmin As Double = x(i - 1)
            Dim xmax As Double = x(i)
            Dim ymin As Double = y(i - 1)
            Dim ymax As Double = y(i)
            Dim sign As Integer = stdNum.Sign(ymax - ymin)

            Yield (New DoubleRange(xmin, xmax), New DoubleRange(ymin, ymax), sign)
        Next
    End Function

    Public Function eval(i As Double) As Double
        Dim n = range.Where(Function(r) r.range.IsInside(i)).First
        Dim p As Double = n.range.ScaleMapping(i, n.y)

        If n.sign < 0 Then
            p = n.y.Max - p
        End If

        Return p
    End Function
End Class

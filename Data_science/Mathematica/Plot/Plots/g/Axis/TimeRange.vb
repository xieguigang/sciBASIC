Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.ValueTypes

Namespace Graphic.Axis

    Public Class TimeRange

        Public ReadOnly Property [From] As Date
        Public ReadOnly Property [To] As Date
        Public ReadOnly Property Ticks As Date()

        ReadOnly timeRange As DoubleRange

        Sub New(from As Date, [to] As Date)
            Dim ticks#() = New Double() {
                from.UnixTimeStamp, [to].UnixTimeStamp
            }.CreateAxisTicks

            Me.From = CLng(ticks(0)).FromUnixTimeStamp
            Me.To = CLng(ticks.Last).FromUnixTimeStamp
            Me.Ticks = ticks _
                .Select(Function(d) CLng(d).FromUnixTimeStamp) _
                .ToArray
            Me.timeRange = ticks
        End Sub

        Public Function Scaler(range As DoubleRange) As Func(Of Date, Double)
            Return Function(d) As Double
                       Return timeRange.ScaleMapping(d.UnixTimeStamp, range)
                   End Function
        End Function

        Public Overrides Function ToString() As String
            Return $"[{From}, {[To]}]"
        End Function

        Public Overloads Shared Widening Operator CType(dateList As Date()) As TimeRange
            Dim order = dateList _
                .OrderBy(Self(Of Date)) _
                .ToArray
            Return New TimeRange(order(0), order.Last)
        End Operator
    End Class
End Namespace
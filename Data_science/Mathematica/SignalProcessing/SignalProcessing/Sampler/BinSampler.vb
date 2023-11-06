Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Math.Distributions.BinBox

''' <summary>
''' signal data aggregate helper
''' </summary>
Public Class BinSampler

    Dim signal As Signal

    Public ReadOnly Property Range As DoubleRange
        Get
            Return New DoubleRange(signal.times)
        End Get
    End Property

    Public Function AggregateSignal(dt As Double) As Signal
        Return New Signal(GetTicks(dt))
    End Function

    Private Iterator Function GetTicks(dt As Double) As IEnumerable(Of TimeSignal)
        For Each box In CutBins.FixedWidthBins(signal, width:=dt, range:=Range, eval:=Function(ti) ti.time)
            If box.Count = 0 Then
                Continue For
            End If

            Dim time As Double = Aggregate ti As TimeSignal In box.Raw Into Average(ti.time)
            Dim into As Double = Aggregate ti As TimeSignal In box.Raw Into Sum(ti.intensity)

            Yield New TimeSignal(time, into)
        Next
    End Function

End Class

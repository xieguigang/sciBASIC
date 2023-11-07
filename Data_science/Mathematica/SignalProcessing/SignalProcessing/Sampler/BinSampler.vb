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

    Sub New(signal As Signal)
        Me.signal = signal
    End Sub

    Sub New(time As Double(), intensity As Double())
        Me.signal = New Signal(time.Select(Function(ti, i) New TimeSignal(ti, intensity(i))))
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="dt"></param>
    ''' <param name="aggregate">
    ''' default is sum intensity data
    ''' </param>
    ''' <returns></returns>
    Public Function AggregateSignal(dt As Double, Optional aggregate As Func(Of IEnumerable(Of Double), Double) = Nothing) As Signal
        Static sum As Func(Of IEnumerable(Of Double), Double) = AddressOf Enumerable.Sum
        Return New Signal(GetTicks(dt, If(aggregate, sum)))
    End Function

    Private Iterator Function GetTicks(dt As Double, aggregate As Func(Of IEnumerable(Of Double), Double)) As IEnumerable(Of TimeSignal)
        For Each box In CutBins.FixedWidthBins(signal, width:=dt, range:=Range, eval:=Function(ti) ti.time)
            If box.Count = 0 Then
                Continue For
            End If

            Dim time As Double = Aggregate ti As TimeSignal In box.Raw Into Average(ti.time)
            Dim into As Double = aggregate(From ti As TimeSignal
                                           In box.Raw
                                           Select ti.intensity)

            Yield New TimeSignal(time, into)
        Next
    End Function

End Class

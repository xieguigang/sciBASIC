#Region "Microsoft.VisualBasic::29521ef3388eb234bac5313fe88b3b7f, Data_science\Mathematica\SignalProcessing\SignalProcessing\Sampler\BinSampler.vb"

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

    '   Total Lines: 53
    '    Code Lines: 32
    ' Comment Lines: 11
    '   Blank Lines: 10
    '     File Size: 1.84 KB


    ' Class BinSampler
    ' 
    '     Properties: Range
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: AggregateSignal, GetTicks
    ' 
    ' /********************************************************************************/

#End Region

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

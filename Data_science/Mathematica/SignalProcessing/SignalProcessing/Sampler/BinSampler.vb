#Region "Microsoft.VisualBasic::eb505180599911b3d55870013318d47f, Data_science\Mathematica\SignalProcessing\SignalProcessing\Sampler\BinSampler.vb"

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

    '   Total Lines: 66
    '    Code Lines: 38 (57.58%)
    ' Comment Lines: 17 (25.76%)
    '    - Xml Docs: 94.12%
    ' 
    '   Blank Lines: 11 (16.67%)
    '     File Size: 2.74 KB


    ' Class BinSampler
    ' 
    '     Properties: Range
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: (+3 Overloads) AggregateSignal, GetTicks
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Math.Distributions.BinBox

''' <summary>
''' signal data aggregate helper
''' </summary>
Public Class BinSampler

    Dim signal As Signal

    ''' <summary>
    ''' the signal x axis range
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Range As DoubleRange

    Sub New(signal As Signal)
        Me.signal = signal
        Me.Range = New DoubleRange(signal.times)
    End Sub

    Sub New(time As Double(), intensity As Double())
        Call Me.New(New Signal(time.Select(Function(ti, i) New TimeSignal(ti, intensity(i)))))
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="dt">
    ''' the signal x axis resolution
    ''' </param>
    ''' <param name="aggregate">
    ''' default is sum intensity data
    ''' </param>
    ''' <returns></returns>
    Public Function AggregateSignal(dt As Double, Optional aggregate As Func(Of IEnumerable(Of Double), Double) = Nothing) As Signal
        Static sum As Func(Of IEnumerable(Of Double), Double) = AddressOf Enumerable.Sum
        Return New Signal(GetTicks(dt, Function(t, i) New TimeSignal(t, i), If(aggregate, sum)))
    End Function

    Public Function AggregateSignal(n As Integer, Optional aggregate As Func(Of IEnumerable(Of Double), Double) = Nothing) As Signal
        Return AggregateSignal(Range.Length / n, aggregate)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function AggregateSignal(Of T As ITimeSignal)(n As Integer, [new] As Func(Of Double, Double, T), Optional aggregate As Func(Of IEnumerable(Of Double), Double) = Nothing) As IEnumerable(Of T)
        Return GetTicks(Range.Length / n, [new], If(aggregate, New Func(Of IEnumerable(Of Double), Double)(AddressOf Enumerable.Sum)))
    End Function

    Private Iterator Function GetTicks(Of T As ITimeSignal)(dt As Double, [new] As Func(Of Double, Double, T), aggregate As Func(Of IEnumerable(Of Double), Double)) As IEnumerable(Of T)
        For Each box In CutBins.FixedWidthBins(signal, width:=dt, range:=Range, eval:=Function(ti) ti.time)
            If box.Count = 0 Then
                Continue For
            End If

            Dim time As Double = Aggregate ti As TimeSignal In box.Raw Into Average(ti.time)
            Dim into As Double = aggregate(From ti As TimeSignal
                                           In box.Raw
                                           Select ti.intensity)
            Yield [new](time, into)
        Next
    End Function

End Class

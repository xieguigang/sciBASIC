#Region "Microsoft.VisualBasic::17b17800ff8690e16ea1f96e849489c2, Data_science\Mathematica\SignalProcessing\SignalProcessing\TimeSignals.vb"

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

    '   Total Lines: 58
    '    Code Lines: 42 (72.41%)
    ' Comment Lines: 7 (12.07%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (15.52%)
    '     File Size: 1.64 KB


    ' Structure TimeSignal
    ' 
    '     Properties: m_intensity, m_time
    ' 
    '     Constructor: (+3 Overloads) Sub New
    '     Function: SignalSequence, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' a single scatter point in a time signal
''' </summary>
Public Structure TimeSignal : Implements ITimeSignal

    Dim time As Double
    Dim intensity As Double

    Private ReadOnly Property m_time As Double Implements ITimeSignal.time
        Get
            Return time
        End Get
    End Property

    Private ReadOnly Property m_intensity As Double Implements ITimeSignal.intensity
        Get
            Return intensity
        End Get
    End Property

    Sub New(t As Double, intensity As Double)
        Me.time = t
        Me.intensity = intensity
    End Sub

    Sub New(point As PointF)
        Me.time = point.X
        Me.intensity = point.Y
    End Sub

    ''' <summary>
    ''' make signal tick data copy
    ''' </summary>
    ''' <param name="tick"></param>
    Sub New(tick As ITimeSignal)
        Me.time = tick.time
        Me.intensity = tick.intensity
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function ToString() As String
        Return $"[{time}, {intensity}]"
    End Function

    Public Shared Iterator Function SignalSequence(data As IEnumerable(Of Double)) As IEnumerable(Of TimeSignal)
        For Each p As SeqValue(Of Double) In data.SeqIterator(offset:=1)
            Yield New TimeSignal With {
                .time = p.i,
                .intensity = p.value
            }
        Next
    End Function
End Structure

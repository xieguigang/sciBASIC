#Region "Microsoft.VisualBasic::d4b4948a3bb654fabcfbed9e27cf829d, sciBASIC#\Data_science\Mathematica\SignalProcessing\SignalProcessing\TimeSignals.vb"

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
'    Code Lines: 41
' Comment Lines: 0
'   Blank Lines: 12
'     File Size: 1.52 KB


' Structure TimeSignal
' 
'     Properties: m_intensity, m_time
' 
'     Function: SignalSequence, ToString
' 
' Interface ITimeSignal
' 
'     Properties: intensity, time
' 
' Class Signal
' 
'     Constructor: (+1 Overloads) Sub New
'     Operators: +
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Language.Vectorization
Imports Microsoft.VisualBasic.Linq

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

Public Class Signal : Inherits Vector(Of TimeSignal)

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Friend Sub New(data As IEnumerable(Of TimeSignal))
        Call MyBase.New(data)
    End Sub

    Public Shared Operator +(a As Signal, b As Signal) As Signal
        Throw New NotImplementedException
    End Operator
End Class

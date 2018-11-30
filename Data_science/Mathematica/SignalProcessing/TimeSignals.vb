#Region "Microsoft.VisualBasic::55fea020473ebb7b64dca7cec8deb246, Data_science\Mathematica\SignalProcessing\TimeSignals.vb"

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

' Class TimeSignal
' 
'     Properties: intensity, time
' 
'     Function: SignalSequence, ToString
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Vectorization
Imports Microsoft.VisualBasic.Linq

Public Class TimeSignal

    Public Property time As Double
    Public Property intensity As Double

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
End Class

Public Class Signal : Inherits Vector(Of TimeSignal)

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Friend Sub New(data As IEnumerable(Of TimeSignal))
        Call MyBase.New(data)
    End Sub

    Public Shared Operator +(a As Signal, b As Signal) As Signal

    End Operator
End Class
#Region "Microsoft.VisualBasic::afc9d1ca624155a933b48efa62b7c308, Data_science\Mathematica\Math\Math\DownSampling\WeightedEvent.vb"

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

    '   Total Lines: 76
    '    Code Lines: 63 (82.89%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 13 (17.11%)
    '     File Size: 2.52 KB


    '     Class WeightedEvent
    ' 
    '         Properties: [Event], Time, Value, Weight
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Equals, GetHashCode, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace DownSampling

    Public Class WeightedEvent : Implements ITimeSignal

        Public Sub New(time As Long, value As Double)
            Me.Event = New PlainEvent(time, value)
        End Sub

        Public Sub New(e As ITimeSignal)
            Me.Event = e
        End Sub

        Public Overridable ReadOnly Property [Event] As ITimeSignal

        Public Overridable ReadOnly Property Time As Double Implements ITimeSignal.time
            Get
                Return _Event.time
            End Get
        End Property

        Public Overridable ReadOnly Property Value As Double Implements ITimeSignal.intensity
            Get
                Return _Event.intensity
            End Get
        End Property

        Public Overridable Property Weight As Double

        Public Overrides Function ToString() As String
            If _Event Is Nothing Then
                Return "[null event]"
            End If
            Return "[t=" & _Event.time & ", v=" & _Event.intensity & "]"
        End Function

        Public Overrides Function GetHashCode() As Integer
            If _Event Is Nothing Then
                Return MyBase.GetHashCode()
            End If
            Const prime As Integer = 31
            Dim result As Integer = 1
            result = prime * result + CInt(_Event.time Xor (CLng(CULng(_Event.time) >> 32)))
            Dim temp As Long
            temp = System.BitConverter.DoubleToInt64Bits(_Event.intensity)
            result = prime * result + CInt(temp Xor (CLng(CULng(temp) >> 32)))
            Return result
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            If Me Is obj Then
                Return True
            End If
            If obj Is Nothing Then
                Return False
            End If
            If Me.GetType() <> obj.GetType() Then
                Return False
            End If
            Dim other As WeightedEvent = DirectCast(obj, WeightedEvent)
            If other.Event Is Nothing OrElse _Event Is Nothing Then
                Return False
            End If
            If _Event.time <> other.Event.time Then
                Return False
            End If
            If _Event.intensity <> other.Event.intensity Then
                Return False
            End If
            Return True
        End Function

    End Class

End Namespace

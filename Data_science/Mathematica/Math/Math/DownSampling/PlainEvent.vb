#Region "Microsoft.VisualBasic::da16395e1423cc9532737a6a11e53a86, Data_science\Mathematica\Math\Math\DownSampling\PlainEvent.vb"

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

    '   Total Lines: 46
    '    Code Lines: 37 (80.43%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (19.57%)
    '     File Size: 1.43 KB


    '     Class PlainEvent
    ' 
    '         Properties: Time, Value
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Equals, GetHashCode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace DownSampling

    Public Class PlainEvent : Implements ITimeSignal

        Public Sub New(time As Long, value As Double)
            Me.Time = time
            Me.Value = value
        End Sub

        Public Sub New(time As Double, value As Double)
            Me.Time = time
            Me.Value = value
        End Sub

        Public Overridable ReadOnly Property Time As Double Implements ITimeSignal.time
        Public Overridable ReadOnly Property Value As Double Implements ITimeSignal.intensity

        Public Overrides Function GetHashCode() As Integer
            Const prime As Integer = 31
            Dim result As Integer = 1
            result = prime * result + CInt(Time Xor (CLng(CULng(Time) >> 32)))
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
            Dim other As PlainEvent = DirectCast(obj, PlainEvent)
            If Time <> other.Time Then
                Return False
            End If
            Return True
        End Function

    End Class

End Namespace

#Region "Microsoft.VisualBasic::3bac0745b5316dd2f2ecb86b9b30b2d8, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\core.Test\PriorityQueueTest.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ApplicationServices

Module PriorityQueueTest

    Sub Main()

        Dim q As New PriorityQueue(Of XXX)
        '   Dim h As New PriorityQueueTable(Of XXX)

        Call Time(Sub()
                      Call Test(q)
                  End Sub)

        'Call "table......................".__INFO_ECHO

        'Call Time(Sub()
        '              Call Test(h)
        '          End Sub)

        Pause()
    End Sub

    Sub Test(Of T As PriorityQueue(Of XXX))(q As T)
        Dim n = 5000

        Call println("enqueue")

        Call Time(Sub()
                      For i As Integer = 0 To n
                          Call q.Enqueue(New XXX With {.i = i})
                      Next
                  End Sub)

        Call println("contains")

        Call Time(Sub()
                      For Each x In q
                          Call q.Contains(x)
                      Next
                  End Sub)

        Call println("dequeue")

        Call Time(Sub()
                      For i As Integer = n / 3 To n / 2
                          Call q.Dequeue()
                      Next
                  End Sub)

        Dim lefts = q.ToList

        Call println("remove")

        Call Time(Sub()
                      For Each x In lefts
                          Call q.Remove(x)
                      Next
                  End Sub)
    End Sub

    Structure XXX
        Implements IComparable, IComparable(Of XXX), IReadOnlyId

        Dim i%
        Public ReadOnly Property Identity As String Implements IReadOnlyId.Identity
            Get
                Return CStr(i)
            End Get
        End Property

        Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
            Return CompareTo(other:=DirectCast(obj, XXX))
        End Function

        Public Function CompareTo(other As XXX) As Integer Implements IComparable(Of XXX).CompareTo
            Return Me.i.CompareTo(other.i)
        End Function
    End Structure
End Module


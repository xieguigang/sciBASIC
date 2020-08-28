#Region "Microsoft.VisualBasic::660bec7b7b3b17cef3440515d7cdc2d9, Data_science\MachineLearning\MachineLearning\SVM\Cache.vb"

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

    '     Class Cache
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetData
    ' 
    '         Sub: lru_delete, lru_insert, swap, SwapIndex
    '         Class head_t
    ' 
    '             Properties: EnclosingInstance
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' 
' * SVM.NET Library
' * Copyright (C) 2008 Matthew Johnson
' * 
' * This program is free software: you can redistribute it and/or modify
' * it under the terms of the GNU General Public License as published by
' * the Free Software Foundation, either version 3 of the License, or
' * (at your option) any later version.
' * 
' * This program is distributed in the hope that it will be useful,
' * but WITHOUT ANY WARRANTY; without even the implied warranty of
' * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' * GNU General Public License for more details.
' * 
' * You should have received a copy of the GNU General Public License
' * along with this program.  If not, see <http://www.gnu.org/licenses/>.



Imports System.Runtime.InteropServices

Namespace SVM
    Friend Class Cache
        Private _count As Integer
        Private _size As Long

        Private NotInheritable Class head_t
            Public Sub New(enclosingInstance As Cache)
                _enclosingInstance = enclosingInstance
            End Sub

            Private _enclosingInstance As Cache

            Public ReadOnly Property EnclosingInstance As Cache
                Get
                    Return _enclosingInstance
                End Get
            End Property

            Friend prev, [next] As head_t ' a cicular list
            Friend data As Single()
            Friend len As Integer ' data[0,len) is cached in this entry
        End Class

        Private head As head_t()
        Private lru_head As head_t

        Public Sub New(count As Integer, size As Long)
            _count = count
            _size = size
            head = New head_t(_count - 1) {}

            For i = 0 To _count - 1
                head(i) = New head_t(Me)
            Next

            _size = CLng(_size / 4)
            _size -= CLng(_count * (16 / 4)) ' sizeof(head_t) == 16
            lru_head = New head_t(Me)
            lru_head.prev = lru_head
            lru_head.next = lru_head
        End Sub

        Private Sub lru_delete(h As head_t)
            ' delete from current location
            h.prev.next = h.next
            h.next.prev = h.prev
        End Sub

        Private Sub lru_insert(h As head_t)
            ' insert to last position
            h.next = lru_head
            h.prev = lru_head.prev
            h.prev.next = h
            h.next.prev = h
        End Sub

        Private Shared Sub swap(Of T)(ByRef lhs As T, ByRef rhs As T)
            Dim tmp = lhs
            lhs = rhs
            rhs = tmp
        End Sub

        ' request data [0,len)
        ' return some position p where [p,len) need to be filled
        ' (p >= len if nothing needs to be filled)
        ' java: simulate pointer using single-element array
        Public Function GetData(index As Integer, <Out> ByRef data As Single(), len As Integer) As Integer
            Dim h = head(index)
            If h.len > 0 Then lru_delete(h)
            Dim more = len - h.len

            If more > 0 Then
                ' free old space
                While _size < more
                    Dim old = lru_head.next
                    lru_delete(old)
                    _size += old.len
                    old.data = Nothing
                    old.len = 0
                End While

                ' allocate new space
                Dim new_data = New Single(len - 1) {}
                If h.data IsNot Nothing Then Array.Copy(h.data, 0, new_data, 0, h.len)
                h.data = new_data
                _size -= more
                swap(h.len, len)
            End If

            lru_insert(h)
            data = h.data
            Return len
        End Function

        Public Sub SwapIndex(i As Integer, j As Integer)
            If i = j Then Return
            If head(i).len > 0 Then lru_delete(head(i))
            If head(j).len > 0 Then lru_delete(head(j))
            swap(head(i).data, head(j).data)
            swap(head(i).len, head(j).len)
            If head(i).len > 0 Then lru_insert(head(i))
            If head(j).len > 0 Then lru_insert(head(j))
            If i > j Then swap(i, j)
            Dim h = lru_head.next

            While h IsNot lru_head

                If h.len > i Then
                    If h.len > j Then
                        swap(h.data(i), h.data(j))
                    Else
                        ' give up
                        lru_delete(h)
                        _size += h.len
                        h.data = Nothing
                        h.len = 0
                    End If
                End If

                h = h.next
            End While
        End Sub
    End Class
End Namespace

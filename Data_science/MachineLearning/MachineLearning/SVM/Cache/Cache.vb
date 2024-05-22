#Region "Microsoft.VisualBasic::3b049b3ee3b51a8682534cdf96635719, Data_science\MachineLearning\MachineLearning\SVM\Cache\Cache.vb"

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

    '   Total Lines: 141
    '    Code Lines: 82 (58.16%)
    ' Comment Lines: 37 (26.24%)
    '    - Xml Docs: 48.65%
    ' 
    '   Blank Lines: 22 (15.60%)
    '     File Size: 4.51 KB


    '     Class Cache
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetData
    ' 
    '         Sub: lru_delete, lru_insert, SwapIndex
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

        Dim m_count As Integer
        Dim m_size As Long
        Dim head As head_t()
        Dim lru_head As head_t

        Public Sub New(count As Integer, size As Long)
            m_count = count
            m_size = size
            head = New head_t(m_count - 1) {}

            For i = 0 To m_count - 1
                head(i) = New head_t(Me)
            Next

            m_size = CLng(m_size / 4)
            m_size -= CLng(m_count * (16 / 4)) ' sizeof(head_t) == 16
            lru_head = New head_t(Me)
            lru_head.prev = lru_head
            lru_head.next = lru_head
        End Sub

        ''' <summary>
        ''' delete from current location
        ''' </summary>
        ''' <param name="h"></param>
        Private Sub lru_delete(h As head_t)
            h.prev.next = h.next
            h.next.prev = h.prev
        End Sub

        ''' <summary>
        ''' insert to last position
        ''' </summary>
        ''' <param name="h"></param>
        Private Sub lru_insert(h As head_t)
            h.next = lru_head
            h.prev = lru_head.prev
            h.prev.next = h
            h.next.prev = h
        End Sub

        ''' <summary>
        ''' request data [0,len)
        ''' return some position p where [p,len) need to be filled
        ''' (p >= len if nothing needs to be filled)
        ''' java: simulate pointer using single-element array
        ''' </summary>
        ''' <param name="index"></param>
        ''' <param name="data"></param>
        ''' <param name="len"></param>
        ''' <returns></returns>
        Public Function GetData(index As Integer, <Out> ByRef data As Single(), len As Integer) As Integer
            Dim h = head(index)
            If h.len > 0 Then lru_delete(h)
            Dim more = len - h.len

            If more > 0 Then
                ' free old space
                While m_size < more
                    Dim old = lru_head.next
                    lru_delete(old)
                    m_size += old.len
                    old.data = Nothing
                    old.len = 0
                End While

                ' allocate new space
                Dim new_data = New Single(len - 1) {}
                If h.data IsNot Nothing Then Array.Copy(h.data, 0, new_data, 0, h.len)
                h.data = new_data
                m_size -= more
                h.len.Swap(len)
            End If

            lru_insert(h)
            data = h.data
            Return len
        End Function

        Public Sub SwapIndex(i As Integer, j As Integer)
            If i = j Then
                Return
            End If

            If head(i).len > 0 Then lru_delete(head(i))
            If head(j).len > 0 Then lru_delete(head(j))

            Call head(i).data.Swap(head(j).data)
            Call head(i).len.Swap(head(j).len)

            If head(i).len > 0 Then lru_insert(head(i))
            If head(j).len > 0 Then lru_insert(head(j))

            If i > j Then
                i.Swap(j)
            End If

            Dim h = lru_head.next

            While h IsNot lru_head

                If h.len > i Then
                    If h.len > j Then
                        h.data(i).Swap(h.data(j))
                    Else
                        ' give up
                        lru_delete(h)
                        m_size += h.len
                        h.data = Nothing
                        h.len = 0
                    End If
                End If

                h = h.next
            End While
        End Sub
    End Class
End Namespace

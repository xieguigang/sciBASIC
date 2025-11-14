#Region "Microsoft.VisualBasic::1ddff95eb64e37e432b078b011527db6, Microsoft.VisualBasic.Core\src\ComponentModel\DataStructures\Deque\ReverseQueue.vb"

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

    '   Total Lines: 169
    '    Code Lines: 87 (51.48%)
    ' Comment Lines: 59 (34.91%)
    '    - Xml Docs: 96.61%
    ' 
    '   Blank Lines: 23 (13.61%)
    '     File Size: 6.36 KB


    '     Class ReverseQueue
    ' 
    '         Properties: Count, First, IsReadOnly, Last
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Contains, GetEnumerator, GetEnumerator1, IndexOf, Remove
    '                   RemoveHead, RemoveTail, Reverse
    ' 
    '         Sub: Add, AddHead, Clear, CopyTo, Insert
    '              RemoveAt
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.Collection.Deque

    Public Class ReverseQueue(Of T) : Implements IDeque(Of T)

        Default Public Property Item(index As Integer) As T Implements IList(Of T).Item
            Get
                Return deque(Count - 1 - index)
            End Get
            Set(value As T)
                deque(Count - 1 - index) = value
            End Set
        End Property

        ''' <summary>
        ''' peek the firts element of the reversed Deque(Of T)
        ''' </summary>
        Public ReadOnly Property First As T Implements IDeque(Of T).First
            Get
                Return deque.Last
            End Get
        End Property

        ''' <summary>
        ''' peek the last element of the reversed Deque(Of T)
        ''' </summary>
        Public ReadOnly Property Last As T Implements IDeque(Of T).Last
            Get
                Return deque.First
            End Get
        End Property

        ''' <summary>
        ''' Number of elements in Deque(Of T)
        ''' </summary>
        Public ReadOnly Property Count As Integer Implements ICollection(Of T).Count
            Get
                Return deque.Count
            End Get
        End Property

        Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of T).IsReadOnly
            Get
                Return deque.IsReadOnly
            End Get
        End Property

        ''' <summary>
        ''' Deque(Of T) that this instance of ReverseView wraps and allows to access in the reversed order
        ''' </summary>
        Dim deque As Deque(Of T)

        Public Sub New(que As Deque(Of T))
            deque = que
        End Sub

        ''' <summary>
        ''' Adds an object to the end of the reversed Deque(Of T).
        ''' </summary>
        ''' <param name="item"></param>
        Public Sub Add(item As T) Implements ICollection(Of T).Add
            deque.AddHead(item)
        End Sub

        ''' <summary>
        ''' Adds an element to the beggining of the reversed Deque(Of T)
        ''' </summary>
        ''' <param name="item"></param>
        Public Sub AddHead(item As T) Implements IDeque(Of T).AddHead
            deque.Add(item)
        End Sub

        ''' <summary>
        ''' Removes all elements from the Deque(Of T).
        ''' </summary> 
        Public Sub Clear() Implements ICollection(Of T).Clear
            deque.Clear()
        End Sub

        ''' <summary>
        ''' Determines whether an element is in the Deque(Of T).
        ''' </summary>
        ''' <param name="item"></param>
        ''' <returns>true if item is found in the List(Of T); otherwise, false</returns> 
        Public Function Contains(item As T) As Boolean Implements ICollection(Of T).Contains
            Return deque.Contains(item)
        End Function

        ''' <summary>
        ''' Copies the entire Deque(Of T) to a compatible one-dimensional array, starting at the specified index of the target array.
        ''' </summary> 
        Public Sub CopyTo(array As T(), arrayIndex As Integer) Implements ICollection(Of T).CopyTo
            deque.CopyToReversed(array, arrayIndex)
        End Sub

        Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            Return New ReversedEnumerator(Of T)(deque, deque.version)
        End Function

        Private Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function

        ''' <summary>
        ''' Searches for the specified object and returns the zero-based index of the first occurrence within the entire Deque(Of T).
        ''' </summary>
        ''' <returns>e zero-based index of the first occurrence of item within the entire Deque(Of T), if found; otherwise, -1.</returns>
        Public Function IndexOf(item As T) As Integer Implements IList(Of T).IndexOf
            For index As Integer = 0 To Count - 1
                If Equals(Me(index), item) Then
                    Return index
                End If
            Next

            Return -1
        End Function

        ''' <summary>
        ''' Inserts an element into the reversedDeque(Of T) at the specified index.
        ''' </summary>
        Public Sub Insert(index As Integer, item As T) Implements IList(Of T).Insert
            'user wants to add item to begining of reversed list
            If index = 0 Then
                'I need to add it to end of actual list
                deque.Add(item)
            Else
                deque.Insert(Count - index, item)
            End If
        End Sub

        ''' <summary>
        ''' Removes the first occurrence of a specific object from the Deque(Of T).
        ''' </summary>
        ''' <returns>true if item is successfully removed; otherwise, false. This method also returns false if item was not found in the List(Of T).</returns>
        Public Function Remove(item As T) As Boolean Implements ICollection(Of T).Remove
            Return deque.Remove(item)
        End Function

        ''' <summary>
        ''' Removes the element at the specified index of the Deque(Of T).
        ''' </summary>
        Public Sub RemoveAt(index As Integer) Implements IList(Of T).RemoveAt
            deque.RemoveAt(Count - 1 - index)
        End Sub

        ''' <summary>
        ''' returns the firts element of the reversed Deque(Of T) and removes it from Deque(Of T)
        ''' </summary>
        ''' <returns></returns>
        Public Function RemoveHead() As T Implements IDeque(Of T).RemoveHead
            Return deque.RemoveTail()
        End Function

        ''' <summary>
        ''' returns the last element of the reversed Deque(Of T) and removes it from Deque(Of T)
        ''' </summary>
        ''' <returns></returns>
        Public Function RemoveTail() As T Implements IDeque(Of T).RemoveTail
            Return deque.RemoveHead()
        End Function

        ''' <summary>
        ''' returns 
        ''' </summary>
        ''' <returns></returns>
        Public Function Reverse() As IDeque(Of T) Implements IDeque(Of T).Reverse
            Return deque
        End Function
    End Class
End Namespace

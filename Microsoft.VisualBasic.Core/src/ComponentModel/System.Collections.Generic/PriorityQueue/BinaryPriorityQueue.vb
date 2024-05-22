#Region "Microsoft.VisualBasic::7ea121c6ee5b5da0ea643b7db0334da4, Microsoft.VisualBasic.Core\src\ComponentModel\System.Collections.Generic\PriorityQueue\BinaryPriorityQueue.vb"

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

    '   Total Lines: 260
    '    Code Lines: 197 (75.77%)
    ' Comment Lines: 28 (10.77%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 35 (13.46%)
    '     File Size: 9.02 KB


    '     Class BinaryPriorityQueue
    ' 
    '         Properties: Count, IList_IsFixedSize, IList_IsReadOnly, IsSynchronized, SyncRoot
    ' 
    '         Constructor: (+5 Overloads) Sub New
    ' 
    '         Function: [ReadOnly], Clone, Contains, IEnumerable_GetEnumerator, IList_Add
    '                   IList_IndexOf, OnCompare, Peek, Pop, Push
    '                   Syncronized
    ' 
    '         Sub: Clear, CopyTo, IList_Insert, IList_Remove, IList_RemoveAt
    '              SwitchElements, Update
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace ComponentModel.Collection

    Public Class BinaryPriorityQueue
        Implements IPriorityQueue(Of Object)
        Implements ICollection
        Implements ICloneable
        Implements IList
        Protected InnerList As New ArrayList()
        Protected Comparer As IComparer

#Region "contructors"
        Public Sub New()
            Me.New(System.Collections.Comparer.[Default])
        End Sub

        Public Sub New(c As IComparer)
            Comparer = c
        End Sub

        Public Sub New(C As Integer)
            Me.New(System.Collections.Comparer.[Default], C)
        End Sub

        Public Sub New(c As IComparer, Capacity As Integer)
            Comparer = c
            InnerList.Capacity = Capacity
        End Sub

        Protected Sub New(Core As ArrayList, Comp As IComparer, Copy As Boolean)
            If Copy Then
                InnerList = TryCast(Core.Clone(), ArrayList)
            Else
                InnerList = Core
            End If
            Comparer = Comp
        End Sub
#End Region

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Protected Sub SwitchElements(i As Integer, j As Integer)
            Dim h As Object = InnerList(i)
            InnerList(i) = InnerList(j)
            InnerList(j) = h
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Protected Overridable Function OnCompare(i As Integer, j As Integer) As Integer
            Return Comparer.Compare(InnerList(i), InnerList(j))
        End Function

#Region "Public methods of Queue"

        ''' <summary>
        ''' Push an object onto the PQ
        ''' </summary>
        ''' <param name="O">The new object</param>
        ''' <returns>The index in the list where the object is _now_. This will change when objects are taken from or put onto the PQ.</returns>
        Public Function Push(O As Object) As Integer Implements IPriorityQueue(Of Object).Push
            Dim p As Integer = InnerList.Count, p2 As Integer
            InnerList.Add(O)
            ' E[p] = O
            Do
                If p = 0 Then
                    Exit Do
                End If
                p2 = (p - 1) \ 2
                If OnCompare(p, p2) < 0 Then
                    SwitchElements(p, p2)
                    p = p2
                Else
                    Exit Do
                End If
            Loop While True

            Return p
        End Function

        ''' <summary>
        ''' Get the smallest object and remove it.
        ''' </summary>
        ''' <returns>The smallest object</returns>
        Public Function Pop() As Object Implements IPriorityQueue(Of Object).Pop
            Dim result As Object = InnerList(0)
            Dim p As Integer = 0, p1 As Integer, p2 As Integer, pn As Integer
            InnerList(0) = InnerList(InnerList.Count - 1)
            InnerList.RemoveAt(InnerList.Count - 1)
            Do
                pn = p
                p1 = 2 * p + 1
                p2 = 2 * p + 2
                If InnerList.Count > p1 AndAlso OnCompare(p, p1) > 0 Then
                    ' links kleiner
                    p = p1
                End If
                If InnerList.Count > p2 AndAlso OnCompare(p, p2) > 0 Then
                    ' rechts noch kleiner
                    p = p2
                End If

                If p = pn Then
                    Exit Do
                End If
                SwitchElements(p, pn)
            Loop While True

            Return result
        End Function

        ''' <summary>
        ''' Notify the PQ that the object at position i has changed
        ''' and the PQ needs to restore order.
        ''' Since you dont have access to any indexes (except by using the
        ''' explicit IList.this) you should not call this function without knowing exactly
        ''' what you do.
        ''' </summary>
        ''' <param name="i">The index of the changed object.</param>
        Public Sub Update(i As Integer)
            Dim p As Integer = i, pn As Integer
            Dim p1 As Integer, p2 As Integer
            Do
                ' aufsteigen
                If p = 0 Then
                    Exit Do
                End If
                p2 = (p - 1) \ 2
                If OnCompare(p, p2) < 0 Then
                    SwitchElements(p, p2)
                    p = p2
                Else
                    Exit Do
                End If
            Loop While True
            If p < i Then
                Return
            End If
            Do
                ' absteigen
                pn = p
                p1 = 2 * p + 1
                p2 = 2 * p + 2
                If InnerList.Count > p1 AndAlso OnCompare(p, p1) > 0 Then
                    ' links kleiner
                    p = p1
                End If
                If InnerList.Count > p2 AndAlso OnCompare(p, p2) > 0 Then
                    ' rechts noch kleiner
                    p = p2
                End If

                If p = pn Then
                    Exit Do
                End If
                SwitchElements(p, pn)
            Loop While True
        End Sub

        ''' <summary>
        ''' Get the smallest object without removing it.
        ''' </summary>
        ''' <returns>The smallest object</returns>
        Public Function Peek() As Object Implements IPriorityQueue(Of Object).Peek
            If InnerList.Count > 0 Then
                Return InnerList(0)
            End If
            Return Nothing
        End Function
#End Region

#Region "explicit implementation"
        Public Function Contains(value As Object) As Boolean Implements IList.Contains
            Return InnerList.Contains(value)
        End Function

        Public Sub Clear() Implements IList.Clear
            InnerList.Clear()
        End Sub

        Public ReadOnly Property Count() As Integer Implements ICollection.Count
            Get
                Return InnerList.Count
            End Get
        End Property

        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return InnerList.GetEnumerator()
        End Function

        Public Sub CopyTo(array As Array, index As Integer) Implements ICollection.CopyTo
            InnerList.CopyTo(array, index)
        End Sub

        Public Function Clone() As Object Implements ICloneable.Clone
            Return New BinaryPriorityQueue(InnerList, Comparer, True)
        End Function

        Public ReadOnly Property IsSynchronized() As Boolean Implements ICollection.IsSynchronized
            Get
                Return InnerList.IsSynchronized
            End Get
        End Property

        Public ReadOnly Property SyncRoot() As Object Implements ICollection.SyncRoot
            Get
                Return Me
            End Get
        End Property

        Private ReadOnly Property IList_IsReadOnly() As Boolean Implements IList.IsReadOnly
            Get
                Return False
            End Get
        End Property

        Default Public Property IList_Item(index As Integer) As Object Implements IList.Item
            Get
                Return InnerList(index)
            End Get
            Set(value As Object)
                InnerList(index) = value
                Update(index)
            End Set
        End Property

        Private Function IList_Add(o As Object) As Integer Implements IList.Add
            Return Push(o)
        End Function

        Private Sub IList_RemoveAt(index As Integer) Implements IList.RemoveAt
            Throw New NotSupportedException()
        End Sub

        Private Sub IList_Insert(index As Integer, value As Object) Implements IList.Insert
            Throw New NotSupportedException()
        End Sub

        Private Sub IList_Remove(value As Object) Implements IList.Remove
            Throw New NotSupportedException()
        End Sub

        Private Function IList_IndexOf(value As Object) As Integer Implements IList.IndexOf
            Throw New NotSupportedException()
        End Function

        Private ReadOnly Property IList_IsFixedSize() As Boolean Implements IList.IsFixedSize
            Get
                Return False
            End Get
        End Property

        Public Shared Function Syncronized(P As BinaryPriorityQueue) As BinaryPriorityQueue
            Return New BinaryPriorityQueue(ArrayList.Synchronized(P.InnerList), P.Comparer, False)
        End Function
        Public Shared Function [ReadOnly](P As BinaryPriorityQueue) As BinaryPriorityQueue
            Return New BinaryPriorityQueue(ArrayList.[ReadOnly](P.InnerList), P.Comparer, False)
        End Function
#End Region
    End Class
End Namespace

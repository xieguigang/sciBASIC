#Region "Microsoft.VisualBasic::adc97a216a255b5b2342bf03d8d0493b, ..\visualbasic_App\UXFramework\DataVisualization.Enterprise\Microsoft.VisualBasic.DataVisualization.Enterprise\PieChart\PieChartControls\PieChartControl.ItemCollection.vb"

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

Imports System.Collections
Imports System.Collections.Generic
Imports System.Text
Imports System.Windows.Forms
Imports System.ComponentModel
Imports System.ComponentModel.Design
Imports System.Drawing.Design

Namespace Windows.Forms.Nexus

    Partial Public Class PieChart
        Inherits Control
        ''' <summary>
        ''' Stores a collection of PieChartItem objects associated with a PieChart.
        ''' </summary>
        <Editor(GetType(ItemCollectionEditor), GetType(UITypeEditor))>
        Public Class ItemCollection
            Implements IList(Of PieChartItem)
            Implements IList
#Region "Constructor"
            ''' <summary>
            ''' Constructs a new instance.
            ''' </summary>
            ''' <param name="container">The PieChart that this collection is associated with.</param>
            Friend Sub New(container As PieChart)
                Me.container = container
                Me.items = New List(Of PieChartItem)()
            End Sub
#End Region

#Region "Fields"
            ''' <summary>
            ''' The PieChart this collection is associated with.
            ''' </summary>
            Private container As PieChart

            ''' <summary>
            ''' The list of items stored in this control.
            ''' </summary>
            Private items As List(Of PieChartItem)

            ''' <summary>
            ''' The total weight of all items in this collection.
            ''' </summary>
            Private totalWeight As Double = 0
#End Region

#Region "Properties"
            Public ReadOnly Property TotalItemWeight() As Double
                Get
                    Return totalWeight
                End Get
            End Property
#End Region

#Region "Methods"
            ''' <summary>
            ''' Sorts the items in the collection based on item weight.
            ''' </summary>
            Public Sub Sort()
                items.Sort()
                container.MarkStructuralChange()
            End Sub

            ''' <summary>
            ''' Sorts the items in the collection using the provided comparer.
            ''' </summary>
            ''' <param name="comparer">The comparer used to compare PieChartItems.</param>
            Public Sub Sort(comparer As IComparer(Of PieChartItem))
                items.Sort(comparer)
                container.MarkStructuralChange()
            End Sub

            Friend Sub ChangeItemWeight(difference As Double)
                totalWeight += difference
            End Sub
#End Region

#Region "IEnumerable Members"
            ''' <summary>
            ''' Returns an enumerator that iterates through a collection.
            ''' </summary>
            ''' <returns>An IEnumerator object that can be used to iterate through the collection.</returns>
            Public Function GetEnumerator() As IEnumerator(Of PieChartItem) Implements IEnumerable(Of PieChartItem).GetEnumerator
                Return items.GetEnumerator()
            End Function

            ''' <summary>
            ''' Returns an enumerator that iterates through a collection.
            ''' </summary>
            ''' <returns>An IEnumerator object that can be used to iterate through the collection.</returns>
            Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
                Return items.GetEnumerator()
            End Function
#End Region

#Region "ICollection<T> Members"
            ''' <summary>
            ''' Gets the number of elements contained in the collection.
            ''' </summary>
            Public ReadOnly Property Count() As Integer Implements ICollection(Of PieChartItem).Count, ICollection.Count
                Get
                    Return items.Count
                End Get
            End Property

            ''' <summary>
            ''' Gets a value indicating whether the collection is read-only.
            ''' </summary>
            Private ReadOnly Property ICollection_IsReadOnly() As Boolean Implements ICollection(Of PieChartItem).IsReadOnly
                Get
                    Return False
                End Get
            End Property

            ''' <summary>
            ''' Adds an item to the collection.
            ''' </summary>
            ''' <param name="item">The item to add to the collection.</param>
            Public Sub Add(item As PieChartItem) Implements IList(Of PieChartItem).Add
                items.Add(item)
                item.SetOwner(container)
                container.MarkStructuralChange()
            End Sub

            ''' <summary>
            ''' Removes all items from the collection.
            ''' </summary>
            Public Sub Clear() Implements ICollection(Of PieChartItem).Clear, IList.Clear
                For Each item As PieChartItem In items
                    item.SetOwner(Nothing)
                Next

                items.Clear()
                container.MarkStructuralChange()
            End Sub

            ''' <summary>
            ''' Determines whether the collection contains a specific value.
            ''' </summary>
            ''' <param name="item">The object to locate in the collection.</param>
            ''' <returns>True if the item is found in the collection, otherwise false.</returns>
            Public Function Contains(item As PieChartItem) As Boolean Implements IList(Of PieChartItem).Contains
                Return items.Contains(item)
            End Function

            ''' <summary>
            ''' Copies the elements of the collection to an array, starting at a particular array index.
            ''' </summary>
            ''' <param name="array">The one-dimensional array that is the destination of the elements copied from the collection.
            ''' The array must have zero-based indexing.</param>
            ''' <param name="index">The zero-based index in array at which copying begins.</param>
            Private Sub ICollection_CopyTo(array As PieChartItem(), index As Integer) Implements ICollection(Of PieChartItem).CopyTo
                items.CopyTo(array, index)
            End Sub

            ''' <summary>
            ''' Removes the first occurrence of a specific object from the collection.
            ''' </summary>
            ''' <param name="item">The object to remove from the collection.</param>
            ''' <returns>True if the item was successfully removed from the colleection, otherwise false.  This method
            ''' also returns false if the item is not found in the original collection.</returns>
            Public Function Remove(item As PieChartItem) As Boolean Implements IList(Of PieChartItem).Remove
                item.SetOwner(Nothing)
                Dim rval As Boolean = items.Remove(item)
                container.MarkStructuralChange()
                Return rval
            End Function
#End Region

#Region "ICollection Members"
            ''' <summary>
            ''' Copies the elements of the collection to an array, starting at a particular array index.
            ''' </summary>
            ''' <param name="array">The one-dimensional array that is the destination of the elements copied from the collection.
            ''' The array must have zero-based indexing.</param>
            ''' <param name="index">The zero-based index in array at which copying begins</param>
            Private Sub ICollection_CopyTo(array As Array, index As Integer) Implements ICollection.CopyTo
                DirectCast(items, ICollection).CopyTo(array, index)
            End Sub

            ''' <summary>
            ''' Gets an object that can be used to synchronize access to the collection.
            ''' </summary>
            Private ReadOnly Property ICollection_SyncRoot() As Object Implements ICollection.SyncRoot
                Get
                    Return Me
                End Get
            End Property

            ''' <summary>
            ''' Gets a value indicating whether access to the collection is synchronized (thread safe).
            ''' </summary>
            Private ReadOnly Property ICollection_IsSynchronized() As Boolean Implements ICollection.IsSynchronized
                Get
                    Return False
                End Get
            End Property
#End Region

#Region "IList<T> Members"
            ''' <summary>
            ''' Gets or sets the element at the specified index.
            ''' </summary>
            ''' <param name="index">The zero-based index of the element to get or set.</param>
            ''' <returns>The element at the specified index.</returns>
            Default Public Property Item(index As Integer) As PieChartItem Implements IList(Of PieChartItem).Item
                Get
                    Return items(index)
                End Get
                Set(value As PieChartItem)
                    If items(index) IsNot value Then
                        items(index).SetOwner(Nothing)
                        items(index) = value
                        items(index).SetOwner(container)
                        container.MarkStructuralChange()
                    End If
                End Set
            End Property

            ''' <summary>
            ''' Determines the index of a specific item in the list.
            ''' </summary>
            ''' <param name="item">The object to locate in the list.</param>
            ''' <returns>The index of the item if found in the list, otherwise -1.</returns>
            Public Function IndexOf(item As PieChartItem) As Integer Implements IList(Of PieChartItem).IndexOf
                Return items.IndexOf(item)
            End Function

            ''' <summary>
            ''' Inserts an item to the list at the specified index.
            ''' </summary>
            ''' <param name="index">The zero-based index at which item should be inserted.</param>
            ''' <param name="item">The object to insert into the list.</param>
            Public Sub Insert(index As Integer, item As PieChartItem) Implements IList(Of PieChartItem).Insert
                item.SetOwner(container)
                items.Insert(index, item)
                container.MarkStructuralChange()
            End Sub

            ''' <summary>
            ''' Removes the item at the specified index.
            ''' </summary>
            ''' <param name="index">The zero-based index of the item to remove.</param>
            Public Sub RemoveAt(index As Integer) Implements IList(Of PieChartItem).RemoveAt, IList.RemoveAt
                items(index).SetOwner(Nothing)
                items.RemoveAt(index)
                container.MarkStructuralChange()
            End Sub
#End Region

#Region "IList Members"
            ''' <summary>
            ''' Adds an item to the list.
            ''' </summary>
            ''' <param name="obj">The item to add to the list.</param>
            ''' <returns>The position at which the item was inserted.</returns>
            Private Function Add(obj As Object) As Integer Implements IList.Add
                Me.Add(DirectCast(obj, PieChartItem))
                Return Me.Count - 1
            End Function

            ''' <summary>
            ''' Determines whether the list contains a specific value.
            ''' </summary>
            ''' <param name="obj">The object to locate in the list.</param>
            ''' <returns>True if an instance of the item was found in the list, otherwise false.</returns>
            Private Function Contains(obj As Object) As Boolean Implements IList.Contains
                Return Me.Contains(DirectCast(obj, PieChartItem))
            End Function

            ''' <summary>
            ''' Determines the index of a specific item in the list.
            ''' </summary>
            ''' <param name="obj">The object to locate in the list.</param>
            ''' <returns>The index of the item if found in the list, otherwise -1.</returns>
            Public Function IndexOf(obj As Object) As Integer Implements IList.IndexOf
                Return Me.IndexOf(DirectCast(obj, PieChartItem))
            End Function

            ''' <summary>
            ''' Inserts an item to the list at the specified index.
            ''' </summary>
            ''' <param name="index">The zero-based index at which item should be inserted.</param>
            ''' <param name="obj">The object to insert into the list.</param>
            Private Sub Insert(index As Integer, obj As Object) Implements IList.Insert
                Me.Insert(index, DirectCast(obj, PieChartItem))
            End Sub

            ''' <summary>
            ''' Removes the first occurrence of a specific object from the collection.
            ''' </summary>
            ''' <param name="obj">The object to remove from the collection.</param>
            Private Sub Remove(obj As Object) Implements IList.Remove
                Me.Remove(DirectCast(obj, PieChartItem))
            End Sub

            ''' <summary>
            ''' Gets or sets the element at the specified index.
            ''' </summary>
            ''' <param name="index">The zero-based index of the element to get or set.</param>
            ''' <returns>The element at the specified index.</returns>
            Public Property Item2(index As Integer) As Object Implements IList.Item
                Get
                    Return Me(index)
                End Get
                Set(value As Object)
                    Me(index) = DirectCast(value, PieChartItem)
                End Set
            End Property

            ''' <summary>
            ''' Gets a value indicating whether the list is read-only.
            ''' </summary>
            Private ReadOnly Property IsReadOnly() As Boolean Implements IList.IsReadOnly
                Get
                    Return False
                End Get
            End Property

            ''' <summary>
            ''' Gets a value indicating whether the list has a fixed size.
            ''' </summary>
            Private ReadOnly Property IsFixedSize() As Boolean Implements IList.IsFixedSize
                Get
                    Return False
                End Get
            End Property
#End Region
        End Class
    End Class

End Namespace

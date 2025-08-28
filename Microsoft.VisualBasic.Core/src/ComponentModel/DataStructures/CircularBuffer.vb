#Region "Microsoft.VisualBasic::d4c005bc2003c79169245380689a3152, Microsoft.VisualBasic.Core\src\ComponentModel\DataStructures\CircularBuffer.vb"

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

    '   Total Lines: 148
    '    Code Lines: 75 (50.68%)
    ' Comment Lines: 58 (39.19%)
    '    - Xml Docs: 93.10%
    ' 
    '   Blank Lines: 15 (10.14%)
    '     File Size: 6.03 KB


    '     Class CircularBuffer
    ' 
    '         Properties: Capacity, Count, First, HeadIndex, Last
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetEnumerator, IEnumerable_GetEnumerator
    ' 
    '         Sub: Add, Clear
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.Collection

    ''' <summary>
    ''' Represents a fixed-capacity circular buffer (or ring list).
    ''' When the buffer is full, adding a new element overwrites the oldest element.
    ''' </summary>
    ''' <typeparam name="T">The type of elements in the buffer.</typeparam>
    Public Class CircularBuffer(Of T) : Implements IEnumerable(Of T)

        ReadOnly _data As T()

        ''' <summary>
        ''' Points to the next position to write (tail)
        ''' </summary>
        Dim _ends As Integer

        ''' <summary>
        ''' Gets the number of elements currently stored in the buffer.
        ''' </summary>
        Public Property Count As Integer

        ''' <summary>
        ''' Gets the total capacity of the buffer.
        ''' </summary>
        ''' <remarks>
        ''' if this property value is equals to the <see cref="Count"/> then it
        ''' means current array buffer is full of data
        ''' </remarks>
        Public ReadOnly Property Capacity As Integer
            Get
                Return _data.Length
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the element at the specified logical index.
        ''' Index 0 is the oldest element, index Count-1 is the newest.
        ''' </summary>
        ''' <param name="index">The logical index of the element.</param>
        ''' <exception cref="IndexOutOfRangeException">Thrown when the index is out of range.</exception>
        Default Public Property Item(index As Integer) As T
            Get
                If index < 0 OrElse index >= Count Then
                    Throw New IndexOutOfRangeException("Index is out of the valid range of the buffer.")
                End If
                Dim actualIndex As Integer = (HeadIndex + index) Mod Capacity
                Return _data(actualIndex)
            End Get
            Set(value As T)
                If index < 0 OrElse index >= Count Then
                    Throw New IndexOutOfRangeException("Index is out of the valid range of the buffer.")
                End If
                Dim actualIndex As Integer = (HeadIndex + index) Mod Capacity
                _data(actualIndex) = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the first (oldest) element in the buffer.
        ''' </summary>
        ''' <exception cref="InvalidOperationException">Thrown when the buffer is empty.</exception>
        Public ReadOnly Property First As T
            Get
                If Count = 0 Then Throw New InvalidOperationException("Buffer is empty.")
                Return _data(HeadIndex)
            End Get
        End Property

        ''' <summary>
        ''' Gets the last (newest) element in the buffer.
        ''' </summary>
        ''' <exception cref="InvalidOperationException">Thrown when the buffer is empty.</exception>
        Public ReadOnly Property Last As T
            Get
                If Count = 0 Then Throw New InvalidOperationException("Buffer is empty.")
                ' _end points to the next write position, so the last written position is (_end - 1)
                Dim lastIndex As Integer = (_ends - 1 + Capacity) Mod Capacity
                Return _data(lastIndex)
            End Get
        End Property

        ''' <summary>
        ''' Calculates and gets the index of the first element (head) in the internal array.
        ''' </summary>
        Private ReadOnly Property HeadIndex As Integer
            Get
                If Count = 0 Then Return 0
                ' When Count < Capacity, _end equals Count, resulting in 0.
                ' When the buffer is full (Count == Capacity), _end wraps; this formula correctly computes the head index.
                Return (_ends - Count + Capacity) Mod Capacity
            End Get
        End Property

        ''' <summary>
        ''' Initializes a new instance of the CircularBuffer class.
        ''' </summary>
        ''' <param name="capacity">The buffer capacity. Must be a positive number.</param>
        ''' <exception cref="ArgumentException">Thrown when capacity is ≤ 0.</exception>
        Public Sub New(capacity As Integer)
            If capacity <= 0 Then
                Throw New ArgumentException("Capacity must be a positive number.", NameOf(capacity))
            End If
            _data = New T(capacity - 1) {}
            _ends = 0
            Count = 0
        End Sub

        ''' <summary>
        ''' Adds a new element to the tail of the buffer.
        ''' If the buffer is full, this operation overwrites the oldest element.
        ''' </summary>
        ''' <param name="item">The element to add.</param>
        Public Sub Add(item As T)
            _data(_ends) = item
            _ends = (_ends + 1) Mod Capacity

            If Count < Capacity Then
                Count += 1
            End If
        End Sub

        ''' <summary>
        ''' Clears all elements in the buffer.
        ''' </summary>
        Public Sub Clear()
            ' Only reset counters; no need to clear array data
            Count = 0
            _ends = 0
        End Sub

        ''' <summary>
        ''' Returns an enumerator that iterates through the collection.
        ''' </summary>
        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            Dim head As Integer = HeadIndex
            For i As Integer = 0 To Count - 1
                Yield _data((head + i) Mod Capacity)
            Next
        End Function

        ''' <summary>
        ''' Returns an enumerator that iterates through the collection.
        ''' </summary>
        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function
    End Class
End Namespace

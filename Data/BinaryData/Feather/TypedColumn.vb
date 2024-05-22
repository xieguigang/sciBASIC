#Region "Microsoft.VisualBasic::3cf3a448834d7ea516fc413f0b8d953d, Data\BinaryData\Feather\TypedColumn.vb"

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

    '   Total Lines: 447
    '    Code Lines: 217 (48.55%)
    ' Comment Lines: 174 (38.93%)
    '    - Xml Docs: 87.93%
    ' 
    '   Blank Lines: 56 (12.53%)
    '     File Size: 17.06 KB


    ' Class TypedColumnEnumerator
    ' 
    '     Properties: Current, CurrentProp
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: MoveNext
    ' 
    '     Sub: Dispose, Reset
    ' 
    ' Class TypedColumn
    ' 
    '     Properties: Count, Index, IsReadOnly, Length, Name
    '                 Type
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Contains, Equals, GetEnumerator, GetEnumerator1, GetEnumerator2
    '               GetHashCode, GetRange, IndexOf, LongIndexOf, Remove
    '               ToArray, ToString, (+3 Overloads) TryGetValue
    ' 
    '     Sub: Add, Clear, CopyTo, (+6 Overloads) GetRange, Insert
    '          RemoveAt, (+3 Overloads) ToArray
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.DataStorage.FeatherFormat.Impl

''' <summary>
''' Allocation free enumerator for a typed column.
''' </summary>
Public Class TypedColumnEnumerator(Of TColumnType)
    Implements IEnumerator(Of TColumnType)

    ''' <summary>
    ''' <see cref="System.Collections.Generic.IEnumerator(Of T)"/>
    ''' </summary>
    Private _CurrentProp As TColumnType
    Private Inner As Column
    Private Index As Long

    Public Property CurrentProp As TColumnType Implements IEnumerator(Of TColumnType).Current
        Get
            Return _CurrentProp
        End Get
        Private Set(value As TColumnType)
            _CurrentProp = value
        End Set
    End Property

    Private ReadOnly Property Current As Object Implements IEnumerator.Current
        Get
            Return CurrentProp
        End Get
    End Property

    Friend Sub New(inner As Column)
        CurrentProp = Nothing
        Me.Inner = inner
        Index = -1
    End Sub

    ''' <summary>
    ''' <see cref="System.Collections.Generic.IEnumerator(Of T)"/>
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
    End Sub

    ''' <summary>
    ''' <see cref="System.Collections.Generic.IEnumerator(Of T)"/>
    ''' </summary>
    Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
        Index += 1

        Dim value As Value = Nothing
        If Not Inner.TryGetValueTranslated(Index, value) Then Return False

        Dim category = Inner.Parent.Metadata.Columns(Inner.TranslatedColumnIndex).GetCategoryEnumMap(Of TColumnType)()

        CurrentProp = value.UnsafeCast(Of TColumnType)(category)
        Return True
    End Function

    ''' <summary>
    ''' <see cref="System.Collections.Generic.IEnumerator(Of T)"/>
    ''' </summary>
    Public Sub Reset() Implements IEnumerator.Reset
        Index = -1
    End Sub
End Class

''' <summary>
''' Represents a column of a TypedDataFrame, or a column of a DataFrame that has been mapped.
''' 
''' Typing is validated eagerly, but coercision of particular values is done lazily.
''' </summary>
Public Class TypedColumn(Of TColumnType)
    Implements IEnumerable(Of TColumnType), IColumn(Of TColumnType)
    Private Inner As Column

    ''' <summary>
    ''' Returns the Index (in the appropriate basis) of this column in the original dataframe.
    ''' </summary>
    Public ReadOnly Property Index As Long Implements IColumn(Of TColumnType).Index
        Get
            Return Inner.Index
        End Get
    End Property
    ''' <summary>
    ''' Returns the name of this column.
    ''' </summary>
    Public ReadOnly Property Name As String Implements IColumn(Of TColumnType).Name
        Get
            Return Inner.Name
        End Get
    End Property
    ''' <summary>
    ''' Returns the .NET equivalent type of this column.
    ''' </summary>
    Public ReadOnly Property Type As Type Implements IColumn(Of TColumnType).Type
        Get
            Return Inner.Type
        End Get
    End Property
    ''' <summary>
    ''' Returns the number of entries in the this column.
    ''' This will always match the number of rows in the original dataframe.
    ''' </summary>
    Public ReadOnly Property Length As Long Implements IColumn(Of TColumnType).Length
        Get
            Return Inner.Length
        End Get
    End Property

    ''' <summary>
    ''' <see cref="System.Collections.Generic.IList(Of T)"/>
    ''' 
    ''' Throws if <see cref="Length"/> will not fit in an int.
    ''' </summary>
    Private ReadOnly Property Count As Integer Implements ICollection(Of TColumnType).Count
        Get
            Return Length
        End Get
    End Property

    ''' <summary>
    ''' <see cref="System.Collections.Generic.IList(Of T)"/>
    ''' 
    ''' Always return true.
    ''' </summary>
    Private ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of TColumnType).IsReadOnly
        Get
            Return True
        End Get
    End Property

    ''' <summary>
    ''' &lt;see cref="this[long]"/&gt;
    ''' <see cref="System.Collections.Generic.IList(Of T)"/>
    ''' </summary>
    Default Public Property Item(index As Integer) As TColumnType Implements IList(Of TColumnType).Item
        Get
            Return Me(CLng(index))
        End Get
        Set(value As TColumnType)
            Throw New NotSupportedException("TypedColumn is ReadOnly")
        End Set
    End Property

    ''' <summary>
    ''' Return the value at the given index.
    ''' 
    ''' Will throw if the index is out of bounds.  Use <see cref="TryGetValue"/> for non-throwing gets.
    ''' </summary>
    ''' <param name="rowIndex">The index of the value to get, in the appropriate basis.</param>
    Default Public ReadOnly Property Item(rowIndex As Long) As TColumnType
        Get
            Dim category = Inner.Parent.Metadata.Columns(Inner.TranslatedColumnIndex).GetCategoryEnumMap(Of TColumnType)()

            Return Inner(rowIndex).UnsafeCast(Of TColumnType)(category)
        End Get
    End Property

    Friend Sub New(inner As Column)
        Me.Inner = inner
    End Sub

    ''' <summary>
    ''' Converts this column to an array of TColumnType.
    ''' 
    ''' Throws if the column cannot fit in an array.
    ''' </summary>
    Public Function ToArray() As TColumnType() Implements IColumn(Of TColumnType).ToArray
        Return Inner.ToArray(Of TColumnType)()
    End Function

    ''' <summary>
    ''' Converts a subset of this column to an array of TColumnType.
    ''' 
    ''' Throws if the subset cannot fit in an array.
    ''' </summary>
    Public Function GetRange(index As Long, length As Integer) As TColumnType() Implements IColumn(Of TColumnType).GetRange
        Return Inner.GetRange(Of TColumnType)(index, length)
    End Function

    ''' <summary>
    ''' Converts a subset of this column to an array of TColumnType.
    ''' 
    ''' The array is stored at index 0 in the passed array reference, which is initialized or resized if needed.
    ''' </summary>
    Public Sub ToArray(ByRef array As TColumnType()) Implements IColumn(Of TColumnType).ToArray
        Inner.ToArray(array)
    End Sub
    ''' <summary>
    ''' Converts this column to an array of <see cref="Value"/>.
    ''' 
    ''' Throws if the column cannot fit in an array.
    ''' </summary>
    Public Sub ToArray(ByRef array As Value()) Implements IColumn(Of TColumnType).ToArrayValue
        Inner.ToArray(array)
    End Sub

    ''' <summary>
    ''' Converts a subset of this column to an array of TColumnType.
    ''' 
    ''' The column subset starts at the given index (in the dataframe's basis) and is of the given length.
    ''' 
    ''' The array is stored at index 0 in the passed array reference, which is initialized or resized if needed.
    ''' 
    ''' Throws if the subset cannot fit in an array.
    ''' </summary>
    Public Sub GetRange(sourceIndex As Long, length As Integer, ByRef array As TColumnType()) Implements IColumn(Of TColumnType).GetRange
        Inner.GetRange(sourceIndex, length, array)
    End Sub
    ''' <summary>
    ''' Converts a subset of this column to an array of <see cref="Value"/>.
    ''' 
    ''' The column subset starts at the given index (in the dataframe's basis) and is of the given length.
    ''' 
    ''' The array is stored at index 0 in the passed array reference, which is initialized or resized if needed.
    ''' </summary>
    Public Sub GetRange(sourceIndex As Long, length As Integer, ByRef array As Value()) Implements IColumn(Of TColumnType).GetRangeValue
        Inner.GetRange(sourceIndex, length, array)
    End Sub

    ''' <summary>
    ''' Converts a subset of this column to an array of TColumnType.
    ''' 
    ''' The column subset starts at the given index (in the dataframe's basis) and is of the given length.
    ''' 
    ''' The array is stored at the given index in the passed array reference, which is initialized or resized if needed.
    ''' 
    ''' Throws if the subset cannot fit in an array.
    ''' </summary>
    Public Sub GetRange(sourceIndex As Long, length As Integer, ByRef array As TColumnType(), destinationIndex As Integer) Implements IColumn(Of TColumnType).GetRange
        Inner.GetRange(sourceIndex, length, array, destinationIndex)
    End Sub

    ''' <summary>
    ''' Converts a subset of this column to an array of <see cref="Value"/>.
    ''' 
    ''' The column subset starts at the given index (in the dataframe's basis) and is of the given length.
    ''' 
    ''' The array is stored at the given index in the passed array reference, which is initialized or resized if needed.
    ''' 
    ''' Throws if the column cannot be coerced to the given type, or the subset cannot fit in an array.
    ''' </summary>
    Public Sub GetRange(sourceIndex As Long, length As Integer, ByRef array As Value(), destinationIndex As Integer) Implements IColumn(Of TColumnType).GetRangeValue
        Inner.GetRange(sourceIndex, length, array, destinationIndex)
    End Sub

    ''' <summary>
    ''' <see cref="System.Collections.Generic.IEnumerable(Of T).GetEnumerator"/>
    ''' </summary>
    Public Function GetEnumerator() As TypedColumnEnumerator(Of TColumnType)
        Return New TypedColumnEnumerator(Of TColumnType)(Inner)
    End Function

    Private Function GetEnumerator1() As IEnumerator(Of TColumnType) Implements IEnumerable(Of TColumnType).GetEnumerator
        Return GetEnumerator()
    End Function

    Private Function GetEnumerator2() As IEnumerator Implements IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function

    ''' <summary>
    ''' Sets value to the value of the row at the passed index (in the dataframe's basis).
    ''' 
    ''' If the passed index is out of bounds false is returned.  Otherwise, true is returned;
    ''' </summary>
    Public Function TryGetValue(rowIndex As Long, <Out> ByRef value As TColumnType) As Boolean Implements IColumn(Of TColumnType).TryGetValue
        Dim innerValue As Value = Nothing
        If Not Inner.TryGetValue(rowIndex, innerValue) Then
            value = Nothing
            Return False
        End If

        Dim category = Inner.Parent.Metadata.Columns(Inner.TranslatedColumnIndex).GetCategoryEnumMap(Of TColumnType)()

        value = innerValue.UnsafeCast(Of TColumnType)(category)
        Return True
    End Function

    ''' <summary>
    ''' Converts this column to an array of the specified type.
    ''' 
    ''' Throws if the column cannot be coerced to the given type, or cannot fit in an array.
    ''' </summary>
    Public Sub ToArray(Of V)(ByRef array As V()) Implements IColumn(Of TColumnType).ToArray
        Inner.ToArray(array)
    End Sub

    ''' <summary>
    ''' <see cref="Column.GetRange(Of T)(Long,Integer)"/>
    ''' </summary>
    Public Sub GetRange(Of V)(rowSourceIndex As Long, length As Integer, ByRef array As V()) Implements IColumn(Of TColumnType).GetRange
        Inner.GetRange(rowSourceIndex, length, array)
    End Sub

    ''' <summary>
    ''' <see cref="Column.GetRange(Long,Integer,,Integer)"/>
    ''' </summary>
    Public Sub GetRange(Of V)(rowSourceIndex As Long, length As Integer, ByRef array As V(), destinationIndex As Integer) Implements IColumn(Of TColumnType).GetRange
        Inner.GetRange(rowSourceIndex, length, array, destinationIndex)
    End Sub

    ''' <summary>
    ''' <see cref="Column.TryGetValue(Of T)(Long,T)"/>
    ''' </summary>
    Public Function TryGetValue(Of V)(rowIndex As Long, <Out> ByRef value As V) As Boolean Implements IColumn(Of TColumnType).TryGetValue
        Return Inner.TryGetValue(rowIndex, value)
    End Function

    ''' <summary>
    ''' <see cref="Column.TryGetValue(Long,Value)"/>
    ''' </summary>
    Public Function TryGetValue(rowIndex As Long, <Out> ByRef value As Value) As Boolean Implements IColumn(Of TColumnType).TryGetValueCell
        Return Inner.TryGetValue(rowIndex, value)
    End Function

    ''' <summary>
    ''' <see cref="Object.Equals(Object)"/>
    ''' </summary>
    Public Overrides Function Equals(obj As Object) As Boolean
        If Not (TypeOf obj Is TypedColumn(Of TColumnType)) Then Return False

        Dim other = CType(obj, TypedColumn(Of TColumnType))
        Return other.Inner.Equals(Inner)
    End Function

    ''' <summary>
    ''' <see cref="Object.GetHashCode"/>
    ''' </summary>
    Public Overrides Function GetHashCode() As Integer
        Return Inner.GetHashCode() * 17 + GetType(TColumnType).GetHashCode()
    End Function
    ''' <summary>
    ''' <see cref="Object.ToString"/>
    ''' </summary>
    Public Overrides Function ToString() As String
        Return $"TypedColumn<{GetType(TColumnType).Name}> ""{Name}"" Index = {Index}"
    End Function

    ''' <summary>
    ''' Finds a given value in the column.  Throws if the index will not fit in an int.
    ''' </summary>
    Public Function IndexOf(item As TColumnType) As Integer Implements IList(Of TColumnType).IndexOf
        Dim ret = LongIndexOf(item)

        If ret > Integer.MaxValue Then Throw New InvalidOperationException($"Index {ret:N0} exceeded Int32.MaxValue")

        Return ret
    End Function

    ''' <summary>
    ''' Finds a given value in the column.
    ''' </summary>
    Public Function LongIndexOf(item As TColumnType) As Long
        Dim itemIsNull = False
        If Not GetType(TColumnType).IsValueType Then
            itemIsNull = item Is Nothing
        End If

        Dim ix As Long = 0
        For Each val As TColumnType In Me
            Dim valIsNull = False
            If Not GetType(TColumnType).IsValueType Then
                valIsNull = val Is Nothing
            End If

            If itemIsNull Then
                If valIsNull Then Return ix
            Else
                If item.Equals(val) Then Return ix
            End If
            ix += 1
        Next

        Return -1
    End Function

    ''' <summary>
    ''' Not supported.
    ''' </summary>
    Public Sub Insert(index As Integer, item As TColumnType) Implements IList(Of TColumnType).Insert
        Throw New NotSupportedException("Column is ReadOnly")
    End Sub

    ''' <summary>
    ''' Not supported.
    ''' </summary>
    Public Sub RemoveAt(index As Integer) Implements IList(Of TColumnType).RemoveAt
        Throw New NotSupportedException("Column is ReadOnly")
    End Sub

    ''' <summary>
    ''' Not supported.
    ''' </summary>
    Public Sub Add(item As TColumnType) Implements ICollection(Of TColumnType).Add
        Throw New NotSupportedException("Column is ReadOnly")
    End Sub

    ''' <summary>
    ''' Not supported.
    ''' </summary>
    Public Sub Clear() Implements ICollection(Of TColumnType).Clear
        Throw New NotSupportedException("Column is ReadOnly")
    End Sub

    ''' <summary>
    ''' Not supported.
    ''' </summary>
    Public Function Remove(item As TColumnType) As Boolean Implements ICollection(Of TColumnType).Remove
        Throw New NotSupportedException("Column is ReadOnly")
    End Function

    ''' <summary>
    ''' <see cref="System.Collections.Generic.ICollection(Of T).Contains(T)"/>
    ''' </summary>
    Public Function Contains(item As TColumnType) As Boolean Implements ICollection(Of TColumnType).Contains
        Return LongIndexOf(item) <> -1
    End Function

    ''' <summary>
    ''' <see cref="System.Collections.Generic.ICollection(Of T).CopyTo"/>
    ''' </summary>
    Public Sub CopyTo(array As TColumnType(), arrayIndex As Integer) Implements ICollection(Of TColumnType).CopyTo
        If array Is Nothing Then Throw New ArgumentNullException(NameOf(array))
        If arrayIndex < 0 Then Throw New ArgumentOutOfRangeException(NameOf(arrayIndex))
        If Length > Integer.MaxValue Then Throw New InvalidOperationException($"Column is too large {Length:N0} to copy into an array")
        If array.Length < Length Then Throw New ArgumentException($"Column (size: {Length:N0}) cannot fit in array (size: {array.Length:N0})")

        GetRange(Inner.Parent.UntranslateIndex(0), Length, array, arrayIndex)
    End Sub

    ''' <summary>
    ''' Equivalent to <see cref="ToArray()"/>
    ''' </summary>
    Public Shared Narrowing Operator CType(col As TypedColumn(Of TColumnType)) As Value()
        Dim ret As Value() = Nothing
        col.ToArray(ret)
        Return ret
    End Operator

    ''' <summary>
    ''' Equivalent to <see cref="ToArray()"/>
    ''' </summary>
    Public Shared Narrowing Operator CType(col As TypedColumn(Of TColumnType)) As TColumnType()
        Return col.ToArray()
    End Operator
End Class

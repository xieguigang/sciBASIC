﻿#Region "Microsoft.VisualBasic::fb3cfbe14b98aa6b8698609d48d2da28, Data\BinaryData\Feather\Column.vb"

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

    '   Total Lines: 528
    '    Code Lines: 283 (53.60%)
    ' Comment Lines: 174 (32.95%)
    '    - Xml Docs: 85.63%
    ' 
    '   Blank Lines: 71 (13.45%)
    '     File Size: 20.74 KB


    ' Class ColumnValueEnumerator
    ' 
    '     Properties: Current, CurrentProp
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: MoveNext
    ' 
    '     Sub: Dispose, Reset
    ' 
    ' Class Column
    ' 
    '     Properties: Count, Index, IsReadOnly, Length, Name
    '                 OnDiskType, Type
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: CanBeBlitted, Cast, Contains, Equals, GetEnumerator
    '               GetEnumerator1, GetEnumerator2, GetHashCode, (+2 Overloads) GetRange, IndexOf
    '               LongIndexOf, Remove, (+2 Overloads) ToArray, ToString, (+2 Overloads) TryGetValue
    '               TryGetValueTranslated
    ' 
    '     Sub: Add, Clear, CopyTo, (+4 Overloads) GetRange, Insert
    '          RemoveAt, (+2 Overloads) ToArray
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.DataStorage.FeatherFormat.Impl

''' <summary>
''' Allocation free enumerator for a column.
''' </summary>
Public Class ColumnValueEnumerator
    Implements IEnumerator(Of Value)

    ''' <summary>
    ''' <see cref="System.Collections.Generic.IEnumerator(Of T)"/>
    ''' </summary>
    Private _CurrentProp As Value
    Friend Parent As DataFrame
    Friend TranslatedColumnIndex As Long
    Friend Index As Long

    Public Property CurrentProp As Value Implements IEnumerator(Of Value).Current
        Get
            Return _CurrentProp
        End Get
        Private Set(value As Value)
            _CurrentProp = value
        End Set
    End Property

    Private ReadOnly Property Current As Object Implements IEnumerator.Current
        Get
            Return CurrentProp
        End Get
    End Property

    Friend Sub New(parent As DataFrame, translatedColumnIndex As Long)
        CurrentProp = Nothing
        Me.Parent = parent
        Me.TranslatedColumnIndex = translatedColumnIndex
        Index = -1
    End Sub

    ''' <summary>
    ''' <see cref="System.Collections.Generic.IEnumerator(Of T)"/>
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        Parent = Nothing
    End Sub

    ''' <summary>
    ''' <see cref="System.Collections.Generic.IEnumerator(Of T)"/>
    ''' </summary>
    Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
        Index += 1

        Dim value As Value = Nothing
        If Not Parent.TryGetValueTranslated(Index, TranslatedColumnIndex, value) Then Return False

        CurrentProp = value
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
''' Represents a column of a DataFrame.
''' 
''' Is untyped, but returned values can be implicitly coerced to built-in types.
''' </summary>
Public Class Column
    Implements IEnumerable(Of Value), IColumn(Of Value)
    Friend Parent As DataFrame
    Friend TranslatedColumnIndex As Long

    ''' <summary>
    ''' Returns the Index (in the appropriate basis) of this column in the original dataframe.
    ''' </summary>
    Public ReadOnly Property Index As Long Implements IColumn(Of Value).Index
        Get
            Return Parent.UntranslateIndex(TranslatedColumnIndex)
        End Get
    End Property
    ''' <summary>
    ''' Returns the name of this column.
    ''' </summary>
    Public ReadOnly Property Name As String Implements IColumn(Of Value).Name
        Get
            Return Parent.Metadata.Columns(TranslatedColumnIndex).Name
        End Get
    End Property
    ''' <summary>
    ''' Returns the .NET equivalent type of this column.
    ''' </summary>
    Public ReadOnly Property Type As Type Implements IColumn(Of Value).Type
        Get
            Return Parent.Metadata.Columns(TranslatedColumnIndex).MappedType
        End Get
    End Property
    ''' <summary>
    ''' Returns the number of entries in the this column.
    ''' This will always match the number of rows in the original dataframe.
    ''' </summary>
    Public ReadOnly Property Length As Long Implements IColumn(Of Value).Length
        Get
            Return Parent.RowCount
        End Get
    End Property

    ''' <summary>
    ''' Return the value at the given index.
    ''' 
    ''' Will throw if the index is out of bounds.  Use <see cref="TryGetValue"/> for non-throwing gets.
    ''' </summary>
    Default Public ReadOnly Property Item(rowIndex As Long) As Value
        Get
            Dim translatedRowIndex = Parent.TranslateIndex(rowIndex)
            Dim value As Value = Nothing
            If Not Parent.TryGetValueTranslated(translatedRowIndex, TranslatedColumnIndex, value) Then
                Dim minLegal As Long
                Dim maxLegal As Long
                Select Case Parent.Basis
                    Case BasisType.One
                        minLegal = 1
                        maxLegal = Parent.Metadata.NumRows
                    Case BasisType.Zero
                        minLegal = 0
                        maxLegal = Parent.Metadata.NumRows - 1
                    Case Else
                        Throw New InvalidOperationException($"Unexpected Basis: {Parent.Basis}")
                End Select

                Throw New ArgumentOutOfRangeException($"Row index out of range, valid between [{minLegal}, {maxLegal}] found {rowIndex}")
            End If

            Return value
        End Get
    End Property

    Friend ReadOnly Property OnDiskType As ColumnType
        Get
            Return Parent.Metadata.Columns(TranslatedColumnIndex).Type
        End Get
    End Property

    ''' <summary>
    ''' <see cref="System.Collections.Generic.IList(Of T)"/>
    ''' 
    ''' Throws if <see cref="Length"/> will not fit in an int.
    ''' </summary>
    Public ReadOnly Property Count As Integer Implements ICollection(Of Value).Count
        Get
            Return Length
        End Get
    End Property
    ''' <summary>
    ''' <see cref="System.Collections.Generic.IList(Of T)"/>
    ''' 
    ''' Always return true.
    ''' </summary>
    Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of Value).IsReadOnly
        Get
            Return True
        End Get
    End Property

    ''' <summary>
    ''' &lt;see cref="this[long]"/&gt;
    ''' <see cref="System.Collections.Generic.IList(Of T)"/>
    ''' </summary>
    Default Public Property Item(index As Integer) As Value Implements IList(Of Value).Item
        Get
            Return Me(CLng(index))
        End Get
        Set(value As Value)
            Throw New NotSupportedException("Column is ReadOnly")
        End Set
    End Property

    Friend Sub New(parent As DataFrame, translatedColumnIndex As Long)
        Me.Parent = parent
        Me.TranslatedColumnIndex = translatedColumnIndex
    End Sub

    Friend Sub New()
    End Sub

    ''' <summary>
    ''' Converts this column to an array of the specified type.
    ''' 
    ''' Throws if the column cannot be coerced to the given type, or cannot fit in an array.
    ''' </summary>
    Public Function ToArray(Of T)() As T()
        If Length > Integer.MaxValue Then Throw New InvalidOperationException($"Length ({Length:N0}) greater that int.MaxValue, can't possibly fit in a single array")

        Dim ret As T() = Nothing
        GetRange(Parent.UntranslateIndex(0), Length, ret)
        Return ret
    End Function

    ''' <summary>
    ''' Converts this column to an array of <see cref="Value"/>.
    ''' 
    ''' Throws if the column cannot fit in an array.
    ''' </summary>
    Public Function ToArray() As Value() Implements IColumn(Of Value).ToArray
        Return ToArray(Of Value)()
    End Function

    ''' <summary>
    ''' Converts a subset of this column to an array of the specified type.
    ''' 
    ''' Throws if the column cannot be coerced to the given type, or the subset cannot fit in an array.
    ''' </summary>
    Public Function GetRange(Of T)(rowIndex As Long, length As Integer) As T()
        Dim ret As T() = Nothing
        GetRange(rowIndex, length, ret)
        Return ret
    End Function

    ''' <summary>
    ''' Converts a subset of this column to an array of <see cref="Value"/>.
    ''' 
    ''' Throws if the subset cannot fit in an array.
    ''' </summary>
    Public Function GetRange(rowIndex As Long, length As Integer) As Value() Implements IColumn(Of Value).GetRange
        Return GetRange(Of Value)(rowIndex, length)
    End Function

    ''' <summary>
    ''' Converts a subset of this column to an array of the specified type.
    ''' 
    ''' The array is stored at index 0 in the passed array reference, which is initialized or resized if needed.
    ''' </summary>
    Public Sub ToArray(Of T)(ByRef array As T()) Implements IColumn(Of Value).ToArray
        If Length > Integer.MaxValue Then Throw New InvalidOperationException($"Length ({Length:N0}) greater that int.MaxValue, can't possibly fit in a single array")

        GetRange(Parent.UntranslateIndex(0), Length, array)
    End Sub

    ''' <summary>
    ''' Converts this column to an array of <see cref="Value"/>.
    ''' 
    ''' The array is stored at index 0 in the passed array reference, which is initialized or resized if needed.
    ''' </summary>
    Public Sub ToArray(ByRef array As Value()) Implements IColumn(Of Value).ToArray, IColumn(Of Value).ToArrayValue
        ToArray(Of Value)(array)
    End Sub

    ''' <summary>
    ''' Converts a subset of this column to an array of the specified type.
    ''' 
    ''' The column subset starts at the given index (in the dataframe's basis) and is of the given length.
    ''' 
    ''' The array is stored at index 0 in the passed array reference, which is initialized or resized if needed.
    ''' 
    ''' Throws if the column cannot be coerced to the given type, or the subset cannot fit in an array.
    ''' </summary>
    Public Sub GetRange(Of T)(rowSourceIndex As Long, length As Integer, ByRef array As T()) Implements IColumn(Of Value).GetRange
        GetRange(rowSourceIndex, length, array, 0)
    End Sub

    ''' <summary>
    ''' Converts a subset of this column to an array of <see cref="Value"/>.
    ''' 
    ''' The column subset starts at the given index (in the dataframe's basis) and is of the given length.
    ''' 
    ''' The array is stored at index 0 in the passed array reference, which is initialized or resized if needed.
    ''' </summary>
    Public Sub GetRange(rowSourceIndex As Long, length As Integer, ByRef array As Value()) Implements IColumn(Of Value).GetRangeValue, IColumn(Of Value).GetRange
        GetRange(Of Value)(rowSourceIndex, length, array)
    End Sub

    ''' <summary>
    ''' Converts a subset of this column to an array of the specified type.
    ''' 
    ''' The column subset starts at the given index (in the dataframe's basis) and is of the given length.
    ''' 
    ''' The array is stored at the given index in the passed array reference, which is initialized or resized if needed.
    ''' 
    ''' Throws if the column cannot be coerced to the given type, or the subset cannot fit in an array.
    ''' </summary>
    Public Sub GetRange(Of T)(rowSourceIndex As Long, length As Integer, ByRef array As T(), destinationIndex As Integer) Implements IColumn(Of Value).GetRange
        If Not OnDiskType.CanMapTo(GetType(T), Parent.Metadata.Columns(TranslatedColumnIndex).CategoryLevels) Then
            Throw New InvalidOperationException($"Cannot convert {Type.Name} to {GetType(T).Name}")
        End If

        Dim translatedIndex = Parent.TranslateIndex(rowSourceIndex)

        If translatedIndex < 0 OrElse translatedIndex >= Me.Length Then Throw New ArgumentOutOfRangeException(NameOf(rowSourceIndex))

        If length < 0 Then Throw New ArgumentOutOfRangeException(NameOf(length))

        Dim lastElem = translatedIndex + length
        If lastElem > Me.Length Then Throw New ArgumentOutOfRangeException(NameOf(length))

        If destinationIndex < 0 Then Throw New ArgumentOutOfRangeException(NameOf(destinationIndex))

        Dim arraySize = destinationIndex + length

        If array Is Nothing Then
            array = New T(arraySize - 1) {}
        Else
            If array.Length < arraySize Then
                System.Array.Resize(array, arraySize)
            End If
        End If

        If CanBeBlitted(Of T)() Then
            Parent.UnsafeFastGetRowRange(translatedIndex, TranslatedColumnIndex, array, destinationIndex, length)
            Return
        End If

        Dim category = Parent.Metadata.Columns(TranslatedColumnIndex).GetCategoryEnumMap(Of T)()

        For i = 0 To length - 1
            Dim value = Me(rowSourceIndex + i)
            array(destinationIndex + i) = value.UnsafeCast(Of T)(category)
        Next
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
    Public Sub GetRange(rowSourceIndex As Long, length As Integer, ByRef array As Value(), destinationIndex As Integer) Implements IColumn(Of Value).GetRangeValue, IColumn(Of Value).GetRange
        GetRange(Of Value)(rowSourceIndex, length, array, destinationIndex)
    End Sub

    ''' <summary>
    ''' Sets value to the value of the row at the passed index (in the dataframe's basis), having coerced it to the appropriate type if possible.
    ''' 
    ''' If the passed index is out of bounds, or the coercing fails, false is returned.  Otherwise, true is returned;
    ''' </summary>
    Public Function TryGetValue(Of T)(rowIndex As Long, <Out> ByRef value As T) As Boolean Implements IColumn(Of Value).TryGetValue
        Dim rawValue As Value = Nothing
        If Not TryGetValue(rowIndex, rawValue) Then
            value = Nothing
            Return False
        End If

        Dim columnDetails = Parent.Metadata.Columns(TranslatedColumnIndex)
        If Not columnDetails.CanMapTo(GetType(T)) Then
            value = Nothing
            Return False
        End If

        Dim category = columnDetails.GetCategoryEnumMap(Of T)()
        value = rawValue.UnsafeCast(Of T)(category)
        Return True
    End Function

    ''' <summary>
    ''' Sets value to the <see cref="Value"/> of the row at the passed index (in the dataframe's basis).
    ''' 
    ''' If the passed index is out of bounds false is returned.  Otherwise, true is returned;
    ''' </summary>
    Public Function TryGetValue(rowIndex As Long, <Out> ByRef value As Value) As Boolean Implements IColumn(Of Value).TryGetValueCell, IColumn(Of Value).TryGetValue
        Dim translatedRowIndex = Parent.TranslateIndex(rowIndex)

        Return TryGetValueTranslated(translatedRowIndex, value)
    End Function

    ''' <summary>
    ''' <see cref="System.Collections.Generic.IEnumerable(Of T).GetEnumerator"/>
    ''' </summary>
    Public Function GetEnumerator() As ColumnValueEnumerator
        Return New ColumnValueEnumerator(Parent, TranslatedColumnIndex)
    End Function

    Private Function GetEnumerator1() As IEnumerator(Of Value) Implements IEnumerable(Of Value).GetEnumerator
        Return GetEnumerator()
    End Function
    Private Function GetEnumerator2() As IEnumerator Implements IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function

    ''' <summary>
    ''' <see cref="Object.Equals(Object)"/>
    ''' </summary>
    Public Overrides Function Equals(obj As Object) As Boolean
        If Not (TypeOf obj Is Column) Then Return False

        Dim other = CType(obj, Column)
        Return ReferenceEquals(other.Parent, Parent) AndAlso other.TranslatedColumnIndex = TranslatedColumnIndex
    End Function

    ''' <summary>
    ''' <see cref="Object.GetHashCode"/>
    ''' </summary>
    Public Overrides Function GetHashCode() As Integer
        Return Parent.GetHashCode() * 17 + TranslatedColumnIndex.GetHashCode()
    End Function
    ''' <summary>
    ''' <see cref="Object.ToString"/>
    ''' </summary>
    Public Overrides Function ToString() As String
        Return $"Column ""{Name}"" Index = {Index}"
    End Function

    ''' <summary>
    ''' Converts the column to a typed column of the given type.
    ''' 
    ''' Throws if the conversion isn't allowed.
    ''' </summary>
    Public Function Cast(Of T)() As TypedColumn(Of T)
        If Not OnDiskType.CanMapTo(GetType(T), Parent.Metadata.Columns(TranslatedColumnIndex).CategoryLevels) Then
            Throw New InvalidCastException($"Cannot convert {Type.Name} to {GetType(T).Name}")
        End If

        Dim typedColumn = New TypedColumn(Of T)(Me)
        Return typedColumn
    End Function

    ''' <summary>
    ''' Finds a given value in the column.  Throws if the index will not fit in an int.
    ''' </summary>
    Public Function IndexOf(item As Value) As Integer Implements IList(Of Value).IndexOf
        Dim ret = LongIndexOf(item)

        If ret > Integer.MaxValue Then
            Throw New InvalidOperationException($"Index {ret:N0} exceeded Int32.MaxValue")
        End If

        Return ret
    End Function

    ''' <summary>
    ''' Finds a given value in the column.
    ''' </summary>
    Public Function LongIndexOf(item As Value) As Long
        Dim ix As Long = 0
        For Each val As Value In Me
            If item.Equals(val) Then Return ix
            ix += 1
        Next

        Return -1
    End Function

    ''' <summary>
    ''' Not supported.
    ''' </summary>
    Public Sub Insert(index As Integer, item As Value) Implements IList(Of Value).Insert
        Throw New NotSupportedException("Column is ReadOnly")
    End Sub

    ''' <summary>
    ''' Not supported.
    ''' </summary>
    Public Sub RemoveAt(index As Integer) Implements IList(Of Value).RemoveAt
        Throw New NotSupportedException("Column is ReadOnly")
    End Sub

    ''' <summary>
    ''' Not supported.
    ''' </summary>
    Public Sub Add(item As Value) Implements ICollection(Of Value).Add
        Throw New NotSupportedException("Column is ReadOnly")
    End Sub

    ''' <summary>
    ''' Not supported.
    ''' </summary>
    Public Sub Clear() Implements ICollection(Of Value).Clear
        Throw New NotSupportedException("Column is ReadOnly")
    End Sub

    ''' <summary>
    ''' <see cref="System.Collections.Generic.ICollection(Of T).Contains(T)"/>
    ''' </summary>
    Public Function Contains(item As Value) As Boolean Implements ICollection(Of Value).Contains
        Return LongIndexOf(item) <> -1
    End Function

    ''' <summary>
    ''' <see cref="System.Collections.Generic.ICollection(Of T).CopyTo"/>
    ''' </summary>
    Public Sub CopyTo(array As Value(), arrayIndex As Integer) Implements ICollection(Of Value).CopyTo
        If array Is Nothing Then Throw New ArgumentNullException(NameOf(array))
        If arrayIndex < 0 Then Throw New ArgumentOutOfRangeException(NameOf(arrayIndex))
        If Length > Integer.MaxValue Then Throw New InvalidOperationException($"Column is too large {Length:N0} to copy into an array")
        If array.Length < Length Then Throw New ArgumentException($"Column (size: {Length:N0}) cannot fit in array (size: {array.Length:N0})")

        GetRange(Parent.UntranslateIndex(0), Length, array, arrayIndex)
    End Sub

    ''' <summary>
    ''' Not supported.
    ''' </summary>
    Public Function Remove(item As Value) As Boolean Implements ICollection(Of Value).Remove
        Throw New NotSupportedException("Column is ReadOnly")
    End Function

    ''' <summary>
    ''' Equivalent to <see cref="ToArray()"/>
    ''' </summary>
    Public Shared Narrowing Operator CType(col As Column) As Value()
        Return col.ToArray()
    End Operator

    Friend Function TryGetValueTranslated(translatedRowIndex As Long, <Out> ByRef value As Value) As Boolean
        Return Parent.TryGetValueTranslated(translatedRowIndex, TranslatedColumnIndex, value)
    End Function

    ' this is generic because, in theory, the JIT can turn this into a no-op
    Private Shared Function CanBeBlitted(Of T)() As Boolean
        If GetType(T) Is GetType(Byte) Then Return True
        If GetType(T) Is GetType(SByte) Then Return True
        If GetType(T) Is GetType(Short) Then Return True
        If GetType(T) Is GetType(UShort) Then Return True
        If GetType(T) Is GetType(Integer) Then Return True
        If GetType(T) Is GetType(UInteger) Then Return True
        If GetType(T) Is GetType(Long) Then Return True
        If GetType(T) Is GetType(ULong) Then Return True
        If GetType(T) Is GetType(Single) Then Return True
        If GetType(T) Is GetType(Double) Then Return True

        Return False
    End Function
End Class

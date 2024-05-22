#Region "Microsoft.VisualBasic::086dbe62950a074e8024d00302544e60, Data\BinaryData\Feather\Row.vb"

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

    '   Total Lines: 502
    '    Code Lines: 287 (57.17%)
    ' Comment Lines: 127 (25.30%)
    '    - Xml Docs: 88.19%
    ' 
    '   Blank Lines: 88 (17.53%)
    '     File Size: 24.83 KB


    ' Class RowValueEnumerator
    ' 
    '     Properties: Current, CurrentProp
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: MoveNext
    ' 
    '     Sub: Dispose, Reset
    ' 
    ' Class Row
    ' 
    '     Properties: Index, Length
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Equals, GetEnumerator, GetEnumerator1, GetEnumerator2, GetHashCode
    '               GetRange, (+8 Overloads) Map, ToArray, ToString, (+4 Overloads) TryGetValue
    '               UnsafeGetTranslated
    ' 
    '     Sub: (+2 Overloads) GetRange, ToArray
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.DataStorage.FeatherFormat.Impl

''' <summary>
''' Allocation free enumerator for a row.
''' </summary>
Public Class RowValueEnumerator
    Implements IEnumerator(Of Value)

    ''' <summary>
    ''' <see cref="System.Collections.Generic.IEnumerator(Of T)"/>
    ''' </summary>
    Private _CurrentProp As Value
    Friend Parent As DataFrame
    Friend TranslatedRowIndex As Long
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

    Friend Sub New(parent As DataFrame, translatedRowIndex As Long)
        CurrentProp = Nothing
        Me.Parent = parent
        Me.TranslatedRowIndex = translatedRowIndex
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
        If Not Parent.TryGetValueTranslated(TranslatedRowIndex, Index, value) Then Return False

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
''' Represents a row of a DataFrame.
''' 
''' Is untyped, but returned values can be implicitly coerced to built-in types.
''' </summary>
Public Class Row
    Implements IEnumerable(Of Value), IRow
    Friend Parent As DataFrame
    Friend TranslatedRowIndex As Long

    ''' <summary>
    ''' Returns the Index (in the appropriate basis) of this row in the original dataframe.
    ''' </summary>
    Public ReadOnly Property Index As Long Implements IRow.Index
        Get
            Return Parent.UntranslateIndex(TranslatedRowIndex)
        End Get
    End Property
    ''' <summary>
    ''' Returns the number of entries in the this column.
    ''' This will always match the number of columns in the original dataframe.
    ''' </summary>
    Public ReadOnly Property Length As Long Implements IRow.Length
        Get
            Return Parent.ColumnCount
        End Get
    End Property

    ''' <summary>
    ''' Return the value at the given index.
    ''' 
    ''' Will throw if the index is out of bounds.  Use <see cref="TryGetValue"/> for non-throwing gets.
    ''' </summary>
    Default Public ReadOnly Property Item(columnIndex As Long) As Value
        Get
            Dim translatedColumnIndex = Parent.TranslateIndex(columnIndex)

            Dim value As Value = Nothing
            If Not Parent.TryGetValueTranslated(TranslatedRowIndex, translatedColumnIndex, value) Then
                Dim rowIndex = Parent.UntranslateIndex(TranslatedRowIndex)

                Dim minRowIx, minColIx, maxRowIx, maxColIx As Long
                Select Case Parent.Basis
                    Case BasisType.One
                        minRowIx = 1
                        maxRowIx = Parent.Metadata.NumRows
                        minColIx = 1
                        maxColIx = Parent.Metadata.Columns.Length
                    Case BasisType.Zero
                        minRowIx = 0
                        maxRowIx = Parent.Metadata.NumRows - 1
                        minColIx = 0
                        maxColIx = Parent.Metadata.Columns.Length - 1
                    Case Else
                        Throw New InvalidOperationException($"Unexpected Basis: {Parent.Basis}")
                End Select

                Throw New ArgumentOutOfRangeException($"Address out of range, legal range is [{minRowIx}, {minColIx}] - [{maxRowIx}, {maxColIx}], found [{rowIndex}, {columnIndex}]")
            End If

            Return value
        End Get
    End Property

    ''' <summary>
    ''' Return the value in the column with the given name.
    ''' 
    ''' Will throw if no column with that name exists .  Use <see cref="TryGetValue"/> for non-throwing gets.
    ''' </summary>
    Default Public ReadOnly Property Item(columnName As String) As Value
        Get
            Dim translatedColumnIndex As Long
            If Not Parent.TryLookupTranslatedColumnIndex(columnName, translatedColumnIndex) Then
                Throw New KeyNotFoundException($"Could not find column with name ""{columnName}""")
            End If

            Dim value As Value = Nothing
            If Not Parent.TryGetValueTranslated(TranslatedRowIndex, translatedColumnIndex, value) Then
                Dim rowIndex = Parent.UntranslateIndex(TranslatedRowIndex)
                Dim columnIndex = Parent.UntranslateIndex(translatedColumnIndex)

                Dim minRowIx, minColIx, maxRowIx, maxColIx As Long
                Select Case Parent.Basis
                    Case BasisType.One
                        minRowIx = 1
                        maxRowIx = Parent.Metadata.NumRows
                        minColIx = 1
                        maxColIx = Parent.Metadata.Columns.Length
                    Case BasisType.Zero
                        minRowIx = 0
                        maxRowIx = Parent.Metadata.NumRows - 1
                        minColIx = 0
                        maxColIx = Parent.Metadata.Columns.Length - 1
                    Case Else
                        Throw New InvalidOperationException($"Unexpected Basis: {Parent.Basis}")
                End Select

                Throw New ArgumentOutOfRangeException($"Address out of range, legal range is [{minRowIx}, {minColIx}] - [{maxRowIx}, {maxColIx}], found [{rowIndex}, {columnIndex}]")
            End If

            Return value
        End Get
    End Property

    Friend Sub New(parent As DataFrame, translatedRowIndex As Long)
        Me.Parent = parent
        Me.TranslatedRowIndex = translatedRowIndex
    End Sub

    ''' <summary>
    ''' <see cref="System.Collections.Generic.IEnumerable(Of T).GetEnumerator"/>
    ''' </summary>
    Public Function GetEnumerator() As RowValueEnumerator
        Return New RowValueEnumerator(Parent, TranslatedRowIndex)
    End Function

    Private Function GetEnumerator1() As IEnumerator(Of Value) Implements IEnumerable(Of Value).GetEnumerator
        Return GetEnumerator()
    End Function
    Private Function GetEnumerator2() As IEnumerator Implements IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function

    ''' <summary>
    ''' Sets value to the <see cref="Value"/> of the column at the passed index (in the dataframe's basis).
    ''' 
    ''' If the passed index is out of bounds false is returned.  Otherwise, true is returned;
    ''' </summary>
    Public Function TryGetValue(columnIndex As Long, <Out> ByRef value As Value) As Boolean Implements IRow.TryGetValue
        Dim translatedColumnIndex = Parent.TranslateIndex(columnIndex)
        Return Parent.TryGetValueTranslated(TranslatedRowIndex, translatedColumnIndex, value)
    End Function

    ''' <summary>
    ''' Sets value to the value of the column at the passed index (in the dataframe's basis), having coerced it to the appropriate type if possible.
    ''' 
    ''' If the passed index is out of bounds, or the coercing fails, false is returned.  Otherwise, true is returned;
    ''' </summary>
    Public Function TryGetValue(Of T)(columnIndex As Long, <Out> ByRef value As T) As Boolean Implements IRow.TryGetValue
        Dim rawValue As Value
        If Not TryGetValue(columnIndex, rawValue) Then
            value = Nothing
            Return False
        End If

        Dim columnSpec = Parent.Metadata.Columns(rawValue.TranslatedColumnIndex)

        If Not columnSpec.CanMapTo(GetType(T)) Then
            value = Nothing
            Return False
        End If

        value = rawValue.UnsafeCast(Of T)(columnSpec.GetCategoryEnumMap(Of T)())
        Return True
    End Function

    ''' <summary>
    ''' Sets value to the <see cref="Value"/> of the column with the passed name.
    ''' 
    ''' If the passed index is out of bounds false is returned.  Otherwise, true is returned;
    ''' </summary>
    Public Function TryGetValue(columnName As String, <Out> ByRef value As Value) As Boolean Implements IRow.TryGetValue
        Dim translatedColumnIndex As Long
        If Not Parent.TryLookupTranslatedColumnIndex(columnName, translatedColumnIndex) Then
            value = Nothing
            Return False
        End If

        Return Parent.TryGetValueTranslated(TranslatedRowIndex, translatedColumnIndex, value)
    End Function

    ''' <summary>
    ''' Sets value to the value of the column with the given name, having coerced it to the appropriate type if possible.
    ''' 
    ''' If the passed index is out of bounds, or the coercing fails, false is returned.  Otherwise, true is returned;
    ''' </summary>
    Public Function TryGetValue(Of T)(columnName As String, <Out> ByRef value As T) As Boolean Implements IRow.TryGetValue
        Dim rawValue As Value
        If Not TryGetValue(columnName, rawValue) Then
            value = Nothing
            Return False
        End If

        Dim columnSpec = Parent.Metadata.Columns(rawValue.TranslatedColumnIndex)

        If Not columnSpec.CanMapTo(GetType(T)) Then
            value = Nothing
            Return False
        End If

        value = rawValue.UnsafeCast(Of T)(columnSpec.GetCategoryEnumMap(Of T)())
        Return True
    End Function

    ''' <summary>
    ''' Converts this column to an array of <see cref="Value"/>.
    ''' 
    ''' Throws if the row cannot fit in an array.
    ''' </summary>
    Public Function ToArray() As Value() Implements IRow.ToArray
        Dim ret As Value() = Nothing
        ToArray(ret)
        Return ret
    End Function

    ''' <summary>
    ''' Converts this row to an array of <see cref="Value"/>.
    ''' 
    ''' The array is stored at index 0 in the passed array reference, which is initialized or resized if needed.
    ''' </summary>
    Public Sub ToArray(ByRef array As Value()) Implements IRow.ToArray
        GetRange(Parent.UntranslateIndex(0), Length, array)
    End Sub

    ''' <summary>
    ''' Converts a subset of this row to an array of <see cref="Value"/>.
    ''' 
    ''' Throws if the subset cannot fit in an array.
    ''' </summary>
    Public Function GetRange(columnIndex As Long, length As Integer) As Value() Implements IRow.GetRange
        Dim ret As Value() = Nothing
        GetRange(columnIndex, length, ret)
        Return ret
    End Function

    ''' <summary>
    ''' Converts a subset of this row to an array of <see cref="Value"/>.
    ''' 
    ''' The subset starts at the given index (in the dataframe's basis) and is of the given length.
    ''' 
    ''' The array is stored at index 0 in the passed array reference, which is initialized or resized if needed.
    ''' </summary>
    Public Sub GetRange(columnSourceIndex As Long, length As Integer, ByRef array As Value()) Implements IRow.GetRange
        GetRange(columnSourceIndex, length, array, 0)
    End Sub

    ''' <summary>
    ''' Converts a subset of this row to an array of <see cref="Value"/>.
    ''' 
    ''' The subset starts at the given index (in the dataframe's basis) and is of the given length.
    ''' 
    ''' The array is stored at the given index in the passed array reference, which is initialized or resized if needed.
    ''' 
    ''' Throws if subset cannot fit in an array.
    ''' </summary>
    Public Sub GetRange(columnSourceIndex As Long, length As Integer, ByRef array As Value(), destinationIndex As Integer) Implements IRow.GetRange
        Dim translatedColumnIndex = Parent.TranslateIndex(columnSourceIndex)

        If translatedColumnIndex < 0 OrElse translatedColumnIndex >= Me.Length Then Throw New ArgumentOutOfRangeException(NameOf(columnSourceIndex))

        If length < 0 Then Throw New ArgumentOutOfRangeException(NameOf(length))

        Dim lastElem = translatedColumnIndex + length
        If lastElem > Me.Length Then Throw New ArgumentOutOfRangeException(NameOf(length))

        If destinationIndex < 0 Then Throw New ArgumentOutOfRangeException(NameOf(destinationIndex))

        Dim arraySize = destinationIndex + length

        If array Is Nothing Then
            array = New Value(arraySize - 1) {}
        Else
            If array.Length < arraySize Then
                System.Array.Resize(array, arraySize)
            End If
        End If

        For i = 0 To length - 1
            Dim value = Me(columnSourceIndex + i)
            array(destinationIndex + i) = value
        Next
    End Sub

    ''' <summary>
    ''' <see cref="Object.Equals(Object)"/>
    ''' </summary>
    Public Overrides Function Equals(obj As Object) As Boolean
        If Not (TypeOf obj Is Row) Then Return False

        Dim other = CType(obj, Row)
        Return ReferenceEquals(other.Parent, Parent) AndAlso other.TranslatedRowIndex = TranslatedRowIndex
    End Function

    ''' <summary>
    ''' <see cref="Object.GetHashCode"/>
    ''' </summary>
    Public Overrides Function GetHashCode() As Integer
        Return Parent.GetHashCode() * 17 + TranslatedRowIndex.GetHashCode()
    End Function

    ''' <summary>
    ''' <see cref="Object.ToString"/>
    ''' </summary>
    Public Overrides Function ToString() As String
        Return $"Row Index = {Parent.UntranslateIndex(TranslatedRowIndex)}"
    End Function

    ''' <summary>
    ''' Maps this row to a <see cref="TypedRow(Of TCol1)"/>.
    ''' </summary>
    Public Function Map(Of TCol1)() As TypedRow(Of TCol1)
        If Parent.ColumnCount < 1 Then Throw New ArgumentException($"Cannot map row, mapping has 1 column while row has {Parent.ColumnCount:N0} columns")

        Dim badMap = Not Parent.Metadata.Columns(0).CanMapTo(GetType(TCol1))

        If badMap Then
            Throw New ArgumentException($"Cannot map dataframe given mapping: {GetType(TCol1).Name} = {Parent.Metadata.Columns(0).MappedType.Name}")
        End If

        Return New TypedRow(Of TCol1)(Me)
    End Function

    ''' <summary>
    ''' Maps this row to a <see cref="TypedRowType(Of TCol1,TCol2)"/>.
    ''' </summary>
    Public Function Map(Of TCol1, TCol2)() As TypedRowType(Of TCol1, TCol2)
        If Parent.ColumnCount < 2 Then Throw New ArgumentException($"Cannot map row, mapping has 2 columns while row has {Parent.ColumnCount:N0} columns")

        Dim badMap = Not Parent.Metadata.Columns(0).CanMapTo(GetType(TCol1)) OrElse Not Parent.Metadata.Columns(1).CanMapTo(GetType(TCol2))

        If badMap Then
            Throw New ArgumentException($"Cannot map dataframe given mapping: {GetType(TCol1).Name} = {Parent.Metadata.Columns(0).MappedType.Name}, {GetType(TCol2).Name} = {Parent.Metadata.Columns(1).MappedType.Name}")
        End If

        Return New TypedRowType(Of TCol1, TCol2)(Me)
    End Function

    ''' <summary>
    ''' Maps this row to a <see cref="TypedRowType1(Of TCol1,TCol2,TCol3)"/>.
    ''' </summary>
    Public Function Map(Of TCol1, TCol2, TCol3)() As TypedRowType1(Of TCol1, TCol2, TCol3)
        If Parent.ColumnCount < 3 Then Throw New ArgumentException($"Cannot map row, mapping has 3 columns while row has {Parent.ColumnCount:N0} columns")

        Dim badMap = Not Parent.Metadata.Columns(0).CanMapTo(GetType(TCol1)) OrElse Not Parent.Metadata.Columns(1).CanMapTo(GetType(TCol2)) OrElse Not Parent.Metadata.Columns(2).CanMapTo(GetType(TCol3))

        If badMap Then
            Throw New ArgumentException($"Cannot map dataframe given mapping: {GetType(TCol1).Name} = {Parent.Metadata.Columns(0).MappedType.Name}, {GetType(TCol2).Name} = {Parent.Metadata.Columns(1).MappedType.Name}, {GetType(TCol3).Name} = {Parent.Metadata.Columns(2).MappedType.Name}")
        End If

        Return New TypedRowType1(Of TCol1, TCol2, TCol3)(Me)
    End Function

    ''' <summary>
    ''' Maps this row to a <see cref="TypedRowType2(Of TCol1,TCol2,TCol3,TCol4)"/>.
    ''' </summary>
    Public Function Map(Of TCol1, TCol2, TCol3, TCol4)() As TypedRowType2(Of TCol1, TCol2, TCol3, TCol4)
        If Parent.ColumnCount < 4 Then Throw New ArgumentException($"Cannot map row, mapping has 4 columns while row has {Parent.ColumnCount:N0} columns")

        Dim badMap = Not Parent.Metadata.Columns(0).CanMapTo(GetType(TCol1)) OrElse Not Parent.Metadata.Columns(1).CanMapTo(GetType(TCol2)) OrElse Not Parent.Metadata.Columns(2).CanMapTo(GetType(TCol3)) OrElse Not Parent.Metadata.Columns(3).CanMapTo(GetType(TCol4))

        If badMap Then
            Throw New ArgumentException($"Cannot map dataframe given mapping: {GetType(TCol1).Name} = {Parent.Metadata.Columns(0).MappedType.Name}, {GetType(TCol2).Name} = {Parent.Metadata.Columns(1).MappedType.Name}, {GetType(TCol3).Name} = {Parent.Metadata.Columns(2).MappedType.Name}, {GetType(TCol4).Name} = {Parent.Metadata.Columns(3).MappedType.Name}")
        End If

        Return New TypedRowType2(Of TCol1, TCol2, TCol3, TCol4)(Me)
    End Function

    ''' <summary>
    ''' Maps this row to a <see cref="TypedRowType3(Of TCol1,TCol2,TCol3,TCol4,TCol5)"/>.
    ''' </summary>
    Public Function Map(Of TCol1, TCol2, TCol3, TCol4, TCol5)() As TypedRowType3(Of TCol1, TCol2, TCol3, TCol4, TCol5)
        If Parent.ColumnCount < 5 Then Throw New ArgumentException($"Cannot map row, mapping has 5 columns while row has {Parent.ColumnCount:N0} columns")

        Dim badMap = Not Parent.Metadata.Columns(0).CanMapTo(GetType(TCol1)) OrElse Not Parent.Metadata.Columns(1).CanMapTo(GetType(TCol2)) OrElse Not Parent.Metadata.Columns(2).CanMapTo(GetType(TCol3)) OrElse Not Parent.Metadata.Columns(3).CanMapTo(GetType(TCol4)) OrElse Not Parent.Metadata.Columns(4).CanMapTo(GetType(TCol5))

        If badMap Then
            Throw New ArgumentException($"Cannot map dataframe given mapping: {GetType(TCol1).Name} = {Parent.Metadata.Columns(0).MappedType.Name}, {GetType(TCol2).Name} = {Parent.Metadata.Columns(1).MappedType.Name}, {GetType(TCol3).Name} = {Parent.Metadata.Columns(2).MappedType.Name}, {GetType(TCol4).Name} = {Parent.Metadata.Columns(3).MappedType.Name}, {GetType(TCol5).Name} = {Parent.Metadata.Columns(4).MappedType.Name}")
        End If

        Return New TypedRowType3(Of TCol1, TCol2, TCol3, TCol4, TCol5)(Me)
    End Function

    ''' <summary>
    ''' Maps this row to a <see cref="TypedRowType4(Of TCol1,TCol2,TCol3,TCol4,TCol5,TCol6)"/>.
    ''' </summary>
    Public Function Map(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6)() As TypedRowType4(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6)
        If Parent.ColumnCount < 6 Then Throw New ArgumentException($"Cannot map row, mapping has 6 columns while row has {Parent.ColumnCount:N0} columns")

        Dim badMap = Not Parent.Metadata.Columns(0).CanMapTo(GetType(TCol1)) OrElse Not Parent.Metadata.Columns(1).CanMapTo(GetType(TCol2)) OrElse Not Parent.Metadata.Columns(2).CanMapTo(GetType(TCol3)) OrElse Not Parent.Metadata.Columns(3).CanMapTo(GetType(TCol4)) OrElse Not Parent.Metadata.Columns(4).CanMapTo(GetType(TCol5)) OrElse Not Parent.Metadata.Columns(5).CanMapTo(GetType(TCol6))

        If badMap Then
            Throw New ArgumentException($"Cannot map dataframe given mapping: {GetType(TCol1).Name} = {Parent.Metadata.Columns(0).MappedType.Name}, {GetType(TCol2).Name} = {Parent.Metadata.Columns(1).MappedType.Name}, {GetType(TCol3).Name} = {Parent.Metadata.Columns(2).MappedType.Name}, {GetType(TCol4).Name} = {Parent.Metadata.Columns(3).MappedType.Name}, {GetType(TCol5).Name} = {Parent.Metadata.Columns(4).MappedType.Name}, {GetType(TCol6).Name} = {Parent.Metadata.Columns(5).MappedType.Name}")
        End If

        Return New TypedRowType4(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6)(Me)
    End Function

    ''' <summary>
    ''' Maps this row to a <see cref="TypedRowType5(Of TCol1,TCol2,TCol3,TCol4,TCol5,TCol6,TCol7)"/>.
    ''' </summary>
    Public Function Map(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7)() As TypedRowType5(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7)
        If Parent.ColumnCount < 7 Then Throw New ArgumentException($"Cannot map row, mapping has 7 columns while row has {Parent.ColumnCount:N0} columns")

        Dim badMap = Not Parent.Metadata.Columns(0).CanMapTo(GetType(TCol1)) OrElse Not Parent.Metadata.Columns(1).CanMapTo(GetType(TCol2)) OrElse Not Parent.Metadata.Columns(2).CanMapTo(GetType(TCol3)) OrElse Not Parent.Metadata.Columns(3).CanMapTo(GetType(TCol4)) OrElse Not Parent.Metadata.Columns(4).CanMapTo(GetType(TCol5)) OrElse Not Parent.Metadata.Columns(5).CanMapTo(GetType(TCol6)) OrElse Not Parent.Metadata.Columns(6).CanMapTo(GetType(TCol7))

        If badMap Then
            Throw New ArgumentException($"Cannot map dataframe given mapping: {GetType(TCol1).Name} = {Parent.Metadata.Columns(0).MappedType.Name}, {GetType(TCol2).Name} = {Parent.Metadata.Columns(1).MappedType.Name}, {GetType(TCol3).Name} = {Parent.Metadata.Columns(2).MappedType.Name}, {GetType(TCol4).Name} = {Parent.Metadata.Columns(3).MappedType.Name}, {GetType(TCol5).Name} = {Parent.Metadata.Columns(4).MappedType.Name}, {GetType(TCol6).Name} = {Parent.Metadata.Columns(5).MappedType.Name}, {GetType(TCol7).Name} = {Parent.Metadata.Columns(6).MappedType.Name}")
        End If

        Return New TypedRowType5(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7)(Me)
    End Function

    ''' <summary>
    ''' Maps this row to a <see cref="TypedRowType6(Of TCol1,TCol2,TCol3,TCol4,TCol5,TCol6,TCol7,TCol8)"/>.
    ''' </summary>
    Public Function Map(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7, TCol8)() As TypedRowType6(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7, TCol8)
        If Parent.ColumnCount < 8 Then Throw New ArgumentException($"Cannot map row, mapping has 8 columns while row has {Parent.ColumnCount:N0} columns")

        Dim badMap = Not Parent.Metadata.Columns(0).CanMapTo(GetType(TCol1)) OrElse Not Parent.Metadata.Columns(1).CanMapTo(GetType(TCol2)) OrElse Not Parent.Metadata.Columns(2).CanMapTo(GetType(TCol3)) OrElse Not Parent.Metadata.Columns(3).CanMapTo(GetType(TCol4)) OrElse Not Parent.Metadata.Columns(4).CanMapTo(GetType(TCol5)) OrElse Not Parent.Metadata.Columns(5).CanMapTo(GetType(TCol6)) OrElse Not Parent.Metadata.Columns(6).CanMapTo(GetType(TCol7)) OrElse Not Parent.Metadata.Columns(7).CanMapTo(GetType(TCol8))

        If badMap Then
            Throw New ArgumentException($"Cannot map dataframe given mapping: {GetType(TCol1).Name} = {Parent.Metadata.Columns(0).MappedType.Name}, {GetType(TCol2).Name} = {Parent.Metadata.Columns(1).MappedType.Name}, {GetType(TCol3).Name} = {Parent.Metadata.Columns(2).MappedType.Name}, {GetType(TCol4).Name} = {Parent.Metadata.Columns(3).MappedType.Name}, {GetType(TCol5).Name} = {Parent.Metadata.Columns(4).MappedType.Name}, {GetType(TCol6).Name} = {Parent.Metadata.Columns(5).MappedType.Name}, {GetType(TCol7).Name} = {Parent.Metadata.Columns(6).MappedType.Name}, {GetType(TCol8).Name} = {Parent.Metadata.Columns(7).MappedType.Name}")
        End If

        Return New TypedRowType6(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7, TCol8)(Me)
    End Function

    ''' <summary>
    ''' Equivalent to <see cref="ToArray()"/>
    ''' </summary>
    Public Shared Narrowing Operator CType(row As Row) As Value()
        Return row.ToArray()
    End Operator

    Friend Function UnsafeGetTranslated(Of T)(translatedColumnIndex As Long) As T
        Dim value As Value = Nothing
        If Not Parent.TryGetValueTranslated(TranslatedRowIndex, translatedColumnIndex, value) Then
            Throw New InvalidOperationException($"Unexpectedly couldn't find value at {translatedColumnIndex}")
        End If

        Dim category = Parent.Metadata.Columns(translatedColumnIndex).GetCategoryEnumMap(Of T)()

        Return value.UnsafeCast(Of T)(category)
    End Function
End Class

#Region "Microsoft.VisualBasic::c78ce2cff18dc2a2c4474bac2ca1b6f0, Data\BinaryData\Feather\TypedRow.vb"

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

    '   Total Lines: 1636
    '    Code Lines: 831
    ' Comment Lines: 619
    '   Blank Lines: 186
    '     File Size: 57.98 KB


    ' Class TypedRowValueEnumerator
    ' 
    '     Properties: Current, CurrentProp
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: MoveNext
    ' 
    '     Sub: Dispose, Reset
    ' 
    ' Class TypedRow
    ' 
    '     Properties: Column1, Index, Length
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Equals, GetEnumerator, GetEnumerator1, GetEnumerator2, GetHashCode
    '               GetRange, ToArray, ToString, (+4 Overloads) TryGetValue
    ' 
    '     Sub: (+2 Overloads) GetRange, ToArray
    ' 
    ' Class TypedRowType
    ' 
    '     Properties: Column1, Column2, Index, Length
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Equals, GetEnumerator, GetEnumerator3, GetEnumerator4, GetHashCode
    '               GetRange, ToArray, ToString, (+4 Overloads) TryGetValue
    ' 
    '     Sub: (+2 Overloads) GetRange, ToArray
    ' 
    ' Class TypedRowType1
    ' 
    '     Properties: Column1, Column2, Column3, Index, Length
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Equals, GetEnumerator, GetEnumerator5, GetEnumerator6, GetHashCode
    '               GetRange, ToArray, ToString, (+4 Overloads) TryGetValue
    ' 
    '     Sub: (+2 Overloads) GetRange, ToArray
    ' 
    ' Class TypedRowType2
    ' 
    '     Properties: Column1, Column2, Column3, Column4, Index
    '                 Length
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Equals, GetEnumerator, GetEnumerator7, GetEnumerator8, GetHashCode
    '               GetRange, ToArray, ToString, (+4 Overloads) TryGetValue
    ' 
    '     Sub: (+2 Overloads) GetRange, ToArray
    ' 
    ' Class TypedRowType3
    ' 
    '     Properties: Column1, Column2, Column3, Column4, Column5
    '                 Index, Length
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Equals, GetEnumerator, GetEnumerator10, GetEnumerator9, GetHashCode
    '               GetRange, ToArray, ToString, (+4 Overloads) TryGetValue
    ' 
    '     Sub: (+2 Overloads) GetRange, ToArray
    ' 
    ' Class TypedRowType4
    ' 
    '     Properties: Column1, Column2, Column3, Column4, Column5
    '                 Column6, Index, Length
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Equals, GetEnumerator, GetEnumerator11, GetEnumerator12, GetHashCode
    '               GetRange, ToArray, ToString, (+4 Overloads) TryGetValue
    ' 
    '     Sub: (+2 Overloads) GetRange, ToArray
    ' 
    ' Class TypedRowType5
    ' 
    '     Properties: Column1, Column2, Column3, Column4, Column5
    '                 Column6, Column7, Index, Length
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Equals, GetEnumerator, GetEnumerator13, GetEnumerator14, GetHashCode
    '               GetRange, ToArray, ToString, (+4 Overloads) TryGetValue
    ' 
    '     Sub: (+2 Overloads) GetRange, ToArray
    ' 
    ' Class TypedRowType6
    ' 
    '     Properties: Column1, Column2, Column3, Column4, Column5
    '                 Column6, Column7, Column8, Index, Length
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Equals, GetEnumerator, GetEnumerator15, GetEnumerator16, GetHashCode
    '               GetRange, ToArray, ToString, (+4 Overloads) TryGetValue
    ' 
    '     Sub: (+2 Overloads) GetRange, ToArray
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.DataStorage.FeatherFormat.Impl

''' <summary>
''' Allocation free enumerator for a typed row.
''' </summary>
Public Class TypedRowValueEnumerator
    Implements IEnumerator(Of Value)
    Private Inner As RowValueEnumerator
    Private Length As Long
    Private Index As Long

    ''' <summary>
    ''' <see cref="System.Collections.Generic.IEnumerator(Of T)"/>
    ''' </summary>
    Public ReadOnly Property CurrentProp As Value Implements IEnumerator(Of Value).Current
        Get
            Return Inner.CurrentProp
        End Get
    End Property

    Private ReadOnly Property Current As Object Implements IEnumerator.Current
        Get
            Return CurrentProp
        End Get
    End Property

    Friend Sub New(inner As RowValueEnumerator, length As Long)
        Me.Inner = inner
        Me.Length = length
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
        If Index >= Length Then Return False

        If Not Inner.MoveNext() Then Return False

        Return True
    End Function

    ''' <summary>
    ''' <see cref="System.Collections.Generic.IEnumerator(Of T)"/>
    ''' </summary>
    Public Sub Reset() Implements IEnumerator.Reset
        Inner.Reset()
        Index = -1
    End Sub
End Class

''' <summary>
''' Represents a typed row with 1 column.
''' </summary>
Public Class TypedRow(Of TCol1)
    Implements IEnumerable(Of Value), IRow
    Private Inner As Row

    ''' <summary>
    ''' Returns the Index (in the appropriate basis) of this row in the original dataframe.
    ''' </summary>
    Public ReadOnly Property Index As Long Implements IRow.Index
        Get
            Return Inner.Index
        End Get
    End Property
    ''' <summary>
    ''' Returns the number of entries in the this column.
    ''' This will always be 1.
    ''' </summary>
    Public ReadOnly Property Length As Long Implements IRow.Length
        Get
            Return 1
        End Get
    End Property

    ''' <summary>
    ''' Return the value at the given index.
    ''' 
    ''' Will throw if the index is out of bounds.  Use <see cref="TryGetValue(Long,Value)"/> for non-throwing gets.
    ''' </summary>
    Default Public ReadOnly Property Item(columnIndex As Long) As Value
        Get
            Return Inner(columnIndex)
        End Get
    End Property

    ''' <summary>
    ''' Return the value in the column with the given name.
    ''' 
    ''' Will throw if no column with that name exists .  Use <see cref="TryGetValue(String,Value)"/> for non-throwing gets.
    ''' </summary>
    Default Public ReadOnly Property Item(columnName As String) As Value
        Get
            Return Inner(columnName)
        End Get
    End Property

    ''' <summary>
    ''' Returns the value of the 1st column in the row.
    ''' </summary>
    Public ReadOnly Property Column1 As TCol1
        Get
            Return Inner.UnsafeGetTranslated(Of TCol1)(0)
        End Get
    End Property

    Friend Sub New(inner As Row)
        Me.Inner = inner
    End Sub

    ''' <summary>
    ''' <see cref="Object.Equals(Object)"/>
    ''' </summary>
    Public Overrides Function Equals(obj As Object) As Boolean
        If Not (TypeOf obj Is TypedRow(Of TCol1)) Then Return False

        Dim other = CType(obj, TypedRow(Of TCol1))
        Return other.Inner.Equals(Inner)
    End Function

    ''' <summary>
    ''' <see cref="Object.GetHashCode"/>
    ''' </summary>
    Public Overrides Function GetHashCode() As Integer
        Return Inner.GetHashCode() * 17 + GetType(TCol1).GetHashCode()
    End Function

    ''' <summary>
    ''' <see cref="Object.ToString"/>
    ''' </summary>
    Public Overrides Function ToString() As String
        Return $"TypedRow<{GetType(TCol1).Name}> Index = {Index}"
    End Function

    ''' <summary>
    ''' <see cref="System.Collections.Generic.IEnumerable(Of T).GetEnumerator"/>
    ''' </summary>
    Public Function GetEnumerator() As TypedRowValueEnumerator
        Return New TypedRowValueEnumerator(Inner.GetEnumerator(), Length)
    End Function

    Private Function GetEnumerator1() As IEnumerator(Of Value) Implements IEnumerable(Of Value).GetEnumerator
        Return GetEnumerator()
    End Function

    Private Function GetEnumerator2() As IEnumerator Implements IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(Long,Value)"/>
    ''' </summary>
    Public Function TryGetValue(columnIndex As Long, <Out> ByRef value As Value) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnIndex, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(OfT)(Long,T)"/>
    ''' </summary>
    Public Function TryGetValue(Of T)(columnIndex As Long, <Out> ByRef value As T) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnIndex, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(String,Value)"/>
    ''' </summary>
    Public Function TryGetValue(columnName As String, <Out> ByRef value As Value) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnName, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(OfT)(String,T)"/>
    ''' </summary>
    Public Function TryGetValue(Of T)(columnName As String, <Out> ByRef value As T) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnName, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.ToArray()"/>
    ''' </summary>
    Public Function ToArray() As Value() Implements IRow.ToArray
        Return Inner.ToArray()
    End Function

    ''' <summary>
    ''' <see cref="Row.GetRange(Long,Integer)"/>
    ''' </summary>
    Public Function GetRange(columnIndex As Long, length As Integer) As Value() Implements IRow.GetRange
        Return Inner.GetRange(columnIndex, length)
    End Function

    ''' <summary>
    ''' <see cref="Row.ToArray()"/>
    ''' </summary>
    Public Sub ToArray(ByRef array As Value()) Implements IRow.ToArray
        ToArray(array)
    End Sub

    ''' <summary>
    ''' <see cref="Row.GetRange(Long,Integer,)"/>
    ''' </summary>
    Public Sub GetRange(columnSourceIndex As Long, length As Integer, ByRef array As Value()) Implements IRow.GetRange
        GetRange(columnSourceIndex, length, array)
    End Sub

    ''' <summary>
    ''' <see cref="Row.GetRange(Long,Integer,,Integer)"/>
    ''' </summary>
    Public Sub GetRange(columnSourceIndex As Long, length As Integer, ByRef array As Value(), destinationIndex As Integer) Implements IRow.GetRange
        GetRange(columnSourceIndex, length, array, destinationIndex)
    End Sub

    ''' <summary>
    ''' Equivalent to <see cref="Row.ToArray()"/>
    ''' </summary>
    Public Shared Narrowing Operator CType(row As TypedRow(Of TCol1)) As Value()
        Return row.ToArray()
    End Operator
End Class

''' <summary>
''' Represents a typed row with 2 columns.
''' </summary>
Public Class TypedRowType(Of TCol1, TCol2)
    Implements IEnumerable(Of Value), IRow
    Private Inner As Row

    ''' <summary>
    ''' Returns the Index (in the appropriate basis) of this row in the original dataframe.
    ''' </summary>
    Public ReadOnly Property Index As Long Implements IRow.Index
        Get
            Return Inner.Index
        End Get
    End Property
    ''' <summary>
    ''' Returns the number of entries in the this column.
    ''' This will always be 2.
    ''' </summary>
    Public ReadOnly Property Length As Long Implements IRow.Length
        Get
            Return 2
        End Get
    End Property
    ''' <summary>
    ''' Return the value at the given index.
    ''' 
    ''' Will throw if the index is out of bounds.  Use <see cref="TryGetValue(Long,Value)"/> for non-throwing gets.
    ''' </summary>
    Default Public ReadOnly Property Item(columnIndex As Long) As Value
        Get
            Return Inner(columnIndex)
        End Get
    End Property

    ''' <summary>
    ''' Return the value in the column with the given name.
    ''' 
    ''' Will throw if no column with that name exists .  Use <see cref="TryGetValue(String,Value)"/> for non-throwing gets.
    ''' </summary>
    Default Public ReadOnly Property Item(columnName As String) As Value
        Get
            Return Inner(columnName)
        End Get
    End Property

    ''' <summary>
    ''' Returns the value of the 1st column in the row.
    ''' </summary>
    Public ReadOnly Property Column1 As TCol1
        Get
            Return Inner.UnsafeGetTranslated(Of TCol1)(0)
        End Get
    End Property
    ''' <summary>
    ''' Returns the value of the 2nd column in the row.
    ''' </summary>
    Public ReadOnly Property Column2 As TCol2
        Get
            Return Inner.UnsafeGetTranslated(Of TCol2)(1)
        End Get
    End Property

    Friend Sub New(inner As Row)
        Me.Inner = inner
    End Sub

    ''' <summary>
    ''' <see cref="Object.Equals(Object)"/>
    ''' </summary>
    Public Overrides Function Equals(obj As Object) As Boolean
        If Not (TypeOf obj Is TypedRowType(Of TCol1, TCol2)) Then Return False

        Dim other = CType(obj, TypedRowType(Of TCol1, TCol2))
        Return other.Inner.Equals(Inner)
    End Function

    ''' <summary>
    ''' <see cref="Object.GetHashCode"/>
    ''' </summary>
    Public Overrides Function GetHashCode() As Integer
        Return (Inner.GetHashCode() * 17 + GetType(TCol1).GetHashCode()) * 17 + GetType(TCol2).GetHashCode()
    End Function

    ''' <summary>
    ''' <see cref="Object.ToString"/>
    ''' </summary>
    Public Overrides Function ToString() As String
        Return $"TypedRow<{GetType(TCol1).Name}, {GetType(TCol2).Name}> Index = {Index}"
    End Function

    ''' <summary>
    ''' <see cref="System.Collections.Generic.IEnumerable(Of T).GetEnumerator"/>
    ''' </summary>
    Public Function GetEnumerator() As TypedRowValueEnumerator
        Return New TypedRowValueEnumerator(Inner.GetEnumerator(), Length)
    End Function

    Private Function GetEnumerator3() As IEnumerator(Of Value) Implements IEnumerable(Of Value).GetEnumerator
        Return GetEnumerator()
    End Function

    Private Function GetEnumerator4() As IEnumerator Implements IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(Long,Value)"/>
    ''' </summary>
    Public Function TryGetValue(columnIndex As Long, <Out> ByRef value As Value) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnIndex, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(OfT)(Long,T)"/>
    ''' </summary>
    Public Function TryGetValue(Of T)(columnIndex As Long, <Out> ByRef value As T) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnIndex, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(String,Value)"/>
    ''' </summary>
    Public Function TryGetValue(columnName As String, <Out> ByRef value As Value) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnName, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(OfT)(String,T)"/>
    ''' </summary>
    Public Function TryGetValue(Of T)(columnName As String, <Out> ByRef value As T) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnName, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.ToArray()"/>
    ''' </summary>
    Public Function ToArray() As Value() Implements IRow.ToArray
        Return Inner.ToArray()
    End Function

    ''' <summary>
    ''' <see cref="Row.GetRange(Long,Integer)"/>
    ''' </summary>
    Public Function GetRange(columnIndex As Long, length As Integer) As Value() Implements IRow.GetRange
        Return Inner.GetRange(columnIndex, length)
    End Function

    ''' <summary>
    ''' <see cref="Row.ToArray()"/>
    ''' </summary>
    Public Sub ToArray(ByRef array As Value()) Implements IRow.ToArray
        ToArray(array)
    End Sub

    ''' <summary>
    ''' <see cref="Row.GetRange(Long,Integer,)"/>
    ''' </summary>
    Public Sub GetRange(columnSourceIndex As Long, length As Integer, ByRef array As Value()) Implements IRow.GetRange
        GetRange(columnSourceIndex, length, array)
    End Sub

    ''' <summary>
    ''' <see cref="Row.GetRange(Long,Integer,,Integer)"/>
    ''' </summary>
    Public Sub GetRange(columnSourceIndex As Long, length As Integer, ByRef array As Value(), destinationIndex As Integer) Implements IRow.GetRange
        GetRange(columnSourceIndex, length, array, destinationIndex)
    End Sub

    ''' <summary>
    ''' Equivalent to <see cref="Row.ToArray()"/>
    ''' </summary>
    Public Shared Narrowing Operator CType(row As TypedRowType(Of TCol1, TCol2)) As Value()
        Return row.ToArray()
    End Operator
End Class

''' <summary>
''' Represents a typed row with 3 columns.
''' </summary>
Public Class TypedRowType1(Of TCol1, TCol2, TCol3)
    Implements IEnumerable(Of Value), IRow
    Private Inner As Row

    ''' <summary>
    ''' Returns the Index (in the appropriate basis) of this row in the original dataframe.
    ''' </summary>
    Public ReadOnly Property Index As Long Implements IRow.Index
        Get
            Return Inner.Index
        End Get
    End Property
    ''' <summary>
    ''' Returns the number of entries in the this column.
    ''' This will always be 3.
    ''' </summary>
    Public ReadOnly Property Length As Long Implements IRow.Length
        Get
            Return 3
        End Get
    End Property
    ''' <summary>
    ''' Return the value at the given index.
    ''' 
    ''' Will throw if the index is out of bounds.  Use <see cref="TryGetValue(Long,Value)"/> for non-throwing gets.
    ''' </summary>
    Default Public ReadOnly Property Item(columnIndex As Long) As Value
        Get
            Return Inner(columnIndex)
        End Get
    End Property

    ''' <summary>
    ''' Return the value in the column with the given name.
    ''' 
    ''' Will throw if no column with that name exists .  Use <see cref="TryGetValue(String,Value)"/> for non-throwing gets.
    ''' </summary>
    Default Public ReadOnly Property Item(columnName As String) As Value
        Get
            Return Inner(columnName)
        End Get
    End Property

    ''' <summary>
    ''' Returns the value of the 1st column in the row.
    ''' </summary>
    Public ReadOnly Property Column1 As TCol1
        Get
            Return Inner.UnsafeGetTranslated(Of TCol1)(0)
        End Get
    End Property
    ''' <summary>
    ''' Returns the value of the 2nd column in the row.
    ''' </summary>
    Public ReadOnly Property Column2 As TCol2
        Get
            Return Inner.UnsafeGetTranslated(Of TCol2)(1)
        End Get
    End Property
    ''' <summary>
    ''' Returns the value of the 3rd column in the row.
    ''' </summary>
    Public ReadOnly Property Column3 As TCol3
        Get
            Return Inner.UnsafeGetTranslated(Of TCol3)(2)
        End Get
    End Property

    Friend Sub New(inner As Row)
        Me.Inner = inner
    End Sub

    ''' <summary>
    ''' <see cref="Object.Equals(Object)"/>
    ''' </summary>
    Public Overrides Function Equals(obj As Object) As Boolean
        If Not (TypeOf obj Is TypedRowType1(Of TCol1, TCol2, TCol3)) Then Return False

        Dim other = CType(obj, TypedRowType1(Of TCol1, TCol2, TCol3))
        Return other.Inner.Equals(Inner)
    End Function

    ''' <summary>
    ''' <see cref="Object.GetHashCode"/>
    ''' </summary>
    Public Overrides Function GetHashCode() As Integer
        Return ((Inner.GetHashCode() * 17 + GetType(TCol1).GetHashCode()) * 17 + GetType(TCol2).GetHashCode()) * 17 + GetType(TCol3).GetHashCode()
    End Function

    ''' <summary>
    ''' <see cref="Object.ToString"/>
    ''' </summary>
    Public Overrides Function ToString() As String
        Return $"TypedRow<{GetType(TCol1).Name}, {GetType(TCol2).Name}, {GetType(TCol3).Name}> Index = {Index}"
    End Function

    ''' <summary>
    ''' <see cref="System.Collections.Generic.IEnumerable(Of T).GetEnumerator"/>
    ''' </summary>
    Public Function GetEnumerator() As TypedRowValueEnumerator
        Return New TypedRowValueEnumerator(Inner.GetEnumerator(), Length)
    End Function

    Private Function GetEnumerator5() As IEnumerator(Of Value) Implements IEnumerable(Of Value).GetEnumerator
        Return GetEnumerator()
    End Function

    Private Function GetEnumerator6() As IEnumerator Implements IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(Long,Value)"/>
    ''' </summary>
    Public Function TryGetValue(columnIndex As Long, <Out> ByRef value As Value) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnIndex, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(Of T)(Long,T)"/>
    ''' </summary>
    Public Function TryGetValue(Of T)(columnIndex As Long, <Out> ByRef value As T) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnIndex, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(String,Value)"/>
    ''' </summary>
    Public Function TryGetValue(columnName As String, <Out> ByRef value As Value) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnName, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(OfT)(String,T)"/>
    ''' </summary>
    Public Function TryGetValue(Of T)(columnName As String, <Out> ByRef value As T) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnName, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.ToArray()"/>
    ''' </summary>
    Public Function ToArray() As Value() Implements IRow.ToArray
        Return Inner.ToArray()
    End Function

    ''' <summary>
    ''' <see cref="Row.GetRange(Long,Integer)"/>
    ''' </summary>
    Public Function GetRange(columnIndex As Long, length As Integer) As Value() Implements IRow.GetRange
        Return Inner.GetRange(columnIndex, length)
    End Function

    ''' <summary>
    ''' <see cref="Row.ToArray()"/>
    ''' </summary>
    Public Sub ToArray(ByRef array As Value()) Implements IRow.ToArray
        ToArray(array)
    End Sub

    ''' <summary>
    ''' <see cref="Row.GetRange(Long,Integer,)"/>
    ''' </summary>
    Public Sub GetRange(columnSourceIndex As Long, length As Integer, ByRef array As Value()) Implements IRow.GetRange
        GetRange(columnSourceIndex, length, array)
    End Sub

    ''' <summary>
    ''' <see cref="Row.GetRange(Long,Integer,,Integer)"/>
    ''' </summary>
    Public Sub GetRange(columnSourceIndex As Long, length As Integer, ByRef array As Value(), destinationIndex As Integer) Implements IRow.GetRange
        GetRange(columnSourceIndex, length, array, destinationIndex)
    End Sub

    ''' <summary>
    ''' Equivalent to <see cref="Row.ToArray()"/>
    ''' </summary>
    Public Shared Narrowing Operator CType(row As TypedRowType1(Of TCol1, TCol2, TCol3)) As Value()
        Return row.ToArray()
    End Operator
End Class

''' <summary>
''' Represents a typed row with 4 columns.
''' </summary>
Public Class TypedRowType2(Of TCol1, TCol2, TCol3, TCol4)
    Implements IEnumerable(Of Value), IRow
    Private Inner As Row

    ''' <summary>
    ''' Returns the Index (in the appropriate basis) of this row in the original dataframe.
    ''' </summary>
    Public ReadOnly Property Index As Long Implements IRow.Index
        Get
            Return Inner.Index
        End Get
    End Property
    ''' <summary>
    ''' Returns the number of entries in the this column.
    ''' This will always be 4.
    ''' </summary>
    Public ReadOnly Property Length As Long Implements IRow.Length
        Get
            Return 4
        End Get
    End Property
    ''' <summary>
    ''' Return the value at the given index.
    ''' 
    ''' Will throw if the index is out of bounds.  Use <see cref="TryGetValue(Long,Value)"/> for non-throwing gets.
    ''' </summary>
    Default Public ReadOnly Property Item(columnIndex As Long) As Value
        Get
            Return Inner(columnIndex)
        End Get
    End Property

    ''' <summary>
    ''' Return the value in the column with the given name.
    ''' 
    ''' Will throw if no column with that name exists .  Use <see cref="TryGetValue(String,Value)"/> for non-throwing gets.
    ''' </summary>
    Default Public ReadOnly Property Item(columnName As String) As Value
        Get
            Return Inner(columnName)
        End Get
    End Property

    ''' <summary>
    ''' Returns the value of the 1st column in the row.
    ''' </summary>
    Public ReadOnly Property Column1 As TCol1
        Get
            Return Inner.UnsafeGetTranslated(Of TCol1)(0)
        End Get
    End Property
    ''' <summary>
    ''' Returns the value of the 2nd column in the row.
    ''' </summary>
    Public ReadOnly Property Column2 As TCol2
        Get
            Return Inner.UnsafeGetTranslated(Of TCol2)(1)
        End Get
    End Property
    ''' <summary>
    ''' Returns the value of the 3rd column in the row.
    ''' </summary>
    Public ReadOnly Property Column3 As TCol3
        Get
            Return Inner.UnsafeGetTranslated(Of TCol3)(2)
        End Get
    End Property
    ''' <summary>
    ''' Returns the value of the 4th column in the row.
    ''' </summary>
    Public ReadOnly Property Column4 As TCol4
        Get
            Return Inner.UnsafeGetTranslated(Of TCol4)(3)
        End Get
    End Property

    Friend Sub New(inner As Row)
        Me.Inner = inner
    End Sub

    ''' <summary>
    ''' <see cref="Object.Equals(Object)"/>
    ''' </summary>
    Public Overrides Function Equals(obj As Object) As Boolean
        If Not (TypeOf obj Is TypedRowType2(Of TCol1, TCol2, TCol3, TCol4)) Then Return False

        Dim other = CType(obj, TypedRowType2(Of TCol1, TCol2, TCol3, TCol4))
        Return other.Inner.Equals(Inner)
    End Function

    ''' <summary>
    ''' <see cref="Object.GetHashCode"/>
    ''' </summary>
    Public Overrides Function GetHashCode() As Integer
        Return (((Inner.GetHashCode() * 17 + GetType(TCol1).GetHashCode()) * 17 + GetType(TCol2).GetHashCode()) * 17 + GetType(TCol3).GetHashCode()) * 17 + GetType(TCol4).GetHashCode()
    End Function

    ''' <summary>
    ''' <see cref="Object.ToString"/>
    ''' </summary>
    Public Overrides Function ToString() As String
        Return $"TypedRow<{GetType(TCol1).Name}, {GetType(TCol2).Name}, {GetType(TCol3).Name}, {GetType(TCol4).Name}> Index = {Index}"
    End Function

    ''' <summary>
    ''' <see cref="System.Collections.Generic.IEnumerable(Of T).GetEnumerator"/>
    ''' </summary>
    Public Function GetEnumerator() As TypedRowValueEnumerator
        Return New TypedRowValueEnumerator(Inner.GetEnumerator(), Length)
    End Function

    Private Function GetEnumerator7() As IEnumerator(Of Value) Implements IEnumerable(Of Value).GetEnumerator
        Return GetEnumerator()
    End Function

    Private Function GetEnumerator8() As IEnumerator Implements IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(Long,Value)"/>
    ''' </summary>
    Public Function TryGetValue(columnIndex As Long, <Out> ByRef value As Value) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnIndex, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(OfT)(Long,T)"/>
    ''' </summary>
    Public Function TryGetValue(Of T)(columnIndex As Long, <Out> ByRef value As T) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnIndex, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(String,Value)"/>
    ''' </summary>
    Public Function TryGetValue(columnName As String, <Out> ByRef value As Value) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnName, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(OfT)(String,T)"/>
    ''' </summary>
    Public Function TryGetValue(Of T)(columnName As String, <Out> ByRef value As T) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnName, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.ToArray()"/>
    ''' </summary>
    Public Function ToArray() As Value() Implements IRow.ToArray
        Return Inner.ToArray()
    End Function

    ''' <summary>
    ''' <see cref="Row.GetRange(Long,Integer)"/>
    ''' </summary>
    Public Function GetRange(columnIndex As Long, length As Integer) As Value() Implements IRow.GetRange
        Return Inner.GetRange(columnIndex, length)
    End Function

    ''' <summary>
    ''' <see cref="Row.ToArray()"/>
    ''' </summary>
    Public Sub ToArray(ByRef array As Value()) Implements IRow.ToArray
        ToArray(array)
    End Sub

    ''' <summary>
    ''' <see cref="Row.GetRange(Long,Integer,)"/>
    ''' </summary>
    Public Sub GetRange(columnSourceIndex As Long, length As Integer, ByRef array As Value()) Implements IRow.GetRange
        GetRange(columnSourceIndex, length, array)
    End Sub

    ''' <summary>
    ''' <see cref="Row.GetRange(Long,Integer,,Integer)"/>
    ''' </summary>
    Public Sub GetRange(columnSourceIndex As Long, length As Integer, ByRef array As Value(), destinationIndex As Integer) Implements IRow.GetRange
        GetRange(columnSourceIndex, length, array, destinationIndex)
    End Sub

    ''' <summary>
    ''' Equivalent to <see cref="Row.ToArray()"/>
    ''' </summary>
    Public Shared Narrowing Operator CType(row As TypedRowType2(Of TCol1, TCol2, TCol3, TCol4)) As Value()
        Return row.ToArray()
    End Operator
End Class

''' <summary>
''' Represents a typed row with 5 columns.
''' </summary>
Public Class TypedRowType3(Of TCol1, TCol2, TCol3, TCol4, TCol5)
    Implements IEnumerable(Of Value), IRow
    Private Inner As Row

    ''' <summary>
    ''' Returns the Index (in the appropriate basis) of this row in the original dataframe.
    ''' </summary>
    Public ReadOnly Property Index As Long Implements IRow.Index
        Get
            Return Inner.Index
        End Get
    End Property
    ''' <summary>
    ''' Returns the number of entries in the this column.
    ''' This will always be 5.
    ''' </summary>
    Public ReadOnly Property Length As Long Implements IRow.Length
        Get
            Return 5
        End Get
    End Property
    ''' <summary>
    ''' Return the value at the given index.
    ''' 
    ''' Will throw if the index is out of bounds.  Use <see cref="TryGetValue(Long,Value)"/> for non-throwing gets.
    ''' </summary>
    Default Public ReadOnly Property Item(columnIndex As Long) As Value
        Get
            Return Inner(columnIndex)
        End Get
    End Property

    ''' <summary>
    ''' Return the value in the column with the given name.
    ''' 
    ''' Will throw if no column with that name exists .  Use <see cref="TryGetValue(String,Value)"/> for non-throwing gets.
    ''' </summary>
    Default Public ReadOnly Property Item(columnName As String) As Value
        Get
            Return Inner(columnName)
        End Get
    End Property

    ''' <summary>
    ''' Returns the value of the 1st column in the row.
    ''' </summary>
    Public ReadOnly Property Column1 As TCol1
        Get
            Return Inner.UnsafeGetTranslated(Of TCol1)(0)
        End Get
    End Property
    ''' <summary>
    ''' Returns the value of the 2nd column in the row.
    ''' </summary>
    Public ReadOnly Property Column2 As TCol2
        Get
            Return Inner.UnsafeGetTranslated(Of TCol2)(1)
        End Get
    End Property
    ''' <summary>
    ''' Returns the value of the 3rd column in the row.
    ''' </summary>
    Public ReadOnly Property Column3 As TCol3
        Get
            Return Inner.UnsafeGetTranslated(Of TCol3)(2)
        End Get
    End Property
    ''' <summary>
    ''' Returns the value of the 4th column in the row.
    ''' </summary>
    Public ReadOnly Property Column4 As TCol4
        Get
            Return Inner.UnsafeGetTranslated(Of TCol4)(3)
        End Get
    End Property
    ''' <summary>
    ''' Returns the value of the 5th column in the row.
    ''' </summary>
    Public ReadOnly Property Column5 As TCol5
        Get
            Return Inner.UnsafeGetTranslated(Of TCol5)(4)
        End Get
    End Property

    Friend Sub New(inner As Row)
        Me.Inner = inner
    End Sub

    ''' <summary>
    ''' <see cref="Object.Equals(Object)"/>
    ''' </summary>
    Public Overrides Function Equals(obj As Object) As Boolean
        If Not (TypeOf obj Is TypedRowType3(Of TCol1, TCol2, TCol3, TCol4, TCol5)) Then Return False

        Dim other = CType(obj, TypedRowType3(Of TCol1, TCol2, TCol3, TCol4, TCol5))
        Return other.Inner.Equals(Inner)
    End Function

    ''' <summary>
    ''' <see cref="Object.GetHashCode"/>
    ''' </summary>
    Public Overrides Function GetHashCode() As Integer
        Return ((((Inner.GetHashCode() * 17 + GetType(TCol1).GetHashCode()) * 17 + GetType(TCol2).GetHashCode()) * 17 + GetType(TCol3).GetHashCode()) * 17 + GetType(TCol4).GetHashCode()) * 17 + GetType(TCol5).GetHashCode()
    End Function

    ''' <summary>
    ''' <see cref="Object.ToString"/>
    ''' </summary>
    Public Overrides Function ToString() As String
        Return $"TypedRow<{GetType(TCol1).Name}, {GetType(TCol2).Name}, {GetType(TCol3).Name}, {GetType(TCol4).Name}, {GetType(TCol5).Name}> Index = {Index}"
    End Function

    ''' <summary>
    ''' <see cref="System.Collections.Generic.IEnumerable(Of T).GetEnumerator"/>
    ''' </summary>
    Public Function GetEnumerator() As TypedRowValueEnumerator
        Return New TypedRowValueEnumerator(Inner.GetEnumerator(), Length)
    End Function

    Private Function GetEnumerator9() As IEnumerator(Of Value) Implements IEnumerable(Of Value).GetEnumerator
        Return GetEnumerator()
    End Function

    Private Function GetEnumerator10() As IEnumerator Implements IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(Long,Value)"/>
    ''' </summary>
    Public Function TryGetValue(columnIndex As Long, <Out> ByRef value As Value) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnIndex, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(OfT)(Long,T)"/>
    ''' </summary>
    Public Function TryGetValue(Of T)(columnIndex As Long, <Out> ByRef value As T) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnIndex, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(String,Value)"/>
    ''' </summary>
    Public Function TryGetValue(columnName As String, <Out> ByRef value As Value) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnName, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(OfT)(String,T)"/>
    ''' </summary>
    Public Function TryGetValue(Of T)(columnName As String, <Out> ByRef value As T) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnName, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.ToArray()"/>
    ''' </summary>
    Public Function ToArray() As Value() Implements IRow.ToArray
        Return Inner.ToArray()
    End Function

    ''' <summary>
    ''' <see cref="Row.GetRange(Long,Integer)"/>
    ''' </summary>
    Public Function GetRange(columnIndex As Long, length As Integer) As Value() Implements IRow.GetRange
        Return Inner.GetRange(columnIndex, length)
    End Function

    ''' <summary>
    ''' <see cref="Row.ToArray()"/>
    ''' </summary>
    Public Sub ToArray(ByRef array As Value()) Implements IRow.ToArray
        ToArray(array)
    End Sub

    ''' <summary>
    ''' <see cref="Row.GetRange(Long,Integer,)"/>
    ''' </summary>
    Public Sub GetRange(columnSourceIndex As Long, length As Integer, ByRef array As Value()) Implements IRow.GetRange
        GetRange(columnSourceIndex, length, array)
    End Sub

    ''' <summary>
    ''' <see cref="Row.GetRange(Long,Integer,,Integer)"/>
    ''' </summary>
    Public Sub GetRange(columnSourceIndex As Long, length As Integer, ByRef array As Value(), destinationIndex As Integer) Implements IRow.GetRange
        GetRange(columnSourceIndex, length, array, destinationIndex)
    End Sub

    ''' <summary>
    ''' Equivalent to <see cref="Row.ToArray()"/>
    ''' </summary>
    Public Shared Narrowing Operator CType(row As TypedRowType3(Of TCol1, TCol2, TCol3, TCol4, TCol5)) As Value()
        Return row.ToArray()
    End Operator
End Class

''' <summary>
''' Represents a typed row with 6 columns.
''' </summary>
Public Class TypedRowType4(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6)
    Implements IEnumerable(Of Value), IRow
    Private Inner As Row

    ''' <summary>
    ''' Returns the Index (in the appropriate basis) of this row in the original dataframe.
    ''' </summary>
    Public ReadOnly Property Index As Long Implements IRow.Index
        Get
            Return Inner.Index
        End Get
    End Property
    ''' <summary>
    ''' Returns the number of entries in the this column.
    ''' This will always be 6.
    ''' </summary>
    Public ReadOnly Property Length As Long Implements IRow.Length
        Get
            Return 6
        End Get
    End Property
    ''' <summary>
    ''' Return the value at the given index.
    ''' 
    ''' Will throw if the index is out of bounds.  Use <see cref="TryGetValue(Long,Value)"/> for non-throwing gets.
    ''' </summary>
    Default Public ReadOnly Property Item(columnIndex As Long) As Value
        Get
            Return Inner(columnIndex)
        End Get
    End Property

    ''' <summary>
    ''' Return the value in the column with the given name.
    ''' 
    ''' Will throw if no column with that name exists .  Use <see cref="TryGetValue(String,Value)"/> for non-throwing gets.
    ''' </summary>
    Default Public ReadOnly Property Item(columnName As String) As Value
        Get
            Return Inner(columnName)
        End Get
    End Property

    ''' <summary>
    ''' Returns the value of the 1st column in the row.
    ''' </summary>
    Public ReadOnly Property Column1 As TCol1
        Get
            Return Inner.UnsafeGetTranslated(Of TCol1)(0)
        End Get
    End Property
    ''' <summary>
    ''' Returns the value of the 2nd column in the row.
    ''' </summary>
    Public ReadOnly Property Column2 As TCol2
        Get
            Return Inner.UnsafeGetTranslated(Of TCol2)(1)
        End Get
    End Property
    ''' <summary>
    ''' Returns the value of the 3rd column in the row.
    ''' </summary>
    Public ReadOnly Property Column3 As TCol3
        Get
            Return Inner.UnsafeGetTranslated(Of TCol3)(2)
        End Get
    End Property
    ''' <summary>
    ''' Returns the value of the 4th column in the row.
    ''' </summary>
    Public ReadOnly Property Column4 As TCol4
        Get
            Return Inner.UnsafeGetTranslated(Of TCol4)(3)
        End Get
    End Property
    ''' <summary>
    ''' Returns the value of the 5th column in the row.
    ''' </summary>
    Public ReadOnly Property Column5 As TCol5
        Get
            Return Inner.UnsafeGetTranslated(Of TCol5)(4)
        End Get
    End Property
    ''' <summary>
    ''' Returns the value of the 6th column in the row.
    ''' </summary>
    Public ReadOnly Property Column6 As TCol6
        Get
            Return Inner.UnsafeGetTranslated(Of TCol6)(5)
        End Get
    End Property

    Friend Sub New(inner As Row)
        Me.Inner = inner
    End Sub

    ''' <summary>
    ''' <see cref="Object.Equals(Object)"/>
    ''' </summary>
    Public Overrides Function Equals(obj As Object) As Boolean
        If Not (TypeOf obj Is TypedRowType4(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6)) Then Return False

        Dim other = CType(obj, TypedRowType4(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6))
        Return other.Inner.Equals(Inner)
    End Function

    ''' <summary>
    ''' <see cref="Object.GetHashCode"/>
    ''' </summary>
    Public Overrides Function GetHashCode() As Integer
        Return (((((Inner.GetHashCode() * 17 + GetType(TCol1).GetHashCode()) * 17 + GetType(TCol2).GetHashCode()) * 17 + GetType(TCol3).GetHashCode()) * 17 + GetType(TCol4).GetHashCode()) * 17 + GetType(TCol5).GetHashCode()) * 17 + GetType(TCol6).GetHashCode()
    End Function

    ''' <summary>
    ''' <see cref="Object.ToString"/>
    ''' </summary>
    Public Overrides Function ToString() As String
        Return $"TypedRow<{GetType(TCol1).Name}, {GetType(TCol2).Name}, {GetType(TCol3).Name}, {GetType(TCol4).Name}, {GetType(TCol5).Name}, {GetType(TCol6).Name}> Index = {Index}"
    End Function

    ''' <summary>
    ''' <see cref="System.Collections.Generic.IEnumerable(Of T).GetEnumerator"/>
    ''' </summary>
    Public Function GetEnumerator() As TypedRowValueEnumerator
        Return New TypedRowValueEnumerator(Inner.GetEnumerator(), Length)
    End Function

    Private Function GetEnumerator11() As IEnumerator(Of Value) Implements IEnumerable(Of Value).GetEnumerator
        Return GetEnumerator()
    End Function

    Private Function GetEnumerator12() As IEnumerator Implements IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(Long,Value)"/>
    ''' </summary>
    Public Function TryGetValue(columnIndex As Long, <Out> ByRef value As Value) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnIndex, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(OfT)(Long,T)"/>
    ''' </summary>
    Public Function TryGetValue(Of T)(columnIndex As Long, <Out> ByRef value As T) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnIndex, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(String,Value)"/>
    ''' </summary>
    Public Function TryGetValue(columnName As String, <Out> ByRef value As Value) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnName, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(OfT)(String,T)"/>
    ''' </summary>
    Public Function TryGetValue(Of T)(columnName As String, <Out> ByRef value As T) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnName, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.ToArray()"/>
    ''' </summary>
    Public Function ToArray() As Value() Implements IRow.ToArray
        Return Inner.ToArray()
    End Function

    ''' <summary>
    ''' <see cref="Row.GetRange(Long,Integer)"/>
    ''' </summary>
    Public Function GetRange(columnIndex As Long, length As Integer) As Value() Implements IRow.GetRange
        Return Inner.GetRange(columnIndex, length)
    End Function

    ''' <summary>
    ''' <see cref="Row.ToArray()"/>
    ''' </summary>
    Public Sub ToArray(ByRef array As Value()) Implements IRow.ToArray
        ToArray(array)
    End Sub

    ''' <summary>
    ''' <see cref="Row.GetRange(Long,Integer,)"/>
    ''' </summary>
    Public Sub GetRange(columnSourceIndex As Long, length As Integer, ByRef array As Value()) Implements IRow.GetRange
        GetRange(columnSourceIndex, length, array)
    End Sub

    ''' <summary>
    ''' <see cref="Row.GetRange(Long,Integer,,Integer)"/>
    ''' </summary>
    Public Sub GetRange(columnSourceIndex As Long, length As Integer, ByRef array As Value(), destinationIndex As Integer) Implements IRow.GetRange
        GetRange(columnSourceIndex, length, array, destinationIndex)
    End Sub

    ''' <summary>
    ''' Equivalent to <see cref="Row.ToArray()"/>
    ''' </summary>
    Public Shared Narrowing Operator CType(row As TypedRowType4(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6)) As Value()
        Return row.ToArray()
    End Operator
End Class

''' <summary>
''' Represents a typed row with 7 columns.
''' </summary>
Public Class TypedRowType5(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7)
    Implements IEnumerable(Of Value), IRow
    Private Inner As Row

    ''' <summary>
    ''' Returns the Index (in the appropriate basis) of this row in the original dataframe.
    ''' </summary>
    Public ReadOnly Property Index As Long Implements IRow.Index
        Get
            Return Inner.Index
        End Get
    End Property
    ''' <summary>
    ''' Returns the number of entries in the this column.
    ''' This will always be 7.
    ''' </summary>
    Public ReadOnly Property Length As Long Implements IRow.Length
        Get
            Return 7
        End Get
    End Property
    ''' <summary>
    ''' Return the value at the given index.
    ''' 
    ''' Will throw if the index is out of bounds.  Use <see cref="TryGetValue(Long,Value)"/> for non-throwing gets.
    ''' </summary>
    Default Public ReadOnly Property Item(columnIndex As Long) As Value
        Get
            Return Inner(columnIndex)
        End Get
    End Property

    ''' <summary>
    ''' Return the value in the column with the given name.
    ''' 
    ''' Will throw if no column with that name exists .  Use <see cref="TryGetValue(String,Value)"/> for non-throwing gets.
    ''' </summary>
    Default Public ReadOnly Property Item(columnName As String) As Value
        Get
            Return Inner(columnName)
        End Get
    End Property

    ''' <summary>
    ''' Returns the value of the 1st column in the row.
    ''' </summary>
    Public ReadOnly Property Column1 As TCol1
        Get
            Return Inner.UnsafeGetTranslated(Of TCol1)(0)
        End Get
    End Property
    ''' <summary>
    ''' Returns the value of the 2nd column in the row.
    ''' </summary>
    Public ReadOnly Property Column2 As TCol2
        Get
            Return Inner.UnsafeGetTranslated(Of TCol2)(1)
        End Get
    End Property
    ''' <summary>
    ''' Returns the value of the 3rd column in the row.
    ''' </summary>
    Public ReadOnly Property Column3 As TCol3
        Get
            Return Inner.UnsafeGetTranslated(Of TCol3)(2)
        End Get
    End Property
    ''' <summary>
    ''' Returns the value of the 4th column in the row.
    ''' </summary>
    Public ReadOnly Property Column4 As TCol4
        Get
            Return Inner.UnsafeGetTranslated(Of TCol4)(3)
        End Get
    End Property
    ''' <summary>
    ''' Returns the value of the 5th column in the row.
    ''' </summary>
    Public ReadOnly Property Column5 As TCol5
        Get
            Return Inner.UnsafeGetTranslated(Of TCol5)(4)
        End Get
    End Property
    ''' <summary>
    ''' Returns the value of the 6th column in the row.
    ''' </summary>
    Public ReadOnly Property Column6 As TCol6
        Get
            Return Inner.UnsafeGetTranslated(Of TCol6)(5)
        End Get
    End Property
    ''' <summary>
    ''' Returns the value of the 7th column in the row.
    ''' </summary>
    Public ReadOnly Property Column7 As TCol7
        Get
            Return Inner.UnsafeGetTranslated(Of TCol7)(6)
        End Get
    End Property

    Friend Sub New(inner As Row)
        Me.Inner = inner
    End Sub

    ''' <summary>
    ''' <see cref="Object.Equals(Object)"/>
    ''' </summary>
    Public Overrides Function Equals(obj As Object) As Boolean
        If Not (TypeOf obj Is TypedRowType5(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7)) Then Return False

        Dim other = CType(obj, TypedRowType5(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7))
        Return other.Inner.Equals(Inner)
    End Function

    ''' <summary>
    ''' <see cref="Object.GetHashCode"/>
    ''' </summary>
    Public Overrides Function GetHashCode() As Integer
        Return ((((((Inner.GetHashCode() * 17 + GetType(TCol1).GetHashCode()) * 17 + GetType(TCol2).GetHashCode()) * 17 + GetType(TCol3).GetHashCode()) * 17 + GetType(TCol4).GetHashCode()) * 17 + GetType(TCol5).GetHashCode()) * 17 + GetType(TCol6).GetHashCode()) * 17 + GetType(TCol7).GetHashCode()
    End Function

    ''' <summary>
    ''' <see cref="Object.ToString"/>
    ''' </summary>
    Public Overrides Function ToString() As String
        Return $"TypedRow<{GetType(TCol1).Name}, {GetType(TCol2).Name}, {GetType(TCol3).Name}, {GetType(TCol4).Name}, {GetType(TCol5).Name}, {GetType(TCol6).Name}, {GetType(TCol7).Name}> Index = {Index}"
    End Function

    ''' <summary>
    ''' <see cref="System.Collections.Generic.IEnumerable(Of T).GetEnumerator"/>
    ''' </summary>
    Public Function GetEnumerator() As TypedRowValueEnumerator
        Return New TypedRowValueEnumerator(Inner.GetEnumerator(), Length)
    End Function

    Private Function GetEnumerator13() As IEnumerator(Of Value) Implements IEnumerable(Of Value).GetEnumerator
        Return GetEnumerator()
    End Function

    Private Function GetEnumerator14() As IEnumerator Implements IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(Long,Value)"/>
    ''' </summary>
    Public Function TryGetValue(columnIndex As Long, <Out> ByRef value As Value) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnIndex, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(OfT)(Long,T)"/>
    ''' </summary>
    Public Function TryGetValue(Of T)(columnIndex As Long, <Out> ByRef value As T) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnIndex, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(String,Value)"/>
    ''' </summary>
    Public Function TryGetValue(columnName As String, <Out> ByRef value As Value) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnName, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(OfT)(String,T)"/>
    ''' </summary>
    Public Function TryGetValue(Of T)(columnName As String, <Out> ByRef value As T) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnName, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.ToArray()"/>
    ''' </summary>
    Public Function ToArray() As Value() Implements IRow.ToArray
        Return Inner.ToArray()
    End Function

    ''' <summary>
    ''' <see cref="Row.GetRange(Long,Integer)"/>
    ''' </summary>
    Public Function GetRange(columnIndex As Long, length As Integer) As Value() Implements IRow.GetRange
        Return Inner.GetRange(columnIndex, length)
    End Function

    ''' <summary>
    ''' <see cref="Row.ToArray()"/>
    ''' </summary>
    Public Sub ToArray(ByRef array As Value()) Implements IRow.ToArray
        ToArray(array)
    End Sub

    ''' <summary>
    ''' <see cref="Row.GetRange(Long,Integer,)"/>
    ''' </summary>
    Public Sub GetRange(columnSourceIndex As Long, length As Integer, ByRef array As Value()) Implements IRow.GetRange
        GetRange(columnSourceIndex, length, array)
    End Sub

    ''' <summary>
    ''' <see cref="Row.GetRange(Long,Integer,,Integer)"/>
    ''' </summary>
    Public Sub GetRange(columnSourceIndex As Long, length As Integer, ByRef array As Value(), destinationIndex As Integer) Implements IRow.GetRange
        GetRange(columnSourceIndex, length, array, destinationIndex)
    End Sub

    ''' <summary>
    ''' Equivalent to <see cref="Row.ToArray()"/>
    ''' </summary>
    Public Shared Narrowing Operator CType(row As TypedRowType5(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7)) As Value()
        Return row.ToArray()
    End Operator
End Class

''' <summary>
''' Represents a typed row with 8 columns.
''' </summary>
Public Class TypedRowType6(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7, TCol8)
    Implements IEnumerable(Of Value), IRow
    Private Inner As Row

    ''' <summary>
    ''' Returns the Index (in the appropriate basis) of this row in the original dataframe.
    ''' </summary>
    Public ReadOnly Property Index As Long Implements IRow.Index
        Get
            Return Inner.Index
        End Get
    End Property
    ''' <summary>
    ''' Returns the number of entries in the this column.
    ''' This will always be 8.
    ''' </summary>
    Public ReadOnly Property Length As Long Implements IRow.Length
        Get
            Return 8
        End Get
    End Property
    ''' <summary>
    ''' Return the value at the given index.
    ''' 
    ''' Will throw if the index is out of bounds.  Use <see cref="TryGetValue(Long,Value)"/> for non-throwing gets.
    ''' </summary>
    Default Public ReadOnly Property Item(columnIndex As Long) As Value
        Get
            Return Inner(columnIndex)
        End Get
    End Property

    ''' <summary>
    ''' Return the value in the column with the given name.
    ''' 
    ''' Will throw if no column with that name exists .  Use <see cref="TryGetValue(String,Value)"/> for non-throwing gets.
    ''' </summary>
    Default Public ReadOnly Property Item(columnName As String) As Value
        Get
            Return Inner(columnName)
        End Get
    End Property

    ''' <summary>
    ''' Returns the value of the 1st column in the row.
    ''' </summary>
    Public ReadOnly Property Column1 As TCol1
        Get
            Return Inner.UnsafeGetTranslated(Of TCol1)(0)
        End Get
    End Property
    ''' <summary>
    ''' Returns the value of the 2nd column in the row.
    ''' </summary>
    Public ReadOnly Property Column2 As TCol2
        Get
            Return Inner.UnsafeGetTranslated(Of TCol2)(1)
        End Get
    End Property
    ''' <summary>
    ''' Returns the value of the 3rd column in the row.
    ''' </summary>
    Public ReadOnly Property Column3 As TCol3
        Get
            Return Inner.UnsafeGetTranslated(Of TCol3)(2)
        End Get
    End Property
    ''' <summary>
    ''' Returns the value of the 4th column in the row.
    ''' </summary>
    Public ReadOnly Property Column4 As TCol4
        Get
            Return Inner.UnsafeGetTranslated(Of TCol4)(3)
        End Get
    End Property
    ''' <summary>
    ''' Returns the value of the 5th column in the row.
    ''' </summary>
    Public ReadOnly Property Column5 As TCol5
        Get
            Return Inner.UnsafeGetTranslated(Of TCol5)(4)
        End Get
    End Property
    ''' <summary>
    ''' Returns the value of the 6th column in the row.
    ''' </summary>
    Public ReadOnly Property Column6 As TCol6
        Get
            Return Inner.UnsafeGetTranslated(Of TCol6)(5)
        End Get
    End Property
    ''' <summary>
    ''' Returns the value of the 7th column in the row.
    ''' </summary>
    Public ReadOnly Property Column7 As TCol7
        Get
            Return Inner.UnsafeGetTranslated(Of TCol7)(6)
        End Get
    End Property
    ''' <summary>
    ''' Returns the value of the 8th column in the row.
    ''' </summary>
    Public ReadOnly Property Column8 As TCol8
        Get
            Return Inner.UnsafeGetTranslated(Of TCol8)(7)
        End Get
    End Property

    Friend Sub New(inner As Row)
        Me.Inner = inner
    End Sub

    ''' <summary>
    ''' <see cref="Object.Equals(Object)"/>
    ''' </summary>
    Public Overrides Function Equals(obj As Object) As Boolean
        If Not (TypeOf obj Is TypedRowType6(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7, TCol8)) Then Return False

        Dim other = CType(obj, TypedRowType6(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7, TCol8))
        Return other.Inner.Equals(Inner)
    End Function

    ''' <summary>
    ''' <see cref="Object.GetHashCode"/>
    ''' </summary>
    Public Overrides Function GetHashCode() As Integer
        Return (((((((Inner.GetHashCode() * 17 + GetType(TCol1).GetHashCode()) * 17 + GetType(TCol2).GetHashCode()) * 17 + GetType(TCol3).GetHashCode()) * 17 + GetType(TCol4).GetHashCode()) * 17 + GetType(TCol5).GetHashCode()) * 17 + GetType(TCol6).GetHashCode()) * 17 + GetType(TCol7).GetHashCode()) * 17 + GetType(TCol8).GetHashCode()
    End Function

    ''' <summary>
    ''' <see cref="Object.ToString"/>
    ''' </summary>
    Public Overrides Function ToString() As String
        Return $"TypedRow<{GetType(TCol1).Name}, {GetType(TCol2).Name}, {GetType(TCol3).Name}, {GetType(TCol4).Name}, {GetType(TCol5).Name}, {GetType(TCol6).Name}, {GetType(TCol7).Name}, {GetType(TCol8).Name}> Index = {Index}"
    End Function

    ''' <summary>
    ''' <see cref="System.Collections.Generic.IEnumerable(Of T).GetEnumerator"/>
    ''' </summary>
    Public Function GetEnumerator() As TypedRowValueEnumerator
        Return New TypedRowValueEnumerator(Inner.GetEnumerator(), Length)
    End Function

    Private Function GetEnumerator15() As IEnumerator(Of Value) Implements IEnumerable(Of Value).GetEnumerator
        Return GetEnumerator()
    End Function

    Private Function GetEnumerator16() As IEnumerator Implements IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(Long,Value)"/>
    ''' </summary>
    Public Function TryGetValue(columnIndex As Long, <Out> ByRef value As Value) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnIndex, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(OfT)(Long,T)"/>
    ''' </summary>
    Public Function TryGetValue(Of T)(columnIndex As Long, <Out> ByRef value As T) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnIndex, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(String,Value)"/>
    ''' </summary>
    Public Function TryGetValue(columnName As String, <Out> ByRef value As Value) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnName, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.TryGetValue(OfT)(String,T)"/>
    ''' </summary>
    Public Function TryGetValue(Of T)(columnName As String, <Out> ByRef value As T) As Boolean Implements IRow.TryGetValue
        Return Inner.TryGetValue(columnName, value)
    End Function

    ''' <summary>
    ''' <see cref="Row.ToArray()"/>
    ''' </summary>
    Public Function ToArray() As Value() Implements IRow.ToArray
        Return Inner.ToArray()
    End Function

    ''' <summary>
    ''' <see cref="Row.GetRange(Long,Integer)"/>
    ''' </summary>
    Public Function GetRange(columnIndex As Long, length As Integer) As Value() Implements IRow.GetRange
        Return Inner.GetRange(columnIndex, length)
    End Function

    ''' <summary>
    ''' <see cref="Row.ToArray()"/>
    ''' </summary>
    Public Sub ToArray(ByRef array As Value()) Implements IRow.ToArray
        ToArray(array)
    End Sub

    ''' <summary>
    ''' <see cref="Row.GetRange(Long,Integer,)"/>
    ''' </summary>
    Public Sub GetRange(columnSourceIndex As Long, length As Integer, ByRef array As Value()) Implements IRow.GetRange
        GetRange(columnSourceIndex, length, array)
    End Sub

    ''' <summary>
    ''' <see cref="Row.GetRange(Long,Integer,,Integer)"/>
    ''' </summary>
    Public Sub GetRange(columnSourceIndex As Long, length As Integer, ByRef array As Value(), destinationIndex As Integer) Implements IRow.GetRange
        GetRange(columnSourceIndex, length, array, destinationIndex)
    End Sub

    ''' <summary>
    ''' Equivalent to <see cref="Row.ToArray()"/>
    ''' </summary>
    Public Shared Narrowing Operator CType(row As TypedRowType6(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7, TCol8)) As Value()
        Return row.ToArray()
    End Operator
End Class


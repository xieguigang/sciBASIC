#Region "Microsoft.VisualBasic::ff0f552a302a6021ea06fefb7f5562dd, Data\BinaryData\Feather\TypedDataFrame.vb"

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

    '   Total Lines: 945
    '    Code Lines: 592 (62.65%)
    ' Comment Lines: 237 (25.08%)
    '    - Xml Docs: 96.20%
    ' 
    '   Blank Lines: 116 (12.28%)
    '     File Size: 30.98 KB


    ' Class TypedRowMap
    ' 
    '     Properties: Count
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class TypedDataFrameBase
    ' 
    '     Properties: AllColumns, AllRows, Basis, ColumnCount, Columns
    '                 Inner, RowCount, Rows
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: TryGetRow, TryGetRowTranslated, (+4 Overloads) TryGetValue
    ' 
    ' Class TypedDataFrame
    ' 
    '     Properties: Column1
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: MapRow
    ' 
    ' Class TypedDataFrameType
    ' 
    '     Properties: Column1, Column2
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: MapRow
    ' 
    ' Class TypedDataFrameType1
    ' 
    '     Properties: Column1, Column2, Column3
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: MapRow
    ' 
    ' Class TypedDataFrameType2
    ' 
    '     Properties: Column1, Column2, Column3, Column4
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: MapRow
    ' 
    ' Class TypedDataFrameType3
    ' 
    '     Properties: Column1, Column2, Column3, Column4, Column5
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: MapRow
    ' 
    ' Class TypedDataFrameType4
    ' 
    '     Properties: Column1, Column2, Column3, Column4, Column5
    '                 Column6
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: MapRow
    ' 
    ' Class TypedDataFrameType5
    ' 
    '     Properties: Column1, Column2, Column3, Column4, Column5
    '                 Column6, Column7
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: MapRow
    ' 
    ' Class TypedDataFrameType6
    ' 
    '     Properties: Column1, Column2, Column3, Column4, Column5
    '                 Column6, Column7, Column8
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: MapRow
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.DataStorage.FeatherFormat.Impl

''' <summary>
''' Utility class for addressing a <see cref="TypedDataFrameBase(Of TRowType)"/> rows.
''' </summary>
Public Class TypedRowMap(Of TRow)
    Private Parent As TypedDataFrameBase(Of TRow)

    ''' <summary>
    ''' Number of rows in the dataframe
    ''' </summary>
    Public ReadOnly Property Count As Long
        Get
            Return Parent.RowCount
        End Get
    End Property

    ''' <summary>
    ''' Returns the row at the given index (in the dataframe's basis).
    ''' 
    ''' Throws if the index is out of range.
    ''' </summary>
    Default Public ReadOnly Property Item(index As Long) As TRow
        Get
            Dim dynRow = Parent.Inner.Rows(index)

            Return Parent.MapRow(dynRow)
        End Get
    End Property

    Friend Sub New(parent As TypedDataFrameBase(Of TRow))
        Me.Parent = parent
    End Sub
End Class

''' <summary>
''' Represents a dataframe, where each column has been typed.
''' 
''' Is backed by a <see cref="DataFrame"/>, and will become invalid when that dataframe is disposed.
''' </summary>
Public MustInherit Class TypedDataFrameBase(Of TRowType)
    Implements IDataFrame

    ''' <summary>
    ''' The backing <see cref="DataFrame"/>
    ''' </summary>
    Dim _Inner As DataFrame

    ''' <summary>
    ''' An enumerable of all the rows in this DataFrame.
    ''' </summary>
    Dim _AllRows As TypedRowEnumerable(Of TRowType)

    ''' <summary>
    ''' A utility accessor for rows in this DataFrame.
    ''' </summary>
    Dim _Rows As TypedRowMap(Of TRowType)

    Public Property Inner As DataFrame
        Get
            Return _Inner
        End Get
        Private Set(value As DataFrame)
            _Inner = value
        End Set
    End Property

    ''' <summary>
    ''' Number of rows in the DataFrame.
    ''' </summary>
    Public ReadOnly Property RowCount As Long Implements IDataFrame.RowCount
        Get
            Return Inner.RowCount
        End Get
    End Property
    ''' <summary>
    ''' Number of columns in the DataFrame.
    ''' </summary>
    Public ReadOnly Property ColumnCount As Long Implements IDataFrame.ColumnCount
        Get
            Return Inner.ColumnCount
        End Get
    End Property

    ''' <summary>
    ''' Whether this DataFrame is addressable with base-0 or base-1 indexes.
    ''' </summary>
    Public ReadOnly Property Basis As BasisType Implements IDataFrame.Basis
        Get
            Return Inner.Basis
        End Get
    End Property

    Public Property AllRows As TypedRowEnumerable(Of TRowType)
        Get
            Return _AllRows
        End Get
        Private Set(value As TypedRowEnumerable(Of TRowType))
            _AllRows = value
        End Set
    End Property
    ''' <summary>
    ''' An enumerable of all the columns in this DataFrame.
    ''' </summary>
    Public ReadOnly Property AllColumns As ColumnEnumerable
        Get
            Return Inner.AllColumns
        End Get
    End Property

    ''' <summary>
    ''' A utility accessor for columns in this DataFrame.
    ''' </summary>
    Public ReadOnly Property Columns As ColumnMap
        Get
            Return Inner.Columns
        End Get
    End Property

    Public Property Rows As TypedRowMap(Of TRowType)
        Get
            Return _Rows
        End Get
        Private Set(value As TypedRowMap(Of TRowType))
            _Rows = value
        End Set
    End Property

    ''' <summary>
    ''' Return the value at the given row and column indexes.
    ''' 
    ''' Will throw if the index is out of bounds.  Use <see cref="TryGetValue"/> for non-throwing gets.
    ''' </summary>
    Default Public ReadOnly Property Item(rowIndex As Long, columnIndex As Long) As Value Implements IDataFrame.Item
        Get
            Return Inner(rowIndex, columnIndex)
        End Get
    End Property

    ''' <summary>
    ''' Return the value at the given row index in the column with the given name.
    ''' 
    ''' Will throw if the index is out of bounds or the column is not found.  Use <see cref="TryGetValue"/> for non-throwing gets.
    ''' </summary>
    Default Public ReadOnly Property Item(rowIndex As Long, columnName As String) As Value Implements IDataFrame.Item
        Get
            Return Inner(rowIndex, columnName)
        End Get
    End Property

    ''' <summary>
    ''' Creates a new TypedDateFrameBase
    ''' </summary>
    Protected Sub New(inner As DataFrame)
        Me.Inner = inner

        AllRows = New TypedRowEnumerable(Of TRowType)(Me)
        Rows = New TypedRowMap(Of TRowType)(Me)
    End Sub

    ''' <summary>
    ''' Sets value to the value at the row and column indexes passed in.
    ''' 
    ''' If the passed indexes are out of bounds false is returned.  Otherwise, true is returned;
    ''' </summary>
    Public Function TryGetValue(rowIndex As Long, columnIndex As Long, <Out> ByRef value As Value) As Boolean Implements IDataFrame.TryGetValue
        Return Inner.TryGetValue(rowIndex, columnIndex, value)
    End Function

    ''' <summary>
    ''' Sets value to the value, coerced to the appropriate type, at the row and column indexes passed in.
    ''' 
    ''' If the passed indexes are out of bounds, or the value cannot be coerced, false is returned.  Otherwise, true is returned;
    ''' </summary>
    Public Function TryGetValue(Of T)(rowIndex As Long, columnIndex As Long, <Out> ByRef value As T) As Boolean Implements IDataFrame.TryGetValue
        Return Inner.TryGetValue(rowIndex, columnIndex, value)
    End Function

    ''' <summary>
    ''' Sets value to the value at the row given row index in the column with the given name.
    ''' 
    ''' If the passed index is out of bounds or no column with the given name exists, false is returned.  Otherwise, true is returned;
    ''' </summary>
    Public Function TryGetValue(rowIndex As Long, columnName As String, <Out> ByRef value As Value) As Boolean Implements IDataFrame.TryGetValue
        Return Inner.TryGetValue(rowIndex, columnName, value)
    End Function

    ''' <summary>
    ''' Sets value to the value, coerced to the appropriate type, at the given row index in the column with the given name.
    ''' 
    ''' If the passed index is out of bounds, no column with the given name exists, or the value cannot be coerced then false is returned.  Otherwise, true is returned;
    ''' </summary>
    Public Function TryGetValue(Of T)(rowIndex As Long, columnName As String, <Out> ByRef value As T) As Boolean Implements IDataFrame.TryGetValue
        Return Inner.TryGetValue(rowIndex, columnName, value)
    End Function

    ''' <summary>
    ''' Sets row to the row at the given index.
    ''' 
    ''' Returns true if a row exists at that index, and false otherwise.
    ''' </summary>
    Public Function TryGetRow(rowIndex As Long, <Out> ByRef row As TRowType) As Boolean
        Dim translated = Inner.TranslateIndex(rowIndex)
        Return TryGetRowTranslated(translated, row)
    End Function

    Friend Function TryGetRowTranslated(translatedRowIndex As Long, <Out> ByRef row As TRowType) As Boolean
        Dim dynRow As Row = Nothing

        If Not Inner.TryGetRowTranslated(translatedRowIndex, dynRow) Then
            row = Nothing
            Return False
        End If

        row = MapRow(dynRow)
        Return True
    End Function

    ''' <summary>
    ''' Maps an untyped row to TRowType.
    ''' </summary>
    Protected Friend MustOverride Function MapRow(row As Row) As TRowType
End Class

''' <summary>
''' Represents a dataframe with one typed column.
''' </summary>
Public NotInheritable Class TypedDataFrame(Of TCol1)
    Inherits TypedDataFrameBase(Of TypedRow(Of TCol1))
    ''' <summary>
    ''' The first column in the dataframe.
    ''' </summary>
    Private _Column1 As TypedColumn(Of TCol1)

    Public Property Column1 As TypedColumn(Of TCol1)
        Get
            Return _Column1
        End Get
        Private Set(value As TypedColumn(Of TCol1))
            _Column1 = value
        End Set
    End Property

    Friend Sub New(inner As DataFrame)
        MyBase.New(inner)
        Dim inner1 As Column = Nothing
        inner.TryGetColumnTranslated(0, inner1)
        Column1 = New TypedColumn(Of TCol1)(inner1)
    End Sub

    ''' <summary>
    ''' <see cref="TypedDataFrameBase(Of TRowType).MapRow(Row)"/>
    ''' </summary>
    Protected Friend Overrides Function MapRow(row As Row) As TypedRow(Of TCol1)
        Return New TypedRow(Of TCol1)(row)
    End Function
End Class

''' <summary>
''' Represents a dataframe with two typed columns.
''' </summary>
Public NotInheritable Class TypedDataFrameType(Of TCol1, TCol2)
    Inherits TypedDataFrameBase(Of TypedRowType(Of TCol1, TCol2))

    Dim _Column1 As TypedColumn(Of TCol1)
    Dim _Column2 As TypedColumn(Of TCol2)

    ''' <summary>
    ''' The first column in the dataframe.
    ''' </summary>
    Public Property Column1 As TypedColumn(Of TCol1)
        Get
            Return _Column1
        End Get
        Private Set(value As TypedColumn(Of TCol1))
            _Column1 = value
        End Set
    End Property

    ''' <summary>
    ''' The second column in the dataframe.
    ''' </summary>
    Public Property Column2 As TypedColumn(Of TCol2)
        Get
            Return _Column2
        End Get
        Private Set(value As TypedColumn(Of TCol2))
            _Column2 = value
        End Set
    End Property

    Friend Sub New(inner As DataFrame)
        MyBase.New(inner)
        Dim inner1 As Column = Nothing
        inner.TryGetColumnTranslated(0, inner1)
        Column1 = New TypedColumn(Of TCol1)(inner1)

        Dim inner2 As Column = Nothing
        inner.TryGetColumnTranslated(1, inner2)
        Column2 = New TypedColumn(Of TCol2)(inner2)
    End Sub

    ''' <summary>
    ''' <see cref="TypedDataFrameBase(Of TRowType).MapRow(Row)"/>
    ''' </summary>
    Protected Friend Overrides Function MapRow(row As Row) As TypedRowType(Of TCol1, TCol2)
        Return New TypedRowType(Of TCol1, TCol2)(row)
    End Function
End Class

''' <summary>
''' Represents a dataframe with three typed columns.
''' </summary>
Public NotInheritable Class TypedDataFrameType1(Of TCol1, TCol2, TCol3)
    Inherits TypedDataFrameBase(Of TypedRowType1(Of TCol1, TCol2, TCol3))

    Private _Column1 As TypedColumn(Of TCol1), _Column2 As TypedColumn(Of TCol2), _Column3 As TypedColumn(Of TCol3)

    ''' <summary>
    ''' The first column in the dataframe.
    ''' </summary>
    Public Property Column1 As TypedColumn(Of TCol1)
        Get
            Return _Column1
        End Get
        Private Set(value As TypedColumn(Of TCol1))
            _Column1 = value
        End Set
    End Property

    ''' <summary>
    ''' The second column in the dataframe.
    ''' </summary>
    Public Property Column2 As TypedColumn(Of TCol2)
        Get
            Return _Column2
        End Get
        Private Set(value As TypedColumn(Of TCol2))
            _Column2 = value
        End Set
    End Property

    ''' <summary>
    ''' The third column in the dataframe.
    ''' </summary>
    Public Property Column3 As TypedColumn(Of TCol3)
        Get
            Return _Column3
        End Get
        Private Set(value As TypedColumn(Of TCol3))
            _Column3 = value
        End Set
    End Property

    Friend Sub New(inner As DataFrame)
        MyBase.New(inner)
        Dim inner1 As Column = Nothing
        inner.TryGetColumnTranslated(0, inner1)
        Column1 = New TypedColumn(Of TCol1)(inner1)

        Dim inner2 As Column = Nothing
        inner.TryGetColumnTranslated(1, inner2)
        Column2 = New TypedColumn(Of TCol2)(inner2)

        Dim inner3 As Column = Nothing
        inner.TryGetColumnTranslated(2, inner3)
        Column3 = New TypedColumn(Of TCol3)(inner3)
    End Sub

    ''' <summary>
    ''' <see cref="TypedDataFrameBase(Of TRowType).MapRow(Row)"/>
    ''' </summary>
    Protected Friend Overrides Function MapRow(row As Row) As TypedRowType1(Of TCol1, TCol2, TCol3)
        Return New TypedRowType1(Of TCol1, TCol2, TCol3)(row)
    End Function
End Class

''' <summary>
''' Represents a dataframe with four typed columns.
''' </summary>
Public NotInheritable Class TypedDataFrameType2(Of TCol1, TCol2, TCol3, TCol4)
    Inherits TypedDataFrameBase(Of TypedRowType2(Of TCol1, TCol2, TCol3, TCol4))

    Private _Column1 As TypedColumn(Of TCol1),
        _Column2 As TypedColumn(Of TCol2),
        _Column3 As TypedColumn(Of TCol3),
        _Column4 As TypedColumn(Of TCol4)

    ''' <summary>
    ''' The first column in the dataframe.
    ''' </summary>
    Public Property Column1 As TypedColumn(Of TCol1)
        Get
            Return _Column1
        End Get
        Private Set(value As TypedColumn(Of TCol1))
            _Column1 = value
        End Set
    End Property

    ''' <summary>
    ''' The second column in the dataframe.
    ''' </summary>
    Public Property Column2 As TypedColumn(Of TCol2)
        Get
            Return _Column2
        End Get
        Private Set(value As TypedColumn(Of TCol2))
            _Column2 = value
        End Set
    End Property

    ''' <summary>
    ''' The third column in the dataframe.
    ''' </summary>
    Public Property Column3 As TypedColumn(Of TCol3)
        Get
            Return _Column3
        End Get
        Private Set(value As TypedColumn(Of TCol3))
            _Column3 = value
        End Set
    End Property

    ''' <summary>
    ''' The fourth column in the dataframe.
    ''' </summary>
    Public Property Column4 As TypedColumn(Of TCol4)
        Get
            Return _Column4
        End Get
        Private Set(value As TypedColumn(Of TCol4))
            _Column4 = value
        End Set
    End Property

    Friend Sub New(inner As DataFrame)
        MyBase.New(inner)
        Dim inner1 As Column = Nothing
        inner.TryGetColumnTranslated(0, inner1)
        Column1 = New TypedColumn(Of TCol1)(inner1)

        Dim inner2 As Column = Nothing
        inner.TryGetColumnTranslated(1, inner2)
        Column2 = New TypedColumn(Of TCol2)(inner2)

        Dim inner3 As Column = Nothing
        inner.TryGetColumnTranslated(2, inner3)
        Column3 = New TypedColumn(Of TCol3)(inner3)

        Dim inner4 As Column = Nothing
        inner.TryGetColumnTranslated(3, inner4)
        Column4 = New TypedColumn(Of TCol4)(inner4)
    End Sub

    ''' <summary>
    ''' <see cref="TypedDataFrameBase(Of TRowType).MapRow(Row)"/>
    ''' </summary>
    Protected Friend Overrides Function MapRow(row As Row) As TypedRowType2(Of TCol1, TCol2, TCol3, TCol4)
        Return New TypedRowType2(Of TCol1, TCol2, TCol3, TCol4)(row)
    End Function
End Class

''' <summary>
''' Represents a dataframe with five typed columns.
''' </summary>
Public NotInheritable Class TypedDataFrameType3(Of TCol1, TCol2, TCol3, TCol4, TCol5)
    Inherits TypedDataFrameBase(Of TypedRowType3(Of TCol1, TCol2, TCol3, TCol4, TCol5))

    Private _Column1 As TypedColumn(Of TCol1),
        _Column2 As TypedColumn(Of TCol2),
        _Column3 As TypedColumn(Of TCol3),
        _Column4 As TypedColumn(Of TCol4),
        _Column5 As TypedColumn(Of TCol5)

    ''' <summary>
    ''' The first column in the dataframe.
    ''' </summary>
    Public Property Column1 As TypedColumn(Of TCol1)
        Get
            Return _Column1
        End Get
        Private Set(value As TypedColumn(Of TCol1))
            _Column1 = value
        End Set
    End Property

    ''' <summary>
    ''' The second column in the dataframe.
    ''' </summary>
    Public Property Column2 As TypedColumn(Of TCol2)
        Get
            Return _Column2
        End Get
        Private Set(value As TypedColumn(Of TCol2))
            _Column2 = value
        End Set
    End Property

    ''' <summary>
    ''' The third column in the dataframe.
    ''' </summary>
    Public Property Column3 As TypedColumn(Of TCol3)
        Get
            Return _Column3
        End Get
        Private Set(value As TypedColumn(Of TCol3))
            _Column3 = value
        End Set
    End Property

    ''' <summary>
    ''' The fourth column in the dataframe.
    ''' </summary>
    Public Property Column4 As TypedColumn(Of TCol4)
        Get
            Return _Column4
        End Get
        Private Set(value As TypedColumn(Of TCol4))
            _Column4 = value
        End Set
    End Property

    ''' <summary>
    ''' The fifth column in the dataframe.
    ''' </summary>
    Public Property Column5 As TypedColumn(Of TCol5)
        Get
            Return _Column5
        End Get
        Private Set(value As TypedColumn(Of TCol5))
            _Column5 = value
        End Set
    End Property

    Friend Sub New(inner As DataFrame)
        MyBase.New(inner)
        Dim inner1 As Column = Nothing
        inner.TryGetColumnTranslated(0, inner1)
        Column1 = New TypedColumn(Of TCol1)(inner1)

        Dim inner2 As Column = Nothing
        inner.TryGetColumnTranslated(1, inner2)
        Column2 = New TypedColumn(Of TCol2)(inner2)

        Dim inner3 As Column = Nothing
        inner.TryGetColumnTranslated(2, inner3)
        Column3 = New TypedColumn(Of TCol3)(inner3)

        Dim inner4 As Column = Nothing
        inner.TryGetColumnTranslated(3, inner4)
        Column4 = New TypedColumn(Of TCol4)(inner4)

        Dim inner5 As Column = Nothing
        inner.TryGetColumnTranslated(4, inner5)
        Column5 = New TypedColumn(Of TCol5)(inner5)
    End Sub

    ''' <summary>
    ''' <see cref="TypedDataFrameBase(Of TRowType).MapRow(Row)"/>
    ''' </summary>
    Protected Friend Overrides Function MapRow(row As Row) As TypedRowType3(Of TCol1, TCol2, TCol3, TCol4, TCol5)
        Return New TypedRowType3(Of TCol1, TCol2, TCol3, TCol4, TCol5)(row)
    End Function
End Class

''' <summary>
''' Represents a dataframe with six typed columns.
''' </summary>
Public NotInheritable Class TypedDataFrameType4(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6)
    Inherits TypedDataFrameBase(Of TypedRowType4(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6))

    Dim _Column1 As TypedColumn(Of TCol1)
    Dim _Column2 As TypedColumn(Of TCol2)
    Dim _Column3 As TypedColumn(Of TCol3)
    Dim _Column4 As TypedColumn(Of TCol4)
    Dim _Column5 As TypedColumn(Of TCol5)
    Dim _Column6 As TypedColumn(Of TCol6)

    ''' <summary>
    ''' The first column in the dataframe.
    ''' </summary>
    Public Property Column1 As TypedColumn(Of TCol1)
        Get
            Return _Column1
        End Get
        Private Set(value As TypedColumn(Of TCol1))
            _Column1 = value
        End Set
    End Property

    ''' <summary>
    ''' The second column in the dataframe.
    ''' </summary>
    Public Property Column2 As TypedColumn(Of TCol2)
        Get
            Return _Column2
        End Get
        Private Set(value As TypedColumn(Of TCol2))
            _Column2 = value
        End Set
    End Property

    ''' <summary>
    ''' The third column in the dataframe.
    ''' </summary>
    Public Property Column3 As TypedColumn(Of TCol3)
        Get
            Return _Column3
        End Get
        Private Set(value As TypedColumn(Of TCol3))
            _Column3 = value
        End Set
    End Property

    ''' <summary>
    ''' The fourth column in the dataframe.
    ''' </summary>
    Public Property Column4 As TypedColumn(Of TCol4)
        Get
            Return _Column4
        End Get
        Private Set(value As TypedColumn(Of TCol4))
            _Column4 = value
        End Set
    End Property

    ''' <summary>
    ''' The fifth column in the dataframe.
    ''' </summary>
    Public Property Column5 As TypedColumn(Of TCol5)
        Get
            Return _Column5
        End Get
        Private Set(value As TypedColumn(Of TCol5))
            _Column5 = value
        End Set
    End Property

    ''' <summary>
    ''' The sixth column in the dataframe.
    ''' </summary>
    Public Property Column6 As TypedColumn(Of TCol6)
        Get
            Return _Column6
        End Get
        Private Set(value As TypedColumn(Of TCol6))
            _Column6 = value
        End Set
    End Property

    Friend Sub New(inner As DataFrame)
        MyBase.New(inner)
        Dim inner1 As Column = Nothing
        inner.TryGetColumnTranslated(0, inner1)
        Column1 = New TypedColumn(Of TCol1)(inner1)

        Dim inner2 As Column = Nothing
        inner.TryGetColumnTranslated(1, inner2)
        Column2 = New TypedColumn(Of TCol2)(inner2)

        Dim inner3 As Column = Nothing
        inner.TryGetColumnTranslated(2, inner3)
        Column3 = New TypedColumn(Of TCol3)(inner3)

        Dim inner4 As Column = Nothing
        inner.TryGetColumnTranslated(3, inner4)
        Column4 = New TypedColumn(Of TCol4)(inner4)

        Dim inner5 As Column = Nothing
        inner.TryGetColumnTranslated(4, inner5)
        Column5 = New TypedColumn(Of TCol5)(inner5)

        Dim inner6 As Column = Nothing
        inner.TryGetColumnTranslated(5, inner6)
        Column6 = New TypedColumn(Of TCol6)(inner6)
    End Sub

    ''' <summary>
    ''' <see cref="TypedDataFrameBase(Of TRowType).MapRow(Row)"/>
    ''' </summary>
    Protected Friend Overrides Function MapRow(row As Row) As TypedRowType4(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6)
        Return New TypedRowType4(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6)(row)
    End Function
End Class

''' <summary>
''' Represents a dataframe with seven typed columns.
''' </summary>
Public NotInheritable Class TypedDataFrameType5(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7)
    Inherits TypedDataFrameBase(Of TypedRowType5(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7))

    Private _Column1 As TypedColumn(Of TCol1),
        _Column2 As TypedColumn(Of TCol2),
        _Column3 As TypedColumn(Of TCol3),
        _Column4 As TypedColumn(Of TCol4),
        _Column5 As TypedColumn(Of TCol5),
        _Column6 As TypedColumn(Of TCol6),
        _Column7 As TypedColumn(Of TCol7)

    ''' <summary>
    ''' The first column in the dataframe.
    ''' </summary>
    Public Property Column1 As TypedColumn(Of TCol1)
        Get
            Return _Column1
        End Get
        Private Set(value As TypedColumn(Of TCol1))
            _Column1 = value
        End Set
    End Property

    ''' <summary>
    ''' The second column in the dataframe.
    ''' </summary>
    Public Property Column2 As TypedColumn(Of TCol2)
        Get
            Return _Column2
        End Get
        Private Set(value As TypedColumn(Of TCol2))
            _Column2 = value
        End Set
    End Property

    ''' <summary>
    ''' The third column in the dataframe.
    ''' </summary>
    Public Property Column3 As TypedColumn(Of TCol3)
        Get
            Return _Column3
        End Get
        Private Set(value As TypedColumn(Of TCol3))
            _Column3 = value
        End Set
    End Property

    ''' <summary>
    ''' The fourth column in the dataframe.
    ''' </summary>
    Public Property Column4 As TypedColumn(Of TCol4)
        Get
            Return _Column4
        End Get
        Private Set(value As TypedColumn(Of TCol4))
            _Column4 = value
        End Set
    End Property

    ''' <summary>
    ''' The fifth column in the dataframe.
    ''' </summary>
    Public Property Column5 As TypedColumn(Of TCol5)
        Get
            Return _Column5
        End Get
        Private Set(value As TypedColumn(Of TCol5))
            _Column5 = value
        End Set
    End Property

    ''' <summary>
    ''' The sixth column in the dataframe.
    ''' </summary>
    Public Property Column6 As TypedColumn(Of TCol6)
        Get
            Return _Column6
        End Get
        Private Set(value As TypedColumn(Of TCol6))
            _Column6 = value
        End Set
    End Property

    ''' <summary>
    ''' The seventh column in the dataframe.
    ''' </summary>
    Public Property Column7 As TypedColumn(Of TCol7)
        Get
            Return _Column7
        End Get
        Private Set(value As TypedColumn(Of TCol7))
            _Column7 = value
        End Set
    End Property

    Friend Sub New(inner As DataFrame)
        MyBase.New(inner)
        Dim inner1 As Column = Nothing
        inner.TryGetColumnTranslated(0, inner1)
        Column1 = New TypedColumn(Of TCol1)(inner1)

        Dim inner2 As Column = Nothing
        inner.TryGetColumnTranslated(1, inner2)
        Column2 = New TypedColumn(Of TCol2)(inner2)

        Dim inner3 As Column = Nothing
        inner.TryGetColumnTranslated(2, inner3)
        Column3 = New TypedColumn(Of TCol3)(inner3)

        Dim inner4 As Column = Nothing
        inner.TryGetColumnTranslated(3, inner4)
        Column4 = New TypedColumn(Of TCol4)(inner4)

        Dim inner5 As Column = Nothing
        inner.TryGetColumnTranslated(4, inner5)
        Column5 = New TypedColumn(Of TCol5)(inner5)

        Dim inner6 As Column = Nothing
        inner.TryGetColumnTranslated(5, inner6)
        Column6 = New TypedColumn(Of TCol6)(inner6)

        Dim inner7 As Column = Nothing
        inner.TryGetColumnTranslated(6, inner7)
        Column7 = New TypedColumn(Of TCol7)(inner7)
    End Sub

    ''' <summary>
    ''' <see cref="TypedDataFrameBase(Of TRowType).MapRow(Row)"/>
    ''' </summary>
    Protected Friend Overrides Function MapRow(row As Row) As TypedRowType5(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7)
        Return New TypedRowType5(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7)(row)
    End Function
End Class

''' <summary>
''' Represents a dataframe with eight typed columns.
''' </summary>
Public NotInheritable Class TypedDataFrameType6(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7, TCol8)
    Inherits TypedDataFrameBase(Of TypedRowType6(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7, TCol8))

    Private _Column1 As TypedColumn(Of TCol1),
        _Column2 As TypedColumn(Of TCol2),
        _Column3 As TypedColumn(Of TCol3),
        _Column4 As TypedColumn(Of TCol4),
        _Column5 As TypedColumn(Of TCol5),
        _Column6 As TypedColumn(Of TCol6),
        _Column7 As TypedColumn(Of TCol7),
        _Column8 As TypedColumn(Of TCol8)

    ''' <summary>
    ''' The first column in the dataframe.
    ''' </summary>
    Public Property Column1 As TypedColumn(Of TCol1)
        Get
            Return _Column1
        End Get
        Private Set(value As TypedColumn(Of TCol1))
            _Column1 = value
        End Set
    End Property

    ''' <summary>
    ''' The second column in the dataframe.
    ''' </summary>
    Public Property Column2 As TypedColumn(Of TCol2)
        Get
            Return _Column2
        End Get
        Private Set(value As TypedColumn(Of TCol2))
            _Column2 = value
        End Set
    End Property

    ''' <summary>
    ''' The third column in the dataframe.
    ''' </summary>
    Public Property Column3 As TypedColumn(Of TCol3)
        Get
            Return _Column3
        End Get
        Private Set(value As TypedColumn(Of TCol3))
            _Column3 = value
        End Set
    End Property

    ''' <summary>
    ''' The fourth column in the dataframe.
    ''' </summary>
    Public Property Column4 As TypedColumn(Of TCol4)
        Get
            Return _Column4
        End Get
        Private Set(value As TypedColumn(Of TCol4))
            _Column4 = value
        End Set
    End Property

    ''' <summary>
    ''' The fifth column in the dataframe.
    ''' </summary>
    Public Property Column5 As TypedColumn(Of TCol5)
        Get
            Return _Column5
        End Get
        Private Set(value As TypedColumn(Of TCol5))
            _Column5 = value
        End Set
    End Property

    ''' <summary>
    ''' The sixth column in the dataframe.
    ''' </summary>
    Public Property Column6 As TypedColumn(Of TCol6)
        Get
            Return _Column6
        End Get
        Private Set(value As TypedColumn(Of TCol6))
            _Column6 = value
        End Set
    End Property

    ''' <summary>
    ''' The seventh column in the dataframe.
    ''' </summary>
    Public Property Column7 As TypedColumn(Of TCol7)
        Get
            Return _Column7
        End Get
        Private Set(value As TypedColumn(Of TCol7))
            _Column7 = value
        End Set
    End Property

    ''' <summary>
    ''' The eigth column in the dataframe.
    ''' </summary>
    Public Property Column8 As TypedColumn(Of TCol8)
        Get
            Return _Column8
        End Get
        Private Set(value As TypedColumn(Of TCol8))
            _Column8 = value
        End Set
    End Property

    Friend Sub New(inner As DataFrame)
        MyBase.New(inner)
        Dim inner1 As Column = Nothing
        inner.TryGetColumnTranslated(0, inner1)
        Column1 = New TypedColumn(Of TCol1)(inner1)

        Dim inner2 As Column = Nothing
        inner.TryGetColumnTranslated(1, inner2)
        Column2 = New TypedColumn(Of TCol2)(inner2)

        Dim inner3 As Column = Nothing
        inner.TryGetColumnTranslated(2, inner3)
        Column3 = New TypedColumn(Of TCol3)(inner3)

        Dim inner4 As Column = Nothing
        inner.TryGetColumnTranslated(3, inner4)
        Column4 = New TypedColumn(Of TCol4)(inner4)

        Dim inner5 As Column = Nothing
        inner.TryGetColumnTranslated(4, inner5)
        Column5 = New TypedColumn(Of TCol5)(inner5)

        Dim inner6 As Column = Nothing
        inner.TryGetColumnTranslated(5, inner6)
        Column6 = New TypedColumn(Of TCol6)(inner6)

        Dim inner7 As Column = Nothing
        inner.TryGetColumnTranslated(6, inner7)
        Column7 = New TypedColumn(Of TCol7)(inner7)

        Dim inner8 As Column = Nothing
        inner.TryGetColumnTranslated(7, inner8)
        Column8 = New TypedColumn(Of TCol8)(inner8)
    End Sub

    ''' <summary>
    ''' <see cref="TypedDataFrameBase(Of TRowType).MapRow(Row)"/>
    ''' </summary>
    Protected Friend Overrides Function MapRow(row As Row) As TypedRowType6(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7, TCol8)
        Return New TypedRowType6(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7, TCol8)(row)
    End Function
End Class

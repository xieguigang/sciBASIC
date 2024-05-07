Imports System
Imports FeatherDotNet.Impl
Imports System.Runtime.InteropServices

Namespace FeatherDotNet
    ''' <summary>
    ''' Utility class for addressing a <seecref="ProxyDataFrame(OfTProxyType)"/>'s rows.
    ''' </summary>
    Public Class ProxyRowMap(Of TProxyType)
        Private Parent As ProxyDataFrame(Of TProxyType)

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
        Default Public ReadOnly Property Item(index As Long) As TProxyType
            Get
                Dim raw = Parent.Inner.Rows(index)
                Return Parent.ProxyRow(raw)
            End Get
        End Property

        Friend Sub New(parent As ProxyDataFrame(Of TProxyType))
            Me.Parent = parent
        End Sub
    End Class

    ''' <summary>
    ''' Represents a dataframe, where each row has been mapped to an instance of a type.
    ''' 
    ''' Is backed by a <seecref="DataFrame"/>, and will become invalid when that dataframe is disposed.
    ''' </summary>
    Public NotInheritable Class ProxyDataFrame(Of TProxyType)
        Implements IDataFrame

        ''' <summary>
        ''' The backing <seecref="DataFrame"/>
        ''' </summary>
                ''' <summary>
        ''' An enumerable of all the rows in this DataFrame.
        ''' </summary>
                ''' <summary>
        ''' A utility accessor for rows in this DataFrame.
        ''' </summary>
        Private _Inner As FeatherDotNet.DataFrame, _AllRows As FeatherDotNet.ProxyRowEnumerable(Of TProxyType), _Rows As FeatherDotNet.ProxyRowMap(Of TProxyType)
        Private ReadOnly Mapper As Func(Of Row, TProxyType, TProxyType)
        Private ReadOnly Factory As Func(Of TProxyType)

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

        ''' <summary>
        ''' An enumerable of all the columns in this DataFrame.
        ''' </summary>
        Public ReadOnly Property AllColumns As ColumnEnumerable
            Get
                Return Inner.AllColumns
            End Get
        End Property

        Public Property AllRows As ProxyRowEnumerable(Of TProxyType)
            Get
                Return _AllRows
            End Get
            Private Set(value As ProxyRowEnumerable(Of TProxyType))
                _AllRows = value
            End Set
        End Property

        ''' <summary>
        ''' A utility accessor for columns in this DataFrame.
        ''' </summary>
        Public ReadOnly Property Columns As ColumnMap
            Get
                Return Inner.Columns
            End Get
        End Property

        Public Property Rows As ProxyRowMap(Of TProxyType)
            Get
                Return _Rows
            End Get
            Private Set(value As ProxyRowMap(Of TProxyType))
                _Rows = value
            End Set
        End Property

        ''' <summary>
        ''' Return the row at the given index.
        ''' 
        ''' Will throw if the index is out of bounds.  Use <seecref="TryGetRow(Long,TProxyType)"/> for non-throwing gets.
        ''' </summary>
        Default Public ReadOnly Property Item(rowIndex As Long) As TProxyType
            Get
                Return Rows(rowIndex)
            End Get
        End Property

        ''' <summary>
        ''' Return the column with the given name.
        ''' 
        ''' Will throw if the name is not found.  Use <seecref="TryGetColumn(String,Column)"/> for non-throwing gets.
        ''' </summary>
        Default Public ReadOnly Property Item(columnName As String) As Column
            Get
                Return Inner(columnName)
            End Get
        End Property

        ''' <summary>
        ''' Return the value at the given row and column indexes.
        ''' 
        ''' Will throw if the index is out of bounds.  Use <seecref="TryGetValue(Long,Long,Value)"/> for non-throwing gets.
        ''' </summary>
        Default Public ReadOnly Property Item(rowIndex As Long, columnIndex As Long) As Value Implements IDataFrame.Item
            Get
                Return Inner(rowIndex, columnIndex)
            End Get
        End Property

        ''' <summary>
        ''' Return the value at the given row index in the column with the given name.
        ''' 
        ''' Will throw if the index is out of bounds or the column is not found.  Use <seecref="TryGetValue(Long,String,Value)"/> for non-throwing gets.
        ''' </summary>
        Default Public ReadOnly Property Item(rowIndex As Long, columnName As String) As Value Implements IDataFrame.Item
            Get
                Return Inner(rowIndex, columnName)
            End Get
        End Property

        Friend Sub New(inner As DataFrame, mapper As Func(Of Row, TProxyType, TProxyType), factory As Func(Of TProxyType))
            Me.Inner = inner
            Me.Mapper = mapper
            Me.Factory = factory

            AllRows = New ProxyRowEnumerable(Of TProxyType)(Me)
            Rows = New ProxyRowMap(Of TProxyType)(Me)
        End Sub

        ''' <summary>
        ''' Sets column to the column at the given index.
        ''' 
        ''' Returns true if a column exists at that index, and false otherwise.
        ''' </summary>
        Public Function TryGetColumn(index As Long, <Out> ByRef column As Column) As Boolean
            Return Inner.TryGetColumn(index, column)
        End Function

        ''' <summary>
        ''' Sets column to the column with the given name.
        ''' 
        ''' Returns true if a column exists with that name, and false otherwise.
        ''' </summary>
        Public Function TryGetColumn(columnName As String, <Out> ByRef column As Column) As Boolean
            Return Inner.TryGetColumn(columnName, column)
        End Function

        ''' <summary>
        ''' Sets row to the row at the given index.
        ''' 
        ''' Returns true if a row exists at that index, and false otherwise.
        ''' </summary>
        Public Function TryGetRow(index As Long, <Out> ByRef row As TProxyType) As Boolean
            Dim translatedRowIndex = Inner.TranslateIndex(index)

            Return TryGetRowTranslated(translatedRowIndex, row)
        End Function

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

        Friend Function TryGetRowTranslated(translatedRowIndex As Long, <Out> ByRef row As TProxyType) As Boolean
            Dim rawRow As Row
            If Not Inner.TryGetRowTranslated(translatedRowIndex, rawRow) Then
                row = Nothing
                Return False
            End If

            row = ProxyRow(rawRow)
            Return True
        End Function

        Friend Function ProxyRow(row As Row) As TProxyType
            Dim toRet = Factory()
            Return Mapper(row, toRet)
        End Function
    End Class
End Namespace

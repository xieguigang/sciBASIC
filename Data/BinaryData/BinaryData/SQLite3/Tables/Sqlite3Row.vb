#Region "Microsoft.VisualBasic::9faf9e3e6de9036dababf6420bd15464, Data\BinaryData\BinaryData\SQLite3\Tables\Sqlite3Row.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class Sqlite3Row
    ' 
    '         Properties: ColumnData, ReadIndex, RowId, Table
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString, (+2 Overloads) TryGetOrdinal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ManagedSqlite.Core.Tables

    Public Class Sqlite3Row

        Public ReadOnly Property Table As Sqlite3Table
        Public ReadOnly Property RowId As Long
        Public ReadOnly Property ReadIndex As Long
        Public ReadOnly Property ColumnData As Object()

        Default Public ReadOnly Property Item(field As Integer) As Object
            Get
                Dim value As Object = Nothing
                Call TryGetOrdinal(field, value)
                Return value
            End Get
        End Property


        Friend Sub New(index&, table As Sqlite3Table, rowId As Long, columnData As Object())
            Me.Table = table
            Me.RowId = rowId
            Me.ColumnData = columnData
            Me.ReadIndex = index
        End Sub

        Public Overrides Function ToString() As String
            Return $"#{ReadIndex} [{RowId}] = {ColumnData.Select(AddressOf Scripting.ToString).GetJson}"
        End Function

        Public Function TryGetOrdinal(index As UShort, ByRef value As Object) As Boolean
            value = Nothing

            If ColumnData.Length > index Then
                value = ColumnData(index)
                Return True
            End If

            Return False
        End Function

        Public Function TryGetOrdinal(Of T)(index As UShort, Optional ByRef value As T = Nothing) As Boolean
            Dim tmp As Object = Nothing

            If Not TryGetOrdinal(index, tmp) Then
                Return False
            End If

            ' TODO: Is null a success case?
            If tmp Is Nothing Then
                Return False
            End If

            value = DirectCast(Convert.ChangeType(tmp, GetType(T)), T)
            Return True
        End Function
    End Class
End Namespace

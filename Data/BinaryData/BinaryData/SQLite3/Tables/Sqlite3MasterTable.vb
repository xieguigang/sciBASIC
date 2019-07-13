#Region "Microsoft.VisualBasic::12b9d4cf84349cfaa9cc5c2bae56004e, Data\BinaryData\BinaryData\SQLite3\Tables\Sqlite3MasterTable.vb"

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

    '     Class Sqlite3MasterTable
    ' 
    '         Properties: schema, tables
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ParseTables, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ManagedSqlite.Core.Tables

    ''' <summary>
    ''' 主表主要是记录用户创建的数据表的结构定义以及在数据库文件之中的读取偏移位置
    ''' </summary>
    Public Class Sqlite3MasterTable

        Public ReadOnly Property tables As Sqlite3SchemaRow()
        Public ReadOnly Property schema As Sqlite3SchemaRow

        Public Sub New(table As Sqlite3Table)
            tables = ParseTables(table).ToArray
            schema = table.SchemaDefinition
        End Sub

        Private Iterator Function ParseTables(master As Sqlite3Table) As IEnumerable(Of Sqlite3SchemaRow)
            Dim rows As IEnumerable(Of Sqlite3Row) = master.EnumerateRows()

            For Each row As Sqlite3Row In rows
                Dim other As New Sqlite3SchemaRow()
                Dim str As String = Nothing
                Dim lng As Long

                row.TryGetOrdinal(0, str)
                other.type = str

                row.TryGetOrdinal(1, str)
                other.name = str

                row.TryGetOrdinal(2, str)
                other.tableName = str

                row.TryGetOrdinal(3, lng)
                other.rootPage = CUInt(lng)

                row.TryGetOrdinal(4, str)
                other.Sql = str

                Yield other
            Next
        End Function

        Public Overrides Function ToString() As String
            Return schema.ToString
        End Function
    End Class
End Namespace

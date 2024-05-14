#Region "Microsoft.VisualBasic::c4b94c96d5ad5d5c88c41b5d22c5c72c, Data\BinaryData\SQLite3\Tables\Sqlite3MasterTable.vb"

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

    '   Total Lines: 47
    '    Code Lines: 32
    ' Comment Lines: 3
    '   Blank Lines: 12
    '     File Size: 1.52 KB


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
                other.Sql = str.TrimNull

                Yield other
            Next
        End Function

        Public Overrides Function ToString() As String
            Return schema.ToString
        End Function
    End Class
End Namespace

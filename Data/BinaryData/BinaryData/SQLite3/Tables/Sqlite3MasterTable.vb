#Region "Microsoft.VisualBasic::7f4d3bf292f808f4f03e1968d229bbec, Data\BinaryData\BinaryData\SQLite3\Tables\Sqlite3MasterTable.vb"

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

    '     Class Sqlite3MasterTable
    ' 
    '         Properties: Tables
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic

Namespace ManagedSqlite.Core.Tables

    Friend Class Sqlite3MasterTable

        Public ReadOnly Property Tables() As List(Of Sqlite3SchemaRow)
        Public ReadOnly Property Schema As Sqlite3SchemaRow

        Public Sub New(table As Sqlite3Table)
            Tables = ParseTables(table).ToList
            Schema = table.SchemaDefinition
        End Sub

        Private Iterator Function ParseTables(master As Sqlite3Table) As IEnumerable(Of Sqlite3SchemaRow)
            Dim rows As IEnumerable(Of Sqlite3Row) = master.EnumerateRows()

            For Each row As Sqlite3Row In rows
                Dim other As New Sqlite3SchemaRow()
                Dim str As String = Nothing
                Dim lng As Long

                row.TryGetOrdinal(0, str)
                other.Type = str

                row.TryGetOrdinal(1, str)
                other.Name = str

                row.TryGetOrdinal(2, str)
                other.TableName = str

                row.TryGetOrdinal(3, lng)
                other.RootPage = CUInt(lng)

                row.TryGetOrdinal(4, str)
                other.Sql = str

                Yield other
            Next
        End Function

        Public Overrides Function ToString() As String
            Return Schema.ToString
        End Function
    End Class
End Namespace

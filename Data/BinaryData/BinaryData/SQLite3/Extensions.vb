Imports System.Collections.Generic
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Tables
Imports Microsoft.VisualBasic.Linq

Namespace ManagedSqlite

    Public Module Extensions

        <Extension>
        Public Iterator Function ExportTable(table As Sqlite3Table) As IEnumerable(Of [Property](Of String))
            Dim schema = table.SchemaDefinition _
                .ParseSchema _
                .Columns _
                .SeqIterator _
                .ToArray
            Dim rowObject As [Property](Of String)

            For Each row As Sqlite3Row In table.EnumerateRows
                rowObject = New [Property](Of String)

                For Each field In schema
                    With field.value
                        Call rowObject.Add(.Name, Scripting.ToString(row(field)))
                    End With
                Next

                Yield rowObject
            Next
        End Function

        <Extension>
        Public Function ExportTable(database As Sqlite3Database, tableName$) As IEnumerable(Of [Property](Of String))
            Return database.GetTable(tableName).ExportTable
        End Function
    End Module
End Namespace
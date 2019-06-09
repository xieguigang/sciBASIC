#Region "Microsoft.VisualBasic::6cb5d811c4b12961fc31f907f9ed39d9, Data\BinaryData\BinaryData\SQLite3\Extensions.vb"

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

    '     Module Extensions
    ' 
    '         Function: (+2 Overloads) ExportTable
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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

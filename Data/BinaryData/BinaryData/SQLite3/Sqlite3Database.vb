#Region "Microsoft.VisualBasic::48b6da883d9d817937f14c93c35bf138, Data\BinaryData\BinaryData\SQLite3\Sqlite3Database.vb"

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

    '     Class Sqlite3Database
    ' 
    '         Properties: GetTables, Header
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetTable
    ' 
    '         Sub: Dispose, Initialize, InitializeMasterTable
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports System.IO
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Internal
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Objects
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Objects.Headers
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Tables

Namespace ManagedSqlite.Core

    ''' <summary>
    ''' MBW.Utilities.ManagedSqlite
    ''' 
    ''' > https://github.com/LordMike/MBW.Utilities.ManagedSqlite
    ''' > https://www.sqlite.org/fileformat.html
    ''' </summary>
    Public Class Sqlite3Database : Implements IDisposable

        Private ReadOnly _settings As Sqlite3Settings
        Private ReadOnly _reader As ReaderBase

        Private _sizeInPages As UInteger
        Private _masterTable As Sqlite3MasterTable

        Public Property Header() As DatabaseHeader

        Public ReadOnly Property GetTables() As IEnumerable(Of Sqlite3SchemaRow)
            Get
                Return _masterTable.Tables
            End Get
        End Property

        Public Sub New(file As Stream, Optional settings As Sqlite3Settings = Nothing)
            _settings = If(settings, New Sqlite3Settings())
            _reader = New ReaderBase(file)

            Initialize()
            InitializeMasterTable()
        End Sub

        Private Sub Initialize()
            Header = DatabaseHeader.Parse(_reader)

            ' Database Size in pages adjustment
            ' https://www.sqlite.org/fileformat.html#in_header_database_size

            Dim expectedPages As UInteger = CUInt(_reader.Length \ Header.PageSize)

            ' TODO: Warn on mismatch
            _sizeInPages = Math.Max(expectedPages, Header.DatabaseSizeInPages)

            _reader.ApplySqliteDatabaseHeader(Header)
        End Sub

        Private Sub InitializeMasterTable()
            ' Parse table on Page 1, the sqlite_master table
            Dim rootBtree As BTreePage = BTreePage.Parse(_reader, 1)

            ' Fake the schema for the sqlite_master table
            Dim schemaRow As New Sqlite3SchemaRow() With {
                 .Type = "table",
                 .Name = "sqlite_master",
                 .TableName = "sqlite_master",
                 .RootPage = rootBtree.Page,
                 .Sql = "CREATE TABLE sqlite_master (type TEXT, name TEXT, tbl_name TEXT, rootpage INTEGER, sql TEXT);"
            }

            Dim table As New Sqlite3Table(_reader, rootBtree, schemaRow)
            _masterTable = New Sqlite3MasterTable(table)
        End Sub

        Public Function GetTable(name As String) As Sqlite3Table
            Dim tables As IEnumerable(Of Sqlite3SchemaRow) = GetTables

            For Each table As Sqlite3SchemaRow In tables
                If table.TableName <> name OrElse table.Type <> "table" Then
                    Continue For
                End If

                ' Found it
                Dim root As BTreePage = BTreePage.Parse(_reader, table.RootPage)
                Dim tbl As New Sqlite3Table(_reader, root, table)
                Return tbl
            Next

            Throw New Exception("Unable to find table named " & name)
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            _reader.Dispose()
        End Sub
    End Class
End Namespace


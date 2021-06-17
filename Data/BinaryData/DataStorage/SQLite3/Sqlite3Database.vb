#Region "Microsoft.VisualBasic::a67b4de7b9e29a3c592d5c80028241dd, Data\BinaryData\DataStorage\SQLite3\Sqlite3Database.vb"

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
    '         Function: GetTable, OpenFile
    ' 
    '         Sub: Dispose, Initialize, InitializeMasterTable
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Internal
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Objects
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Objects.Headers
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Tables
Imports stdNum = System.Math

Namespace ManagedSqlite.Core

    ''' <summary>
    ''' MBW.Utilities.ManagedSqlite
    ''' 
    ''' > https://github.com/LordMike/MBW.Utilities.ManagedSqlite
    ''' > https://www.sqlite.org/fileformat.html
    ''' </summary>
    Public Class Sqlite3Database : Implements IDisposable

        ReadOnly _settings As Sqlite3Settings
        ReadOnly _reader As ReaderBase

        Dim _sizeInPages As UInteger
        Dim _masterTable As Sqlite3MasterTable

        Public Property Header As DatabaseHeader

        Public ReadOnly Property GetTables() As IEnumerable(Of Sqlite3SchemaRow)
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return _masterTable.tables
            End Get
        End Property

        Public Sub New(file As Stream, Optional settings As Sqlite3Settings = Nothing)
            _settings = settings Or Sqlite3Settings.GetDefaultSettings
            _reader = New ReaderBase(file)

            Call Initialize()
            Call InitializeMasterTable()
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="dbFile"></param>
        ''' <param name="settings">Default is <see cref="Sqlite3Settings.GetDefaultSettings"/></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function OpenFile(dbFile$, Optional settings As Sqlite3Settings = Nothing) As Sqlite3Database
            Return New Sqlite3Database(dbFile.Open(FileMode.Open, doClear:=False), settings)
        End Function

        Private Sub Initialize()
            Dim expectedPages As UInteger

            Header = DatabaseHeader.Parse(_reader)
            ' Database Size in pages adjustment
            ' https://www.sqlite.org/fileformat.html#in_header_database_size
            expectedPages = CUInt(_reader.Length \ Header.PageSize)
            ' TODO: Warn on mismatch
            _sizeInPages = stdNum.Max(expectedPages, Header.DatabaseSizeInPages)
            _reader.ApplySqliteDatabaseHeader(Header)
        End Sub

        Private Sub InitializeMasterTable()
            ' Parse table on Page 1, the sqlite_master table
            Dim rootBtree As BTreePage = BTreePage.Parse(_reader, 1)
            Dim table As Sqlite3Table
            ' Fake the schema for the sqlite_master table
            Dim schemaRow As New Sqlite3SchemaRow() With {
                 .Type = "table",
                 .Name = "sqlite_master",
                 .TableName = "sqlite_master",
                 .RootPage = rootBtree.Page,
                 .Sql = "CREATE TABLE sqlite_master (type TEXT, name TEXT, tbl_name TEXT, rootpage INTEGER, sql TEXT);"
            }

            table = New Sqlite3Table(_reader, rootBtree, schemaRow, _settings)
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
                Dim tbl As New Sqlite3Table(_reader, root, table, _settings)

                Return tbl
            Next

            Throw New Exception("Unable to find table named " & name)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Dispose() Implements IDisposable.Dispose
            Call _reader.Dispose()
        End Sub
    End Class
End Namespace

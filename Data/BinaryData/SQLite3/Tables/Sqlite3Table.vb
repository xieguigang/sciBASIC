#Region "Microsoft.VisualBasic::02d4f87eecc4a51a71d321306472cde8, Data\BinaryData\SQLite3\Tables\Sqlite3Table.vb"

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

    '   Total Lines: 162
    '    Code Lines: 110
    ' Comment Lines: 25
    '   Blank Lines: 27
    '     File Size: 6.92 KB


    '     Class Sqlite3Table
    ' 
    '         Properties: SchemaDefinition, Settings
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: EnumerateRows, ParseRow, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Helpers
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Internal
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Objects
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Objects.Enums
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.SQLSchema
Imports Microsoft.VisualBasic.Language

Namespace ManagedSqlite.Core.Tables

    Public Class Sqlite3Table

        ReadOnly reader As ReaderBase
        ReadOnly rootPage As BTreePage

        Public ReadOnly Property SchemaDefinition As Sqlite3SchemaRow
        Public ReadOnly Property Settings As Sqlite3Settings

        Friend Sub New(reader As ReaderBase, rootPage As BTreePage, table As Sqlite3SchemaRow, settings As Sqlite3Settings)
            Me.SchemaDefinition = table
            Me.reader = reader
            Me.rootPage = rootPage
            Me.Settings = settings
        End Sub

        Public Overrides Function ToString() As String
            Return SchemaDefinition.ToString
        End Function

        ''' <summary>
        ''' 枚举出这个表之中的所有的数据记录行
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function EnumerateRows() As IEnumerable(Of Sqlite3Row)
            Dim cells As IEnumerable(Of BTreeCellData) = BTreeTools.WalkTableBTree(rootPage)
            Dim schema As Schema = SchemaDefinition.ParseSchema
            Dim metaInfo As ColumnDataMeta() = schema.columns _
                .Select(Function(field)
                            Dim type As SqliteDataType = DataTypeParser.TryParse(field.Value)
                            Dim name = field.Name

                            Return New ColumnDataMeta(name, type)
                        End Function) _
                .ToArray

            Dim rowData As Object()
            Dim index As i32 = Scan0
            Dim row As Sqlite3Row

            For Each cell As BTreeCellData In cells
                ' Create a new stream to cover any fragmentation that might occur
                ' The stream is started in the current cells "resident" data, 
                ' And will overflow to any other pages as needed
                Using dataStream As New SqliteDataStream(Me.reader, cell)
                    Try
                        rowData = ParseRow(dataStream, metaInfo)
                        row = New Sqlite3Row(++index, Me, cell.Cell.RowId, rowData)

                        Yield row
                    Catch ex As Exception
                        Call ex.PrintException
                    End Try
                End Using
            Next
        End Function

        Private Function ParseRow(dataStream As SqliteDataStream, metaInfos As ColumnDataMeta()) As Object()
            Dim reader As New ReaderBase(dataStream, Me.reader)
            Dim null As Byte
            Dim headerSize As Long = reader.ReadVarInt(null)
            Dim index As Integer = Scan0

            ' 似乎在sqlite3之中,每一个数据区都有自己的一个header区域
            ' 因为字符串或者bytes blob这些可变长的数据需要长度信息
            ' 所以在这里每读取一个数据块之前都需要重新读取一次header信息
            While reader.Position < headerSize
                Dim columnInfo As Long = reader.ReadVarInt(null)
                Dim meta As ColumnDataMeta = metaInfos(index)

                index += 1

                If columnInfo = 0 Then
                    ' meta.type = SqliteDataType.Null
                ElseIf columnInfo = 1 Then
                    ' meta.type = SqliteDataType.[Integer]
                    meta.length = 1
                ElseIf columnInfo = 2 Then
                    ' meta.type = SqliteDataType.[Integer]
                    meta.length = 2
                ElseIf columnInfo = 3 Then
                    ' meta.type = SqliteDataType.[Integer]
                    meta.length = 3
                ElseIf columnInfo = 4 Then
                    ' meta.type = SqliteDataType.[Integer]
                    meta.length = 4
                ElseIf columnInfo = 5 Then
                    ' meta.type = SqliteDataType.[Integer]
                    meta.length = 6
                ElseIf columnInfo = 6 Then
                    ' meta.type = SqliteDataType.[Integer]
                    meta.length = 8
                ElseIf columnInfo = 7 Then
                    ' meta.type = SqliteDataType.Float
                    meta.length = 8
                ElseIf columnInfo = 8 Then
                    ' meta.type = SqliteDataType.Boolean0
                ElseIf columnInfo = 9 Then
                    ' meta.type = SqliteDataType.Boolean1
                ElseIf columnInfo = 10 OrElse columnInfo = 11 Then
                    Throw New ArgumentOutOfRangeException()
                ElseIf (columnInfo And &H1) = &H0 Then
                    ' Even number
                    ' meta.type = SqliteDataType.Blob
                    meta.length = CUShort((columnInfo - 12) \ 2)
                Else
                    ' Odd number
                    ' meta.type = SqliteDataType.Text
                    meta.length = CUShort((columnInfo - 13) \ 2)
                End If
            End While

            Dim rowData As Object() = New Object(metaInfos.Length - 1) {}

            For i As Integer = 0 To metaInfos.Length - 1
                Dim meta As ColumnDataMeta = metaInfos(i)

                Select Case meta.type
                    Case SqliteDataType.Null
                        rowData(i) = Nothing

                    Case SqliteDataType.[Integer]
                        ' TODO: Do we handle negatives correctly?
                        rowData(i) = reader.ReadInteger(CByte(meta.length))

                    Case SqliteDataType.Float
                        rowData(i) = BitConverter.Int64BitsToDouble(reader.ReadInteger(CByte(meta.length)))

                    Case SqliteDataType.Boolean0
                        rowData(i) = False

                    Case SqliteDataType.Boolean1
                        rowData(i) = True

                    Case SqliteDataType.Blob

                        If Settings.blobAsBase64 Then
                            rowData(i) = Convert.ToBase64String(reader.Read(meta.length))
                        Else
                            rowData(i) = reader.Read(meta.length)
                        End If

                    Case SqliteDataType.Text
                        rowData(i) = reader.ReadString(meta.length)

                    Case Else
                        Throw New ArgumentOutOfRangeException()
                End Select
            Next

            Return rowData
        End Function
    End Class
End Namespace

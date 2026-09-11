#Region "Microsoft.VisualBasic::c2a90f69c7d66220ff3063bfe1a53114, Data\BinaryData\SQLite3\Tables\Sqlite3Table.vb"

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

    '   Total Lines: 234
    '    Code Lines: 149 (63.68%)
    ' Comment Lines: 50 (21.37%)
    '    - Xml Docs: 64.00%
    ' 
    '   Blank Lines: 35 (14.96%)
    '     File Size: 9.73 KB


    '     Class Sqlite3Table
    ' 
    '         Properties: schema, SchemaDefinition, Settings
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: EnumerateRows, FindRowIdAliasOrdinal, GetIntegerByteWidth, ParseRow, ReadValue
    '                   ToDeclaredBoolean, ToDeclaredNumber, ToString
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

Namespace Core.Tables

    Public Class Sqlite3Table

        ReadOnly reader As ReaderBase
        ReadOnly rootPage As BTreePage

        Public ReadOnly Property SchemaDefinition As Sqlite3SchemaRow
        Public ReadOnly Property Settings As Sqlite3Settings
        Public ReadOnly Property schema As Schema

        Friend Sub New(reader As ReaderBase, rootPage As BTreePage, table As Sqlite3SchemaRow, settings As Sqlite3Settings)
            Me.SchemaDefinition = table
            Me.reader = reader
            Me.rootPage = rootPage
            Me.Settings = settings
            Me.schema = SchemaDefinition.ParseSchema
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
            Dim metaInfo As ColumnDataMeta() = schema.columns _
                .Select(Function(field)
                            Dim type As SqliteDataType = DataTypeParser.TryParse(field.Value)
                            Dim name = field.Name

                            Return New ColumnDataMeta(name, type)
                        End Function) _
                .ToArray

            Dim rowIdAlias As Integer = FindRowIdAliasOrdinal()
            Dim rowData As Object()
            Dim index As i32 = Scan0
            Dim row As Sqlite3Row

            For Each cell As BTreeCellData In cells
                ' Create a new stream to cover any fragmentation that might occur
                ' The stream is started in the current cells "resident" data, 
                ' And will overflow to any other pages as needed
                Using dataStream As New SqliteDataStream(Me.reader, cell)
                    Try
                        rowData = ParseRow(dataStream, metaInfo, cell.Cell.RowId, rowIdAlias)
                        row = New Sqlite3Row(++index, Me, cell.Cell.RowId, rowData)

                        Yield row
                    Catch ex As Exception
                        Call ex.PrintException
                    End Try
                End Using
            Next
        End Function

        Private Function ParseRow(dataStream As SqliteDataStream, metaInfos As ColumnDataMeta(), rowId As Long, rowIdAlias As Integer) As Object()
            Dim reader As New ReaderBase(dataStream, Me.reader)
            Dim null As Byte
            Dim headerSize As Long = reader.ReadVarInt(null)

            ' 每一个记录的开头都有属于自己的 header 区域, 其中保存了每一列的真实存储类型
            ' (serial type)。SQLite 是动态类型数据库, 只有 serial type 才是解码的唯一依据,
            ' 表结构之中的声明类型仅仅用于提供亲和性(affinity)信息。
            Dim serialTypes As New System.Collections.Generic.List(Of Long)

            While reader.Position < headerSize
                serialTypes.Add(reader.ReadVarInt(null))
            End While

            Dim rowData As Object() = New Object(metaInfos.Length - 1) {}

            For i As Integer = 0 To metaInfos.Length - 1
                If i >= serialTypes.Count Then
                    ' 记录之中的列数少于 schema 声明的列数
                    rowData(i) = Nothing
                    Continue For
                End If

                rowData(i) = ReadValue(reader, serialTypes(i), metaInfos(i))
            Next

            If rowIdAlias >= 0 AndAlso rowIdAlias < rowData.Length AndAlso rowData(rowIdAlias) Is Nothing Then
                ' ``INTEGER PRIMARY KEY`` 是 rowid 的别名, 在记录体之中以 NULL 存储, 这里回填 rowid
                rowData(rowIdAlias) = rowId
            End If

            Return rowData
        End Function

        ''' <summary>
        ''' 查找表之中的 rowid 别名列(即声明为 ``INTEGER PRIMARY KEY`` 的列)的序号, 不存在时返回 -1
        ''' </summary>
        Private Function FindRowIdAliasOrdinal() As Integer
            Dim pks As String() = Me.schema.PrimaryKeys

            If pks Is Nothing OrElse pks.Length = 0 Then
                Return -1
            End If

            Dim cols = Me.schema.columns

            For i As Integer = 0 To cols.Length - 1
                For Each pk As String In pks
                    If String.Equals(cols(i).Name, pk, StringComparison.OrdinalIgnoreCase) AndAlso
                       String.Equals(If(cols(i).Value, "").Trim(), "integer", StringComparison.OrdinalIgnoreCase) Then

                        Return i
                    End If
                Next
            Next

            Return -1
        End Function

        ''' <summary>
        ''' 依据记录头之中的 serial type 解码出单个列的值。
        ''' 
        ''' > https://www.sqlite.org/fileformat2.html#serialtype
        ''' </summary>
        ''' <param name="reader"></param>
        ''' <param name="serialType">记录头之中的存储类型编号</param>
        ''' <param name="meta">列的声明类型信息, 用于亲和性转换</param>
        Private Function ReadValue(reader As ReaderBase, serialType As Long, meta As ColumnDataMeta) As Object
            Select Case serialType
                Case 0
                    ' NULL
                    Return Nothing

                Case 1, 2, 3, 4, 5, 6
                    ' 有符号整数: serial type 1/2/3/4 对应 1/2/3/4 字节, serial type 5 对应 6 字节, serial type 6 对应 8 字节
                    Return ToDeclaredNumber(reader.ReadInteger(CByte(GetIntegerByteWidth(serialType))), meta)

                Case 7
                    ' 8 字节 IEEE 浮点数
                    Return BitConverter.Int64BitsToDouble(reader.ReadInteger(CByte(8)))

                Case 8, 9
                    ' 整数 0 / 1
                    Return ToDeclaredBoolean(serialType - 8L, meta)

                Case 10, 11
                    ' 保留类型, 按照 NULL 处理
                    Return Nothing

                Case Else
                    If (serialType And 1L) = 0L Then
                        ' 偶数 >= 12: BLOB, 长度为 (serialType - 12) / 2
                        Dim length As Integer = CInt((serialType - 12L) \ 2L)

                        If Settings.blobAsBase64 Then
                            Return Convert.ToBase64String(reader.Read(length))
                        Else
                            Return reader.Read(length)
                        End If
                    Else
                        ' 奇数 >= 13: TEXT, 长度为 (serialType - 13) / 2
                        Dim length As Integer = CInt((serialType - 13L) \ 2L)
                        Return reader.ReadString(length)
                    End If
            End Select
        End Function

        ''' <summary>
        ''' 依据 serial type 计算整数所占的字节宽度
        ''' 
        ''' | serial type | 字节宽度 |
        ''' | --- | --- |
        ''' | 1 | 1 |
        ''' | 2 | 2 |
        ''' | 3 | 3 |
        ''' | 4 | 4 |
        ''' | 5 | 6 |
        ''' | 6 | 8 |
        ''' </summary>
        Private Shared Function GetIntegerByteWidth(serialType As Long) As Integer
            Select Case serialType
                Case 1L
                    Return 1
                Case 2L
                    Return 2
                Case 3L
                    Return 3
                Case 4L
                    Return 4
                Case 5L
                    Return 6
                Case Else
                    Return 8
            End Select
        End Function

        ''' <summary>
        ''' 将整数存储值转换为声明类型所对应的 CLR 类型
        ''' (FLOAT 亲和性转换为 Double, BOOLEAN 转换为 Boolean)
        ''' </summary>
        Private Function ToDeclaredNumber(value As Long, meta As ColumnDataMeta) As Object
            Select Case meta.type
                Case SqliteDataType.Boolean0, SqliteDataType.Boolean1
                    Return value <> 0L
                Case SqliteDataType.Float
                    Return CDbl(value)
                Case Else
                    Return value
            End Select
        End Function

        ''' <summary>
        ''' 将 serial type 8/9(整数 0/1) 转换为声明类型所对应的 CLR 类型
        ''' </summary>
        Private Function ToDeclaredBoolean(value As Long, meta As ColumnDataMeta) As Object
            Select Case meta.type
                Case SqliteDataType.Boolean0, SqliteDataType.Boolean1
                    Return value <> 0L
                Case SqliteDataType.Float
                    ' 数值列之中的 0/1 仍然应保持数值类型
                    Return CDbl(value)
                Case Else
                    Return value
            End Select
        End Function
    End Class
End Namespace

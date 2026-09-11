#Region "Microsoft.VisualBasic::61a76cd948c43933a8181d8620348740, Data\BinaryData\SQLite3\Writer\Internal\SqliteFileSerializer.vb"

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

    '   Total Lines: 240
    '    Code Lines: 167 (69.58%)
    ' Comment Lines: 26 (10.83%)
    '    - Xml Docs: 73.08%
    ' 
    '   Blank Lines: 47 (19.58%)
    '     File Size: 9.82 KB


    '     Class WriterTableSnapshot
    ' 
    '         Properties: Entries, Name, Sql
    ' 
    '     Class SqliteFileSerializer
    ' 
    '         Properties: ChangeCounter, PageSize
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: AllocateOverflow, BuildDatabaseHeader, Serialize
    ' 
    '         Sub: SerializeNode, WriteUInt16, WriteUInt32
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports System.IO
Imports System.Text

Namespace Writer.Internal

    ''' <summary>
    ''' 一张表在提交时的快照数据
    ''' </summary>
    Friend Class WriterTableSnapshot
        Public Property Name As String
        Public Property Sql As String
        ''' <summary>按 rowid 升序排列的记录</summary>
        Public Property Entries As List(Of LeafEntry)
    End Class

    ''' <summary>
    ''' 把内存模型序列化为一个完整的 SQLite3 数据库文件。
    ''' 
    ''' 页号分配策略: 用户表从第 2 页起按 pre-order 分配; <c>sqlite_master</c> 的根页固定为第 1 页,
    ''' 其子页在所有用户表页之后分配; 溢出页最后分配。这样父页始终可以引用已经确定的子页号。
    ''' </summary>
    Friend Class SqliteFileSerializer

        Private ReadOnly _btree As BTreeWriter
        Private ReadOnly _pages As New Dictionary(Of Long, Byte())
        Private _nextPage As Long
        Private _changeCounter As Long

        Public Sub New(pageSize As Integer, Optional reservedSpace As Integer = 0, Optional changeCounter As Long = 1)
            _btree = New BTreeWriter(pageSize, reservedSpace)
            _changeCounter = changeCounter
        End Sub

        Public ReadOnly Property PageSize As Integer
            Get
                Return _btree.PageSize
            End Get
        End Property

        Public Property ChangeCounter As Long
            Get
                Return _changeCounter
            End Get
            Set(value As Long)
                _changeCounter = value
            End Set
        End Property

        ''' <summary>
        ''' 序列化整库, 返回完整的文件字节。
        ''' </summary>
        Public Function Serialize(tables As IList(Of WriterTableSnapshot)) As Byte()
            _pages.Clear()
            _nextPage = 1

            ' 1. 用户表建树
            Dim trees As New List(Of BTreeNode)()
            For Each t As WriterTableSnapshot In tables
                trees.Add(_btree.BuildTree(t.Entries))
            Next

            ' 2. 用户表页号从第 2 页开始分配
            Dim counter As Long = 2
            For Each tree As BTreeNode In trees
                BTreeWriter.AssignPages(tree, counter)
            Next

            ' 3. 构建 sqlite_master, 根页固定为第 1 页
            Dim masterEntries As New List(Of LeafEntry)()
            Dim masterRowId As Long = 1

            For i As Integer = 0 To tables.Count - 1
                Dim t As WriterTableSnapshot = tables(i)
                Dim record As Byte() = RecordEncoder.EncodeRecord(
                    New Object() {"table", t.Name, t.Name, trees(i).Page, t.Sql}, -1)

                masterEntries.Add(New LeafEntry With {.RowId = masterRowId, .Record = record})
                masterRowId += 1
            Next

            Dim masterTree As BTreeNode = _btree.BuildTree(masterEntries)
            masterTree.Page = 1

            If Not masterTree.IsLeaf Then
                For Each child As BTreeNode In masterTree.Children
                    BTreeWriter.AssignPages(child, counter)
                Next
            End If

            ' 4. 序列化所有页(过程中可能分配溢出页)
            _nextPage = counter
            Call SerializeNode(masterTree, PageBuffer.Page1ContentOffset)

            For Each tree As BTreeNode In trees
                Call SerializeNode(tree, 0)
            Next

            ' 5. 组装文件内容
            Dim totalPages As Long = _nextPage - 1
            Dim header As Byte() = BuildDatabaseHeader(totalPages)

            If _pages.ContainsKey(1) Then
                Call Array.Copy(header, 0, _pages(1), 0, header.Length)
            Else
                _pages(1) = header
            End If

            Dim ms As New MemoryStream()

            For p As Long = 1 To totalPages
                Dim page As Byte()

                If _pages.ContainsKey(p) Then
                    page = _pages(p)
                Else
                    page = New Byte(_btree.PageSize - 1) {}
                End If

                ms.Write(page, 0, page.Length)
            Next

            Return ms.ToArray()
        End Function

        Private Sub SerializeNode(node As BTreeNode, baseOffset As Integer)
            Dim cells As New List(Of Byte())()

            If node.IsLeaf Then
                For Each entry As LeafEntry In node.LeafCells
                    cells.Add(_btree.BuildLeafCell(entry, AddressOf AllocateOverflow))
                Next

                _pages(node.Page) = PageBuffer.BuildBTreePage(
                    _btree.PageSize, PageBuffer.PageTypeLeafTable, cells,
                    rightMostPointer:=0, baseOffset:=baseOffset, usableSize:=_btree.UsableSize)
                Return
            End If

            For i As Integer = 0 To node.Children.Count - 2
                Dim child As BTreeNode = node.Children(i)
                cells.Add(_btree.BuildInteriorCell(child.Page, child.MaxRowId))
            Next

            Dim rightMost As Long = node.Children(node.Children.Count - 1).Page

            _pages(node.Page) = PageBuffer.BuildBTreePage(
                _btree.PageSize, PageBuffer.PageTypeInteriorTable, cells,
                rightMostPointer:=rightMost, baseOffset:=baseOffset, usableSize:=_btree.UsableSize)

            For Each child As BTreeNode In node.Children
                Call SerializeNode(child, 0)
            Next
        End Sub

        ''' <summary>
        ''' 为超出页内可容纳范围的记录分配溢出页链, 返回首个溢出页号。
        ''' 每个溢出页 = 4 字节下一页指针 + 最多 (U - 4) 字节数据。
        ''' </summary>
        Private Function AllocateOverflow(record As Byte(), inlineSize As Integer) As Long
            Dim capacity As Integer = _btree.UsableSize - 4
            Dim remaining As Integer = record.Length - inlineSize
            Dim pageCount As Integer = (remaining + capacity - 1) \ capacity

            Dim firstPage As Long = _nextPage
            _nextPage += pageCount

            Dim offset As Integer = inlineSize

            For i As Integer = 0 To pageCount - 1
                Dim pageNo As Long = firstPage + i
                Dim page As New PageBuffer(_btree.PageSize)
                Dim nextNo As Long = If(i < pageCount - 1, pageNo + 1, 0L)

                page.WriteUInt32(0, nextNo)

                Dim length As Integer = System.Math.Min(capacity, record.Length - offset)
                Dim chunk(length - 1) As Byte
                Call Array.Copy(record, offset, chunk, 0, length)
                page.WriteBytes(4, chunk)

                _pages(pageNo) = page.Data
                offset += length
            Next

            Return firstPage
        End Function

        ''' <summary>
        ''' 构建 100 字节的数据库文件头(字段顺序与读取侧 DatabaseHeader.Parse 严格对齐)。
        ''' </summary>
        Private Function BuildDatabaseHeader(totalPages As Long) As Byte()
            Dim pageSize As Integer = _btree.PageSize
            Dim header As Byte() = New Byte(99) {}

            Dim magic As Byte() = Encoding.ASCII.GetBytes("SQLite format 3")
            Call Array.Copy(magic, 0, header, 0, magic.Length)
            header(15) = 0

            Call WriteUInt16(header, 16, If(pageSize = 65536, 1, pageSize))
            header(18) = 1                       ' WriteVersion = Legacy
            header(19) = 1                       ' ReadVersion  = Legacy
            header(20) = 0                       ' ReservedSpaceAtEndOfPage
            header(21) = 64                      ' MaximumEmbeddedPayloadFraction
            header(22) = 32                      ' MinimumEmbeddedPayloadFraction
            header(23) = 32                      ' LeafPayloadFraction
            Call WriteUInt32(header, 24, _changeCounter)
            Call WriteUInt32(header, 28, totalPages)
            Call WriteUInt32(header, 32, 0)      ' FirstFreelistTrunkPage
            Call WriteUInt32(header, 36, 0)      ' FreeListPages
            Call WriteUInt32(header, 40, _changeCounter)  ' SchemaCookie
            Call WriteUInt32(header, 44, 4)      ' SchemaFormat
            Call WriteUInt32(header, 48, 0)      ' DefaultPageCacheSize
            Call WriteUInt32(header, 52, 0)      ' LargestRootBTreePage
            Call WriteUInt32(header, 56, 1)      ' TextEncoding = UTF8
            Call WriteUInt32(header, 60, 0)      ' UserVersion
            Call WriteUInt32(header, 64, 0)      ' IncrementalVacuumMode
            Call WriteUInt32(header, 68, 0)      ' ApplicationId
            ' 偏移 72..91 为保留区, 保持为 0
            Call WriteUInt32(header, 92, _changeCounter)  ' VersionValidFor
            Call WriteUInt32(header, 96, 3045000)         ' SQLite version number

            Return header
        End Function

        Private Shared Sub WriteUInt16(buffer As Byte(), offset As Integer, value As Integer)
            buffer(offset) = CByte((value >> 8) And &HFF)
            buffer(offset + 1) = CByte(value And &HFF)
        End Sub

        Private Shared Sub WriteUInt32(buffer As Byte(), offset As Integer, value As Long)
            buffer(offset) = CByte((value >> 24) And &HFF)
            buffer(offset + 1) = CByte((value >> 16) And &HFF)
            buffer(offset + 2) = CByte((value >> 8) And &HFF)
            buffer(offset + 3) = CByte(value And &HFF)
        End Sub

    End Class

End Namespace


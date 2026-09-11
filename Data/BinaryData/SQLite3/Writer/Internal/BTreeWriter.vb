#Region "Microsoft.VisualBasic::4a2f741ede55c751dd1bb9c990c791a1, Data\BinaryData\SQLite3\Writer\Internal\BTreeWriter.vb"

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

    '   Total Lines: 244
    '    Code Lines: 160 (65.57%)
    ' Comment Lines: 39 (15.98%)
    '    - Xml Docs: 92.31%
    ' 
    '   Blank Lines: 45 (18.44%)
    '     File Size: 9.38 KB


    '     Structure LeafEntry
    ' 
    ' 
    ' 
    '     Class BTreeNode
    ' 
    '         Properties: Children, IsLeaf, LeafCells, MaxRowId, Page
    ' 
    '         Function: CreateInterior, CreateLeaf
    ' 
    '     Class BTreeWriter
    ' 
    '         Properties: PageSize, ReservedSpace, UsableSize
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: BuildInteriorCell, BuildInteriorLevel, BuildLeafCell, BuildTree, GetInlineSize
    '                   GetLeafCellSize
    ' 
    '         Sub: AssignPages, WriteUInt32BigEndian
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports System.IO

Namespace Writer.Internal

    ''' <summary>
    ''' 表 B 树叶页之中的一条记录(按 rowid 升序排列)
    ''' </summary>
    Friend Structure LeafEntry
        Public RowId As Long
        Public Record As Byte()
    End Structure

    ''' <summary>
    ''' 表 B 树的节点(叶节点或内部节点), 页号在结构构建完成之后单独分配。
    ''' </summary>
    Friend Class BTreeNode

        Public Property IsLeaf As Boolean
        Public Property LeafCells As List(Of LeafEntry)
        Public Property Children As List(Of BTreeNode)
        Public Property Page As Long

        Friend Shared Function CreateLeaf(cells As List(Of LeafEntry)) As BTreeNode
            Return New BTreeNode With {
                .IsLeaf = True,
                .LeafCells = cells,
                .Children = New List(Of BTreeNode)()
            }
        End Function

        Friend Shared Function CreateInterior(children As List(Of BTreeNode)) As BTreeNode
            Return New BTreeNode With {
                .IsLeaf = False,
                .LeafCells = New List(Of LeafEntry)(),
                .Children = children
            }
        End Function

        ''' <summary>
        ''' 子树之中的最大 rowid(用于内部页的 key)
        ''' </summary>
        Public ReadOnly Property MaxRowId As Long
            <System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)>
            Get
                If IsLeaf Then
                    Return LeafCells(LeafCells.Count - 1).RowId
                Else
                    Return Children(Children.Count - 1).MaxRowId
                End If
            End Get
        End Property
    End Class

    ''' <summary>
    ''' 表 B 树写入器: 负责容量计算、自底向上建树、cell 编码与页号分配。
    ''' </summary>
    Friend Class BTreeWriter

        Public ReadOnly Property PageSize As Integer
        Public ReadOnly Property ReservedSpace As Integer
        ''' <summary>可用页大小 U = PageSize - ReservedSpace</summary>
        Public ReadOnly Property UsableSize As Integer

        ''' <summary>table leaf 页在无溢出时可以内联的最大 payload(X = U - 35)</summary>
        Private ReadOnly _x As Integer
        ''' <summary>溢出时保证内联的最小 payload(M = ((U-12)*32/255)-23)</summary>
        Private ReadOnly _m As Integer

        Public Sub New(pageSize As Integer, reservedSpace As Integer)
            Me.PageSize = pageSize
            Me.ReservedSpace = reservedSpace
            Me.UsableSize = pageSize - reservedSpace
            _x = UsableSize - 35
            _m = (UsableSize - 12) * 32 \ 255 - 23
        End Sub

        ''' <summary>
        ''' 计算 payload 之中内联保存在 cell 内部的字节数(与读取侧 <c>ParseCellInternal</c> 严格对称)。
        ''' </summary>
        Public Function GetInlineSize(payloadSize As Long) As Integer
            If payloadSize <= _x Then
                ' 全部内联, 无溢出
                Return CInt(payloadSize)
            End If

            Dim k As Integer = CInt(_m + ((payloadSize - _m) Mod (UsableSize - 4)))

            If k <= _x Then
                Return k
            End If

            Return _m
        End Function

        ''' <summary>
        ''' 计算叶 cell 在页内所占的字节数(含可能的 4 字节溢出页指针)
        ''' </summary>
        Public Function GetLeafCellSize(entry As LeafEntry) As Integer
            Dim payload As Long = entry.Record.Length
            Dim inlineSize As Integer = GetInlineSize(payload)

            Dim size As Integer = RecordEncoder.GetVarIntLength(payload) +
                                  RecordEncoder.GetVarIntLength(entry.RowId) +
                                  inlineSize

            If payload > inlineSize Then
                size += 4
            End If

            Return size
        End Function

        ''' <summary>
        ''' 自底向上把有序记录构建为表 B 树。
        ''' </summary>
        Public Function BuildTree(entries As List(Of LeafEntry)) As BTreeNode
            Dim nodes As New List(Of BTreeNode)()
            Dim current As New List(Of LeafEntry)()
            Dim used As Long = 0

            For Each e As LeafEntry In entries
                Dim cellSize As Long = GetLeafCellSize(e)
                ' 指针数组随 cell 数量线性增长, 必须按加入之后的 cell 总数计算
                Dim pointers As Long = PageBuffer.CellPointerSize * (current.Count + 1)

                If current.Count > 0 AndAlso
                   (PageBuffer.LeafHeaderSize + used + cellSize + pointers) > UsableSize Then

                    nodes.Add(BTreeNode.CreateLeaf(current))
                    current = New List(Of LeafEntry)()
                    used = 0
                End If

                current.Add(e)
                used += cellSize
            Next

            If current.Count > 0 OrElse nodes.Count = 0 Then
                nodes.Add(BTreeNode.CreateLeaf(current))
            End If

            While nodes.Count > 1
                nodes = BuildInteriorLevel(nodes)
            End While

            Return nodes(0)
        End Function

        Private Function BuildInteriorLevel(children As List(Of BTreeNode)) As List(Of BTreeNode)
            Dim parents As New List(Of BTreeNode)()
            Dim current As New List(Of BTreeNode)()
            Dim used As Long = 0

            For Each child As BTreeNode In children
                Dim addCell As Long = 0
                Dim addPointer As Long = 0

                If current.Count > 0 Then
                    ' 追加一个子节点会在末尾新增一个 cell(左指针=当前最后一个子节点, key=其最大 rowid)
                    addCell = 4 + RecordEncoder.GetVarIntLength(current(current.Count - 1).MaxRowId)
                    addPointer = PageBuffer.CellPointerSize
                End If

                If current.Count > 0 AndAlso
                   (PageBuffer.InteriorHeaderSize + used + addCell + addPointer) > UsableSize Then

                    parents.Add(BTreeNode.CreateInterior(current))
                    current = New List(Of BTreeNode)()
                    used = 0
                End If

                current.Add(child)
                used += addCell + addPointer
            Next

            If current.Count > 0 Then
                parents.Add(BTreeNode.CreateInterior(current))
            End If

            Return parents
        End Function

        ''' <summary>
        ''' 按 pre-order(父页先于子页)分配页号, 保证父页可以引用已确定的子页号。
        ''' </summary>
        Public Shared Sub AssignPages(node As BTreeNode, ByRef nextPage As Long)
            node.Page = nextPage
            nextPage += 1

            If Not node.IsLeaf Then
                For Each child As BTreeNode In node.Children
                    AssignPages(child, nextPage)
                Next
            End If
        End Sub

        ''' <summary>
        ''' 构建叶 cell 字节: varint(P) + varint(rowid) + 内联 payload [+ 4 字节溢出页指针]
        ''' </summary>
        ''' <param name="allocateOverflow">
        ''' 溢出页分配回调, 参数为(完整记录体, 内联字节数), 返回首个溢出页号。
        ''' </param>
        Public Function BuildLeafCell(entry As LeafEntry, allocateOverflow As Func(Of Byte(), Integer, Long)) As Byte()
            Dim payload As Long = entry.Record.Length
            Dim inlineSize As Integer = GetInlineSize(payload)

            Using ms As New MemoryStream()
                Call RecordEncoder.WriteVarInt(ms, payload)
                Call RecordEncoder.WriteVarInt(ms, entry.RowId)

                If payload <= inlineSize Then
                    ms.Write(entry.Record, 0, entry.Record.Length)
                Else
                    ms.Write(entry.Record, 0, inlineSize)
                    Dim firstOverflow As Long = allocateOverflow(entry.Record, inlineSize)
                    Call WriteUInt32BigEndian(ms, firstOverflow)
                End If

                Return ms.ToArray()
            End Using
        End Function

        ''' <summary>
        ''' 构建内部 cell 字节: 4 字节左子页号 + varint(key)
        ''' </summary>
        Public Function BuildInteriorCell(leftChildPage As Long, key As Long) As Byte()
            Using ms As New MemoryStream()
                Call WriteUInt32BigEndian(ms, leftChildPage)
                Call RecordEncoder.WriteVarInt(ms, key)
                Return ms.ToArray()
            End Using
        End Function

        Public Shared Sub WriteUInt32BigEndian(stream As Stream, value As Long)
            stream.WriteByte(CByte((value >> 24) And &HFF))
            stream.WriteByte(CByte((value >> 16) And &HFF))
            stream.WriteByte(CByte((value >> 8) And &HFF))
            stream.WriteByte(CByte(value And &HFF))
        End Sub

    End Class

End Namespace


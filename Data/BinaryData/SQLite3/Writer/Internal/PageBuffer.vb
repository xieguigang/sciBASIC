#Region "Microsoft.VisualBasic::3efbbaeb48d9324dd1f2b1e4824e9c0f, Data\BinaryData\SQLite3\Writer\Internal\PageBuffer.vb"

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

    '   Total Lines: 128
    '    Code Lines: 74 (57.81%)
    ' Comment Lines: 27 (21.09%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 27 (21.09%)
    '     File Size: 5.08 KB


    '     Class PageBuffer
    ' 
    '         Properties: Data, PageSize
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: BuildBTreePage
    ' 
    '         Sub: WriteByte, WriteBytes, WriteUInt16, WriteUInt32
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Writer.Internal

    ''' <summary>
    ''' 单个数据库页的字节缓冲, 提供大端整数写入与 B 树页组装能力。
    ''' </summary>
    Friend Class PageBuffer

        ''' <summary>
        ''' 一个 cell 指针占用 2 个字节
        ''' </summary>
        Public Const CellPointerSize As Integer = 2

        ''' <summary>
        ''' 表叶页(0x0D)的页头长度
        ''' </summary>
        Public Const LeafHeaderSize As Integer = 8

        ''' <summary>
        ''' 表内部页(0x05)的页头长度(8 字节 + 4 字节最右子页指针)
        ''' </summary>
        Public Const InteriorHeaderSize As Integer = 12

        ''' <summary>
        ''' 第 1 页起始处的数据库文件头长度(仅第 1 页需要偏移)
        ''' </summary>
        Public Const Page1ContentOffset As Integer = 100

        Public Const PageTypeInteriorTable As Byte = &H5
        Public Const PageTypeLeafTable As Byte = &HD

        Public ReadOnly Property PageSize As Integer

        Private ReadOnly _data As Byte()

        Public Sub New(pageSize As Integer)
            Me.PageSize = pageSize
            _data = New Byte(pageSize - 1) {}
        End Sub

        ''' <summary>
        ''' 底层的页字节缓冲(长度为 PageSize)
        ''' </summary>
        Public ReadOnly Property Data As Byte()
            Get
                Return _data
            End Get
        End Property

        Public Sub WriteByte(offset As Integer, value As Byte)
            _data(offset) = value
        End Sub

        Public Sub WriteUInt16(offset As Integer, value As Integer)
            _data(offset) = CByte((value >> 8) And &HFF)
            _data(offset + 1) = CByte(value And &HFF)
        End Sub

        Public Sub WriteUInt32(offset As Integer, value As Long)
            _data(offset) = CByte((value >> 24) And &HFF)
            _data(offset + 1) = CByte((value >> 16) And &HFF)
            _data(offset + 2) = CByte((value >> 8) And &HFF)
            _data(offset + 3) = CByte(value And &HFF)
        End Sub

        Public Sub WriteBytes(offset As Integer, bytes As Byte())
            If bytes Is Nothing OrElse bytes.Length = 0 Then
                Return
            End If

            Call Array.Copy(bytes, 0, _data, offset, bytes.Length)
        End Sub

        ''' <summary>
        ''' 组装一个表 B 树页: 页头 + cell 指针数组(按 cell 顺序) + 自可用区末尾向下排列的 cell 内容。
        ''' </summary>
        ''' <param name="pageSize">页大小</param>
        ''' <param name="pageType">页类型(0x05 内部表页 / 0x0D 表叶页)</param>
        ''' <param name="cells">按 key(rowid)升序排列的 cell 字节</param>
        ''' <param name="rightMostPointer">内部页的最右子页号, 叶页忽略</param>
        ''' <param name="baseOffset">页头起始偏移(第 1 页为 100, 其余为 0)</param>
        ''' <param name="usableSize">可用区大小(页大小扣除预留空间), 传 0 表示等于页大小</param>
        Public Shared Function BuildBTreePage(pageSize As Integer,
                                               pageType As Byte,
                                               cells As System.Collections.Generic.IList(Of Byte()),
                                               Optional rightMostPointer As Long = 0,
                                               Optional baseOffset As Integer = 0,
                                               Optional usableSize As Integer = 0) As Byte()

            If usableSize <= 0 Then
                usableSize = pageSize
            End If

            Dim page As New PageBuffer(pageSize)
            Dim isInterior As Boolean = pageType = PageTypeInteriorTable
            Dim headerSize As Integer = If(isInterior, InteriorHeaderSize, LeafHeaderSize)
            Dim cellCount As Integer = If(cells Is Nothing, 0, cells.Count)

            page.WriteByte(baseOffset + 0, pageType)
            page.WriteUInt16(baseOffset + 1, 0)     ' FirstFreeBlock
            page.WriteUInt16(baseOffset + 3, cellCount)
            page.WriteByte(baseOffset + 7, 0)       ' FragmentedFreeBytes

            If isInterior Then
                page.WriteUInt32(baseOffset + 8, rightMostPointer)
            End If

            Dim offsets(cellCount - 1) As Integer
            Dim contentBegin As Integer = usableSize

            For i As Integer = 0 To cellCount - 1
                Dim cell As Byte() = cells(i)
                contentBegin -= cell.Length
                page.WriteBytes(contentBegin, cell)
                offsets(i) = contentBegin
            Next

            page.WriteUInt16(baseOffset + 5, contentBegin)  ' CellContentBegin

            For i As Integer = 0 To cellCount - 1
                page.WriteUInt16(baseOffset + headerSize + i * CellPointerSize, offsets(i))
            Next

            Return page.Data
        End Function

    End Class

End Namespace


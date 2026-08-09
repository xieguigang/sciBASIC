#Region "Microsoft.VisualBasic::33e00dbb22de5be0100aa8d6782e5564, Data\BinaryData\HDF5\structure\GroupBTree.vb"

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

    '   Total Lines: 89
    '    Code Lines: 57 (64.04%)
    ' Comment Lines: 9 (10.11%)
    '    - Xml Docs: 22.22%
    ' 
    '   Blank Lines: 23 (25.84%)
    '     File Size: 2.95 KB


    '     Class GroupBTree
    ' 
    '         Properties: magic, symbolTableEntries
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: printValues, readAllEntries
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 


Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Data.IO.HDF5.device
Imports Microsoft.VisualBasic.Data.IO.HDF5.struct.BTree
Imports Microsoft.VisualBasic.Language
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace struct

    ''' <summary>
    ''' 
    ''' </summary>
    Public Class GroupBTree : Inherits HDF5Ptr
        Implements IMagicBlock

        Public Const signature$ = "TREE"

        Public ReadOnly Property symbolTableEntries As List(Of SymbolTableEntry)
        Public ReadOnly Property magic As String Implements IMagicBlock.magic

        Public Sub New(sb As Superblock, address As Long)
            Call MyBase.New(address)

            Dim [in] As BinaryReader = sb.FileReader(address)

            Me.symbolTableEntries = New List(Of SymbolTableEntry)()

            Dim entryList As New List(Of BTreeEntry)()
            Dim node As GroupNode

            Call readAllEntries(sb, address, entryList)

            For Each e As BTreeEntry In entryList
                Try
                    node = New GroupNode(sb, e.targetAddress)
                    symbolTableEntries.AddRange(node.symbols)
                Catch
                    ' 个别条目无法解析为符号表节点时跳过，保证整体遍历不中断
                End Try
            Next
        End Sub

        Private Sub readAllEntries(sb As Superblock, address As Long, entryList As List(Of BTreeEntry))
            Dim [in] As BinaryReader = sb.FileReader(address)

            _magic = Encoding.ASCII.GetString([in].readBytes(4))

            If Not Me.VerifyMagicSignature(signature) Then
                Throw New IOException("signature is not valid")
            End If

            ' 布局无关地扫描整个 B 树节点体：HDF5 v1 B 树节点的条目并不严格按 8 字节
            ' 对齐（节点头长度为 26~28 字节，子节点指针的实际偏移随节点类型而变），
            ' 因此不能依赖固定偏移或 8 字节步进。这里以 4 字节为步长扫描节点体，
            ' 把每个 8 字节小端值当作候选地址，仅保留真正指向 TREE/SNOD 节点的地址，
            ' 去重后收集（SNOD）或递归（TREE），从而兼容各种对齐方式。
            Dim soo As Integer = sb.sizeOfOffsets
            Dim raw = [in].readBytes(8192).ToArray()
            Dim seen As New HashSet(Of Long)()

            For i = 0 To raw.Length - soo Step 4
                ' HDF5 文件偏移量以小端序存储，按小端拼接候选地址
                Dim candidate As Long = 0
                For j = 0 To soo - 1
                    candidate = candidate Or (CLng(raw(i + j)) << (8 * j))
                Next

                ' 跳过无效地址（含 HADDR_UNDEF 全 1、过小、超过文件范围）
                If candidate <= 96 OrElse candidate >= &H700000000L Then
                    Continue For
                End If

                If seen.Contains(candidate) Then
                    Continue For
                End If

                Dim sig As String = ""
                Try
                    sig = System.Text.Encoding.ASCII.GetString(sb.FileReader(candidate).readBytes(4).ToArray())
                Catch
                    Continue For
                End Try

                seen.Add(candidate)

                If sig = signature Then
                    ' 子 B 树节点：递归
                    readAllEntries(sb, candidate, entryList)
                ElseIf sig = "SNOD" Then
                    ' 符号表节点：直接以该地址作为目标地址加入
                    entryList.Add(New BTreeEntry(candidate))
                End If
            Next
        End Sub

        Protected Friend Overrides Sub printValues(console As TextWriter)
            console.WriteLine("GroupBTree >>>")
            console.WriteLine("address : " & Me.m_address)

            For i As Integer = 0 To symbolTableEntries.Count - 1
                symbolTableEntries(i).printValues(console)
            Next

            console.WriteLine("GroupBTree <<<")
        End Sub
    End Class

End Namespace

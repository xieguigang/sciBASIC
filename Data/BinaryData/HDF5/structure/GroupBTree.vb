#Region "Microsoft.VisualBasic::ac20f643cb9c255ce46c524fa0015b7a, Data\BinaryData\HDF5\structure\GroupBTree.vb"

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
    '    Code Lines: 81 (63.28%)
    ' Comment Lines: 19 (14.84%)
    '    - Xml Docs: 10.53%
    ' 
    '   Blank Lines: 28 (21.88%)
    '     File Size: 4.82 KB


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

            Me.symbolTableEntries = New List(Of SymbolTableEntry)()
            Dim visited As New HashSet(Of Long)()

            Call traverseBTree(sb, address, visited)
        End Sub

        ''' <summary>
        ''' 按照 HDF5 v1 B-tree (type=0 group) 的二进制结构正确遍历整棵树。
        '''
        ''' 节点布局:
        '''   "TREE" 签名 (4 bytes)
        '''   节点类型 (1 byte)  — 0 = group
        '''   节点层级 (1 byte)  — 0 = leaf
        '''   已用条目数 (2 bytes)
        '''   左兄弟地址 (sizeOfOffsets bytes)
        '''   右兄弟地址 (sizeOfOffsets bytes)
        '''   交错排列的 key 与 child pointer:
        '''     key[0] (sizeOfOffsets bytes)  — link name offset into local heap
        '''     child[0] (sizeOfOffsets bytes) — SNOD 地址(leaf) 或子 B-tree 地址(internal)
        '''     key[1], child[1], ...
        '''     key[entriesUsed] (sizeOfOffsets bytes) — trailing key, 无 child
        '''
        ''' Leaf(level=0): child → SNOD(GroupNode) → 收集 SymbolTableEntry
        ''' Internal(level&gt;0): child → 子 B-tree 节点 → 递归
        ''' </summary>
        Private Sub traverseBTree(sb As Superblock, address As Long, visited As HashSet(Of Long))
            If address <= 0 OrElse visited.Contains(address) Then
                Return
            End If

            visited.Add(address)

            Dim [in] As BinaryReader = sb.FileReader(address)

            ' 1. 读取 TREE 签名
            _magic = Encoding.ASCII.GetString([in].readBytes(4))

            If Not Me.VerifyMagicSignature(signature) Then
                Return
            End If

            ' 2. 读取节点头
            Dim nodeType As Integer = [in].readByte()    ' 0 = group
            Dim level As Integer = [in].readByte()        ' 0 = leaf
            Dim entriesUsed As Integer = [in].readShort() ' 子节点数

            ' 3. 读取兄弟地址（跳过，遍历不需要）
            ReadHelper.readO([in], sb) ' leftSibling
            ReadHelper.readO([in], sb) ' rightSibling

            ' 4. 先读出所有 child 地址，避免 GroupNode 构造时改变 reader 位置
            Dim childAddresses As New List(Of Long)(entriesUsed)

            For i As Integer = 0 To entriesUsed - 1
                ' key[i]: sizeOfOffsets 字节的 link name offset（遍历不需要，跳过）
                ReadHelper.readO([in], sb)

                ' child[i]: sizeOfOffsets 字节的子节点地址
                Dim childAddr As Long = ReadHelper.readO([in], sb)
                childAddresses.Add(childAddr)
            Next

            ' 5. 处理各 child 地址
            For Each childAddr As Long In childAddresses
                If childAddr <= 0 Then
                    Continue For
                End If

                If level = 0 Then
                    ' Leaf: child 指向 SNOD (Symbol Table Node)
                    Try
                        Dim snod As New GroupNode(sb, childAddr)
                        symbolTableEntries.AddRange(snod.symbols)
                    Catch
                        ' 个别 SNOD 无法解析时跳过，不中断整体遍历
                    End Try
                Else
                    ' Internal: child 指向子 B-tree 节点，递归
                    Call traverseBTree(sb, childAddr, visited)
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

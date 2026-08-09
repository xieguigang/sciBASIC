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

            ' HDF5 v1 B 树节点头：
            '   signature(4) + version(1) + type(1) + entriesUsed(2) + nodeID(4)
            ' 其中 type: 0 = 内部节点(需递归), 1 = 叶子节点(指向符号表节点)。
            ' 注意：v1 B 树节点本身不内嵌 sizeOfKey/sizeOfValue，
            '       key 固定为 4 字节，value 为 sizeOfOffsets 字节。
            Dim version As Integer = [in].readByte()
            Dim nodeType As Integer = [in].readByte()
            Dim entriesUsed As Integer = [in].readShort()
            [in].readInt() ' nodeID（4 字节），当前遍历不需要

            If nodeType = 1 Then
                ' 叶子节点：每个条目 = [key(4)][符号表节点地址(O)]
                For i As Integer = 0 To entriesUsed - 1
                    [in].skipBytes(4) ' 跳过 key（4 字节）
                    Dim childAddr As Long = ReadHelper.readO([in], sb)
                    collectNode(sb, childAddr, entryList)
                Next
            Else
                ' 内部节点：每个条目 = [key(4)][left(O)][right(O)]
                ' 子 B 树节点地址为各条目的 left 与 right（以及首条目的左兄弟）。
                Dim childAddrs As New List(Of Long)()
                Dim firstLeft As Long = -1

                For i As Integer = 0 To entriesUsed - 1
                    [in].skipBytes(4) ' 跳过 key（4 字节）
                    Dim left As Long = ReadHelper.readO([in], sb)
                    Dim right As Long = ReadHelper.readO([in], sb)

                    If i = 0 Then
                        firstLeft = left
                    End If

                    childAddrs.Add(left)
                    childAddrs.Add(right)
                Next

                If firstLeft > 0 Then
                    childAddrs.Add(firstLeft)
                End If

                For Each ca In childAddrs
                    ' HADDR_UNDEF 为全 1（转为 Long 即 -1），跳过无效地址
                    If ca > 0 Then
                        collectNode(sb, ca, entryList)
                    End If
                Next
            End If
        End Sub

        ''' <summary>
        ''' 容错地收集一个子节点：TREE 节点继续递归，SNOD 符号表节点直接加入，
        ''' 其余未知签名则跳过，避免单个坏地址导致整个遍历崩溃。
        ''' </summary>
        Private Sub collectNode(sb As Superblock, address As Long, entryList As List(Of BTreeEntry))
            If address <= 0 Then
                Return
            End If

            Dim sig As String

            Try
                sig = System.Text.Encoding.ASCII.GetString(sb.FileReader(address).readBytes(4).ToArray())
            Catch
                Return
            End Try

            If sig = signature Then
                readAllEntries(sb, address, entryList)
            ElseIf sig = "SNOD" Then
                entryList.Add(New BTreeEntry(sb, address))
            Else
                ' 未知签名：跳过，避免崩溃
                Return
            End If
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

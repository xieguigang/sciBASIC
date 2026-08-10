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

            Dim entryList As New List(Of BTreeEntry)()
            Dim visited As New HashSet(Of Long)()
            Dim node As GroupNode

            Call readAllEntries(sb, address, entryList, visited, 0)

            ' 去除重复的目标地址，避免同一 SNOD 节点被多次加入导致符号表条目重复
            Dim dedup As New List(Of BTreeEntry)()
            Dim seenTargets As New HashSet(Of Long)()
            For Each e As BTreeEntry In entryList
                If seenTargets.Add(e.targetAddress) Then
                    dedup.Add(e)
                End If
            Next

            For Each e As BTreeEntry In dedup
                Try
                    node = New GroupNode(sb, e.targetAddress)
                    symbolTableEntries.AddRange(node.symbols)
                Catch
                    ' 个别条目无法解析为符号表节点时跳过，保证整体遍历不中断
                End Try
            Next
        End Sub

        ' 布局无关地扫描 B 树节点体：以 4 字节为步长扫描，把每个 8 字节小端值当作
        ' 候选地址，仅保留真正指向 TREE/SNOD 节点的地址。SNOD 直接收集，TREE 递归。
        ' visited 在所有递归调用间共享，避免重复扫描同一节点造成无限递归；
        ' depth 作为额外保护，防止异常深的嵌套。
        Private Sub readAllEntries(sb As Superblock, address As Long, entryList As List(Of BTreeEntry), visited As HashSet(Of Long), depth As Integer)
            If address <= 0 OrElse visited.Contains(address) OrElse depth > 64 Then
                Return
            End If

            visited.Add(address)

            Dim [in] As BinaryReader = sb.FileReader(address)

            _magic = Encoding.ASCII.GetString([in].readBytes(4))

            If Not Me.VerifyMagicSignature(signature) Then
                Return
            End If

            Dim soo As Integer = sb.sizeOfOffsets
            Dim raw = [in].readBytes(8192).ToArray()

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

                If visited.Contains(candidate) Then
                    Continue For
                End If

                Dim sig As String = ""
                Try
                    sig = System.Text.Encoding.ASCII.GetString(sb.FileReader(candidate).readBytes(4).ToArray())
                Catch
                    Continue For
                End Try

                visited.Add(candidate)

                If sig = signature Then
                    ' 子 B 树节点：递归
                    readAllEntries(sb, candidate, entryList, visited, depth + 1)
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

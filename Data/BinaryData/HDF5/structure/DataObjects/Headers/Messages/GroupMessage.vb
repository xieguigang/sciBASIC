#Region "Microsoft.VisualBasic::8cdd832248f3427b45b6f9676cb7aebd, Data\BinaryData\HDF5\structure\DataObjects\Headers\Messages\GroupMessage.vb"

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

    '   Total Lines: 57
    '    Code Lines: 25 (43.86%)
    ' Comment Lines: 19 (33.33%)
    '    - Xml Docs: 68.42%
    ' 
    '   Blank Lines: 13 (22.81%)
    '     File Size: 1.88 KB


    '     Class GroupMessage
    ' 
    '         Properties: bTreeAddress, nameHeapAddress
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: printValues
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
Imports Microsoft.VisualBasic.Data.IO.HDF5.device
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace struct.messages

    ''' <summary>
    ''' The Symbol Table Message
    ''' </summary>
    Public Class GroupMessage : Inherits Message

        ''' <summary>
        ''' This value is the address of the v1 B-tree containing the symbol table 
        ''' entries for the group.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property bTreeAddress() As Long

        ''' <summary>
        ''' This value is the address of the local heap containing the link names 
        ''' for the symbol table entries for the group.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property nameHeapAddress() As Long

        Public Sub New(sb As Superblock, address As Long)
            Call MyBase.New(address)

            Dim [in] As BinaryReader = sb.FileReader(address)

            Me.bTreeAddress = ReadHelper.readO([in], sb)
            Me.nameHeapAddress = ReadHelper.readO([in], sb)

            ' HDF5 v1 对象头可能使用 continuation：Group 消息的数据并不内联，
            ' 而是存放在一个 continuation 块里。此时 bTreeAddress 字段实际保存的是
            ' continuation 块地址，其数据为真正的 (bTree, nameHeap) 对。
            ' 若 bTreeAddress 指向一个 continuation 消息（type=17），则解析该块，
            ' 用其中的 (bTree, nameHeap) 覆盖当前值。
            If IsContinuation(sb, Me.bTreeAddress) Then
                Dim c = sb.FileReader(Me.bTreeAddress)
                c.readBytes(8) ' 跳过 continuation 头部：type(2)+size(2)+flags(1)+reserved(3)
                Me.bTreeAddress = ReadHelper.readO(c, sb)
                Me.nameHeapAddress = ReadHelper.readO(c, sb)
            End If
        End Sub

        ''' <summary>
        ''' 判断指定地址是否是一个对象头 continuation 消息块（H5O_MSG_CONT = 17）。
        ''' </summary>
        Private Shared Function IsContinuation(sb As Superblock, address As Long) As Boolean
            If address <= 0 Then
                Return False
            End If

            Try
                Dim head = sb.FileReader(address).readBytes(4)
                ' 小端 2 字节消息类型
                Dim typeNo As Integer = head(0) Or (head(1) << 8)
                Return typeNo = 17
            Catch
                Return False
            End Try
        End Function

        Public Overrides Function ToString() As String
            Return $"{MyBase.ToString} {bTreeAddress} -> {nameHeapAddress}"
        End Function

        Protected Friend Overrides Sub printValues(console As TextWriter)
            console.WriteLine("GroupMessage >>>")
            console.WriteLine("address : " & Me.m_address)
            console.WriteLine("btree address : " & Me.bTreeAddress)
            console.WriteLine("nameheap address : " & Me.nameHeapAddress)
            console.WriteLine("GroupMessage <<<")
        End Sub

    End Class

End Namespace

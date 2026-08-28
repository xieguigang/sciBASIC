#Region "Microsoft.VisualBasic::9e561ec1c88ec5b6085e1f3f7308b611, Data\BinaryData\HDF5\structure\DataObjects\Headers\Messages\GroupMessage.vb"

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

    '   Total Lines: 98
    '    Code Lines: 55 (56.12%)
    ' Comment Lines: 27 (27.55%)
    '    - Xml Docs: 48.15%
    ' 
    '   Blank Lines: 16 (16.33%)
    '     File Size: 4.32 KB


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

            ' Symbol Table Message 直接包含 B-tree 地址和 Local Heap 地址。
            Me.bTreeAddress = ReadHelper.readO([in], sb)
            Me.nameHeapAddress = ReadHelper.readO([in], sb)

            ' h5py 生成的 v0 superblock 文件中，Symbol Table Message 的 bTreeAddress
            ' 可能不直接指向 B-tree，而是指向一个 Object Header Continuation 消息块，
            ' 其中 continuation-offset 字段才是真正的 B-tree 地址，
            ' continuation-length 字段是真正的 Local Heap 地址。
            ' 这不是 HDF5 规范行为，但 h5py 的实际输出如此，需要特殊处理。
            If Me.bTreeAddress > 0 Then
                Try
                    Dim probe = sb.FileReader(Me.bTreeAddress).readBytes(2).ToArray()
                    Dim typeNo As Integer = probe(0) Or (probe(1) << 8)
                    If typeNo = 17 Then
                        ' 确实是 continuation 消息：读取其 data 字段作为真实地址
                        Dim c = sb.FileReader(Me.bTreeAddress)
                        c.readBytes(8) ' 跳过 continuation 头部：type(2)+size(2)+flags(1)+reserved(3)
                        Me.bTreeAddress = ReadHelper.readO(c, sb)
                        Me.nameHeapAddress = ReadHelper.readO(c, sb)
                    End If
                Catch
                End Try
            End If

            Call Console.WriteLine($"[DIAG] GroupMessage@{address}: bTreeAddr={bTreeAddress}, nameHeapAddr={nameHeapAddress}")

            ' 诊断：打印地址处的十六进制字节
            If bTreeAddress > 0 Then
                Try
                    Dim bytes = sb.FileReader(bTreeAddress).readBytes(16).ToArray()
                    Call Console.WriteLine($"[DIAG]   bTreeAddr {bTreeAddress} -> hex={BitConverter.ToString(bytes)}")
                Catch ex As Exception
                    Call Console.WriteLine($"[DIAG]   bTreeAddr {bTreeAddress} -> read error: {ex.Message}")
                End Try
            End If
            If nameHeapAddress > 0 Then
                Try
                    Dim bytes = sb.FileReader(nameHeapAddress).readBytes(16).ToArray()
                    Call Console.WriteLine($"[DIAG]   nameHeapAddr {nameHeapAddress} -> hex={BitConverter.ToString(bytes)}")
                Catch ex As Exception
                    Call Console.WriteLine($"[DIAG]   nameHeapAddr {nameHeapAddress} -> read error: {ex.Message}")
                End Try
            End If
        End Sub

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

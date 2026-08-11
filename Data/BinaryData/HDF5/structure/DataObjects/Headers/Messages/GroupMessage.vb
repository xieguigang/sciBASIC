#Region "Microsoft.VisualBasic::88bfa3e4a979c111c1fe9b1db1e8c05c, Data\BinaryData\HDF5\structure\DataObjects\Headers\Messages\GroupMessage.vb"

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

    '   Total Lines: 87
    '    Code Lines: 43 (49.43%)
    ' Comment Lines: 28 (32.18%)
    '    - Xml Docs: 57.14%
    ' 
    '   Blank Lines: 16 (18.39%)
    '     File Size: 3.35 KB


    '     Class GroupMessage
    ' 
    '         Properties: bTreeAddress, nameHeapAddress
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: IsContinuation, ToString
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
            ' 此前有一个非标准的 IsContinuation hack 会检查 bTreeAddress 是否指向
            ' continuation 消息并从中读取新地址，但这不是 HDF5 规范行为，
            ' 可能在某些文件中破坏正确的地址。已移除，直接使用原始字段。
            Me.bTreeAddress = ReadHelper.readO([in], sb)
            Me.nameHeapAddress = ReadHelper.readO([in], sb)

            Call Console.WriteLine($"[DIAG] GroupMessage@{address}: bTreeAddr={bTreeAddress}, nameHeapAddr={nameHeapAddress}")

            ' 诊断：检查地址处的前 4 字节签名
            If bTreeAddress > 0 Then
                Try
                    Dim sig As String = System.Text.Encoding.ASCII.GetString(sb.FileReader(bTreeAddress).readBytes(4).ToArray())
                    Call Console.WriteLine($"[DIAG]   bTreeAddr {bTreeAddress} -> sig='{sig}'")
                Catch ex As Exception
                    Call Console.WriteLine($"[DIAG]   bTreeAddr {bTreeAddress} -> read error: {ex.Message}")
                End Try
            End If
            If nameHeapAddress > 0 Then
                Try
                    Dim sig As String = System.Text.Encoding.ASCII.GetString(sb.FileReader(nameHeapAddress).readBytes(4).ToArray())
                    Call Console.WriteLine($"[DIAG]   nameHeapAddr {nameHeapAddress} -> sig='{sig}'")
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

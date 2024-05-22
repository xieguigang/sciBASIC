﻿#Region "Microsoft.VisualBasic::01b08c6e3007da60376c93a41ed803ec, Microsoft.VisualBasic.Core\src\Extensions\IO\Extensions\StreamHelper.vb"

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

    '   Total Lines: 146
    '    Code Lines: 86 (58.90%)
    ' Comment Lines: 40 (27.40%)
    '    - Xml Docs: 87.50%
    ' 
    '   Blank Lines: 20 (13.70%)
    '     File Size: 4.91 KB


    ' Module StreamHelper
    ' 
    '     Function: CastByte, CastSByte, CopyStream, PopulateBlocks, ReadStringZero
    '               ReadZEROBlock
    ' 
    '     Sub: Write, WriteLine
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language

Public Module StreamHelper

    ''' <summary>
    ''' Download stream data from the http response.
    ''' </summary>
    ''' <param name="stream">
    ''' Create from <see cref="WebServiceUtils.GetRequestRaw"/>
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("Stream.Copy")>
    <Extension>
    Public Function CopyStream(stream As Stream, Optional target As Stream = Nothing, Optional bufferSize% = 64 * 1024) As Stream
        If stream Is Nothing Then
            Return If(target, New MemoryStream)
        End If

        Dim buffer As Byte() = New Byte(bufferSize - 1) {}
        Dim i As i32 = Scan0

        With target Or DirectCast(New MemoryStream(), Stream).AsDefault
            Do While (i = stream.Read(buffer, 0, buffer.Length)) > 0
                Call .Write(buffer, 0, i)
            Loop

            Return .ByRef
        End With
    End Function

    ''' <summary>
    ''' 这个函数会重置流的指针位置
    ''' </summary>
    ''' <param name="buffer"></param>
    ''' <param name="chunkSize%"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function PopulateBlocks(buffer As Stream, Optional chunkSize% = 2048) As IEnumerable(Of Byte())
        Dim chunk As Byte() = New Byte(chunkSize - 1) {}
        Dim ends& = buffer.Length
        Dim dl&

        ' 重置读取指针位置
        Call buffer.Seek(Scan0, SeekOrigin.Begin)

        ' 分块读取buffer，然后写入流数据
        Do While buffer.Position < ends
            dl = ends - buffer.Position

            If dl > chunkSize Then
                ' buffer之中还存在充足的数据进行复制
                Call buffer.Read(chunk, Scan0, chunkSize)
            Else
                ' 数据不足了
                chunk = New Byte(dl - 1) {}
                buffer.Read(chunk, Scan0, dl)
            End If

            Yield chunk

            If dl < chunkSize Then
                Exit Do
            End If
        Loop
    End Function

    ''' <summary>
    ''' write target string <paramref name="value"/> into the 
    ''' given <paramref name="stream"/> in no size prefix and
    ''' zero terminator.
    ''' </summary>
    ''' <param name="stream"></param>
    ''' <param name="value">target string value to write into the given <paramref name="stream"/></param>
    ''' <param name="encoding"></param>
    <Extension>
    Public Sub Write(stream As Stream, value$, Optional encoding As Encoding = Nothing)
        With (encoding Or UTF8).GetBytes(value)
            Call stream.Write(.ByRef, Scan0, .Length)
            Call stream.Flush()
        End With
    End Sub

    ''' <summary>
    ''' read current stream data until read a byte flag ZERO
    ''' </summary>
    ''' <param name="bin"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function ReadZEROBlock(bin As BinaryReader) As IEnumerable(Of Byte)
        Dim [byte] As Value(Of Byte) = 0

        Do While ([byte] = bin.ReadByte) <> 0
            Yield [byte].Value
        Loop
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ReadStringZero(file As BinaryReader, encoding As Encoding) As String
        Return encoding.GetString(file.ReadZEROBlock.ToArray)
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="stream"></param>
    ''' <param name="value"></param>
    ''' <param name="encoding"></param>
    ''' <param name="newLine"></param>
    ''' <remarks>
    ''' default use utf8 encoding
    ''' </remarks>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Sub WriteLine(stream As Stream, Optional value$ = "", Optional encoding As Encoding = Nothing, Optional newLine$ = vbCrLf)
        Call stream.Write(value & newLine, encoding)
    End Sub

    <Extension>
    Public Function CastByte(signed As SByte()) As Byte()
        Dim unsigned As Byte() = New Byte(signed.Length - 1) {}
        Buffer.BlockCopy(signed, Scan0, unsigned, Scan0, signed.Length - 1)
        Return unsigned
    End Function

    <Extension>
    Public Function CastSByte(unsigned As Byte()) As SByte()
        Dim signed As SByte() = New SByte(unsigned.Length - 1) {}
        Dim b127 As Byte = 127
        Dim b256 As Short = 256

        For i As Integer = 0 To unsigned.Length - 1
            If unsigned(i) > b127 Then
                signed(i) = CSByte(CShort(unsigned(i)) - b256)
            Else
                signed(i) = CSByte(unsigned(i))
            End If
        Next

        Return signed
    End Function
End Module

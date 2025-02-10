#Region "Microsoft.VisualBasic::09ca46566da14677b8437afe15811f48, Microsoft.VisualBasic.Core\src\Net\HTTP\Stream\ZipStream.vb"

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

    '   Total Lines: 133
    '    Code Lines: 59 (44.36%)
    ' Comment Lines: 58 (43.61%)
    '    - Xml Docs: 67.24%
    ' 
    '   Blank Lines: 16 (12.03%)
    '     File Size: 5.12 KB


    '     Module ZipStreamExtensions
    ' 
    '         Function: Deflate, TestZipMagic, UnZipStream, Zip
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.IO.Compression
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace Net.Http

    ''' <summary>
    ''' zip magic header: 120, 218
    ''' </summary>
    Public Module ZipStreamExtensions

        ''' <summary>
        ''' test the zip magic header
        ''' </summary>
        ''' <param name="buffer"></param>
        ''' <returns></returns>
        <Extension>
        Public Function TestZipMagic(buffer As Byte()) As Boolean
            If buffer.IsNullOrEmpty OrElse buffer.Length < 2 Then
                Return False
            Else
                Return buffer(0) = 120 AndAlso buffer(1) = 218
            End If
        End Function

        ''' <summary>
        ''' zip stream compression
        ''' </summary>
        ''' <param name="stream"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' this function will add the zlib magic header when 
        ''' <paramref name="magicHeader"/> is set to true. [NOTE:
        ''' the zlib compression result via the .NET library did
        ''' not contains the zlib magic header.]
        ''' </remarks>
        <Extension>
        Public Function Zip(stream As Stream, Optional magicHeader As Boolean = True) As MemoryStream
            Dim deflatMs As New MemoryStream()

            Using deflatestream As New DeflateStream(deflatMs, CompressionMode.Compress, True)
                Call stream.CopyTo(deflatestream, 8192)
            End Using

            If magicHeader Then
                Dim newBuf As New MemoryStream

                '
                ' zlib stream missing the magic header bytes
                ' add byte to the stream based on the options
                '
                ' pako.js inflate stream data required of the
                ' zlib magic bytes, or error will happends
                '
                ' ERROR_-3: incorrect header check
                '
                Call newBuf.Write(New Byte() {120, 218}, Scan0, 2)
                Call newBuf.Write(deflatMs.ToArray, Scan0, deflatMs.Length)

                Return newBuf
            Else
                Return deflatMs
            End If
        End Function

        ''' <summary>
        ''' 进行zlib数据流的zip解压缩
        ''' 
        ''' > https://bbs.csdn.net/topics/392275364
        ''' </summary>
        ''' <param name="stream"></param>
        ''' <param name="noMagic">
        ''' does the given stream data not contains the ziplib magic header?
        ''' 
        ''' default parameter value false means not contains, then the given stream data will be deflate directly.
        ''' otherwise this function will try to populate out two bytes(zlib magic header) and then make stream 
        ''' data decompression if the parameter value is TRUE.
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' ###### 2018-11-15 经过测试，没有bug，与zlibnet的测试结果一致
        ''' </remarks>
        <Extension>
        Public Function UnZipStream(stream As IEnumerable(Of Byte), Optional noMagic As Boolean = False) As MemoryStream
            Dim pBytes = stream.SafeQuery.ToArray

            ' Deflate 算法压缩之后的数据
            ' 第一个字节是 78h(120b)
            ' 第二个字节是 DAh(218b)
            If pBytes.Length < 2 Then
                Return New MemoryStream(pBytes)
            ElseIf (Not pBytes(0) = 120 AndAlso Not pBytes(1) = 218) Then
                If noMagic Then
                    ' 最开始的两个字节不是magic标记
                    ' 并且也指定了没有magic头的参数选项
                    ' 在这里直接尝试解压缩
                    Using compress As New MemoryStream(pBytes)
                        Return compress.Deflate
                    End Using
                Else
                    Return New MemoryStream(pBytes)
                End If
            End If

            Using compress As New MemoryStream(pBytes)

                ' 先读取前两个deflate压缩算法标识字节，然后才能用deflateStream解压
                ' 这个行为与 zlib库、sharpZiplib库等不同（这些库都是直接传入解压）
                Call compress.ReadByte()
                Call compress.ReadByte()

                Return compress.Deflate
            End Using
        End Function

        ''' <summary>
        ''' Decompress.
        ''' 在这里应该是正确的跳过了deflate压缩算法标识字节的数据流
        ''' </summary>
        ''' <returns></returns>
        <Extension>
        Public Function Deflate(compress As Stream) As MemoryStream
            Dim deflatMs As New MemoryStream()

            Using deflatestream As New DeflateStream(compress, CompressionMode.Decompress)
                deflatestream.CopyTo(deflatMs, 8192)
            End Using

            Return deflatMs
        End Function
    End Module
End Namespace

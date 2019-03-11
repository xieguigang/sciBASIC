#Region "Microsoft.VisualBasic::fdcad2bd4be97ba4e41f9b0e3eebdcad, Microsoft.VisualBasic.Core\Net\HTTP\ZipStream.vb"

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

    '     Module ZipStreamExtensions
    ' 
    '         Function: UnZipStream
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.IO.Compression
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace Net.Http

    Public Module ZipStreamExtensions

        ''' <summary>
        ''' 进行zlib数据流的zip解压缩
        ''' 
        ''' > https://bbs.csdn.net/topics/392275364
        ''' </summary>
        ''' <param name="stream"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' ###### 2018-11-15 经过测试，没有bug，与zlibnet的测试结果一致
        ''' </remarks>
        <Extension>
        Public Function UnZipStream(stream As IEnumerable(Of Byte)) As MemoryStream
            Dim pBytes = stream.SafeQuery.ToArray

            ' Deflate 算法压缩之后的数据，第一个字节是 78h（120b），第二个字节是 DAh(218b)
            If pBytes.Length < 2 OrElse (Not pBytes(0) = 120 AndAlso Not pBytes(1) = 218) Then
                Return New MemoryStream(pBytes)
            End If

            Using compress As New MemoryStream(pBytes)
                Dim deflatMs As New MemoryStream()

                ' 先读取前两个deflate压缩算法标识字节，然后才能用deflateStream解压
                ' 这个行为与 zlib库、sharpZiplib库等不同（这些库都是直接传入解压）
                compress.ReadByte()
                compress.ReadByte()

                Using deflatestream As New DeflateStream(compress, CompressionMode.Decompress, False)
                    deflatestream.CopyTo(deflatMs, 8192)
                End Using

                Return deflatMs
            End Using
        End Function
    End Module
End Namespace

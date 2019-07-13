#Region "Microsoft.VisualBasic::2bc81f82cf1a26d06e4d18edad34ebe8, Microsoft.VisualBasic.Core\Net\HTTP\Stream\ZipStream.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module ZipStreamExtensions
    ' 
    '         Function: Deflate, UnZipStream
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
        Public Function UnZipStream(stream As IEnumerable(Of Byte), Optional noMagic As Boolean = False) As MemoryStream
            Dim pBytes = stream.SafeQuery.ToArray

            ' Deflate 算法压缩之后的数据，第一个字节是 78h（120b），第二个字节是 DAh(218b)
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

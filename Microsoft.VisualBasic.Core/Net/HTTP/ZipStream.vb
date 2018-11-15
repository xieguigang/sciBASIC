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
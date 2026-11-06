Option Strict On
Option Explicit On
Option Infer On

Imports System.IO
Imports System.IO.Compression
Imports SnappyCodec = Microsoft.VisualBasic.Data.IO.Snappy.Snappy

''' <summary>
''' Parquet 页面数据的压缩与解压分发层。
''' <para>
''' SNAPPY 直接复用基础代码库中的 raw block 实现（<c>Microsoft.VisualBasic.Data.IO.Snappy.Snappy</c>），
''' GZIP 使用 <see cref="GZipStream"/>，其余编码暂不支持。
''' </para>
''' </summary>
Public Module ParquetCodec

    ''' <summary>
    ''' 压缩一段页面数据。
    ''' </summary>
    Public Function Compress(codec As Format.CompressionCodec, data As Byte()) As Byte()
        Select Case codec
            Case Format.CompressionCodec.UNCOMPRESSED
                Return data
            Case Format.CompressionCodec.SNAPPY
                Return SnappyCodec.Compress(data)
            Case Format.CompressionCodec.GZIP
                Using output As New MemoryStream()
                    Using gzip As New GZipStream(output, CompressionMode.Compress, leaveOpen:=True)
                        gzip.Write(data, 0, data.Length)
                    End Using

                    Return output.ToArray()
                End Using
            Case Else
                Throw New ParquetException($"the compression codec {codec} is not supported for writing!")
        End Select
    End Function

    ''' <summary>
    ''' 解压一段页面数据。
    ''' </summary>
    ''' <param name="codec">页面使用的压缩算法</param>
    ''' <param name="data">压缩后的数据</param>
    ''' <param name="uncompressedSize">页面头中声明解压后的字节数</param>
    Public Function Decompress(codec As Format.CompressionCodec, data As Byte(), uncompressedSize As Integer) As Byte()
        Select Case codec
            Case Format.CompressionCodec.UNCOMPRESSED
                Return data
            Case Format.CompressionCodec.SNAPPY
                Dim result As Byte() = SnappyCodec.Uncompress(data)

                Call VerifySize(codec, result.Length, uncompressedSize)

                Return result
            Case Format.CompressionCodec.GZIP
                Using input As New MemoryStream(data, writable:=False)
                    Using gzip As New GZipStream(input, CompressionMode.Decompress)
                        Using output As New MemoryStream()
                            gzip.CopyTo(output)

                            Dim result As Byte() = output.ToArray()

                            Call VerifySize(codec, result.Length, uncompressedSize)

                            Return result
                        End Using
                    End Using
                End Using
            Case Else
                Throw New ParquetException($"the compression codec {codec} is not supported for reading!")
        End Select
    End Function

    Private Sub VerifySize(codec As Format.CompressionCodec, actual As Integer, expected As Integer)
        If expected > 0 AndAlso actual <> expected Then
            Throw New ParquetException($"the {codec} decompressed page size ({actual} bytes) does not match the page header ({expected} bytes)!")
        End If
    End Sub

End Module

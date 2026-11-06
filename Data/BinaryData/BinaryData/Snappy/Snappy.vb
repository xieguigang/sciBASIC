Option Strict On
Option Explicit On

Imports System.IO

Namespace Snappy

    ''' <summary>
    ''' Snappy 原始块格式（raw block format）的压缩与解压入口。
    ''' <para>
    ''' 原始块格式是一段独立的“varint 未压缩长度 + 字面量/复制指令流”，
    ''' 常用于 Parquet、Hadoop 等文件格式的内嵌压缩字段。若需要带流标识、
    ''' 分块与 CRC32C 校验的流式格式，请使用 <see cref="SnappyOutputStream"/>
    ''' 与 <see cref="SnappyInputStream"/>。
    ''' </para>
    ''' </summary>
    Public NotInheritable Class Snappy

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 压缩一个字节数组，返回完整的 Snappy 原始块。
        ''' </summary>
        ''' <exception cref="ArgumentNullException"><paramref name="input"/> 为 Nothing。</exception>
        Public Shared Function Compress(input As Byte()) As Byte()
            If input Is Nothing Then Throw New ArgumentNullException(NameOf(input))
            Return SnappyRawEncoder.Encode(input, 0, input.Length)
        End Function

        ''' <summary>
        ''' 从流中读取全部数据并压缩为 Snappy 原始块。
        ''' </summary>
        Public Shared Function Compress(input As Stream) As Byte()
            If input Is Nothing Then Throw New ArgumentNullException(NameOf(input))
            If Not input.CanRead Then Throw New ArgumentException("输入流必须可读。", NameOf(input))

            Using copy As New MemoryStream()
                input.CopyTo(copy)
                Return Compress(copy.ToArray())
            End Using
        End Function

        ''' <summary>
        ''' 解压一个完整的 Snappy 原始块。
        ''' </summary>
        ''' <exception cref="InvalidDataException">数据被截断、包含非法指令或长度不符。</exception>
        Public Shared Function Uncompress(compressed As Byte()) As Byte()
            If compressed Is Nothing Then Throw New ArgumentNullException(NameOf(compressed))

            Dim pos As Integer = 0
            Dim result As Byte() = SnappyRawDecoder.Decode(compressed, pos, compressed.Length)

            If pos <> compressed.Length Then
                Throw New InvalidDataException($"Snappy: 原始块末尾存在 {compressed.Length - pos} 字节未消费的数据。")
            End If

            Return result
        End Function

        ''' <summary>
        ''' 从流中读取一个完整的 Snappy 原始块并解压。
        ''' </summary>
        Public Shared Function Uncompress(input As Stream) As Byte()
            If input Is Nothing Then Throw New ArgumentNullException(NameOf(input))
            If Not input.CanRead Then Throw New ArgumentException("输入流必须可读。", NameOf(input))

            Using copy As New MemoryStream()
                input.CopyTo(copy)
                Return Uncompress(copy.ToArray())
            End Using
        End Function

        ''' <summary>
        ''' 只读取原始块开头的长度前缀，返回声明的未压缩长度。
        ''' </summary>
        Public Shared Function UncompressedLength(compressed As Byte()) As Integer
            If compressed Is Nothing Then Throw New ArgumentNullException(NameOf(compressed))

            Dim pos As Integer = 0
            Return SnappyRawDecoder.UncompressedLength(compressed, pos, compressed.Length)
        End Function

        ''' <summary>
        ''' 返回在参考实现的最坏情况下，压缩 <paramref name="sourceLength"/> 字节所需的缓冲区上界。
        ''' </summary>
        Public Shared Function MaxCompressedLength(sourceLength As Integer) As Integer
            If sourceLength < 0 Then Throw New ArgumentOutOfRangeException(NameOf(sourceLength))

            Dim bound As Long = 32L + CLng(sourceLength) + CLng(sourceLength \ 6)

            If bound > Integer.MaxValue Then
                Throw New ArgumentOutOfRangeException(NameOf(sourceLength), "数据过大，压缩缓冲区超出 .NET 数组上限。")
            End If

            Return CInt(bound)
        End Function

        ''' <summary>
        ''' 尝试完整解压给定缓冲区，成功返回 True，数据非法返回 False。
        ''' </summary>
        Public Shared Function IsValidCompressedBuffer(compressed As Byte()) As Boolean
            If compressed Is Nothing Then Return False

            Try
                Uncompress(compressed)
                Return True
            Catch ex As InvalidDataException
                Return False
            Catch ex As ArgumentException
                Return False
            End Try
        End Function

    End Class
End Namespace

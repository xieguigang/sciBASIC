Imports System.IO
Imports System.IO.Compression
Imports System.Runtime.CompilerServices

Public Module GZStream

    ''' <summary>
    ''' Unzip the stream data in the <paramref name="base64"/> string.
    ''' </summary>
    ''' <param name="base64$"></param>
    ''' <returns></returns>
    <Extension>
    Public Function UnzipBase64(base64$) As MemoryStream
        Dim bytes As Byte() = Convert.FromBase64String(base64)
        Dim ms As New MemoryStream
        Dim gz As New GZipStream(New MemoryStream(bytes), CompressionMode.Decompress)
        Call gz.CopyTo(ms)
        Return ms
    End Function

    ''' <summary>
    ''' 将目标流对象之中的数据进行zip压缩，然后转换为base64字符串
    ''' </summary>
    ''' <param name="stream"></param>
    ''' <returns></returns>
    <Extension> Public Function ZipAsBase64(stream As Stream) As String
        Dim ms As New MemoryStream
        Dim gz As New GZipStream(ms, CompressionMode.Compress)
        Call stream.CopyTo(gz)
        Dim bytes As Byte() = ms.ToArray
        Dim s$ = Convert.ToBase64String(bytes)
        Return s
    End Function
End Module

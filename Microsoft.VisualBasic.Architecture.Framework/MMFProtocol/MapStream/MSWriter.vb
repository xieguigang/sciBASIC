Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
Imports Microsoft.VisualBasic.Net.Protocols

Namespace MMFProtocol.MapStream

#Region "Nested Type Definitions"

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' mmfServer的主要功能是创建并维护一个内存映射文件
    ''' </remarks>
    Public Class MSWriter : Inherits IMapBase

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="uri"></param>
        ''' <remarks>对象实例会首先尝试以服务器的角色建立连接，当不成功的时候会以客户端的形式建立连接</remarks>
        Sub New(uri As String, chunkSize As Long)
            Call MyBase.New(uri, chunkSize)

            Try
                _mmfileStream =
                    MemoryMappedFiles.MemoryMappedFile.CreateNew(
                    uri, chunkSize)
            Catch ex As Exception
                _mmfileStream =
                    MemoryMappedFiles.MemoryMappedFile.CreateOrOpen(
                    uri, chunkSize)
            Finally
            End Try
        End Sub

        Public Sub WriteStream(byteData As Byte())
            If Not byteData.IsNullOrEmpty Then
                Call _mmfileStream.CreateViewStream.Write(
                    byteData, Scan0, byteData.Length)
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return URI
        End Function
    End Class

    <Serializable> Public Class MMFStream : Inherits RawStream

        ''' <summary>
        ''' Stamp; Update Badge
        ''' </summary>
        Public Property udtBadge As Long

        ''' <summary>
        ''' 内存映射文件中所存储的将要进行进程间交换的数据
        ''' </summary>
        ''' <remarks></remarks>
        Public Property byteData As Byte()

        Sub New()
        End Sub

        Sub New(rawStream As Byte())
            Dim bytTmp As Byte() = New Byte(INT64 - 1) {}
            Call Array.ConstrainedCopy(rawStream, Scan0, bytTmp, Scan0, bytTmp.Length)
            udtBadge = BitConverter.ToInt64(bytTmp, Scan0)
            Dim bytLen As Integer = rawStream.Length - bytTmp.Length
            bytTmp = New Byte(bytLen - 1) {}
            Call Array.ConstrainedCopy(rawStream, INT64, bytTmp, Scan0, bytLen)
            byteData = bytTmp
        End Sub

        Public Overrides Function ToString() As String
            Return $"{NameOf(udtBadge)}:={udtBadge} // {NameOf(byteData)}:={byteData.Length} bytes...."
        End Function

        Public Overrides Function Serialize() As Byte()
            Dim ChunkBuffer As Byte() = New Byte(INT64 + byteData.Length - 1) {}
            Call Array.ConstrainedCopy(BitConverter.GetBytes(udtBadge), Scan0, ChunkBuffer, Scan0, INT64)
            Call Array.ConstrainedCopy(byteData, Scan0, ChunkBuffer, INT64, byteData.Length)

            Return ChunkBuffer
        End Function
    End Class
#End Region
End Namespace
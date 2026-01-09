Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Unit

Namespace Data.Repository

    Public Class FileHashCheckSum

        ''' <summary>
        ''' 支持的哈希算法类型
        ''' </summary>
        Public Enum HashType
            MD5
            SHA1
            SHA256
            SHA384
            SHA512
        End Enum

        ''' <summary>
        ''' 计算文件的哈希值 (优化版，适合大文件)
        ''' </summary>
        ''' <param name="filePath">文件路径</param>
        ''' <param name="algo">哈希算法类型</param>
        ''' <param name="bufferSize">
        ''' 应该小于2GB
        ''' </param>
        ''' <returns>哈希值的十六进制字符串</returns>
        Public Shared Function ComputeHash(filePath As String, Optional algo As HashType = HashType.SHA256, Optional bufferSize As Integer = 32 * ByteSize.MB) As String
            If String.IsNullOrEmpty(filePath) Then
                Throw New ArgumentNullException(NameOf(filePath))
            End If

            If Not File.Exists(filePath) Then
                Throw New FileNotFoundException("找不到文件", filePath)
            End If

            ' 1. 创建哈希算法实例
            Using hashAlgorithm As HashAlgorithm = CreateHashAlgorithm(algo)

                ' 2. 配置文件流参数
                ' bufferSize: 设置为 4MB，这是大文件处理的性能关键
                ' FileOptions.SequentialScan: 提示系统我们将顺序读取，系统会进行预读优化
                Dim fileOptions As FileOptions = FileOptions.SequentialScan

                Using fileStream As New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, fileOptions)
                    ' 3. 分块读取并计算哈希
                    ' 我们不使用 ComputeHash(fileStream) 的重载，因为它的默认缓冲区较小
                    ' 使用 TransformBlock 可以手动控制缓冲区大小
                    Dim buffer(bufferSize - 1) As Byte
                    Dim bytesRead As Integer = 0

                    ' 循环读取文件
                    bytesRead = fileStream.Read(buffer, 0, buffer.Length)
                    While bytesRead > 0
                        ' 将读取的数据块传给哈希算法进行计算
                        hashAlgorithm.TransformBlock(buffer, 0, bytesRead, Nothing, Nothing)
                        bytesRead = fileStream.Read(buffer, 0, buffer.Length)
                    End While

                    ' 4. 完成计算并获取结果
                    ' TransformFinalBlock 必须调用，以告知计算已完成并处理剩余数据（如果有）
                    hashAlgorithm.TransformFinalBlock(buffer, 0, 0)
                    Dim hashBytes As Byte() = hashAlgorithm.Hash

                    ' 5. 将字节数组转换为十六进制字符串
                    Dim sb As New StringBuilder()
                    For Each b As Byte In hashBytes
                        sb.Append(b.ToString("x2")) ' 转为小写十六进制
                    Next

                    Return sb.ToString()
                End Using
            End Using
        End Function

        ''' <summary>
        ''' 工厂方法：根据枚举创建具体的 HashAlgorithm 对象
        ''' </summary>
        Private Shared Function CreateHashAlgorithm(algo As HashType) As HashAlgorithm
            Select Case algo
                Case HashType.MD5 : Return System.Security.Cryptography.MD5.Create()
                Case HashType.SHA1 : Return SHA1.Create()
                Case HashType.SHA256 : Return SHA256.Create()
                Case HashType.SHA384 : Return SHA384.Create()
                Case HashType.SHA512 : Return SHA512.Create()
                Case Else
                    Return SHA256.Create() ' 默认返回 SHA256
            End Select
        End Function

    End Class
End Namespace
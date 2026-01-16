
Imports System.IO
Imports System.Net.Http
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Unit
Imports std = System.Math

Namespace Net.WebClient

    Friend Class AxelTask

        ReadOnly url As String
        ReadOnly startByte As Long
        ReadOnly endByte As Long
        ReadOnly destinationPath As String
        ReadOnly bytesToDownload As Long
        ReadOnly host As Axel

        Public Property bufferSize As Integer = 4 * ByteSize.KB

        Dim finished As Boolean = False
        Dim downloadBytes As Long

        Sub New(host As Axel, startByte As Long, endByte As Long, destinationPath As String)
            Me.host = host
            Me.url = host.url
            Me.startByte = startByte
            Me.endByte = endByte
            Me.destinationPath = destinationPath
            Me.bytesToDownload = endByte - startByte + 1
        End Sub

        Public Async Function DownloadChunkAsync(Optional maxRetries As Integer = 9, Optional connectionTimeoutSeconds As Integer = 30) As Task
            ' --- 重试循环开始 ---
            For retryCount As Integer = 1 To maxRetries
                Try
                    Dim downloadSuccess As Boolean = Await TryDownload(connectionTimeoutSeconds)

                    If downloadSuccess Then
                        ' 如果代码执行到这里，说明下载成功，退出重试循环
                        Exit For
                    End If
                Catch ex As TaskCanceledException
                    ' 当超过30秒无响应（超时）时会触发此异常
                    ' 如果是最后一次重试，则抛出错误；否则稍等后继续下一次循环
                    If retryCount = maxRetries Then
                        Throw New Exception($"下载失败：连接超时（超过 {connectionTimeoutSeconds} 秒无响应）。已重试 {maxRetries} 次仍未成功。", ex)
                    End If
                Catch ex As HttpRequestException
                    ' 当网络错误（如DNS解析失败、连接被拒绝等）时触发
                    If retryCount = maxRetries Then
                        Throw New Exception($"下载失败：网络请求错误。已重试 {maxRetries} 次仍未成功。", ex)
                    End If
                Catch ex As Exception
                    ' 捕获其他所有未知错误
                    If retryCount = maxRetries Then
                        Throw New Exception($"下载失败：发生未知错误。已重试 {maxRetries} 次仍未成功。", ex)
                    End If
                End Try

                ' --- 如果发生错误且未达到最大重试次数 ---
                ' 等待1秒后进行下一次重试，避免立即重试导致系统资源浪费或被服务器封禁
                Await Task.Delay(1000)
            Next

            finished = True
        End Function

        Private Async Function TryDownload(connectionTimeoutSeconds As Integer) As Task(Of Boolean)
            Dim downloadSuccess As Boolean = False

            Using httpClient As New HttpClient()
                ' 1. 设置连接超时为30秒
                httpClient.Timeout = TimeSpan.FromSeconds(connectionTimeoutSeconds)

                Using request As New HttpRequestMessage(HttpMethod.Get, url)
                    ' 设置 Range 请求头
                    request.Headers.Range = New Headers.RangeHeaderValue(startByte, endByte)

                    ' 2. 使用 ResponseHeadersRead 选项
                    ' 这意味着 SendAsync 在收到响应头后就会完成，而不需要等待整个内容下载完。
                    ' 因此，30秒的超时主要作用于"建立连接"阶段，而不是下载阶段。
                    Using response = Await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                        response.EnsureSuccessStatusCode()

                        Using contentStream = Await response.Content.ReadAsStreamAsync()
                            downloadSuccess = Await CopyStream(contentStream)
                        End Using
                    End Using
                End Using
            End Using

            Return downloadSuccess
        End Function

        Private Async Function CopyStream(contentStream As Stream) As Task(Of Boolean)
            Dim buffer As Byte() = New Byte(bufferSize - 1) {}
            Dim bytesRead As Integer
            ' 计算目标大小（用于循环判断，或者用于校验）
            Dim bytesRemaining = bytesToDownload

            downloadBytes = 0

            ' FileMode.Create 确保每次重试都会覆盖之前未完成的文件
            Using fileStream As New FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, True)
                ' 循环直到读取完毕
                Do While bytesRemaining > 0
                    ' 计算本次最多能读多少字节
                    ' 不能超过 buffer 大小，也不能超过剩余需要的字节数
                    ' 这样可以防止 ReadAsync 读取超出 Range 范围的数据（如果有）
                    Dim bytesToRead As Integer = CInt(std.Min(bufferSize, bytesRemaining))

                    ' 使用 bytesToRead 进行读取，而不是 bufferSize
                    bytesRead = Await contentStream.ReadAsync(Buffer, Scan0, bytesToRead)
                    ' 如果读取到0字节，说明流已结束，必须退出循环
                    If bytesRead = 0 Then
                        Exit Do
                    End If

                    ' 只写入实际读取到的字节数，而不是 bufferSize
                    Await fileStream.WriteAsync(Buffer, Scan0, bytesRead)

                    ' 更新剩余字节数
                    bytesRemaining -= bytesRead
                    downloadBytes += bytesRead

                    ' 更新已下载字节数（用于进度条）
                    ' 进度增加量也必须是实际读取量
                    SyncLock host.lockObject
                        host.totalBytesDownloaded += bytesRead
                    End SyncLock
                Loop
            End Using

            If bytesRemaining > 0 Then
                ' bytesRead = 0 的时候提前退出了
                Return False
            Else
                Return True
            End If
        End Function

        Public Overrides Function ToString() As String
            If finished Then
                Return $"Range({StringFormats.Lanudry(startByte)}-{StringFormats.Lanudry(endByte)}) Finished!"
            Else
                Return $"Range({StringFormats.Lanudry(startByte)}-{StringFormats.Lanudry(endByte)}) Downloading {StringFormats.Lanudry(downloadBytes)}"
            End If
        End Function

    End Class
End Namespace
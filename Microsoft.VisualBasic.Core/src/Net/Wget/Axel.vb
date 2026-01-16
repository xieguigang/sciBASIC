#Region "Microsoft.VisualBasic::9d387955ccd0aa2103114c275a1d6e20, Microsoft.VisualBasic.Core\src\Net\Wget\Axel.vb"

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

'   Total Lines: 169
'    Code Lines: 115 (68.05%)
' Comment Lines: 24 (14.20%)
'    - Xml Docs: 12.50%
' 
'   Blank Lines: 30 (17.75%)
'     File Size: 8.10 KB


'     Class Axel
' 
'         Sub: ShowProgress
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Net.Http
Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.ConsoleProgressBar
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Unit
Imports Microsoft.VisualBasic.ValueTypes
Imports std = System.Math

Namespace Net.WebClient

    ''' <summary>
    ''' linux axel liked multiple thread http file download module
    ''' </summary>
    Public Class Axel

        ' --- 配置区 ---
        Private Const DefaultThreadCount As Integer = 4
        ' --- 配置结束 ---

        ' 用于跟踪已下载的总字节数，用于进度显示
        Private Shared totalBytesDownloaded As Long = 0
        Private Shared totalFileSize As Long = 0
        Private Shared lockObject As New Object()

        <STAThread>
        Public Async Function Download(url As String, fileName As String, Optional nThreads As Integer? = Nothing) As Task
            Dim threadCount As Integer = If(nThreads, DefaultThreadCount)

            If threadCount <= 0 Then
                threadCount = DefaultThreadCount
            End If

            Console.WriteLine($"准备下载: {url}")
            Console.WriteLine($"使用 {threadCount} 个线程。")
            Console.WriteLine("正在获取文件信息...")

            ' 2. 异步执行下载任务
            Await DownloadFileAsync(url, fileName, threadCount)
        End Function

        Private Async Function DownloadFileAsync(url As String, fileName As String, threadCount As Integer) As Task
            Using httpClient As New HttpClient()
                ' 3. 获取文件信息
                Dim response = Await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead)

                ' --- 需求1: 处理非200 HTTP错误 ---
                If Not response.IsSuccessStatusCode Then
                    Console.WriteLine($"[错误] 下载失败: {url}")
                    Console.WriteLine($"        状态码: {CInt(response.StatusCode)} ({response.StatusCode})")
                    Console.WriteLine($"        原因: {response.ReasonPhrase}")
                    ' 记录日志后跳过，直接返回
                    Return
                End If

                totalFileSize = response.Content.Headers.ContentLength.GetValueOrDefault()
                If totalFileSize = 0 Then
                    Console.WriteLine("错误: 无法获取文件大小或文件为空。")
                    Return
                End If

                ' --- 需求2: 验证已存在文件 ---
                If File.Exists(fileName) Then
                    Dim existingFileInfo As New FileInfo(fileName)
                    If existingFileInfo.Length = totalFileSize Then
                        Call Console.WriteLine($"[跳过] 文件已存在且大小匹配，跳过下载: {Path.GetFileName(fileName)}")

                        Return
                    Else
                        Console.WriteLine($"[信息] 文件已存在但大小不匹配 (本地: {StringFormats.Lanudry(existingFileInfo.Length)}, 远程: {StringFormats.Lanudry(totalFileSize)})，将重新下载。")
                    End If
                End If

                ' 检查服务器是否支持范围请求
                If Not response.Headers.AcceptRanges.Contains("bytes") Then
                    Console.WriteLine("警告: 服务器不支持多线程下载，将使用单线程下载。")
                    threadCount = 1
                End If

                Console.WriteLine($"文件名: {fileName}")
                Console.WriteLine($"文件大小: {StringFormats.Lanudry(totalFileSize)}")
                Console.WriteLine("-------------------------------------")

                Dim tempFiles As New List(Of String)
                Dim downloadTasks As New List(Of Task)

                ' 5. 创建并启动下载任务
                Dim chunkSize As Long = totalFileSize \ threadCount
                For i As Integer = 0 To threadCount - 1
                    Dim startByte As Long = i * chunkSize
                    Dim endByte As Long = If(i = threadCount - 1, totalFileSize - 1, (i + 1) * chunkSize - 1)

                    ' 为每个分块创建一个临时文件
                    Dim tempFile = Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(fileName)}.part{i}{Path.GetExtension(fileName)}")
                    tempFiles.Add(tempFile)

                    ' 启动异步下载任务
                    Dim downloadTask = DownloadChunkAsync(url, startByte, endByte, tempFile)
                    downloadTasks.Add(downloadTask)
                Next

                ' 6. 等待所有下载任务完成，并显示进度
                Call Task.Run(AddressOf ShowProgress)

                Await Task.WhenAll(downloadTasks)
                ' 确保进度显示任务完成
                ' (在实际应用中，可以用 CancellationTokenSource 来更优雅地停止)
                ' 这里简单等待一下，让进度条显示100%
                Await Task.Delay(1000)

                ' 7. 合并文件
                Console.WriteLine(vbCrLf & "所有分块下载完成，正在合并文件...")
                Await MergeFilesAsync(tempFiles, fileName)

                Call Console.WriteLine("文件合并完成！")

                ' 8. 清理临时文件
                For Each tempFile In tempFiles
                    Try
                        File.Delete(tempFile)
                    Catch
                        ' 忽略清理错误
                    End Try
                Next

                Console.WriteLine($"下载完成: {Path.GetFullPath(fileName)}")
            End Using
        End Function

        Private Async Function DownloadChunkAsync(url As String, startByte As Long, endByte As Long, destinationPath As String) As Task
            ' 配置参数
            Dim maxRetries As Integer = 9
            Dim connectionTimeoutSeconds As Integer = 30
            Dim bufferSize As Integer = 4 * ByteSize.KB
            Dim bytesToDownload As Long = endByte - startByte + 1

            ' --- 重试循环开始 ---
            For retryCount As Integer = 1 To maxRetries
                Try
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
                                    ' FileMode.Create 确保每次重试都会覆盖之前未完成的文件
                                    Using fileStream As New FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, True)
                                        Dim buffer As Byte() = New Byte(bufferSize - 1) {}
                                        ' 计算目标大小（用于循环判断，或者用于校验）
                                        Dim bytesRemaining As Long = bytesToDownload
                                        Dim bytesRead As Integer

                                        ' 循环直到读取完毕
                                        Do While bytesRemaining > 0
                                            ' 计算本次最多能读多少字节
                                            ' 不能超过 buffer 大小，也不能超过剩余需要的字节数
                                            ' 这样可以防止 ReadAsync 读取超出 Range 范围的数据（如果有）
                                            Dim bytesToRead As Integer = CInt(std.Min(bufferSize, bytesRemaining))

                                            ' 使用 bytesToRead 进行读取，而不是 bufferSize
                                            bytesRead = Await contentStream.ReadAsync(buffer, Scan0, bytesToRead)
                                            ' 如果读取到0字节，说明流已结束，必须退出循环
                                            If bytesRead = 0 Then
                                                Exit Do
                                            End If

                                            ' 只写入实际读取到的字节数，而不是 bufferSize
                                            Await fileStream.WriteAsync(buffer, Scan0, bytesRead)

                                            ' 更新剩余字节数
                                            bytesRemaining -= bytesRead
                                            ' 更新已下载字节数（用于进度条）
                                            ' 进度增加量也必须是实际读取量
                                            SyncLock lockObject
                                                totalBytesDownloaded += bytesRead
                                            End SyncLock
                                        Loop

                                        If bytesRemaining > 0 Then
                                            ' bytesRead = 0 的时候提前退出了
                                            downloadSuccess = False
                                        Else
                                            downloadSuccess = True
                                        End If
                                    End Using
                                End Using
                            End Using
                        End Using
                    End Using

                    ' 如果代码执行到这里，说明下载成功，退出重试循环
                    Exit For

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
        End Function


        Private Async Function MergeFilesAsync(sourceFiles As List(Of String), destinationPath As String) As Task
            Using destinationStream As New FileStream(destinationPath, FileMode.Create)
                For Each sourceFile In sourceFiles
                    Using sourceStream As New FileStream(sourceFile, FileMode.Open)
                        Await sourceStream.CopyToAsync(destinationStream)
                    End Using
                Next
            End Using
        End Function

        Private Sub ShowProgress()
            Dim t0 As Double = Now.UnixTimeStamp

            Using bar As New ProgressBar With {.Maximum = totalFileSize}
                Dim dt As Double = (Now.UnixTimeStamp - t0) + 0.00001
                Dim speed As Double = totalBytesDownloaded / dt

                Call bar.Text.Description.Processing _
                    .AddNew _
                    .SetValue(Function(b)
                                  Return $"下载进度 {(b.Value / totalFileSize * 100).ToString("F2")}%  {StringFormats.Lanudry(speed)}/s  [{StringFormats.Lanudry(b.Value)}/{StringFormats.Lanudry(totalFileSize)}]"
                              End Function)

                While totalBytesDownloaded < totalFileSize
                    Thread.Sleep(300)
                    bar.SetValue(totalBytesDownloaded)
                End While
            End Using
        End Sub
    End Class

End Namespace

Imports System.IO
Imports System.Net.Http

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
                response.EnsureSuccessStatusCode()

                totalFileSize = response.Content.Headers.ContentLength.GetValueOrDefault()
                If totalFileSize = 0 Then
                    Console.WriteLine("错误: 无法获取文件大小或文件为空。")
                    Return
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
                Dim progressTask = Task.Run(AddressOf ShowProgress)
                Await Task.WhenAll(downloadTasks)
                ' 确保进度显示任务完成
                ' (在实际应用中，可以用 CancellationTokenSource 来更优雅地停止)
                ' 这里简单等待一下，让进度条显示100%
                System.Threading.Thread.Sleep(500)


                ' 7. 合并文件
                Console.WriteLine(vbCrLf & "所有分块下载完成，正在合并文件...")
                Await MergeFilesAsync(tempFiles, fileName)
                Console.WriteLine("文件合并完成！")

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
            Using httpClient As New HttpClient()
                Using request As New HttpRequestMessage(HttpMethod.Get, url)
                    ' 设置 Range 请求头
                    request.Headers.Range = New Headers.RangeHeaderValue(startByte, endByte)

                    Using response = Await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                        response.EnsureSuccessStatusCode()

                        Using contentStream = Await response.Content.ReadAsStreamAsync()
                            Using fileStream = New FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, True)
                                Await contentStream.CopyToAsync(fileStream)
                                ' 更新已下载字节数（用于进度条）
                                SyncLock lockObject
                                    totalBytesDownloaded += (endByte - startByte + 1)
                                End SyncLock
                            End Using
                        End Using
                    End Using
                End Using
            End Using
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
            Dim cursorTop = Console.CursorTop
            While totalBytesDownloaded < totalFileSize
                Dim progress = CInt((totalBytesDownloaded / totalFileSize) * 100)
                Dim downloadedSize = StringFormats.Lanudry(totalBytesDownloaded)
                Dim speed = "N/A" ' 简化版，不计算实时速度

                Dim progressText = $"[{progress}%] [{downloadedSize}/{StringFormats.Lanudry(totalFileSize)}] [Speed: {speed}]"

                ' 使用 \r 回到行首，实现进度条更新效果
                Console.SetCursorPosition(0, cursorTop)
                Console.Write(progressText.PadRight(Console.WindowWidth - 1))

                System.Threading.Thread.Sleep(200) ' 每200ms更新一次
            End While
        End Sub
    End Class

End Namespace
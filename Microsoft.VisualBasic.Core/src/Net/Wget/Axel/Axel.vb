#Region "Microsoft.VisualBasic::23e3976c6aa9ba3a7b38923fab61aa2a, Microsoft.VisualBasic.Core\src\Net\Wget\Axel\Axel.vb"

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

    '   Total Lines: 193
    '    Code Lines: 133 (68.91%)
    ' Comment Lines: 21 (10.88%)
    '    - Xml Docs: 14.29%
    ' 
    '   Blank Lines: 39 (20.21%)
    '     File Size: 8.11 KB


    '     Class Axel
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: ShowProgress
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Net.Http
Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.ConsoleProgressBar
Imports Microsoft.VisualBasic.ValueTypes

Namespace Net.WebClient

    ''' <summary>
    ''' linux axel liked multiple thread http file download module
    ''' </summary>
    Public Class Axel

        ' --- 配置区 ---
        Private Const DefaultThreadCount As Integer = 4
        ' --- 配置结束 ---

        ' 用于跟踪已下载的总字节数，用于进度显示
        Friend totalBytesDownloaded As Long = 0
        Friend totalFileSize As Long = 0

        Friend ReadOnly url As String
        Friend ReadOnly lockObject As New Object()

        Dim axelTasks As New List(Of AxelTask)

        Sub New(url As String)
            Me.url = url
        End Sub

        <STAThread>
        Public Async Function Download(fileName As String, Optional nThreads As Integer? = Nothing) As Task
            Dim threadCount As Integer = If(nThreads, DefaultThreadCount)

            If threadCount <= 0 Then
                threadCount = DefaultThreadCount
            End If

            Console.WriteLine($"准备下载: {url}")
            Console.WriteLine($"使用 {threadCount} 个线程。")
            Console.WriteLine("正在获取文件信息...")

            ' 2. 异步执行下载任务
            Await DownloadFileAsync(fileName, threadCount)
        End Function

        Private Async Function DownloadFileAsync(fileName As String, threadCount As Integer) As Task
            Dim info As AxelRequest = Await New AxelRequest(url).RequestInfo

            If info.RequestError Then
                Return
            Else
                totalFileSize = info.TotalFileBytes

                If Not info.SupportsMultipleConnection Then
                    threadCount = 1
                End If
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
                Dim task As New AxelTask(Me, startByte, endByte, tempFile)

                Call tempFiles.Add(tempFile)
                Call axelTasks.Add(task)
                ' 启动异步下载任务
                Call downloadTasks.Add(task.DownloadChunkAsync(maxRetries:=30))
            Next

            ' 6. 等待所有下载任务完成，并显示进度
            Call Task.Run(AddressOf ShowProgress)

            Await Task.WhenAll(downloadTasks)
            ' 确保进度显示任务完成
            ' (在实际应用中，可以用 CancellationTokenSource 来更优雅地停止)
            ' 这里简单等待一下，让进度条显示100%
            Await Task.Delay(1000)

            If axelTasks.All(Function(t) t.DownloadSuccess) Then
                ' 7. 合并文件
                Console.WriteLine(vbCrLf & "所有分块下载完成，正在合并文件...")
                Await MergeFilesAsync(tempFiles, fileName)

                Call Console.WriteLine("文件合并完成！")

                ' 8. 清理临时文件
                For Each tempFile As String In tempFiles
                    Try
                        File.Delete(tempFile)
                    Catch
                        ' 忽略清理错误
                    End Try
                Next

                Console.WriteLine($"[{Now.ToString}] 下载完成: {Path.GetFullPath(fileName)}")
            Else
                Call Console.WriteLine("文件下载错误！")
            End If
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

        ' 取消令牌源，用于控制任务的取消
        Friend cts As New CancellationTokenSource()

        Private Sub ShowProgress()
            Dim t0 As Double = Now.UnixTimeStamp
            Dim dt As Double
            Dim speed As Double

            Using bar As New ProgressBar With {.Maximum = totalFileSize, .Delay = 400}
                Call bar.Text.Description.Processing _
                    .AddNew _
                    .SetValue(Function(b)
                                  Return $"下载进度 {(b.Value / totalFileSize * 100).ToString("F2")}%  {StringFormats.Lanudry(speed)}/s  [{StringFormats.Lanudry(b.Value)}/{StringFormats.Lanudry(totalFileSize)}]"
                              End Function)

                For Each task As AxelTask In axelTasks
                    Call bar.Text.Description.Processing _
                        .AddNew _
                        .SetValue(Function(b)
                                      Return task.ToString
                                  End Function)
                Next

                Dim previousBytes As Long = totalBytesDownloaded
                Dim t1 As Date = Now

                While totalBytesDownloaded < totalFileSize
                    Thread.Sleep(300)
                    bar.SetValue(totalBytesDownloaded)
                    dt = (Now.UnixTimeStamp - t0) + 0.00001
                    speed = totalBytesDownloaded / dt

                    If previousBytes = totalBytesDownloaded Then
                        ' no download data
                        Dim zerospan As TimeSpan = Now - t1

                        If zerospan.TotalSeconds > 30 Then
                            ' TODO: 超过30秒没有下载进度，则在这里中断所有任务，进行重试
                            Console.WriteLine("[警告] 检测到下载停滞超过30秒，正在中断未完成的任务并重试...")
                            cts.Cancel()
                            Thread.Sleep(1000)
                            cts.Dispose()
                            cts = New CancellationTokenSource()
                            t1 = Now
                        End If
                    Else
                        previousBytes = totalBytesDownloaded
                        t1 = Now
                    End If
                End While

                Call Console.WriteLine($"[{Now.ToString} 下载成功！]")
            End Using
        End Sub
    End Class

End Namespace

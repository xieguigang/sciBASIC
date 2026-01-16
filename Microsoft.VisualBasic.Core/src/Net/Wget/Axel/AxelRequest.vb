Imports System.Net.Http

Namespace Net.WebClient

    Public Class AxelRequest

        Public ReadOnly Property TotalFileBytes As Long
        Public ReadOnly Property SupportsMultipleConnection As Boolean = True
        Public ReadOnly Property Url As String
        Public ReadOnly Property RequestError As Boolean = False

        Sub New(url As String)
            Me.Url = url
        End Sub

        Public Async Function RequestInfo() As Task(Of AxelRequest)
            Using httpClient As New HttpClient()
                ' 3. 获取文件信息
                Dim response = Await httpClient.GetAsync(Url, HttpCompletionOption.ResponseHeadersRead)

                ' --- 需求1: 处理非200 HTTP错误 ---
                If Not response.IsSuccessStatusCode Then
                    Console.WriteLine($"[错误] 下载失败: {Url}")
                    Console.WriteLine($"        状态码: {CInt(response.StatusCode)} ({response.StatusCode})")
                    Console.WriteLine($"        原因: {response.ReasonPhrase}")

                    ' 记录日志后跳过，直接返回
                    _RequestError = True
                    Return Me
                Else
                    _RequestError = False
                End If

                _TotalFileBytes = response.Content.Headers.ContentLength.GetValueOrDefault()

                If _TotalFileBytes = 0 Then
                    Console.WriteLine("错误: 无法获取文件大小或文件为空。")
                    _RequestError = True
                    Return Me
                End If

                ' 检查服务器是否支持范围请求
                If Not response.Headers.AcceptRanges.Contains("bytes") Then
                    Console.WriteLine("警告: 服务器不支持多线程下载，将使用单线程下载。")
                    _SupportsMultipleConnection = False
                End If
            End Using

            Return Me
        End Function

    End Class
End Namespace
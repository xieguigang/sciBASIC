#Region "Microsoft.VisualBasic::eec16d3a00bcffbda6561081042dbf6a, Microsoft.VisualBasic.Core\src\Net\Wget\Axel\AxelRequest.vb"

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

    '   Total Lines: 53
    '    Code Lines: 37 (69.81%)
    ' Comment Lines: 4 (7.55%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 12 (22.64%)
    '     File Size: 2.06 KB


    '     Class AxelRequest
    ' 
    '         Properties: RequestError, SupportsMultipleConnection, TotalFileBytes, Url
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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

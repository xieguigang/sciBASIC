#Region "Microsoft.VisualBasic::ac22a2658404e4aedaa58e4e7bc43049, Microsoft.VisualBasic.Core\src\Extensions\WebServices\HttpGet.vb"

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

    '   Total Lines: 240
    '    Code Lines: 166 (69.17%)
    ' Comment Lines: 36 (15.00%)
    '    - Xml Docs: 58.33%
    ' 
    '   Blank Lines: 38 (15.83%)
    '     File Size: 9.21 KB


    ' Module HttpGet
    ' 
    '     Properties: HttpRequestTimeOut
    ' 
    '     Function: [GET], BuildWebRequest, httpRequest, LogException, UrlGet
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.IO.Compression
Imports System.Net
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser
Imports ASCII = Microsoft.VisualBasic.Text.ASCII

''' <summary>
''' Tools for http get
''' </summary>
Public Module HttpGet

    ''' <summary>
    ''' Get the html page content from a website request or a html file on the local filesystem.
    ''' (同时支持http位置或者本地文件，失败或者错误会返回空字符串)
    ''' </summary>
    ''' <param name="url">web http request url or a file path handle</param>
    ''' <param name="retry">发生错误的时候的重试的次数</param>
    ''' <param name="timeoutSec">设置请求超时的时间长度，单位为秒</param>
    ''' <returns>失败或者错误会返回空字符串</returns>
    ''' <remarks>这个工具只适合于文本数据的传输操作</remarks>
    ''' 
    <Extension>
    Public Function [GET](url As String,
                          <Parameter("Request.TimeOut")>
                          Optional retry As UInt16 = 0,
                          Optional headers As Dictionary(Of String, String) = Nothing,
                          Optional proxy As String = Nothing,
                          Optional doNotRetry404 As Boolean = True,
                          Optional UA$ = Nothing,
                          Optional refer$ = Nothing,
                          Optional ByRef is404 As Boolean = False,
                          Optional echo As Boolean = True,
                          Optional timeoutSec As Long = 6000) As String

        Dim isFileUrl As String = (InStr(url, "http://", CompareMethod.Text) <> 1) AndAlso (InStr(url, "https://", CompareMethod.Text) <> 1)

        ' do status indicator reset
        is404 = False

        ' 类似于php之中的file_get_contents函数,可以读取本地文件内容
        If File.Exists(url) Then
            If echo Then
                Call $"GET {If(isFileUrl, url.ToFileURL, url)}".debug
            End If

            Return url.ReadAllText
        Else
            If isFileUrl Then
                If echo Then
                    Call $"URL {url.ToFileURL} can not be solved on your filesystem!".Warning
                End If

                is404 = True
                Return ""
            End If
        End If

        If Not refer.StringEmpty Then
            If headers Is Nothing Then
                headers = New Dictionary(Of String, String)
            End If

            headers(NameOf(refer)) = refer
        End If

        Return url.httpRequest(retry, headers, proxy, doNotRetry404, UA, is404, echo, timeoutSec)
    End Function

    <Extension>
    Private Function httpRequest(url$,
                                 retries%,
                                 headers As Dictionary(Of String, String),
                                 proxy$,
                                 doNotRetry404 As Boolean,
                                 UA$,
                                 ByRef is404 As Boolean,
                                 echo As Boolean,
                                 timeout As Long) As String

        Dim retryTime As Integer = 0

        If String.IsNullOrEmpty(proxy) Then
            proxy = WebServiceUtils.Proxy
        End If

        Try
Re0:
            Return BuildWebRequest(url, headers, proxy, UA, timeout:=timeout).UrlGet(echo:=echo).html

            ' 20230620 the http error message at here could be various
            ' just check for the http status code 404 at here
            ' default message for 404: (404) Not Found
            ' but it also could be a custom error text, example like the ncbi web server response: (404) PUGREST.NotFound 
            ' so just check for the http error code 404 at here
        Catch ex As Exception When InStr(ex.Message, "(404)") > 0 AndAlso doNotRetry404
            is404 = True
            Return LogException(url, New Exception(url, ex))

        Catch ex As Exception When retryTime < retries

            retryTime += 1

            Call "Data download error, retry connect to the server!".PrintException
            GoTo Re0

        Catch ex As Exception
            is404 = InStr(ex.Message, "(404)") > 0
            ex = New Exception(url, ex)
            ex.PrintException

            Return LogException(url, ex)
        End Try
    End Function

    Private Function LogException(url$, ex As Exception) As String
        Dim exMsg As String = {
            "Unable to get the http request!",
           $"  Url:=[{url}]",
            "  EXCEPTION ===>",
            "",
            ex.ToString
        }.JoinBy(ASCII.LF)

        Return App.LogException(exMsg, NameOf([GET]) & "::HTTP_REQUEST_EXCEPTION")
    End Function

    Const doctorcomError$ = "Please login your Campus Broadband Network Client at first!"

    ''' <summary>
    ''' Request timeout unit in seconds.
    ''' </summary>
    ''' <returns></returns>
    Public Property HttpRequestTimeOut As Double

    Public Function BuildWebRequest(url$,
                                    headers As Dictionary(Of String, String),
                                    proxy$,
                                    UA$,
                                    Optional isPost As Boolean = False,
                                    Optional timeout As Long = 600) As HttpWebRequest

        Dim webRequest As HttpWebRequest = HttpWebRequest.Create(url)

        webRequest.Headers.Add("Accept-Language", "en-US,en;q=0.8,zh-Hans-CN;q=0.5,zh-Hans;q=0.3")
        webRequest.UserAgent = UA Or DefaultUA

        If isPost Then
            webRequest.Method = "POST"
        End If

        If HttpRequestTimeOut > 0 Then
            webRequest.Timeout = 1000 * HttpRequestTimeOut
        Else
            webRequest.Timeout = 1000 * timeout
        End If

        If Not headers.IsNullOrEmpty Then
            For Each x As KeyValuePair(Of String, String) In headers
                webRequest.Headers(x.Key) = x.Value
            Next
        End If
        If Not String.IsNullOrEmpty(proxy) Then
            webRequest.SetProxy(proxy)
        Else
            webRequest.Proxy = Nothing
        End If

        Return webRequest
    End Function

    ''' <summary>
    ''' Perform a web url query request
    ''' </summary>
    ''' <param name="webrequest"></param>
    ''' <returns></returns>
    <Extension>
    Public Function UrlGet(webrequest As HttpWebRequest, echo As Boolean) As WebResponseResult
        Dim timer As Stopwatch = Stopwatch.StartNew
        Dim url As String = webrequest.RequestUri.ToString
        Dim html As String

        Using response As HttpWebResponse = webrequest.GetResponse
            ' 检查内容编码是否为gzip
            If response.ContentEncoding.ToLower().Contains("gzip") Then
                ' 使用GZipStream解压缩响应流
                Using gzipStream As New GZipStream(response.GetResponseStream(), CompressionMode.Decompress)
                    ' 读取解压缩后的流
                    Using reader As New StreamReader(gzipStream, Encoding.UTF8)
                        ' 返回响应内容
                        html = reader.ReadToEnd()
                    End Using
                End Using
            Else
                ' 如果不是gzip压缩，直接读取响应流
                Using reader As New StreamReader(response.GetResponseStream(), Encoding.UTF8)
                    ' 返回响应内容
                    html = reader.ReadToEnd()
                End Using
            End If

            Dim timespan As Long = timer.ElapsedMilliseconds
            Dim headers As New ResponseHeaders(response.Headers)

            ' 判断是否是由于还没有登陆校园网客户端而导致的错误
            If InStr(html, "http://www.doctorcom.com", CompareMethod.Text) > 0 Then
                Call doctorcomError.PrintException

                Return New WebResponseResult With {
                    .url = url,
                    .html = ""
                }
            ElseIf echo Then
                Dim title As String = html.HTMLTitle
                Dim time$ = StringFormats.ReadableElapsedTime(timespan)
                Dim debug$ = $"[{url}] {title} - {Len(html)} chars in {time}"

                If timespan > 1000 Then
                    Call debug.Warning
                Else
                    Call debug.info
                End If
            End If

#If DEBUG Then
            Call html.SaveTo($"{App.AppSystemTemp}/{App.PID}/{url.NormalizePathString}.html")
#End If
            Return New WebResponseResult With {
                .url = url,
                .timespan = timespan,
                .html = html,
                .headers = headers
            }
        End Using
    End Function
End Module

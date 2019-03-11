#Region "Microsoft.VisualBasic::e72c1819df62abe71b1a2879fc89c6d0, Microsoft.VisualBasic.Core\Extensions\WebServices\HttpGet.vb"

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

    ' Module HttpGet
    ' 
    '     Properties: HttpRequestTimeOut
    ' 
    '     Function: [GET], __get, __httpRequest, Get_PageContent, LogException
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Net
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser

''' <summary>
''' Tools for http get
''' </summary>
Public Module HttpGet

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' Get the html page content from a website request or a html file on the local filesystem.
    ''' (同时支持http位置或者本地文件，失败或者错误会返回空字符串)
    ''' </summary>
    ''' <param name="url">web http request url or a file path handle</param>
    ''' <param name="retry">发生错误的时候的重试的次数</param>
    ''' <returns>失败或者错误会返回空字符串</returns>
    ''' <remarks>这个工具只适合于文本数据的传输操作</remarks>
    <ExportAPI("Webpage.Request", Info:="Get the html page content from a website request Or a html file on the local filesystem.")>
    <Extension> Public Function [GET](url As String,
                                      <Parameter("Request.TimeOut")>
                                      Optional retry As UInt16 = 0,
                                      <Parameter("FileSystem.Works?", "Is this a local html document on your filesystem?")>
                                      Optional isFileUrl As Boolean = False,
                                      Optional headers As Dictionary(Of String, String) = Nothing,
                                      Optional proxy As String = Nothing,
                                      Optional doNotRetry404 As Boolean = True,
                                      Optional UA$ = Nothing,
                                      Optional refer$ = Nothing) As String
#Else
    ''' <summary>
    ''' Get the html page content from a website request or a html file on the local filesystem.
    ''' </summary>
    ''' <param name="url">web http request url or a file path handle</param>
    ''' <param name="RequestTimeOut">发生错误的时候的重试的次数</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <Extension> Public Function Get_PageContent(url As String, Optional RequestTimeOut As UInteger = 20, Optional FileSystemUrl As Boolean = False) As String
#End If
        ' Call $"Request data from: {If(isFileUrl, url.ToFileURL, url)}".__DEBUG_ECHO
        Call $"GET {If(isFileUrl, url.ToFileURL, url)}".__DEBUG_ECHO

        If FileIO.FileSystem.FileExists(url) Then
            Call "[Job DONE!]".__DEBUG_ECHO
            Return FileIO.FileSystem.ReadAllText(url)
        Else
            If isFileUrl Then
                Call $"URL {url.ToFileURL} can not solved on your filesystem!".Warning
                Return ""
            End If
        End If

        If Not refer.StringEmpty Then
            If headers Is Nothing Then
                headers = New Dictionary(Of String, String)
            End If

            headers(NameOf(refer)) = refer
        End If

        Return url.__httpRequest(retry, headers, proxy, doNotRetry404, UA)
    End Function

    <Extension>
    Private Function __httpRequest(url$, retries%, headers As Dictionary(Of String, String), proxy$, DoNotRetry404 As Boolean, UA$) As String
        Dim retryTime As Integer = 0

        If String.IsNullOrEmpty(proxy) Then
            proxy = WebServiceUtils.Proxy
        End If

        Try
RETRY:      Return __get(url, headers, proxy, UA)
        Catch ex As Exception When InStr(ex.Message, "(404) Not Found") > 0 AndAlso DoNotRetry404
            Return LogException(url, New Exception(url, ex))

        Catch ex As Exception When retryTime < retries

            retryTime += 1

            Call "Data download error, retry connect to the server!".PrintException
            GoTo RETRY

        Catch ex As Exception
            ex = New Exception(url, ex)
            ex.PrintException

            Return LogException(url, ex)
        End Try
    End Function

    Private Function LogException(url As String, ex As Exception) As String
        Dim exMessage As String = String.Format("Unable to get the http request!" & vbCrLf &
                                                "  Url:=[{0}]" & vbCrLf &
                                                "  EXCEPTION ===>" & vbCrLf & ex.ToString, url)
        Call App.LogException(exMessage, NameOf([GET]) & "::HTTP_REQUEST_EXCEPTION")
        Return ""
    End Function

    Const doctorcomError$ = "Please login your Campus Broadband Network Client at first!"

    ''' <summary>
    ''' Request timeout unit in seconds.
    ''' </summary>
    ''' <returns></returns>
    Public Property HttpRequestTimeOut As Double

    Private Function __get(url$, headers As Dictionary(Of String, String), proxy$, UA$) As String
        Dim timer As Stopwatch = Stopwatch.StartNew
        Dim webRequest As HttpWebRequest = HttpWebRequest.Create(url)

        webRequest.Headers.Add("Accept-Language", "en-US,en;q=0.8,zh-Hans-CN;q=0.5,zh-Hans;q=0.3")
        webRequest.UserAgent = UA Or DefaultUA

        If HttpRequestTimeOut > 0 Then
            webRequest.Timeout = 1000 * HttpRequestTimeOut
        End If

        If Not headers.IsNullOrEmpty Then
            For Each x In headers
                webRequest.Headers(x.Key) = x.Value
            Next
        End If
        If Not String.IsNullOrEmpty(proxy) Then
            Call webRequest.SetProxy(proxy)
        End If

        Using respStream As Stream = webRequest.GetResponse.GetResponseStream,
            reader As New StreamReader(respStream)

            Dim htmlBuilder As New StringBuilder
            Dim line As Value(Of String) = ""

            Do While Not (line = reader.ReadLine) Is Nothing
                htmlBuilder.AppendLine(line)
            Loop

            Dim html As String = htmlBuilder.ToString
            Dim title As String = html.HTMLTitle

            ' 判断是否是由于还没有登陆校园网客户端而导致的错误
            If InStr(html, "http://www.doctorcom.com", CompareMethod.Text) > 0 Then
                Call doctorcomError.PrintException
                Return ""
            End If

            Call $"[{title}  {url}] --> sizeOf:={Len(html)} chars; response_time:={timer.ElapsedMilliseconds} ms.".__DEBUG_ECHO
#If DEBUG Then
            Call html.SaveTo($"{App.AppSystemTemp}/{App.PID}/{url.NormalizePathString}.html")
#End If
            Return html
        End Using
    End Function
End Module

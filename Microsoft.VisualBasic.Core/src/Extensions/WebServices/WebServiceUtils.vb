#Region "Microsoft.VisualBasic::4a63c9a99364e3b25ba4b7731ff340b9, Microsoft.VisualBasic.Core\src\Extensions\WebServices\WebServiceUtils.vb"

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

    ' Module WebServiceUtils
    ' 
    '     Properties: DefaultUA, LocalIPAddress, Protocols, Proxy
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: BuildArgs, (+2 Overloads) BuildReqparm, BuildUrlData, CheckValidationResult, DownloadFile
    '               GetDownload, getIPAddressInternal, GetMyIPAddress, GetProxy, (+2 Overloads) GetRequest
    '               GetRequestRaw, IsSocketPortOccupied, isURL, IsURLPattern, ParseUrlQueryParameters
    '               (+2 Overloads) POST, POSTFile, (+2 Overloads) PostRequest, PostUrlDataParser, QueryStringParameters
    '               UrlDecode, UrlEncode, UrlPathEncode
    ' 
    '     Sub: (+2 Overloads) SetProxy, UrlDecode, UrlEncode
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Specialized
Imports System.IO
Imports System.Net
Imports System.Net.Security
Imports System.Runtime.CompilerServices
Imports System.Security.Cryptography.X509Certificates
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Web
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.FileIO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq.Extensions
#If NET_48 = 1 Or netcore5 = 1 Then
Imports Microsoft.VisualBasic.Net
#End If
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text
Imports IPEndPoint = Microsoft.VisualBasic.Net.IPEndPoint
Imports r = System.Text.RegularExpressions.Regex

''' <summary>
''' The extension module for web services works.
''' </summary>
'''
<Package("Utils.WebServices",
                  Description:="The extension module for web services programming in your scripting.",
                  Category:=APICategories.UtilityTools,
                  Publisher:="<a href=""mailto://xie.guigang@gmail.com"">xie.guigang@gmail.com</a>")>
Public Module WebServiceUtils

    ''' <summary>
    ''' Web protocols enumeration
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Protocols As IReadOnlyCollection(Of String) = {"http://", "https://", "ftp://", "sftp://"}

    Public Const URLPattern$ = "http(s)?://([\w+?\.\w+])+([a-zA-Z0-9\~\!\@\#\$\%\^\&\*\(\)_\-\=\+\\\/\?\.\:\;\'\,]*)?"

    ''' <summary>
    ''' Determine that is this uri string is a network location?
    ''' (判断这个uri字符串是否是一个网络位置)
    ''' </summary>
    ''' <param name="url"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function isURL(url As String) As Boolean
        Return url.IndexOfAny({ASCII.LF, ASCII.CR}) = -1 AndAlso url.InStrAny(DirectCast(Protocols, String())) = 1
    End Function

    ''' <summary>
    ''' is a web location?
    ''' </summary>
    ''' <param name="str"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function IsURLPattern(str As String) As Boolean
        Return str.isURL OrElse str.IsPattern(URLPattern)
    End Function

    ''' <summary>
    ''' Build the request parameters for the HTTP POST
    ''' </summary>
    ''' <param name="dict"></param>
    ''' <returns></returns>
    <Extension>
    Public Function BuildReqparm(dict As Dictionary(Of String, String)) As NameValueCollection
        Dim reqparm As New NameValueCollection

        For Each Value As KeyValuePair(Of String, String) In dict
            Call reqparm.Add(Value.Key, Value.Value)
        Next

        Return reqparm
    End Function

    ''' <summary>
    ''' Build the request parameters for the HTTP POST
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    <Extension>
    Public Function BuildReqparm(data As IEnumerable(Of KeyValuePair(Of String, String))) As Specialized.NameValueCollection
        Dim reqparm As New Specialized.NameValueCollection
        For Each Value As KeyValuePair(Of String, String) In data
            Call reqparm.Add(Value.Key, Value.Value)
        Next
        Return reqparm
    End Function

    Const PortOccupied As String = "Only one usage of each socket address (protocol/network address/port) Is normally permitted"

    ''' <summary>
    ''' Only one usage of each socket address (protocol/network address/port) Is normally permitted
    ''' </summary>
    ''' <param name="ex"></param>
    ''' <returns></returns>
    <Extension> Public Function IsSocketPortOccupied(ex As Exception) As Boolean
        If TypeOf ex Is System.Net.Sockets.SocketException AndAlso
            InStr(ex.ToString, PortOccupied, CompareMethod.Text) Then
            Return True
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' Create a parameter dictionary from the request parameter tokens.
    ''' (请注意，字典的key默认为转换为小写的形式)
    ''' </summary>
    ''' <param name="tokens">
    ''' 元素的个数必须要大于1，因为从url里面解析出来的元素之中第一个元素是url本身，则不再对url做字典解析
    ''' </param>
    ''' <returns>
    ''' ###### 2016-11-21
    ''' 因为post可能会传递数组数据进来，则这个时候就会出现重复的键名，则已经不再适合字典类型了，这里改为返回<see cref="NameValueCollection"/>
    ''' </returns>
    <Extension>
    Public Function ParseUrlQueryParameters(tokens As String(), Optional lowercase As Boolean = True) As NameValueCollection
        Dim query As New NameValueCollection

        If tokens.IsNullOrEmpty Then
            Return query
        End If
        If tokens.Length = 1 Then
            ' 只有url，没有附带的参数，则返回一个空的字典集合
            If InStr(tokens(Scan0), "=") = 0 Then
                Return query
            End If
        End If

        For Each x As NamedValue(Of String) In tokens.Select(Function(q) q.GetTagValue("="))
            Dim name As String = If(lowercase, x.Name.ToLower, x.Name)
            Dim value As String = UrlDecode(x.Value)

            Call query.Add(name, value)
        Next

        Return query
    End Function

    ''' <summary>
    ''' 不像<see cref="PostUrlDataParser(String, Boolean)"/>函数，这个函数不会替换掉转义字符，并且所有的Key都已经被默认转换为小写形式的了
    ''' </summary>
    ''' <param name="url">URL parameters</param>
    ''' <returns></returns>
    <ExportAPI("Request.Parser")>
    <Extension> Public Function QueryStringParameters(url$, Optional transLower As Boolean = True) As NameValueCollection
        Dim tokens$()

        With InStr(url, "://")
            If .ByRef < 10 AndAlso .ByRef > 0 Then
                url = url.GetTagValue("?").Value
            End If

            tokens = url.Split("&"c)
        End With

        Return ParseUrlQueryParameters(tokens, transLower)
    End Function

    ReadOnly urlEscaping As [Default](Of Func(Of String, String)) = New Func(Of String, String)(AddressOf UrlEncode)
    Friend ReadOnly noEscaping As [Default](Of Func(Of String, String)) = New Func(Of String, String)(Function(s) s)

    ''' <summary>
    ''' 生成URL请求的参数
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="escaping">是否进行对value部分的字符串数据进行转义</param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function BuildUrlData(data As IEnumerable(Of KeyValuePair(Of String, String)),
                                             Optional escaping As Boolean = False,
                                             Optional stripNull As Boolean = True) As String
        If stripNull Then
            data = data _
                .Where(Function(a)
                           Return (Not a.Key.StringEmpty) AndAlso (Not a.Value = Nothing)
                       End Function) _
                .ToArray
        End If

        Return data _
            .Select(Function(x)
                        Return $"{x.Key}={(noEscaping Or urlEscaping.When(escaping))(x.Value)}"
                    End Function) _
            .JoinBy("&")
    End Function

    <ExportAPI("Build.Args")>
    Public Function BuildArgs(ParamArray params As String()()) As String
        If params.IsNullOrEmpty Then
            Return ""
        Else
            Dim values = params.Select(Function(arg) $"{arg(Scan0)}={arg(1)}").ToArray
            Return String.Join("&", values)
        End If
    End Function

    ''' <summary>
    ''' 在服务器端对URL进行解码还原
    ''' </summary>
    ''' <param name="s"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    <Extension> <ExportAPI("URL.Decode")>
    Public Function UrlDecode(s$, Optional encoding As Encoding = Nothing) As String
        If s.StringEmpty Then
            Return ""
        End If
        If encoding IsNot Nothing Then
            Return HttpUtility.UrlDecode(s, encoding)
        Else
            Return HttpUtility.UrlDecode(s)
        End If
    End Function

    <ExportAPI("URL.Decode")>
    Public Sub UrlDecode(s As String, ByRef output As TextWriter)
        If s IsNot Nothing Then
            output.Write(UrlDecode(s))
        End If
    End Sub

    ''' <summary>
    ''' 进行url编码，将特殊字符进行转码
    ''' </summary>
    ''' <param name="s"></param>
    ''' <param name="encoding"></param>
    ''' <param name="jswhitespace">
    ''' 空格符号默认被转义为``+``, 如果这个参数为真的话,则空格会被转义为``%20``
    ''' </param>
    ''' <returns></returns>
    ''' <remarks>
    ''' A extension method wrapper for <see cref="HttpUtility.UrlEncode"/>
    ''' </remarks>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <ExportAPI("URL.Encode")>
    <Extension>
    Public Function UrlEncode(s As String, Optional encoding As Encoding = Nothing, Optional jswhitespace As Boolean = False) As String
        Dim component As String

        If encoding IsNot Nothing Then
            component = HttpUtility.UrlEncode(s, encoding)
        Else
            component = HttpUtility.UrlEncode(s)
        End If

        If jswhitespace Then
            ' 20190517 因为+号被转义为%2b,所以在这里可以直接替换
            ' 由空格转义而得到的+符号为%20
            component = component.Replace("+", "%20")
        End If

        Return component
    End Function

    <ExportAPI("URL.Encode")>
    Public Sub UrlEncode(s As String, ByRef output As TextWriter)
        If s IsNot Nothing Then
            output.Write(UrlEncode(s))
        End If
    End Sub

    ''' <summary>
    ''' 编码整个URL，这个函数会自动截取出query string parameter部分，然后对截取出来的query string parameter进行编码
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns></returns>
    <ExportAPI("URL.PathEncode")>
    <Extension>
    Public Function UrlPathEncode(s As String) As String
        If s Is Nothing Then
            Return Nothing
        End If

        Dim idx As Integer = s.IndexOf("?"c)
        Dim s2 As String = Nothing

        If idx <> -1 Then
            s2 = s.Substring(0, idx)
            s2 = HttpUtility.UrlEncode(s2) & s.Substring(idx)
        Else
            s2 = HttpUtility.UrlEncode(s)
        End If

        Return s2
    End Function

    ''' <summary>
    ''' 假若你的数据之中包含有SHA256的加密数据，则非常不推荐使用这个函数进行解析。因为请注意，这个函数会替换掉一些转义字符的，所以会造成一些非常隐蔽的BUG
    ''' </summary>
    ''' <param name="data">转义的时候大小写无关</param>
    ''' <returns></returns>
    '''
    <ExportAPI("PostRequest.Parsing")>
    <Extension> Public Function PostUrlDataParser(data$, Optional toLower As Boolean = True) As NameValueCollection
        If String.IsNullOrEmpty(data) Then
            Return New NameValueCollection
        End If

        Dim params$() = data.Split("&"c)
        Dim table = ParseUrlQueryParameters(params, toLower)
        Return table
    End Function

    ''' <summary>
    ''' GET http request
    ''' </summary>
    ''' <param name="strUrl$"></param>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <Extension>
    Public Function GetRequest(strUrl$, ParamArray args As String()()) As String
        If args.IsNullOrEmpty Then
            Return GetRequest(strUrl)
        Else
            Dim params As String = BuildArgs(args)

            If String.IsNullOrEmpty(params) Then
                Return GetRequest(strUrl)
            Else
                Return GetRequest($"{strUrl}?{params}")
            End If
        End If
    End Function

    ''' <summary>
    ''' GET http request
    ''' </summary>
    ''' <param name="url"></param>
    ''' <returns></returns>
    <Extension>
    Public Function GetRequest(url$, Optional https As Boolean = False, Optional userAgent As String = Nothing) As String
        Dim strData As String = ""
        Dim strValue As New List(Of String)
        Dim reader As New StreamReader(GetRequestRaw(url, https, userAgent), Encoding.UTF8)

        Do While True
            strData = reader.ReadLine()
            If strData Is Nothing Then
                Exit Do
            Else
                strValue += strData
            End If
        Loop

        strData = String.Join(vbCrLf, strValue.ToArray)
        Return strData
    End Function

    Sub New()
        ServicePointManager.ServerCertificateValidationCallback = New RemoteCertificateValidationCallback(AddressOf CheckValidationResult)
    End Sub

    Private Function CheckValidationResult(sender As Object,
                                           certificate As X509Certificate,
                                           chain As X509Chain,
                                           errors As SslPolicyErrors) As Boolean
        Return True
    End Function

    ''' <summary>
    ''' Example for xx-net tool:
    ''' 
    ''' ```
    ''' http://127.0.0.1:8087/
    ''' ```
    ''' </summary>
    ''' <returns></returns>
    Public Property Proxy As String

    ''' <summary>
    ''' GET http request
    ''' </summary>
    ''' <param name="url"></param>
    ''' <param name="https"></param>
    ''' <param name="userAgent">
    ''' 
    ''' fix a bug for github API:
    ''' 
    ''' Protocol violation using Github api
    ''' 
    ''' You need to set UserAgent like this:
    ''' webRequest.UserAgent = "YourAppName"
    ''' Otherwise it will give The server committed a protocol violation. Section=ResponseStatusLine Error.
    ''' </param>
    ''' <returns>
    ''' this function returns a stream object that produced by
    ''' <see cref="HttpWebResponse.GetResponseStream()"/>
    ''' </returns>
    <Extension>
    Public Function GetRequestRaw(url As String,
                                  Optional https As Boolean = False,
                                  Optional userAgent As String = Nothing,
                                  Optional headers As Dictionary(Of String, String) = Nothing) As Stream

        Dim request As HttpWebRequest

        If https Then
            request = WebRequest.CreateDefault(New Uri(url))
        Else
            request = DirectCast(WebRequest.Create(url), HttpWebRequest)
        End If

        request.Method = "GET"
        request.KeepAlive = False
        request.ServicePoint.Expect100Continue = False
        request.UserAgent = userAgent Or DefaultUA

        If Not headers.IsNullOrEmpty Then
            For Each x As KeyValuePair(Of String, String) In headers
                request.Headers(x.Key) = x.Value
            Next
        End If

        Dim response As HttpWebResponse = DirectCast(request.GetResponse, HttpWebResponse)
        Dim s As Stream = response.GetResponseStream()

        Return s
    End Function

    ''' <summary>
    ''' POST http request
    ''' </summary>
    ''' <param name="url"></param>
    ''' <param name="params"></param>
    ''' <returns></returns>
    Public Function PostRequest(url As String, Optional params As IEnumerable(Of KeyValuePair(Of String, String)) = Nothing) As WebResponseResult
        Return url.POST(params.BuildReqparm)
    End Function

    ''' <summary>
    ''' POST http request
    ''' </summary>
    ''' <param name="url"></param>
    ''' <param name="params"></param>
    ''' <returns></returns>
    Public Function PostRequest(url As String, ParamArray params As String()()) As WebResponseResult
        Dim post As KeyValuePair(Of String, String)()

        If params Is Nothing Then
            post = Nothing
        Else
            post = params _
                .Select(Function(value)
                            Return New KeyValuePair(Of String, String)(value(0), value(1))
                        End Function) _
                .ToArray
        End If

        Return PostRequest(url, post)
    End Function

    ''' <summary>
    ''' POST http request for get html.
    ''' (请注意，假若<paramref name="params"/>之中含有字符串数组的话，则会出错，这个时候需要使用
    ''' <see cref="Post(String, Dictionary(Of String, String()), String, String, String)"/>方法)
    ''' </summary>
    ''' <param name="url$"></param>
    ''' <param name="params"></param>
    ''' <param name="Referer"></param>
    ''' <returns>
    ''' this function will returns nothing if the http error happends.
    ''' </returns>
    <Extension>
    Public Function POST(url$,
                         Optional params As NameValueCollection = Nothing,
                         Optional headers As Dictionary(Of String, String) = Nothing,
                         Optional Referer$ = "",
                         Optional proxy$ = Nothing,
                         Optional contentEncoding As Encodings = Encodings.UTF8,
                         Optional retry As Integer = 5) As WebResponseResult

        Static emptyBody As New [Default](Of NameValueCollection) With {
            .value = New NameValueCollection,
            .assert = Function(c)
                          Return c Is Nothing OrElse DirectCast(c, NameValueCollection).Count = 0
                      End Function
        }

        Using request As New WebClient

            Call request.Headers.Add("User-Agent", UserAgent.GoogleChrome)
            Call request.Headers.Add(NameOf(Referer), Referer)

            For Each header In headers.SafeQuery
                If Not request.Headers.ContainsKey(header.Key) Then
                    request.Headers.Add(header.Key, header.Value)
                End If
            Next

            If String.IsNullOrEmpty(proxy) Then
                proxy = WebServiceUtils.Proxy
            End If
            If Not String.IsNullOrEmpty(proxy) Then
                Call request.SetProxy(proxy)
            End If

            Call $"[POST] {url}....".__DEBUG_ECHO

            Dim timer As Stopwatch = Stopwatch.StartNew
            Dim response As Byte() = Nothing
            Dim str$

            For i As Integer = 0 To retry
                Try
                    response = request.UploadValues(url, "POST", params Or emptyBody)
                    Exit For
                Catch ex As Exception
                    Call App.LogException(ex)
                End Try
            Next

            If response Is Nothing Then
                Return Nothing
            Else
                str = contentEncoding _
                    .CodePage _
                    .GetString(response)

                Call $"[GET] {response.Length} bytes...".__DEBUG_ECHO
            End If

            Dim rtvlHeaders As New ResponseHeaders(request.ResponseHeaders)
            Dim result As New WebResponseResult With {
                .url = url,
                .html = str,
                .timespan = timer.ElapsedMilliseconds,
                .headers = rtvlHeaders
            }

            Return result
        End Using
    End Function

    ''' <summary>
    ''' 通过post上传文件
    ''' </summary>
    ''' <param name="url$"></param>
    ''' <param name="name$"></param>
    ''' <param name="referer$"></param>
    ''' <returns></returns>
    <Extension>
    Public Function POSTFile(url$, buffer As Byte(), Optional name$ = "", Optional referer$ = Nothing) As String
        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(url), HttpWebRequest)

        request.Method = "POST"
        request.Accept = "application/json"
        request.ContentLength = buffer.Length
        request.ContentType = "multipart/form-data; boundary=------WebKitFormBoundaryBpijhG6dKsQpCMdN--;"
        request.UserAgent = UserAgent.GoogleChrome
        request.Referer = referer
        '  request.Headers("fileName") = name Or File.FileName.AsDefault

        If Not String.IsNullOrEmpty(Proxy) Then
            Call request.SetProxy(Proxy)
        End If

        Call $"[POST] {url}....".__DEBUG_ECHO

        ' post data Is sent as a stream
        With request.GetRequestStream()
            ' Dim buffer = File.ReadBinary

            ' Call New StreamWriter(.ByRef).Write(vbCrLf)
            ' Call .Flush()
            Call .Write(buffer, Scan0, buffer.Length)
            Call .Flush()
        End With

        ' returned values are returned as a stream, then read into a string
        Dim response = DirectCast(request.GetResponse(), HttpWebResponse)

        Using responseStream As New StreamReader(response.GetResponseStream())
            Dim html As New StringBuilder
            Dim s As New Value(Of String)

            Do While Not (s = responseStream.ReadLine) Is Nothing
                Call html.AppendLine(+s)
            Loop

            Call $"Get {html.Length} bytes from server response...".__DEBUG_ECHO

            Return html.ToString
        End Using
    End Function

    ''' <summary>
    ''' POST http request for get html
    ''' </summary>
    ''' <param name="url$"></param>
    ''' <param name="data"></param>
    ''' <param name="Referer$"></param>
    ''' <returns></returns>
    <Extension>
    Public Function POST(url$, data As Dictionary(Of String, String()),
                        Optional Referer$ = "",
                        Optional proxy$ = Nothing,
                        Optional ua As String = UserAgent.GoogleChrome) As String

        Dim postString As New List(Of String)

        For Each postValue As KeyValuePair(Of String, String()) In data
            postString += postValue.Value _
                .Select(Function(v) postValue.Key & "=" & HttpUtility.UrlEncode(v))
        Next

        Dim postData As String = postString.JoinBy("&")
        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(url), HttpWebRequest)

        request.Method = "POST"
        request.Accept = "application/json"
        request.ContentLength = postData.Length
        request.ContentType = "application/x-www-form-urlencoded; charset=utf-8"
        request.UserAgent = ua
        request.Referer = Referer

        If Not String.IsNullOrEmpty(proxy) Then
            Call request.SetProxy(proxy)
        End If

        Call $"[POST] {url}....".__DEBUG_ECHO

        ' post data Is sent as a stream
        Using sender As New StreamWriter(request.GetRequestStream())
            sender.Write(postData)
        End Using

        ' returned values are returned as a stream, then read into a string
        Dim response = DirectCast(request.GetResponse(), HttpWebResponse)
        Using responseStream As New StreamReader(response.GetResponseStream())
            Dim html As New StringBuilder
            Dim s As New Value(Of String)

            Do While Not (s = responseStream.ReadLine) Is Nothing
                Call html.AppendLine(+s)
            Loop

            Call $"[GET] {html.Length} bytes...".__DEBUG_ECHO

            Return html.ToString
        End Using
    End Function

    <Extension>
    Public Sub SetProxy(ByRef request As HttpWebRequest, proxy As String)
        request.Proxy = proxy.GetProxy
    End Sub

    <Extension>
    Public Sub SetProxy(ByRef request As WebClient, proxy As String)
        request.Proxy = proxy.GetProxy
    End Sub

    <Extension>
    Public Function GetProxy(proxy As String) As WebProxy
        Return New WebProxy With {
            .Address = New Uri(proxy),
            .Credentials = New NetworkCredential()
        }
    End Function

    ''' <summary>
    ''' 设置默认的http请求的user-agent，默认为Google Chrome的UA字符串
    ''' </summary>
    ''' <returns></returns>
    Public Property DefaultUA As [Default](Of String) = UserAgent.GoogleChrome

    ''' <summary>
    ''' download the file from <paramref name="strUrl"></paramref> to <paramref name="save">local file</paramref>.
    ''' </summary>
    ''' <param name="strUrl"></param>
    ''' <param name="save">The file path of the file saved</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension>
    Public Function DownloadFile(<Parameter("url")> strUrl$,
                                 <Parameter("Path.Save", "The saved location of the downloaded file data.")>
                                 save$,
                                 Optional proxy$ = Nothing,
                                 Optional ua$ = Nothing,
                                 Optional retry% = 0,
                                 Optional progressHandle As DownloadProgressChangedEventHandler = Nothing,
                                 Optional refer$ = Nothing,
                                 <CallerMemberName>
                                 Optional trace$ = Nothing) As Boolean
RE0:
        Try
            Using browser As New WebClient()
                If Not String.IsNullOrEmpty(proxy) Then
                    Call browser.SetProxy(proxy)
                End If
                If Not refer.StringEmpty Then
                    browser.Headers.Add(NameOf(refer), refer)
                End If
                If Not progressHandle Is Nothing Then
                    AddHandler browser.DownloadProgressChanged, progressHandle
                End If

                strUrl = NetFile.MapGithubRawUrl(strUrl)

                Call browser.Headers.Add(UserAgent.UAheader, ua Or DefaultUA)
                Call $"{strUrl} --> {save}".__DEBUG_ECHO
                Call save.ParentPath.MakeDir
                Call browser.DownloadFile(strUrl, save)
            End Using

            Return True
        Catch ex As Exception
            Call App.LogException(New Exception(strUrl, ex), trace)
            Call ex.PrintException

            If retry > 0 Then
                retry -= 1
                GoTo RE0
            Else

            End If

            Return False
        Finally
            If save.FileExists Then
                Call $"[{FileIO.FileSystem.GetFileInfo(save).Length} Bytes]".__DEBUG_ECHO
            Else
                Call $"Download failure!".__DEBUG_ECHO
            End If
        End Try
    End Function

    ''' <summary>
    ''' Download file from http request and save to a specific location.
    ''' (使用GET方法下载文件)
    ''' </summary>
    ''' <param name="url"></param>
    ''' <param name="savePath"></param>
    ''' <returns></returns>
    '''
    <Extension>
    Public Function GetDownload(url As String, savePath As String) As Boolean
        Try
            Dim responseStream As Stream = GetRequestRaw(url)
            Dim localBuffer As Stream = responseStream.CopyStream
            Call $"[{localBuffer.Length} Bytes]".__DEBUG_ECHO
            Return localBuffer.FlushStream(savePath)
        Catch ex As Exception
            ex = New Exception(url, ex)
            Call ex.PrintException
            Call App.LogException(ex)
            Return False
        End Try
    End Function

    Const IPAddress As String = "http://ipaddress.com/"
    ''' <summary>
    ''' Microsoft DNS Server
    ''' </summary>
    Const MicrosoftDNS As String = "4.2.2.1"

    ''' <summary>
    ''' 获取我的公网IP地址，假若没有连接互联网的话则会返回局域网IP地址
    ''' </summary>
    ''' <returns></returns>
    Public Function GetMyIPAddress() As String
        Dim hasInternet As Boolean = False

#If NET_48 = 1 Or netcore5 = 1 Then

        Try
            hasInternet = Not PingUtility.Ping(System.Net.IPAddress.Parse(MicrosoftDNS)) > Integer.MaxValue
        Catch ex As Exception
            hasInternet = False
        End Try

#End If

        If hasInternet Then
            ' IPAddress on Internet
            Return getIPAddressInternal()
        Else
            ' IPAddress in LAN
            Return LocalIPAddress
        End If
    End Function

    ''' <summary>
    ''' Gets the IP address of this local machine.
    ''' (获取本机对象的IP地址，请注意这个属性获取得到的仅仅是本机在局域网内的ip地址，
    ''' 假若需要获取得到公网IP地址，还需要外部服务器的帮助才行)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>
    ''' becareful, this property may produce IPV6 address if avaiable
    ''' </remarks>
    Public ReadOnly Property LocalIPAddress As String
        Get
#Disable Warning
            Dim IP As System.Net.IPAddress = Dns.Resolve(Dns.GetHostName).AddressList(0)
            Dim IPAddr As String = IP.ToString
#Enable Warning
            Return IPAddr
        End Get
    End Property

    ''' <summary>
    ''' Request an external server and then returns the ip address from the server side.
    ''' </summary>
    ''' <returns></returns>
    Private Function getIPAddressInternal() As String
        Dim ipResult$ = IPAddress.GET

        ipResult = r.Match(ipResult, $"IP[:] {IPEndPoint.RegexIPAddress}<br>", RegexOptions.IgnoreCase).Value
        ipResult = r.Match(ipResult, IPEndPoint.RegexIPAddress).Value

        Return ipResult
    End Function
End Module

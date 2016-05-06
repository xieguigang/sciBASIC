Imports System.Text.RegularExpressions
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.IO
Imports System.Net
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Scripting
Imports System.Reflection
Imports System.Web
Imports Microsoft.VisualBasic.HtmlParser

''' <summary>
''' The extension module for web services works.
''' </summary>
'''
<PackageNamespace("Utils.WebServices",
                  Description:="The extension module for web services programming in your scripting.",
                  Category:=APICategories.UtilityTools,
                  Publisher:="<a href=""mailto://xie.guigang@gmail.com"">xie.guigang@gmail.com</a>")>
Public Module WebServiceUtils

    ''' <summary>
    ''' Web protocols enumeration
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Protocols As String() = {"http://", "https://", "ftp://", "sftp://"}

    ''' <summary>
    ''' Determine that is this uri string is a network location?
    ''' (判断这个uri字符串是否是一个网络位置)
    ''' </summary>
    ''' <param name="url"></param>
    ''' <returns></returns>
    <Extension> Public Function isURL(url As String) As Boolean
        Return url.InStrAny(Protocols) > -1
    End Function

    ''' <summary>
    ''' Build the request parameters for the HTTP POST
    ''' </summary>
    ''' <param name="dict"></param>
    ''' <returns></returns>
    <ExportAPI("Build.Reqparm",
               Info:="Build the request parameters for the HTTP POST")>
    <Extension> Public Function BuildReqparm(dict As Dictionary(Of String, String)) As Specialized.NameValueCollection
        Dim reqparm As New Specialized.NameValueCollection
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
    <ExportAPI("Build.Reqparm", Info:="Build the request parameters for the HTTP POST")>
    <Extension>
    Public Function BuildReqparm(data As IEnumerable(Of KeyValuePair(Of String, String))) As Specialized.NameValueCollection
        Dim reqparm As New Specialized.NameValueCollection
        For Each Value As KeyValuePair(Of String, String) In data
            Call reqparm.Add(Value.Key, Value.Value)
        Next
        Return reqparm
    End Function

    ''' <summary>
    ''' Gets the link text in the html fragement text.
    ''' </summary>
    ''' <param name="html">A string that contains the url string pattern like: href="url_text"</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI("Html.Href")>
    <Extension> Public Function Get_href(<Parameter("HTML",
                                                    "A string that contains the url string pattern like: href=""url_text""")>
                                         html As String) As String
        If String.IsNullOrEmpty(html) Then
            Return ""
        End If

        Dim url As String = Regex.Match(html, "href="".+?""", RegexOptions.IgnoreCase).Value

        If String.IsNullOrEmpty(url) Then
            Return ""
        Else
            url = Mid(url, 6)
            url = Mid(url, 2, Len(url) - 2)
            Return url
        End If
    End Function

    Public Const IMAGE_SOURCE As String = "<img.+?src=.+?>"

    ''' <summary>
    ''' Parsing image source url from the img html tag.
    ''' </summary>
    ''' <param name="str"></param>
    ''' <returns></returns>
    <Extension> Public Function ImageSource(str As String) As String
        str = Regex.Match(str, "src="".+?""", RegexOptions.IgnoreCase).Value
        str = Mid(str, 5)
        str = Mid(str, 2, Len(str) - 2)
        Return str
    End Function

    Const HTML_TAG As String = "</?.+?(\s+.+?="".+?"")*>"

    ''' <summary>
    ''' Removes the html tags from the text string.
    ''' </summary>
    ''' <param name="str"></param>
    ''' <returns></returns>
    <ExportAPI("Html.Tag.Trim")>
    <Extension> Public Function TrimHTMLTag(str As String) As String
        If String.IsNullOrEmpty(str) Then
            Return ""
        End If

        str = Regex.Replace(str, HTML_TAG, "")
        Return str
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
    ''' <returns></returns>
    <ExportAPI("CreateDirectory", Info:="Create a parameter dictionary from the request parameter tokens.")>
    <Extension>
    Public Function GenerateDictionary(tokens As String(), Optional lowercase As Boolean = True) As Dictionary(Of String, String)
        If tokens.IsNullOrEmpty Then
            Return New Dictionary(Of String, String)
        End If
        If tokens.Length = 1 Then  ' 只有url，没有附带的参数，则返回一个空的字典集合
            If InStr(tokens(Scan0), "=") = 0 Then
                Return New Dictionary(Of String, String)
            End If
        End If

        Dim LQuery = (From s As String In tokens
                      Let p As Integer = InStr(s, "="c)
                      Let Key As String = Mid(s, 1, p - 1)
                      Let Value = Mid(s, p + 1)
                      Select Key, Value).ToArray
        Return LQuery.ToDictionary(Function(obj) If(lowercase, obj.Key.ToLower, obj.Key), Function(obj) obj.Value)
    End Function

    ''' <summary>
    ''' 不像<see cref="postRequestParser(String, Boolean)"/>函数，这个函数不会替换掉转义字符，并且所有的Key都已经被默认转换为小写形式的了
    ''' </summary>
    ''' <param name="argvs"></param>
    ''' <returns></returns>
    '''
    <ExportAPI("Request.Parser")>
    <Extension> Public Function requestParser(argvs As String, Optional TransLower As Boolean = True) As Dictionary(Of String, String)
        Dim Tokens As String() = argvs.Split("&"c)
        Return GenerateDictionary(Tokens, TransLower)
    End Function

    ''' <summary>
    '''
    ''' </summary>
    ''' <param name="hash"></param>
    ''' <param name="escaping">是否进行对value部分的字符串数据进行转义</param>
    ''' <returns></returns>
    <Extension> Public Function BuildArgvs(hash As IEnumerable(Of KeyValuePair(Of String, String)),
                                           Optional escaping As Boolean = False) As String
        If escaping Then

        End If

        Dim str As String = String.Join("&", (From obj In hash.AsParallel Select $"{obj.Key}={obj.Value}").ToArray)
        Return str
    End Function

    <ExportAPI("Build.Args")>
    Public Function BuildArgs(ParamArray params As String()()) As String
        If params.IsNullOrEmpty Then
            Return ""
        Else
            Dim values = params.ToArray(Function(arg) $"{arg(Scan0)}={arg(1)}")
            Return String.Join("&", values)
        End If
    End Function

    <Extension> <ExportAPI("URL.Decode")>
    Public Function UrlDecode(s As String, Optional encoding As Encoding = Nothing) As String
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

    <ExportAPI("URL.Encode")>
    <Extension>
    Public Function UrlEncode(s As String, Optional encoding As Encoding = Nothing) As String
        If encoding IsNot Nothing Then
            Return HttpUtility.UrlEncode(s, encoding)
        Else
            Return HttpUtility.UrlEncode(s)
        End If
    End Function

    <ExportAPI("URL.Encode")>
    Public Sub UrlEncode(s As String, ByRef output As TextWriter)
        If s IsNot Nothing Then
            output.Write(UrlEncode(s))
        End If
    End Sub

    ''' <summary>
    ''' 编码整个URL
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns></returns>
    '''
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
    <Extension> Public Function postRequestParser(data As String, Optional TransLower As Boolean = True) As Dictionary(Of String, String)
        If String.IsNullOrEmpty(data) Then
            Return New Dictionary(Of String, String)
        End If

        Dim Tokens As String() = data.UrlDecode.Split("&"c)
        Dim hash = GenerateDictionary(Tokens, TransLower)
        Return hash
    End Function

    ''' <summary>
    ''' Download stream data from the http response.
    ''' </summary>
    ''' <param name="stream">Create from <see cref="WebServiceUtils.GetRequestRaw(String)"/></param>
    ''' <returns></returns>
    <ExportAPI("Stream.Copy", Info:="Download stream data from the http response.")>
    <Extension> Public Function CopyStream(stream As Stream) As Byte()
        If stream Is Nothing Then
            Return New Byte() {}
        End If

        Dim stmMemory As MemoryStream = New MemoryStream()
        Dim buffer As Byte() = New Byte(64 * 1024) {}
        Dim i As Integer
        Do While stream.Read(buffer, 0, buffer.Length).ShadowCopy(i) > 0
            Call stmMemory.Write(buffer, 0, i)
        Loop
        buffer = stmMemory.ToArray()
        Call stmMemory.Close()
        Return buffer
    End Function

    <ExportAPI("GET", Info:="GET http request")>
    <Extension> Public Function GetRequest(strUrl As String, ParamArray args As String()()) As String
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

    <ExportAPI("GET", Info:="GET http request")>
    <Extension> Public Function GetRequest(strUrl As String) As String
        Dim strDate As String = ""
        Dim strValue As String = ""
        Dim Reader As StreamReader = New StreamReader(GetRequestRaw(strUrl), Encoding.UTF8)

        Do While Not Reader.ReadLine().ShadowCopy(strDate) Is Nothing
            strValue &= (strDate & vbCrLf)
        Loop

        Return Mid(strValue, 1, Len(strValue) - vbCrLfLen)
    End Function

    <ExportAPI("GET.Raw", Info:="GET http request")>
    <Extension> Public Function GetRequestRaw(url As String) As Stream
        Dim request As HttpWebRequest
        request = WebRequest.Create(url).As(Of HttpWebRequest)
        request.Method = "GET"
        Dim response = request.GetResponse.As(Of HttpWebResponse)
        Dim s As Stream = response.GetResponseStream()
        Return s
    End Function

    <ExportAPI("POST", Info:="POST http request")>
    Public Function PostRequest(url As String, Optional params As IEnumerable(Of KeyValuePair(Of String, String)) = Nothing) As String
        Return url.PostRequest(params.BuildReqparm)
    End Function

    <ExportAPI("POST", Info:="POST http request")>
    Public Function PostRequest(url As String, ParamArray params As String()()) As String
        Dim post As KeyValuePair(Of String, String)()
        If params Is Nothing Then
            post = Nothing
        Else
            post = params.ToArray(Function(value) New KeyValuePair(Of String, String)(value(0), value(1)))
        End If
        Return PostRequest(url, post)
    End Function

    <ExportAPI("POST", Info:="POST http request")>
    <Extension> Public Function PostRequest(url As String, params As Specialized.NameValueCollection) As String
        Using request As New WebClient
            Call $"[POST] {url}....".__DEBUG_ECHO
            Dim response As Byte() = request.UploadValues(url, "POST", params)
            Dim strData As String = System.Text.Encoding.UTF8.GetString(response)
            Call $"[GET] {response.Length} bytes...".__DEBUG_ECHO

            Return strData
        End Using
    End Function

    ''' <summary>
    ''' 有些时候后面可能会存在多余的vbCrLf，则使用这个函数去除
    ''' </summary>
    ''' <param name="value"></param>
    ''' <returns></returns>
    <Extension> Public Function TrimResponseTail(value As String) As String
        If String.IsNullOrEmpty(value) Then
            Return ""
        End If

        Dim l As Integer = Len(value)
        Dim i As Integer = value.LastIndexOf(vbCrLf)
        If i = l - 2 Then
            Return Mid(value, 1, l - 2)
        Else
            Return value
        End If
    End Function

    Private ReadOnly vbCrLfLen As Integer = Len(vbCrLf)

    ''' <summary>
    ''' 获取两个尖括号之间的内容
    ''' </summary>
    ''' <param name="html"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI("Html.GetValue", Info:="Gets the string value between two wrapper character.")>
    <Extension> Public Function GetValue(html As String) As String
        Return html.GetStackValue(">", "<")
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' Get the html page content from a website request or a html file on the local filesystem.(同时支持http位置或者本地文件)
    ''' </summary>
    ''' <param name="url">web http request url or a file path handle</param>
    ''' <param name="RequestTimeOut">发生错误的时候的重试的次数</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI("Webpage.Request", Info:="Get the html page content from a website request or a html file on the local filesystem.")>
    <Extension> Public Function [GET](url As String,
                                      <Parameter("Request.TimeOut")>
                                      Optional RequestTimeOut As UInteger = 20,
                                      <Parameter("FileSystem.Works?", "Is this a local html document on your filesystem?")>
                                      Optional FileSystemUrl As Boolean = False) As String
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
        Call $"Request data from: {If(FileSystemUrl, url.ToFileURL, url)}".__DEBUG_ECHO

        If FileIO.FileSystem.FileExists(url) Then
            Call "[Job DONE!]".__DEBUG_ECHO
            Return FileIO.FileSystem.ReadAllText(url)
        Else
            If FileSystemUrl Then
                Call $"url {url.ToFileURL} can not be solved on your filesystem!".__DEBUG_ECHO
                Return ""
            End If
        End If

#If FRAMEWORD_CORE Then
        Using Process As ConsoleDevice.Utility.CBusyIndicator =
            New ConsoleDevice.Utility.CBusyIndicator(_start:=True)
#End If
            Return __downloadWebpage(url, RequestTimeOut)
#If FRAMEWORD_CORE Then
        End Using
#End If
        Return ""
    End Function

    Private Function __downloadWebpage(url As String, RequestTimeOut As UInteger) As String
        Dim RequestTime As Integer = 0
        Try
RETRY:      Return __downloadWebpage(url)
        Catch ex As Exception

            Call Console.WriteLine(ex.ToString)

            If RequestTime < RequestTimeOut Then
                RequestTime += 1
                Call "Data downloading error, retry connect to the server!".__DEBUG_ECHO
                GoTo RETRY
            Else
                Return LogException(url, ex)
            End If
        End Try
    End Function

    Private Function LogException(url As String, ex As Exception) As String
        Dim exMessage As String = String.Format("Unable to get the http request!" & vbCrLf &
                                                "  Url:=[{0}]" & vbCrLf &
                                                "  EXCEPTION ===>" & vbCrLf & ex.ToString, url)
        Call App.LogException(exMessage, NameOf([GET]) & "::HTTP_REQUEST_EXCEPTION")
        Return ""
    End Function

    Private Function __downloadWebpage(url As String) As String
        Call "Waiting for the server reply..".__DEBUG_ECHO

        Dim Timer As Stopwatch = Stopwatch.StartNew
        Dim WebRequest As System.Net.HttpWebRequest = System.Net.HttpWebRequest.Create(url)

        WebRequest.Headers.Add("Accept-Language", "en-US,en;q=0.8,zh-Hans-CN;q=0.5,zh-Hans;q=0.3")
        WebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.10240"

        Dim WebResponse As WebResponse = WebRequest.GetResponse

        Using respStream As Stream = WebResponse.GetResponseStream, ioStream As StreamReader = New StreamReader(respStream)
            Dim html As String = ioStream.ReadToEnd
            Dim title As String = html.HTMLtitle

            If InStr(html, "http://www.doctorcom.com") Then
                Return ""
            End If

            Call $"[{title}  {url}] -->  Package Size:= {Len(html)}bytes; Response time:= {Timer.ElapsedMilliseconds}ms".__DEBUG_ECHO
            Call html.SaveTo($"{App.AppSystemTemp}/{url.NormalizePathString}.tmp")

            Return html
        End Using
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' download the file from <paramref name="strUrl"></paramref> to <paramref name="save">local file</paramref>.
    ''' </summary>
    ''' <param name="strUrl"></param>
    ''' <param name="save">The file path of the file saved</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <ExportAPI("wget", Info:="Download data from the specific URL location.")>
    <Extension> Public Function DownloadFile(<Parameter("url")> strUrl As String,
                                             <Parameter("Path.Save", "The saved location of the downloaded file data.")>
                                             save As String) As Boolean
#Else
    ''' <summary>
    ''' download the file from <paramref name="strUrl"></paramref> to <paramref name="SavedPath">local file</paramref>.
    ''' </summary>
    ''' <param name="strUrl"></param>
    ''' <param name="SavedPath"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function DownloadFile(strUrl As String, SavedPath As String) As Boolean
#End If
        Try
            Using dwl As New System.Net.WebClient()
                Call dwl.DownloadFile(strUrl, save)
            End Using
            Return True
        Catch ex As Exception
            Dim trace As String = MethodBase.GetCurrentMethod.GetFullName & ":: " & strUrl
            Call App.LogException(ex, trace)
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
    ''' 使用GET方法下载文件
    ''' </summary>
    ''' <param name="url"></param>
    ''' <param name="savePath"></param>
    ''' <returns></returns>
    '''
    <ExportAPI("GET.Download", Info:="Download file from http request and save to a specific location.")>
    <Extension> Public Function GetDownload(url As String, savePath As String) As Boolean
        Try
            Dim responseStream As Stream = GetRequestRaw(url)
            Dim buffer As Byte() = responseStream.CopyStream
            Call $"[{buffer.Length} Bytes]".__DEBUG_ECHO
            Return buffer.FlushStream(savePath)
        Catch ex As Exception
            ex = New Exception(url, ex)
            Call ex.PrintException
            Call App.LogException(ex)
            Return False
        End Try
    End Function

    Public Const IPAddress As String = "http://ipaddress.com/"
    ''' <summary>
    ''' Microsoft DNS Server
    ''' </summary>
    Public Const MicrosoftDNS As String = "4.2.2.1"

    ''' <summary>
    ''' 获取我的公网IP地址，假若没有连接互联网的话则会返回局域网IP地址
    ''' </summary>
    ''' <returns></returns>
    Public Function GetMyIPAddress() As String
        Dim hasInternet As Boolean

        Try
            hasInternet = Not Net.PingUtility.Ping(System.Net.IPAddress.Parse(MicrosoftDNS)) > Integer.MaxValue
        Catch ex As Exception
            hasInternet = False
        End Try

        If hasInternet Then
            Return __getMyIPAddress()   'IPAddress on Internet
        Else
            Return Net.AsynInvoke.LocalIPAddress  'IPAddress in LAN
        End If
    End Function

    Public Const RegexIPAddress As String = "\d{1,3}(\.\d{1,3}){3}"

    Private Function __getMyIPAddress() As String
        Dim page As String = IPAddress.GET
        Dim ipResult As String = Regex.Match(page, $"IP[:] {RegexIPAddress}<br><img", RegexOptions.IgnoreCase).Value
        ipResult = Regex.Match(ipResult, RegexIPAddress).Value
        Return ipResult
    End Function
End Module

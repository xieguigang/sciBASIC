Imports System.Text.RegularExpressions
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.IO
Imports System.Net
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports System.Reflection

''' <summary>
''' The extension module for web services works.
''' </summary>
''' 
<PackageNamespace("WebServices",
                  Description:="The extension module for web services programming in your scripting.",
                  Category:=APICategories.UtilityTools,
                  Publisher:="<a href=""mailto://xie.guigang@gmail.com"">xie.guigang@gmail.com</a>")>
Public Module WebServices

    ''' <summary>
    ''' Build the request parameters for the HTTP POST
    ''' </summary>
    ''' <param name="dict"></param>
    ''' <returns></returns>
    <ExportAPI("Build.Reqparm", Info:="Build the request parameters for the HTTP POST")>
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
    Public Function BuildReqparm(data As Generic.IEnumerable(Of KeyValuePair(Of String, String))) As Specialized.NameValueCollection
        Dim reqparm As New Specialized.NameValueCollection
        For Each Value As KeyValuePair(Of String, String) In data
            Call reqparm.Add(Value.Key, Value.Value)
        Next
        Return reqparm
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="s_Data">A string that contains the url string pattern like: href="url_text"</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("Html.Href")>
    <Extension> Public Function Get_href(<Parameter("A string that contains the url string pattern like: href=""url_text""")> s_Data As String) As String
        Dim url As String = Regex.Match(s_Data, "href="".+?""", RegexOptions.IgnoreCase).Value
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
    ''' 
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

    <ExportAPI("Html.Tag.Trim")>
    <Extension> Public Function TrimHTMLTag(str As String) As String
        If String.IsNullOrEmpty(str) Then
            Return ""
        End If

        str = Regex.Replace(str, HTML_TAG, "")
        Return str
    End Function

    Const PortOccupied As String = "Only one usage of each socket address (protocol/network address/port) Is normally permitted"

    <Extension> Public Function IsSocketPortOccupied(ex As Exception) As Boolean
        If TypeOf ex Is System.Net.Sockets.SocketException AndAlso
            InStr(ex.ToString, PortOccupied, CompareMethod.Text) Then
            Return True
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' Create a parameter dictionary from the request parameter tokens.(请注意，字典的key默认为转换为小写的形式)
    ''' </summary>
    ''' <param name="Tokens"></param>
    ''' <returns></returns>
    <ExportAPI("CreateDirectory", Info:="Create a parameter dictionary from the request parameter tokens.")>
    <Extension> Public Function GenerateDictionary(Tokens As String(),
                                                   Optional TransLower As Boolean = True) As Dictionary(Of String, String)
        Dim LQuery = (From s As String In Tokens
                      Let p As Integer = InStr(s, "="c)
                      Let Key As String = Mid(s, 1, p - 1)
                      Let Value = Mid(s, p + 1)
                      Select Key, Value).ToArray
        Return LQuery.ToDictionary(Function(obj) If(TransLower, obj.Key.ToLower, obj.Key), elementSelector:=Function(obj) obj.Value)
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
    ''' <param name="Escaping">是否进行对value部分的字符串数据进行转义</param>
    ''' <returns></returns>
    <Extension> Public Function BuildArgvs(hash As Generic.IEnumerable(Of KeyValuePair(Of String, String)),
                                           Optional Escaping As Boolean = False) As String
        If Escaping Then

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

    'email=xie.guigang%40live.com&AppName=mipaimai+webapi--&Url=mipaimai.com%2F%2B%2B&Description=a+b+c+d+e+f+g+h%25%26&Publisher=siyu.com&submit=submit

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

        Dim sbr As New StringBuilder(data)

        Call sbr.Replace("+", " ")
        Call sbr.Replace("%7E", "~")
        Call sbr.Replace("%21", "!")
        Call sbr.Replace("%40", "@")
        Call sbr.Replace("%23", "#")
        Call sbr.Replace("%24", "$")
        Call sbr.Replace("%5E", "^")
        Call sbr.Replace("%28", "(")
        Call sbr.Replace("%29", ")")
        Call sbr.Replace("%2B", "+")
        Call sbr.Replace("%60", "`")
        Call sbr.Replace("%3D", "=")
        Call sbr.Replace("%5B", "[")
        Call sbr.Replace("%5D", "]")
        Call sbr.Replace("%5C", "\")
        Call sbr.Replace("%3B", ";")
        Call sbr.Replace("%27", "'")
        Call sbr.Replace("%2C", ",")
        Call sbr.Replace("%2F", "/")
        Call sbr.Replace("%7B", "{")
        Call sbr.Replace("%7D", "}")
        Call sbr.Replace("%7C", "|")
        Call sbr.Replace("%3A", ":")
        Call sbr.Replace("%22", """")
        Call sbr.Replace("%3C", "<")
        Call sbr.Replace("%3E", ">")
        Call sbr.Replace("%3F", "?")
        Call sbr.Replace("%25", "%")

        ' 小写

        Call sbr.Replace("%7e", "~")
        Call sbr.Replace("%5e", "^")
        Call sbr.Replace("%2b", "+")
        Call sbr.Replace("%3d", "=")
        Call sbr.Replace("%5b", "[")
        Call sbr.Replace("%5d", "]")
        Call sbr.Replace("%5c", "\")
        Call sbr.Replace("%3b", ";")
        Call sbr.Replace("%2c", ",")
        Call sbr.Replace("%2f", "/")
        Call sbr.Replace("%7b", "{")
        Call sbr.Replace("%7d", "}")
        Call sbr.Replace("%7c", "|")
        Call sbr.Replace("%3a", ":")
        Call sbr.Replace("%3c", "<")
        Call sbr.Replace("%3e", ">")
        Call sbr.Replace("%3f", "?")

        Dim Tokens As String() = sbr.ToString.Split("&"c)
        Tokens = (From s As String In Tokens Select s.Replace("%26", "&")).ToArray

        Dim hash = GenerateDictionary(Tokens, TransLower)
        Return hash
    End Function

    ''' <summary>
    ''' Download stream data from the http response.
    ''' </summary>
    ''' <param name="stream">Create from <see cref="WebServices.GetRequestRaw(String)"/></param>
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
        Dim request As System.Net.HttpWebRequest
        request = WebRequest.Create(url).As(Of System.Net.HttpWebRequest)
        request.Method = "GET"
        Dim response = request.GetResponse.As(Of System.Net.HttpWebResponse)
        Dim s As Stream = response.GetResponseStream()
        Return s
    End Function

    <ExportAPI("POST", Info:="POST http request")>
    Public Function PostRequest(url As String, Optional params As Generic.IEnumerable(Of KeyValuePair(Of String, String)) = Nothing) As String
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
        Using request As New System.Net.WebClient
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

    Public Const PAGE_CONTENT_TITLE As String = "<title>.+</title>"

    ''' <summary>
    ''' 获取两个尖括号之间的内容
    ''' </summary>
    ''' <param name="s_Data"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("Html.GetValue", Info:="Gets the string value between two wrapper character.")>
    <Extension> Public Function GetValue(s_Data As String) As String
        If Len(s_Data) < 2 Then
            Return ""
        End If

        Dim p = InStr(s_Data, ">") + 1
        Dim q = InStrRev(s_Data, "<")

        If p = 0 Or q = 0 Then
            Return s_Data
        End If

        s_Data = Mid(s_Data, p, q - p)
        Return s_Data
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
    <Extension> Public Function Get_PageContent(url As String,
                                               <Parameter("Request.TimeOut")> Optional RequestTimeOut As UInteger = 20,
                                               <Parameter("FileSystem.Works?", "Is this a local html document on your filesystem?")> Optional FileSystemUrl As Boolean = False) As String
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
        Using Process As ConsoleDevice.Utility.ConsoleBusyIndicator =
            New ConsoleDevice.Utility.ConsoleBusyIndicator(_start:=True)
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
        Call App.LogException(exMessage, NameOf(Get_PageContent) & "::HTTP_REQUEST_EXCEPTION")
        Return ""
    End Function

    Private Function __downloadWebpage(url As String) As String
        Call "Waiting for the server reply..".__DEBUG_ECHO

        Dim Timer As Stopwatch = Stopwatch.StartNew
        Dim WebRequest As System.Net.HttpWebRequest = System.Net.HttpWebRequest.Create(url)

        WebRequest.Headers.Add("Accept-Language", "en-US,en;q=0.8,zh-Hans-CN;q=0.5,zh-Hans;q=0.3")
        WebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.10240"
        ' WebRequest.Headers.Add("Accept-Encoding", "gzip, deflate")

        Dim WebResponse As WebResponse = WebRequest.GetResponse

        Using respStream As Stream = WebResponse.GetResponseStream, ioStream As StreamReader = New StreamReader(respStream)
            Dim pageContent As String = ioStream.ReadToEnd

            If InStr(pageContent, "http://www.doctorcom.com") Then
                Return ""
            End If

            Dim Title = Regex.Match(pageContent, PAGE_CONTENT_TITLE, RegexOptions.IgnoreCase).Value
            If String.IsNullOrEmpty(Title) Then
                Title = "NULL_TITLE"
            Else
                Title = Mid(Title, 8, Len(Title) - 15)
            End If

            Call $"[{Title}  {url}] -->  Package Size:= {Len(pageContent)}bytes; Response time:= {Timer.ElapsedMilliseconds}ms".__DEBUG_ECHO
            Call pageContent.SaveTo($"{App.LocalDataTemp}/{url.NormalizePathString}.tmp")

            Return pageContent
        End Using
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' download the file from <paramref name="strUrl"></paramref> to <paramref name="SavedPath">local file</paramref>.
    ''' </summary>
    ''' <param name="strUrl"></param>
    ''' <param name="SavedPath"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <ExportAPI("wget", Info:="Download data from the specific URL location.")>
    <Extension> Public Function DownloadFile(<Parameter("url")> strUrl As String,
                                             <Parameter("Path.Save", "The saved location of the downloaded file data.")> SavedPath As String) As Boolean
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
                Call dwl.DownloadFile(strUrl, SavedPath)
            End Using
            Return True
        Catch ex As Exception
            Dim trace As String = MethodBase.GetCurrentMethod.GetFullName & ":: " & strUrl
            Call App.LogException(ex, trace)
            Return False
        Finally
            If SavedPath.FileExists Then
                Call $"[{FileIO.FileSystem.GetFileInfo(SavedPath).Length} Bytes]".__DEBUG_ECHO
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
        Dim page As String = IPAddress.Get_PageContent
        Dim ipResult As String = Regex.Match(page, $"IP[:] {RegexIPAddress}<br><img", RegexOptions.IgnoreCase).Value
        ipResult = Regex.Match(ipResult, RegexIPAddress).Value
        Return ipResult
    End Function

End Module

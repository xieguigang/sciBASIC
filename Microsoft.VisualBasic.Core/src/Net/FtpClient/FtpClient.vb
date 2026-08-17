' ============================================================================
' FtpClient.vb - 基于 BCL TcpClient 从头实现的 FTP 客户端
'
' 模仿 HttpClient 设计模式，支持:
'   - 被动模式 PASV/EPSV (兼容 IPv4/IPv6)
'   - FTP over TLS (FTPS, 显式/隐式)
'   - 异步文件下载与进度报告
'   - 文件信息查询 (SIZE / MDTM)
'   - 工作目录操作 (CWD / PWD)
'
' 替代 .NET 中已移除的 FtpWebRequest。
'
' 用法:
'   Using client As New FtpClient("ftp.example.com", 21,
'                              New FtpClientOptions With {.EnableSsl = True},
'                              New FtpCredentials("user", "pass"))
'       Await client.DownloadFileAsync("/remote/file.zip", "C:\local\file.zip",
'           progress:=New Progress(Of FtpDownloadProgress)(Sub(p)
'               Console.WriteLine($"{p.BytesTransferred}/{p.TotalBytes}")
'           End Sub))
'   End Using
' ============================================================================

Imports System.IO
Imports System.Net.Security
Imports System.Net.Sockets
Imports System.Security.Cryptography.X509Certificates
Imports System.Threading

Namespace Net.FTP

    ''' <summary>
    ''' FTP 客户端，基于 BCL TcpClient + SslStream 从头实现。
    ''' 模仿 HttpClient 异步设计模式。
    ''' 注意: 此类非线程安全，请勿从多线程同时操作同一实例。
    ''' </summary>
    Public Class FtpClient
        Implements IDisposable

        ' --- 常量 ---
        Private Const DefaultPort As Integer = 21
        Private Const ImplicitSslPort As Integer = 990
        Private Const BufferSize As Integer = 81920  ' 80 KB
        Private Const CrLf As String = vbCrLf

        ' --- 控制连接 ---
        Private _tcpClient As TcpClient
        Private _controlStream As Stream
        Private _reader As StreamReader

        ' --- 数据连接 (每次操作创建/销毁) ---
        Private _dataTcpClient As TcpClient

        ' --- 状态 ---
        Private _isConnected As Boolean
        Private _isAuthenticated As Boolean
        Private _isDisposed As Boolean
        Private _isTlsActive As Boolean

        ' --- 配置 ---
        Private ReadOnly _host As String
        Private ReadOnly _port As Integer
        Private _credentials As FtpCredentials
        Private ReadOnly _options As FtpClientOptions

        ' ========================================================================
        '  构造函数
        ' ========================================================================

        ''' <summary>
        ''' 创建 FTP 客户端实例。
        ''' </summary>
        ''' <param name="host">FTP 服务器主机名或 IP</param>
        ''' <param name="port">端口，默认 21 (隐式 SSL 时默认 990)</param>
        ''' <param name="options">配置选项，为 Nothing 时使用默认值</param>
        ''' <param name="credentials">登录凭据，为 Nothing 时使用匿名登录</param>
        Public Sub New(host As String,
                   Optional port As Integer = DefaultPort,
                   Optional options As FtpClientOptions = Nothing,
                   Optional credentials As FtpCredentials = Nothing)

            If String.IsNullOrWhiteSpace(host) Then
                Throw New ArgumentException("主机地址不能为空。", NameOf(host))
            End If

            ' 隐式 SSL 但未指定端口时自动用 990
            If options IsNot Nothing AndAlso options.EnableSsl AndAlso
           options.ImplicitSsl AndAlso port = DefaultPort Then
                port = ImplicitSslPort
            End If

            _host = host
            _port = port
            _options = If(options, New FtpClientOptions())
            _credentials = If(credentials, FtpCredentials.Anonymous)
        End Sub

        ' ========================================================================
        '  公开属性
        ' ========================================================================

        Public ReadOnly Property Host As String
            Get
                Return _host
            End Get
        End Property

        Public ReadOnly Property Port As Integer
            Get
                Return _port
            End Get
        End Property

        Public ReadOnly Property IsConnected As Boolean
            Get
                Return _isConnected AndAlso _tcpClient IsNot Nothing AndAlso _tcpClient.Connected
            End Get
        End Property

        Public ReadOnly Property IsAuthenticated As Boolean
            Get
                Return _isAuthenticated
            End Get
        End Property

        Public ReadOnly Property IsSecureConnection As Boolean
            Get
                Return _isTlsActive
            End Get
        End Property

        ''' <summary>
        ''' 登录凭据。必须在连接前设置。
        ''' </summary>
        Public Property Credentials As FtpCredentials
            Get
                Return _credentials
            End Get
            Set(value As FtpCredentials)
                If _isAuthenticated Then
                    Throw New InvalidOperationException("认证后不能修改凭据。")
                End If
                _credentials = value
            End Set
        End Property

        ' ========================================================================
        '  连接 / 断开
        ' ========================================================================

        ''' <summary>
        ''' 显式连接到 FTP 服务器并完成认证。
        ''' 通常无需调用——首次操作时会自动连接。
        ''' </summary>
        Public Async Function ConnectAsync(Optional ct As CancellationToken = Nothing) As Task
            ThrowIfDisposed()
            If IsConnected Then Return
            Await ConnectInternalAsync(ct)
        End Function

        ''' <summary>
        ''' 断开连接。可再次调用 ConnectAsync 重连。
        ''' </summary>
        Public Sub Disconnect()
            CloseDataConnection()
            CloseControlConnection()
            _isConnected = False
            _isAuthenticated = False
            _isTlsActive = False
        End Sub

        ' ========================================================================
        '  下载
        ' ========================================================================

        ''' <summary>
        ''' 下载远程文件到指定流。
        ''' </summary>
        ''' <param name="remotePath">远程文件路径</param>
        ''' <param name="destination">目标流</param>
        ''' <param name="ct">取消令牌</param>
        ''' <param name="progress">进度回调 (可为 Nothing)</param>
        Public Async Function DownloadAsync(
        remotePath As String,
        destination As Stream,
        Optional ct As CancellationToken = Nothing,
        Optional progress As IProgress(Of FtpDownloadProgress) = Nothing) As Task

            If String.IsNullOrWhiteSpace(remotePath) Then
                Throw New ArgumentException("远程文件路径不能为空。", NameOf(remotePath))
            End If
            If destination Is Nothing Then
                Throw New ArgumentNullException(NameOf(destination))
            End If

            ThrowIfDisposed()
            Await EnsureConnectedAsync(ct)
            Await DownloadInternalAsync(remotePath, destination, ct, progress)
        End Function

        ''' <summary>
        ''' 下载远程文件到本地路径。
        ''' </summary>
        ''' <param name="remotePath">远程文件路径</param>
        ''' <param name="localPath">本地保存路径</param>
        ''' <param name="overwrite">是否覆盖已有文件，默认 True</param>
        ''' <param name="ct">取消令牌</param>
        ''' <param name="progress">进度回调 (可为 Nothing)</param>
        Public Async Function DownloadFileAsync(
        remotePath As String,
        localPath As String,
        Optional overwrite As Boolean = True,
        Optional ct As CancellationToken = Nothing,
        Optional progress As IProgress(Of FtpDownloadProgress) = Nothing) As Task

            If String.IsNullOrWhiteSpace(remotePath) Then
                Throw New ArgumentException("远程文件路径不能为空。", NameOf(remotePath))
            End If
            If String.IsNullOrWhiteSpace(localPath) Then
                Throw New ArgumentException("本地文件路径不能为空。", NameOf(localPath))
            End If

            ThrowIfDisposed()

            Dim fileMode As FileMode = If(overwrite, FileMode.Create, FileMode.CreateNew)
            Using fs As New FileStream(localPath, fileMode, FileAccess.Write, FileShare.None, BufferSize, useAsync:=True)
                Await DownloadAsync(remotePath, fs, ct, progress)
            End Using
        End Function

        ' ========================================================================
        '  文件信息
        ' ========================================================================

        ''' <summary>
        ''' 获取远程文件大小 (字节)。服务器不支持 SIZE 时抛异常。
        ''' </summary>
        Public Async Function GetFileSizeAsync(
        remotePath As String, Optional ct As CancellationToken = Nothing) As Task(Of Long)

            If String.IsNullOrWhiteSpace(remotePath) Then
                Throw New ArgumentException("远程文件路径不能为空。", NameOf(remotePath))
            End If

            ThrowIfDisposed()
            Await EnsureConnectedAsync(ct)

            Await SendCommandAsync("TYPE I", ct)
            Dim resp = Await SendCommandAsync("SIZE " & remotePath, ct)
            If resp.StatusCode <> 213 Then
                Throw New FtpException("SIZE 命令失败: " & resp.ToString(), resp.StatusCode)
            End If

            Dim parts = resp.StatusText.Split({" "c}, StringSplitOptions.RemoveEmptyEntries)
            If parts.Length >= 1 Then
                Dim size As Long
                If Long.TryParse(parts(0), size) Then Return size
            End If
            Throw New FtpProtocolException("无法解析 SIZE 响应: " & resp.ToString())
        End Function

        ''' <summary>
        ''' 检查远程文件是否存在。
        ''' </summary>
        Public Async Function FileExistsAsync(
        remotePath As String, Optional ct As CancellationToken = Nothing) As Task(Of Boolean)

            If String.IsNullOrWhiteSpace(remotePath) Then
                Throw New ArgumentException("远程文件路径不能为空。", NameOf(remotePath))
            End If

            ThrowIfDisposed()
            Await EnsureConnectedAsync(ct)

            ' 尝试 SIZE 命令
            Await SendCommandAsync("TYPE I", ct)
            Dim resp = Await SendCommandAsync("SIZE " & remotePath, ct)
            If resp.StatusCode = 213 Then Return True
            If resp.StatusCode = 550 Then
                ' 550 可能是文件不存在，也可能是不支持 SIZE
                ' 再试 MDTM
                resp = Await SendCommandAsync("MDTM " & remotePath, ct)
                If resp.StatusCode = 213 Then Return True
                Return False
            End If
            ' 其他状态码 (如 500/502) 说明服务器不支持 SIZE，保守返回 True
            Return True
        End Function

        ''' <summary>
        ''' 获取远程文件信息 (大小 + 修改时间)。
        ''' </summary>
        Public Async Function GetFileInfoAsync(
        remotePath As String, Optional ct As CancellationToken = Nothing) As Task(Of FtpFileInfo)

            If String.IsNullOrWhiteSpace(remotePath) Then
                Throw New ArgumentException("远程文件路径不能为空。", NameOf(remotePath))
            End If

            ThrowIfDisposed()
            Await EnsureConnectedAsync(ct)

            Dim info As New FtpFileInfo With {.Path = remotePath}

            ' 获取大小
            Try
                info.Size = Await GetFileSizeAsync(remotePath, ct)
            Catch
                info.Size = -1
            End Try

            ' 获取修改时间
            Try
                Dim resp = Await SendCommandAsync("MDTM " & remotePath, ct)
                If resp.StatusCode = 213 Then
                    info.LastModified = ParseMdtmDate(resp.StatusText.Trim())
                End If
            Catch
                ' MDTM 不支持时忽略
            End Try

            Return info
        End Function

        ' ========================================================================
        '  目录操作
        ' ========================================================================

        ''' <summary>
        ''' 更改远程工作目录 (CWD)。
        ''' </summary>
        Public Async Function SetWorkingDirectoryAsync(
        path As String, Optional ct As CancellationToken = Nothing) As Task

            If String.IsNullOrWhiteSpace(path) Then
                Throw New ArgumentException("路径不能为空。", NameOf(path))
            End If

            ThrowIfDisposed()
            Await EnsureConnectedAsync(ct)

            Dim resp = Await SendCommandAsync("CWD " & path, ct)
            If resp.StatusCode <> 250 Then
                Throw New FtpException("CWD 失败: " & resp.ToString(), resp.StatusCode)
            End If
        End Function

        ''' <summary>
        ''' 获取当前工作目录 (PWD)。
        ''' </summary>
        Public Async Function PrintWorkingDirectoryAsync(
        Optional ct As CancellationToken = Nothing) As Task(Of String)

            ThrowIfDisposed()
            Await EnsureConnectedAsync(ct)

            Dim resp = Await SendCommandAsync("PWD", ct)
            If resp.StatusCode <> 257 Then
                Throw New FtpException("PWD 失败: " & resp.ToString(), resp.StatusCode)
            End If

            ' 解析: 257 "/path" is current directory
            Dim text = resp.StatusText
            Dim start = text.IndexOf(""""c)
            Dim [end] = text.IndexOf(""""c, start + 1)
            If start >= 0 AndAlso [end] > start Then
                Return text.Substring(start + 1, [end] - start - 1)
            End If
            Return text
        End Function

        ' ========================================================================
        '  内部实现: 连接
        ' ========================================================================

        Private Async Function EnsureConnectedAsync(ct As CancellationToken) As Task
            ThrowIfDisposed()
            If IsConnected Then Return
            Await ConnectInternalAsync(ct)
        End Function

        Private Async Function ConnectInternalAsync(ct As CancellationToken) As Task
            _tcpClient = New TcpClient()

            ' --- TCP 连接 (带超时) ---
            Dim connectCts = CancellationTokenSource.CreateLinkedTokenSource(ct)
            connectCts.CancelAfter(_options.ConnectTimeout)
            Try
                Await _tcpClient.ConnectAsync(_host, _port).WaitAsync(connectCts.Token)
            Catch ex As OperationCanceledException When (Not ct.IsCancellationRequested)
                SafeCloseTcp(_tcpClient)
                _tcpClient = Nothing
                Throw New FtpConnectionException(
                $"连接 {_host}:{_port} 超时 (超过 {_options.ConnectTimeout.TotalSeconds:F0} 秒)")
            Catch ex As SocketException
                SafeCloseTcp(_tcpClient)
                _tcpClient = Nothing
                Throw New FtpConnectionException(
                $"连接 {_host}:{_port} 失败: {ex.Message}", ex)
            Finally
                connectCts.Dispose()
            End Try

            _controlStream = _tcpClient.GetStream()

            ' --- 隐式 SSL: 立即升级 ---
            If _options.EnableSsl AndAlso _options.ImplicitSsl Then
                Await UpgradeToSslAsync(ct)
                _isTlsActive = True
            End If

            _reader = New StreamReader(_controlStream, _options.Encoding, False, 1024, leaveOpen:=True)

            ' --- 读取欢迎信息 (220) ---
            Dim welcome = Await ReadResponseAsync(ct)
            If Not welcome.IsSuccess Then
                Throw New FtpConnectionException("服务器拒绝连接: " & welcome.ToString())
            End If

            ' --- 显式 SSL (AUTH TLS) ---
            If _options.EnableSsl AndAlso Not _options.ImplicitSsl Then
                Dim authResp = Await SendCommandAsync("AUTH TLS", ct)
                If authResp.StatusCode = 234 Then
                    Await UpgradeToSslAsync(ct)
                    _isTlsActive = True
                ElseIf authResp.StatusCode = 500 OrElse authResp.StatusCode = 502 Then
                    ' 服务器不支持 TLS，继续明文
                Else
                    Throw New FtpConnectionException("AUTH TLS 失败: " & authResp.ToString())
                End If
            End If

            _isConnected = True

            ' --- 认证 ---
            Await AuthenticateAsync(ct)
        End Function

        Private Async Function UpgradeToSslAsync(ct As CancellationToken) As Task
            Dim sslStream As New SslStream(_controlStream, leaveInnerStreamOpen:=False,
            AddressOf ValidateServerCertificate)

            Dim authOptions As New SslClientAuthenticationOptions With {
            .TargetHost = _host,
            .RemoteCertificateValidationCallback = AddressOf ValidateServerCertificate
        }
            Await sslStream.AuthenticateAsClientAsync(authOptions, ct)

            _controlStream = sslStream
            _reader = New StreamReader(sslStream, _options.Encoding, False, 1024, leaveOpen:=True)
        End Function

        Private Function ValidateServerCertificate(
        sender As Object,
        certificate As X509Certificate,
        chain As X509Chain,
        sslPolicyErrors As SslPolicyErrors) As Boolean

            ' 用户关闭了验证 → 接受所有证书
            If Not _options.ValidateCertificate Then Return True
            ' 无错误 → 接受
            If sslPolicyErrors = SslPolicyErrors.None Then Return True
            ' 有错误且开启了验证 → 拒绝
            Return False
        End Function

        Private Async Function AuthenticateAsync(ct As CancellationToken) As Task
            Dim creds = _credentials
            If creds Is Nothing Then creds = FtpCredentials.Anonymous

            ' --- USER ---
            Dim resp = Await SendCommandAsync("USER " & creds.UserName, ct)
            If resp.StatusCode = 331 Then
                ' 需要密码
                resp = Await SendCommandAsync("PASS " & creds.Password, ct)
            End If

            If resp.StatusCode <> 230 Then
                Throw New FtpAuthenticationException("认证失败: " & resp.ToString(), resp.StatusCode)
            End If

            _isAuthenticated = True

            ' --- TLS 数据通道保护 ---
            If _isTlsActive Then
                Await SendCommandAsync("PBSZ 0", ct)
                If _options.DataChannelEncryption Then
                    Await SendCommandAsync("PROT P", ct)
                Else
                    Await SendCommandAsync("PROT C", ct)
                End If
            End If

            ' --- 尝试 UTF-8 ---
            Dim optsResp = Await SendCommandAsync("OPTS UTF8 ON", ct)
            ' 500/502 表示不支持，忽略

            ' --- 二进制模式 ---
            Await SendCommandAsync("TYPE I", ct)
        End Function

        ' ========================================================================
        '  内部实现: 下载
        ' ========================================================================

        Private Async Function DownloadInternalAsync(
        remotePath As String,
        destination As Stream,
        ct As CancellationToken,
        progress As IProgress(Of FtpDownloadProgress)) As Task

            ' --- 确保二进制模式 ---
            Await SendCommandAsync("TYPE I", ct)

            ' --- 获取文件大小 (用于进度, best-effort) ---
            Dim fileSize As Long = -1
            Try
                Dim sizeResp = Await SendCommandAsync("SIZE " & remotePath, ct)
                If sizeResp.StatusCode = 213 Then
                    Dim parts = sizeResp.StatusText.Split({" "c}, StringSplitOptions.RemoveEmptyEntries)
                    If parts.Length >= 1 Then
                        Long.TryParse(parts(0), fileSize)
                    End If
                End If
            Catch
                ' SIZE 不支持时忽略
            End Try

            ' --- 打开数据连接 ---
            Dim dataStream As Stream = Await OpenDataConnectionAsync(ct)

            ' --- 发送 RETR ---
            Dim retrResp = Await SendCommandAsync("RETR " & remotePath, ct)
            If retrResp.StatusCode = 550 Then
                dataStream?.Dispose()
                CloseDataConnection()
                Throw New FtpFileNotFoundException(
                "文件未找到或无权限: " & remotePath, retrResp.StatusCode)
            End If
            If retrResp.StatusCode <> 150 AndAlso retrResp.StatusCode <> 125 Then
                dataStream?.Dispose()
                CloseDataConnection()
                Throw New FtpException("RETR 失败: " & retrResp.ToString(), retrResp.StatusCode)
            End If

            ' --- 从数据连接读取数据 ---
            Dim buffer(BufferSize - 1) As Byte
            Dim totalRead As Long = 0
            Dim sw As Stopwatch = Stopwatch.StartNew()
            Dim lastReportTime As Long = Stopwatch.GetTimestamp()
            Const ReportIntervalMs As Integer = 100

            Try
                ' 取消时关闭数据连接以中止传输
                Using ctReg As CancellationTokenRegistration = ct.Register(
                    Sub()
                        Try
                            _dataTcpClient?.Close()
                        Catch
                        End Try
                    End Sub)

                    Do
                        Dim read As Integer = Await dataStream.ReadAsync(buffer, ct)
                        If read = 0 Then Exit Do

                        Await destination.WriteAsync(buffer, 0, read, ct)
                        totalRead += read

                        ' 节流进度报告 (每 100ms)
                        If progress IsNot Nothing Then
                            Dim elapsed = Stopwatch.GetElapsedTime(lastReportTime)
                            If elapsed.TotalMilliseconds >= ReportIntervalMs Then
                                progress.Report(New FtpDownloadProgress(totalRead, fileSize, sw.Elapsed))
                                lastReportTime = Stopwatch.GetTimestamp()
                            End If
                        End If
                    Loop
                End Using

                ' 最终进度报告
                If progress IsNot Nothing Then
                    progress.Report(New FtpDownloadProgress(totalRead, If(fileSize >= 0, fileSize, totalRead), sw.Elapsed))
                End If

            Catch ex As OperationCanceledException
                Call Cancel.GetAwaiter.GetResult()
                Throw
            Finally
                dataStream?.Dispose()
                CloseDataConnection()
            End Try

            ' --- 读取传输完成响应 (226) ---
            Dim completeResp = Await ReadResponseAsync(ct)
            If completeResp.StatusCode <> 226 AndAlso completeResp.StatusCode <> 250 Then
                Throw New FtpException("传输未完成: " & completeResp.ToString(), completeResp.StatusCode)
            End If
        End Function

        Private Async Function Cancel() As Task
            ' 用户取消: 尝试发送 ABOR
            Try
                Await SendCommandAsync("ABOR", CancellationToken.None)
                Await ReadResponseAsync(CancellationToken.None)
            Catch
                ' 控制连接可能已不可用
                _isConnected = False
            End Try
        End Function

        ' ========================================================================
        '  内部实现: 数据连接 (PASV / EPSV)
        ' ========================================================================

        Private Async Function OpenDataConnectionAsync(ct As CancellationToken) As Task(Of Stream)
            Dim dataHost As String = _host
            Dim dataPort As Integer = 0

            ' --- 尝试 EPSV (兼容 IPv4/IPv6) ---
            Dim useEpsv As Boolean = True
            Dim epsvResp As FtpResponse = Nothing

            Try
                epsvResp = Await SendCommandAsync("EPSV", ct)
            Catch
                useEpsv = False
            End Try

            If useEpsv AndAlso epsvResp.StatusCode = 229 Then
                dataPort = ParseEpsvResponse(epsvResp.StatusText)
                dataHost = _host
            Else
                ' --- 回退到 PASV (仅 IPv4) ---
                Dim pasvResp = Await SendCommandAsync("PASV", ct)
                If pasvResp.StatusCode <> 227 Then
                    Throw New FtpException("PASV 失败: " & pasvResp.ToString(), pasvResp.StatusCode)
                End If
                Dim parsed = ParsePasvResponse(pasvResp.StatusText)
                dataHost = parsed.Host
                dataPort = parsed.Port
            End If

            ' --- 连接数据通道 ---
            _dataTcpClient = New TcpClient()

            Dim connectCts = CancellationTokenSource.CreateLinkedTokenSource(ct)
            connectCts.CancelAfter(_options.ConnectTimeout)
            Try
                Await _dataTcpClient.ConnectAsync(dataHost, dataPort).WaitAsync(connectCts.Token)
            Catch ex As OperationCanceledException When (Not ct.IsCancellationRequested)
                SafeCloseTcp(_dataTcpClient)
                _dataTcpClient = Nothing
                Throw New FtpConnectionException(
                $"数据连接到 {dataHost}:{dataPort} 超时")
            Catch ex As SocketException
                SafeCloseTcp(_dataTcpClient)
                _dataTcpClient = Nothing
                Throw New FtpConnectionException(
                $"数据连接到 {dataHost}:{dataPort} 失败: {ex.Message}", ex)
            Finally
                connectCts.Dispose()
            End Try

            Dim dataStream As Stream = _dataTcpClient.GetStream()

            ' --- TLS 加密数据通道 (如果控制连接已加密且启用了数据通道保护) ---
            If _isTlsActive AndAlso _options.DataChannelEncryption Then
                Dim sslData As New SslStream(dataStream, leaveInnerStreamOpen:=False,
                AddressOf ValidateServerCertificate)

                Dim authOptions As New SslClientAuthenticationOptions With {
                .TargetHost = _host,
                .RemoteCertificateValidationCallback = AddressOf ValidateServerCertificate
            }
                Await sslData.AuthenticateAsClientAsync(authOptions, ct)
                dataStream = sslData
            End If

            Return dataStream
        End Function

        ' ========================================================================
        '  内部实现: 命令收发
        ' ========================================================================

        Private Async Function SendCommandAsync(
        command As String, ct As CancellationToken) As Task(Of FtpResponse)

            If _controlStream Is Nothing Then
                Throw New FtpConnectionException("未连接到 FTP 服务器。")
            End If

            ' 写入命令 (CRLF 结尾)
            Dim data() As Byte = _options.Encoding.GetBytes(command & CrLf)
            Await _controlStream.WriteAsync(data, 0, data.Length, ct)
            Await _controlStream.FlushAsync(ct)

            ' 读取响应
            Return Await ReadResponseAsync(ct)
        End Function

        Private Async Function ReadResponseAsync(ct As CancellationToken) As Task(Of FtpResponse)
            Dim lines As New List(Of String)
            Dim codeStr As String = Nothing

            ' --- 读取第一行 ---
            Dim line As String = Await _reader.ReadLineAsync(ct)
            If line Is Nothing Then
                Throw New FtpConnectionException("服务器关闭了连接。")
            End If
            If line.Length < 3 Then
                Throw New FtpProtocolException("无效的 FTP 响应: " & line)
            End If

            codeStr = line.Substring(0, 3)
            lines.Add(line)

            ' --- 检查是否为多行响应 (第 4 个字符是 '-') ---
            Dim isMultiLine As Boolean = line.Length >= 4 AndAlso line.Chars(3) = "-"c

            ' --- 读取后续行直到结束 ---
            Do While isMultiLine
                line = Await _reader.ReadLineAsync(ct)
                If line Is Nothing Then
                    Throw New FtpConnectionException("读取多行响应时连接被关闭。")
                End If
                lines.Add(line)

                ' 结束行: 同样的状态码 + 空格
                If line.Length >= 4 AndAlso line.Substring(0, 3) = codeStr AndAlso line.Chars(3) = " "c Then
                    Exit Do
                End If
                If line.Length = 3 AndAlso line.Substring(0, 3) = codeStr Then
                    Exit Do
                End If
            Loop

            ' --- 解析状态码 ---
            Dim statusCode As Integer
            If Not Integer.TryParse(codeStr, statusCode) Then
                Throw New FtpProtocolException("无效的状态码: " & codeStr)
            End If

            ' --- 提取状态文本 (最后一行去掉 "NNN " 前缀) ---
            Dim lastLine As String = lines(lines.Count - 1)
            Dim statusText As String
            If lastLine.Length >= 4 Then
                statusText = lastLine.Substring(4)
            Else
                statusText = lastLine
            End If

            Return New FtpResponse(statusCode, statusText, lines.AsReadOnly())
        End Function

        ' ========================================================================
        '  内部实现: 响应解析
        ' ========================================================================

        ''' <summary>
        ''' 解析 PASV 响应: "Entering Passive Mode (h1,h2,h3,h4,p1,p2)"
        ''' </summary>
        Private Function ParsePasvResponse(statusText As String) As (Host As String, Port As Integer)
            Dim start As Integer = statusText.IndexOf("("c)
            Dim [end] As Integer = statusText.IndexOf(")"c)

            If start < 0 OrElse [end] < 0 OrElse [end] <= start + 1 Then
                Throw New FtpProtocolException("无效的 PASV 响应: " & statusText)
            End If

            Dim inner As String = statusText.Substring(start + 1, [end] - start - 1)
            Dim parts() As String = inner.Split(","c)
            If parts.Length < 6 Then
                Throw New FtpProtocolException("无效的 PASV 响应: " & statusText)
            End If

            Dim ip As String = parts(0) & "." & parts(1) & "." & parts(2) & "." & parts(3)
            Dim port As Integer = Integer.Parse(parts(4)) * 256 + Integer.Parse(parts(5))
            Return (ip, port)
        End Function

        ''' <summary>
        ''' 解析 EPSV 响应: "Entering Extended Passive Mode (|||port|)"
        ''' </summary>
        Private Function ParseEpsvResponse(statusText As String) As Integer
            Dim start As Integer = statusText.IndexOf("("c)
            Dim [end] As Integer = statusText.IndexOf(")"c)

            If start < 0 OrElse [end] < 0 OrElse [end] <= start + 3 Then
                Throw New FtpProtocolException("无效的 EPSV 响应: " & statusText)
            End If

            Dim inner As String = statusText.Substring(start + 1, [end] - start - 1)
            ' inner 格式: "|||port|" → 第一个字符是分隔符，之后 2 个分隔符，然后是端口，最后 1 个分隔符
            Dim delim As Char = inner.Chars(0)
            Dim portStart As Integer = 3  ' 跳过 3 个分隔符
            Dim portEnd As Integer = inner.IndexOf(delim, portStart)

            If portEnd < 0 Then
                Throw New FtpProtocolException("无效的 EPSV 响应: " & statusText)
            End If

            Dim portStr As String = inner.Substring(portStart, portEnd - portStart)
            Return Integer.Parse(portStr)
        End Function

        ''' <summary>
        ''' 解析 MDTM 日期: "20230817123045" → DateTime (UTC)
        ''' </summary>
        Private Shared Function ParseMdtmDate(dateStr As String) As DateTime
            ' 格式: YYYYMMDDHHMMSS (可能带 .sss 毫秒部分)
            If dateStr.Length < 14 Then Return DateTime.MinValue

            Try
                Dim year As Integer = Integer.Parse(dateStr.Substring(0, 4))
                Dim month As Integer = Integer.Parse(dateStr.Substring(4, 2))
                Dim day As Integer = Integer.Parse(dateStr.Substring(6, 2))
                Dim hour As Integer = Integer.Parse(dateStr.Substring(8, 2))
                Dim minute As Integer = Integer.Parse(dateStr.Substring(10, 2))
                Dim second As Integer = Integer.Parse(dateStr.Substring(12, 2))
                Return New DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc)
            Catch
                Return DateTime.MinValue
            End Try
        End Function

        ' ========================================================================
        '  内部实现: 资源清理
        ' ========================================================================

        Private Sub CloseDataConnection()
            If _dataTcpClient IsNot Nothing Then
                Try : _dataTcpClient.Close() : Catch : End Try
                _dataTcpClient = Nothing
            End If
        End Sub

        Private Sub CloseControlConnection()
            If _reader IsNot Nothing Then
                Try : _reader.Dispose() : Catch : End Try
                _reader = Nothing
            End If
            If _controlStream IsNot Nothing Then
                Try : _controlStream.Dispose() : Catch : End Try
                _controlStream = Nothing
            End If
            If _tcpClient IsNot Nothing Then
                Try : _tcpClient.Close() : Catch : End Try
                _tcpClient = Nothing
            End If
        End Sub

        Private Shared Sub SafeCloseTcp(ByRef client As TcpClient)
            If client IsNot Nothing Then
                Try : client.Close() : Catch : End Try
                client = Nothing
            End If
        End Sub

        Private Sub ThrowIfDisposed()
            If _isDisposed Then
                Throw New ObjectDisposedException(NameOf(FtpClient))
            End If
        End Sub

        ' ========================================================================
        '  IDisposable
        ' ========================================================================

        Public Sub Dispose() Implements IDisposable.Dispose
            If _isDisposed Then Return
            CloseDataConnection()
            CloseControlConnection()
            _isConnected = False
            _isAuthenticated = False
            _isDisposed = True
            GC.SuppressFinalize(Me)
        End Sub

        Protected Overrides Sub Finalize()
            Try
                CloseDataConnection()
                CloseControlConnection()
            Finally
                MyBase.Finalize()
            End Try
        End Sub

    End Class
End Namespace
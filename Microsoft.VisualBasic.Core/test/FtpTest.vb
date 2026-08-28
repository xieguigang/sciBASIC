#Region "Microsoft.VisualBasic::b0f16648ad741d20d8295a4c93bd2428, Microsoft.VisualBasic.Core\test\FtpTest.vb"

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

    '   Total Lines: 224
    '    Code Lines: 170 (75.89%)
    ' Comment Lines: 24 (10.71%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 30 (13.39%)
    '     File Size: 9.06 KB


    ' Module FtpTest
    ' 
    '     Function: FormatSize
    ' 
    '     Sub: ShowHelp
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' Program.vb - FtpClient 命令行演示
'
' 用法:
'   FtpClient <host> <remotePath> [localPath] [user] [password] [--ssl]
'
' 示例:
'   # 匿名下载
'   FtpClient ftp.gnu.org /gnu/gnu/gnu-1.0.tar.gz
'
'   # 认证下载
'   FtpClient 192.168.1.100 /data/report.zip C:\Downloads\report.zip user mypass
'
'   # 使用 FTPS
'   FtpClient ftp.example.com /file.txt file.txt user pass --ssl
' ============================================================================

Imports System.IO
Imports Microsoft.VisualBasic.Net.FTP
Imports Microsoft.VisualBasic.Net.WebClient

Module FtpTest

    Public Async Function Run(args As String()) As Task
        Console.OutputEncoding = Text.Encoding.UTF8

        If args.Length < 2 Then
            ShowHelp()
            Return
        End If

        Dim host As String = args(0)
        Dim remotePath As String = args(1)
        Dim localPath As String = If(args.Length > 2 AndAlso Not args(2).StartsWith("--"),
                                     args(2), Path.GetFileName(remotePath))

        Dim user As String = "anonymous"
        Dim password As String = "anonymous@localhost"
        Dim port As Integer = 21
        Dim useSsl As Boolean = False
        Dim skipCert As Boolean = False
        Dim useWget As Boolean = False

        ' 解析可选位置参数
        Dim idx As Integer = 2
        If args.Length > 2 AndAlso Not args(2).StartsWith("--") Then
            ' args(2) 是 localPath，继续看后面的
            If args.Length > 3 AndAlso Not args(3).StartsWith("--") Then
                user = args(3) : idx = 3
                If args.Length > 4 AndAlso Not args(4).StartsWith("--") Then
                    password = args(4) : idx = 4
                End If
            End If
        End If

        ' 解析开关
        For i As Integer = idx + 1 To args.Length - 1
            Select Case args(i)
                Case "--port"
                    If i + 1 < args.Length Then Integer.TryParse(args(i + 1), port) : i += 1
                Case "--ssl", "--ftps"
                    useSsl = True
                Case "--no-cert", "--skip-cert"
                    skipCert = True
                Case "--wget"
                    useWget = True
            End Select
        Next

        ' 配置
        Dim options As New FtpClientOptions With {
            .EnableSsl = useSsl,
            .ValidateCertificate = Not skipCert,
            .ConnectTimeout = TimeSpan.FromSeconds(30)
        }

        Dim creds As New FtpCredentials(user, password)

        Console.WriteLine($"FTP 服务器 : {host}:{port}")
        Console.WriteLine($"远程文件   : {remotePath}")
        Console.WriteLine($"本地保存   : {localPath}")
        Console.WriteLine($"用户       : {user}")
        Console.WriteLine($"FTPS 加密  : {If(useSsl, "是", "否")}")
        Console.WriteLine()

        Using client As New FtpClient(host, port, options, creds)

            ' 显示文件信息
            Try
                Console.WriteLine("正在获取文件信息...")
                Dim info = Await client.GetFileInfoAsync(remotePath)
                Console.WriteLine($"  文件大小 : {FormatSize(info.Size)}")
                If info.LastModified <> DateTime.MinValue Then
                    Console.WriteLine($"  修改时间 : {info.LastModified:yyyy-MM-dd HH:mm:ss} UTC")
                End If
                Console.WriteLine()
            Catch ex As Exception
                Console.WriteLine($"  (无法获取文件信息: {ex.Message})")
                Console.WriteLine()
            End Try

            ' 列出远程根目录 (NLST)
            Try
                Console.WriteLine("正在列目录 (NLST /)...")
                Dim listing = Await client.ListDirectoryAsync("/")
                Console.WriteLine($"  共 {listing.Length} 个条目:")
                For Each item In listing
                    Console.WriteLine($"    - {item}")
                Next
                Console.WriteLine()
            Catch ex As Exception
                Console.WriteLine($"  (无法列目录: {ex.Message})")
                Console.WriteLine()
            End Try

            ' 下载文件
            Dim lastPercent As Double = -1
            Dim progress As New Progress(Of FtpDownloadProgress)(
                Sub(p)
                    If p.TotalBytes >= 0 Then
                        Dim pct As Double = p.ProgressPercentage
                        If pct - lastPercent >= 1 OrElse pct = 100 Then
                            Console.Write($"\r  下载中: {FormatSize(p.BytesTransferred)} / {FormatSize(p.TotalBytes)}" &
                                          $" ({pct:F1}%)  {FormatSize(CLng(p.TransferRateBytesPerSecond))}/s")
                            lastPercent = pct
                        End If
                    Else
                        Console.Write($"\r  下载中: {FormatSize(p.BytesTransferred)}  {FormatSize(CLng(p.TransferRateBytesPerSecond))}/s")
                    End If
                End Sub)

            Try
                Console.WriteLine("开始下载...")
                Dim sw As Stopwatch = Stopwatch.StartNew()

                Await client.DownloadFileAsync(remotePath, localPath,
                                              overwrite:=True,
                                              progress:=progress)

                sw.Stop()
                Console.WriteLine()
                Console.WriteLine($"下载完成! 用时 {sw.Elapsed.TotalSeconds:F1}s")
                Console.WriteLine($"本地文件: {localPath} ({FormatSize(New FileInfo(localPath).Length)})")

            Catch ex As FtpFileNotFoundException
                Console.WriteLine()
                Console.WriteLine($"文件未找到: {ex.Message}")
            Catch ex As FtpAuthenticationException
                Console.WriteLine()
                Console.WriteLine($"认证失败: {ex.Message}")
            Catch ex As FtpConnectionException
                Console.WriteLine()
                Console.WriteLine($"连接失败: {ex.Message}")
            Catch ex As FtpException
                Console.WriteLine()
                Console.WriteLine($"FTP 错误: {ex.Message}")
            Catch ex As Exception
                Console.WriteLine()
                Console.WriteLine($"错误: {ex.Message}")
                Console.WriteLine(ex.StackTrace)
            End Try

        End Using

        ' 可选: 验证重构后的 FtpDownloader (替代 FtpWebRequest)
        If useWget Then
            Try
                Console.WriteLine()
                Console.WriteLine("验证重构后的 FtpDownloader (ftp:// 下载)...")
                Dim wgetPath As String = "_wget_" & Path.GetFileName(remotePath)
                Dim ftpUrl As String = $"ftp://{host}:{port}{remotePath}"
                Dim wget As New FtpDownloader(ftpUrl, wgetPath, user, password)
                AddHandler wget.DownloadProgressChanged, Sub(s, e)
                                                             Console.WriteLine($"    {FormatSize(e.BytesReceived)} / {FormatSize(e.TotalBytes)}")
                                                         End Sub
                Await wget.DownloadFileAsync()
                Console.WriteLine($"  FtpDownloader 完成: {wgetPath} ({FormatSize(New FileInfo(wgetPath).Length)})")
                Console.WriteLine()
            Catch ex As Exception
                Console.WriteLine($"  FtpDownloader 失败: {ex.Message}")
                Console.WriteLine(ex.StackTrace)
            End Try
        End If
    End Function

    Private Function FormatSize(bytes As Long) As String
        If bytes < 0 Then Return "未知"
        If bytes < 1024 Then Return bytes & " B"
        If bytes < 1024 * 1024 Then Return (bytes / 1024).ToString("F1") & " KB"
        If bytes < 1024 * 1024 * 1024 Then Return (bytes / (1024 * 1024)).ToString("F1") & " MB"
        Return (bytes / (1024 * 1024 * 1024)).ToString("F2") & " GB"
    End Function

    Private Sub ShowHelp()
        Console.WriteLine(
"FtpClient - VB.NET FTP 客户端命令行工具

用法:
  FtpClient <host> <remotePath> [localPath] [user] [password] [选项]

参数:
  host           FTP 服务器地址
  remotePath     远程文件路径
  localPath      本地保存路径 (默认: 当前目录下的文件名)
  user           用户名 (默认: anonymous)
  password       密码 (默认: anonymous@localhost)

选项:
  --port <n>     端口 (默认: 21, 本地测试可指到 2121)
  --ssl           使用 FTPS (FTP over TLS)
  --no-cert       跳过证书验证 (用于自签名证书)

示例:
  # 匿名下载
  FtpClient ftp.gnu.org /gnu/gnu/gnu-1.0.tar.gz

  # 认证下载到指定路径
  FtpClient 192.168.1.100 /data/report.zip C:\Downloads\report.zip admin secretpass

  # 使用 FTPS + 跳过证书
  FtpClient ftp.example.com /file.txt file.txt user pass --ssl --no-cert")
    End Sub

End Module


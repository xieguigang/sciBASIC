Imports System.Collections.Generic
Imports System.IO
Imports System.IO.Compression
Imports System.Linq
Imports System.Net.Http
Imports System.Text.Json
Imports Microsoft.VisualBasic.Net.Http

Namespace VBProj.NuGet

    ''' <summary>
    ''' 轻量 nuget 客户端: 基于 nuget.org flat-container 协议获取包, 并复用
    ''' NuGet 全局包目录(<c>~/.nuget/packages</c>)作为本地缓存。
    ''' </summary>
    ''' <remarks>
    ''' 功能边界(刻意保持轻量, 不引入 NuGet 官方客户端库):
    ''' <list type="bullet">
    ''' <item>版本索引: <c>{source}/{id}/index.json</c>;</item>
    ''' <item>包下载: <c>{source}/{id}/{version}/{id}.{version}.nupkg</c>(流式落盘);</item>
    ''' <item>本地缓存: 解压到 <c>~/.nuget/packages/{id}/{version}/</c>, 布局与 NuGet 完全一致,
    ''' 因此可以直接复用其它工具(Visual Studio / dotnet restore)已经安装过的包;
    ''' 以 NuGet 的 <c>.nupkg.metadata</c> 作为"安装完成"标记, 避免复用半成品目录;</item>
    ''' <item>原子写入: 先解压到同级临时目录, 全部成功后再改名, 中断不会污染缓存。</item>
    ''' </list>
    ''' </remarks>
    Public Class NuGetClient

        ''' <summary>nuget.org v3 flat-container 源</summary>
        Public Const NuGetOrgSource As String = "https://api.nuget.org/v3-flatcontainer/"

        ''' <summary>NuGet 安装完成标记文件名</summary>
        Public Const InstallMarker As String = ".nupkg.metadata"

        ''' <summary>进程内的版本索引缓存(避免同一 id 反复联网)</summary>
        Private Shared ReadOnly VersionCache As New Dictionary(Of String, NuGetVersion())(StringComparer.OrdinalIgnoreCase)
        Private Shared ReadOnly CacheLock As New Object()
        Private Shared ReadOnly InstallLock As New Object()

        ''' <summary>包源地址(以 <c>/</c> 结尾)</summary>
        Public Property Source As String = NuGetOrgSource

        ''' <summary>本地缓存根目录(默认为 NuGet 全局包目录)</summary>
        Public Property CacheRoot As String

        Public Sub New()
            CacheRoot = DefaultCacheRoot
        End Sub

        Public Sub New(source As String, Optional cacheRoot As String = Nothing)
            Me.Source = If(String.IsNullOrEmpty(source), NuGetOrgSource, source)
            Me.CacheRoot = If(String.IsNullOrEmpty(cacheRoot), DefaultCacheRoot, cacheRoot)
        End Sub

        ''' <summary>
        ''' NuGet 全局包目录: 优先取环境变量 <c>NUGET_PACKAGES</c>, 否则取
        ''' <c>~/.nuget/packages</c>。
        ''' </summary>
        Public Shared ReadOnly Property DefaultCacheRoot As String
            Get
                Dim env As String = Environment.GetEnvironmentVariable("NUGET_PACKAGES")

                If Not String.IsNullOrWhiteSpace(env) Then
                    Return env
                End If

                Return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".nuget",
                    "packages")
            End Get
        End Property

        ''' <summary>
        ''' 读取一个包的可用版本列表(按版本号降序)。结果会按 id 缓存在进程内。
        ''' </summary>
        Public Function GetVersions(packageId As String) As NuGetVersion()
            If String.IsNullOrWhiteSpace(packageId) Then
                Throw New NuGetException("nuget 包 id 不能为空")
            End If

            SyncLock CacheLock
                Dim cached As NuGetVersion() = Nothing

                If VersionCache.TryGetValue(packageId, cached) Then
                    Return cached
                End If
            End SyncLock

            Dim url As String = $"{Source.TrimEnd("/"c)}/{packageId.ToLowerInvariant()}/index.json"
            Dim json As String

            Try
                json = HttpClientFactory.GetStringSync(url)
            Catch ex As Exception
                Throw New NuGetException($"无法从 nuget 源读取包 '{packageId}' 的版本索引: {url}{vbCrLf}{ex.Message}", ex)
            End Try

            Dim versions = ParseVersionIndex(json, packageId) _
                .OrderByDescending(Function(v) v) _
                .ToArray()

            If versions.Length = 0 Then
                Throw New NuGetException($"nuget 包 '{packageId}' 没有任何可用版本: {url}")
            End If

            SyncLock CacheLock
                VersionCache(packageId) = versions
            End SyncLock

            Return versions
        End Function

        Private Function ParseVersionIndex(json As String, packageId As String) As IEnumerable(Of NuGetVersion)
            Dim list As New List(Of NuGetVersion)

            Using doc As JsonDocument = JsonDocument.Parse(json)
                Dim versions As JsonElement

                If Not doc.RootElement.TryGetProperty("versions", versions) Then
                    Return list
                End If

                For Each item As JsonElement In versions.EnumerateArray()
                    Dim version As NuGetVersion = Nothing

                    If NuGetVersion.TryParse(item.GetString(), version) Then
                        Call list.Add(version)
                    End If
                Next
            End Using

            Return list
        End Function

        ''' <summary>
        ''' 取最新版本: 默认取最新稳定版(跳过 prerelease); <paramref name="allowPrerelease"/> 为 True 时取最新版。
        ''' </summary>
        Public Function GetBestVersion(packageId As String, Optional allowPrerelease As Boolean = False) As NuGetVersion
            Dim versions As NuGetVersion() = GetVersions(packageId)
            Dim stable As NuGetVersion() = versions.Where(Function(v) Not v.IsPrerelease).ToArray()

            If Not allowPrerelease AndAlso stable.Length > 0 Then
                Return stable(0)
            End If

            Return versions(0)
        End Function

        ''' <summary>包解压目录: <c>{cache}/{id-lower}/{version-lower}</c></summary>
        Public Function GetPackageFolder(packageId As String, version As NuGetVersion) As String
            Return Path.Combine(
                CacheRoot,
                packageId.ToLowerInvariant(),
                version.FolderName)
        End Function

        ''' <summary>该包是否已经完成安装(存在解压目录与安装标记)</summary>
        Public Function IsInstalled(packageId As String, version As NuGetVersion) As Boolean
            Dim folder As String = GetPackageFolder(packageId, version)

            Return Directory.Exists(folder) AndAlso
                File.Exists(Path.Combine(folder, InstallMarker)) AndAlso
                FindNuspec(folder) IsNot Nothing
        End Function

        ''' <summary>
        ''' 确保指定版本的包已经存在于本地缓存, 返回包的解压目录。
        ''' 缓存未命中时从包源下载并解压。
        ''' </summary>
        Public Function Install(packageId As String, version As NuGetVersion) As String
            Dim folder As String = GetPackageFolder(packageId, version)

            If IsInstalled(packageId, version) Then
                Return folder
            End If

            SyncLock InstallLock
                If IsInstalled(packageId, version) Then
                    Return folder
                End If

                Call DownloadAndExtract(packageId, version, folder)
            End SyncLock

            Return folder
        End Function

        Private Sub DownloadAndExtract(packageId As String, version As NuGetVersion, folder As String)
            Dim id As String = packageId.ToLowerInvariant()
            Dim ver As String = version.FolderName
            Dim url As String = $"{Source.TrimEnd("/"c)}/{id}/{ver}/{id}.{ver}.nupkg"
            Dim parent As String = Path.GetDirectoryName(folder)
            Dim staging As String = folder & ".tmp-" & Guid.NewGuid().ToString("N")
            Dim nupkg As String = Path.Combine(Path.GetTempPath(), $"{id}.{ver}.{Guid.NewGuid().ToString("N")}.nupkg")

            Call Directory.CreateDirectory(parent)

            Try
                Call Download(url, nupkg)

                Call Directory.CreateDirectory(staging)

                Try
                    ZipFile.ExtractToDirectory(nupkg, staging, overwriteFiles:=True)
                Catch ex As Exception
                    Throw New NuGetException($"解压 nuget 包失败: {packageId}@{ver}{vbCrLf}{url}{vbCrLf}{ex.Message}", ex)
                End Try

                If FindNuspec(staging) Is Nothing Then
                    Throw New NuGetException($"nuget 包 '{packageId}@{ver}' 中找不到 .nuspec 描述文件: {url}")
                End If

                ' 安装标记: 必须与 NuGet 自己的格式兼容 —— 当前版本的 NuGet restore
                ' 会反序列化该文件, 并且要求 contentHash 属性存在(缺失时 restore 直接报错)。
                Call File.WriteAllText(
                    Path.Combine(staging, InstallMarker),
                    "{""version"":2,""contentHash"":""" & ContentHash(nupkg) & """,""source"":""" & url & """}")

                If Directory.Exists(folder) Then
                    ' 已有半成品目录(缺少安装标记), 直接替换
                    Call Directory.Delete(folder, recursive:=True)
                End If

                Call Directory.Move(staging, folder)
            Finally
                Call Cleanup(staging)

                If File.Exists(nupkg) Then
                    Try
                        Call File.Delete(nupkg)
                    Catch ex As Exception
                        ' 临时文件清理失败不影响主流程
                    End Try
                End If
            End Try
        End Sub

        ''' <summary>
        ''' 计算 nupkg 的内容哈希(base64 编码的 SHA512), 用于写入 NuGet 兼容的安装标记。
        ''' </summary>
        Private Shared Function ContentHash(nupkg As String) As String
            Using sha As System.Security.Cryptography.SHA512 = System.Security.Cryptography.SHA512.Create()
                Using stream As New FileStream(nupkg, FileMode.Open, FileAccess.Read, FileShare.Read)
                    Return Convert.ToBase64String(sha.ComputeHash(stream))
                End Using
            End Using
        End Function

        Private Sub Download(url As String, target As String)
            Try
                Using response As HttpResponseMessage =
                    HttpClientFactory.Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult()

                    If Not response.IsSuccessStatusCode Then
                        Throw New NuGetException(
                            $"下载 nuget 包失败({CInt(response.StatusCode)}): {url}")
                    End If

                    Using network As Stream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult()
                        Using file As New FileStream(target, FileMode.Create, FileAccess.Write, FileShare.None)
                            Call network.CopyTo(file)
                        End Using
                    End Using
                End Using
            Catch ex As NuGetException
                Throw
            Catch ex As Exception
                Throw New NuGetException(
                    $"下载 nuget 包失败(请检查网络或代理设置): {url}{vbCrLf}{ex.Message}", ex)
            End Try
        End Sub

        Private Shared Sub Cleanup(folder As String)
            If Not Directory.Exists(folder) Then
                Return
            End If

            Try
                Call Directory.Delete(folder, recursive:=True)
            Catch ex As Exception
                ' 清理失败不影响主流程
            End Try
        End Sub

        ''' <summary>
        ''' 在包目录根层查找 <c>.nuspec</c> 描述文件; 不存在时返回 Nothing。
        ''' </summary>
        Public Shared Function FindNuspec(packageFolder As String) As String
            If String.IsNullOrEmpty(packageFolder) OrElse Not Directory.Exists(packageFolder) Then
                Return Nothing
            End If

            Dim files As String() = Directory.GetFiles(packageFolder, "*.nuspec", SearchOption.TopDirectoryOnly)

            If files.Length = 0 Then
                Return Nothing
            End If

            ' 优先取与包目录同名的 nuspec
            Dim name As String = Path.GetFileName(packageFolder)

            Return files _
                .OrderByDescending(Function(f) String.Equals(
                    Path.GetFileNameWithoutExtension(f), name, StringComparison.OrdinalIgnoreCase)) _
                .First()
        End Function

        ''' <summary>清空进程内的版本索引缓存(主要用于测试)</summary>
        Public Shared Sub ClearVersionCache()
            SyncLock CacheLock
                Call VersionCache.Clear()
            End SyncLock
        End Sub
    End Class
End Namespace

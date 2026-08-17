---
name: FtpClient测试与FtpWebRequest重构
overview: 使用 Python 编写简易 FTP 测试服务器，修改 test/FtpTest.vb 验证新 Net.FTP.FtpClient 模块的下载功能；测试通过后将仓库中所有 FtpWebRequest 过时代码重构为基于 FtpClient，并新增 ListDirectoryAsync 以支撑 R# webKit 的 list.ftp_dirs 功能。
todos:
  - id: python-ftp-server
    content: 编写 Python 明文 FTP 测试服务器（被动模式/认证/RETR/NLST/SIZE/MDTM）并自动生成测试文件
    status: completed
  - id: ftp-test-entry
    content: 改造 FtpTest.vb（Main 改 Run、新增 --port 参数）并在 Program.vb 增加 --ftp 入口分支
    status: completed
    dependencies:
      - python-ftp-server
  - id: verify-download
    content: 构建并运行 FTP 下载测试验证 FtpClient 下载/文件信息，修复发现的问题
    status: completed
    dependencies:
      - ftp-test-entry
  - id: add-list-directory
    content: 为 FtpClient 新增 ListDirectoryAsync(NLST) 并扩展测试验证列目录功能
    status: completed
    dependencies:
      - verify-download
  - id: refactor-wget
    content: 重构 FtpContext.vb（CreateFtpClient）与 FtpDownloader.vb 为 FtpClient 引用，移除 FtpWebRequest
    status: completed
    dependencies:
      - add-list-directory
  - id: refactor-rsharp
    content: 重构 R# webKit/FTP.vb 的 list_ftpdirs/ftpget 使用 FtpClient 同步阻塞包装
    status: completed
    dependencies:
      - refactor-wget
  - id: verify-cleanup
    content: 用 [subagent:code-explorer] 全仓库验证无 FtpWebRequest 残留，构建 Core/test/webKit 并清理临时文件
    status: completed
    dependencies:
      - refactor-rsharp
---

## 用户需求

1. 用 Python 编写一个简单的 FTP 测试服务器，用于验证新构建的 `Microsoft.VisualBasic.Net.FTP.FtpClient` 模块（该模块用于替代已过时的 `FtpWebRequest`）。
2. 通过修改 `test/FtpTest.vb` 进行测试，重点验证 **FTP 下载功能**。
3. FTP 下载测试通过后，将当前项目中所有引用 `FtpWebRequest` 的过时代码重构为针对新 `FtpClient` 模块的引用。

## 澄清确认

- **重构范围**：一并重构 R# webKit 项目（`R-sharp/studio/Rsharp_kit/webKit/FTP.vb`），保持全仓库可编译；为此需为 `FtpClient` 新增 `ListDirectoryAsync` 方法以支撑其 `list.ftp_dirs` 功能。
- **测试范围**：仅明文 FTP（不测 FTPS），Python 标准库实现，覆盖 USER/PASS 认证、EPSV/PASV、RETR 下载、SIZE/MDTM、NLST 列目录等主流程。

## 核心功能

- Python 明文 FTP 测试服务器（被动模式，匿名+账号密码认证，预置测试文件）
- 改造 `FtpTest.vb` 为可调用的测试入口并接入测试项目
- 验证 `FtpClient` 下载、文件信息查询、列目录功能
- 为 `FtpClient` 新增 `ListDirectoryAsync` 方法
- 重构 `FtpContext.vb`、`FtpDownloader.vb` 及 R# `webKit/FTP.vb`，彻底移除 `FtpWebRequest` 引用

## 技术栈

- Python 3 标准库（socket/threading/os）编写测试 FTP 服务器，零第三方依赖
- VB.NET / .NET 10（net10.0），dotnet CLI 构建与测试运行
- 被测试/重构对象：`Microsoft.VisualBasic.Net.FTP.FtpClient`（基于 TcpClient 从零实现）

## 实施方案

### 1. Python 明文 FTP 测试服务器（test/scripts/ftp_server.py）

单文件、纯标准库、每控制连接一线程。默认监听 `127.0.0.1:2121`（避免占用 21 端口需要管理员权限），`--port/--root/--user/--password` 可配置。启动时在 root 目录自动生成测试文件：`test_1MB.bin`（约 1MB 随机字节，用于内容一致性校验）与 `test_small.txt`（文本文件）。支持命令：

- 认证：`USER`(331)/`PASS`(230)/`QUIT`(221)，支持匿名（anonymous）与指定账号密码
- 会话：`TYPE I/A`(200)、`OPTS UTF8 ON`(200)、`SYST`(215)、`PWD`(257)、`CWD`(250)、`PBSZ`(200)/`PROT`(200，应答即可)、`ABOR`(226)
- 文件：`SIZE`(213)、`MDTM`(213 YYYYMMDDHHMMSS)、`RETR`(150→数据连接传输→226)、`NLST`(150→数据连接发送文件名列表→226)
- 被动模式：`EPSV`(229 `(|||port|)`)、`PASV`(227 `(h1,h2,h3,h4,p1,p2)`)，数据连接每次操作新建，支持 IPv4/IPv6
- 所有响应严格使用 CRLF 行尾；文件不存在返回 550

### 2. 测试入口改造（test/FtpTest.vb、test/Program.vb）

当前 `FtpTest.vb` 的 `Async Function Main` 与 `Program.vb` 的 `Sub Main` 存在双入口点冲突，必须解决：

- `FtpTest.vb`：`Async Function Main(args As String())` 重命名为 `Public Async Function Run(args As String()) As Task`，CLI 逻辑保留；新增 `--port <n>` 参数解析（默认 21），使 `New FtpClient(host, port, options, creds)` 可指向 2121 测试端口
- `Program.vb`：`Sub Main` 开头增加分支——当 `args(0) = "--ftp"` 时调用 `FtpTest.Run(args.Skip(1).ToArray()).GetAwaiter().GetResult()` 后 Return，不破坏现有测试序列

### 3. 下载测试验证（核心验收）

- 后台启动 Python 服务器 → `dotnet build` test.vbproj → 运行 `test.exe --ftp 127.0.0.1 --port 2121 /test_1MB.bin <out> <user> <pass>`
- 验证点：认证成功、`GetFileInfoAsync` 返回正确大小/时间、`DownloadFileAsync` 下载完成且进度回调正常、本地文件与远程大小一致（追加 SHA256 校验）
- 若发现 `FtpClient.vb` 内部实现 bug（协议解析、被动模式、传输完成判定等），一并修复

### 4. FtpClient 新增 ListDirectoryAsync（src/Net/FtpClient/FtpClient.vb）

```
Public Async Function ListDirectoryAsync(remotePath As String,
    Optional ct As CancellationToken = Nothing) As Task(Of String())
```

实现完全复用 `DownloadInternalAsync` 的数据连接模式（`OpenDataConnectionAsync` → `SendCommandAsync("NLST " & remotePath)` 期待 150/125 → 用 `StreamReader(_options.Encoding)` 从数据流逐行读取目录项 → 关闭数据连接 → 读取 226 完成响应）。`FtpClient` 支持 `IProgress(Of FtpDownloadProgress)` 与 `CancellationToken`，保持既有风格。

### 5. 重构 FtpContext.vb（src/Net/Wget/FtpContext.vb）

- 保留类与属性 `username/password/server`（R# 侧 `<RTypeExport("ftp", GetType(FtpContext))>` 依赖该类型存在）
- `CreateRequest(dir) As FtpWebRequest` 改为 `CreateFtpClient() As FtpClient`：匿名时传 `FtpCredentials.Anonymous`，否则 `New FtpCredentials(username, password)`，`New Net.FTP.FtpClient(server, 21, Nothing, creds)`；远程路径由调用方直接传给 `DownloadFileAsync/ListDirectoryAsync`，语义等价于原 `ftp://{server}/{path}` URI
- 移除 `System.Net` 引用与 `#Disable Warning SYSLIB0014`

### 6. 重构 FtpDownloader.vb（src/Net/Wget/FtpDownloader.vb）

保持对外契约不变（继承 `Net.WebClient`，`LocalSaveFile`/`DownloadFileAsync`/`OpenSaveStream`/进度事件），仅替换内部实现：

- 字段 `ReadOnly request As FtpWebRequest` 移除，改为持有 `FtpClient`（从 ftpUri 解析 host/port/AbsolutePath 构造，Uri.Port 默认时用 21）
- `DownloadFileAsync` 内部改为 `Await _client.DownloadAsync(_remotePath, saveStream, progress:=…)`，将 `FtpDownloadProgress.BytesTransferred/TotalBytes` 映射为 `ProgressUpdate(New ProgressChangedEventArgs(...))`，完成后 `ProgressFinished()`；内部创建的文件流才 Dispose，外部传入的 buffer 流保持不 Dispose（兼容原行为）

### 7. 重构 R# webKit/FTP.vb（R-sharp/studio/Rsharp_kit/webKit/FTP.vb）

- `Imports Microsoft.VisualBasic.Net.FTP`；删除 `System.Net` 相关
- `list_ftpdirs`：`Using client As FtpClient = ftp.CreateFtpClient() … client.ListDirectoryAsync(dir).GetAwaiter().GetResult() …`，保留原 try/throwEx 错误处理与目录名前缀清理逻辑
- `ftpget`：`client.DownloadFileAsync(file, filepath, overwrite:=True).GetAwaiter().GetResult()`，保留原 save 路径判定逻辑（`save.StringEmpty`/以 "/" 结尾时拼接 `file.FileName`）
- 因 `ExportAPI` 是同步函数，`GetAwaiter().GetResult()` 阻塞包装是必要取舍

### 8. 验证与清理

- `dotnet build` Core.vbproj 与 test.vbproj（Debug|AnyCPU），构建 webKit.NET5.vbproj 验证 R# 侧
- 全仓库搜索 `FtpWebRequest`/`FtpWebResponse`/`WebRequestMethods.Ftp` 确认仅剩注释提及，无代码引用
- 清理测试下载产物与临时文件（保留 ftp_server.py 供回归复用）

## 性能与可靠性

- 下载采用流式传输（80KB 缓冲区）与进度节流（100ms），测试文件 1MB 级，无性能瓶颈
- Python 服务器单线程处理单连接内命令，数据连接独立 socket 短生命周期，避免连接泄漏
- 重构保持所有既有公开 API 契约（FtpContext 属性、FtpDownloader 接口、R# 函数签名），将跨库破坏面控制在最小

## 目录结构

```
Microsoft.VisualBasic.Core/
├── test/
│   ├── scripts/
│   │   └── ftp_server.py        # [NEW] Python 明文 FTP 测试服务器，被动模式+认证+测试文件生成
│   ├── FtpTest.vb               # [MODIFY] Main→Run(args)，新增 --port 参数；下载/文件信息/列目录验证
│   └── Program.vb               # [MODIFY] 增加 "--ftp" 命令行分支调用 FtpTest.Run
└── src/
    ├── Net/
    │   ├── FtpClient/
    │   │   └── FtpClient.vb     # [MODIFY] 新增 ListDirectoryAsync（NLST 走数据连接）
    │   └── Wget/
    │       ├── FtpContext.vb    # [MODIFY] CreateRequest→CreateFtpClient()，移除 FtpWebRequest
    │       └── FtpDownloader.vb # [MODIFY] 内部改用 FtpClient 下载，保留对外契约与进度事件
    └── (R-sharp 跨项目)
        R-sharp/studio/Rsharp_kit/webKit/FTP.vb  # [MODIFY] 改用 FtpClient 同步阻塞包装
```

## 代理扩展

### SubAgent

- **code-explorer**
- 用途：在最终验证阶段执行全仓库（排除 bin/obj/node_modules）残留搜索，确认 `FtpWebRequest`/`FtpWebResponse`/`WebRequestMethods.Ftp` 无代码级引用、`FtpContext.CreateRequest` 调用方已全部迁移
- 预期结果：输出残留引用清单（应为空或仅注释），证明重构覆盖完整、无遗漏调用方
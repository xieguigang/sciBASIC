---
name: HttpWebRequest到HttpClientFactory重构
overview: 将 Microsoft.VisualBasic.Core 项目中所有基于 HttpWebRequest/WebRequest 的 HTTP 请求代码重构为基于共享 HttpClientFactory 的 HttpClient 实现，消除对过时 API (SYSLIB0014) 的直接引用。
todos:
  - id: verify-impact
    content: 用 [subagent:code-explorer] 全仓核查 BuildWebRequest/wgetTask/ReportRequest 下游引用及遗留 HttpWebRequest 点
    status: completed
  - id: refactor-httpget
    content: 为 HttpClientFactory 增加带超时 SendSync 重载，重构 HttpGet.BuildWebRequest 返回 HttpRequestMessage 并透传超时
    status: completed
    dependencies:
      - verify-impact
  - id: refactor-wget
    content: 重构 wgetTask/wget 下载流程：事件签名、流式下载、Dns 解析替代 BindIPEndPointDelegate
    status: completed
    dependencies:
      - refactor-httpget
  - id: refactor-multipart
    content: 重构 MultipartForm.POST 为 HttpRequestMessage + ByteArrayContent + HttpClientFactory.SendSync
    status: completed
  - id: build-verify
    content: dotnet build 验证编译，并用 [skill:lsp-code-analysis] 复核无残留 HttpWebRequest 引用
    status: completed
    dependencies:
      - refactor-httpget
      - refactor-wget
      - refactor-multipart
---

## 需求概述

将当前 `Microsoft.VisualBasic.Core` 项目中所有直接使用 `HttpWebRequest` / `WebRequest`（HTTP 用途）的代码重构为基于已存在的 `Net.Http.HttpClientFactory` 共享 `HttpClient` 的引用，消除对已过时 API（SYSLIB0014）的依赖，并修复当前重构过程中 `HttpGet.vb` 的编译断点。

## 核心功能

- 重构 `HttpGet.BuildWebRequest`：由返回 `HttpWebRequest` 改为返回 `HttpRequestMessage`，并同步修复其唯一调用链（`HttpGet.httpRequest` → `UrlGet`）。
- 重构 `wgetTask` / `wget` 下载流程：事件签名、响应读取、远端 IP 展示全部改为基于 `HttpRequestMessage` / `HttpResponseMessage`，保持流式下载与进度事件语义。
- 重构 `MultipartForm.POST`：由 `HttpWebRequest` 上传 multipart 表单改为 `HttpRequestMessage` + `ByteArrayContent` + `HttpClientFactory.SendSync`。
- 为 `HttpClientFactory` 补充带超时参数的 `SendSync` 重载，保留原 `HttpGet` 的每请求超时语义。
- 全仓影响面核查与编译验证，确保无遗留 `HttpWebRequest` 引用且下游不受破坏。
- FTP 相关（`FtpDownloader.vb`、`FtpContext.vb`）因 HttpClient 不支持 FTP 协议，不在本次重构范围。

## 技术栈

- 语言：VB.NET（现有项目语言，不引入新技术）
- 目标框架：net10.0（默认）/ net48 / net5.0 / net6.0 多目标，所用 API 均兼容
- 依赖：`System.Net.Http`（HttpRequestMessage / HttpResponseMessage / ByteArrayContent / HttpCompletionOption / MediaTypeHeaderValue）、`System.Net.Dns`

## 实现方案

以现有 `Net.Http.HttpClientFactory` 共享客户端为唯一入口，逐文件替换剩余的 `HttpWebRequest` 直接引用，保持既有调用签名与行为语义（UA、代理、超时、重试、错误处理）。

### 关键技术决策

1. **`BuildWebRequest` 返回类型改为 `HttpRequestMessage`**：参数签名不变（url/headers/proxy/UA/isPost/timeout），内部改为构建 `HttpRequestMessage` 并设置 Method/Headers，代理通过 `HttpClientFactory.SetProxy` 应用。该改动天然修复第 150 行 `BuildWebRequest(...).UrlGet(echo:=echo)` 的编译断点（`UrlGet` 已接收 `HttpRequestMessage`）。
2. **超时语义保留**：`HttpRequestMessage` 无 Timeout 属性，故为 `HttpClientFactory.SendSync` 增加可选 `timeout As TimeSpan` 参数，内部用 `CancellationTokenSource(timeout)` + `Client.SendAsync(request, token)` 实现；`HttpGet.UrlGet` 增加可选 timeout 参数传递该值，保留原 `HttpRequestTimeOut` 优先级逻辑（>0 时优先）。
3. **wget 流式下载**：使用 `HttpClientFactory.Client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead)` 先取响应头，再 `ReadAsStreamAsync` 逐块写入，保持原有 O(1) 内存占用与下载进度事件；`doDownloadTask` 入参由 `WebResponse` 改为 `Stream`。
4. **远端 IP 展示替代**：原 `BindIPEndPointDelegate` 捕获连接 IP 在 HttpClient 无等价物，改用 `Dns.GetHostAddresses(New Uri(url).Host)` 取首个地址作为展示值（仅用于控制台信息，失败时回退 "NA"，加 Try/Catch 兜底）。
5. **Multipart POST 迁移**：`ByteArrayContent(buffer.ToArray)` + `MediaTypeHeaderValue("multipart/form-data; boundary=...")`；成功/失败分支改按 `response.IsSuccessStatusCode` 分流，失败分支复用同程序集 `WebServiceUtils.readStreamText`；`CredentialCache.DefaultCredentials` 无等价物（共享 Handler 未启用 UseDefaultCredentials），删除并在注释中说明行为差异。
6. **保留 `UA Or DefaultUA` 表达式**：`DefaultUA` 为 `WebServiceUtils.DefaultUA`（`Default(Of String)`），继续沿用现有 Language 空值合并写法。

### 性能与可靠性

- 大文件下载保持流式逐块写盘，无整包缓冲；共享单例 HttpClient 避免 socket 耗尽（与 `WebServiceUtils` 既有迁移模式一致）。
- 所有同步阻塞通过 `.GetAwaiter().GetResult()` 与既有代码风格保持一致，不引入异步改造，控制爆炸半径。
- 公共 API `BuildWebRequest` 仅仓库内两处调用，但 GCModeller 全仓较大（此前全仓 grep 超时），实施前用子代理做全仓影响面核查，实施后用构建验证兜底。

### 实现注意事项

- `wgetTask.ReportRequest` 事件签名由 `(WebRequest, WebResponse, String)` 改为 `(HttpRequestMessage, HttpResponseMessage, String)`，`wget.vb` 处理器同步修改：`ContentLength` → `Content.Headers.ContentLength.GetValueOrDefault(-1)`；`ResponseUri.Host` → `RequestMessage.RequestUri.Host`；`req.Method` → `req.Method.Method`；`ProtocolVersion` → `resp.Version`；`ContentType` → `Content.Headers.ContentType.MediaType`（判空）。
- `Multipart.vb` 与 `wgetTask.vb`、`wget.vb` 需新增 `Imports System.Net.Http`（`Multipart.vb` 另需 `Imports System.Net.Http.Headers`；`WebResponse.vb` 已证明在 `Net.Http` 命名空间内导入 `System.Net.Http` 无类型歧义）。
- 不触碰 `build_log.txt`、`app.config` 中的文本匹配；不修改 FTP 相关文件；避免对工作树中已有未提交改动的无关文件做格式化类改动。

## 架构设计

- 数据流（重构后）：调用方 → `HttpGet.BuildWebRequest` / 直接构建 `HttpRequestMessage` → `HttpClientFactory.SendSync（可选超时）` / `HttpClientFactory.Client.SendAsync` → `HttpResponseMessage` → 文本/流/WebResponseResult。
- 组件关系：`HttpClientFactory`（唯一共享客户端 + 代理 + 超时）为基础设施；`HttpGet`、`WebServiceUtils`、`MultipartForm`、`wgetTask/wget` 均为消费方，不再直接接触 `HttpWebRequest`。

## 目录结构

```
src/
├── Net/HTTP/HttpClientFactory.vb        # [MODIFY] 新增 SendSync(request, timeout) 重载：CancellationTokenSource 实现每请求超时；新增 Imports System.Threading
├── Extensions/WebServices/HttpGet.vb    # [MODIFY] BuildWebRequest 返回 HttpRequestMessage（Method/Headers/代理设置改写）；UrlGet 增加可选 timeout 参数并透传；保留 UA Or DefaultUA、HttpRequestTimeOut 优先级逻辑
├── Net/Wget/wgetTask.vb                 # [MODIFY] 事件签名改 HttpRequestMessage/HttpResponseMessage；doTaskInternal 用 SendAsync(ResponseHeadersRead)+ReadAsStreamAsync 流式下载；Dns.GetHostAddresses 替代 BindIPEndPointDelegate；doDownloadTask 入参改 Stream
├── Net/Wget/wget.vb                     # [MODIFY] ReportRequest 处理器适配新事件签名与 HttpResponseMessage 属性访问
└── Net/HTTP/Multipart.vb                # [MODIFY] POST 改为 HttpRequestMessage + ByteArrayContent + HttpClientFactory.SendSync；按 IsSuccessStatusCode 分流；移除 CredentialCache 并注释说明
```

## 关键代码结构

核心接口契约（新签名）：

```
' HttpClientFactory.vb：带超时的同步发送重载
Public Function SendSync(request As HttpRequestMessage, timeout As TimeSpan) As HttpResponseMessage

' HttpGet.vb：BuildWebRequest 新签名（参数不变，返回类型变更）
Public Function BuildWebRequest(url$,
                                headers As Dictionary(Of String, String),
                                proxy$,
                                UA$,
                                Optional isPost As Boolean = False,
                                Optional timeout As Long = 600) As HttpRequestMessage

' HttpGet.vb：UrlGet 透传超时
<Extension>
Public Function UrlGet(webrequest As HttpRequestMessage,
                       echo As Boolean,
                       Optional timeout As TimeSpan = Nothing) As WebResponseResult

' wgetTask.vb：事件签名变更
Public Event ReportRequest(req As HttpRequestMessage, resp As HttpResponseMessage, remote$)
```

## Agent Extensions

### SubAgent

- **code-explorer**
- 用途：全仓检索 `BuildWebRequest`、`ReportRequest`、`wgetTask`、`MultipartForm.POST` 的下游引用（此前全仓 grep 超时，无法确认仓库外影响面），并定位可能遗漏的 `HttpWebRequest`/`WebRequest` 引用点
- 预期结果：输出完整的调用方清单与影响面结论，确保重构不破坏外部调用

### Skill

- **lsp-code-analysis**
- 用途：重构完成后进行语义级复核，确认无残留 `HttpWebRequest` 引用、新签名调用点（`BuildWebRequest`、`ReportRequest` 事件）全部正确
- 预期结果：符号级验证通过，与 dotnet build 结果互为印证
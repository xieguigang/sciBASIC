---
name: LiteJs-NodeFs-Api-Simulation
overview: 向 LiteJs（VB.NET JavaScript 解释器）项目新增 Node.js 文件系统 API 模拟：实现 fs 模块（同步 + 回调式异步 + fs.promises）、Buffer、path、process 四个内置对象，并编写独立 JS 测试脚本，以真实 Node.js 的运行结果为基准调试对齐模拟实现。
todos:
  - id: probe-entrypoints
    content: 使用 [subagent:code-explorer] 与 [skill:lsp-code-analysis] 定位 DispatchBuiltin/JsGet/GlobalIdentifiers/CodeGenerator 未知全局名解析链路，确定模块接入点与属性表扩展方案
    status: completed
  - id: impl-support-modules
    content: 实现 path、process、Buffer 三个配套模块（含 sep/argv/platform 等属性表与 Buffer 构造/实例方法）
    status: completed
    dependencies:
      - probe-entrypoints
  - id: impl-fs-module
    content: 实现 fs 模块：同步/回调异步/fs.promises + constants，底层封装 System.IO，错误映射为带 code 的 JS Error
    status: completed
    dependencies:
      - impl-support-modules
  - id: register-dispatch
    content: 在 GlobalIdentifiers 注册四模块、DispatchBuiltin 增分派、JsGet 扩展属性表；确保解释器与 CodeGenerator 双路径可用
    status: completed
    dependencies:
      - impl-fs-module
      - impl-support-modules
  - id: build-runner
    content: 新增 LiteJs.Runner 控制台宿主：解析 .js/argv，设置 process.cwd 与 __dirname/__filename，Interpreter.Run 输出到 stdout
    status: completed
    dependencies:
      - register-dispatch
  - id: write-and-align-tests
    content: 编写覆盖各 API 的独立 JS 脚本，用真实 node 生成基准并逐条对齐调试（含错误路径）
    status: completed
    dependencies:
      - build-runner
---

## 用户需求

向 VB.NET 编写的 JavaScript 解释器 `JavaScript/LiteJs.vbproj` 中新增对 Node.js 文件系统相关 API 的模拟，使脚本可在该引擎内像在真实 Node.js 中一样进行文件读写等操作；并编写独立的 JavaScript 测试脚本，以真实 Node.js 的运行结果为基准，调试对齐模拟实现。

## 产品概述

在现有 LiteJs 引擎（词法→AST→解释器/代码生成器，单一语义源 `JsRuntime`）之上，新增四个内置模块 `fs`、`Buffer`、`path`、`process`，让原本只能做纯计算/内置对象运算的脚本具备文件与路径能力，并通过"与 node 同脚本输出逐条 diff"的方式验证保真度。

## 核心特性

- **fs（最完整形态）**：同步方法（readFileSync/writeFileSync/existsSync/readdirSync/mkdirSync/rmdirSync/statSync/unlinkSync/appendFileSync/copyFileSync/renameSync 等）、回调式异步（readFile(path, cb)/writeFile(path, data, cb) 等）、Promise 式（fs.promises.readFile 等，可 `await`），以及 `fs.constants`、默认 utf8 文本 / Buffer 二进制。
- **Buffer**：`Buffer.from/alloc/concat`、实例 `length`、索引访问、`slice/toString(encoding)/equals`，作为二进制读写载体。
- **path**：`join/resolve/basename/dirname/extname/normalize/parse`、`sep`、`delimiter`。
- **process**：`cwd()`、`argv`、`platform`、以及相对路径解析所需的 `__dirname`/`__filename` 上下文。
- **测试形态**：独立 `.js` 脚本 + 真实 `node` 基准输出（`.baseline.txt`），在自研 LiteJs 宿主上运行并逐条对齐（含"文件不存在"等错误路径）。

## 技术栈

- 语言/运行时：VB.NET / .NET 10，仅用 BCL（零第三方依赖），与现有项目一致。
- 引擎结构：Lexer → Parser → AST（`Ast.vb`）→ 两条执行路径共用单一语义源 `JsRuntime`（解释器 `Interpreter.vb` 树遍历 + `CodeGenerator.vb` 生成可编译 VB）。新增模块须同时服务于两条路径。
- 测试：独立 JS 脚本对比，不改动现有 115 项 xunit 测试。

## 实现方法

沿用现有内置对象模式（`console`/`Math` 等均为 `NativeObject` + `JsRuntime.DispatchBuiltin` 分派），新增四个模块并以统一方式接入。关键决策与权衡：

1. **接入点（已核实）**：全局标识符在 `JsRuntime.GlobalIdentifiers()` 注册；成员访问在 `JsGet`（约 499–505 行，`TypeOf obj Is NativeObject` 分支，一律返回 `New BuiltinMethod(obj, name)`）；调用分派在 `DispatchBuiltin`（约 1164 行，`Select Case ...Name`）。`NativeObject` 本身只是带 `Name` 的标记类（见 `Runtime/NativeObject.vb`），不含成员表。

2. **属性 vs 可调用 —— 核心风险与方案**：通用 `NativeObject` 对**任何**成员访问都返回"可调用"的 `BuiltinMethod`，因此 `process.platform`、`process.argv`、`path.sep`、`fs.constants`、`fs.promises` 这类**非调用属性**直接用该模式会拿到函数而非值（与 node 输出不符）。决策：引入一个轻量的 `JsModule`（继承 `NativeObject`，新增 `PropertyTable As Dictionary(Of String, Object)` 与可选 `MethodTable`）。在 `JsGet` 的 `NativeObject` 分支中**优先查 `PropertyTable` 返回真实值**，否则回退为 `BuiltinMethod(obj, name)`；`DispatchBuiltin` 增加 `Case "fs"/"path"/"process"/"Buffer"` 路由到各模块的分派函数。该改动局部、向后兼容（既有 console/Math 行为不变）。

3. **异步与 Promise**：引擎无事件循环、Promise 同步结算、微任务在 `await` 与脚本结束排空（README 已确认）。因此 fs 回调式异步在调用处**同步结算**（直接 `cb(null, data)` 或 `cb(err)`），Promise 式返回引擎已有的 `JsPromise`（已有 `then/catch/finally` 与 `await` 支持），无需线程/定时器。

4. **底层实现**：全部走 `System.IO.File` / `System.IO.Directory` / `System.IO.Path`；文本默认 utf8（`System.Text.Encoding.UTF8`），二进制返回 `Buffer` 封装的 `Byte()`；`stat` 暴露 `size/isFile()/isDirectory()` 等。错误：捕获 `IOException` 等，映射为带 `code`（如 `ENOENT`）、`message` 的 JS `Error`（`JsRuntimeException` 构造），同步抛错、回调以 `(err, data)`、Promise 以 `reject` 形式。

5. **可扩展性与复用**：每个模块一个独立 `.vb` 文件，方法实现集中在模块内分派函数；新增 API 只需在对应 `MethodTable`/`PropertyTable` 与分派 `Select Case` 加分支，无架构级改动。

## 实现注意（防回归）

- **双路径可用**：`CodeGenerator` 对未知全局名经 `ClrInterop.Register`/宿主注册表解析（README）。需确认生成产物中 `fs`/`path`/`process`/`Buffer` 同样走 `GlobalIdentifiers`→`DispatchBuiltin`，必要时将四模块登记进同一注册表；变更后必须用 `ArtifactSyncTests` 验证生成代码与解释器输出一致。
- **路径语义对齐**：相对路径相对 `process.cwd()`；宿主运行 `.js` 时把 `cwd` 设为脚本所在目录、`__dirname` 设为脚本父目录、`__filename` 设为脚本全路径、`argv[0]` 设为 `"node"`（或记录约定）、`argv[1]` 设为脚本路径，以便与 `node file.js` 输出可比。
- **行尾与编码**：Windows 下测试临时文件注意 `\r\n`/`\n` 一致性，基准与模拟两端用同一种换行约定，避免 diff 噪声；读取二进制用 `File.ReadAllBytes`、写入用 `File.WriteAllBytes`。
- **日志/输出**：`console.log` 已写入 `ScriptIO`（如 `LogTextIO`）；宿主把 sink 接到标准输出，确保 `node` 与 LiteJs 两端输出格式一致（对象/数组序列化与现有 `JSON.stringify`/`console` 行为一致）。

## 架构设计

```mermaid
flowchart TD
    A[JS 脚本 .js] --> B[Parser.Parse]
    B --> C[Interpreter.Run / CodeGenerator]
    C --> D[JsRuntime 单一语义源]
    D --> E[GlobalIdentifiers 注册 fs/path/process/Buffer]
    D --> F[DispatchBuiltin Case 分派]
    D --> G[JsGet: 先查 PropertyTable 否则 BuiltinMethod]
    E --> H1[FsModule.vb]
    E --> H2[PathModule.vb]
    E --> H3[ProcessModule.vb]
    E --> H4[BufferModule.vb + BufferInstance]
    H1 -.System.IO.File/Directory.-> I[(磁盘文件/目录)]
    J[LiteJs.Runner 控制台宿主] --> B
    J --> K[设置 cwd/argv/__dirname/__filename]
    node[真实 Node.js 基准] --> A
```

## 目录结构

```
JavaScript/
├── Runtime/
│   ├── JsModule.vb          # [NEW] NativeObject 子类 + PropertyTable/MethodTable，统一的模块对象基类，承载非调用属性与可调用方法
│   ├── BufferModule.vb      # [NEW] Buffer 构造器 + 静态方法(from/alloc/concat) + 实例方法(slice/toString/equals/索引)；底层 Byte()
│   ├── PathModule.vb        # [NEW] path.join/resolve/basename/dirname/extname/normalize/parse + sep/delimiter（包装 System.IO.Path）
│   ├── ProcessModule.vb     # [NEW] process.cwd()/argv/platform + __dirname/__filename 上下文（由宿主注入的 cwd/scriptDir）
│   ├── FsModule.vb          # [NEW] fs 同步/回调/Promise 方法 + fs.constants + fs.promises 子对象（包装 System.IO）
│   └── Runtime.vb           # [MODIFY] GlobalIdentifiers 注册四模块；DispatchBuiltin 增 Case 分派；JsGet 的 NativeObject 分支优先查 PropertyTable
├── LiteJs.Runner/           # [NEW] 极简控制台宿主工程（引用 LiteJs.vbproj）
│   ├── LiteJs.Runner.vbproj
│   └── Program.vb           # 解析 .js 路径与 argv，设置 cwd/__dirname/__filename，Interpreter.Run 并把 ScriptIO 接到 stdout
└── test/js/                 # [NEW] 独立 JS 测试脚本 + .baseline.txt 基准
    ├── fs_text_readwrite.js
    ├── fs_binary_buffer.js
    ├── fs_async_callback.js
    ├── fs_promises_await.js
    ├── fs_dir_stat.js
    ├── fs_errors.js
    ├── path_basic.js
    └── process_basic.js
```

（如 `LiteJs.vbproj` 显式列出源文件，需 [MODIFY] 加入新增的 Runtime 文件；并行工程需加入解决方案或单独 `dotnet build`。）

## 关键代码结构（接口级）

```
' 统一模块基类：可持有真实值属性 + 延迟可调用方法
Namespace Runtime
    Public Class JsModule
        Inherits NativeObject
        Public ReadOnly PropertyTable As New Dictionary(Of String, Object)()
        Public Sub New(name As String, props As Dictionary(Of String, Object))
            MyBase.New(name)
            If props IsNot Nothing Then
                For Each kv In props : PropertyTable(kv.Key) = kv.Value : Next
            End If
        End Sub
    End Class

    ' fs 方法分派签名（同步/异步统一入口，name 决定具体行为）
    Public Module FsModule
        Public Function Dispatch(name As String, args As Object()) As Object
        Public Function DispatchAsync(name As String, args As Object()) As Object  ' 回调/Promise
    End Module
End Namespace
```

## 可用扩展

### SubAgent

- **code-explorer**
- 用途：在实现前深入定位 `DispatchBuiltin`/`JsGet`/`GlobalIdentifiers` 调用链，以及 `CodeGenerator` 对未知全局名的解析路径（`ClrInterop.Register`/宿主注册表），确认 `fs`/`path`/`process`/`Buffer` 在解释器与生成代码两条路径上的接入点。
- 预期结果：给出精确的修改位置清单（行号/方法），避免遗漏 CodeGenerator 路径，降低回归风险。

### Skill

- **lsp-code-analysis**
- 用途：用 LSP 语义导航（`find references`/`go to definition`）核验 `JsRuntime.DispatchBuiltin`、`JsGet`、`JsInvoke` 的调用关系与影响面，确认新增 `Case` 分支与属性表扩展不会破坏既有内置对象。
- 预期结果：明确改动边界与测试用例范围，保证现有 115 项测试不受影响。
---
name: VBS脚本引擎增强-预处理指令与魔法方法
overview: "在 VBS VB.NET 脚本引擎中新增 #package/#author/#title/#version 预处理指令、let 动态类型声明（与 LINQ let 区分）、以及依赖脚本上下文的\"魔法方法\"集合，并在 VBS/test 目录编写 demo 脚本，同步更新 README。"
todos:
  - id: verify-impact
    content: 用 [skill:lsp-code-analysis] 核对 ScriptRefactor 构造调用点与扩展方法签名，用 [subagent:code-explorer] 排查 Script API 外部引用
    status: completed
  - id: metadata-directives
    content: "新增 ScriptMetadata，实现 #package/#author/#title/#version 解析与 assembly 元数据注入"
    status: completed
    dependencies:
      - verify-impact
  - id: let-dynamic
    content: 新增 LetStatement，实现 let 改写为 Dim As Object 并区分 LINQ let
    status: completed
    dependencies:
      - verify-impact
  - id: magic-methods
    content: 新增 Magics，生成脚本上下文、元数据反射与依赖定位魔法方法并接入 BuildCode
    status: completed
    dependencies:
      - metadata-directives
  - id: demo-tests
    content: 在 VBS/test 编写 metadata、let、magics 演示脚本并补齐 test_dynamics.vb
    status: completed
    dependencies:
      - metadata-directives
      - let-dynamic
      - magic-methods
  - id: build-verify
    content: 构建 VBS 并用 vbs 宿主运行全部演示脚本，校验输出正确性
    status: completed
    dependencies:
      - demo-tests
  - id: docs
    content: 更新 VBS/README.md 补充新指令、let 动态类型与魔法方法说明
    status: completed
    dependencies:
      - build-verify
---

## 用户需求

在现有 `VBS\VBS.vbproj`(基于 Roslyn 的 VB.NET 脚本引擎)中扩展预处理能力与脚本体验：

1. 新增 4 个预处理指令：

- `#package "xxx"`：设置动态编译 assembly 的 assembly name；
- `#author "xxx"`：生成 assembly 级 `AssemblyCompanyAttribute`；
- `#title "xxx"`：生成 assembly 级 `AssemblyTitleAttribute`；
- `#version "x.x.x.x"`：生成 assembly 级 `AssemblyVersionAttribute`。

2. 引入动态类型语法：`Dim x = "..."` 仍由 Roslyn 类型推断为强类型；`let y = "..."` 在预处理阶段改写为 `Dim y As Object = "..."`，强制为动态类型；必须能区分 LINQ 查询中的 `let` 子句而不误改写。
3. 新增"真·魔法方法"(只有引擎在预处理阶段注入、依赖脚本上下文，而非普通工具函数)：脚本自身上下文、预处理元数据反射、依赖与路径定位。
4. 完成后在 `VBS\test` 编写脚本做 demo 测试，并同步更新 `VBS\README.md`。

## 产品概述

脚本编写者可在脚本头部用 `#package/#author/#title/#version` 声明程序集元数据；用 `let` 快速声明可动态绑定成员的对象变量；并直接调用 `Here/ScriptDir/ScriptFile/ScriptName/ScriptText/ScriptLines/Self`、`Package/Author/Title/Version/Meta`、`Includes/Locate` 等由引擎烘焙进生成代码的魔法方法，无需任何 import。

## 核心功能

- 指令解析与 assembly 元数据注入(assemblyName + Company/Title/Version 特性)。
- `let` 声明改写为 `Dim … As Object`，并与 LINQ `let` 正确隔离。
- 3 组脚本上下文魔法方法注入到 `VBScriptHostMagics` 模块。
- `VBS/test` 下可运行的验证脚本(元数据、let、magics)与 README 文档更新。

## 技术栈

- 语言/运行时：VB.NET，`net10.0`，`LangVersion=16`；宿主工程 `VBS/VBS.vbproj`(`AssemblyName=vbs`，`RootNamespace=VBScriptHost`，`OutputPath=../../.nuget/`)。
- 编译：`Microsoft.CodeAnalysis.VisualBasic`(Roslyn) 5.9.0，内存编译为 `DynamicallyLinkedLibrary`。
- 复用现有约定：`ScriptRefactor` 的"逐行文本预处理 + 块栈扫描 + 组装"流水线；`TupleDestructuring.Expand` 作为逐行预处理参考实现；sciBASIC 扩展方法 `LineTokens / GetFullPath / ParentPath / FileExists / IsNullOrEmpty / i32`；`App.HOME` 为宿主程序目录。
- 新增文件均为 VB 源码，不新增 NuGet 依赖，不改动 `VBS.vbproj`(test 目录已被 `<Compile Remove="test\**" />` 排除，演示脚本不参与编译)。

## 实现方案

总体策略是在"文本预处理 → 扫描 → 组装 → 编译"链路上做最小侵入式扩展，全部新能力都以"解析期收集上下文 → 烘焙进生成源码"的方式实现，与现有 `Here` 魔法方法生成模式保持一致。

1. **指令解析(元数据)**

- 新增 `ScriptMetadata`(Package/Author/Title/Version)。
- 在 `VBScript.ParseScript` 用正则 `^\s*#(?<key>package|author|title|version)\b\s*=?\s*(?<val>[^\r\n]*)(Multiline|IgnoreCase) 提取，`val `去首尾空白并剥离成对双引号，同时支持 `#package "x"` 与 `#version 1.0.0`。
- 指令行不必额外删除：`ScriptRefactor.HandleTopLevel` 已对 `StartsWith("#")` 的行静默忽略。

2. **assembly 元数据注入**

- `ScriptMetadata.BuildAttributes()` 生成特性文本(值中 `"` 转义为 `""`)：
    - `&lt;Assembly: System.Reflection.AssemblyCompanyAttribute("...")&gt;`
    - `&lt;Assembly: System.Reflection.AssemblyTitleAttribute("...")&gt;`
    - `&lt;Assembly: System.Reflection.AssemblyVersionAttribute("...")&gt;`
- 将 `ScriptMetadata` 透传给 `ScriptRefactor`(构造函数扩参)，在 `BuildCode` 中于 `Imports` 之后、`Namespace DynamicDll` 之前输出(与 VS 生成的 AssemblyInfo.vb 顺序一致)；`#package` 不生成特性，仅作为 assemblyName。
- `ScriptParseResult` 增加 `Metadata` 属性；`DynamicDll.CompileScript` 的 assemblyName 取值优先级改为：显式 `asmName` 参数 &gt; `#package` &gt; 脚本文件名。

3. **let 动态类型预处理**

- 新增 `LetStatement.Expand(source)`，逐行处理并保留前导空白与行尾注释。
- 行首正则 `^(?<lead>\s*)let\s+(?<name>[A-Za-z_]\w*)\s*=\s*(?<expr>.+?)\s* → `${lead}Dim ${name} As Object = ${expr}`。
- 用查询状态机区分 LINQ `let`：维护 `inQuery`；剥离注释后，一行匹配 `\bfrom\s+[A-Za-z_]\w*\s+in\b` 时进入查询态，一行匹配 `^\s*(select|group)\b` 时退出(若进入行同时含终止子句则立即退出)；仅 `inQuery=False` 时执行改写。`For Each x In …` 关键字为 `for`，不会误判。
- 在 `ScriptRefactor.PreprocessText` 中于 `?""...""` 展开之后、`TupleDestructuring.Expand` 之前调用。

4. **魔法方法(预处理期烘焙)**

- 新增 `Magics.Build(scriptFile, metadata, imports)`，返回若干多行 VB 源码字符串，由 `BuildCode` 注入 `Module VBScriptHostMagics`；脚本路径、元数据值、`#include` 绝对路径、搜索目录均在解析期以常量形式写入生成代码(路径用普通字符串字面量，`"` 转义)。
- 脚本上下文：`Here(relpath)`(`Path.Combine/GetFullPath`，相对路径以脚本目录为基准)、`ScriptDir()`、`ScriptFile()`、`ScriptName(Optional withExtension As Boolean = True)`、`ScriptText()`、`ScriptLines()`、`Self()`。
- 元数据反射：`Package()`、`Author()`、`Title()`、`Version()`、`Meta(key)`(大小写不敏感，未知返回 Nothing)。
- 依赖定位：`Includes() As String()` 返回烘焙的 `#include` 绝对路径；`Locate(name)` 按 `#include` 相同搜索顺序 `[脚本目录, App.HOME, App.HOME/libs, App.HOME.ParentPath/libs]` 返回绝对路径或 Nothing。
- 生成代码为 `Option Strict Off`/`Option Infer On`，全部使用 `System.IO`/`System.Reflection` 全限定名；模块内辅助函数以 `__magic` 前缀 + `Private` 降低与脚本命名冲突。

5. **性能与可靠性**

- 所有新增处理均为一次性 O(行数)/O(字符数) 文本扫描，无嵌套正则回溯风险，无额外编译期开销。
- 编译器引用收集、`ScriptLoadContext` 加载与卸载逻辑不改动，blast radius 可控；未涉及的构建/运行行为保持兼容。
- 不修改 `#include`、`?""`、元组分解等既有语义。

## 实现要点(Execution Details)

- 复用 `TupleDestructuring` 的 `IndexOfComment`/`SplitTopLevel` 思路处理 `let`，保证字符串内注释/逗号不被误判。
- assembly 特性必须写在 `Imports` 之后、`Namespace` 之前；特性类型用全限定名，避免依赖脚本 Imports。
- 元数据/路径字符串写入生成代码前统一转义双引号，防止生成代码语法错误。
- `#author` 按要求映射到 `AssemblyCompanyAttribute`(非 AssemblyAuthor)。
- 保持 `CompileScript` 显式 `asmName` 参数的优先级，避免破坏以库方式嵌入的调用方。
- 新增公开成员(`ScriptMetadata`、`ScriptParseResult.Metadata`、`ScriptRefactor` 构造函数)时同步更新 XML 文档注释，与原风格一致。

## 架构设计

四个阶段扩展后仍保持原有分层，新增能力集中在"解析/预处理"阶段与"组装"阶段：

```mermaid
flowchart TD
    A[脚本源码 .vb] --&gt; B[ParseScript 解析]
    B --&gt; C[提取 #include 与 #package/#author/#title/#version]
    C --&gt; D[ScriptRefactor.PreprocessText]
    D --&gt; E[let 展开 LetStatement.Expand]
    E --&gt; F[TupleDestructuring.Expand]
    F --&gt; G[ScanLines / PlaceFunctions]
    G --&gt; H[BuildCode: Imports + Assembly特性 + Magics + Main]
    H --&gt; I[DynamicDll.CompileScript Roslyn 内存编译]
    I --&gt; J[ScriptRuntime 反射执行]
```

## 目录结构

```
VBS/
├── src/
│   ├── DynamicDll.vb                          # [MODIFY] assemblyName 默认值改为 显式参数 > #package > 文件名
│   └── VBScript/
│       ├── VBScript.vb                         # [MODIFY] 解析新指令、构建 Magics、透传 Metadata
│       ├── ScriptMetadata.vb                   # [NEW] 指令元数据模型：Parse/BuildAttributes/Meta
│       ├── ScriptParseResult.vb                # [MODIFY] 新增 Metadata 属性
│       ├── ScriptRefactor.vb                   # [MODIFY] 构造函数收 Metadata；PreprocessText 接 let；BuildCode 注入特性
│       ├── LetStatement.vb                     # [NEW] let→Dim As Object 展开 + LINQ let 查询状态机
│       └── Magics.vb                           # [NEW] 生成 3 组脚本上下文魔法方法源码
├── test/
│   ├── test_metadata.vb                        # [NEW] 验证 #package/#author/#title/#version 的编译与读回
│   ├── test_let.vb                             # [NEW] 验证 let 为 Object 且 LINQ let 未被改写
│   ├── test_magics.vb                          # [NEW] 验证 Here/ScriptDir/ScriptFile/ScriptName/ScriptText/ScriptLines/Self/Includes/Locate
│   └── test_dynamics.vb                        # [MODIFY] 填写动态类型综合演示(当前为空)
└── README.md                                   # [MODIFY] 补充新指令、let、魔法方法说明
```

## 关键代码结构

```
' 脚本元数据指令模型
Public Class ScriptMetadata
    Public Property Package As String   ' #package → assembly name
    Public Property Author As String    ' #author  → AssemblyCompanyAttribute
    Public Property Title As String     ' #title   → AssemblyTitleAttribute
    Public Property Version As String   ' #version → AssemblyVersionAttribute

    Public Shared Function Parse(source As String) As ScriptMetadata
    Public Function BuildAttributes() As String()
    Public Function Meta(key As String) As String
End Class

' 魔法方法源码生成器
Public Module Magics
    Public Function Build(scriptFile As String,
                          metadata As ScriptMetadata,
                          imports As IEnumerable(Of String)) As IEnumerable(Of String)
End Module
```

## Agent Extensions

### Skill

- **lsp-code-analysis**
- Purpose: 在改动前用语义分析确认 `ScriptRefactor` 构造函数的唯一调用点、`ScriptParseResult` 属性引用点，以及 `LineTokens/GetFullPath/ParentPath` 等扩展方法确实可用。
- Expected outcome: 得到确认的调用点/引用清单与符号签名，避免构造函数扩参或属性新增造成编译遗漏。

### SubAgent

- **code-explorer**
- Purpose: 在整个 `src/framework` 范围内排查是否存在 VBS 之外对 `VBScriptHost.Script`(`ParseScript/CompileScript/ScriptParseResult/ScriptRuntime`)的外部调用方，以及仓库内既有 `.vb` 演示脚本的组织约定。
- Expected outcome: 明确 blast radius(是否需兼容外部调用方)并给出与既有约定一致的 demo 组织方式。
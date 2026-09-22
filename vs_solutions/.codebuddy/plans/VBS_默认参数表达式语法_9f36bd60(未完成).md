---
name: VBS 默认参数表达式语法
overview: 在 VBS 脚本引擎的文本预处理阶段新增"默认参数表达式(DefaultParameterExpression)"：把顶层 Function/Sub 中非常数的默认参数表达式改写为 `Optional c As T = Nothing`，并为每个"实参子集"生成桥接函数 `<name>_defaults`，把省略了默认参数的调用点改写为对该桥接函数的调用；无法覆盖的调用点则让对应参数退化为必填以在编译期报错。
todos:
  - id: impl-signature-model
    content: 新增 DefaultParameterSignature 模块：参数项解析、默认值常量/动态分类、类型与修饰符提取、桥接函数源码发射与命名冲突规避
    status: pending
  - id: impl-stage-module
    content: 新增 DefaultParameterExpression 模块：顶层函数声明收集、调用点扫描与仅替换被调用名、覆盖判定与报告汇总
    status: pending
    dependencies:
      - impl-signature-model
  - id: impl-decl-and-bridge
    content: 实现声明改写（动态默认值改为 Nothing；存在无法覆盖调用点时退化为必填）并在脚本末尾追加桥接函数
    status: pending
    dependencies:
      - impl-signature-model
      - impl-stage-module
  - id: wire-pipeline
    content: 在 PreprocessText 中于元组分解之后、向量化之前接入新阶段并补充 --verbose 统计；用 [skill:lsp-code-analysis] 核对 PreprocessText 全部调用点无遗漏
    status: pending
    dependencies:
      - impl-decl-and-bridge
  - id: add-demo
    content: 新增 VBS/test/test_default_expression.vb，覆盖具名实参子集、While/If 条件、嵌套调用、常量默认参数、Sub 与模块级变量引用
    status: pending
    dependencies:
      - wire-pipeline
  - id: update-readme
    content: 更新 VBS/README.md：新增默认参数表达式语法章节（改写规则、保守条件、已知局限）与源码结构表条目
    status: pending
    dependencies:
      - wire-pipeline
  - id: verify-e2e
    content: 构建并端到端验证：dotnet build、vbs 运行 demo（含 --verbose 检查生成代码）与 make-project 转换构建
    status: pending
    dependencies:
      - add-demo
      - update-readme
---

## 产品概述

为 VBS 脚本引擎新增「默认参数表达式」语法：顶层 `Function`/`Sub` 的 `Optional` 参数可以书写**任意可产生值的表达式**作为默认值（例如 `Optional c As data = If(b, New testdata(a), New testdata("default"))`），不再局限于常数与常数表达式。引擎在脚本编译前自动把这种写法改写成等价且可编译的代码，脚本作者无需做任何额外声明。无 UI 变更，全部是脚本语法与生成代码的行为变化。

## 核心功能

1. **非常数默认值表达式**：可选参数的默认值可以是任意表达式，可以引用同一函数中更靠前的参数、模块级变量、`New` 构造与函数调用，例如 `Optional c As data = If(b, New testdata(a), New testdata("default"))`。
2. **声明自动改写**：含非常数默认值的参数被改写为 `Optional c As data = Nothing`，从而满足编译要求；参数类型、修饰符、其余原样保留。
3. **调用点自动改写（桥接函数方案）**：为「省略了至少一个动态默认参数的调用点」按需生成一个顶层桥接函数，调用点只把被调用名替换为桥接函数名、实参表原样保留，因此 `While test(a,b)`、`If test(...) Then`、`print(test(...))` 等任意表达式位置都能被支持，且不改变任何求值顺序。
4. **桥接函数形态**：形参为该调用点实际提供的参数（按声明顺序，沿用原修饰符与类型子句），返回类型沿用原函数；函数体内按声明顺序为每个被省略的参数生成 `Dim 参数名 = 默认值表达式原文`，然后按声明顺序完整调用原函数。例如：
`Function test_defaults(a As String, b As Boolean) As data` / `Dim c = If(b, New testdata(a), New testdata("default"))` / `Return test(a, b, c)`。
5. **作用域等价**：被提供的参数在桥接函数中与原名同名，被省略的参数由同名局部量承载，因此默认值表达式可原样复用且求值作用域与原函数一致，可正确引用模块级变量与其它参数。
6. **未覆盖时编译期报错**：若某函数存在「找到了但无法改写」的调用点，则把该函数的动态默认参数退化为必填（去掉 `Optional` 与默认值），让这些调用点在编译期报「未提供参数」，不会静默取到 `Nothing`；已被改写的调用点不受影响。
7. **最小侵入**：常量默认参数保持原样；提供了全部参数的调用点保持原样；不含非常数默认参数的函数完全不受影响。
8. **命名冲突规避**：桥接函数名在 `函数名_defaults` 基础上检测全文符号冲突并追加数字后缀。

## 技术栈

- 复用现有工程：VB.NET（`net10.0`，`LangVersion 16`），`VBS/VBS.vbproj` 为 SDK 风格、默认通配包含 `src/**`（仅排除 `test/**`），因此**新增源文件无需修改工程文件**（已核对 `VBS.vbproj` 第 41-45 行仅 `<Compile Remove="test\**" />`）。
- 不新增任何第三方依赖；本阶段采用**纯文本级改写**，与既有 `LetStatement` / `TupleDestructuring` / `PropertyProjection` 的实现风格一致（逐行、掩码、配平扫描、不确定即放弃），无需 Roslyn 语义模型。
- 复用既有可访问成员：
- `PropertyProjection.MaskLiterals(line)`（`Friend Shared`，等长掩码，可直接复用偏移）；
- `ScriptStructure.SplitTopLevel(s)`（`Friend Shared`，顶层逗号切分，已处理字符串）、`IsFunctionBlockStart` / `IsTypeBlockStart` / `IsBlockEnd` / `IsNestedBlockStart` / `IsControlBlockStart`（均 `Friend`，用于顶层块判定）。
- 注意 `VectorExpressionRewriter.ExtractParameterList` / `MatchingClose` 是 `Private`，新模块需自带同形状的小工具（或将其提升为 `Friend`，优先选择前者以缩小改动面）。

## 实现方案

**核心策略**：在 `ScriptRefactor.PreprocessText` 的文本预处理链中插入一个新阶段 `DefaultParameterExpression.Expand(source, report)`，分 4 趟完成：收集声明、改写调用点并生成桥接函数、改写声明、追加桥接函数。

**为什么用「桥接函数」而不是在调用点插入临时变量**：临时变量只能插入到语句之间，因此 `While test(a,b)`、`If test(...) Then`、`print(test(...))` 这类位置无法处理。桥接函数把「默认值求值」搬进一个真正存在的函数作用域，调用点退化为一个词元级名字替换，于是任意表达式位置都可支持，且不改变任何求值顺序、不产生临时变量名污染调用点。

**关键决策与取舍**：

1. **只替换被调用名、实参表原样保留**：桥接函数形参名与原函数参数名完全一致，因此具名实参（`a:=...`）与位置实参都无需重排，调用点的求值顺序与语义完全不变。
2. **桥接函数形参 = 该调用点实际提供的参数**（按声明顺序），因此同一函数的不同参数子集会各生成一个桥接函数；以「提供参数名列表（声明顺序）」为 key 去重，避免重复生成。
3. **桥接函数体内用「同名局部量」承载被省略的参数**：`Dim c = <默认值原文>`，于是默认值表达式**完全不需要做名字替换**即可原样复用；被提供的参数仍在桥接函数作用域内且同名。该设计天然满足 VB「默认值只能引用更靠前参数」的约束，被省略参数之间的相互引用按声明顺序声明局部量即可正确解析。
4. **常量默认值的判定取「宁可判为动态」的保守方向**：仅字面量、`True`/`False`/`Nothing`、以及它们之间的一元正负与算术/拼接/括号才判为常量；其余（含十六进制、日期字面量、枚举常量、`1E+10`）一律判为动态。误判为动态不改变行为（仍生成同名局部量承载原默认值文本），因此安全；漏判为常量才危险，故方向取此。
5. **版本兼容优先**：`PreprocessText` 只新增**可选**参数，`ProjectCodeBuilder.BuildIncludedSources` 等既有调用方零改动；不修改 `ScriptParseResult`。
6. **性能**：整体为单趟文本扫描，每行一次掩码（O(行长)），目标函数名匹配 O(目标函数数)，与既有阶段同级；桥接函数数量上界为「各函数的不同调用点子集数」，实际远小于调用点数。无额外 IO、无编译开销、无内存放大（文本级拼接，规模与脚本自身同阶）。

**放弃改写（保守）条件**——任一命中即不改写该函数，其声明保留原默认值表达式（该脚本今天本来也编译不过，因此不存在行为回归）：

- 参数含 `ParamArray`，或方法含泛型类型参数表 `(Of T)`；
- 声明行参数表括号不配平（参数表跨物理行）；
- 某个默认值表达式引用了自身参数名或更靠后的参数名；
- 脚本的类型定义块内部存在同名成员（`Function`/`Sub`），会与顶层函数产生名字遮蔽。

**「找到了但无法改写」的判定**（命中则按澄清结论，把该函数的动态默认参数退化为必填）：

- 无括号的 `Sub` 调用（`test a, b`）、实参表括号配平失败、实参解析失败；
- 重复或未知的具名实参、位置实参出现在具名实参之后；
- 某次调用省略了**没有默认值**的参数。
- 判定方式：先记录被成功改写的名字出现位置，文本中其余「非声明行、非成员访问（前置 `.`）、非 `AddressOf`」的函数名出现一律视为疑似调用点；宁判为未覆盖（代价仅是该函数失去 `= Nothing` 兜底，而已改写调用点仍然正常）。

**已知局限（需写入 README）**：

- 只处理**主脚本顶层**的 `Function`/`Sub`；类型定义块（`Class`/`Module`/`Structure`/`Interface`/`Enum`）内部的方法不处理；
- 常数默认参数不参与改写；
- 默认值表达式本身不能跨物理行续行；
- 运行期路径中顶层函数是 `Main` 内的匿名函数，VB 不允许局部匿名函数递归或互递归；若默认值表达式使顶层函数之间形成相互引用环，运行期路径与既有行为一致地报「未声明」（工程期路径不受影响）——属于既有行为，不新增失败模式。

## 实现说明（执行细节）

- **插入位置**：`ScriptRefactor.PreprocessText` 中 `TupleDestructuring.Expand(code)` 之后、`Vectorization.Expand(...)` 之前（即第 76-78 行之间）。这样桥接函数体内的 `@` 投影与数值向量化也会被后续阶段正确处理，两条发射路径（运行期 `VBScript.ParseScript`、工程期 `ProjectCodeBuilder`）自动保持一致。
- **偏移与回写**：一律基于 `MaskLiterals` 的等长掩码定位，掩码偏移可直接用于原文；同一物理行的多处替换必须**从右向左**回写（与既有 `Render` / `RewriteLine` 的做法一致），保证左侧偏移仍然有效。
- **只改必要部分**：声明改写只替换动态参数的 `= 表达式` 区间（保留 `Optional`、类型子句与常量默认参数）；调用点只替换被调用名那一段字符；不做全行重排、不重写实参表。
- **不动既有行为**：不修改任何既有阶段逻辑与公开签名（仅新增可选参数）；常量默认参数、提供全部参数的调用点、无默认参数的函数全部原样输出；`#include` 引入的脚本不允许有顶层函数与顶层语句，本阶段对其为空操作。
- **生成代码可读性**：追加的桥接函数带生成注释头（原函数签名与改写说明），便于 `--verbose` 与 `make-project` 之后人工阅读与排查。
- **报告与诊断**：沿用现有 `--verbose` 输出块（`VBScript.PrintVectorization` 处），打印改写调用点数、生成的桥接函数名、被退化为必填的参数（形如 `test.c`）、以及放弃改写的函数名；不引入日志框架、不输出脚本内容以外的敏感信息。

## 架构设计

新阶段是既有线性预处理流水线中的一环，产物仍是「可直接被 Roslyn 编译的脚本文本」，不改动扫描/发射两个下游阶段。

```mermaid
flowchart LR
    A[#include 剔除 / 参数展开] --> B[let 展开]
    B --> C[元组分解展开]
    C --> D[默认参数表达式改写 NEW]
    D --> E[向量化 与 投影展开]
    E --> F[ScriptStructure 扫描与代码发射]
```

模块职责划分：

- **DefaultParameterExpression 模块**：阶段入口 `Expand`；顶层函数块的声明收集（自带块深度扫描）；调用点扫描与名字替换；声明改写；报告汇总。
- **DefaultParameterSignature 模块**：签名/参数的文本解析模型（参数项拆分、默认值常量/动态分类、类型子句与修饰符提取）与桥接函数源码发射（形参构造、函数体构造、命名冲突规避）。

## 目录结构

```
VBS/
├── src/
│   └── VBScript/
│       └── Syntax/
│           ├── DefaultParameterExpression.vb   # [NEW] 新阶段入口与改写编排
│           ├── DefaultParameterSignature.vb    # [NEW] 参数/签名解析模型与桥接函数发射
│           ├── ScriptRefactor.vb               # [MODIFY] 在 PreprocessText 中接入新阶段
│           └── Vectorization/
│               └── PropertyProjection.vb       # [只读复用] MaskLiterals 等长掩码
├── src/
│   └── VBScript/
│       └── VBScript.vb                         # [MODIFY] ParseScript 创建并传入报告；verbose 打印统计
├── test/
│   └── test_default_expression.vb              # [NEW] 可运行 demo，覆盖全部场景
└── README.md                                   # [MODIFY] 新增语法章节与源码结构表条目
```

- `VBS/src/VBScript/Syntax/DefaultParameterExpression.vb` [NEW]
- 用途：本特性的唯一入口与编排。定义 `Public Module DefaultParameterExpression` 与报告类 `DefaultParameterReport`。
- 功能：

    1. `Public Function Expand(source As String, Optional report As DefaultParameterReport = Nothing) As String`，按「收集声明 → 改写调用点并生成桥接函数 → 改写声明 → 追加上桥接函数」四趟执行；
    2. 自带轻量顶层块扫描器，仅识别**深度 0** 的 `Function`/`Sub` 块首行，复用 `ScriptStructure` 的 `Friend` 块判定函数；同时记录声明行号与函数块文本（用于递归/覆盖判定）；
    3. 调用点扫描：逐行取 `MaskLiterals` 掩码，用词边界查找目标函数名并排除 `前置 .` / `AddressOf` / 声明行 / `Function|Sub 名` 场景；要求紧随 `(`，用配平扫描取实参表，`SplitTopLevel` 切分实参，判定位置实参与 `name := expr` 具名实参并映射到形参；按「提供参数名列表」取用或创建桥接函数，**仅替换被调用名**（从右向左回写）；
    4. 声明改写：动态参数的 `= 表达式` 区间替换为 `= Nothing`（保留 `Optional`）；若该函数被标记为「存在无法覆盖的调用点」，则同时删除 `Optional` 与默认值使其成为必填参数（注意不要破坏其后跟随的常量 `Optional` 参数的合法性）；
    5. 覆盖判定与报告：记录改写调用点数、生成的桥接函数名、退化为必填的参数、放弃改写的函数名；
    6. 追加桥接函数：在脚本文本末尾以生成注释头分隔后写入全部桥接函数文本。

- 实现要求：全部为保守文本改写；任何不确定都不得生成猜测性代码；同一行多处替换从右向左；掩码偏移直接复用原文；不引入新依赖。

- `VBS/src/VBScript/Syntax/DefaultParameterSignature.vb` [NEW]
- 用途：签名与参数的文本级模型，以及桥接函数源码发射，供上述模块使用。
- 功能：

    1. 解析一条顶层函数声明首行，得到：`Function`/`Sub` 类型、函数名、参数表原文、返回类型子句原文（含 `As Xxx` 有无）、参数项列表；参数表定位需跳过方法泛型表 `(Of T)`，括号配平用与 `VectorExpressionRewriter.ExtractParameterList` 同形状的实现（本模块自带私有副本）；
    2. 解析单个参数项为 `修饰符 / 名称 / 数组标记() / As 类型子句 / 默认值原文`：以**首个顶层 `=`** 作为默认值分隔符（括号内的 `=` 不算），类型子句与默认值之间允许空白；
    3. 默认值常量/动态分类：先 `MaskLiterals` 屏蔽字符串与注释，再把 `True|False|Nothing` 归一化后要求剩余字符仅含数字与运算符/括号；不满足即判为**动态**（保守方向）；
    4. 桥接函数发射：形参文本 = 原参数项去掉 `Optional` 关键字与 `= 默认值` 后的原文（保留 `ByVal`/`ByRef` 与 `As` 子句）；函数体按声明顺序为每个被省略参数生成 `Dim 参数名 = 默认值原文`；随后生成完整调用（`Function` 用 `Return test(所有参数按声明顺序)`，`Sub` 用 `Call test(...)`）；返回类型沿用原函数（无返回类型子句则不加）；
    5. 桥接函数命名：`函数名_defaults`，生成前对整份脚本文本做词边界冲突检查（并排除 `Function|Sub 名` 位置），冲突则追加 `2`、`3` 等数字后缀。

- 实现要求：桥接函数体中 `Dim` 采用类型推断（不写 `As` 子句），与用户示例一致并规避数组/泛型类型子句拼接的复杂性；全部输出为可直接编译的 VB 文本；解析失败必须返回明确的失败信号（由上层执行「放弃/退化必填」策略），不得抛异常中断编译。

- `VBS/src/VBScript/Syntax/ScriptRefactor.vb` [MODIFY]
- 用途：文本预处理唯一入口（运行期与工程期共用）。
- 修改点：`PreprocessText` 增加 `Optional defaultReport As DefaultParameterReport = Nothing` 参数（放在现有参数之后，保持既有具名调用兼容）；在 `TupleDestructuring.Expand(code)` 之后、`Vectorization.Expand(...)` 之前调用 `DefaultParameterExpression.Expand(code, defaultReport)`。不修改其它任何逻辑。

- `VBS/src/VBScript/VBScript.vb` [MODIFY]
- 用途：脚本解析入口与 verbose 输出。
- 修改点：`ParseScript` 中创建 `DefaultParameterReport` 并传入 `PreprocessText`；`PrintVectorization`（第 88-116 行）新增一段打印：改写调用点数、桥接函数名列表、退化为必填的参数列表、放弃改写的函数列表，输出风格与现有 `----- vectorization: ... -----` 保持一致。不修改 `ScriptParseResult`。

- `VBS/test/test_default_expression.vb` [NEW]
- 用途：与 `test_let.vb` / `test_vectorize_basic.vb` 同风格的「带说明注释头的可运行 demo」。
- 功能：定义 `testdata` 类与 `test(a As String, b As Boolean, Optional c As data = If(b, New testdata(a), New testdata("default")))`，用 `print` 打印结果；必须覆盖：动态默认值引用更靠前的参数；提供全部参数（不被改写）；仅提供部分位置参数；具名实参子集（`test(a:="xxxxx", b:=check())`）；调用点出现在 `While` 条件、`If ... Then` 条件与嵌套表达式 `print(test(...))` 中；常量默认参数保持原样；`Sub` 的默认参数；默认值表达式引用模块级变量；`test(a:=x, c:=y)` 这类跳过中间可选参数的子集。
- 实现要求：注释头写明运行命令（`vbs ./test/test_default_expression.vb`、`--verbose`、`make-project`），并在注释中给出改写前后的对照，便于人工核对。

- `VBS/README.md` [MODIFY]
- 用途：项目文档（现有「脚本语法」章节 1-13 节每个语法特性都有专节）。
- 修改点：新增第 14 节「默认参数表达式」：语法说明、改写前后的对照示例（声明改 `= Nothing`、生成桥接函数、调用点仅替换名字）、常量默认参数与「提供全部参数」不变、未覆盖调用点退化为必填的报错行为、保守放弃条件与已知局限；并在「项目源码结构」表格中补上两个新文件与 `VBScript.vb` / `ScriptRefactor.vb` 的职责描述更新。

## 关键代码结构

```
Namespace Script

    ''' 默认参数表达式改写的统计与诊断信息(供 --verbose 输出)。
    Public Class DefaultParameterReport
        ''' 被桥接改写的调用点数量
        Public Property Rewritten As Integer
        ''' 含非常数默认值表达式并被改写的函数名
        Public ReadOnly Property Functions As New List(Of String)
        ''' 生成的桥接函数名
        Public ReadOnly Property Bridges As New List(Of String)
        ''' 因存在无法覆盖的调用点而被退化为必填的参数(形如 "test.c")
        Public ReadOnly Property Required As New List(Of String)
        ''' 因保守条件而放弃改写的函数名
        Public ReadOnly Property Skipped As New List(Of String)
    End Class

    Public Module DefaultParameterExpression
        ''' 把「非常数默认参数表达式」改写为可编译形态: 声明改 = Nothing,
        ''' 调用点改名为按需生成的桥接函数, 桥接函数追加到脚本末尾。
        Public Function Expand(source As String,
                               Optional report As DefaultParameterReport = Nothing) As String
    End Module

End Namespace
```

生成的桥接函数形态（实现的唯一输出契约，供运行期与工程期两条发射路径消费）：

```
' ---------------------------------------------------------------
' 由脚本引擎生成: 默认参数表达式桥接函数
' 原函数: Function test(a As String, b As Boolean, Optional c As data = ...)
' ---------------------------------------------------------------
Function test_defaults(a As String, b As Boolean) As data
    Dim c = If(b, New testdata(a), New testdata("default"))
    Return test(a, b, c)
End Function
```

## Agent Extensions

### Skill

- **lsp-code-analysis**
- Purpose: 在改动 `ScriptRefactor.PreprocessText` 的签名（新增可选参数）与新增预处理阶段时，用语义级引用分析核对 `PreprocessText`、`Vectorization.Expand`、`PreprocessText` 报告参数的**全部调用点**，确认没有遗漏的调用方（例如 `ProjectCodeBuilder.BuildIncludedSources` 之外的调用路径），避免因签名或阶段顺序变更导致的静默不一致。
- Expected outcome: 得到一份 `PreprocessText` 的调用点清单及其传参方式，据此确认「仅新增可选参数、既有调用方零改动」成立，并确认新阶段的位置不会破坏任何现有调用路径；若发现遗漏的调用点则补入目录结构与任务范围。
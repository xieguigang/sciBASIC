---
name: fix-vbs-top-level-func-order
overview: 修复 VBS 预处理中顶层函数被统一提前到 Main 开头、导致捕获后声明变量时报 BC32000 的问题：改为按依赖位置放置函数块，再用 --verbose 校验 hola_layout.vb，并回归 tutorials\VBS 下全部 6 个脚本。
todos:
  - id: refactor-slot-model
    content: 改造 RefactorScript 的顶层内容收集为槽位模型：语句槽位记录声明名，函数块记录名称、文本与引用标识符
    status: pending
  - id: implement-placement
    content: 实现函数落位算法：slot 取依赖变量槽位与被调函数槽位的最大值，并保持函数间源码相对顺序
    status: pending
    dependencies:
      - refactor-slot-model
  - id: rewrite-assembly
    content: 改造组装阶段，按槽位顺序输出语句与函数块，取代原先函数一律提前的逻辑
    status: pending
    dependencies:
      - implement-placement
  - id: build-verify-hola
    content: 编译 VBS 并用 --verbose 校验 hola_layout.vb 生成代码中 lambda 位于 Dim 之后且脚本跑通
    status: pending
    dependencies:
      - rewrite-assembly
  - id: regress-all-scripts
    content: 回归运行 tutorials\VBS 全部脚本：cuda、kmeans、mnist_umap、tuple、word2vector
    status: pending
    dependencies:
      - build-verify-hola
---

## 用户需求

修复 VBS 脚本引擎预处理阶段的**顶层函数代码顺序**缺陷，并回归验证全部教程脚本。

### 问题现象

`tutorials\VBS\hola_layout.vb` 中源码顺序为：

```
Dim g As New NetworkGraph
Dim rnd As New Random(12345)

public function rndPos() as FDGVector2 
    return New FDGVector2(rnd.NextDouble() * 1000.0, rnd.NextDouble() * 1000.0)
End function

public sub addNode(label As String)
    Call g.AddNode(...)
End Sub
```

预处理后生成的 `Main` 里两个 lambda 被排到了 `Dim g` / `Dim rnd` **之前**，编译报：

```
error BC32000: Local variable 'rnd' cannot be referred to before it is declared.
error BC32000: Local variable 'g' cannot be referred to before it is declared.
```

### 交付要求

1. 完善 `vs_solutions\VBS\src\VBScript\VBScript.vb:74` 的 `RefactorScript` 代码顺序逻辑；
2. 编译 `vs_solutions\VBS\VBS.vbproj`，用 `"G:\GCModeller\src\runtime\sciBASIC#\.nuget\net10.0\vbs.exe" G:\GCModeller\src\runtime\sciBASIC#\tutorials\VBS\hola_layout.vb --verbose` 检查生成代码是否正确；
3. 把 `tutorials\VBS` 中**所有脚本**各跑一遍，确保预处理函数无问题。

### 功能内容

- 顶层函数重写为匿名函数后，在 `Main` 中的落位需满足 VB 的"局部变量先声明后使用"约束：不能早于它捕获的顶层变量、也不能早于它调用的其它顶层函数。
- 不依赖任何顶层变量/函数的纯函数仍应被提前到 `Main` 开头，保留原有"先定义后调用"的脚本书写习惯。
- 其余顶层语句（`Dim`、控制流块、可执行语句）保持源码相对顺序不变。

## 技术栈

- VB.NET / net10.0，SDK 风格工程 `vs_solutions\VBS\VBS.vbproj`（`RootNamespace VBScriptHost`，`LangVersion 16`）
- Roslyn `Microsoft.CodeAnalysis.VisualBasic` 内存编译（`src\DynamicDll.vb`）
- 正则 + 栈扫描的预处理（`src\VBScript\VBScript.vb`）
- 构建产物输出目录 `..\..\..\.nuget\` → `.nuget\net10.0\vbs.exe`

## 根因

`RefactorScript` 组装阶段（VBScript.vb 198-208 行）**先把全部 `funcBlocks` 输出进 `Main`，再输出 `mainBody`**，注释写着"顶层函数(匿名函数形式)必须先于顶层语句声明"。

顶层函数被 `ToLambdaSignature` 重写为 `Dim f = Function()...` / `Dim f = Sub(...)...` 后就是 `Main` 里的**局部变量**，VB 要求先声明后使用，且 lambda 捕获的同样是 `Main` 的局部变量。因此"一律提前"仅在函数既不捕获顶层变量、也不调用其它顶层函数时成立。

## 实现方案

核心思路：**顶层函数不再统一提前，而是放到"它依赖的东西都已声明"的最早位置**。

### 1. 槽位建模

把 `Main` 的内容建模成按源码顺序排列的**槽位**列表：

- 顶层语句（单行语句、`For`/`Using`/`If`/`Try` 等语句块）依次成为一个槽位；
- 每个槽位记录它**声明的顶层变量名**（`Dim` / `Const`，含 `Dim rows = 5, cols = 4` 这类逗号分隔的多个名字）；
- 顶层函数块单独记录：函数名、块文本、引用的标识符集合。

### 2. 落位算法

对每个函数（按源码顺序处理）计算槽位：

```
slot = max( 引用的顶层变量所在的槽位, 调用的其它顶层函数的槽位 )
slot = max( slot, 上一个函数的 slot )        ' 保持函数之间的源码相对顺序
```

无依赖时为 `-1`，即放在所有语句之前 —— 等价于原先的"提前声明"。

### 3. 组装输出

按槽位顺序输出：每个槽位先输出它的语句，再输出挂在该槽位上的函数块（块后补一个空行）。

### 效果

- `hola_layout.vb`：`rndPos` 依赖 `rnd`（槽 1）→ 落在 `Dim rnd` 之后；`addNode` 依赖 `g`（槽 0）与 `rndPos`（槽 1）→ 同样落在 `Dim rnd` 之后，与源码顺序一致，BC32000 消失。
- 纯函数（不捕获顶层变量、不调用其它顶层函数）仍落到槽 `-1` 被提前到 `Main` 开头，保留原有能力，不产生回归。

## 执行细节

- 声明名提取要覆盖 `Dim a, b As Integer`、`Dim grid(rows - 1, cols - 1) As String`、`Const` 等形式；**宁可多识别**（只是不提前，等价源码顺序），不可少识别（会导致 BC32000 复发）。
- 函数→函数依赖必须计入（`addNode` 调 `rndPos`）。
- 必须保留既有修复，不得回退：
- `IsBlockEnd` 的 `blockType.ToLower()` 大小写无关判定（`For`/`Do` 块闭合）；
- 扫描结束兜底 flush 残留 `buffer`；
- `TupleDestructuring` 计数器显式递增（`i += 1`）。
- 类型定义块（`typeBlocks`）仍作为 `Module` 的嵌套类型输出在 `Main` 之后，逻辑不变。
- 复杂度：预处理仍是一次 O(n) 扫描 + 一次按槽位 O(n) 输出；依赖扫描用正则词边界匹配，脚本规模（几十~几百行）下无性能压力。
- 函数捕获了源码中更后面才声明的变量时，`slot` 取 `max` 会把函数往后移；若该变量的声明位于调用点之后，属脚本自身写法问题，VB 语义下无解，保持编译报错即可（报错信息与 VB 原生一致）。

## 目录结构

```
vs_solutions/VBS/src/VBScript/
└── VBScript.vb   # [MODIFY] RefactorScript(74行起)
                  #   - 新增槽位模型: 顶层语句槽位 + 声明名 + 顶层函数块(名/文本/引用)
                  #   - 新增 SlotItem / FuncItem 私有类型(或并行 List)承载上述结构
                  #   - 新增依赖解析: 提取 Dim/Const 声明名、函数体引用标识符
                  #   - 新增落位算法: slot = max(依赖槽位, 上一函数槽位)
                  #   - 改造组装段(198-208行): 按槽位顺序输出语句与函数块, 取代"函数一律提前"

tutorials/VBS/    # [验证] hola_layout.vb(--verbose 校验) + cuda/kmeans/mnist_umap/tuple/word2vector 回归
```

## 验证

- `dotnet build vs_solutions/VBS/VBS.vbproj -c Release`
- `cd .nuget\net10.0; .\vbs.exe <abs>\tutorials\VBS\hola_layout.vb --verbose`
- 生成代码里 `Dim rndPos = Function() ...` / `Dim addNode = Sub(...) ...` 必须位于 `Dim g As New NetworkGraph`、`Dim rnd As New Random(12345)` **之后**
- 脚本正常跑完并生成 `Z:\HOLA_complex_layout.png`
- 依次运行 `cuda.vb`、`kmeans.vb`、`mnist_umap.vb`、`tuple.vb`、`word2vector.vb`，要求无 BC32000、无预处理丢代码、无编译诊断

## 风险与兜底

| 风险 | 兜底 |
| --- | --- |
| 标识符/声明名扫描漏判，导致函数被提得过前 | 声明名提取覆盖 `Dim`/`Const` 的逗号分隔多名字与数组声明；多识别只是不提前（等价源码顺序），安全 |
| 函数互相调用 | 函数→函数依赖计入 slot，并按源码顺序取 max 保证相对次序 |
| 函数捕获更后面才声明的变量 | slot 取 max 会把函数后移；若变量声明在调用点之后属脚本写法问题，保持 VB 原生报错 |
| 破坏已修复的 `For`/`Do` 闭合与兜底 flush | 改动只涉及"顺序"与"落位"，保留 `IsBlockEnd` 大小写修复与结束 flush，并用全部 6 个脚本回归 |
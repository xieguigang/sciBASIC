---
name: LPA标签传播社区发现算法实现
overview: 在 Analysis\Community\LPA 文件夹中，参照 Louvain 模块（Builder.Load 泛型工厂 + 链式前向星邻接表 + SolveClusters/GetCommunity API）实现标签传播算法（LPA）的网络社区划分功能，并在 test 项目中添加演示验证。
todos:
  - id: lpa-core
    content: 创建 LPA\Edge.vb 与 LPA\LabelPropagation.vb，实现随机序列异步加权标签传播、平局规则与收敛检测
    status: completed
  - id: lpa-builder
    content: 创建 LPA\Builder.vb 泛型 Load 工厂，将 NetworkGraph 转为链式前向星邻接表并初始化标签
    status: completed
    dependencies:
      - lpa-core
  - id: lpa-test
    content: 新增 test\LPATest.vb 双团测试演示，接入 Module1.vb 的 Main 调用
    status: completed
    dependencies:
      - lpa-builder
  - id: lpa-verify
    content: 编译 graph 项目并运行 test 验证 LPA 社区划分正确性
    status: completed
    dependencies:
      - lpa-test
---

## 用户需求

基于当前网络图项目中的数据结构（`NetworkGraph(Of Node, Edge)`），在 `Analysis\Community\LPA` 文件夹中实现基于标签传播算法（LPA）的网络图社区划分功能。LPA 迭代逻辑：每个节点把自己的标签改成邻居中出现最多的标签，收敛后标签即社区。要求参照 `Analysis\Community\Louvain\Builder.vb` 模块的代码编写模式编写实现 LPA 算法的 API 函数。

## 产品概述

为 Microsoft.VisualBasic.Data.GraphTheory 图算法库新增 LPA 社区发现模块，与现有 Louvain/FastUnfolding 社区发现算法并列，供外部程序调用进行网络图社区划分。

## 核心功能

- 泛型 `Load` 工厂函数：从 `NetworkGraph` 构建内部邻接表（支持加权/无权图，无权边默认权重 1.0）
- 核心迭代：随机节点顺序的异步加权标签传播，邻居中加权票数最多的标签胜出；平局时保持当前标签防止振荡
- 收敛检测：一轮完整遍历无标签变化即收敛，提供最大迭代次数上限保护
- 结果查询 API：`GetCommunity()`（按节点顺序的标签数组）、`GetClusterCount()`（社区数量）、`GetClusters()`（社区→成员节点 label 映射）
- 边界处理：孤立节点保持自身标签、跳过自环、空图安全返回；控制台进度输出

## 技术栈

- 语言：VB.NET（net10.0，SDK 风格项目 `graph-netcore5.vbproj`，新 `.vb` 文件自动参与编译，**无需修改项目文件**）
- 命名空间：`Microsoft.VisualBasic.Data.GraphTheory.Analysis.LPA`（对齐 Louvain 的 `Analysis.Louvain` 模式：文件夹含 Community 层级但命名空间不含）
- 数据结构：链式前向星邻接表（`Friend Class Edge`：v/weight/next + `head()` 数组 + `top` 指针），与 Louvain 模块完全一致
- 依赖：Microsoft.VisualBasic.Core（`randf.seeds` 随机数、`VBDebugger.Echo/EchoLine` 进度输出，均无需额外 import）

## 实现方案

### 模块结构（三文件，完全对齐 Louvain 模块布局）

1. `Edge.vb` — 链式前向星边结构（v/weight/next），仿 `Louvain\Edge.vb`
2. `LabelPropagation.vb` — 算法主体与结果 API（对应 `LouvainCommunity`）
3. `Builder.vb` — 泛型 `Load` 工厂（对应 Louvain `Builder.Load`），将 `NetworkGraph` 转为邻接表并初始化标签

### 算法设计（加权异步 LPA，Raghavan et al. 2007）

- 初始化：`label(i) = i`（每个节点一个独立标签，天然保证孤立节点/空图合法输出）
- 每轮迭代：Fisher-Yates 洗牌生成随机节点序列（复用 Louvain 的 `randf.seeds.Next(n)` 模式）
- 单节点更新：沿邻接表统计邻居标签加权票数（跳过自环），取最大者；平局集合中含当前标签则保持，否则随机选一（防振荡）
- 收敛：一轮无标签变化即退出；`maxIterations` 上限（默认 100，LPA 通常 5~10 轮收敛）

### 关键技术决策

- **性能优化**：采用"共享计数器数组 + touched 列表重置"方案，单节点更新 O(deg(i))，避免 Louvain `TryMoveNode` 每节点分配 O(n) 数组的问题；单轮复杂度 O(n+m)
- **异步更新**（就地改标签）比同步更新收敛更快，且与 Louvain 的就地 `TryMoveNode` 行为一致
- **ID 约定**：节点 ID 直接作数组下标（0-based），与 Louvain `loadGraphMatrix` 的 `link.U.ID` 用法完全一致（`NetworkGraph(nodes, edges)` 构造器保证 0-based）
- **有向图处理**：按无向投影双向插边（与 Louvain 一致，社区检测标准做法）

### 实现要点

- 无权检测：`g.graphEdges.Any(Function(l) l.weight <> 0.0)`，无权则边权 1.0（对齐 Louvain）
- `Builder.Load` 时缓存 `nodeLabels As String()`（按节点 ID 下标存 `Vertex.label`），供 `GetClusters()` 输出成员节点名
- 边数组大小 `m = g.size.edges * 2`（双向），与 Louvain 相同
- 字段可见性：邻接表字段设为 `Friend`，由 `Builder.Load` 填充（对齐 Louvain 的 Builder/算法类协作模式）
- 进度输出：每轮一行 `VBDebugger.EchoLine($" [LPA loop_{count}] ...")`，避免刷屏
- 文件头沿用 GPL3 `#Region "Microsoft.VisualBasic::..."` 注释块 + 中文注释（对齐 DinicMaxFlow/Louvain 现有风格）
- **爆炸半径控制**：不改动任何现有库文件；test 项目已被主项目 `<Compile Remove="test\**" />` 排除，测试改动零风险

## 架构设计

数据流简单清晰，无需图示：

```
NetworkGraph(Of Node, Edge)
  → Builder.Load(g, maxIterations)      ' 转链式前向星邻接表 + label(i)=i 初始化
  → LabelPropagation.SolveClusters()    ' 随机序列异步加权传播至收敛，返回 Me（链式调用）
  → GetCommunity() / GetClusterCount() / GetClusters()  ' 结果查询
```

## 目录结构

```
g:\GCModeller\src\runtime\sciBASIC#\Data_science\Graph\
├── Analysis\Community\LPA\
│   ├── Edge.vb                # [NEW] 链式前向星边结构：Friend Class Edge（v/weight/next），Namespace Analysis.LPA，仿 Louvain\Edge.vb
│   ├── LabelPropagation.vb    # [NEW] LPA 算法主体：Friend 字段（n/m/label/edge/head/top/nodeLabels/maxIterations）；SolveClusters() 实现洗牌序列+加权投票+平局规则+收敛检测；GetCommunity()/GetClusterCount()/GetClusters() 结果 API
│   └── Builder.vb             # [NEW] 泛型工厂：Shared Load(Of Node, Edge)(g, Optional maxIterations) 构建 LabelPropagation，遍历 graphEdges 双向插边（hasWeight 检测、自环处理），仿 Louvain\Builder.vb
└── test\
    ├── LPATest.vb             # [NEW] LPA 演示测试：构造"双团+桥"测试图（两个 5 节点团 + 1 条桥边），断言划分出 2 个社区且团员归属正确
    └── Module1.vb             # [MODIFY] Main 中追加调用 LPA 测试（保留现有 testFlow 调用）
```

## 关键代码结构

```
' Namespace Analysis.LPA —— 公共 API 契约（对齐 LouvainCommunity/Builder 模式）
Namespace Analysis.LPA
    Public Class LabelPropagation
        Public Function SolveClusters() As LabelPropagation
        ' 迭代至收敛，返回 Me 支持链式调用（同 Louvain SolveClusters）

        Public Function GetCommunity() As String()
        ' 按节点 ID 顺序的社区标签字符串数组（同 Louvain GetCommunity）

        Public Function GetClusterCount() As Integer

        Public Function GetClusters() As Dictionary(Of String, String())
        ' 社区标签 → 该社区成员节点的 Vertex.label 数组
    End Class

    Public Class Builder
        Public Shared Function Load(Of Node As {New, Network.Node},
                                        Edge As {New, Network.Edge(Of Node)})(
            g As NetworkGraph(Of Node, Edge),
            Optional maxIterations As Integer = 100) As LabelPropagation
    End Class
End Namespace
```
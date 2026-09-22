---
name: hierarchical-clustering-performance-optimization
overview: 优化 hierarchical-clustering（hctree.NET5.vbproj）的层次聚类算法：修复 DistanceMap 导致的 O(n⁴) 瓶颈并改造为索引堆结构，同时基于项目现有 BIRCH 实现新增「特征表 → BIRCH 预聚类 → 凝聚层次聚类」的近似加速通道，使 >20000 样本可高效聚类，并在 HCTNumericTableTests 中补充基准与正确性回归测试。
todos:
  - id: heap-distance-map
    content: 将 DistanceMap 改为二叉最小堆+惰性删除，Remove 降为 O(1)，构造一次性 Heapify，保持公开 API 与 MinimalDistance 语义
    status: completed
  - id: streamline-agglomerate
    content: 精简 HierarchyBuilder.Agglomerate：去 PLINQ/临时集合/每轮 Sort，顺序原地更新；createLinkages 统一批量构建
    status: completed
    dependencies:
      - heap-distance-map
  - id: cluster-hash-optimize
    content: 用 [subagent:code-explorer] 与 [skill:lsp-code-analysis] 核对调用点后，实现 Leafs 缓存、LeafNames 惰性化与整数 Id 哈希键
    status: completed
    dependencies:
      - heap-distance-map
  - id: birch-approx-api
    content: 新增 BIRCH 预聚类通道（BIRCH/CFEntry 质心、外置自动重建/去 GC.Collect、hcaApprox/hcutApprox 及标签回填）
    status: completed
    dependencies:
      - streamline-agglomerate
      - cluster-hash-optimize
  - id: tests-benchmark
    content: 在 HCTNumericTableTests 保留 N=40 断言并新增近似与精确通道一致性校验，以及 n>=20000 计时基准
    status: completed
    dependencies:
      - birch-approx-api
  - id: build-verify
    content: 构建并运行 dotnet run 测试与基准，核对耗时、子簇数与簇划分正确性，输出前后对比结论
    status: completed
    dependencies:
      - tests-benchmark
---

## 产品概述

针对 `hierarchical-clustering/hctree.NET5.vbproj`（VB.NET / net10.0）的层次聚类引擎做性能优化，使其能够处理 **2 万样本以上**的大数据集。当前实现的凝聚过程存在 O(n⁴) 级瓶颈，大数据集下不可用；用户接受以**近似结果**换取大幅加速，并要求补充性能基准与正确性回归测试。

## 核心功能

- **修复凝聚聚类核心的复杂度瓶颈**：消除链接表的线性查找删除与反复全量排序，保持 `ClusteringAlgorithm` / `Cluster` / `LinkageStrategy` / `DistanceMap` 等公开 API 与聚类语义不变，仅在数据结构与常数层面提速。
- **降低单次合并开销**：去掉每轮并行查询与临时集合分配，合并时直接原地更新链接表，并降低链接哈希键（字符串名哈希）的计算成本。
- **减少聚类树的内存与重复计算**：缓存的簇叶数、按需计算的叶名集合，消除每次合并 O(n) 的叶子名拷贝与递归重算。
- **新增大规模近似通道**：输入特征表时，先用 BIRCH CF-tree 预聚类把 n 个样本压缩为 m 个子簇（m 可配置），再对子簇质心执行（优化后的）凝聚层次聚类，最后把簇号回填到原始样本；全程**不构造 n×n 距离矩阵**。
- **加固 BIRCH 实现**：关闭默认自动重建、移除根分裂时的阻塞式全量 GC，避免预聚类阶段自身成为新的性能瓶颈。
- **测试与基准**：保留现有 N=40 冒烟断言，新增近似通道与精确通道在可分数据上的一致性校验，以及 n≥20000 合成数据的计时基准。

## 视觉/输出效果

无 UI 变更；输出仍为 `Cluster` 树与写回 `cluster` 标签列的 `NumericTable`，仅在耗时与可处理规模上有数量级改善。

## 技术栈

- 语言/框架：VB.NET，`TargetFrameworks=net10.0`（沿用现有 `netcore5=1` 等 DefineConstants 与配置）
- 工程：`hierarchical-clustering/hctree.NET5.vbproj`（保持 `GeneratePackageOnBuild`、多 Configuration 不变）
- 依赖：继续复用 `Microsoft.VisualBasic.Core`、`Math.NET5`、`DataMining.NET5`，**不引入新的 NuGet 包**
- 测试：`HCTNumericTableTests/HCTNumericTableTests.vbproj`（Console Exe，`dotnet run`）

## 实现方案

### 1) DistanceMap：排序列表 → 索引二叉最小堆 + 惰性删除

现状 `data: List(Of HierarchyLink)` 每次插入/每轮合并都 `Sort()`，`Remove` 走 `List.Remove` 线性查找，累计约 O(n⁴)。改为：

- `linkTable: Dictionary(Of ULong, HierarchyLink)` 保留 O(1) 按键查找（`FindByCodePair` 不变）；
- 新增数组式二叉最小堆 `heap: List(Of HierarchyLink)`，按 `LinkageDistance` 维护堆序；
- `Add`：入字典 + `Push`（O(log m)），`direct` 参数保留但退化为无操作；
- `Remove(link)`：仅置 `removed=True` 并从字典移除，**O(1)，不再触碰堆**；
- `RemoveFirst` / `MinimalDistance`：出堆/窥顶时跳过 `removed` 项（堆顶惰性清理），保持 `flatAgg` 依赖的「窥顶即当前最小距离」语义；
- 构造：一次性建堆（`Heapify`，O(m)），替代 `Sort()`；
- 公开签名（`MinimalDistance / Add / Remove / RemoveFirst / FindByCodePair / Sort / ToList / ToString`）全部保持不变，`Sort()` 保留为兼容空实现。

复杂度：单次合并的删除由 O(c³) 降为 O(c)，整体由 **O(n⁴) → O(n² log n)** 量级。

### 2) Agglomerate 精简与哈希键优化

- `HierarchyBuilder.Agglomerate`：去掉每轮 `Clusters.AsParallel(...).Select(...).ToArray()`，改为单次顺序循环，对每个存活簇直接「移除两条旧链接 + 推入一条新链接」，删除每轮的 `Distances.Sort()`；不再为每个簇分配 `List(Of HierarchyTreeNode)`；`evaluateDistance` 内联。
- `DefaultClusteringAlgorithm.createLinkages`：删除 `<100` 分支的逐条 `Add`（改为批量构建后一次性 `Heapify`），n≥100 分支沿用批量构建；`PDistClusteringAlgorithm.createLinkages` 同步改为批量构建。
- `HierarchyLink.hashCodePair`：为 `Cluster` 增加内部单调递增整型 `Id`，链接键改用 `HashMap.HashCodePair(id1, id2)`，消除每次查找的 `Name.GetHashCode` 与 `Name.CompareTo` 字符串开销（`Name` 及 `Equals/GetHashCode` 的对外行为不变）。

### 3) Cluster：缓存叶数、惰性叶名

- `Cluster.Leafs` 由「每次递归重算」改为增量缓存字段（合并 `Agglomerate` 时 `leafCount = Left.Leafs + Right.Leafs`，叶子为 1）；`CountLeafs` 不再重复调用 `child.Leafs()`；
- `LeafNames` 移除 `HierarchyTreeNode.Agglomerate` 中的 eager `AppendLeafNames` 累积（消除 O(n²) 拷贝/内存），改为按需从 `Children` 递归计算（保留 `AddLeafName/AppendLeafNames` 公开方法以兼容）；
- `build path` 已确认 `HCTreePlot/*` 与 `NumericTableExtensions.collectLeafs/OrderLeafs/cutTree` 只依赖 `Children`/`OrderLeafs`/`Leafs`，改造前用引用检索确认无其他 `LeafNames` 依赖。

### 4) BIRCH 近似通道（面向 n>20000）

新增两级流水线，复用既有 `BIRCH/CFTree.insertEntry(x, index)` 与 `SubclusterMembers`：

```mermaid
graph LR
    A[特征表 NumericTable] -->|逐行 insertEntry x,index| B[BIRCH CF-tree]
    B -->|SubclusterMembers IndexList| C[m 个子簇质心]
    C -->|Euclidean 距离矩阵 m x m| D[优化后的凝聚 HAC]
    D -->|Cluster 树| E[按 k/阈值切分]
    E -->|子簇成员回填| F[原始样本 cluster 标签]
```

- 新增 `CFEntry` 公开只读 `Centroid`（由 `sumX/n` 计算，不复制大数组；或经 `IndexList` + 源特征表计算，二选一由实现定），供第二级距离计算；
- 新增预聚类参数对象（如 `maxNodeEntries`（B）、`threshold`（T）、`distFunction`、`targetSubclusters`）与扩展方法 `hcaApprox(...)` / `hcutApprox(...)`：先 BIRCH 压缩至 m，再调用优化后的 `performClustering` / `performFlatClustering`，最后 `writeClusterLabels` 按成员回填；
- 现有 `hca()/hcut()` 的「输入必须是方阵距离矩阵」契约与签名**保持不变**，近似通道使用独立方法名，避免重载歧义；
- BIRCH 加固：该通道默认 `AutomaticRebuild(False)`；`CTree.splitRoot()` 去除 `GC.Collect()`；`rebuildIfAboveMemLimit` 不再被隐式触发。m 目标区间建议 1000~5000，使第二级 O(m² log m) 可控。

### 5) 性能与可靠性要点

- 内存：核心对象数由 O(n²) 个 `HierarchyTreeNode/HierarchyLink` 降为第二级 O(m²)，且近似通道完全避免 n×n 距离矩阵（n=20000 精确通道需约 3.2GB，近似通道按 m=3000 约数十 MB 量级）。
- 正确性：`flatAgg` 阈值语义、`AverageLinkageStrategy` 等四种策略的**成对合并公式保持不变**（现实现平均连接实际为 WPGMA 风格，不做行为修正以免破坏既有结果）。
- 兼容性：所有公开类型与成员签名不变；`HCTreePlot`、`tutorials/VBS/scripts/hierarchical_clustering` 无需改动。

## 架构设计

沿用现有分层：`ClusteringAlgorithm`（入口/编排）→ `HierarchyBuilder`（凝聚核心）→ `Hierarchy`（数据模型）；BIRCH 作为独立的**预聚类前置层**接入 `NumericTableExtensions` 门面，不改变原有精确通道的调用链。改造点集中在 `DistanceMap`（数据结构）与 `Agglomerate`（算法循环），属替换实现而非新增架构模式。

## 目录结构

```
hierarchical-clustering/
├── HierarchyBuilder/
│   ├── DistanceMap.vb          # [MODIFY] 内部改为二叉最小堆 + 惰性删除 + 一次性 Heapify；公开成员与 MinimalDistance/FindByCodePair 语义保持不变
│   ├── HierarchyBuilder.vb     # [MODIFY] Agglomerate 去 PLINQ/临时集合/每轮 Sort，顺序循环原地更新；evaluateDistance 内联
│   ├── HierarchyLink.vb        # [MODIFY] hashCodePair 改用整数 Cluster.Id 组合键，去除字符串哈希/比较
│   ├── HierarchyTreeNode.vb    # [MODIFY] Agglomerate 增量维护叶数缓存，移除 eager LeafNames 累积
│   └── Distance.vb             # [MODIFY] 视需要补充轻量构造/比较辅助（可不改）
├── ClusteringAlgorithm/
│   ├── Cluster.vb              # [MODIFY] 新增内部 Id 与 Leafs 缓存；LeafNames 改惰性计算
│   ├── DefaultClusteringAlgorithm.vb   # [MODIFY] createLinkages 统一批量构建，移除 <100 分支逐条排序
│   ├── PDistClusteringAlgorithm.vb     # [MODIFY] createLinkages 同步批量构建
│   └── LinkageStrategy.vb      # [KEEP] 四种策略成对合并公式不变
├── BIRCH/
│   ├── CFTree.vb               # [MODIFY] 移除 splitRoot 的 GC.Collect；自动重建默认关闭
│   ├── CFEntry.vb              # [MODIFY] 新增公开只读 Centroid
│   └── BirchPreclustering.vb   # [NEW] 特征表→CF-tree→子簇质心/成员索引的封装（参数对象、targetSubclusters 控制）
├── NumericTableExtensions.vb   # [MODIFY] 新增 hcaApprox()/hcutApprox() 扩展与子簇标签回填；保留 hca/hcut 契约
└── hctree.NET5.vbproj          # [KEEP] 不新增包引用

HCTNumericTableTests/
├── Program.vb                  # [MODIFY] 保留 N=40 断言，新增近似 vs 精确一致性校验
└── Benchmark.vb                # [NEW] n>=20000 合成特征表计时基准（精确/近似通道耗时、子簇数、规模）
```

## 关键代码结构

`DistanceMap` 内部（接口级，公开签名不变）：

```
' 现有字段 → 新内部结构
Private linkTable As Dictionary(Of ULong, HierarchyLink)   ' O(1) 查找，保持不变
Private heap As List(Of HierarchyLink)                      ' 二叉最小堆，按 LinkageDistance
Private Function PopMin() As HierarchyLink                  ' 出堆并跳过 removed 项
Private Sub Push(ByRef item As HierarchyLink)               ' O(log m)
Private Sub Heapify()                                       ' 构造时一次性建堆 O(m)
Public ReadOnly Property MinimalDistance As Double          ' 窥顶（惰性清理堆顶）
Public Function Remove(link As HierarchyTreeNode) As Boolean ' 仅置 removed 并移出字典，O(1)
```

近似通道对外入口（新增，不修改既有 `hca/hcut` 签名）：

```
Public Class BirchOptions
    Public Property maxNodeEntries As Integer = 50
    Public Property threshold As Double = 0
    Public Property distFunction As Integer = CFTree.D0_DIST
    Public Property targetSubclusters As Integer = 2000
    Public Property linkage As LinkageStrategy
    Public Property silent As Boolean = True
End Class

<Extension>
Public Function hcaApprox(source As NumericTable, Optional options As BirchOptions = Nothing) As Cluster

<Extension>
Public Function hcutApprox(source As NumericTable, k As Integer, Optional options As BirchOptions = Nothing) As NumericTable
```

## Agent Extensions

### SubAgent

- **code-explorer**
- Purpose: 在改动公开类型前，批量检索 `Cluster.LeafNames`、`Cluster.Leafs`、`DistanceMap` 公开成员、`HierarchyLink.hashCodePair` 的全部调用点与调用方工程（含 `HCTreePlot/*`、`tutorials/VBS/scripts/hierarchical_clustering`）。
- Expected outcome: 产出一份精确的「改动影响清单」，确认无破坏性调用点，并据此决定 `LeafNames` 惰性化与 `Id` 哈希键改造的兼容边界。
- 二次使用：在核心改造完成后，再次核对新增 `hcaApprox/hcutApprox` 是否与既有 `hca/hcut` 重载产生歧义。

### Skill

- **lsp-code-analysis**
- Purpose: 对 `DistanceMap.Remove/Add/RemoveFirst`、`HierarchyBuilder.Agglomerate`、`Cluster.Leafs/LeafNames` 做定义跳转、引用查找与影响分析，验证重构后的签名一致性。
- Expected outcome: 确认所有引用点在重构后仍可编译（特别是 `flatAgg` 依赖的 `MinimalDistance` 窥顶语义），并快速定位遗漏的调用点。
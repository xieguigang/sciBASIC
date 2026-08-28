---
name: bonsai-test-gdi-visualization
overview: 在 Bonsai\test\ 项目中新增一个 GDI+ 可视化模块，对 Bonsai 复现结果（低维坐标散点图、树结构拓扑图、branch-time 直方图）进行绘图，并改造 test 项目使其能在 net10.0 下使用 System.Drawing 生成 PNG 测试 demo。
todos:
  - id: add-drawing-pkg
    content: 在 test.vbproj 增加 System.Drawing.Common 包引用
    status: completed
  - id: create-plot-module
    content: 新建 Plot.vb 实现 PCA2D 与三个 GDI+ 绘图函数
    status: completed
    dependencies:
      - add-drawing-pkg
  - id: wire-program
    content: 修改 Program.vb 在 Fit 后调用 Plot 生成三张 PNG
    status: completed
    dependencies:
      - create-plot-module
  - id: build-verify
    content: 编译运行 test 项目验证三张 PNG 正常生成
    status: completed
    dependencies:
      - wire-program
---

## 用户需求

在 Bonsai\test\test.vbproj 项目中新增一个基于 GDI+ 的数据可视化模块，对 Bonsai 高维降维算法的复现结果进行绘图，作为可视化测试 demo。

## 产品概述

为已复现的 Bonsai 算法提供结果可视化能力：在现有 test 控制台项目中，用 System.Drawing（GDI+）把算法产出的低维坐标、树结构与 branch-time 伪时间轴绘制为 PNG 图片，验证降维聚类效果与树拓扑正确性。

## 核心功能

- 低维坐标散点图：将 Transform() 返回的 N×D 高维坐标经内置 2D PCA 投影到二维后画散点，按 branch-time 着色并标注样本名，直观验证聚类。
- 树结构拓扑图：基于 Tree.root 递归布局，以累积 branch-time 为横向深度、同深度节点均匀分布为纵向位置，按边长度绘制有根树状图，叶子标注 nodeId。
- Branch-time 直方图：对 BranchTimeCoords() 的伪时间轴分箱统计绘制直方图，展示样本在树深度上的分布。

## 技术栈选择

- 语言/框架：VB.NET（与现有 Bonsai 项目一致），test 项目为 SDK 风格、TargetFramework=net10.0 的 Console 程序。
- 绘图：System.Drawing.Common（GDI+）NuGet 包，提供 Bitmap / Graphics / Pen / Brush / Font 等，与 UMAP\test 的绘图风格对齐。
- 复用：直接调用 Bonsai.vbproj 已实现的 Bonsai API（Fit / Transform / BranchTimeCoords / Tree.root / ToNewick），以及 BonsaiNode 的 getLeafs()、tParent、childs、nodeId、ltqs 字段；数学计算复用 Microsoft.VisualBasic.Math（矩阵/向量）中的既有工具，PCA 幂迭代自实现以避免引入重型依赖。

## 实现方案

- 策略：在 test 项目内新增独立 Plot 模块（Module Plot），将"绘图"与"算法验证"职责分离；Program.vb 在现有 Fit 之后调用三个绘图函数生成 PNG。
- 关键决策：

1. net10.0 使用 GDI+ 必须引用 System.Drawing.Common 包（UMAP 的旧 test 是 .NET Framework 4.8 直接 Reference System.Drawing，不适用于本 SDK 项目），故在 test.vbproj 增加 PackageReference。
2. PCA 采用去中心化 + 协方差矩阵幂迭代求前两主成分（2~3 次幂迭代即可收敛，复杂度 O(N·D·iters)），无需 Eigen 库；输入 D 可能很小（demo 中 D=3），性能充足。
3. 树布局用广度优先按累积 tParent 计算每节点 x（深度），同深度节点按出现顺序均分 y，避免重叠；线宽固定、节点画实心圆，叶子用 DrawString 标 nodeId。
4. 散点着色用 branch-time 归一化映射到颜色梯度（ColorTranslator.FromHtml 调色板，参考 UMAP 调色方式），保证视觉区分度。

- 性能与可靠性：绘图为一次性离线 demo，数据规模小（N≤数百），Bitmap 用 Using 包裹确保 Dispose；坐标归一化做 min/max 防除零；branch-time 全为 0 时退化为单色。

## 实现注意事项

- 复用现有 test.vbproj 的 ProjectReference（已含 ..\Bonsai.vbproj），仅追加 System.Drawing.Common 包引用，不改变其他引用与构建目标，控制影响面。
- 绘图输出路径使用 AppContext.BaseDirectory 或相对 bin 路径，避免硬编码绝对路径导致在其他机器失败。
- 保留 Program.vb 原有优化器自测与诊断打印，仅在 Main 末尾追加绘图调用，不破坏既有验证逻辑。
- PCA 投影前检查 D>=2，否则回退直接取前两维（D=1 时第二维补 0），保证不越界。

## 架构设计

- 现有结构：Program（验证入口）→ Bonsai（算法）→ BonsaiTree/BonsaiNode（结构）。
- 新增：test 项目内 Plot 模块作为纯展示层，单向依赖 Bonsai API 输出（Double()()、Double()、BonsaiNode），不反向耦合算法内部。
- 数据流：Fit → Transform/BranchTimeCoords/Tree.root → Plot 各函数 → 保存 PNG。

## 目录结构

```
Bonsai/
├── test/
│   ├── test.vbproj              # [MODIFY] 增加 System.Drawing.Common 包引用，使 net10.0 可用 GDI+。
│   ├── Program.vb               # [MODIFY] 在 Main 末尾调用 Plot 三函数生成 PNG 并打印路径，保留原诊断逻辑。
│   └── Plot.vb                  # [NEW] GDI+ 可视化模块（Module Plot）。实现 PCA2D 投影、PlotScatter、PlotTree、PlotBranchTimeHistogram 三个公开绘图函数，统一 Using Bitmap/Graphics，输出 PNG 到指定路径。
```

## 关键代码结构

```
' Plot.vb 公开接口（示意签名，非实现体）
Module Plot
    ' 对 N×D 矩阵做简单 2D PCA 投影，返回 N×2 坐标
    Public Function PCA2D(X As Double()()) As Double()()

    ' 低维散点图：coords2d 为 PCA 投影结果，branchTimes 用于着色，按 filePath 保存 PNG
    Public Sub PlotScatter(coords2d As Double()(), labels As String(), branchTimes As Double(), filePath As String)

    ' 树拓扑图：root 为 Bonsai 根节点，按累积 tParent 布局，保存 PNG
    Public Sub PlotTree(root As Microsoft.VisualBasic.DataMining.Bonsai.BonsaiNode, filePath As String)

    ' branch-time 直方图：times 为伪时间轴，分箱统计保存 PNG
    Public Sub PlotBranchTimeHistogram(times As Double(), filePath As String)
End Module
```
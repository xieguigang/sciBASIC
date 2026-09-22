---
name: svm-demo-cli-gdi-plot
overview: 将 test/svm/SVMDemo.vb 从不可编译的 WPF 界面重写为纯命令行 SVM 演示程序：内置随机合成二维二分类数据，训练 C-SVC(RBF) 模型，并用 GDI+ 渲染决策区域与样本点输出 PNG 图片；同时修正 test.vbproj 使其可编译运行。
todos:
  - id: explore-verify-api
    content: 用 [subagent:code-explorer] 核查 SVM 公共 API 签名与旧 SVMDemo WPF 类型的外部引用，确认无残留依赖
    status: completed
  - id: rewrite-svmdemo-entry
    content: 重写 test/svm/SVMDemo.vb 为控制台入口：移除 WPF，新增 Module SVMDemoProgram.Main，实现造数据、缩放、训练与准确率输出
    status: completed
    dependencies:
      - explore-verify-api
  - id: gdi-render-png
    content: 新增 test/svm/SVMDemoPlot.vb：GDI+ 绘制决策区域热力图、边界、样本点与支持向量高亮并保存 PNG
    status: completed
    dependencies:
      - rewrite-svmdemo-entry
  - id: wire-project
    content: 修改 test/test.vbproj：添加 StartupObject 指向 SVM demo，并引入 System.Drawing.Common 包引用
    status: completed
  - id: build-and-verify
    content: 构建并运行 demo，验证控制台准确率输出与 PNG 正确生成，用 [skill:lsp-code-analysis] 校验符号全名并修正异常
    status: completed
    dependencies:
      - rewrite-svmdemo-entry
      - gdi-render-png
      - wire-project
---

## 产品概述

将当前无法编译的 WPF 版 `test/svm/SVMDemo.vb` 改造为一个可直接运行的**命令行 SVM 演示程序**：使用内置随机合成二维数据，用 LibSVM（C-SVC + RBF 核）完成训练与评估，并通过 GDI+ 将决策区域与样本点渲染为 PNG 图片保存到磁盘。运行 `dotnet run`（或以 test 项目为启动项）即可完成"造数据 → 训练 → 评估 → 出图"的完整流程，无窗口、无交互依赖。

## 核心功能

- **一键运行**：程序入口为控制台 `Main`，运行后自动完成全流程，控制台打印关键信息后退出（不弹窗、不阻塞）。
- **内置合成数据**：使用 SVM 库自带的 `SVMUtilities.CreateTwoClassProblem` 生成二维二分类样本（开箱即跑，无需外部数据文件），并额外生成一份测试集用于评估。
- **固定训练参数**：C-SVC + RBF 核，gamma = 0.5，C = 1（参数写死在代码中，最简形式）。
- **训练与评估输出**：先做 `RangeTransform` 数据缩放，再训练模型；打印样本数、维度、支持向量数量，以及训练集/测试集分类准确率。
- **GDI+ 可视化出图**：渲染并保存 PNG 图片，包含：
- 决策区域热力图（不同类别区域用不同淡色填充，直观展示分类边界）；
- 决策边界（区域颜色交界处的轮廓）；
- 训练样本点（按真实类别着色，实心圆 + 深色描边）；
- 支持向量高亮（用环形/加粗描边标出，体现 SVM 的核心概念）；
- 标题（核函数与参数）、类别图例、准确率文字标注。
- **结果落盘与提示**：图片默认输出为相对路径（如 `./svm_demo.png`），并在控制台打印生成的绝对路径。

## 技术栈

- 语言/运行时：VB.NET，工程 `test/test.vbproj`（SDK 风格，`OutputType=Exe`，`TargetFrameworks=net10.0-windows`）
- 机器学习：复用现有 `Microsoft.VisualBasic.MachineLearning.SVM`（LibSVM 移植：`Problem`/`Parameter`/`RangeTransform`/`Training`/`Prediction`/`SVMPrediction`）
- 绘图：GDI+（`System.Drawing`），通过 NuGet 包 `System.Drawing.Common` 提供（`Bitmap`/`Graphics`/`Font` 等不在共享框架内，必须显式引用）
- 参考范例：`Data_science/DataMining/Bonsai/test/Plot.vb`（纯 GDI+ 控制台出 PNG 的既有范式）、`Bonsai/test/test.vbproj`（`net10.0-windows` + `System.Drawing.Common` 的既有配置）

## 实现方案

1. **整体策略**：把原来"WPF 交互式手绘数据 → 后台线程分类 → 图片作为 Canvas 背景"的逻辑，等价改写为"控制台批量流程 → GDI+ 离屏位图 → 保存 PNG"。算法语义完全保留（RangeTransform 缩放 + `Training.Train` + 逐网格点 `Predict` 上色），仅把像素坐标系换成数据坐标系、把 XAML 控件换成 Bitmap。
2. **训练链路**（复用库 API，不新增算法代码）：

- `SVMUtilities.CreateTwoClassProblem(n, True/False)` 得到训练集/测试集（`maxIndex = 2`，标签经 `ClassEncoder` 编码为因子 1、2）；
- `RangeTransform.Compute(train)` → `transform.Scale(train)` 与 `transform.Scale(test)`（缩放必须用同一 transform，保证训练/预测/绘图一致）；
- `Training.Train(scaledTrain, New Parameter With {.svmType = SvmType.C_SVC, .kernelType = KernelType.RBF, .gamma = 0.5, .c = 1})`；
- 评估：`Prediction.Predict(test, Nothing, model, False)` 取回"百分比正确"得分；支持向量数取 `model.supportVectorCount`；如需交叉验证可调用 `Training.PerformCrossValidation(train, param, 5)`（可选）。

3. **可视化链路（关键性能决策）**：不做全像素逐点预测，而是沿用原 demo 的**降采样策略**——以 `stride`（建议 4px）在整幅图上求网格类别标签，再按块填充像素：

- 预测次数从 `W×H`（约 63 万次）降至 `(W/stride)×(H/stride)`（约 4 万次），RBF 核单次代价 O(支持向量数)，整体秒级完成；
- 像素→数据坐标用训练数据各维 min/max 的线性映射（带 padding），再 `transform.Transform(Node())` 后 `model.Predict`，取 `SVMPrediction.class`；
- 用一次遍历同时完成"区域底色 + 类别交界轮廓"绘制，避免二次扫描。

4. **配色与图元**：自定义 2 色调色板（如番茄红 / 钢蓝），决策区域用与白色混合后的淡色，样本点用饱和色；支持向量用双层圆环高亮；图例、标题、准确率用 `DrawString` 绘制。选择自定义调色板而非依赖 `ClassEncoder` 内部颜色，避免引入对 `Microsoft.VisualBasic.Imaging` 颜色的额外耦合（类别名仍从 `ColorClass.name` 读取）。
5. **工程质量与卫生**：

- 时间/空间复杂度：训练 O(样本数² ~ ³)（库内实现，样本量取 200~300 控制在秒级）；绘图 O(网格点数 × 支持向量数)，内存仅一张 `W×H×4` 位图；
- 资源管理：`Bitmap`/`Graphics`/`Font`/`Pen`/`Brush` 全部 `Using` 释放，PNG 用 `ImageFormat.Png` 保存；
- 错误处理：数据为空、`Problem` 维度缺失、保存路径不可写时给出明确 `Console.WriteLine`/异常信息，不静默失败；
- 日志：复用库内 `Logging.IsVerbose`/`Logging.flush()` 保持与库一致（默认 False），演示信息直接用 `Console.WriteLine`，不打印敏感信息、不刷屏。

## 实施要点（防止返工）

- **必须彻底移除 WPF**：删除 `Imports System.Windows.*` 与所有 XAML 控件引用（`plot`/`classCB`/`classifyPB`/`placementCB`/`svmTypeCB`/`kernelTypeCB`）及 `Partial Class MainWindow : Inherits Window`，否则工程仍无法编译。
- **入口点冲突**：`test/rf.vb` 已有 `Sub Main()`，新增入口必须配合 `test.vbproj` 的 `<StartupObject>`（带 RootNamespace 前缀，即 `test.SVMDemo.SVMDemoProgram`），否则报"定义了多个入口点"。
- **GDI+ 依赖**：`Color`/`PointF` 来自共享框架的 System.Drawing.Primitives（`DataPoint.vb` 无需改动即可编译），但 `Bitmap`/`Graphics`/`ImageFormat` 需要 `System.Drawing.Common` 包；版本优先 10.0.12（与仓库多数 net10 工程一致），若解析失败退回 9.0.0。
- **类别因子对齐**：`ClassEncoder` 首次分配因子为 1，故 `SVMPrediction.class` 取值为 1/2，绘图与配色数组必须按因子索引，避免越界（建议按 `ClassEncoder.Colors` 动态构建调色板长度）。
- **不阻塞退出**：不使用 `Pause()`（其解析依赖调用上下文，存在风险），仅打印结果路径；如需 IDE 双击停留，可用 `Console.ReadLine()` 兜底。
- **影响范围控制**：`rf.vb`、`Module1.vb`、`activeTest.vb`、`simpleANNtest.vb`、`DataPoint.vb` 与 SVM 库代码全部保持不变，仅 3 个文件参与改动。

## 目录结构

```
MachineLearning/test/
├── test.vbproj              # [MODIFY] 增加 StartupObject 指向 SVM demo；新增 System.Drawing.Common 包引用
└── svm/
    ├── SVMDemo.vb           # [MODIFY] 重写为控制台程序：移除 WPF；新增 Module SVMDemoProgram + Sub Main，
    │                        #          负责造数据、RangeTransform 缩放、Training.Train、准确率评估与控制台输出
    ├── SVMDemoPlot.vb       # [NEW] GDI+ 绘图模块（Module SVMDemoPlot）：决策区域热力图（stride 降采样）、
    │                        #       决策边界、训练样本点、支持向量高亮、标题/图例/准确率，保存 PNG
    └── DataPoint.vb         # [REUSE] 复用现有 test.SVMDemo.DataPoint（Position As PointF、Label As Double），无需修改
```

## 关键代码结构

```
Namespace SVMDemo

    ' 控制台入口；test.vbproj 的 StartupObject 需设置为 test.SVMDemo.SVMDemoProgram
    Public Module SVMDemoProgram
        Public Sub Main()
            ' 1) CreateTwoClassProblem 造训练/测试数据
            ' 2) RangeTransform.Compute + Scale
            ' 3) Training.Train(C_SVC + RBF, gamma=0.5, c=1)
            ' 4) Prediction.Predict 评估准确率并打印
            ' 5) 调用 SVMDemoPlot.PlotResult(...) 生成 PNG
        End Sub
    End Module

    ' GDI+ 绘图层：纯展示，只依赖 SVM 公共 API
    Public Module SVMDemoPlot
        ''' <param name="problem">原始（未缩放）训练问题，用于取数据范围与真实标签</param>
        ''' <param name="model">已训练模型</param>
        ''' <param name="transform">训练时使用的缩放变换，用于把像素坐标映射后的数据点送入模型</param>
        ''' <param name="file">PNG 输出路径</param>
        Public Sub PlotResult(problem As SVM.Problem,
                              model As SVM.Model,
                              transform As SVM.IRangeTransform,
                              file As String)
        End Sub
    End Module

End Namespace
```

## Agent Extensions

### SubAgent

- **code-explorer**
- Purpose: 在改造前做一次有界的代码核查，确认 (1) 无其他文件引用旧 `SVMDemo` 的 WPF 类型（避免残留编译错误）、(2) SVM 公共 API 的准确签名（`Problem`/`RangeTransform.Scale`/`Training.Train`/`Prediction.Predict`/`SVMPrediction.class`）、(3) 现有 GDI+ 出 PNG 的写法范式。
- Expected outcome: 得到已核实的关键调用清单与无残留引用结论，保证新代码一次编译通过。

### Skill

- **lsp-code-analysis**
- Purpose: 在收尾阶段做语义级校验，精确定位 `Problem`、`Training.Train`、`RangeTransform`、`DataPoint`、`SVMDemoProgram` 等符号的完整限定名与引用位置，确认 `StartupObject` 填写值与实际类型全名一致。
- Expected outcome: 确认 `test.SVMDemo.SVMDemoProgram` 等 FQ 名称正确，且无对已删除 WPF 成员的悬空引用。
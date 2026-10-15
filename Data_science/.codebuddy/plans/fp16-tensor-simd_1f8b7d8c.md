---
name: fp16-tensor-simd
overview: 在 MachineLearning/TensorFlow 工程中新增 fp16（Half）精度张量栈：TensorH 张量类 + ITensorComputeH 后端契约 + TensorComputeHBase 标量兜底 + SIMDTensorH（基于已有 HalfSimd.vb 的 8 路 SIMD 内核），并扩充 HalfSimd 缺失的算子核，实现与 Tensor/TensorF 三层可插拔架构完全同构的 fp16 tensor 算子对象。
todos:
  - id: extend-halfsimd
    content: 扩充 HalfSimd.vb：追加 Maximum/Minimum 二元核、一元核组（Abs/Negate/Square/Reciprocal/Sqrt/Exp/Log/Tanh/Sigmoid/Relu/Clip）、Half/Double 标量重载、MatMulH GEMM 核与 ToHalfChecked，保持既有 lane 核与 bit-exact 契约不变
    status: completed
  - id: tensorh-class
    content: 新建 TensorH.vb：以 TensorF.vb 为模板实现 Half 张量对象（存储/索引器/工厂/互转/设备纪元/computeKernelH）
    status: completed
    dependencies:
      - extend-halfsimd
  - id: itensorcomputeh
    content: 新建 Compute/ITensorComputeH.vb：以 ITensorComputeF.vb 为蓝本定义约 60 个算子的 fp16 后端契约（裁剪双精度栈独有算子）
    status: completed
    dependencies:
      - tensorh-class
  - id: tensorcomputehbase
    content: 新建 Compute/TensorComputeHBase.vb：实现全部算子的标量兜底参考实现（含 Conv2D/MaxPool2D/CFD 原语/softmax/损失，中间量 Double 域）
    status: completed
    dependencies:
      - itensorcomputeh
  - id: simdtensorh
    content: 新建 Compute/SIMDTensorH.vb：基于 HalfSimd 重写确有收益的算子（逐元素/标量/归约/MatMul/Softmax 并行），阈值与 SIMDTensorF 对齐，其余继承标量兜底
    status: completed
    dependencies:
      - tensorcomputehbase
  - id: build-verify
    content: 编译 TensorFlow.vbproj 与 Math.NET5.vbproj，修复所有警告/错误，确认零新依赖且既有 Tensor/TensorF 调用方不受影响
    status: completed
    dependencies:
      - simdtensorh
---

## User Requirements

用户要在 `MachineLearning/TensorFlow/TensorFlow.vbproj`（RootNamespace = `Microsoft.VisualBasic.MachineLearning.TensorFlow`，net10.0）中新增 fp16（`System.Half`）精度的 tensor 算子对象，复用基础数学库中已构建的 `Mathematica/Math/Math/Numerics/HalfSimd.vb`（命名空间 `Microsoft.VisualBasic.Math.Numerics`，8 路 `Vector128` SIMD 内核，`Vector(Of Half)` 不受支持故走 `MemoryMarshal.Cast(Of Half, Short)` 通道）。本次交付范围限定为"基于 SIMD 计算加速的 fp16 tensor 算子对象"这一层：张量类、后端契约、标量兜底、SIMD 后端，以及 HalfSimd 缺失算子核的补充；上层 nn/Math/NumPy 封装、GPU 后端、对拍测试工程列为后续阶段，不作为本次必须交付。

## Product Overview

一条与既有双精度栈（`Tensor` → `ITensorCompute` → `TensorComputeBase` → `SIMDTensor`）和单精度栈（`TensorF` → `ITensorComputeF` → `TensorComputeFBase` → `SIMDTensorF`）完全同构的 fp16 栈：`TensorH`（Half 存储）→ `ITensorComputeH`（后端契约）→ `TensorComputeHBase`（标量兜底，保证语义正确）→ `SIMDTensorH`（基于 HalfSimd 的 SIMD 加速，只重写确有收益的算子）。契约以 `ITensorComputeF` 为蓝本裁剪（去除双精度栈独有的 Sin/Cos/Pow/Prod/StdDev/Slice/Concat/TopK/Heaviside/五个融合批算子/PinDevice64），保持逐元素二元/一元、标量、矩阵、归约、softmax、损失、训练算子、卷积池化、CFD 原语的完整集合。TensorH 索引器设计为纯数组读写（无版本计数/原子操作），与 TensorF 的热循环定位一致；同时提供与 Tensor/TensorF 的双向互转（批量转换直接复用 HalfSimd 的 `ToHalf/ToSingle`）。新增 HalfSimd 缺核：二元 Maximum/Minimum、一元核组（Abs/Negate/Square/Reciprocal/Sqrt/Exp/Log/Tanh/Sigmoid/Relu/Clip）、Half/Double 标量重载、MatMul 专用 GEMM 核、fp16 范围安全转换 `ToHalfChecked`。数值约束：fp16 上界 65520、exp 在 x>11.1 溢出，所有 softmax/loss/激活一律"中间量升到 Single/Double、出口一次舍入"；HalfSimd 融合算子与标量 `a+b` 逐位一致，可做差分回归。

## Core Features

- TensorH 张量对象：Half() 行优先存储、形状/步长、1/2/3/任意维索引器、工厂方法（Zeros/Ones/Filled/Scalar/Wrap）、Clone/CopyFrom/Clear/Fill、设备缓存纪元与 MarkHostModified、ToTensor/ToTensorF/ToDoubles 与 FromTensor/FromTensorF/FromDoubles 互转、`computeKernelH` 可插拔后端
- ITensorComputeH 后端契约：约 60 个算子签名（以 ITensorComputeF 为蓝本，设备钉住只保留 FP32 语义通道）
- TensorComputeHBase 标量兜底：全部算子的语义正确参考实现，含 Conv2D 四件套、MaxPool2D 两件套、CFD 三原语、逐轴归约、softmax/损失/MaskedCrossEntropy（中间量 Double 域）
- SIMDTensorH：基于 HalfSimd 加速确有收益的算子（二元四则/极值、一元、标量、FMA、Sum/Mean/Min/MaxAll、MatMul、Transpose、Softmax/LogSoftmax 并行），阈值与 SIMDTensorF 对齐（ReductionParallelThreshold=4096、MatMulParallelThreshold=10000）
- HalfSimd 扩充：Maximum/Minimum、一元核组、Half/Double 标量重载、MatMul GEMM 核、ToHalfChecked

## Tech Stack Selection

- 语言/框架：VB.NET（net10.0，Option Strict On 惯例），沿用工程既有 SDK 风格 vbproj 自动包含 .vb 文件，无需手工登记
- 计算内核：`System.Runtime.Intrinsics.Vector128`（8 路 Half 通道），复用 `Microsoft.VisualBasic.Math.Numerics.HalfSimd` 的既有 lane 核与 `SIMDEnvironment.IsEnabled` 判据
- 并行：`System.Threading.Tasks.Parallel.For`（与 SIMDTensorF 同阈值策略）
- 依赖：零新增——`TensorFlow.vbproj` 第 51 行已引用 `..\..\Mathematica\Math\Math\Math.NET5.vbproj`，HalfSimd 立即可用；Math 工程 RootNamespace 为 `Microsoft.VisualBasic.Math`，故全名为 `Microsoft.VisualBasic.Math.Numerics.HalfSimd`，工程内须 `Imports halfSimd = Microsoft.VisualBasic.Math.Numerics`

## Implementation Approach

策略：**模式复刻 + 内核补齐**。以 `TensorF/ITensorComputeF/TensorComputeFBase/SIMDTensorF` 四文件为精确模板，将 `Single` 机械替换为 `Half`、`Vector(Of Single)` 路径机械替换为 HalfSimd 调用；HalfSimd 已有内核的算子直接挂接，没有的内核先在 HalfSimd.vb 中按同一 lane 风格补齐（widen→Single lane 运算→Narrow，标量尾部与 BCL 逐位一致），保证"SIMD 路径与标量路径可差分回归"。关键决策与取舍：

1. **契约裁剪而非照搬 ITensorCompute**：fp16 初始范围对齐 ITensorComputeF（约 60 成员），双精度栈独有的 Sin/Cos/Pow/Prod/StdDev/Slice/Concat/TopK/Heaviside/五个融合批算子（LtcRk4Batch/GraphLayerBatch/GraphFeatureBatch/FluxHeadBatch/SystemTauBatch）/PinDevice64 明确排除——它们在 F32 契约中也不存在，纳入只会制造无调用方的死代码；后续需要时再以接口扩展方式追加，不破坏本次交付的稳定性。
2. **三层分层保证正确性优先**：TensorComputeHBase 给出全部算子的标量参考实现（语义基准 + 未来 GPU 后端的对拍基准 + SIMD 缺失时的回退），SIMDTensorH 只重写"确有向量指令收益"的算子，与 SIMDTensorF 的取舍完全一致。
3. **数值稳定性约定显式化**：Softmax/LogSoftmax/损失/激活一律 Double/Single 中间量、出口 `CType(..., Half)` 一次舍入；MatMul/SpMM 累加用 Double；这既是 HalfSimd 注释已确立的约定，也是 fp16 动态范围（max 65520、exp 11.1 溢出）的硬约束。
4. **标量参数类型扩张**：HalfSimd 现有 scalar 重载只接 Single；TensorH 公开 API 需要 Half/Double 标量版本，在 HalfSimd.vb 中以 Thin Wrapper 补充（内部转 Single 广播，尾手与 BCL 一致），避免每个调用点重复写转换。
5. **互转零成本**：TensorH↔TensorF/Tensor 的批量转换直接调 HalfSimd.ConvertToHalf/ConvertToSingle/ToHalf(Double())，不写逐元素循环。
6. **MatMul 专用核**：HalfSimd 现有只有逐向量 Dot（逐输出元素调用有函数调用与重复 load 开销）；在 HalfSimd.vb 增加 `MatMulH(a, b)` 分块/ikj GEMM：k 维用 Double 累加（或 Single lane + 周期性 Double 收尾），出口一次舍入，与标量三重循环语义一致（归约顺序固定，可逐位对拍）。
7. **ToHalfChecked**：对标 MixedPrecisionPolicy.ToSingleChecked，NaN→0、|v|>65520 夹断并记 MixedPrecisionDiagnostics.Fallback，防止 Double→Half 静默 Inf 污染动量/二阶矩。

性能要点：逐元素算子为内存带宽受限，Half 存储带宽需求减半（相对 Single）、与 Single 相同 lane 数（8 路），理论吞吐相对 Single 栈约 2 倍；热点路径 SumAll/Dot 走 Double lane 累加避免精度回退；逐轴归约/softmax/MatMul/Transpose 与 SIMDTensorF 同阈值并行。复杂度：逐元素 O(n) 一次分配（`GC.AllocateUninitializedArray` 风格在 HalfSimd 中已用，TensorH 层沿用 `New Half(n-1)` 与既有 F32 兜底一致以保持模式统一）；MatMul O(m·k·n)。

## Implementation Notes

- 复用既有 helper 命名与结构：TensorComputeHBase 的 MapBinary/MapUnary/RequireSameShape/Wrap 与 F32 版逐字同构（仅类型替换）；SIMDTensorH 的并行阈值常量名与 SIMDTensorF 保持一致（ReductionParallelThreshold/MatMulParallelThreshold）
- VB 陷阱：`Vector(Of Half)` 不可用（读 Count 抛 NotSupportedException）→ 一律走 HalfSimd；Math 命名空间遮蔽，HalfSimd.vb 内部已用 `System.Math` 全限定，新代码同样遵守；Half 为值类型，转换统一 `CType(CSng(x), Half)`；数组类型写 `Half()`
- 日志/诊断：范围夹断走 MixedPrecisionDiagnostics.Fallback（既有计数器），不新增日志通道；不在热循环打印
- 爆炸半径控制：本次只新增 4 个新文件 + 扩充 1 个既有文件（HalfSimd.vb 纯追加新 Region，不改动既有 lane 核与常量，保证既有 F32/双精度调用方零影响）；不改 Tensor.vb/TensorF.vb/既有 Compute 文件/任何 vbproj
- 回归保障：HalfSimd 文档声明的 bit-exact 契约是差分测试基础；SIMDTensorH 每个重写算子应能与 TensorComputeHBase 标量版对拍（本次交付代码，测试工程列入后续阶段）

## Architecture Design

```mermaid
graph LR
    subgraph TensorFlow["MachineLearning.TensorFlow (新增)"]
        TH["TensorH.vb<br/>Half 张量对象<br/>computeKernelH 可插拔"]
        ITH["Compute/ITensorComputeH.vb<br/>~60 算子契约"]
        THB["Compute/TensorComputeHBase.vb<br/>标量兜底/参考实现"]
        SIH["Compute/SIMDTensorH.vb<br/>HalfSimd SIMD 加速"]
    end
    subgraph Math["Microsoft.VisualBasic.Math (扩充)"]
        HS["Numerics/HalfSimd.vb<br/>+Maximum/Minimum +一元核组<br/>+Half/Double 标量重载 +MatMulH +ToHalfChecked"]
    end
    TH -->|委托| ITH
    ITH <--.">| THB -.实现.-> SIH
    SIH -->|调用| HS
    THB -.语义基准/回退.-> SIH
```

数据流：调用方 `TensorH` 公开 API/运算符 → `TensorH.computeKernelH`（默认 `SIMDTensorH.Default`）→ SIMD 路径（HalfSimd 内核，n>=8 且 SIMD 可用）或标量尾部 → 结果 `Half()` 包回 `TensorH`；后端可整体替换（未来 `CudaTensorH`），与两条既有栈的切换语义一致。

## Directory Structure

```
g:/pixelArtist/src/framework/Data_science/
├── MachineLearning/TensorFlow/
│   ├── TensorH.vb                          # [NEW] fp16 张量对象。以 TensorF.vb 为模板：Half() 行优先存储、_Shape/_DimProds、
│   │                                       #       无参/shape/data 三个构造、Zeros/Ones/Filled/Scalar/Wrap 工厂、
│   │                                       #       1/2/3/任意维索引器（纯数组读写无版本计数）、Clear/Fill/Clone/CloneT/CopyFrom、
│   │                                       #       DeviceEpoch/Version/MarkHostModified/InvalidateAllDeviceCaches、
│   │                                       #       Shared computeKernelH As ITensorComputeH = SIMDTensorH.Default、
│   │                                       #       ToTensor/ToTensorF/ToDoubles/FromTensor/FromTensorF/FromDoubles（复用 HalfSimd 批量转换）、
│   │                                       #       Get1DInd/UpdateDimProds、Dispose、ToString。加法运算符支持与 Tensor 一致的
│   │                                       #       (1,n)+(n,1) 广播遗留语义（照搬 BroadcastedAddition 逻辑）。
│   └── Compute/
│       ├── ITensorComputeH.vb              # [NEW] fp16 后端契约。以 ITensorComputeF.vb 为蓝本：Name 属性；逐元素二元 6、
│       │                                   #       一元 13+Heaviside 档（按 F32 集合裁剪，去掉 F32 也没有的 Sin/Cos/Pow 等）、
│       │                                   #       标量 5（AddScalar/MultiplyScalar/DivideScalar/Clip，标量类型 Half/Double）、
│       │                                   #       MatMul/Transpose/SpMM/LifStep、归约（SumAll/MeanAll/MinAll/MaxAll/Sum/Mean/Max/Min/
│       │                                   #       ArgMax/ArgMin）、Softmax/LogSoftmax/SigmoidCrossEntropyWithLogits/MseLoss/L2Loss/
│       │                                   #       HuberLoss、MaskedCrossEntropy/TryAdamWStep、设备钉住 5（仅 FP32 语义通道，
│       │                                   #       去掉 PinDevice64）、Conv2D×4/MaxPool2D×2、CFD 原语×4（Laplacian7/JacobiStencil7/
│       │                                   #       AdvectTrilinear/ApplyMask）。参数/返回类型 Tensor→TensorH、Single→Half；
│       │                                   #       累加器类返回（SumAll 等）保持 Double。
│       ├── TensorComputeHBase.vb           # [NEW] ITensorComputeH 标量兜底（MustInherit）。模板 TensorComputeFBase.vb：
│       │                                   #       MapBinary/MapUnary/RequireSameShape/Wrap helpers；全部算子标量参考实现；
│       │                                   #       Conv2D 四件套与 MaxPool2D 两件套（Double 累加 CSng 出口）；
│       │                                   #       CFD 三原语 + TrilinearAt + ApplyMask（网格 idx=(i*ny+j)*nz+k 约定）；
│       │                                   #       ReduceAlongAxisH/ReduceArgAxisH/ArgGlobalH；Softmax/LogSoftmax/损失/
│       │                                   #       MaskedCrossEntropy 中间量一律 Double 域；SpMM/LifStep 保持 Double 累加
│       │                                   #       （SparseCsr.Values 为 Double()，直接消费）；TryAdamWStep/设备钉住默认 False 回退。
│       └── SIMDTensorH.vb                  # [NEW] HalfSimd SIMD 后端（Inherits TensorComputeHBase）。Name="SIMD-F16"；
│   │                                       #       [Default] 单例 + Register()；重写：Add/Subtract/Multiply/Divide/Maximum/Minimum、
│   │                                       #       Sqrt/Square/Abs/Negate/Reciprocal/Exp/Log/Tanh/Sigmoid/Relu、AddScalar/
│   │                                       #       MultiplyScalar/DivideScalar/Clip、SumAll/MeanAll/MinAll/MaxAll、Sum/Mean/Max/Min
│   │                                       #       逐轴并行、Softmax/LogSoftmax 并行、MatMul（HalfSimd.MatMulH 或行并行+
│   │                                       #       Dot）、Transpose；阈值 ReductionParallelThreshold=4096、
│   │                                       #       MatMulParallelThreshold=10000（与 SIMDTensorF 对齐）。
└── Mathematica/Math/Math/Numerics/
└── HalfSimd.vb                         # [MODIFY] 纯追加新 Region，不改既有 lane 核/常量/既有函数：

#   +二元 Maximum/Minimum（Half,HHalf→Half）

#   +一元核组：Abs/Negate/Square/Reciprocal/Sqrt/Exp/Log/Tanh/Sigmoid/Relu/Clip

#     （widen→Single lane→逐 lane std.XXX 或位技巧→Narrow；标量尾部与 BCL 一致）

#   +Half/Double 标量重载（AddScalar/SubtractScalar/MultiplyScalar/DivideScalar 的

#     Half/Double 版本，内部转 Single 广播）

#   +MatMulH(a As Half(), b As Half(), m,k,n) As Half()：ikj 分块 GEMM，

#     Double 累加、出口一次舍入，与标量三重循环归约顺序一致

#   +ToHalfChecked(value As Double) As Half（±65520 夹断+NaN→0，走

#     MixedPrecisionDiagnostics.Fallback 计数——注意该模块在 TensorFlow 工程，

#     若产生循环依赖则改为在本模块内计数或由调用方传入回调）

# Agent Extensions

无可用扩展。本任务为 VB.NET 计算库代码实现，上下文提供的 skills（多模态内容生成、lsp-code-analysis、skill-creator、pptx、pdf、docx、xlsx、agent-browser、playwright-cli、find-skills）均不适用于该场景；未提供任何 MCP 或 Integration。代码探索已在前期调研中完成，后续实现可直接依据已核实的文件路径与模式进行。
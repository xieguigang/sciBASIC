---
name: gaussian-splatting-perf
overview: 对 sciBASIC# 的 3D Gaussian Splatting 类库做全面性能重构：把 O(N_params×全量重渲染) 的中心差分梯度换成 tile-based 光栅化 + 解析反向传播，热点用 fp32 TensorF（宿主注册 CUDA 后自动上 GPU）批处理、非热点用 fp64 Tensor 算子 + Core/Math/SIMD 的 SIMD+parallel 内核；同时修正两处数学缺陷、新增 SplatTrainer/后端注入接口与基准对拍工具链。
todos:
  - id: baseline-harness
    content: 在 test 工程建立 bench/verify 脚手架与分阶段计时，固化旧实现的基线与 CSV 输出
    status: completed
  - id: model-soa
    content: 重构 GaussianModel 为 SoA 双精度存储、缓冲复用、densify 保留动量、fp32 镜像与 Version
    status: completed
    dependencies:
      - baseline-harness
  - id: projection-fp32
    content: 实现 SplatProjection：fp32 TensorF 批量投影（含正确 Σ_cam 旋转）并保留 fp64 参考路径
    status: completed
    dependencies:
      - model-soa
  - id: forward-tile
    content: 实现 tile binning、near→far 键值排序与 Parallel tile 前向光栅化，接入 SplatRenderer 门面与 SIMD 版 MSE
    status: completed
    dependencies:
      - projection-fp32
  - id: backward-analytic
    content: 实现解析反向传播与线程本地梯度归约，GradientCalculator 支持 Analytic/FiniteDifference 双模式
    status: completed
    dependencies:
      - forward-tile
  - id: optimizer-io
    content: 用 SIMD 单遍内核重写 Adam，并改造 DensityController、ImageLoader 与模型搬运的零索引器路径
    status: completed
    dependencies:
      - model-soa
  - id: backend-trainer
    content: 抽出 ISplatRasterBackend/SplatBackends 与 CpuTileRasterBackend，新增 SplatTrainer 及 train/bench/verify/--gpu CLI
    status: completed
    dependencies:
      - backward-analytic
      - optimizer-io
  - id: validate-report
    content: 跑通四组 verify 对拍与 bench 加速比，修复回归，更新 README 并达成 0 警告构建
    status: completed
    dependencies:
      - backend-trainer
---

## 用户需求

对 `GaussianSplatting3D/GaussianSplatting`（VB.NET，3D Gaussian Splatting）做**算法级 + 内核级**性能重构：以混合精度 tensor 为核心，热点用 fp32 tensor（宿主注册 CUDA 后自动上 GPU），其余用 fp64 tensor 算子或 Core 内置 SIMD+Parallel 内核加速；库本身保持不引用 CUDA 模块，由 test 工程注入后端。

## 已确认的关键决策

- 梯度：**改为解析反向传播**（dL/dμ, dL/ds, dL/dq, dL/dc, dL/dα），保留中心差分作为对拍基准。
- 数学：**允许修正**两处缺陷 —— ①深度排序改为 near→far 并配合 front-to-back 合成；②协方差补上 `Σ_cam = R_cam·Σ_world·R_camᵀ`。
- GPU：自动分派 + **额外提供可注入的光栅化后端接口**，宿主可提供自定义 CUDA 内核。
- API：**可自由重构**（允许破坏性变更），按最优设计组织类型。

## 产品功能与效果

- **训练可用**：当前「N=2000、12 视图、500 迭代」配置因每个高斯需 240 次全量重渲染而实际不可完成；重构后应在分钟级完成，且 loss 单调下降、PSNR 提升。
- **渲染加速**：单视图渲染经 tile 分块 + 多线程 + SIMD + 零索引器开销后显著变快，并对所有参数可微（一次前向同时产出图像与解析梯度）。
- **精度可控**：提供 fp32 / fp64 / 自动三档精度策略，可逐层对拍校验；关闭 CUDA 时纯 CPU 路径结果一致（容差内）。
- **可诊断、可扩展**：分阶段计时（投影 / 分箱 / 光栅化 / 反向 / Adam / 致密化）、`bench` 与 `verify` 子命令、后端注册表，宿主可替换或新增光栅化实现。
- **产物不变**：仍输出 `training_log.csv`、`reconstruction.ply`、逐视图 PNG。

## 核心特性

1. 解析可微光栅化：tile binning + near→far 排序 + 并行前向 + 一次反向得到全部参数梯度。
2. 混合精度投影：fp32 `TensorF` 批处理相机变换、四元数→旋转、协方差、conic 求逆、sigmoid。
3. SIMD 像素内核：8 宽 `Vector` 二次型、快速 `exp`、三通道融合累加，fp64 累加保精度。
4. SIMD+Parallel 优化器与数据搬运：Adam 单遍 m/v/param、densify 保留动量、零拷贝 SoA 存储。
5. 可注入光栅化后端 + 精度/后端/阈值策略集中管理。
6. 基准与对拍工具链：`bench`、`verify`、`--gpu` / `--gpu-strict`。

## 技术栈

- 语言/运行时：VB.NET，`net10.0`（库）/ `net10.0-windows`（test），沿用现有 `OptionStrict Off`。
- 张量：`Microsoft.VisualBasic.MachineLearning.TensorFlow`
- fp64 `Tensor`（`Data As Double()`，索引器 setter 含 `Interlocked` 版本号 → 热路径禁用）
- fp32 `TensorF`（`Data As Single()`，无运算符，必须显式 `TensorF.computeKernelF.*`）
- 策略层 `MixedPrecisionPolicy` / `MixedPrecisionOps`（`Compute` 命名空间）
- 底层加速：`Microsoft.VisualBasic.Math.SIMD`（`SimdEngine` / `SimdMath` / `SimdReduce` / `SimdParallel` / `SIMDIntrinsics` / `SIMDEnvironment`），均在 Core 程序集，**无需新增引用**。
- 并行：`System.Threading.Tasks.Parallel`（`Parallel.For` / `Parallel.For(Of TLocal)`）。
- GPU：`Microsoft.VisualBasic.Computing.ILCuda.GPUTensor.CudaTensor.Register(...)` → 写入 `Tensor.computeKernel` / `TensorF.computeKernelF`；**仅在 test 工程注册**，库内零编译期依赖。

## 实现思路

### 核心策略

三层递进，按「算法 → 数据布局 → 内核」顺序推进，避免在错误的算法上做微优化：

1. **算法层（收益 4 个数量级）**：中心差分 → 解析反传。现状每高斯 10 参数 × 2 扰动 × 12 视图 = 240 次全量重渲染；解析反传把梯度计算从「O(N·P·V·渲染)」降到「O(V·渲染)」，即每迭代仅需 12 次前向 + 12 次反向。
2. **数据布局层（收益 3~8 倍）**：逐高斯标量 + `Tensor` 索引器 + 二维托管数组 → SoA 连续数组 + 原始 `Data` 直写 + tile 局部缓冲。消除每元素 `Interlocked.Increment`、每高斯 `Double(,)` 分配、委托比较器排序。
3. **内核层（收益 2~6 倍叠加）**：像素内 `Vector(Of Double)` 8 宽二次型/快速 exp/三通道融合；tile 级 `Parallel.For`；Adam 单遍 SIMD；归约走 `SimdReduce.DistanceSquared` / `SimdParallel`。

### 关键决策与权衡

| 决策 | 选择 | 理由 / 权衡 |
| --- | --- | --- |
| 渲染范式 | **tile-based（16×16）** 而非逐高斯 bbox 扫描 | 每 tile 局部分缓冲（16×16×3 + 16×16 透射率）→ 零竞争、缓存友好、可并行、天然支持反向贡献链。代价：需一次 binning（O(N·tiles)），对小 N 有固定开销 → 设 `MinSplatsForTiling` 阈值回落直接路径 |
| 合成顺序 | near→far + `C+=T·α·c; T*=1-α` | 修正现有 far→near 与 front-to-back 公式的矛盾；解析反传基于该正确公式 |
| 协方差 | `Σ_cam = R_cam·Σ_world·R_camᵀ` | 修正丢旋转；`Σ_world = R·diag(s²)·Rᵀ` 保持参数化 |
| 投影精度 | **fp32 `TensorF` 批处理**（默认），保留 fp64 参考实现 | 投影是纯逐元素向量运算，最易命中 GPU；但 `TensorF` 无 in-place API，会产生 N×K 临时分配 → 仅在 `N ≥ TensorPathMinSplats`（默认 8192）或已注册 GPU 时启用，否则用自写 SIMD fp32/fp64 内核，避免小规模倒挂 |
| 像素累加精度 | **fp64 局部缓冲** | 数百个高斯叠加，fp32 透射率连乘误差会累积；符合「其余用 fp64」的要求 |
| 反向梯度归约 | `Parallel.For(Of TLocal)` 线程本地 `Double()` 梯度缓冲 + 末尾归约 | 绝不用逐元素 `Interlocked.Add`（会成为新瓶颈）；设 `MaxThreadGradientMB` 上限，超限自动退化为 tile 分组串行归约 |
| 排序 | `Array.Sort(keys As Double(), items As Integer())` | 消除 `Comparison` 委托的间接调用与装箱；可选 256 桶桶排序 |
| `exp` | `ExpMode.Fast`（2^k 分解 + FMA 多项式，误差 ~1e-6）/ `ExpMode.Exact` | `SimdMath.Exp` 是标量循环，无硬件指令；Fast 用于前向内循环，Exact 用于 verify |
| 后端 | `ISplatRasterBackend` + `SplatBackends` 注册表 | 宿主可注入 CUDA 光栅化内核而不改库；默认 `CpuTileRasterBackend` |
| API | 门面保留 `SplatRenderer.Render` / `ComputeMSE` / `ComputePSNR` 语义，其余重构 | 用户已允许破坏性变更，但保留门面可让 test 与外部调用平滑迁移 |


### 性能与复杂度

| 阶段 | 现状 | 优化后 | 复杂度 |
| --- | --- | --- | --- |
| 单次渲染 | O(N·bbox + N·P 索引器写) | O(N·logN + Σ tile 内高斯·256) | 主项从「每像素 3 次 Interlocked」降为 0 |
| 单次梯度 | O(N·10·2·V·渲染) ≈ 10⁴ × 渲染 | O(V·(前向 + 反向)) | **≈1e4×** |
| Adam | O(N·P) × (2 索引器 + `Pow`) | O(N·P) 单遍 SIMD + 1 次 `MarkHostModified` | 内存带宽受限，≈3–5× |
| MSE | O(HWC) 标量三重循环 | `SimdReduce.DistanceSquared` 单次归约 | ≈10× |
| 投影（N=2000） | O(N) 标量 + 每高斯 3 个二维数组分配 | O(N·K) SIMD 融合或 fp32 tensor（GPU 可命中） | 分配数 → 0 |


不引入任何 `O(N²)` 结构；反向梯度内存为 `threads × N × 14 × 8` 字节。

## 架构设计

```mermaid
graph TD
    A[SplatTrainer<br/>训练循环/进度/评估节流] --> B[GradientCalculator<br/>Analytic | FiniteDifference]
    A --> C[AdamOptimizer<br/>SIMD 单遍]
    A --> D[DensityController<br/>densify/prune]
    B --> E[SplatRenderer<br/>门面]
    E --> F[SplatProjection<br/>fp32 TensorF / fp64 SIMD]
    E --> G[SplatBackends<br/>注册表]
    G --> H[ISplatRasterBackend]
    H --> I[CpuTileRasterBackend<br/>binning + Parallel + SIMD]
    H --> J[宿主自定义<br/>CudaRasterBackend]
    F --> K[TensorF.computeKernelF<br/>CUDA-F32 或 SIMDTensorF]
    C --> L[GaussianModel<br/>SoA Double + fp32 镜像]
    D --> L
    I --> L
```

### 数据流

```
GaussianModel(SoA Double[]) --(Version 变化时一次性转换)--> fp32 SoA 镜像
   → SplatProjection：cam = pos·Rᵀ + t；Σ_world；Σ_cam；Σ'；conic；opacity；radius
   → cull（depth/opacity/det/bbox）→ near→far 深度排序（键值排序）
   → tile binning（计数 → 前缀和 → 填充）
   → CpuTileRasterBackend 前向（Parallel over tiles，fp64 局部缓冲）→ image
   → 反传（同一 binning，一次 far→near 扫描，线程本地梯度 → 归约）→ SplatGradients
   → AdamOptimizer.StepModel（SIMD 单遍）→ 参数更新 → model.Version++
```

## 目录结构

```
GaussianSplatting3D/
├── GaussianSplatting/
│   ├── GaussianSplatting.vbproj              # 不改引用（禁止新增 ILCudaTensor/ILCuda）
│   ├── GaussianModel.vb                      # [MODIFY] SoA Double 存储为主体，Tensor 仅零拷贝视图；梯度/Adam 缓冲复用（Array.Clear 而非重新分配）；AddGaussians/RemoveGaussians 用 Array.Copy 整块搬运并重排保留梯度与 Adam 状态（修复 densify 丢动量）；新增 Version 计数器与 fp32 镜像缓存；FromPointCloud/FromRandom 改 Data 直写
│   ├── Camera.vb                             # [MODIFY] 增加只读缓存视图：R 的 9 标量、T、WorldToCamera 的批量/内联版本，供投影内核零索引器访问；JSON 加载逻辑保持不变
│   ├── SplatRenderer.vb                      # [MODIFY] 门面：Render(model,camera) 保持签名、内部走 SplatBackends；新增 Render(model,views,options) 多视图并行、RenderAndBackward；ComputeMSE 改 SimdReduce.DistanceSquared，ComputePSNR 复用同一 MSE（避免重复渲染/重复归约）
│   ├── GradientCalculator.vb                 # [MODIFY] Mode = Analytic（默认）/ FiniteDifference；Analytic 走一次渲染+反传累积；FiniteDifference 保留原实现并加 SampleCount 限制，仅供 --legacy 对拍
│   ├── Optimizer.vb                          # [MODIFY] Adam：IncrementStep 预计算 bc1/bc2；单遍 Vector(Of Double) 同时更新 m/v/param；Data 原地 + 末尾一次 MarkHostModified；>256K 元素才 Parallel；StepModel 参数组级并行
│   ├── ImageLoader.vb                        # [MODIFY] Load/Save 改 Data 直写 + Parallel.For 分行；PointCloudIO 语义与格式不变（仅去索引器）
│   ├── Rendering/SplatProjection.vb          # [NEW] 逐高斯屏幕投影：fp32 TensorF 批量路径（相机变换/四元数→R/Σ_world/Σ_cam/Σ'/conic/sigmoid/radius）+ fp64 SIMD 参考路径；剔除与紧凑活跃索引；输出 SplatScreenData（SoA）
│   ├── Rendering/SplatRasterizer.vb          # [NEW] 深度键值排序、tile binning（tileOffsets/tileGaussians）、前向光栅化与解析反传的共享几何；贡献链构建
│   ├── Rendering/SplatRasterBackend.vb       # [NEW] ISplatRasterBackend 接口、SplatBackends 注册表、SplatRasterRequest/SplatGradients 契约、CpuTileRasterBackend 默认实现
│   ├── Rendering/SplatPixelKernels.vb        # [NEW] Vector(Of Double) 8 宽内核：二次型 m²、Fast/Exact exp、三通道融合累加、稀疏 tile 行遍历
│   ├── Training/SplatTrainer.vb              # [NEW] 训练循环从 test 迁入：LRScheduler、Adam、DensityController、评估节流、进度/日志回调、PerfTrace 分阶段计时
│   ├── Training/TrainingOptions.vb           # [NEW] 迭代数/LR/densify 区间/视图采样/精度策略(ProjectionPrecision)/ExpMode/并行度/tile 尺寸
│   ├── Diagnostics/PerfTrace.vb              # [NEW] Stopwatch 分阶段累计（projection/binning/rasterize/backward/adam/densify/mse）+ 汇总输出
│   ├── NamespaceDoc.vb                       # [MODIFY] 补新子命名空间说明
│   └── README.md                             # [MODIFY] 增补性能架构、精度策略表、ISplatRasterBackend 注入示例、bench/verify 用法
└── test/
    ├── test.vbproj                           # [MODIFY] 仅当需要时才加（当前引用已足够；SDK 自动包含新 .vb）
    ├── Program.vb                            # [MODIFY] Main 只做 CLI 分派；训练循环改调 SplatTrainer；新增 --gpu/--no-gpu/--gpu-strict
    ├── Commands/TrainCommand.vb              # [NEW] train：加载相机/图像、InitializeModel（Data 直写）、调 SplatTrainer、保存 PLY/PNG/CSV
    ├── Commands/BenchCommand.vb              # [NEW] bench [--gaussians --views --iterations --legacy --gpu]：分阶段计时，输出 output/bench_*.csv
    ├── Commands/VerifyCommand.vb             # [NEW] verify：解析梯度 vs 差分、fp32 投影 vs fp64、新旧前向、GPU 开/关四组对拍
    ├── Backends/CudaProjectionBackend.vb     # [NEW] ISplatRasterBackend 注入示例：fp32 tensor 投影 + 委托 CPU 光栅化，证明接口可插拔
    └── GpuBootstrap.vb                       # [NEW] CudaTensor.Register 封装（对齐 UNet/test/Program.vb 的 TryEnableGpu 模式），打印 computeKernel/computeKernelF 名称与 DescribeKernels()
```

## 关键代码结构

```
Namespace Rendering

    ''' <summary>光栅化请求：调用方已完成投影/剔除/排序/tile binning，后端只做像素合成。</summary>
    Public NotInheritable Class SplatRasterRequest
        Public Property Width As Integer
        Public Property Height As Integer
        Public Property TileSize As Integer
        Public Property Screen As SplatScreenData          ' SoA：meanX/meanY/conicA/conicB/conicC/opacity/colorRGB/radius
        Public Property DepthOrder As Integer()            ' near -> far
        Public Property TileOffsets As Integer()           ' 长度 tileCount + 1
        Public Property TileGaussians As Integer()         ' 每 tile 的高斯槽位（深度有序）
    End Class

    ''' <summary>解析梯度累加目标（长度 = 模型高斯数，按参数分组）。</summary>
    Public NotInheritable Class SplatGradients
        Public Property Positions As Double()
        Public Property Scales As Double()
        Public Property Rotations As Double()
        Public Property Colors As Double()
        Public Property Opacities As Double()
    End Class

    ''' <summary>可注入光栅化后端。默认实现为 CpuTileRasterBackend。</summary>
    Public Interface ISplatRasterBackend
        ReadOnly Property Name As String
        Function IsAvailable() As Boolean
        ''' <summary>前向：写满 image(H*W*3 行优先) 与 transmittance(H*W)。</summary>
        Function Rasterize(request As SplatRasterRequest, image As Double(), transmittance As Double()) As Boolean
        ''' <summary>反向：向 grads 累加 dL/d(param)。</summary>
        Function RasterizeBackward(request As SplatRasterRequest, imageGradient As Double(), grads As SplatGradients) As Boolean
    End Interface

    Public NotInheritable Class SplatBackends
        Public Shared Property [Default] As ISplatRasterBackend     ' 初始为 CpuTileRasterBackend 实例
        Public Shared Sub Register(backend As ISplatRasterBackend, Optional makeDefault As Boolean = True)
        Public Shared Function Available() As ISplatRasterBackend()
    End Class

End Namespace
```

## 实施要点

**精度与一致性**

- 投影双精度参考路径必须与 fp32 路径共享同一套公式与常量（含抗锯齿项 `eps = 0.3*0.3` 加到 `Σ'` 的 a/c），仅数据类型不同，便于逐位对拍。
- `Σ_world = R·diag(s²)·Rᵀ` 只算 6 个独立分量（对称），再由 `R_cam` 做相似变换得到 `Σ_cam` 的 6 个分量。
- 四元数归一化保护：`n < 1e-12` 退回单位矩阵；反传时乘以归一化链式因子 `(I - qqᵀ/|q|²)/|q|`。
- `α` 上限 0.99 时将该处 `dL/dα` 置零（截断不可导）。
- `GradientCalculator.Mode = FiniteDifference` 时，抽样校验只比对被采样高斯的梯度，且用 `Exact` exp 与 fp64 投影，避免容差误判。

**性能**

- 热循环一律使用 `Data` 原始数组 + 局部 `Span`/下标运算；`Tensor`/`TensorF` 索引器与运算符在像素循环内**完全禁用**；批量写入后调一次 `MarkHostModified()`。
- tile 内循环顺序：`for tileGauss in tileList: for y: for x:`，`dx/dy` 用增量递推（`dx2 = dx1 + 2`），避免乘法。
- `Parallel.For` 只在 `tileCount >= 2 * Environment.ProcessorCount` 时启用；过小直接串行，避免调度开销。
- 反向归约：`Parallel.For(Of TLocal)` 的 `localFinally` 用 `SimdEngine.AddInPlace` 合并；线程本地缓冲预分配并按线程复用（避免每视图 GC）。
- `TensorF` 路径产生的临时张量数量要显式控制（把 `cam`、`Σ`、`conic` 的中间量就地覆写），并在 `TensorPathMinSplats` 以下回落到自写 SIMD 内核。
- 不做无关重构：`PointCloudIO` 的 PLY 格式、`Camera.LoadFromJson` 的 JSON 契约、`LRScheduler` 语义保持不变。

**可观测性**

- `PerfTrace` 只在 `TrainingOptions.EnableProfiling=True` 时记录（`Stopwatch` 开销可忽略但不该出现在默认路径的分支预测里）。
- 不新增日志框架；沿用 `Console.WriteLine` 与现有 `Tqdm`。诊断输出只打印后端名、阈值、阶段耗时与高斯数，不打印张量内容。
- `bench` 必须输出「是否真的走了 GPU」（`TensorF.computeKernelF.Name`、`DescribeKernels()`、`MixedPrecisionDiagnostics` 计数）。

**风险与降级**

- CUDA 注册失败/内核翻译失败 → 自动回落 CPU（既有语义），`verify` 需覆盖该分支。
- 解析梯度若数值异常（NaN）→ `SplatTrainer` 跳过该步并打印首个 NaN 参数与前 5 个失败高斯索引，不静默继续训练。
- `ISplatRasterBackend.Rasterize` 返回 `False` 视为「不可用」，调用方自动改用 `CpuTileRasterBackend`，不抛异常。
- 若 `N` 极大导致线程本地梯度缓冲超限 → 自动降为按 tile 组串行归约，并打印切换原因。

**VB.NET 陷阱（必须遵守）**

- 热路径禁止 `Tensor` 索引器 setter（每写一次 `Interlocked.Increment`）。
- `Tensor` 的 `*` 是 Hadamard 而非矩阵乘。
- `SimdEngine`/`SimdMath`/`SimdReduce` 只有 `T()` 重载，无 `Span`；二元内核不校验等长。
- `SimdMath.Exp/Log` 是标量循环，不要以为已向量化。
- `Module` 成员不能写 `Shared`；`Step`/`Call`/`Next`/`Error`/`Sub`/`Index`/`Failure` 为保留字；局部变量不得与所在函数/属性同名（大小写不敏感）。
- `OptionStrict Off` 下混合精度需显式 `CSng`/`CDbl`，否则静默窄化。
- 禁止给库新增 `ILCudaTensor`/`ILCuda` 引用。

## Agent Extensions

### SubAgent

- **code-explorer**
- Purpose: 重构期间对仓库内既有 API 做定点核实（`ITensorCompute`/`ITensorComputeF` 算子清单、`SIMDTensor` 内部实现、`Camera`/`ImageLoader` 调用点、`ILCuda` 内核参数限制），避免凭记忆编造签名。
- Expected outcome: 每次改动前拿到「绝对路径 + 行号 + 精确签名」的证据；确保新增调用点全部落在真实存在的 API 上，编译一次通过。
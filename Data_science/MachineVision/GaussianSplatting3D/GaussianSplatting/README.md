# 3D 高斯泼溅：场景重建与渲染

## 引言

传统的 3D 重建走「网格」路线：从多视角图像重建三角形网格，再贴图渲染。它的问题是——**细小结构（毛发、树叶、半透明物体）几乎无法网格化**。

**3D Gaussian Splatting** 换了一条路：不重建网格，而是用**一堆各向异性的高斯椭球**表示场景：

- 每个高斯有自己的位置、尺度、旋转、不透明度和颜色；
- 渲染就是把所有高斯按投影「泼溅」到图像平面（splatting）；
- 训练就是通过图像重建误差反向优化每个高斯的参数。

好处是：**渲染极快**（可实时）、**细节保留好**（粒子级表达），代价是表示形式不是传统网格。

## 核心能力与关键类型

全部类型位于根命名空间 `Microsoft.VisualBasic.MachineVision.GaussianSplatting3D`：

| 类型 | 职责 |
|---|---|
| `GaussianModel` | 场景表示：扁平 `Double()` 存储的位置/尺度/旋转/不透明度/颜色 + 梯度 + Adam 状态 |
| `ImageLoader` | 多视角图像读写（内置 PNG 编解码器，不依赖 GDI+） |
| `Camera` | 针孔相机模型（内参 + 外参） |
| `SplatProjection` | 逐高斯屏幕投影：相机变换 → 协方差 → conic → 包围盒（SoA 输出） |
| `SplatRasterizer` | 可见性剔除 → 深度升序排序 → tile 分箱 |
| `ISplatRasterBackend` / `SplatBackends` | 可注入的光栅化后端契约与注册表（默认 `CpuTileRasterBackend`） |
| `CpuTileRasterBackend` | 默认后端：tile 分块 + 多线程 + 连续内存的前向合成与解析反向 |
| `SplatProjectionGradient` | 解析反传的投影链：`dL/dconic, dL/dmean` → `dL/dpos, dL/dscale, dL/drot` |
| `SplatRenderer` | 渲染门面：`Render` / `RenderAll` / `ComputeMSE` / `MseAndGradient` |
| `GradientCalculator` | 梯度：解析反传（默认）或中心差分（对拍基线） |
| `AdamOptimizer` / `LRScheduler` | SIMD 单遍 Adam + 余弦退火 |
| `DensityController` | 自适应密度控制（克隆 / 分裂 / 剪枝，保留既有动量） |
| `SplatTrainer` | 训练循环（进度回调、评估节流、分阶段计时） |
| `PerfTrace` | 分阶段耗时统计（projection / binning / rasterize / backward / optimize / densify / loss） |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.MachineVision.GaussianSplatting3D

' 1. 载入多视角图像并建立相机
Dim scene = Camera.LoadFromJson("./cameras.json")
Dim views = ImageLoader.LoadAll("./captures", scene.Cameras.Count)

' 2. 初始化高斯场景
Dim model = GaussianModel.FromPointCloud(points, colors, n, 0.05F)

' 3. 训练
Dim trainer As New SplatTrainer(model, scene.Cameras, views, New TrainingOptions With {.Iterations = 500})
Dim result = trainer.Run(Sub(p) Console.WriteLine(p.ToString()))

' 4. 渲染新视角
Dim image = SplatRenderer.Render(model, newCamera)

' 5. 单步梯度（需要自己控制循环时）
Dim grads As New GradientCalculator()
Dim loss = grads.ComputeLossAndGradients(model, scene.Cameras, views)
Call New AdamOptimizer(0.01).StepModel(model)
```

## 性能架构

三层递进，顺序很重要 —— 在错误的算法上做微优化是纯浪费：

### 1. 算法层：中心差分 → 解析反传

旧实现对每个高斯的 10 个参数做中心差分，每次扰动都要**重渲染全部视图**：
`N × 10 × 2 × V` 次渲染。N=2000、V=12 时单步梯度是 48 万次渲染，实际上不可完成。

```
L(θ+ε) 与 L(θ−ε) 各一次全量渲染  →  一次前向 + 一次反向
```

实测（320×240、12 视图、2000 高斯，Debug/x64）：

| 项目 | 耗时 |
|---|---|
| 单一视图渲染 | 约 7 ms |
| 单视图前向 + 反向 | 约 22 ms |
| 完整一步梯度（12 视图） | **约 0.1~0.2 s** |
| 旧实现一步梯度（外推） | **约 1900 s** |

即训练步级别的加速比在 **10⁴ 量级**（`test bench` 会现场测量并外推旧实现）。

`test train --iterations 100 --profile` 的实测阶段占比（Release，320×240，12 视图，2000 高斯）：

| 阶段 | 占比 |
|---|---|
| 前向光栅化（rasterize） | 31.8% |
| 解析反向（backward） | 55.0% |
| 损失（loss） | 9.0% |
| 投影（projection） | 2.9% |
| 分箱（binning） | 1.1% |
| Adam（optimize） | 0.1% |
| 致密化（densify） | 0.0% |

即：**87% 的时间在像素合成与其反向**，投影/分箱/优化器加起来不到 5%
—— 这也是后续要继续优化时唯一值得投入的方向（把这两段换成 GPU 内核）。

### 2. 数据布局层

- 参数、梯度、Adam 状态全部以扁平 `Double()` 为唯一真实存储；`Tensor` 只是零拷贝视图。
  原因：`Tensor` 的索引器 setter **每次写入都会 `Interlocked.Increment` 版本号**，
  在逐像素循环里是纯粹的浪费。
- 逐高斯量用 SoA 数组一次性算好（`SplatScreenData`），像素循环不再触碰模型对象。
- 排序用 `Array.Sort(keys, items)` 键值对，而不是 `Comparison(Of T)` 委托。
- densify 用整块 `Array.Copy` 搬运，并**保留既有 Adam 动量**（旧实现每次致密化都把优化器清零）。

### 3. 内核层

- **tile 分块（16×16）**：每个 tile 拥有独立的颜色 / 透射率 / 前缀缓冲，
  并行时无写冲突，不需要任何原子操作，且缓存友好。
- **二次型增量递推**：`q(x+1) = q(x) + dq`、`dq += 2a`，内层循环无乘法。
- **快速指数**：`ExpMode.Fast`（2^k 分解 + 6 阶多项式，相对误差 ~1e-7）比 `Math.Exp` 快 2~3 倍；
  `ExpMode.Exact` 用于逐位对拍。
- **并行策略**：tile 数 ≥ 8 时按线程数切块跑 `Parallel.For`；反向传播为每线程分配本地梯度缓冲
  （受 `ThreadGradientBudgetMB` 限制，超限自动降为串行归约），最后用 `SimdEngine.AddInPlace` 合并。
- **Adam**：`Vector(Of Double)` 8 宽单遍同时更新 `m / v / param`，偏差校正在 `IncrementStep` 里算一次。
- **损失**：`MseAndGradient` 一次遍历同时给出 MSE 与 `dL/dC`，并行分块 + 标量展开。

### 精度策略

| 环节 | 精度 | 说明 |
|---|---|---|
| 投影（相机变换 / 协方差 / conic） | fp64 | O(N) 逐高斯稠密运算，比像素合成低 3 个数量级，实测占比 < 5% |
| 像素合成 / 透射率累积 | fp64 | 数百个高斯连乘，fp32 会累积透射率误差 |
| 解析梯度累加 | fp64 | 同上 |
| Adam | fp64 + SIMD | 动量对舍入敏感 |
| 图像空间损失 / 像素梯度 | **fp32 张量后端可用** | 唯一"纯逐元素 + 数据量大（H·W·3）+ 只需 3~4 个算子"的环节，见下 |

`SplatRenderOptions.UseTensorLossBackend`（默认 `True`）：当宿主注册了 CUDA 张量后端、
且元素数达到 `TensorLossMinElements`（默认 200 万 ≈ 1000×700×3）时，
`SplatRenderer.ComputeMSE` 走 `TensorF.computeKernelF.MseLoss`（一次调度到显卡）；
否则落到并行 SIMD 内核。

阈值为什么这么保守 —— 实测（RTX A4000，CUDA-F32）：320×240 的图像（23 万元素）
走 GPU 时单次 MSE 从 **0.067 ms 退化到 1.096 ms**，显存往返（1.8 MB fp64 + 0.9 MB fp32）
完全压倒了内核时间。也就说"能上 GPU"和"该上 GPU"是两件事，阈值必须由实测决定。

> 为什么逐高斯投影不交给 fp32 张量：它每个高斯只有约 200 flops 且完全可向量化，
> 在 N=2×10⁵ 时仍只占单帧耗时的百分之几；GPU 的调度与传输开销会把这些收益吃掉。
> 真正的热点是像素合成，而它是 per-pixel 顺序执行的 gather/accumulate，不适合
> 现有张量算子集 —— 这也是提供 `ISplatRasterBackend` 的原因。

### 接入 GPU

库本身**不引用** `ILCudaTensor` / `ILCuda`。宿主只需在启动时注册一次后端：

```vbnet
Imports gpu = Microsoft.VisualBasic.Computing.ILCuda.GPUTensor
Imports ILCudaRuntime = Microsoft.VisualBasic.Computing.ILCuda.Runtime

If gpu.CudaTensor.Register(New ILCudaRuntime.EngineOptions()) Then
    ' fp64: Tensor.computeKernel  → CUDA
    ' fp32: TensorF.computeKernelF → CUDA-F32
End If
```

之后库内所有经张量后端的批量运算自动落到 GPU；注册失败时后端保持 SIMD，无需任何调用方改动。

### 注入自定义光栅化后端

```vbnet
Public Class MyGpuRasterBackend
    Implements ISplatRasterBackend

    Public ReadOnly Property Name As String Implements ISplatRasterBackend.Name
        Get
            Return "MY-GPU"
        End Get
    End Property

    Public Function IsAvailable() As Boolean Implements ISplatRasterBackend.IsAvailable
        Return True
    End Function

    Public Function Rasterize(request As SplatRasterRequest, image As Double(),
                              transmittance As Double()) As Boolean Implements ISplatRasterBackend.Rasterize
        ' 返回 False 表示"本次不可用"，调用方会自动回退到 CPU 后端
        Return False
    End Function

    Public Function RasterizeBackward(request As SplatRasterRequest, image As Double(), imageGradient As Double(),
                                      grads As SplatGradients) As Boolean Implements ISplatRasterBackend.RasterizeBackward
        Return False
    End Function
End Class

SplatBackends.Register(New MyGpuRasterBackend(), makeDefault:=True)
```

契约要点（自定义后端必须遵守）：

1. `SplatRasterRequest` 里的 `TileOffsets` / `TileGaussians` 已经**按深度升序（near → far）**排好，
   每个 tile 的列表天然有序；后端不要再排序。
2. tile 内局部缓冲的索引是 `off = (y − tileY0) · tileSize + (x − tileX0)`。
3. **前向与反向必须使用完全相同的跳过条件**（`q > σ²`、`alpha < 1e-6`、`T < 1e-6`），
   否则 `S_total` 与前缀贡献和对不上，梯度会系统性偏移。
4. 反向只需累加 `Colors` / `Opacities` / `Conic` / `Means` 四组中间量，
   随后由 `SplatProjectionGradient.Chain` 完成"屏幕空间 → 参数"的链式回传。

## 相对旧实现修正的两处数学缺陷

1. **合成顺序**：旧实现按深度 **far → near** 排序，却使用 front-to-back 公式
   `C += T·α·c; T *= (1−α)`，两者自相矛盾（远处先叠加、近处被当成其后方的遮挡）。
   现改为 **near → far**，与 3DGS 参考实现一致。
2. **协方差旋转**：旧实现直接把世界坐标系下的 `Σ` 当成相机坐标系使用（等价于假设 `R_cam = I`）。
   现补上 `Σ_cam = R_cam · Σ_world · R_camᵀ`（对应参考实现 `computeCov2D` 的 `W·Σ·Wᵀ`）。

两条都影响输出结果（新结果更正确）。解析反传必须建立在正确公式之上，因此这次一并修正。

## 命令与对拍（测试宿主）

```text
test train   --iterations 500 [--gaussians 2000] [--eval-every 10] [--views N]
             [--legacy]（中心差分） [--profile]（分阶段计时） [--no-densify]
test bench   [--gaussians N] [--views N] [--repeat N] [--tile N] [--serial] [--exact]
test verify  [--gaussians N] [--sample N]
test info

全局：--gpu / --gpu-strict / --no-gpu
```

`verify` 的四组对拍（全部通过才返回 0）：

| 检查 | 方法 | 实测 |
|---|---|---|
| 协方差链 | `Σ_world`/`Σ_cam` 与独立的朴素 3×3 矩阵乘比对 | 0（逐位一致） |
| tile 前向 | 与"逐像素 gather"参考实现比对（`ExpMode.Exact`） | ~4e-16 |
| 解析梯度 | 与中心差分比对（抽样高斯 × 14 参数，6σ 抹平截断） | 步长 1e-2→2e-3 时 7.4e-2→1.3e-2（二次收敛） |
| 损失精度 / 后端回退 | fp64 SIMD vs fp32 张量；注册不可用后端后结果不变 | 3.6e-9 / 0 |

## 实现要点

- **为什么用椭球而不是球**：真实表面在不同方向上曲率不同；各向异性（可旋转的椭球）能贴合平面与细长结构，而球形只能表示"一团"。这是高斯泼溅能表达细节的根本原因。
- **可微光栅化**：渲染过程必须对参数可微，否则无法反传。`SplatRenderer` 前向、`ISplatRasterBackend.RasterizeBackward` 反向，`SplatProjectionGradient` 负责投影链。
- **Adam 优化的必要性**：高斯的位置、尺度、旋转与不透明度量纲差异巨大，普通 SGD 很难同时收敛；Adam 的逐参数自适应学习率在这里几乎是必需的。
- **与 `Landscape` 的分工**：`Landscape` 处理**网格与体素**（STL/OBJ/glTF → 体素 / SDF），本包处理**基于粒子的场景表示**；两者面向不同的下游任务，互为补充。

## 包信息

- Assembly：`Microsoft.VisualBasic.MachineVision.GaussianSplatting3D`
- TargetFramework：`net10.0`
- Tags：`scibasic;gaussian-splatting;3d-reconstruction;neural-rendering;computer-vision;adam-optimizer`
- 许可：GPL-3.0-or-later

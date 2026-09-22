# ILCudaTensor：sciBASIC# 的 GPU 张量运行时

## 一、为什么需要它

在 .NET 生态里做深度学习或大规模数值计算，最大的痛点往往不是算法本身，而是**如何把计算稳定地搬到 GPU 上**：既不想为部署引入一堆原生依赖，又希望内核能够按需编译、显存能够被精确管理。

`ILCudaTensor` 是 `sciBASIC#` 计算栈中的 GPU 张量层。它建立在 `ILCuda` 提供的 **CUDA Driver API + NVRTC 运行时编译**能力之上，向上提供两件东西：

1. **设备驻留的张量类型**（`CudaTensor`），以及稠密 / CSR 稀疏两种显存存储；
2. **一批开箱即用的 double 精度 CUDA 内核**（GEMM、softmax、归约、卷积、池化、稀疏 × 稠密）。

最关键的体验是：**只要一次 `Register()`，整个 TensorFlow 风格的 `Tensor` / `Math` / `nn` 运算就会自动切换到 GPU 后端**，业务代码几乎无需改动。

## 二、设计目标

- **全 double 精度**：GPU 侧算子全部以 `double` 实现，不再有 `Double <-> Single` 的降精度桥接。
- **无原生依赖安装**：CUDA 内核以**内嵌资源**形式随程序集分发，运行时经 NVRTC 现场编译。
- **算子永不失效**：`CudaTensor` 继承自 TensorFlow 的标量兜底实现 `TensorComputeBase`，只重写「GPU 有对应内核」的算子；其余算子（比较、乘积归约、`Apply` 等）自动继承 CPU 实现，保证任何后端切换都不会让某个算子失效。
- **显存不泄漏**：所有显存缓冲由 `DeviceCache` 显式管理，规避 ILCuda 无终结器所导致的显存回收问题。

## 三、架构与命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.Computing.ILCuda.GPUTensor` | 本包：GPU 张量运行时（`CudaTensor`、`DeviceCache`、`SparseCsrCache`、`DoubleKernels`、`TensorKernels`） |
| `Microsoft.VisualBasic.Computing.ILCuda.Runtime` | （ILCuda）设备 / 上下文 / 流 / 模块 / 显存对象与 NVRTC 编译 |
| `Microsoft.VisualBasic.Computing.ILCuda.Kernels` | （ILCuda）内嵌 `.cu` 内核源码的目录管理 |
| `Microsoft.VisualBasic.Computing.ILCuda.IL2Cuda` | （ILCuda）IL → CUDA C 源码翻译器 |

### 内核的组织方式

| 内核来源 | 覆盖算子 | 实现文件 |
|---|---|---|
| P2 · IL2Cuda 生成 | 逐元素运算、转置 | `DoubleKernels.vb` |
| P3 · 手写内核 | 末轴 softmax / log-softmax、末轴归约、arg 极值 | `Kernels/tensor.cu` |
| 手写分块 | 矩阵乘（GEMM） | `Kernels/gemm.cu` |
| 手写 | 卷积 / 池化（NHWC，前向 + 反向） | `Kernels/conv.cu`、`Kernels/pool.cu` |
| 手写 | CSR 稀疏 × 稠密（行并行 + `atomicAdd(double)`） | `Kernels/spmm.cu` |
| 复用 | 全局归约（整张量视为一行） | 复用末轴归约内核 |

## 四、关键类型与 API

### `CudaTensor`

`ITensorCompute` 的 CUDA GPU 实现，继承 `TensorComputeBase`，实现 `IDisposable`。

- **注册 / 注销**：`CudaTensor.Register()` / `Unregister()`
- **计算属性**：`Engine`、`Current`、`OnGpu`、`Name`、`LastError`、`KernelFailures`
- **调优参数**：`MinGpuElements`（默认 4096，低于该元素数走 CPU 兜底）、`MinGemmElements`、`MaxStageBlocks`、`DefaultCacheBytes`（默认 1 GiB）、`RowBlockSize` / `StageBlockSize`（均为 256，须与 `tensor.cu` 中的常量保持一致）
- **逐元素算子**：`Add`、`AddScalar`、`Subtract`、`Multiply`、`MultiplyScalar`、`Divide`、`DivideScalar`、`Negate`、`Abs`、`Sqrt`、`Square`、`Exp`、`Log`、`Sin`、`Cos`、`Pow`、`Reciprocal`、`Clip`
- **激活函数**：`Relu`、`LeakyRelu`、`Elu`、`Gelu`、`Swish`、`Sigmoid`、`Tanh`
- **线性代数**：`MatMul`、`Transpose`
- **归一化**：`Softmax`、`LogSoftmax`、`RowSoftmax`
- **归约与统计**：`Sum`、`SumAll`、`Mean`、`MeanAll`、`Max`、`Min`、`ArgMax`、`ArgMin`、`RowReduce`、`ReduceGlobal`
- **内部**：`EwUnary`、`EwBinary`、`LaunchRow`、`IsLastAxis`、`ReducedShape`

### `DeviceCache` / `SparseCsrCache`

显存驻留缓存。`DeviceCache` 依据 `HostComparer` 复用主机缓冲对应的设备缓冲，避免重复分配；`SparseCsrCache` 额外维护 CSR 结构（行指针 / 列索引 / 值）的设备副本。

### `DoubleKernels` / `TensorKernels`

- `DoubleKernels`、`DoubleKernelRegistry` —— 由 IL2Cuda 生成的逐元素与转置内核及其注册表；
- `TensorKernelNames` —— 手写内核（softmax / GEMM / 卷积 / 池化 / 稀疏）的入口符号名。

## 五、快速上手

```vbnet
Imports Microsoft.VisualBasic.Computing.ILCuda.GPUTensor
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

' 1. 注册 GPU 后端：成功之后，
'    所有 Tensor / Math / nn 的运算都会自动走 GPU
If CudaTensor.Register() Then
    Console.WriteLine("CUDA backend enabled")
End If

' 2. 像平时一样编写代码，算子会自动分发到 GPU
Dim a As Tensor = Tensor.RandomNormal(1024, 1024)
Dim b As Tensor = Tensor.RandomNormal(1024, 1024)
Dim c As Tensor = a.MatMul(b)

' 3. 结束时注销
CudaTensor.Unregister()
```

> 提示：`Register()` 返回 `False` 通常意味着当前环境缺少可用的 CUDA 驱动或设备；此时所有算子会自动回退到 CPU 兜底实现，程序不会崩溃。

## 六、与 sciBASIC# 生态的关系

本节说明本包在计算栈中的位置：

- **向下依赖**：`ILCuda`（Driver API + NVRTC + 内核目录）、`TensorFlow`（标量兜底基类与张量类型）、`Math`、`Microsoft.VisualBasic.Core`；
- **向上服务**：需要 GPU 加速的深度学习与数值工作负载。例如脉冲神经网络（`SNN`）在加载 FlyWire 等真实连接组时，其核心算子正是本包提供的 CSR 稀疏 × 稠密内核。

## 七、性能与实现要点

- **两段式全局归约**：阶段一以 `MaxStageBlocks`（默认 1024）个 block 配合 grid-stride 处理任意长度，阶段二合并部分和，避免超长张量下的 block 数量爆炸。
- **CPU 兜底阈值**：`MinGpuElements = 4096`，小张量直接在 CPU 上计算，避免「拷贝到显存比计算本身还贵」。
- **GEMM 分块**：手写分块 GEMM 针对 double 精度优化，`MinGemmElements` 控制 GPU / CPU 切换阈值。
- **最后一轴语义**：`IsLastAxis` 判定是否可直接使用行内核；否则先降维再归约。

## 八、构建与打包

- 平台：**仅 x64**（`Platforms` 固定包含 x64，AnyCPU 平台亦产出 x64 程序集）；
- `Kernels/*.cu` 通过 `EmbeddedResource` 随程序集分发，运行时由 `CudaTensor.Register` 经 `KernelSources.Register` 注入 NVRTC 编译单元；
- 目标框架：`net10.0`；
- `GenerateDocumentationFile` / `GeneratePackageOnBuild` 均已开启，随包附带 XML 文档与符号包（`snupkg`）。

## 九、许可证

GPL-3.0-or-later

# ILCuda —— 纯 Interop 的 VB.NET CUDA 计算框架 + demo

不引用**任何第三方 NuGet 程序包**，只用 .NET 基类库的 `System.Runtime.InteropServices`
直接 P/Invoke `nvcuda.dll`（CUDA Driver API）与 `nvrtc64_*.dll`（NVRTC 运行时编译）。

解决方案现在分成两层：

| 工程 | 类型 | 职责 |
| --- | --- | --- |
| `ILCuda.vbproj` | Library | **CUDA 计算框架**：设备/上下文/流、显存与页锁定内存、NVRTC 多通道内核编译、内核注册表与启动规划、通用归约与线性代数内核。不依赖 `Console`。 |
| `test\test.vbproj` | Exe | **demo**：矩阵行间皮尔逊相关系数矩阵 + 欧氏距离矩阵，引用框架并运行。 |

## 目录结构

```
ILCuda\
├── ILCuda.vbproj               # net10.0 / x64 框架库，零 NuGet 依赖
├── Interop\
│   ├── CudaDriverApi.vb        # nvcuda.dll 的全部 DllImport 与 Check() 包装
│   ├── NvrtcApi.vb             # NVRTC 动态绑定（NativeLibrary + GetDelegateForFunctionPointer）
│   ├── NvrtcLibrary.vb         # 已绑定函数入口的 NVRTC 实例
│   └── CUresult\               # CUresult / CUdevice_attribute / nvrtcResult / CudaException
├── Runtime\
│   ├── CudaDevice.vb           # 设备枚举与属性（计算能力、SM、共享内存...）
│   ├── CudaContext.vb          # primary context 的建立与释放
│   ├── CudaModule.vb           # PTX/cubin 加载与 cuModuleGetFunction
│   ├── CudaKernel.vb           # cuLaunchKernel 参数封送（支持流 / LaunchConfig）
│   ├── CudaStream.vb           # 流：让拷贝与计算重叠、按顺序排队
│   ├── CudaEvent.vb            # 事件：流间依赖、同步与 GPU 侧计时
│   ├── CudaTimer.vb            # 基于 CUDA event 的计时
│   ├── DeviceMemory.vb         # 与类型无关的显存基类
│   ├── DeviceBuffer.vb         # 强类型显存缓冲（同步/异步 H<->D、D->D、Fill）
│   ├── PinnedHostBuffer.vb     # 页锁定主机内存（异步拷贝的主机端）
│   ├── CudaMemory.vb           # 显存信息查询（总/可用/已用）
│   ├── LaunchConfig.vb         # LaunchConfig + LaunchPlanner（grid/block 推导、occupancy）
│   ├── KernelCatalog.vb        # 内核注册表（名称/来源/默认 block/共享内存）
│   ├── NvrtcCompiler.vb        # 多通道内核镜像获取（NVRTC 编译 -> 预编译镜像 -> 失败诊断）
│   ├── CubinInspector.vb       # 预编译镜像与驱动版本的兼容性预检
│   ├── CudaEngine\             # 引擎组装入口 + EngineOptions + 内核名常量
│   └── Diagnostics\            # 结构化诊断（DeviceReport / EnvironmentReport / FixSuggestion）
├── Kernels\
│   ├── basic.cu                # vecAdd / saxpy（框架自带冒烟内核，内嵌资源）
│   ├── reduce.cu               # 通用归约 sum / max / min（两阶段）
│   ├── elementwise.cu          # 逐元素算子族（add/sub/mul/div/scale/axpy/relu/exp/log/sqrt/abs）
│   ├── blas.cu                 # 分块 GEMM 与 GEMV
│   └── KernelSources.vb        # 内嵌 .cu 资源读取 + 外部源码注册
├── Math\
│   ├── GpuBasicMath.vb         # 内置基础内核的冒烟示例
│   ├── GpuReduce.vb            # GpuReduce.Sum/Max/Min
│   ├── GpuElementwise.vb       # GpuElementwise 算子族（显存版 + 主机数组版）
│   └── GpuBlas.vb              # GpuBlas.Gemm / Gemv
├── IL2Cuda\                    # IL -> AST -> CUDA 流水线（详见下文）
│   ├── CudaAttributes.vb       # <CudaKernel> / <CudaIndex> / <CudaInput> / <CudaOutput>
│   ├── CudaTypeMap.vb          # .NET 类型 / Math 函数 / 字面量 -> CUDA C
│   ├── CudaEmitter.vb          # 表达式与语句 -> CUDA C 文本
│   └── IlCudaKernel.vb         # 端到端翻译 + 内核注册与启动
└── test\
    ├── test.vbproj             # demo 工程（Exe），ProjectReference 引用 ILCuda
    ├── Program.vb              # CLI：info / demo / selftest / kernels / emit-kernels / il / il-compare
    ├── Reporter.vb             # 控制台输出层（消费框架的结构化诊断对象）
    ├── Metrics\
    │   ├── MatrixData.vb       # 矩阵容器与结果类型（demo 领域模型）
    │   ├── GpuMetrics.vb       # 同步 / 异步 GPU 流水线 + 内核名与元数据注册
    │   ├── CpuMetrics.vb       # CPU 两遍算法参考实现 + 误差比较
    │   └── KernelEmulator.vb   # 内核索引/公式的 CPU 精确模拟自检
    ├── IlDecompile\
    │   ├── MetricFunctions.vb  # 待反编译的 VB.NET 度量函数（对应 metrics.cu 三个内核）
    │   └── IlCudaComparison.vb # 反编译自检 + IL 内核与手写内核的 GPU 结果对比
    └── Kernels\metrics.cu      # demo 专用内核（内嵌资源，运行时注册进框架）
```

## 使用方法

```bat
dotnet build test\test.vbproj -c Debug -p:Platform=x64
cd test\bin\x64\Debug\net10.0

dotnet ILCuda.Demo.dll info            :: 设备与 NVRTC 环境探测（含显存信息）
dotnet ILCuda.Demo.dll demo            :: 完整演示（默认 512x256）
dotnet ILCuda.Demo.dll selftest        :: 内核逻辑自检（不需要 GPU）
dotnet ILCuda.Demo.dll kernels         :: 列出内核注册表（框架内置 + demo 注册）
dotnet ILCuda.Demo.dll emit-kernels kernels
```

| `ILCuda.Demo il` | IL 反编译：打印 AST 伪代码、生成的 `.cu` 与解释求值自检（**不需要 GPU**） |
| `ILCuda.Demo il-compare` | GPU 上对比「IL 生成内核」与手写 `metrics.cu` 的计算结果 |

常用参数：

| 参数 | 说明 |
| --- | --- |
| `--rows <n> / --cols <n>` | 合成随机矩阵的行数/列数（默认 512 / 256） |
| `--seed <n>` | 随机数种子（默认 42，结果可复现） |
| `--csv <file>` | 从 CSV/TSV 载入矩阵（自动识别分隔符，支持行名） |
| `--device <n>` | 使用第几块 GPU（默认 0） |
| `--nvrtc <dll>` | 显式指定 `nvrtc64_*.dll` |
| `--cubin <file>` | 显式指定预编译的 `.ptx` / `.cubin` / `.fatbin` |
| `--elements <n>` | 基础数学示例的向量规模（默认 1048576） |
| `--preview <n>` | 结果矩阵预览大小（默认 6） |
| `--cpu-only` | 只跑 CPU 参考实现 |
| `--async` | 用页锁定内存 + 独立流跑异步流水线 |
| `--force-image` | 跳过镜像/驱动版本兼容性预检（仅供调试，可能让驱动崩溃） |
| `--no-ast` | `il` 命令不打印还原出的伪代码 |
| `--no-source` | `il` 命令不打印生成的 CUDA 源码 |
| `--dump-il` | `il` 命令额外打印原始 IL 与基本块 / 支配 / 循环结构（排查反编译失败用） |

## 框架能力

### 1. 内核源码可编程注册

框架自带的内核以内嵌资源分发；使用方可以把属于自己的 `.cu` 一起编译进同一个模块：

```vbnet
' 注册本程序集内嵌的全部 *.cu 资源（必须在 CudaEngine.TryCreate 之前调用）
Enigma.ILCuda.Kernels.KernelSources.Register(GetType(Program).Assembly)

' 也可以直接注册一段内联源码
KernelSources.RegisterSource("myKernel.cu", "...cuda source...")
```

`KernelSources.All()` / `CombinedSource()` 会按"内置优先、外部按注册顺序"稳定合并，
并用 `来源::资源名` 去重，避免 NVRTC 因重复定义报错。
`demo` 的 `Kernels\metrics.cu` 就是通过这个机制参与编译的。

### 2. 内核注册表

```vbnet
KernelCatalog.Add(New KernelInfo("rowStatsKernel", "metrics.cu", 256, 2048, "每行的 sum / sumSq"))

Dim info = KernelCatalog.Require("gemmKernel")   ' 名字写错时会列出全部已知内核
```

`ILCuda.Demo kernels` 可以打印整张表。

### 3. Stream 与异步执行

```vbnet
Using stream As New CudaStream(CudaStreamFlags.NonBlocking),
      stageIn As New PinnedHostBuffer(Of Single)(n),
      device  As New DeviceBuffer(Of Single)(n)

    stageIn.Write(hostData)
    device.WriteAsync(stageIn, stream)                       ' 异步 H -> D
    engine.GetKernel(KernelNames.EwScale).Launch(stream, config, device, 2.0F, device, n)
    device.ReadAsync(stageOut, stream)                       ' 异步 D -> H
    stream.Synchronize()
End Using
```

`CudaEvent` 可用于流间依赖（`stream.WaitEvent(evt)`）与 GPU 侧精确计时（`CudaEvent.ElapsedMs`）。

### 4. 通用数值内核

```vbnet
Dim total = GpuReduce.Sum(engine, deviceBuffer)        ' 还有 Max / Min / Summarize
GpuElementwise.Scale(engine, x, 2.0F, out)             ' Add/Subtract/Multiply/Divide/Axpy/Relu/Exp/Log/Sqrt/Abs
Dim c = GpuBlas.Gemm(engine, a, b, m, n, k)            ' 矩阵乘（主机数组便捷入口）
Dim y = GpuBlas.Gemv(engine, a, x, m, k)               ' 矩阵-向量乘
```

归约采用"两阶段"结构（每 block 一个部分结果 + 单 block 汇总），全程不需要主机端同步；
elementwise 内核全部使用 grid-stride 循环，可处理任意长度的向量。

### 5. 显存与启动配置

```vbnet
Dim info = engine.GetMemoryInfo()                      ' 总 / 可用 / 已用
Dim config = LaunchPlanner.For1D(n)                    ' 一维向量运算
Dim config2 = LaunchPlanner.For2D(rows, cols, 16, 16)  ' 二维分块运算
Dim block = LaunchPlanner.SuggestBlockSize(device, kernel)  ' 驱动 occupancy 推导（失败回退启发式）
```

`PinnedHostBuffer(Of T)` 用 `cuMemAllocHost` 申请真正的页锁定内存，是异步拷贝的主机端。

### 6. 结构化诊断（框架不依赖 Console）

```vbnet
Dim report = CudaEnvironment.Probe()                   ' EnvironmentReport：驱动版本/设备/NVRTC 候选/内核源码
For Each s In CudaEnvironment.Suggest(report)          ' FixSuggestion：可操作的修复建议
    Console.WriteLine(s)
Next
```

框架只产生纯数据（`DeviceReport` / `EnvironmentReport` / `FixSuggestion`），
排版与输出交给调用方（本仓库里是 `test\Reporter.vb`）。

## demo 的算法说明

设第 `i` 行 `sum_i = Σx_ik`、`sumSq_i = Σx_ik²`、`dot_ij = Σx_ik·x_jk`，`n` 为列数：

```
mean_i = sum_i / n
var_i  = sumSq_i - n·mean_i²           (= Σ(x_ik - mean_i)²)

皮尔逊相关: corr_ij = (dot_ij - n·mean_i·mean_j) / sqrt(var_i · var_j)
欧氏距离  : dist_ij = sqrt(sumSq_i + sumSq_j - 2·dot_ij)
```

于是整条流水线只有三个内核（`test\Kernels\metrics.cu`）：

1. `rowStatsKernel`：一个 block 负责一行，共享内存树形归约同时得到 `sum` 与 `sumSq`；
2. `gramKernel`：16×16 分块点积，利用对称性只计算上三角块（`if (bx > by) return;`），计算量减半；
3. `finalizeKernel`：用点积与行统计量一次性写出相关矩阵与距离矩阵。

程序内置 CPU 两遍算法参考实现，会输出两类矩阵的最大绝对误差与 GPU/CPU 加速比。

> 数值注意点：`dist = sqrt(sumSq_i + sumSq_j - 2·dot_ij)` 在 `i == j` 处是两个相近大数相减，
> 单精度舍入误差会被 `sqrt` 放大到 1e-2 量级。因此 `finalizeKernel` 对对角线直接赋值
> `corr = 1 / dist = 0`（修复前后实测误差：2.2e-2 → 3.8e-6）。

## 实测数据（RTX A4000 / sm_86 / 驱动支持 CUDA 13.2）

| 矩阵规模 | GPU 内核 | GPU 拷贝 | GPU 合计 | CPU 参考 | 加速比(合计) |
| --- | --- | --- | --- | --- | --- |
| 512 × 256 | 0.18 ms | 0.72 ms | 0.91 ms | 213 ms | ~234 x |
| 700 × 300（`--async`） | 0.41 ms | 0.84 ms | 1.26 ms | 466 ms | ~370 x |

单精度下与 CPU 双精度两遍算法的偏差：相关系数 ~2e-7，欧氏距离 ~4e-6。

## IL -> CUDA：由 VB.NET 方法自动生成内核

手写 `.cu` 的问题是"算法改一处、CUDA 源码跟着改一处"。`IL2Cuda\` 提供另一条路：
把**已经编译进 DLL 的普通 VB.NET 方法**在运行时反编译成表达式树，再发射成 CUDA C，
交给 NVRTC 与手写内核一起编译进同一个模块。

### 管道

```
MethodInfo
  -> MethodBodyReader       读 CIL 字节流 -> ILInstruction[]（含局部变量表 / 异常子句 / 偏移索引）
  -> ControlFlowGraph       扫描跳转目标切基本块；迭代支配树 + 支配边界 + 后支配 + 自然循环
  -> SsaBuilder             迭代支配边界上插 phi；沿支配树做变量重命名
  -> StackSimulator         栈模拟归约：压栈类压节点，运算类弹栈构造二元/一元/调用节点，
                            stloc 出声明、ret 出 return、条件分支出 Condition
  -> StructureRecovery      菱形 -> If / If-Else，自然循环 -> While，归纳变量 -> For
  -> MethodSyntax (AST)     自定义表达式 / 语句节点模型
  -> CudaEmitter            AST -> CUDA C（__device__ 标量函数 + __global__ 内核骨架）
  -> KernelSources          注入编译单元 -> NVRTC -> CudaEngine
```

反编译器与 CUDA 发射器之间只通过 AST 耦合：反编译器不知道 CUDA，
发射器不知道 IL。AST 同时被 `AstInterpreter`（CPU 解释求值自检）消费，
因此"反编译对不对"与"CUDA 发射对不对"两个问题可以分开定位。

### 内核映射约定

优先读标注，没有标注时按约定推断：

```vbnet
<CudaKernel("ilMyKernel", CudaIndexMode.Grid2D)>
Public Shared Function MyCell(x As Single(), rows As Integer,
                              <CudaIndex(CudaIndexKind.Row)> i As Integer,
                              <CudaIndex(CudaIndexKind.Col)> j As Integer) As Single
    ...
End Function
```

| 约定 | 行为 |
| --- | --- |
| 存在名为 `i`（或 `row`）的整型参数 | 一维内核：`i = blockIdx.x * blockDim.x + threadIdx.x` |
| 同时存在 `i` 与 `j`（或 `row` / `col`） | 二维内核：`i` 取行（`blockIdx.y`）、`j` 取列（`blockIdx.x`） |
| 方法体里完全没有数组取元素（纯标量） | 逐元素包裹：每个标量参数都变成"按线程下标读取的数组" |
| 都不满足 | 只生成 `__device__` 标量函数，不生成内核 |

内核参数 = 方法参数（去掉线程索引参数）+ 输出数组 + 元素个数；
生成的所有符号统一带 `il_` 前缀，避免与 `metrics.cu`、框架自带 `.cu` 撞名。

### 用法

```bat
ILCuda.Demo il                 :: 打印 AST 伪代码 + 生成的 .cu + 解释求值自检（不需要 GPU）
ILCuda.Demo il --no-source     :: 只看伪代码与自检
ILCuda.Demo il --dump-il       :: 额外打印 IL 指令流与基本块结构
ILCuda.Demo il-compare         :: GPU 上对比 IL 生成内核与手写 metrics.cu
ILCuda.Demo il-compare --rows 128 --cols 64
```

`il-compare` 的中间量（`rowSum` / `rowSumSq` / `dot`）直接复用基准内核写回的显存缓冲，
因此相关与距离两项比较的是"完全相同的输入 + 完全相同的公式"。

### 实测（RTX A4000 / sm_86，128 x 64）

| 对比项 | 最大绝对误差 | 参考量级 |
| --- | --- | --- |
| `RowSum`（vs CPU 参考） | 4.8e-6 | 2.8e+1 |
| `RowSum`（vs `rowStatsKernel`） | 4.8e-6 | 2.8e+1 |
| `RowSumSq`（vs `rowStatsKernel`） | 3.1e-5 | 1.0e+2 |
| `GramDot`（vs `gramKernel`） | 0.0 | 1.0e+2 |
| `CorrelationCell`（vs `finalizeKernel` 的 corr） | 6.0e-8 | 1.0e+0 |
| `DistanceCell`（vs `finalizeKernel` 的 dist） | 0.0 | 1.5e+1 |
| `PearsonClamp`（逐元素自动包裹） | 0.0 | 1.0e+0 |

`RowSum` / `RowSumSq` 的误差来自归约顺序：手写内核用共享内存树形归约，
IL 生成的是顺序累加，单精度下自然有 `1e-5` 量级差异；其余各项公式与输入完全一致，误差为 0。

### 当前边界

支持：数值字面量 / 参数 / 局部 / 一元 / 二元 / 转换 / `Math`·`MathF` 调用 / 数组取元素 /
赋值 / `If` / `While` / `For` / `Return`。

不支持（会抛 `DecompileException` 并带上 IL 偏移，绝不生成语义不明的代码）：
实例方法（`this`）、`starg`、`stelem`、`switch`、`newobj`、字段访问、装箱、异常、
不可归约的控制流形状、`goto`。

### 顺带修掉的既有缺陷

`vs_solutions\dev\VisualStudio\IL\MethodBodyReader.vb`：

- `Offset` 对 `0xFE` 开头的双字节操作码算错 1 字节（原实现用"读完后的位置 - 1"）；
- `switch` 指令算出了跳转表却从未写回 `Operand`；
- `InlineVar` / `ShortInlineVar` 的操作数类型不统一（`UInt16` / `Byte` 混用）；
- `OperandData` 从未填充；
- 未暴露局部变量表、最大栈深、异常子句与按偏移的指令索引；
- `Dispose` 在没有 IL 体时会对 `Nothing` 调用 Dispose，且会清空已解析好的指令列表。

## 环境要求（重要）

CUDA 运行时有一套硬性的版本匹配规则：

- **显卡驱动版本**决定能 JIT 的 PTX 版本，以及能加载的 cubin 的编译工具包版本；
- **CUDA 工具包版本**（`nvcc` / `nvrtc`）不能高于驱动支持的 CUDA 版本。

程序会自动处理这种情况：

1. 扫描系统中所有 `nvrtc64_*.dll`（CUDA_PATH、`Program Files\...\CUDA\v*\bin\x64`、PATH、
   程序目录、其它 NVIDIA 应用目录），**优先尝试版本不高于驱动的**；
2. 逐个"编译 → `cuModuleLoadData` 实测"，成功即采用，失败换下一个；
3. 每个 NVRTC 版本都按 **`sm_XX` → `compute_XX`** 的顺序尝试（并逐级降级架构）：

   - `-arch=sm_86`（**真实架构**）：NVRTC 内部直接调用 ptxas 产出 **cubin（SASS）**，
     加载时不需要驱动做 PTX JIT —— 这是兼容性最好的通道，也是默认命中的通道；
   - `-arch=compute_86`（虚拟架构）：产出 PTX，由驱动在加载时 JIT 编译，
     一旦"工具包版本 > 驱动支持的 CUDA 版本"就会得到 `CUDA_ERROR_UNSUPPORTED_PTX_VERSION`。
4. 全部失败则尝试 `kernels\*.ptx` / `*.cubin` 预编译镜像（会先做版本预检，避免驱动崩溃）；
5. 仍然失败则输出结构化诊断 + 修复建议，并**自动回退 CPU 实现**，保证命令行永远能跑出结果。

> NVRTC 还依赖同目录的 `nvrtc-builtins64_*.dll`。程序会在加载前把 NVRTC 所在目录加入
> 动态库搜索路径（`SetDllDirectory`），否则会报 `NVRTC_ERROR_BUILTIN_OPERATION_FAILURE`。

若连 cubin 通道也失败（通常意味着**跨 CUDA 大版本**），三选一：

1. 升级 NVIDIA 驱动（CUDA 13.x 需要 r580 及以上，CUDA 12.8 对应 572.x）；
2. 安装与驱动匹配的 CUDA 工具包（驱动 12.8 → CUDA Toolkit 12.8，会提供 `nvrtc64_120_0.dll`）；
3. 离线编译内核为 cubin 后用 `--cubin` 指定（同样要求工具包版本不高于驱动）：
   ```bat
   ILCuda.Demo emit-kernels .\kernels
   nvcc -arch=sm_86 -cubin -o kernels\kernels.cubin kernels\all.cu
   ILCuda.Demo demo --cubin kernels\kernels.cubin
   ```
   （若 nvcc 报找不到 `cl.exe`，加 `-ccbin "<MSVC 安装目录>\VC\Tools\MSVC\<ver>\bin\Hostx64\x64>"`。）

## 自检

在没有可用 GPU（或版本错配）时，可以用 `selftest` 命令验证 demo 内核逻辑：
它会用 CPU 逐线程模拟三个内核的 blockIdx/threadIdx 与分块共享内存的索引方式，
并与 CPU 两遍算法参考实现对比（单精度下误差应在 1e-6 量级）。

```bat
ILCuda.Demo selftest
```

# DirectML —— 纯 Interop 的 VB.NET DirectML 计算框架

不引用**任何第三方 NuGet 程序包**，只用 .NET 基类库的 `System.Runtime.InteropServices`
直接绑定 `dxgi.dll`、`d3d12.dll`、`DirectML.dll` 的原生 API 与 COM vtable，
为 `sciBASIC#` 提供与 `Microsoft.VisualBasic.Computing.ILCuda` 同构的 GPU 计算栈（Windows 版）。

## 为什么手写 vtable

要让 DirectML 跑起来，至少要打通 DXGI（适配器）→ D3D12（设备/资源/命令）→ DirectML（算子）三层。
通常这需要 `Vortice.Direct3D12` / `Silk.NET` 之类的绑定包；本框架把绑定写进仓库：

| 依赖 | 处理方式 |
| --- | --- |
| `dxgi.dll` | `NativeLibrary.TryLoad` 多路径探测 + `Marshal.GetDelegateForFunctionPointer` |
| `d3d12.dll` | 同上，导出 `D3D12CreateDevice` |
| `DirectML.dll` | 同上，导出 `DMLCreateDevice` / `DMLCreateDevice1` |
| COM 接口 | 手写 vtable 槽位（槽位号取自 Windows SDK 头文件的 `Vtbl`，并在真实显卡上实测校验） |

代价是描述符结构体必须手工对齐、生命周期必须手工 `Release`；收益是零依赖，
并且枚举/结构体可以逐项对照 `Windows Kits\10\Include\10.0.26100.0\um\DirectML.h` 与 `d3d12.h`。

## 目录结构

```
DirectML\
├── DirectML.vbproj                  # net10.0 / x64 框架库，零 NuGet 依赖
├── Interop\                         # 只做声明与绑定
│   ├── ComApi.vb                    # vtable 槽位绑定、IUnknown 三槽位、ComObjectBase
│   ├── ComIIDs.vb                   # 接口 IID（取自 SDK 头文件）
│   ├── HResult.vb                   # HRESULT 判定 / 文本 / DmlException
│   ├── NativeModule.vb              # 原生库多路径探测与导出缓存
│   ├── DxgiApi.vb                   # CreateDXGIFactory2、适配器枚举、显存预算
│   ├── D3D12Api.vb                  # D3D12CreateDevice、结构体、枚举、槽位委托
│   ├── DmlApi.vb                    # DMLCreateDevice、IDML* 槽位委托
│   ├── DmlEnums.vb                  # DML 枚举（算子类型序号逐项核对）
│   ├── DmlDescriptors.vb            # 张量/绑定/特性查询描述符
│   ├── DmlInterop.vb                # 非托管描述符内存构建器
│   └── Operators\                   # 各类算子的原生描述符结构体
├── Runtime\                         # 对象生命周期（IDisposable）
│   ├── DmlRuntime.vb                # 原生库探测入口
│   ├── DmlAdapter.vb                # 适配器（名称/显存/是否独显/能否建 D3D12 设备）
│   ├── DmlDevice.vb                 # D3D12 + DML 设备与特性级别探测
│   ├── DmlCommandQueue.vb           # 命令队列
│   ├── DmlCommandAllocator.vb       # 命令分配器
│   ├── DmlCommandList.vb            # 命令列表（拷贝/屏障）
│   ├── DmlFence.vb                  # Fence + 带超时的等待
│   ├── DmlBuffer.vb                 # ID3D12Resource 包装（上传/回读/默认堆）
│   ├── DmlTensor.vb                 # 张量形状/步长描述
│   ├── DmlMemoryPool.vb             # 单块 UAV 缓冲 + 256 字节对齐切片
│   ├── DmlDescriptorHeap.vb         # shader-visible 描述符堆
│   ├── DmlBindingTable.vb           # 绑定表
│   ├── DmlCompiledOperator.vb       # 编译算子 / 算子初始化器 / 命令记录器
│   ├── DmlExecutionContext.vb       # 队列+分配器+命令列表+Fence 的提交编排
│   ├── DmlGraph.vb                  # 计算图：编译一次、重复执行
│   └── DmlEngine\                   # 引擎组装入口（DmlEngine / EngineOptions）
├── Operators\                       # ONNX 算子名 → DML 算子描述符工厂
│   ├── DmlOperatorDefinition.vb     # 算子定义（描述符 + 输入/输出张量）
│   ├── DmlOperatorFactory.vb        # 描述符构造基础设施
│   ├── ElementWiseOperators.vb      # 逐元素：Add/Sub/Mul/Div/Pow/Min/Max/Clip/If/量化…
│   ├── ActivationOperators.vb       # 激活：Relu/LeakyRelu/Elu/Selu/Sigmoid/Tanh/Softmax…
│   ├── MatMulOperators.vb           # GEMM + ONNX MatMul 形状规范化
│   ├── ConvolutionOperators.vb      # Conv/ConvTranspose/AveragePool/MaxPool/LpPool…
│   ├── ReduceOperators.vb           # Reduce/ArgMax/BN/InstanceNorm/LayerNorm/LRN/LpNorm
│   ├── ShapeOperators.vb            # Slice/Join/Split/Pad/Tile/Cast/DepthToSpace/Resample…
│   ├── DataMovementOperators.vb     # Gather/GatherElements/GatherND/ScatterND/TopK/CumSum…
│   ├── DmlShapeInference.vb         # 卷积/池化输出尺寸与 auto_pad 推导
│   └── OperatorCatalog.vb           # ONNX 算子 → DML 映射能力表
├── Diagnostics\                     # 设备/环境报告与修复建议（纯数据，不依赖 Console）
└── test\                            # 控制台测试程序（DirectML.Demo）
```

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Computing.DirectML.Runtime

Using engine As DmlEngine = DmlEngine.TryCreate(New EngineOptions With {.AdapterIndex = 0})
    Console.WriteLine(engine.Device.FeatureLevelText)      ' 例如 5.1
End Using
```

执行一个最小计算图（`input -> Relu -> output`）：

```vbnet
Using engine As DmlEngine = DmlEngine.Create()
    Using graph As DmlGraph = engine.CreateGraph()
        Dim input = graph.DefineTensor("input", {1UI, 1UI, 2UI, 2UI}, DmlTensorKind.Input)
        Dim output = graph.DefineTensor("output", {1UI, 1UI, 2UI, 2UI}, DmlTensorKind.Output)

        graph.AddOperator(ActivationOperators.Relu(input, output, "relu"))
        graph.Compile()
        graph.SetInput("input", {-1.0F, 2.0F, -3.0F, 4.0F})
        graph.Execute()

        Dim result = graph.GetOutput("output")
    End Using
End Using
```

## ONNX 推理后端

ONNX → DirectML 的桥接层位于
`Data_science\MachineLearning\MLDataStorage\ONNX\DirectML\`（由 `MLDataStorage` 引用本包）：

```vbnet
Imports Microsoft.VisualBasic.MachineLearning.DataStorage.ONNX
Imports Microsoft.VisualBasic.MachineLearning.DataStorage.ONNX.DirectML

Dim model = OnnxModel.Load("./model.onnx")
Using session = DmlOnnxSession.Create(model)
    Dim y = session.RunSingle(observation)      ' 与 OnnxGraphEvaluator.RunSingle 同形
End Using
```

数值策略：ONNX 张量在 `OnnxTensor` 里是 `Double`，DirectML 只接受 FP16/FP32；
转换只发生在**图的输入/输出边界**一次，中间结果全程驻留显存。

## 控制台测试程序

```bat
dotnet build test\test.vbproj -c Debug -p:Platform=x64
cd test\bin\x64\Debug\net10.0

DirectML.Demo.exe info       :: 原生库探测、适配器、特性级别、显存
DirectML.Demo.exe ops        :: ONNX 算子 → DirectML 映射表
DirectML.Demo.exe selftest   :: 形状推导自检（纯 CPU，无需显卡）
DirectML.Demo.exe device     :: 逐段创建设备对象（定位原生调用问题）
DirectML.Demo.exe graph relu :: 最小计算图端到端（可换 conv）
DirectML.Demo.exe onnx       :: 小型 ONNX 模型：DirectML 推理 vs CPU 双精度参考
```

所有断言经仓库内的 `DebugAudit` 记录，并写出 `directml-report.json`；
设置环境变量 `DIRECTML_DEBUG=1` 可以打开编译/执行各阶段的 trace。

## 环境要求

- Windows 10 1903 及以上（`DirectML.dll` 由系统通过 DirectX 更新下发到 `System32`）；
- 或把 `Microsoft.AI.DirectML` 分发目录里的 `DirectML.dll` 放到程序目录，
  并用 `EngineOptions.DirectMLDirectory` 指定搜索目录；
- 进程必须是 x64。

## 实现状态（RTX A4000 / Windows 11 / 特性级别 3.0 实测）

| 环节 | 状态 |
| --- | --- |
| 原生库探测（DirectML/d3d12/dxgi） | ✅ 通过（回退到 System32） |
| DXGI 适配器枚举 / 显存预算 | ✅ 通过（识别到 A4000 + UHD 770） |
| `D3D12CreateDevice` + `DMLCreateDevice` + 特性级别探测 | ✅ 通过（level 3.0） |
| 命令队列 / 分配器 / 命令列表 / Fence / 命令记录器 | ✅ 通过 |
| 描述符堆（`GetCPU/GPUDescriptorHandleForHeapStart`） | ✅ 通过（increment = 32） |
| `IDMLDevice::CreateOperator` / `CompileOperator` / `GetBindingProperties` | ✅ 通过（Relu: 2 descriptors, temp = 0） |
| `IDMLDevice::CreateOperatorInitializer` / `CreateBindingTable` | ✅ 通过 |
| `ID3D12Device::CreateCommittedResource`（缓冲区） | ⚠️ 返回 `E_INVALIDARG`，见下节 |

### 待解决：缓冲区资源创建被拒绝

在 RTX A4000 / Intel UHD 770 与 WARP 上，`CreateCommittedResource` 对 **BUFFER** 描述符
一律返回 `0x80070057 (E_INVALIDARG)`，而同一次调用换成 **TEXTURE2D** 描述符则成功，
说明 vtable 槽位（27）、`D3D12_HEAP_PROPERTIES`、`riid`、初始状态都与运行时一致，
问题只出在缓冲区描述符的校验上。

已经排除的因素：

- 结构体布局：`Marshal.SizeOf(D3D12_RESOURCE_DESC) = 56`，`Width @16`、`Height @24`、
  `Format @32`、`SampleDesc @36`、`Layout @44`、`Flags @48`，与 SDK 头文件一致；
- 描述符字节：`01-00-00-00 … 00-02-00-00…`（Dimension=BUFFER、Width=512、Height=1、
  Depth=1、Mip=1、Format=UNKNOWN、SampleDesc=1/0、Layout=UNKNOWN、Flags=NONE）；
- 三种堆类型（DEFAULT/UPLOAD/READBACK）、三种初始状态、`Width` ∈ {4,256,512,65536}、
  `Alignment` ∈ {0,65536}、`Flags` ∈ {NONE, UAV}、`NodeMask` ∈ {0,1} 全部失败。

下一步建议：在创建 D3D12 设备前启用调试层（`D3D12GetDebugInterface` + `EnableDebugLayer`），
再通过 `ID3D12InfoQueue` 读取校验消息，即可直接得到运行时给出的拒绝原因。

## 许可

GPL-3.0-or-later。

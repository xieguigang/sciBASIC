# DirectML —— 纯 Interop 的 VB.NET DirectML 计算框架

不引用**任何第三方 NuGet 程序包**，只用 .NET 基类库的 `System.Runtime.InteropServices`
直接绑定 `dxgi.dll`、`d3d12.dll`、`DirectML.dll` 的原生 API 与 COM vtable，
为 `sciBASIC#` 提供与 `Microsoft.VisualBasic.Computing.ILCuda` 同构的 GPU 计算栈（Windows 版）。

## 为什么手写 vtable

DirectML 建立在 D3D12 之上：创建适配器要 `DXGI`，创建设备要 `D3D12CreateDevice`，
创建算子要 `DMLCreateDevice` + `IDMLDevice`。要让这一整条链路可用，通常需要
`Vortice.Direct3D12` / `Silk.NET` 之类的绑定包；本框架选择把绑定写进仓库：

| 依赖 | 处理方式 |
| --- | --- |
| `dxgi.dll` | `NativeLibrary.TryLoad` 多路径探测 + `Marshal.GetDelegateForFunctionPointer` |
| `d3d12.dll` | 同上，导出的 `D3D12CreateDevice` |
| `DirectML.dll` | 同上，导出的 `DMLCreateDevice` / `DMLCreateDevice1` |
| COM 接口 | 手写 vtable 槽位（槽位号取自 Windows SDK 头文件的 `Vtbl` 结构体） |

代价是描述符结构体必须手工对齐、生命周期必须手工 `Release`；收益是零依赖 +
可以直接跟着 Windows SDK 的版本演进（本仓库的枚举与结构体已逐项对照
`Windows Kits\10\Include\10.0.26100.0\um\DirectML.h` 核对）。

## 目录结构

```
DirectML\
├── DirectML.vbproj              # net10.0 / x64 框架库，零 NuGet 依赖
├── Interop\                     # 只做声明与绑定
│   ├── ComApi.vb                # vtable 槽位绑定、IUnknown 三槽位、ComObjectBase
│   ├── ComIIDs.vb               # 接口 IID（取自 SDK 头文件）
│   ├── HResult.vb               # HRESULT 判定 / 文本 / DmlException
│   ├── NativeModule.vb          # 原生库多路径探测与导出缓存
│   ├── DxgiApi.vb               # CreateDXGIFactory2、适配器枚举、显存预算
│   ├── D3D12Api.vb              # D3D12CreateDevice、结构体、枚举、槽位委托
│   ├── DmlApi.vb                # DMLCreateDevice、IDML* 槽位委托
│   ├── DmlEnums.vb              # DML 枚举（算子类型序号逐项核对）
│   ├── DmlDescriptors.vb        # 张量/绑定/特性查询描述符
│   ├── DmlInterop.vb            # 非托管描述符内存构建器
│   └── Operators\               # 各类算子的原生描述符结构体
├── Runtime\                     # 对象生命周期（IDisposable）
│   ├── DmlRuntime.vb            # 原生库探测入口
│   ├── DmlAdapter.vb            # 适配器（名称/显存/是否独显/能否建 D3D12 设备）
│   ├── DmlDevice.vb             # D3D12 + DML 设备与特性级别探测
│   ├── DmlCommandQueue.vb       # 命令队列
│   ├── DmlCommandAllocator.vb   # 命令分配器
│   ├── DmlCommandList.vb        # 命令列表（拷贝/屏障）
│   ├── DmlFence.vb              # Fence + 带超时的等待
│   ├── DmlBuffer.vb             # ID3D12Resource 包装（上传/回读/默认堆）
│   ├── DmlTensor.vb             # 张量形状/步长描述
│   ├── DmlMemoryPool.vb         # 单块 UAV 缓冲 + 256 字节对齐切片
│   ├── DmlDescriptorHeap.vb     # shader-visible 描述符堆
│   ├── DmlBindingTable.vb       # 绑定表
│   ├── DmlCompiledOperator.vb   # 编译算子 / 算子初始化器 / 命令记录器
│   └── Diagnostics\             # 设备与环境报告（纯数据，不依赖 Console）
└── Operators\                   # ONNX 算子名 → DML 算子描述符工厂
```

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Computing.DirectML.Runtime

Using engine As DmlEngine = DmlEngine.TryCreate(New EngineOptions With {.AdapterIndex = 0})
    Console.WriteLine(engine.Device.FeatureLevelText)      ' 例如 5.1
End Using
```

## 环境要求

- Windows 10 1903 及以上（`DirectML.dll` 由系统通过 DirectX 更新下发）；
- 或者把 `Microsoft.AI.DirectML` 分发目录里的 `DirectML.dll` 放到程序目录，
  通过 `EngineOptions.DirectMLDirectory` 指定搜索目录；
- 进程必须是 x64。

## 相关包

- `Microsoft.VisualBasic.Computing.ILCuda`：同一套分层思路的 CUDA 版本；
- `Microsoft.VisualBasic.MachineLearning.DataStorage`：ONNX 模型读写，
  本包的 ONNX 后端（`ONNX\DirectML\`）在其上一层层实现。

## 许可

GPL-3.0-or-later。

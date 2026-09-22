# 纯 VB.NET 张量库（NumPy 风格 API 与可插拔计算后端）

## 引言

**首先要澄清一件事**：本包的名字虽然叫 `TensorFlow`，但它**不是** Google TensorFlow 的绑定，而是一个**自包含的托管数值引擎**。

它解决的是 .NET 生态的一个真实缺口：需要多维数组与高性能数值计算，但不想引入几百 MB 的原生运行时。

## 设计目标

- **零原生依赖**：纯托管实现，可单文件部署；
- **NumPy 风格 API**：降低从 Python 数值生态迁移的成本；
- **可插拔计算后端**：默认 SIMD 加速的 CPU 实现，注册一次即可整体切到 CUDA GPU。

## 核心能力

| 命名空间 | 能力 |
|---|---|
| `...MachineLearning.TensorFlow`（根） | `Tensor`：托管多维数组，支持切片、广播与算术运算 |
| `....TensorFlow.NumPy` | NumPy 风格 API（`np` 式的数组操作惯例） |
| `....TensorFlow.Compute` | 张量计算引擎（`ITensorCompute` 后端契约，默认 `SIMDTensor`） |
| `....TensorFlow.Math` / `nn` | 数值运算、激活函数与损失函数 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

' 1. 创建张量（NumPy 风格）
Dim a As Tensor = np.RandomNormal(shape:={100, 50})
Dim b As Tensor = np.Ones(shape:={50, 20})

' 2. 常规运算（广播、切片、矩阵乘）
Dim c As Tensor = a.MatMul(b)
Dim column = a(All, 0)

' 3. 神经网络常用构件
Dim activated = nn.Relu(c)
Dim l = nn.MeanSquaredError(c, target)

' 4. 切换计算后端（例如注册 CUDA 实现后整体走 GPU）
' Tensor.computeKernel = CudaTensor.Default
```

## 实现要点

- **数值算子与反向传播分离**：`ITensorCompute` 只提供纯数值算子（含为 ReLU 系反向传播准备的 `Heaviside` 掩码算子），因此下游的深度学习模块既可以把算子整体下放到 GPU，也可以在这些算子的最底层之上手写反向传播。
- **托管实现的取舍**：缺少原生框架级别的 kernel 融合与显存管理优化，因此超大模型训练性能不及原生框架；但作为**可调试、可嵌入、零部署成本**的数值引擎，它对教学、小模型与嵌入式场景非常合适。
- **与 `ILCudaTensor` 的关系**：`ILCudaTensor` 实现了本包定义的 `ITensorCompute` 接口，并继承 `TensorComputeBase` 作为 CPU 兜底——因此在本包之上注册一次 CUDA 后端，所有 Tensor / Math / nn 运算即可透明地切到 GPU。

## 包信息

- Assembly：`Microsoft.VisualBasic.MachineLearning.TensorFlow`
- TargetFramework：`net10.0`
- Tags：`scibasic;tensor;numpy;numerical;computation-engine;activation-functions;simd`
- 许可：GPL-3.0-or-later

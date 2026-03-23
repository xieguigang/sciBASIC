#Region "Microsoft.VisualBasic::2a9e37d8d59674697e952044fcc5b438, Data_science\MachineLearning\LNN\Documentation.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 279
    '    Code Lines: 0 (0.00%)
    ' Comment Lines: 259 (92.83%)
    '    - Xml Docs: 65.25%
    ' 
    '   Blank Lines: 20 (7.17%)
    '     File Size: 10.22 KB


    ' 
    ' /********************************************************************************/

#End Region

'''
''' 液态神经网络(LNN)模块技术文档
''' Liquid Neural Networks Module Technical Documentation
'''
''' 版本: 1.0
''' 作者: 基于Tensor对象实现
''' 日期: 2024
'''

' ============================================================================
' 模块概述
' ============================================================================

'''
''' 液态神经网络(Liquid Neural Networks, LNN)是一种基于连续时间动态系统的神经网络架构。
''' 与传统的离散时间神经网络不同，LNN使用常微分方程(ODE)来描述神经元的状态演化，
''' 这使得它特别适合处理时间序列数据和不规则采样的信号。
'''
''' 核心特点:
''' 1. 连续时间处理 - 可以处理任意时间间隔的输入
''' 2. 可学习的时间常数 - 每个神经元有独立的时间常数，可以自适应调整
''' 3. 稳定的动态行为 - 基于ODE的架构提供了更好的稳定性
''' 4. 可解释性 - 神经元的动态行为可以直接观察和分析
'''

' ============================================================================
' 架构设计
' ============================================================================

'''
''' 模块层次结构:
''' 
''' LiquidNeuralNetwork (完整网络)
'''     ├── LiquidLayer (液态层)
'''     │       └── LiquidCell (液态神经元单元) × N
'''     │               ├── 状态 (State)
'''     │               ├── 时间常数 (Tau)
'''     │               ├── 输入权重 (WeightInput)
'''     │               ├── 循环权重 (WeightRecurrent)
'''     │               └── 偏置 (Bias)
'''     ├── 输出权重 (OutputWeight)
'''     └── 输出偏置 (OutputBias)
'''
''' 辅助模块:
''' ├── ODESolver (ODE求解器)
''' │       ├── EulerStep (欧拉法)
''' │       ├── HeunStep (改进欧拉法)
''' │       ├── RK4Step (四阶龙格-库塔法)
''' │       └── AdaptiveRK45Step (自适应步长)
''' ├── ActivationFunctions (激活函数)
''' │       ├── Sigmoid, Tanh, ReLU, LeakyReLU, Softmax
''' │       └── 对应的导数函数
''' ├── LNNTrainer (训练器)
''' │       ├── SGD优化器
''' │       ├── Adam优化器
''' │       └── 损失函数 (MSE, MAE)
''' └── TimeSeriesUtils (时间序列工具)
'''         ├── 数据预处理 (归一化、标准化)
'''         ├── 滑动窗口数据集创建
'''         └── 评估指标计算
'''

' ============================================================================
' 核心数学原理
' ============================================================================

'''
''' 液态神经元的核心方程:
''' 
''' dx/dt = -x/τ + σ(W·x + U·u + b)
'''
''' 其中:
''' - x: 神经元状态向量 (HiddenSize维)
''' - τ: 时间常数向量 (控制状态衰减速度)
''' - W: 循环权重矩阵 (HiddenSize × HiddenSize)
''' - U: 输入权重矩阵 (InputSize × HiddenSize)
''' - u: 外部输入向量 (InputSize维)
''' - b: 偏置向量 (HiddenSize维)
''' - σ: 激活函数 (tanh, sigmoid, relu等)
'''
''' 时间常数τ的作用:
''' - τ越大，状态衰减越慢，"记忆"越持久
''' - τ越小，状态衰减越快，对当前输入更敏感
''' - τ是可学习参数，网络可以自动学习最优的时间尺度
'''

' ============================================================================
' 使用指南
' ============================================================================

'''
''' 1. 创建网络
''' 
''' Dim lnn As New LiquidNeuralNetwork(
'''     inputSize:=10,           ' 输入特征维度
'''     hiddenSize:=32,          ' 隐藏层神经元数量
'''     outputSize:=1,           ' 输出维度
'''     numLiquidLayers:=2,      ' 液态层数量
'''     activationType:="tanh",  ' 隐藏层激活函数
'''     outputActivation:="none", ' 输出层激活函数
'''     seed:=42                 ' 随机种子
''' )
'''
''' 2. 配置网络参数
''' 
''' lnn.DefaultDt = 0.1          ' 默认时间步长
''' lnn.SolverType = "rk4"       ' ODE求解器类型
''' lnn.RecordHistory = True     ' 记录状态历史
'''
''' 3. 前向传播
''' 
''' ' 单步预测
''' Dim input = New Tensor({10}) ' 输入向量
''' Dim output = lnn.Forward(input)
'''
''' ' 序列处理
''' Dim sequence = New Tensor({100, 10}) ' 时间序列 (时间步 × 特征)
''' Dim outputs = lnn.ProcessSequence(sequence)
'''
''' 4. 训练网络
''' 
''' Dim trainer As New LNNTrainer(lnn, learningRate:=0.01)
''' trainer.OptimizerType = "adam"
''' trainer.UseGradientClipping = True
'''
''' ' 单步训练
''' Dim loss = trainer.TrainStep(input, target)
'''
''' ' 序列训练
''' Dim avgLoss = trainer.TrainSequence(inputSeq, targetSeq)
'''
''' ' 完整训练循环
''' Dim losses = trainer.Fit(trainSequences, targetSequences, epochs:=100)
'''

' ============================================================================
' ODE求解器选择指南
' ============================================================================

'''
''' 求解器比较:
''' 
''' | 求解器   | 精度 | 速度 | 适用场景 |
''' |---------|------|------|---------|
''' | euler   | 一阶 | 最快 | 快速原型、实时应用 |
''' | heun    | 二阶 | 中等 | 平衡精度和速度 |
''' | rk4     | 四阶 | 较慢 | 高精度要求 |
''' | adaptive| 自适应| 变化 | 变步长需求 |
'''
''' 时间步长选择:
''' - dt较小: 更精确，但计算量大
''' - dt较大: 计算快，但可能不稳定
''' - 建议: 从dt=0.1开始，根据结果调整
'''

' ============================================================================
' 参数调优建议
' ============================================================================

'''
''' 1. 网络架构
''' - hiddenSize: 通常16-128之间，根据任务复杂度调整
''' - numLiquidLayers: 1-3层通常足够，过多可能导致训练困难
''' - 激活函数: tanh适合大多数时间序列任务
'''
''' 2. 训练参数
''' - learningRate: 建议从0.001-0.01开始
''' - 使用Adam优化器通常比SGD收敛更快
''' - 启用梯度裁剪防止梯度爆炸
'''
''' 3. 数据预处理
''' - 归一化: 将数据缩放到[0,1]或[-1,1]范围
''' - 标准化: 零均值、单位方差
''' - 滑动窗口: 窗口大小应覆盖足够的历史信息
'''

' ============================================================================
' 常见问题解决
' ============================================================================

'''
''' Q: 训练损失不下降?
''' A: 1) 降低学习率 2) 检查数据预处理 3) 增加隐藏层大小
'''
''' Q: 预测结果不稳定?
''' A: 1) 减小时间步长dt 2) 使用更高阶的ODE求解器 3) 检查输入数据质量
'''
''' Q: 训练速度太慢?
''' A: 1) 使用euler求解器 2) 增大时间步长 3) 减少网络层数
'''
''' Q: 内存占用过大?
''' A: 1) 设置RecordHistory=False 2) 减小batch大小 3) 使用更小的网络
'''

' ============================================================================
' 扩展开发指南
' ============================================================================

'''
''' 1. 添加新的激活函数
''' 
''' 在ActivationFunctions模块中添加:
''' Public Function NewActivation(x As Tensor) As Tensor
'''     Return x.Apply(Function(v) ' 你的激活函数)
''' End Function
'''
''' Public Function NewActivationDerivative(output As Tensor) As Tensor
'''     Return output.Apply(Function(v) ' 导数函数)
''' End Function
'''
''' 2. 自定义ODE方程
''' 
''' 继承LiquidCell并重写ComputeDerivative方法:
''' Public Overrides Function ComputeDerivative(...) As Tensor
'''     ' 自定义的微分方程
''' End Function
'''
''' 3. 添加新的优化器
''' 
''' 在LNNTrainer中添加新的更新方法:
''' Private Sub UpdateParametersNewOptimizer()
'''     ' 优化器实现
''' End Sub
'''

' ============================================================================
' 性能优化建议
' ============================================================================

'''
''' 1. 使用合适的数据类型
''' - Tensor内部使用Double精度，适合科学计算
''' - 如果需要更高性能，可以考虑修改为Single
'''
''' 2. 批处理优化
''' - 尽量使用ProcessSequence处理完整序列
''' - 避免频繁的单步Forward调用
'''
''' 3. 内存管理
''' - 使用Using语句或显式调用Dispose释放资源
''' - 避免不必要的Tensor克隆操作
'''

' ============================================================================
' API参考
' ============================================================================

'''
''' LiquidNeuralNetwork类主要方法:
''' 
''' | 方法 | 说明 | 参数 | 返回值 |
''' |-----|------|-----|-------|
''' | New | 创建网络 | inputSize, hiddenSize, outputSize等 | - |
''' | Forward | 单步前向传播 | input, dt(可选) | Tensor |
''' | ProcessSequence | 处理时间序列 | sequence, dt(可选) | Tensor |
''' | ResetState | 重置网络状态 | - | - |
''' | GetParameters | 获取所有参数 | - | Dictionary |
''' | GetParameterCount | 获取参数数量 | - | Integer |
''' | Dispose | 释放资源 | - | - |
'''
''' LNNTrainer类主要方法:
''' 
''' | 方法 | 说明 | 参数 | 返回值 |
''' |-----|------|-----|-------|
''' | New | 创建训练器 | network, learningRate | - |
''' | TrainStep | 训练单步 | input, target, dt | Double(loss) |
''' | TrainSequence | 训练序列 | inputSeq, targetSeq, dt | Double(avgLoss) |
''' | Fit | 完整训练 | sequences, targets, epochs | List(Of Double) |
'''
''' TimeSeriesUtils模块主要方法:
''' 
''' | 方法 | 说明 | 参数 | 返回值 |
''' |-----|------|-----|-------|
''' | CreateSlidingWindowDataset | 创建滑动窗口数据 | data, windowSize, horizon | (inputs, targets) |
''' | Normalize | 归一化 | data | (normalized, min, max) |
''' | Standardize | 标准化 | data | (standardized, mean, std) |
''' | GenerateSineWave | 生成正弦波 | length, frequency等 | Double() |
''' | CalculateMetrics | 计算评估指标 | predicted, actual | (mse, mae, rmse, mape) |
'''


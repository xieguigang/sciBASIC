' ---------------------------------------------------------------------------
' AdamW（解耦权重衰减），全 fp64 状态
'
' 为什么不复用 GNN.AdamOptimizer：
'   1. 它的参数、矩估计、更新全部是 Single。二阶矩要存 g²，
'      单精度下大梯度很快溢出成 Infinity，随后 Inf/Inf = NaN 会污染全部参数；
'   2. 它没有 weight decay。readme 里的训练要点是 wd≈5e-4 —— 这正是 GCN
'      在只有几十个标签的半监督设置下不过拟合的主要手段；
'   3. AdamW 的“解耦”语义与 L2 正则不同：衰减直接作用于参数而不是梯度，
'      对自适应学习率不会失真。本类按解耦实现。
'
' 设备端快路径：优先调 Tensor.computeKernel.TryAdamWStep（GPU 后端会就地更新
' 参数与 m/v，省掉三次全量往返），返回 False 时走主机 fp64 循环。两条路径的
' 数学定义必须一致，否则 CPU/GPU 结果不可比。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' AdamW 优化器：fp64 一阶/二阶矩 + 解耦权重衰减。
''' </summary>
Public Class GCNAdamW

    Private ReadOnly _parameters As List(Of Tensor)
    Private ReadOnly _gradients As List(Of Tensor)
    Private ReadOnly _momentum As List(Of Tensor)
    Private ReadOnly _velocity As List(Of Tensor)

    Private _steps As Long

    ''' <summary>学习率（readme 推荐 0.01 起步）</summary>
    Public Property LearningRate As Double = 0.01
    ''' <summary>一阶矩的指数衰减率</summary>
    Public Property Beta1 As Double = 0.9
    ''' <summary>二阶矩的指数衰减率</summary>
    Public Property Beta2 As Double = 0.999
    ''' <summary>数值稳定性常数</summary>
    Public Property Epsilon As Double = 1.0E-8
    ''' <summary>解耦权重衰减系数（readme 推荐 5e-4）</summary>
    Public Property WeightDecay As Double = 5.0E-4
    ''' <summary>是否优先使用设备端 AdamW 内核；关闭后强制走主机 fp64 循环</summary>
    Public Property UseDeviceKernel As Boolean = True

    ''' <summary>创建优化器。</summary>
    ''' <param name="parameters">可训练参数（训练期间引用不得替换）</param>
    ''' <param name="gradients">与参数一一对应的梯度张量</param>
    ''' <param name="learningRate">学习率</param>
    ''' <param name="weightDecay">解耦权重衰减</param>
    ''' <param name="beta1">一阶矩衰减率</param>
    ''' <param name="beta2">二阶矩衰减率</param>
    ''' <param name="epsilon">数值稳定性常数</param>
    Public Sub New(parameters As List(Of Tensor),
                   gradients As List(Of Tensor),
                   Optional learningRate As Double = 0.01,
                   Optional weightDecay As Double = 5.0E-4,
                   Optional beta1 As Double = 0.9,
                   Optional beta2 As Double = 0.999,
                   Optional epsilon As Double = 1.0E-8)

        If parameters Is Nothing Then Throw New ArgumentNullException(NameOf(parameters))
        If gradients Is Nothing Then Throw New ArgumentNullException(NameOf(gradients))
        If parameters.Count <> gradients.Count Then
            Throw New ArgumentException(
                $"参数个数 {parameters.Count} 与梯度个数 {gradients.Count} 不一致", NameOf(gradients))
        End If

        For i = 0 To parameters.Count - 1
            If parameters(i).Length <> gradients(i).Length Then
                Throw New ArgumentException(
                    $"第 {i} 个参数（{parameters(i).Length}）与其梯度（{gradients(i).Length}）长度不一致", NameOf(gradients))
            End If
        Next

        _parameters = parameters
        _gradients = gradients
        _momentum = New List(Of Tensor)()
        _velocity = New List(Of Tensor)()

        For Each parameter In parameters
            ' 矩估计必须与参数同形同序：优化器按扁平下标读写，形状不一致会静默错位
            Call _momentum.Add(New Tensor(CType(parameter.Shape.Clone(), Integer())))
            Call _velocity.Add(New Tensor(CType(parameter.Shape.Clone(), Integer())))
        Next

        Me.LearningRate = learningRate
        Me.WeightDecay = weightDecay
        Me.Beta1 = beta1
        Me.Beta2 = beta2
        Me.Epsilon = epsilon
    End Sub

    ''' <summary>已执行的最参数更新步数（偏差修正用）</summary>
    Public ReadOnly Property Steps As Long
        Get
            Return _steps
        End Get
    End Property

    ''' <summary>受管参数</summary>
    Public ReadOnly Property Parameters As List(Of Tensor)
        Get
            Return _parameters
        End Get
    End Property

    ''' <summary>执行一次参数更新。</summary>
    Public Sub [Step]()
        _steps += 1L

        Dim biasCorrection1 = 1.0 - std.Pow(Beta1, _steps)
        Dim biasCorrection2 = 1.0 - std.Pow(Beta2, _steps)

        For i = 0 To _parameters.Count - 1
            Dim parameter = _parameters(i)
            Dim gradient = _gradients(i)
            Dim momentum = _momentum(i)
            Dim velocity = _velocity(i)

            If UseDeviceKernel AndAlso
               Tensor.computeKernel.TryAdamWStep(parameter, gradient, momentum, velocity,
                                                 LearningRate, Beta1, Beta2, Epsilon,
                                                 biasCorrection1, biasCorrection2, WeightDecay) Then
                Continue For
            End If

            UpdateHost(parameter, gradient, momentum, velocity, biasCorrection1, biasCorrection2)
        Next
    End Sub

    ''' <summary>主机 fp64 更新循环（设备端不可用时的参考实现）。</summary>
    Private Sub UpdateHost(parameter As Tensor, gradient As Tensor,
                           momentum As Tensor, velocity As Tensor,
                           biasCorrection1 As Double, biasCorrection2 As Double)

        Dim p = parameter.Data
        Dim g = gradient.Data
        Dim m = momentum.Data
        Dim v = velocity.Data

        Dim beta1 = Me.Beta1
        Dim beta2 = Me.Beta2
        Dim lr = Me.LearningRate
        Dim eps = Me.Epsilon
        Dim wd = Me.WeightDecay
        Dim oneMinusBeta1 = 1.0 - beta1
        Dim oneMinusBeta2 = 1.0 - beta2

        For j = 0 To p.Length - 1
            Dim gj = g(j)
            Dim mj = beta1 * m(j) + oneMinusBeta1 * gj
            Dim vj = beta2 * v(j) + oneMinusBeta2 * gj * gj

            m(j) = mj
            v(j) = vj

            ' 偏差修正后再算步长；衰减项与自适应步长分开，这就是 AdamW 与 Adam+L2 的区别
            Dim adaptive = (mj / biasCorrection1) / (std.Sqrt(vj / biasCorrection2) + eps)

            p(j) -= lr * (adaptive + wd * p(j))
        Next

        Call parameter.MarkHostModified()
        Call momentum.MarkHostModified()
        Call velocity.MarkHostModified()
    End Sub

    ''' <summary>把全部梯度清零。</summary>
    Public Sub ZeroGrad()
        For Each gradient In _gradients
            Dim data = gradient.Data

            For i = 0 To data.Length - 1
                data(i) = 0.0
            Next

            Call gradient.MarkHostModified()
        Next
    End Sub

End Class

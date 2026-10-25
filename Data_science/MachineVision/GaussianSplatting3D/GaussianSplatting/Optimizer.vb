' ---------------------------------------------------------------------------
' Adam 优化器
'
' 性能要点：
'   * 直接操作扁平 Double() 数组（绕开 Tensor 索引器：其 setter 每次写入都会
'     Interlocked.Increment 版本号，逐元素写会白白付出原子操作代价）；
'   * 单遍 SIMD 内核同时更新 m / v / param（Vector(Of Double)，8 宽），
'     而不是三次独立的逐元素循环；
'   * 偏差校正 bc1 / bc2 在 IncrementStep 里算一次，不在每个元素上调用 Pow；
'   * 元素数超过阈值才并行（内存带宽受限的算子并行收益有限）。
' ---------------------------------------------------------------------------

Imports nv = System.Numerics
Imports std = System.Math

''' <summary>
''' Adam 优化器
'''
''' 参数更新规则：
'''   m_t = β₁·m_{t-1} + (1−β₁)·g_t
'''   v_t = β₂·v_{t-1} + (1−β₂)·g_t²
'''   θ_t = θ_{t-1} − lr·(m_t/(1−β₁^t)) / (√(v_t/(1−β₂^t)) + ε)
''' </summary>
Public Class AdamOptimizer

    ''' <summary>元素数超过该值才启用并行</summary>
    Public Const ParallelThreshold As Integer = 262144

    ''' <summary>学习率</summary>
    Public Property LearningRate As Double = 0.01

    ''' <summary>一阶矩衰减率</summary>
    Public Property Beta1 As Double = 0.9

    ''' <summary>二阶矩衰减率</summary>
    Public Property Beta2 As Double = 0.999

    ''' <summary>数值稳定项</summary>
    Public Property Epsilon As Double = 0.00000001

    ''' <summary>时间步</summary>
    Private _t As Integer = 0

    Private _invBc1 As Double = 1.0
    Private _invBc2 As Double = 1.0

    Public Sub New(Optional lr As Double = 0.01)
        Me.LearningRate = lr
    End Sub

    ''' <summary>当前时间步</summary>
    Public ReadOnly Property TimeStep As Integer
        Get
            Return _t
        End Get
    End Property

    Public Sub IncrementStep()
        _t += 1
        _invBc1 = 1.0 / (1.0 - std.Pow(Beta1, _t))
        _invBc2 = 1.0 / (1.0 - std.Pow(Beta2, _t))
    End Sub

    ''' <summary>对一组参数执行 Adam 更新（就地修改）。</summary>
    Public Sub [Step](param As Double(), grad As Double(), m As Double(), v As Double(),
                      Optional lrScale As Double = 1.0)
        If param Is Nothing OrElse param.Length = 0 Then Return

        Dim lr = LearningRate * lrScale
        Dim n = param.Length

        If n < ParallelThreshold Then
            Call UpdateKernel(param, grad, m, v, 0, n, lr)
            Return
        End If

        Dim chunks = std.Min(std.Max(1, Environment.ProcessorCount), std.Max(1, n \ 65536))
        Dim perChunk = (n + chunks - 1) \ chunks

        System.Threading.Tasks.Parallel.For(0, chunks,
            Sub(c)
                Dim from = c * perChunk
                Dim [to] = std.Min(n, from + perChunk)

                Call UpdateKernel(param, grad, m, v, from, [to], lr)
            End Sub)
    End Sub

    ''' <summary>SIMD 单遍 Adam 内核（区间 [from, to)）。</summary>
    Private Sub UpdateKernel(param As Double(), grad As Double(), m As Double(), v As Double(),
                             from As Integer, [to] As Integer, lr As Double)
        Dim width = nv.Vector(Of Double).Count
        Dim b1 = Beta1, b2 = Beta2
        Dim omB1 = 1.0 - b1, omB2 = 1.0 - b2
        Dim invBc1 = _invBc1, invBc2 = _invBc2
        Dim eps = Epsilon

        Dim vBeta1 As New nv.Vector(Of Double)(b1)
        Dim vOmB1 As New nv.Vector(Of Double)(omB1)
        Dim vBeta2 As New nv.Vector(Of Double)(b2)
        Dim vOmB2 As New nv.Vector(Of Double)(omB2)
        Dim vLr As New nv.Vector(Of Double)(-lr)
        Dim vInvBc1 As New nv.Vector(Of Double)(invBc1)
        Dim vInvBc2 As New nv.Vector(Of Double)(invBc2)
        Dim vEps As New nv.Vector(Of Double)(eps)

        Dim i = from
        Dim last = [to] - width

        ' 就地更新：尾部必须走标量（不能用"末块重叠"技巧，否则已经更新的元素会被改两次）
        Do While i <= last
            Dim g = New nv.Vector(Of Double)(grad, i)
            Dim mm = New nv.Vector(Of Double)(m, i)
            Dim vv = New nv.Vector(Of Double)(v, i)
            Dim pp = New nv.Vector(Of Double)(param, i)

            mm = vBeta1 * mm + vOmB1 * g
            vv = vBeta2 * vv + vOmB2 * g * g

            Dim mHat = mm * vInvBc1
            Dim vHat = vv * vInvBc2

            pp = pp + vLr * (mHat / (nv.Vector.SquareRoot(vHat) + vEps))

            mm.CopyTo(m, i)
            vv.CopyTo(v, i)
            pp.CopyTo(param, i)

            i += width
        Loop

        Dim inv1 = invBc1, inv2 = invBc2

        While i < [to]
            Dim g = grad(i)

            Dim mm = beta1 * m(i) + omB1 * g
            Dim vv = beta2 * v(i) + omB2 * g * g

            m(i) = mm
            v(i) = vv

            param(i) -= lr * (mm * inv1) / (std.Sqrt(vv * inv2) + eps)

            i += 1
        End While
    End Sub

    ''' <summary>对整个高斯模型执行一次 Adam 更新。</summary>
    Public Sub StepModel(model As GaussianModel)
        If model Is Nothing OrElse model.Count = 0 Then Return

        Call IncrementStep()

        ' 位置 / 颜色：标准学习率；缩放 / 旋转 / 不透明度：较低学习率
        [Step](model.PositionsData, model.PositionsGradData, model.PositionsMData, model.PositionsVData, 1.0)
        [Step](model.ScalesData, model.ScalesGradData, model.ScalesMData, model.ScalesVData, 0.1)
        [Step](model.RotationsData, model.RotationsGradData, model.RotationsMData, model.RotationsVData, 0.1)
        [Step](model.ColorsData, model.ColorsGradData, model.ColorsMData, model.ColorsVData, 1.0)
        [Step](model.OpacitiesData, model.OpacitiesGradData, model.OpacitiesMData, model.OpacitiesVData, 0.1)

        Call model.MarkModified()
    End Sub

End Class


''' <summary>
''' 学习率调度器（余弦退火）
''' </summary>
Public Class LRScheduler

    Private ReadOnly _initialLr As Double
    Private ReadOnly _finalLr As Double
    Private ReadOnly _totalSteps As Integer

    Public Sub New(initialLr As Double, finalLr As Double, totalSteps As Integer)
        _initialLr = initialLr
        _finalLr = finalLr
        _totalSteps = totalSteps
    End Sub

    ''' <summary>
    ''' 获取第 step 步的学习率（余弦退火）。
    ''' </summary>
    Public Function GetLr([step] As Integer) As Double
        If _totalSteps <= 0 Then Return _finalLr
        If [step] >= _totalSteps Then Return _finalLr

        Dim progress = [step] / _totalSteps
        Dim cosine = 0.5 * (1 + std.Cos(std.PI * progress))

        Return _finalLr + (_initialLr - _finalLr) * cosine
    End Function

End Class

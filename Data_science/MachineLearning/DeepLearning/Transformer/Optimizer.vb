' ---------------------------------------------------------------------------
' Transformer 专用的 Adam 优化器
'
' 迁移前它直接消费 AD 张量的 GetDerivatives()/ClearDerivatives()/MatAdd()；
' 迁移到纯数值 Tensor 之后，改为「参数张量 + 同形梯度累加器」的配对更新模型：
' 反向传播阶段往 Gradient 里原地累加，训练步结束时按 Adam 规则原地更新参数，
' 再把梯度清零。超参与旧实现保持一致（β1=0.9、β2=0.999、eps=1e-8）。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

Namespace Transformer

    ''' <summary>
    ''' 实现 Adam 优化器（参数 + 同形梯度累加器）。
    ''' </summary>
    Public Class Optimizer

        Private Const beta1 As Double = 0.9
        Private Const beta2 As Double = 0.999
        Private Const eps As Double = 0.00000001

        ''' <summary>一阶矩估计（与参数同形）。</summary>
        Private ReadOnly _m As Tensor

        ''' <summary>二阶矩估计（与参数同形）。</summary>
        Private ReadOnly _v As Tensor

        ''' <summary>梯度累加器（与参数同形）。</summary>
        Private ReadOnly _gradient As Tensor

        ''' <summary>
        ''' 与参数同形的梯度累加器；反向传播阶段由调用方往里原地累加。
        ''' </summary>
        Public ReadOnly Property Gradient As Tensor
            Get
                Return _gradient
            End Get
        End Property

        ''' <summary>按参数张量的形状创建优化器状态。</summary>
        Public Sub New(param As Tensor)
            _m = New Tensor(param.Shape)
            _v = New Tensor(param.Shape)
            _gradient = New Tensor(param.Shape)
        End Sub

        ''' <summary>把梯度累加器清零。</summary>
        Public Sub ZeroGrad()
            Call TensorOps.ZeroInPlace(_gradient)
        End Sub

        ''' <summary>
        ''' 按 Adam 规则原地更新参数，并在更新完成后清零梯度累加器。
        ''' </summary>
        ''' <param name="learningRate">学习率</param>
        ''' <param name="step">训练步序号（从 1 开始，用于偏置校正）</param>
        ''' <param name="param">待更新的参数张量</param>
        Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer, param As Tensor)
            Dim p = param.Data
            Dim g = _gradient.Data
            Dim m = _m.Data
            Dim v = _v.Data
            Dim bc1 = 1.0 - std.Pow(beta1, [step])
            Dim bc2 = 1.0 - std.Pow(beta2, [step])

            For i As Integer = 0 To p.Length - 1
                Dim gi = g(i)
                Dim mi = beta1 * m(i) + (1.0 - beta1) * gi
                Dim vi = beta2 * v(i) + (1.0 - beta2) * gi * gi
                Dim mHat = mi / bc1
                Dim vHat = vi / bc2

                m(i) = mi
                v(i) = vi
                p(i) -= learningRate * mHat / (std.Sqrt(vHat) + eps)
            Next

            Call param.MarkHostModified()
            Call ZeroGrad()
        End Sub

    End Class
End Namespace

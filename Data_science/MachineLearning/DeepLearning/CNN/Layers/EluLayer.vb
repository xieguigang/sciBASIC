' ---------------------------------------------------------------------------
' 指数线性单元（ELU, Exponential Linear Unit）
'
'   f(x) = x                  (x >= 0)
'   f(x) = alpha * (exp(x)-1) (x <  0)
'
' 导数：
'   f'(x) = 1                 (x >= 0)
'   f'(x) = f(x) + alpha      (x <  0)
'
' 与 ReLU 相比，负半轴是平滑的指数衰减而不是硬零，因此均值更接近 0、梯度不会突然中断；
' 这一点在强化学习的策略网络（PyTorch 导出的 ONNX 策略模型普遍使用 ELU）里尤其常见，
' 所以本层同时也是 ONNX 模型导入/导出时的标准激活层之一。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports std = System.Math

Namespace CNN.layers

    ''' <summary>
    ''' Exponential linear unit activation layer.
    ''' </summary>
    ''' <remarks>
    ''' 本层没有可训练参数，只做逐元素的非线性映射与梯度回传。
    ''' </remarks>
    Public Class EluLayer : Inherits DataLink
        Implements Layer

        ''' <summary>默认的 <c>alpha</c> 系数（与 ONNX 的 <c>Elu</c> 算子默认值一致）。</summary>
        Public Const DefaultAlpha As Double = 1.0

        ''' <summary>负半轴的缩放系数。</summary>
        Private alphaFactor As Double = DefaultAlpha

        ''' <summary>Gets the scaling factor applied to the negative half axis.</summary>
        ''' <returns>The <c>alpha</c> value.</returns>
        Public ReadOnly Property Alpha As Double
            Get
                Return alphaFactor
            End Get
        End Property

        ''' <summary>Gets the parameter and gradient blocks of this layer; the ELU layer has none.</summary>
        Public Overridable ReadOnly Iterator Property BackPropagationResult As IEnumerable(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                ' no data
            End Get
        End Property

        ''' <summary>Gets the kind of this layer, always <see cref="LayerTypes.Elu"/>.</summary>
        Public ReadOnly Property Type As LayerTypes Implements Layer.Type
            Get
                Return LayerTypes.Elu
            End Get
        End Property

        ''' <summary>Creates an ELU layer with the default <c>alpha = 1</c>.</summary>
        Sub New()
        End Sub

        ''' <summary>
        ''' Creates an ELU layer with the given negative half axis scale.
        ''' </summary>
        ''' <param name="alpha">Scaling factor of the negative half axis; must be positive.</param>
        Public Sub New(alpha As Double)
            If alpha <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(alpha), alpha, "ELU 的 alpha 必须为正数")
            End If

            Me.alphaFactor = alpha
        End Sub

        ''' <summary>
        ''' Applies the exponential linear unit element wise.
        ''' </summary>
        ''' <param name="db">The input data block.</param>
        ''' <param name="training">Ignored; the activation behaves the same in both modes.</param>
        ''' <returns>The activated data block.</returns>
        Public Overridable Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
            Dim out As DataBlock = db.clone()
            Dim src = db.Weights
            Dim dst = out.Weights

            in_act = db

            For i As Integer = 0 To dst.Length - 1
                Dim x As Double = src(i)

                If x >= 0 Then
                    dst(i) = x
                Else
                    dst(i) = alphaFactor * (std.Exp(x) - 1.0)
                End If
            Next

            Call out.MarkValueModified()

            out_act = out

            Return out_act
        End Function

        ''' <summary>
        ''' Backpropagates the gradient through the ELU curve.
        ''' </summary>
        Public Overridable Sub backward() Implements Layer.backward
            Dim V As DataBlock = in_act.clearGradient() ' we need to set dw of this
            Dim V2 As DataBlock = out_act
            Dim Vy = V.Weights ' 前向传播的输入值
            Dim V2w = V2.Weights ' 前向传播的输出值（负半轴的导数可以直接由它算出）
            Dim N As Integer = Vy.Length

            For i As Integer = 0 To N - 1
                Dim g As Double = V2.getGradient(i)

                If Vy(i) >= 0 Then
                    Call V.setGradient(i, g)
                Else
                    Call V.setGradient(i, g * (V2w(i) + alphaFactor))
                End If
            Next
        End Sub

        ''' <summary>Returns a short description of this layer.</summary>
        ''' <returns>A text of the form <c>elu(alpha=1)</c>.</returns>
        Public Overrides Function ToString() As String
            Return $"elu(alpha={alphaFactor})"
        End Function

    End Class

End Namespace

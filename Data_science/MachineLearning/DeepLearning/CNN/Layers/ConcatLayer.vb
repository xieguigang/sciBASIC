' ---------------------------------------------------------------------------
' 跳跃连接拼接层（U-Net 的 "copy and crop"）
'
' 它为什么不能做成普通层：CNN 的 Layer 契约是**单入单出**的
' (``forward(db, training)``)，而拼接需要两个输入——解码器上采样出来的特征图，
' 以及编码器对应层的特征图（跳跃连接）。
'
' 处理方式：主分支仍然走 forward 的入参，跳跃分支由计算图的执行器在调用
' forward 之前通过 SetSkip 注入（因为编码器每次前向都会产出新的 DataBlock，
' 不可能在构造期固定下来）。反向传播时把梯度按通道切成两段，分别累加回两侧，
' 累加而不是覆盖 —— 两侧的输入块都还可能有其它消费者。
' ---------------------------------------------------------------------------

Imports System.Runtime.Serialization
Imports Microsoft.VisualBasic.MachineLearning.CNN.data

Namespace CNN.layers

    ''' <summary>
    ''' 跳跃连接拼接层：把编码器特征图（必要时先中心裁剪）与本层输入沿**通道维**拼接起来。
    ''' </summary>
    ''' <remarks>
    ''' 布局与 <see cref="DataBlock"/> 一致，索引公式为 <c>(SX*y + x)*Depth + depth</c>，
    ''' 因此拼接是"逐像素把两段通道并排写入"，不涉及任何跨行搬运。
    ''' <para>
    ''' 本层没有可训练参数，<see cref="BackPropagationResult"/> 为空。
    ''' </para>
    ''' </remarks>
    Public Class ConcatLayer : Inherits DataLink
        Implements Layer

        ''' <summary>跳跃连接另一端的激活块；由计算图执行器在 forward 之前注入。</summary>
        <IgnoreDataMember> Private skipBlock As DataBlock
        ''' <summary>最近一次前向对跳跃块做的中心裁剪偏移（y, x）。</summary>
        <IgnoreDataMember> Private cropY As Integer
        <IgnoreDataMember> Private cropX As Integer
        ''' <summary>主分支的通道数（拼接后的前 <c>mainDepth</c> 个通道来自主分支）。</summary>
        <IgnoreDataMember> Private mainDepth As Integer

        ''' <summary>Gets the parameter and gradient blocks of this layer; the concat layer has none.</summary>
        Public ReadOnly Iterator Property BackPropagationResult As IEnumerable(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                ' no data
            End Get
        End Property

        ''' <summary>Gets the kind of this layer, always <see cref="LayerTypes.Concat"/>.</summary>
        Public ReadOnly Property Type As LayerTypes Implements Layer.Type
            Get
                Return LayerTypes.Concat
            End Get
        End Property

        ''' <summary>Creates an empty layer, used by the deserializer.</summary>
        Sub New()
        End Sub

        ''' <summary>
        ''' Creates a concat layer and updates the shared output definition with the merged channel count.
        ''' </summary>
        ''' <param name="def">The shared output definition that carries the shape produced so far.</param>
        ''' <param name="skipDepth">Number of channels contributed by the skip connection.</param>
        Public Sub New(def As OutputDefinition, skipDepth As Integer)
            If skipDepth <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(skipDepth), "跳跃连接的通道数必须为正数")
            End If

            def.outX = def.outX
            def.outY = def.outY
            def.depth = def.depth + skipDepth
        End Sub

        ''' <summary>
        ''' 设置跳跃连接的另一端。
        ''' </summary>
        ''' <param name="block">
        ''' 编码器对应层的激活块。它必须不比本层输入小（否则无法裁剪到同一尺寸），
        ''' 通道数必须与构造时声明的一致。
        ''' </param>
        ''' <remarks>
        ''' 必须在每次 <see cref="forward"/> 之前调用：编码器每跑一次前向都会产出新的
        ''' <see cref="DataBlock"/>，因此这个引用无法在构造期固定。
        ''' </remarks>
        Public Sub SetSkip(block As DataBlock)
            skipBlock = block
        End Sub

        ''' <summary>
        ''' Gets the number of channels of the concatenated output produced by the last forward pass.
        ''' </summary>
        Public ReadOnly Property OutDepth As Integer
            Get
                If out_act Is Nothing Then Return 0
                Return out_act.Depth
            End Get
        End Property

        ''' <summary>
        ''' 沿通道维拼接主分支与（中心裁剪后的）跳跃分支。
        ''' </summary>
        ''' <param name="db">主分支的输入块（解码器上采样结果）。</param>
        ''' <param name="training">Ignored; the concat layer behaves the same in both modes.</param>
        ''' <returns>拼接后的特征图，空间尺寸与 <paramref name="db"/> 相同。</returns>
        Public Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
            If skipBlock Is Nothing Then
                Throw New InvalidProgramException("concat layer: 缺少跳跃连接的输入, 请在 forward 之前调用 SetSkip")
            End If
            If db.SX > skipBlock.SX OrElse db.SY > skipBlock.SY Then
                Throw New InvalidProgramException(
                    $"concat layer: 跳跃块({skipBlock.SX}x{skipBlock.SY}) 小于主分支({db.SX}x{db.SY}), 无法裁剪对齐")
            End If

            in_act = db

            Dim w = db.SX, h = db.SY
            Dim d1 = db.Depth, d2 = skipBlock.Depth
            Dim total = d1 + d2

            cropX = (skipBlock.SX - w) \ 2
            cropY = (skipBlock.SY - h) \ 2

            mainDepth = d1

            Dim merged As New DataBlock(w, h, total, 0.0) With {.trace = Me.ToString}
            Dim buf(total * w * h - 1) As Double
            Dim src1 = db.Weights
            Dim src2 = skipBlock.Weights
            Dim sw = skipBlock.SX

            For y As Integer = 0 To h - 1
                Dim rowBase = y * w
                Dim skipRow = (y + cropY) * sw

                For x As Integer = 0 To w - 1
                    Dim oBase = (rowBase + x) * total
                    Dim mBase = (rowBase + x) * d1
                    Dim sBase = (skipRow + (x + cropX)) * d2

                    For c As Integer = 0 To d1 - 1
                        buf(oBase + c) = src1(mBase + c)
                    Next
                    For c As Integer = 0 To d2 - 1
                        buf(oBase + d1 + c) = src2(sBase + c)
                    Next
                Next
            Next

            Call merged.SetValues(buf)

            out_act = merged
            Return out_act
        End Function

        ''' <summary>
        ''' 把上游梯度按通道切成两段，分别写回主分支与跳跃分支。
        ''' </summary>
        Public Sub backward() Implements Layer.backward
            If skipBlock Is Nothing OrElse in_act Is Nothing Then Return

            Dim w = in_act.SX, h = in_act.SY
            Dim d1 = mainDepth, d2 = skipBlock.Depth
            Dim total = out_act.Depth
            Dim g = out_act.Gradients

            Dim gMain(w * h * d1 - 1) As Double
            Dim gSkip(skipBlock.SX * skipBlock.SY * d2 - 1) As Double
            Dim sw = skipBlock.SX

            For y As Integer = 0 To h - 1
                Dim rowBase = y * w
                Dim skipRow = (y + cropY) * sw

                For x As Integer = 0 To w - 1
                    Dim oBase = (rowBase + x) * total
                    Dim mBase = (rowBase + x) * d1
                    Dim sBase = (skipRow + (x + cropX)) * d2

                    For c As Integer = 0 To d1 - 1
                        gMain(mBase + c) = g(oBase + c)
                    Next
                    For c As Integer = 0 To d2 - 1
                        gSkip(sBase + c) = g(oBase + d1 + c)
                    Next
                Next
            Next

            Call WriteInputGradient(in_act, gMain)
            Call WriteInputGradient(skipBlock, gSkip)
        End Sub

        ''' <summary>Returns a short description of this layer.</summary>
        ''' <returns>A text of the form <c>concat(...)</c>.</returns>
        Public Overrides Function ToString() As String
            If out_act Is Nothing Then Return "concat()"
            Return $"concat(depth:{out_act.Depth})"
        End Function
    End Class

End Namespace

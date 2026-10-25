' ---------------------------------------------------------------------------
' U-Net 计算图执行器
'
' 为什么不是 ConvolutionalNN：
'   CNN 框架的网络是**严格串行**的（forward 把上一层的输出直接喂给下一层），
'   而 U-Net 有跳跃连接 —— 同一个特征图会同时喂给池化层（继续下采样）
'   和拼接层（与解码器上采样结果合并）。串行链表达不了扇出，
'   因此这里保留一个显式的**有向图**：节点 = 一个 CNN 层 + 输入节点下标，
'   前向按拓扑序、反向按逆序，拼接层的第二个输入由执行器在 forward 之前注入。
'
' 梯度语义（关键点）：
'   同一个激活块有两个消费者时，两条分支的梯度必须**相加**。
'   所以建图之后把所有层的 DataLink.AccumulateGradient 置 True，
'   并由本执行器在每次反向传播之前统一清零各激活块的梯度。
'   参数梯度（卷积核/偏置）不受影响，仍然跨 mini-batch 样本累加，
'   由训练器在更新之后清零。
' ---------------------------------------------------------------------------

Imports System.IO
Imports Microsoft.VisualBasic.MachineLearning.CNN
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.CNN.layers
Imports std = System.Math
Imports CnnLayer = Microsoft.VisualBasic.MachineLearning.CNN.layers.Layer

Namespace UNet

    ''' <summary>
    ''' U-Net 语义分割模型：对称编码器—解码器 + 跳跃连接。
    ''' </summary>
    Public Class UNetModel

        ''' <summary>
        ''' 计算图中的一个节点。
        ''' </summary>
        Private Class GraphNode

            ''' <summary>该节点对应的层。</summary>
            Public Property Layer As CnnLayer
            ''' <summary>
            ''' 输入节点下标；<c>-1</c> 表示模型的外部输入。
            ''' 长度为 2 时表示拼接层：[0] 主分支（解码器），[1] 跳跃分支（编码器）。
            ''' </summary>
            Public Property Inputs As Integer()
            ''' <summary>最近一次前向产出的激活块（反向传播前统一清零梯度用）。</summary>
            Public Property Output As DataBlock
            ''' <summary>建图时记录下来的结构描述。</summary>
            Public Property Kind As String

            Public Overrides Function ToString() As String
                Return Kind
            End Function

        End Class

        Private ReadOnly opt As UNetOptions
        Private ReadOnly nodes As New List(Of GraphNode)
        Private ReadOnly skipNodes As New List(Of Integer)
        Private lastOutput As DataBlock
        Private paramBlocks As BackPropResult()

        ''' <summary>本模型的配置。</summary>
        Public ReadOnly Property Options As UNetOptions
            Get
                Return opt
            End Get
        End Property

        ''' <summary>最近一次前向的输出（每像素类别 logits）。</summary>
        Public ReadOnly Property Output As DataBlock
            Get
                Return lastOutput
            End Get
        End Property

        ''' <summary>图的节点数（含所有卷积/激活/池化/上采样/拼接层）。</summary>
        Public ReadOnly Property NodeCount As Integer
            Get
                Return nodes.Count
            End Get
        End Property

        ''' <summary>
        ''' 创建并建好整张计算图。
        ''' </summary>
        ''' <param name="options">结构超参数。</param>
        Public Sub New(options As UNetOptions)
            If options Is Nothing Then
                Throw New ArgumentNullException(NameOf(options))
            End If

            Call options.Validate()

            Me.opt = options
            Call BuildGraph()
            Call EnableGraphGradientMode()
            Call CollectParameters()
        End Sub

        ' ---------------- 建图 ----------------

        Private Sub BuildGraph()
            Dim def As New OutputDefinition(opt.InputWidth, opt.InputHeight, opt.InputChannels)

            ' 编码器：Levels 层，每层 (conv → relu → conv → relu → pool)
            For level As Integer = 0 To opt.Levels - 1
                Dim channels = opt.ChannelsOf(level)

                Call AddConv(def, channels, $"enc{level}a")
                Call AddRelu(def, $"enc{level}a_relu")
                Call AddConv(def, channels, $"enc{level}b")
                Call AddRelu(def, $"enc{level}b_relu")

                ' 跳跃连接的源：本层最后一个激活块
                skipNodes.Add(nodes.Count - 1)

                Call AddPool(def, $"enc{level}_pool")
            Next

            ' 瓶颈层
            Call AddConv(def, opt.BottleneckChannels, "bottom_a")
            Call AddRelu(def, "bottom_a_relu")
            Call AddConv(def, opt.BottleneckChannels, "bottom_b")
            Call AddRelu(def, "bottom_b_relu")

            ' 解码器：按相反顺序 (上采样 → 拼接 → conv → relu → conv → relu)
            For level As Integer = opt.Levels - 1 To 0 Step -1
                Dim channels = opt.ChannelsOf(level)

                Call AddUpConv(def, channels, $"dec{level}_up")
                Call AddConcat(def, skipNodes(level), channels, $"dec{level}_concat")
                Call AddConv(def, channels, $"dec{level}a")
                Call AddRelu(def, $"dec{level}a_relu")
                Call AddConv(def, channels, $"dec{level}b")
                Call AddRelu(def, $"dec{level}b_relu")
            Next

            ' 输出层：1×1 卷积，把通道压到类别数
            Call AddConv(def, opt.NumClasses, "out_1x1", kernel:=1, padding:=0)

            If def.outX <> opt.InputWidth OrElse def.outY <> opt.InputHeight Then
                Throw New InvalidProgramException(
                    $"U-Net 输出尺寸 {def.outX}x{def.outY} 与输入 {opt.InputWidth}x{opt.InputHeight} 不一致")
            End If
            If def.depth <> opt.NumClasses Then
                Throw New InvalidProgramException(
                    $"U-Net 输出通道数 {def.depth} 与类别数 {opt.NumClasses} 不一致")
            End If
        End Sub

        Private Function AddNode(layer As CnnLayer, kind As String, ParamArray inputs As Integer()) As Integer
            nodes.Add(New GraphNode With {
                .Layer = layer,
                .Inputs = inputs,
                .Kind = kind
            })

            Return nodes.Count - 1
        End Function

        Private Sub AddConv(def As OutputDefinition, channels As Integer, kind As String,
                            Optional kernel As Integer = -1,
                            Optional padding As Integer = -1)
            Dim k = If(kernel > 0, kernel, opt.ConvKernel)
            Dim p = If(padding >= 0, padding, opt.ConvPadding)
            Dim prev = nodes.Count - 1

            Call AddNode(New ConvolutionLayer(def, k, channels, stride:=1, padding:=p), kind, prev)
        End Sub

        Private Sub AddRelu(def As OutputDefinition, kind As String)
            Dim prev = nodes.Count - 1

            Call AddNode(New RectifiedLinearUnitsLayer(), kind, prev)
        End Sub

        Private Sub AddPool(def As OutputDefinition, kind As String)
            Dim prev = nodes.Count - 1

            Call AddNode(New PoolingLayer(def, 2, stride:=2, padding:=0), kind, prev)
        End Sub

        Private Sub AddUpConv(def As OutputDefinition, channels As Integer, kind As String)
            Dim prev = nodes.Count - 1

            Call AddNode(New Conv2DTransposeLayer(def, opt.UpKernel, channels, stride:=2,
                                                  padding:=0, useBias:=opt.UseBias), kind, prev)
        End Sub

        Private Sub AddConcat(def As OutputDefinition, skipNode As Integer, skipDepth As Integer, kind As String)
            Dim prev = nodes.Count - 1

            Call AddNode(New ConcatLayer(def, skipDepth), kind, prev, skipNode)
        End Sub

        ''' <summary>
        ''' 图模式下把所有层切成"累加式"梯度写入，并在每次反向之前统一清零激活梯度。
        ''' </summary>
        Private Sub EnableGraphGradientMode()
            For Each n In nodes
                Dim link = TryCast(n.Layer, DataLink)

                If link IsNot Nothing Then
                    link.AccumulateGradient = True
                End If
            Next
        End Sub

        Private Sub CollectParameters()
            Dim list As New List(Of BackPropResult)

            For Each n In nodes
                For Each block In n.Layer.BackPropagationResult
                    list.Add(block)
                Next
            Next

            paramBlocks = list.ToArray()
        End Sub

        ''' <summary>
        ''' 全部可训练参数块（卷积核 + 偏置），顺序由建图顺序唯一确定。
        ''' </summary>
        Public ReadOnly Property Parameters As BackPropResult()
            Get
                Return paramBlocks
            End Get
        End Property

        ''' <summary>参数总量（用于诊断输出）。</summary>
        Public ReadOnly Property ParameterCount As Long
            Get
                Dim total As Long = 0

                For Each p In paramBlocks
                    total += p.Weights.LongLength
                Next

                Return total
            End Get
        End Property

        ' ---------------- 前向 / 反向 ----------------

        ''' <summary>创建一个符合本模型输入形状的全零数据块。</summary>
        Public Function CreateInput() As DataBlock
            Return New DataBlock(opt.InputWidth, opt.InputHeight, opt.InputChannels, 0.0)
        End Function

        ''' <summary>
        ''' 按拓扑序跑一遍前向。
        ''' </summary>
        ''' <param name="x">输入数据块，形状必须与 <see cref="Options"/> 一致。</param>
        ''' <param name="training">是否训练模式（影响 dropout 之类的层；本网络的层不区分）。</param>
        ''' <returns>输出块，形状 (InputWidth, InputHeight, NumClasses)，内容是每像素的 logits。</returns>
        Public Function Forward(x As DataBlock, Optional training As Boolean = False) As DataBlock
            Dim produced(nodes.Count - 1) As DataBlock

            For i As Integer = 0 To nodes.Count - 1
                Dim n = nodes(i)
                Dim inputIndex = n.Inputs(0)
                Dim inputBlock = If(inputIndex < 0, x, produced(inputIndex))

                If n.Inputs.Length > 1 Then
                    ' 拼接层的第二个输入是跳跃连接，必须在 forward 之前注入当前这次前向产出的块
                    Dim concat = TryCast(n.Layer, ConcatLayer)

                    If concat Is Nothing Then
                        Throw New InvalidProgramException($"节点 {n.Kind} 有多个输入但不是拼接层")
                    End If

                    Call concat.SetSkip(produced(n.Inputs(1)))
                End If

                Dim y = n.Layer.forward(inputBlock, training)

                produced(i) = y
                n.Output = y
            Next

            lastOutput = produced(nodes.Count - 1)
            Return lastOutput
        End Function

        ''' <summary>
        ''' 从输出块开始反向传播。
        ''' </summary>
        ''' <param name="dLogits">损失对输出 logits 的梯度，长度 = 宽×高×类别数。</param>
        ''' <remarks>
        ''' 会先把所有**激活块**的梯度清零（参数块不清），再写入输出梯度，最后逆序逐层 backward。
        ''' </remarks>
        Public Sub Backward(dLogits As Double())
            If lastOutput Is Nothing Then
                Throw New InvalidProgramException("请先调用 Forward 再调用 Backward")
            End If

            For Each n In nodes
                If n.Output IsNot Nothing Then
                    Call n.Output.clearGradient()
                End If
            Next

            Call lastOutput.setGradient(dLogits)

            For i As Integer = nodes.Count - 1 To 0 Step -1
                Call nodes(i).Layer.backward()
            Next
        End Sub

        ' ---------------- 推理 ----------------

        ''' <summary>
        ''' 预测每像素的类别掩码。
        ''' </summary>
        ''' <param name="x">输入数据块。</param>
        ''' <returns>长度 宽×高 的类别下标数组。</returns>
        Public Function Segment(x As DataBlock) As Integer()
            Dim logits = Forward(x, training:=False)
            Dim values = logits.Weights
            Dim pixels = logits.SX * logits.SY
            Dim classes = logits.Depth
            Dim mask(pixels - 1) As Integer

            For p As Integer = 0 To pixels - 1
                Dim baseIndex = p * classes
                Dim best As Integer = 0
                Dim bestValue = values(baseIndex)

                For c As Integer = 1 To classes - 1
                    If values(baseIndex + c) > bestValue Then
                        bestValue = values(baseIndex + c)
                        best = c
                    End If
                Next

                mask(p) = best
            Next

            Return mask
        End Function

        ''' <summary>
        ''' 预测每像素的类别概率（逐像素 softmax）。
        ''' </summary>
        ''' <param name="x">输入数据块。</param>
        ''' <returns>长度 宽×高×类别数 的概率数组。</returns>
        Public Function Probabilities(x As DataBlock) As Double()
            Dim logits = Forward(x, training:=False)
            Dim values = logits.Weights
            Dim pixels = logits.SX * logits.SY
            Dim classes = logits.Depth
            Dim probs(values.Length - 1) As Double

            For p As Integer = 0 To pixels - 1
                Dim baseIndex = p * classes
                Dim maxValue = values(baseIndex)

                For c As Integer = 1 To classes - 1
                    If values(baseIndex + c) > maxValue Then
                        maxValue = values(baseIndex + c)
                    End If
                Next

                Dim sum As Double = 0

                For c As Integer = 0 To classes - 1
                    Dim e = std.Exp(values(baseIndex + c) - maxValue)
                    probs(baseIndex + c) = e
                    sum += e
                Next

                For c As Integer = 0 To classes - 1
                    probs(baseIndex + c) /= sum
                Next
            Next

            Return probs
        End Function

        ' ---------------- 存档 ----------------

        Private Shared ReadOnly Magic As Byte() = New Byte() {&H55, &H4E, &H45, &H54}

        ''' <summary>
        ''' 把结构与参数写成二进制存档。
        ''' </summary>
        ''' <param name="path">目标文件路径。</param>
        Public Sub Save(path As String)
            Using fs As New FileStream(path, FileMode.Create, FileAccess.Write)
                Using bw As New BinaryWriter(fs)
                    Call bw.Write(Magic)
                    Call bw.Write(1I) ' 版本号

                    Call bw.Write(opt.InputWidth)
                    Call bw.Write(opt.InputHeight)
                    Call bw.Write(opt.InputChannels)
                    Call bw.Write(opt.NumClasses)
                    Call bw.Write(opt.Levels)
                    Call bw.Write(opt.BaseFilters)
                    Call bw.Write(opt.ConvKernel)
                    Call bw.Write(opt.ConvPadding)
                    Call bw.Write(opt.UpKernel)
                    Call bw.Write(opt.UseBias)

                    Dim blocks = Parameters

                    Call bw.Write(blocks.Length)

                    For Each block In blocks
                        Call bw.Write(block.Weights.Length)

                        For Each value In block.Weights
                            Call bw.Write(value)
                        Next
                    Next
                End Using
            End Using
        End Sub

        ''' <summary>
        ''' 从二进制存档恢复模型（结构与参数一起还原）。
        ''' </summary>
        ''' <param name="path">存档路径。</param>
        ''' <returns>恢复出来的模型。</returns>
        Public Shared Function Load(path As String) As UNetModel
            Using fs As New FileStream(path, FileMode.Open, FileAccess.Read)
                Using br As New BinaryReader(fs)
                    Dim head = br.ReadBytes(4)

                    If head.Length <> 4 OrElse head(0) <> Magic(0) OrElse head(1) <> Magic(1) OrElse
                       head(2) <> Magic(2) OrElse head(3) <> Magic(3) Then
                        Throw New InvalidDataException("不是有效的 U-Net 模型存档")
                    End If

                    Dim version = br.ReadInt32()

                    If version <> 1 Then
                        Throw New InvalidDataException($"不支持的存档版本: {version}")
                    End If

                    Dim opt As New UNetOptions With {
                        .InputWidth = br.ReadInt32(),
                        .InputHeight = br.ReadInt32(),
                        .InputChannels = br.ReadInt32(),
                        .NumClasses = br.ReadInt32(),
                        .Levels = br.ReadInt32(),
                        .BaseFilters = br.ReadInt32(),
                        .ConvKernel = br.ReadInt32(),
                        .ConvPadding = br.ReadInt32(),
                        .UpKernel = br.ReadInt32(),
                        .UseBias = br.ReadBoolean()
                    }

                    Dim model As New UNetModel(opt)
                    Dim blocks = model.Parameters
                    Dim count = br.ReadInt32()

                    If count <> blocks.Length Then
                        Throw New InvalidDataException(
                            $"存档中的参数块数量 {count} 与结构重建出的 {blocks.Length} 不一致")
                    End If

                    For i As Integer = 0 To count - 1
                        Dim n = br.ReadInt32()

                        If n <> blocks(i).Weights.Length Then
                            Throw New InvalidDataException($"第 {i} 个参数块长度不符: 存档 {n}, 模型 {blocks(i).Weights.Length}")
                        End If

                        Dim target = blocks(i).Weights

                        For j As Integer = 0 To n - 1
                            target(j) = br.ReadDouble()
                        Next

                        ' 权重数组是绕过 Tensor 索引器就地写入的，必须通知宿主块让设备端缓存失效
                        Call blocks(i).NotifyModified()
                    Next

                    Return model
                End Using
            End Using
        End Function

        ''' <summary>逐层列出网络结构，便于核对形状。</summary>
        Public Function Describe() As String
            Dim lines As New List(Of String)

            lines.Add($"U-Net {opt}")
            lines.Add($"节点 {nodes.Count} 个, 参数块 {paramBlocks.Length} 个, 参数总量 {ParameterCount:N0}")

            For i As Integer = 0 To nodes.Count - 1
                Dim n = nodes(i)
                Dim shape = "-"

                If n.Output IsNot Nothing Then
                    shape = $"{n.Output.SX}x{n.Output.SY}x{n.Output.Depth}"
                End If

                lines.Add($"  [{i:D3}] {n.Kind,-16} {n.Layer.Type.ToString(),-16} {shape}")
            Next

            Return String.Join(Environment.NewLine, lines)
        End Function

        ''' <summary>单行概述。</summary>
        Public Overrides Function ToString() As String
            Return $"U-Net({opt}, nodes:{nodes.Count}, params:{ParameterCount})"
        End Function

    End Class

End Namespace

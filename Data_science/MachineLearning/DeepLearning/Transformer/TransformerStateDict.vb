' ---------------------------------------------------------------------------
' TransformerStateDict —— SequenceTransformer 与 PyTorch state_dict 之间的键名映射
'
' 官方 smiles-transformer 权重（trfm_*.pkl）是
'     torch.save(TrfmSeq2seq(...).state_dict(), path)
' 的产物，键名遵循 nn.Transformer 的默认布局：
'     embed.weight / pe /
'     trfm.encoder.layers.{N}.{self_attn.*, linear1.*, linear2.*, norm1.*, norm2.*} /
'     trfm.encoder.norm.* /
'     trfm.decoder.layers.{N}.{self_attn.*, multihead_attn.*, linear1.*, linear2.*, norm1/2/3.*} /
'     trfm.decoder.norm.* / out.weight / out.bias
'
' 三处形状差异必须在这一层消化（其余代码一律按 VB 内部布局工作）：
'   1. nn.MultiheadAttention 用 fused in_proj_weight [3H, H]；本框架按 head 分开存；
'   2. nn.Linear.weight 是 [out, in]；本框架存 [in, out]（需要转置）；
'   3. pe 在 torch 里是 [1, T, H] 的 buffer；本框架存 [T, H]。
'
' 本类只依赖"名称 → Tensor"的字典，不依赖 MLDataStorage（后者反过来依赖本程序集），
' 因此读取 .pt/.pkl 的工作留给调用方：PtFile.LoadStateDict(...).AsTensors()。
' ---------------------------------------------------------------------------

Imports System.IO
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

Namespace Transformer

    ''' <summary>
    ''' 一次权重加载的结果：缺失键与多余键。
    ''' </summary>
    Public Class TransformerLoadReport

        Private ReadOnly _missing As IReadOnlyList(Of String)
        Private ReadOnly _unexpected As IReadOnlyList(Of String)

        Friend Sub New(missing As IReadOnlyList(Of String), unexpected As IReadOnlyList(Of String))
            _missing = missing
            _unexpected = unexpected
        End Sub

        ''' <summary>模型需要但权重文件中缺失的键。</summary>
        Public ReadOnly Property MissingKeys As IReadOnlyList(Of String)
            Get
                Return _missing
            End Get
        End Property

        ''' <summary>权重文件中有但模型不使用的键。</summary>
        Public ReadOnly Property UnexpectedKeys As IReadOnlyList(Of String)
            Get
                Return _unexpected
            End Get
        End Property

        ''' <summary>是否完全匹配（无缺失、无多余）。</summary>
        Public ReadOnly Property IsPerfectMatch As Boolean
            Get
                Return _missing.Count = 0 AndAlso _unexpected.Count = 0
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"missing={_missing.Count}, unexpected={_unexpected.Count}"
        End Function

    End Class

    ''' <summary>
    ''' <see cref="SequenceTransformer"/> 与 PyTorch <c>state_dict</c> 的互转。
    ''' </summary>
    ''' <remarks>
    ''' 严格模式下缺失键与多余键都会抛异常（与 <c>load_state_dict(strict=True)</c> 一致）；
    ''' 非严格模式只记录到 <see cref="TransformerLoadReport"/>。
    ''' </remarks>
    Public Class TransformerStateDict

        ''' <summary>
        ''' 键名别名：官方实现把 <c>PositionalEncoding</c> 模块挂在 <c>self.pe</c> 上，
        ''' 其 <c>register_buffer('pe', ...)</c> 在 state_dict 里就是 <c>pe.pe</c>；
        ''' 某些实现则直接命名为 <c>pe</c>。两者都接受。
        ''' </summary>
        Private Shared ReadOnly Aliases As New Dictionary(Of String, String)(StringComparer.Ordinal) From {
            {"pe.pe", "pe"}
        }

        ''' <summary>一个 state_dict 条目与模型内部张量之间的映射。</summary>
        Private Class Slot
            ''' <summary>PyTorch 侧的键名。</summary>
            Public Name As String
            ''' <summary>PyTorch 侧的形状。</summary>
            Public Shape As Integer()
            ''' <summary>把 torch 布局的展平数据写入模型参数。</summary>
            Public Push As Action(Of Double())
            ''' <summary>把模型参数导出为 torch 布局的展平数据。</summary>
            Public Pull As Func(Of Double())
            ''' <summary>
            ''' 允许文件中的"时间维"更长（仅用于 <c>pe</c> 这种按 max_len 生成的 buffer：
            ''' 官方模型把它生成到 5000 行，而模型只需要前 220 行）。
            ''' </summary>
            Public AllowLonger As Boolean = False
        End Class

        ''' <summary>
        ''' 把 <paramref name="state"/> 中的权重复制进 <paramref name="model"/>。
        ''' </summary>
        ''' <param name="model">目标模型。</param>
        ''' <param name="state">名称 → 张量（通常来自 <c>PtFile.LoadStateDict(path).AsTensors()</c>）。</param>
        ''' <param name="strict">严格模式：任何缺失/多余键都抛异常。</param>
        ''' <exception cref="InvalidDataException">张量形状与模型不匹配，或严格模式下键集合不一致。</exception>
        Public Shared Function Apply(model As SequenceTransformer,
                                     state As IDictionary(Of String, Tensor),
                                     Optional strict As Boolean = True) As TransformerLoadReport
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If state Is Nothing Then Throw New ArgumentNullException(NameOf(state))

            Dim missing As New List(Of String)
            Dim unexpected As New List(Of String)
            Dim consumed As New HashSet(Of String)(StringComparer.Ordinal)

            For Each slot As Slot In buildSlots(model)
                Dim tensor As Tensor = Nothing
                Dim actualName As String = slot.Name

                If Not state.TryGetValue(actualName, tensor) OrElse tensor Is Nothing Then
                    Dim aliasName As String = Nothing

                    If Aliases.TryGetValue(slot.Name, aliasName) AndAlso
                       state.TryGetValue(aliasName, tensor) AndAlso tensor IsNot Nothing Then
                        actualName = aliasName
                    Else
                        missing.Add(slot.Name)
                        Continue For
                    End If
                End If

                consumed.Add(actualName)

                If Not shapeMatches(slot, tensor) Then
                    Throw New InvalidDataException(
                        $"权重 '{slot.Name}' 形状不匹配：文件中为 [{String.Join(", ", tensor.Shape)}]，" &
                        $"模型期望 [{String.Join(", ", slot.Shape)}]")
                End If

                Call slot.Push(tensor.Data)
            Next

            For Each name As String In state.Keys
                If Not consumed.Contains(name) Then unexpected.Add(name)
            Next

            If strict Then
                If missing.Count > 0 Then
                    Throw New InvalidDataException("state_dict 缺少以下键：" & String.Join(", ", missing))
                End If

                If unexpected.Count > 0 Then
                    Throw New InvalidDataException("state_dict 包含模型不认识的多余键：" & String.Join(", ", unexpected))
                End If
            End If

            Return New TransformerLoadReport(missing, unexpected)
        End Function

        ''' <summary>
        ''' 判断文件中的张量形状是否可接受。
        ''' </summary>
        ''' <remarks>
        ''' 一般情况下必须完全一致；<see cref="Slot.AllowLonger"/> 为 <c>True</c> 时（仅 <c>pe</c>），
        ''' 允许时间维更长，因为官方实现按 <c>max_len=5000</c> 生成位置编码 buffer。
        ''' </remarks>
        Private Shared Function shapeMatches(slot As Slot, tensor As Tensor) As Boolean
            If tensor.Shape.SequenceEqual(slot.Shape) Then Return True
            If Not slot.AllowLonger Then Return False
            If tensor.Rank <> slot.Shape.Length Then Return False
            If tensor.Shape(0) <> slot.Shape(0) Then Return False
            If tensor.Shape(tensor.Rank - 1) <> slot.Shape(slot.Shape.Length - 1) Then Return False

            Return tensor.Shape(tensor.Rank - 2) >= slot.Shape(slot.Shape.Length - 2)
        End Function

        ''' <summary>
        ''' 把模型参数导出为 <c>state_dict</c>（调用方可用 <c>PtStateDict.FromTensors</c> 落盘）。
        ''' </summary>
        ''' <param name="model">源模型。</param>
        Public Shared Function Export(model As SequenceTransformer) As Dictionary(Of String, Tensor)
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))

            Dim result As New Dictionary(Of String, Tensor)(StringComparer.Ordinal)

            For Each slot As Slot In buildSlots(model)
                result(slot.Name) = New Tensor(slot.Pull(), slot.Shape)
            Next

            Return result
        End Function

        ''' <summary>构建全部映射项。</summary>
        Private Shared Function buildSlots(model As SequenceTransformer) As List(Of Slot)
            Dim slots As New List(Of Slot)
            Dim h As Integer = model.HiddenSize

            ' 词嵌入与位置编码 buffer
            slots.Add(flatSlot("embed.weight", model.EmbeddingLayer.Weights))
            slots.Add(bufferSlot("pe.pe", model.EmbeddingLayer.Positional, New Integer() {1, model.SequenceLength, h}))

            For i As Integer = 0 To model.NLayers - 1
                Dim layer As SequenceEncoderLayer = model.EncoderLayers(i)
                Dim prefix As String = $"trfm.encoder.layers.{i}"

                Call addAttention(slots, prefix & ".self_attn", layer.Attention, model.NHeads, h)
                Call addFeedForward(slots, prefix, layer.FeedForwardLayer)
                Call addNorm(slots, prefix & ".norm1", layer.Norm1)
                Call addNorm(slots, prefix & ".norm2", layer.Norm2)
            Next

            Call addNorm(slots, "trfm.encoder.norm", model.EncoderNorm)

            For i As Integer = 0 To model.NLayers - 1
                Dim layer As SequenceDecoderLayer = model.DecoderLayers(i)
                Dim prefix As String = $"trfm.decoder.layers.{i}"

                Call addAttention(slots, prefix & ".self_attn", layer.SelfAttentionLayer, model.NHeads, h)
                Call addAttention(slots, prefix & ".multihead_attn", layer.CrossAttentionLayer, model.NHeads, h)
                Call addFeedForward(slots, prefix, layer.FeedForwardLayer)
                Call addNorm(slots, prefix & ".norm1", layer.Norm1)
                Call addNorm(slots, prefix & ".norm2", layer.Norm2)
                Call addNorm(slots, prefix & ".norm3", layer.Norm3)
            Next

            Call addNorm(slots, "trfm.decoder.norm", model.DecoderNorm)

            ' nn.Linear 的 [out, in] → 本框架的 [in, out]
            slots.Add(transposedSlot("out.weight", model.OutputWeights))
            slots.Add(flatSlot("out.bias", model.OutputBias))

            Return slots
        End Function

        ''' <summary>形状一致、无转置的映射项。</summary>
        Private Shared Function flatSlot(name As String, value As Tensor) As Slot
            Return New Slot With {
                .Name = name,
                .Shape = CType(value.Shape.Clone(), Integer()),
                .Push = Sub(data) copyInto(value, data),
                .Pull = Function() CType(value.Data.Clone(), Double())
            }
        End Function

        ''' <summary>二维转置映射项（torch 为 <c>[out, in]</c>，模型内为 <c>[in, out]</c>）。</summary>
        Private Shared Function transposedSlot(name As String, value As Tensor) As Slot
            Dim rows As Integer = value.Shape(0)
            Dim cols As Integer = value.Shape(1)

            Return New Slot With {
                .Name = name,
                .Shape = New Integer() {cols, rows},
                .Push = Sub(data)
                            For r As Integer = 0 To rows - 1
                                For c As Integer = 0 To cols - 1
                                    value.Data(r * cols + c) = data(c * rows + r)
                                Next
                            Next

                            Call value.MarkHostModified()
                        End Sub,
                .Pull = Function()
                            Dim data(rows * cols - 1) As Double

                            For r As Integer = 0 To rows - 1
                                For c As Integer = 0 To cols - 1
                                    data(c * rows + r) = value.Data(r * cols + c)
                                Next
                            Next

                            Return data
                        End Function
            }
        End Function

        ''' <summary>
        ''' 只校验长度、不改变布局的映射项（用于 <c>pe</c> 这类 buffer）。
        ''' </summary>
        Private Shared Function bufferSlot(name As String, value As Tensor, shape As Integer()) As Slot
            Return New Slot With {
                .Name = name,
                .Shape = shape,
                .AllowLonger = True,
                .Push = Sub(data)
                            If data.Length < value.Length Then
                                Throw New InvalidDataException(
                                    $"buffer '{name}' 的数据长度（{data.Length}）小于模型所需（{value.Length}）")
                            End If

                            Array.Copy(data, 0, value.Data, 0, value.Length)
                            Call value.MarkHostModified()
                        End Sub,
                .Pull = Function()
                            Dim data(value.Length - 1) As Double

                            Array.Copy(value.Data, data, value.Length)

                            Return data
                        End Function
            }
        End Function

        ''' <summary>把一个 LayerNorm 的 <c>weight</c>/<c>bias</c> 加入映射。</summary>
        Private Shared Sub addNorm(slots As List(Of Slot), prefix As String, norm As LayerNormAffine)
            slots.Add(flatSlot(prefix & ".weight", norm.Gamma))
            slots.Add(flatSlot(prefix & ".bias", norm.Beta))
        End Sub

        ''' <summary>把一个前馈网络的两组线性层加入映射。</summary>
        Private Shared Sub addFeedForward(slots As List(Of Slot), prefix As String, ff As FeedForwardNetwork)
            slots.Add(transposedSlot(prefix & ".linear1.weight", ff.Weights1))
            slots.Add(flatSlot(prefix & ".linear1.bias", ff.Bias1))
            slots.Add(transposedSlot(prefix & ".linear2.weight", ff.Weights2))
            slots.Add(flatSlot(prefix & ".linear2.bias", ff.Bias2))
        End Sub

        ''' <summary>
        ''' 把一个多头注意力加入映射：fused <c>in_proj_weight</c>/<c>in_proj_bias</c>
        ''' 按 Q|K|V 分块后再按 head 切分。
        ''' </summary>
        Private Shared Sub addAttention(slots As List(Of Slot), prefix As String, attn As MultiHeadAttention,
                                        nHeads As Integer, embeddingSize As Integer)
            Dim dk As Integer = attn.QueryWeights(0).Shape(1)
            Dim dv As Integer = attn.ValueWeights(0).Shape(1)

            slots.Add(New Slot With {
                .Name = prefix & ".in_proj_weight",
                .Shape = New Integer() {embeddingSize * 3, embeddingSize},
                .Push = Sub(data) scatterQkvWeights(attn, nHeads, embeddingSize, dk, dv, data),
                .Pull = Function() gatherQkvWeights(attn, nHeads, embeddingSize, dk, dv)
            })

            slots.Add(New Slot With {
                .Name = prefix & ".in_proj_bias",
                .Shape = New Integer() {embeddingSize * 3},
                .Push = Sub(data) scatterQkvBias(attn, nHeads, embeddingSize, dk, dv, data),
                .Pull = Function() gatherQkvBias(attn, nHeads, embeddingSize, dk, dv)
            })

            slots.Add(transposedSlot(prefix & ".out_proj.weight", attn.OutputWeights))
            slots.Add(flatSlot(prefix & ".out_proj.bias", attn.OutputBias))
        End Sub

        ''' <summary>把 fused 权重 <c>[3H, H]</c> 拆到每个 head 的 <c>[H, dk]</c> 张量。</summary>
        Private Shared Sub scatterQkvWeights(attn As MultiHeadAttention, nHeads As Integer, embeddingSize As Integer,
                                             dk As Integer, dv As Integer, data As Double())
            Dim blocks As Tensor()() = {attn.QueryWeights, attn.KeyWeights, attn.ValueWeights}
            Dim dims As Integer() = {dk, dk, dv}
            Dim rowBase As Integer = 0

            For block As Integer = 0 To 2
                For head As Integer = 0 To nHeads - 1
                    Dim target As Tensor = blocks(block)(head)
                    Dim blockDim As Integer = dims(block)

                    For r As Integer = 0 To blockDim - 1
                        Dim torchRow As Integer = rowBase + head * blockDim + r

                        For c As Integer = 0 To embeddingSize - 1
                            target.Data(c * blockDim + r) = data(torchRow * embeddingSize + c)
                        Next
                    Next

                    Call target.MarkHostModified()
                Next

                rowBase += embeddingSize
            Next
        End Sub

        ''' <summary>把每个 head 的 <c>[H, dk]</c> 权重拼回 fused <c>[3H, H]</c>。</summary>
        Private Shared Function gatherQkvWeights(attn As MultiHeadAttention, nHeads As Integer, embeddingSize As Integer,
                                                 dk As Integer, dv As Integer) As Double()
            Dim blocks As Tensor()() = {attn.QueryWeights, attn.KeyWeights, attn.ValueWeights}
            Dim dims As Integer() = {dk, dk, dv}
            Dim data(embeddingSize * 3 * embeddingSize - 1) As Double
            Dim rowBase As Integer = 0

            For block As Integer = 0 To 2
                For head As Integer = 0 To nHeads - 1
                    Dim source As Tensor = blocks(block)(head)
                    Dim blockDim As Integer = dims(block)

                    For r As Integer = 0 To blockDim - 1
                        Dim torchRow As Integer = rowBase + head * blockDim + r

                        For c As Integer = 0 To embeddingSize - 1
                            data(torchRow * embeddingSize + c) = source.Data(c * blockDim + r)
                        Next
                    Next
                Next

                rowBase += embeddingSize
            Next

            Return data
        End Function

        ''' <summary>把 fused bias <c>[3H]</c> 拆到每个 head。</summary>
        Private Shared Sub scatterQkvBias(attn As MultiHeadAttention, nHeads As Integer, embeddingSize As Integer,
                                          dk As Integer, dv As Integer, data As Double())
            Dim blocks As Tensor()() = {attn.QueryBias, attn.KeyBias, attn.ValueBias}
            Dim dims As Integer() = {dk, dk, dv}
            Dim offset As Integer = 0

            For block As Integer = 0 To 2
                For head As Integer = 0 To nHeads - 1
                    Dim target As Tensor = blocks(block)(head)

                    For r As Integer = 0 To dims(block) - 1
                        target.Data(r) = data(offset + head * dims(block) + r)
                    Next

                    Call target.MarkHostModified()
                Next

                offset += embeddingSize
            Next
        End Sub

        ''' <summary>把每个 head 的 bias 拼回 fused <c>[3H]</c>。</summary>
        Private Shared Function gatherQkvBias(attn As MultiHeadAttention, nHeads As Integer, embeddingSize As Integer,
                                              dk As Integer, dv As Integer) As Double()
            Dim blocks As Tensor()() = {attn.QueryBias, attn.KeyBias, attn.ValueBias}
            Dim dims As Integer() = {dk, dk, dv}
            Dim data(embeddingSize * 3 - 1) As Double
            Dim offset As Integer = 0

            For block As Integer = 0 To 2
                For head As Integer = 0 To nHeads - 1
                    Dim source As Tensor = blocks(block)(head)

                    For r As Integer = 0 To dims(block) - 1
                        data(offset + head * dims(block) + r) = source.Data(r)
                    Next
                Next

                offset += embeddingSize
            Next

            Return data
        End Function

        ''' <summary>把数据复制进已有张量并标记为已修改。</summary>
        Private Shared Sub copyInto(target As Tensor, data As Double())
            If data.Length <> target.Length Then
                Throw New InvalidDataException($"张量长度不匹配：data={data.Length}, target={target.Length}")
            End If

            Array.Copy(data, target.Data, data.Length)
            Call target.MarkHostModified()
        End Sub

    End Class

End Namespace

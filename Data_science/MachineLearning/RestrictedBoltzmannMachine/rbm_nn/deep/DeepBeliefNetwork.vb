Imports System.Globalization
Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math.functions.doubledouble.rbm
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.nn.rbm.learn

Namespace nn.rbm.deep

    ''' <summary>
    ''' 深度信念网络（Deep Belief Network, DBN）。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 网络由若干个RBM逐层堆叠而成：最上面的两层构成一个无向的RBM，而下面的各层则构成有向的
    ''' sigmoid 信念网络。训练与使用分为两个阶段：
    ''' </para>
    ''' <list type="number">
    ''' <item>
    ''' 逐层贪心预训练（<see cref="TrainLayerWise"/>）：从最底层开始使用CD-k训练当前的RBM，
    ''' 然后把这个RBM的隐层输出作为下一层的输入，重复直到最顶层被训练完毕。
    ''' </item>
    ''' <item>
    ''' 使用：自底向上的均值场推断得到特征（<see cref="Encode"/>），
    ''' 自顶向下的传播完成重构或者生成（<see cref="Decode"/>、<see cref="Reconstruct"/>、<see cref="Generate"/>）。
    ''' </item>
    ''' </list>
    ''' <para>
    ''' 输入数据必须是一个稠密矩阵，每一个行向量代表一个样本，元素取值为<c>0</c>或者<c>1</c>。
    ''' </para>
    ''' </remarks>
    Public Class DeepBeliefNetwork

        ''' <summary>
        ''' 模型文件的格式标记
        ''' </summary>
        Public Const FORMAT_TAG As String = "scibasic.rbm.dbn"

        Private Shared ReadOnly ACTIVATION As DoubleDoubleFunction = New ActivationState()

        Private ReadOnly rbmLayers As New List(Of RBM)

        ''' <summary>
        ''' 网络之中的RBM层，按照自底向上的顺序排列
        ''' </summary>
        Public ReadOnly Property Layers As RBM()
            Get
                Return rbmLayers.ToArray()
            End Get
        End Property

        ''' <summary>
        ''' 网络之中的RBM层的数目
        ''' </summary>
        Public ReadOnly Property LayerCount As Integer
            Get
                Return rbmLayers.Count
            End Get
        End Property

        ''' <summary>
        ''' 最底层的可见层单元数
        ''' </summary>
        Public ReadOnly Property VisibleSize As Integer
            Get
                If rbmLayers.Count = 0 Then
                    Return 0
                End If

                Return rbmLayers(0).VisibleSize
            End Get
        End Property

        ''' <summary>
        ''' 最顶层的隐藏层单元数
        ''' </summary>
        Public ReadOnly Property HiddenSize As Integer
            Get
                If rbmLayers.Count = 0 Then
                    Return 0
                End If

                Return rbmLayers(rbmLayers.Count - 1).HiddenSize
            End Get
        End Property

        ''' <summary>
        ''' 添加一层RBM：新添加的RBM的可见层单元数必须等于上一层的隐藏层单元数。
        ''' </summary>
        ''' <param name="rbm">可以直接传入一个已经预训练好的RBM</param>
        Public Function AddLayer(rbm As RBM) As DeepBeliefNetwork
            If rbm Is Nothing Then
                Throw New ArgumentNullException(NameOf(rbm))
            End If

            If rbmLayers.Count > 0 AndAlso rbmLayers(rbmLayers.Count - 1).HiddenSize <> rbm.VisibleSize Then
                Throw New ArgumentException(
                    $"layer #{rbmLayers.Count}: the visible units ({rbm.VisibleSize}) of the new RBM should match " &
                    $"the hidden units ({rbmLayers(rbmLayers.Count - 1).HiddenSize}) of the previous layer")
            End If

            rbmLayers.Add(rbm)

            Return Me
        End Function

        ''' <summary>
        ''' 添加一层随机初始化的RBM
        ''' </summary>
        Public Function AddLayer(visibleUnits As Integer, hiddenUnits As Integer) As DeepBeliefNetwork
            Return AddLayer(New RBM(visibleUnits, hiddenUnits))
        End Function

        ''' <summary>
        ''' 逐层贪心预训练（DBN Greedy Training）：
        ''' <c>Train P(v|h1), use h1 for each v to train P(h1|h2), repeat until P(hn-1|hn) is trained</c>
        ''' </summary>
        ''' <param name="data">训练数据，每一个行向量是一个样本，元素取值为<c>0</c>或者<c>1</c></param>
        ''' <param name="epochs">每一层的训练轮数</param>
        ''' <param name="learningRate">学习速率</param>
        ''' <param name="k">CD-k算法之中的Gibbs采样步数</param>
        ''' <param name="sampleStates">
        ''' 是否使用二值采样状态作为下一层的输入。默认值为<c>False</c>，即使用隐层的均值场概率作为下一层的输入，
        ''' 这是Hinton推荐的DBN预训练做法；当数据噪声较大的时候可以尝试使用二值采样状态。
        ''' </param>
        ''' <param name="log">是否输出训练过程的日志</param>
        Public Sub TrainLayerWise(data As DenseMatrix,
                                  Optional epochs As Integer = 10,
                                  Optional learningRate As Double = 0.01,
                                  Optional k As Integer = 1,
                                  Optional sampleStates As Boolean = False,
                                  Optional log As Boolean = False)

            If rbmLayers.Count = 0 Then
                Throw New InvalidOperationException("can not train an empty deep belief network, please add at least one RBM layer first")
            End If
            If data Is Nothing Then
                Throw New ArgumentNullException(NameOf(data))
            End If
            If data.columns() <> VisibleSize Then
                Throw New ArgumentException($"the input data has {data.columns()} columns, but the network expects {VisibleSize} visible units")
            End If

            Dim current As DenseMatrix = data.copy()

            For i As Integer = 0 To rbmLayers.Count - 1
                Dim rbm As RBM = rbmLayers(i)
                Dim parameters As New LearningParameters()

                Call parameters.setEpochs(epochs).setLearningRate(learningRate).setK(k).setLog(log)

                Dim trainer As New ContrastiveDivergence(parameters)
                Call trainer.learn(rbm, current)

                ' 把当前层的输出作为下一层的输入
                If sampleStates Then
                    current = rbm.SampleHidden(current)
                Else
                    current = rbm.HiddenProbabilities(current)
                End If
            Next
        End Sub

        ''' <summary>
        ''' 自底向上的均值场推断：返回最顶层的隐层激活概率。
        ''' </summary>
        ''' <param name="data">可见层的输入数据，列数必须等于<see cref="VisibleSize"/></param>
        Public Function Encode(data As DenseMatrix) As DenseMatrix
            Dim current As DenseMatrix = CheckData(data)

            For Each rbm As RBM In rbmLayers
                current = rbm.HiddenProbabilities(current)
            Next

            Return current
        End Function

        ''' <summary>
        ''' 自底向上的随机采样推断：返回最顶层的隐层二值状态。
        ''' </summary>
        Public Function EncodeSample(data As DenseMatrix) As DenseMatrix
            Dim current As DenseMatrix = CheckData(data)

            For Each rbm As RBM In rbmLayers
                current = rbm.SampleHidden(current)
            Next

            Return current
        End Function

        ''' <summary>
        ''' 从最顶层的隐层状态自顶向下传播，返回最底层的可见层激活概率。
        ''' </summary>
        ''' <param name="hidden">最顶层的隐层状态，列数必须等于<see cref="HiddenSize"/></param>
        Public Function Decode(hidden As DenseMatrix) As DenseMatrix
            If rbmLayers.Count = 0 Then
                Throw New InvalidOperationException("can not decode with an empty deep belief network")
            End If
            If hidden Is Nothing Then
                Throw New ArgumentNullException(NameOf(hidden))
            End If
            If hidden.columns() <> HiddenSize Then
                Throw New ArgumentException($"the top hidden layer expects {HiddenSize} units but the input matrix has {hidden.columns()} columns")
            End If

            Dim current As DenseMatrix = hidden

            For i As Integer = rbmLayers.Count - 1 To 0 Step -1
                current = rbmLayers(i).VisibleProbabilities(current)
            Next

            Return current
        End Function

        ''' <summary>
        ''' 使用均值场的方式进行重构：<c>Decode(Encode(data))</c>。
        ''' </summary>
        Public Function Reconstruct(data As DenseMatrix) As DenseMatrix
            Return Decode(Encode(data))
        End Function

        ''' <summary>
        ''' 输入数据的重构均方误差，可以用于评估预训练的效果。
        ''' </summary>
        Public Function ReconstructionError(data As DenseMatrix) As Double
            Dim reconstructed As DenseMatrix = Reconstruct(data)
            Dim n As Integer = data.rows() * data.columns()

            Return data.copy().subtract(reconstructed).pow(2).sum() / n
        End Function

        ''' <summary>
        ''' 输入数据在最底层RBM上面的平均自由能：训练良好的时候，这个数值应该随着预训练的进行而下降。
        ''' </summary>
        Public Function AverageFreeEnergy(data As DenseMatrix) As Double
            Dim current As DenseMatrix = CheckData(data)
            Dim energies As Double() = rbmLayers(0).FreeEnergy(current)
            Dim sum As Double = 0

            For Each energy As Double In energies
                sum += energy
            Next

            Return sum / energies.Length
        End Function

        ''' <summary>
        ''' 从最顶层的RBM开始进行Gibbs采样，然后自顶向下生成最底层可见层的样本。
        ''' </summary>
        ''' <param name="samples">需要生成的样本数目</param>
        ''' <param name="gibbsSteps">最顶层RBM的Gibbs采样步数</param>
        Public Function Generate(samples As Integer, Optional gibbsSteps As Integer = 1) As DenseMatrix
            If rbmLayers.Count = 0 Then
                Throw New InvalidOperationException("can not generate samples from an empty deep belief network")
            End If
            If samples <= 0 Then
                Throw New ArgumentException("the number of samples should be a positive integer", NameOf(samples))
            End If

            Dim top As RBM = rbmLayers(rbmLayers.Count - 1)
            Dim state As DenseMatrix = RandomBinary(samples, top.VisibleSize)

            ' 最顶层的两层构成一个无向RBM，在这里进行交替Gibbs采样
            For step As Integer = 1 To System.Math.Max(1, gibbsSteps)
                Dim hidden As DenseMatrix = top.SampleHidden(state)

                state = top.SampleVisible(hidden)
            Next

            ' 然后把顶层的可见层状态自顶向下传播到最底层
            Dim current As DenseMatrix = state

            For i As Integer = rbmLayers.Count - 2 To 0 Step -1
                current = rbmLayers(i).VisibleProbabilities(current)
            Next

            Return current
        End Function

        ''' <summary>
        ''' 生成服从<c>P(x=1) = 0.5</c>的随机二值矩阵
        ''' </summary>
        Private Shared Function RandomBinary(rows As Integer, cols As Integer) As DenseMatrix
            Return DenseMatrix.random(rows, cols).apply(DenseMatrix.fill(rows, cols, 0.5), ACTIVATION)
        End Function

        Private Function CheckData(data As DenseMatrix) As DenseMatrix
            If rbmLayers.Count = 0 Then
                Throw New InvalidOperationException("the deep belief network does not contain any RBM layer")
            End If
            If data Is Nothing Then
                Throw New ArgumentNullException(NameOf(data))
            End If
            If data.columns() <> VisibleSize Then
                Throw New ArgumentException($"the input data has {data.columns()} columns, but the network expects {VisibleSize} visible units")
            End If

            Return data
        End Function

        ''' <summary>
        ''' 将网络之中的权重以及偏置保存到文本文件之中。
        ''' </summary>
        Public Sub Save(path As String)
            If String.IsNullOrEmpty(path) Then
                Throw New ArgumentException("the model file path can not be empty", NameOf(path))
            End If

            Dim text As New StringBuilder()

            Call text.AppendLine("# " & FORMAT_TAG)
            Call text.AppendLine("layers=" & rbmLayers.Count.ToString(CultureInfo.InvariantCulture))

            For i As Integer = 0 To rbmLayers.Count - 1
                Dim rbm As RBM = rbmLayers(i)
                Dim key As String = "layer" & i.ToString(CultureInfo.InvariantCulture)

                Call text.AppendLine(key & ".visible=" & rbm.VisibleSize.ToString(CultureInfo.InvariantCulture))
                Call text.AppendLine(key & ".hidden=" & rbm.HiddenSize.ToString(CultureInfo.InvariantCulture))
                Call text.AppendLine(key & ".vbias=" & joinVector(rbm.VBias))
                Call text.AppendLine(key & ".hbias=" & joinVector(rbm.HBias))
                Call text.AppendLine(key & ".weights=" & joinWeights(rbm))
            Next

            Call File.WriteAllText(path, text.ToString())
        End Sub

        ''' <summary>
        ''' 从<see cref="Save"/>方法所生成的文本文件之中加载深度信念网络。
        ''' </summary>
        Public Shared Function Load(path As String) As DeepBeliefNetwork
            If String.IsNullOrEmpty(path) Then
                Throw New ArgumentException("the model file path can not be empty", NameOf(path))
            End If
            If Not File.Exists(path) Then
                Throw New FileNotFoundException("the model file could not be found!", path)
            End If

            Dim map As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

            For Each line As String In File.ReadAllLines(path)
                Dim text As String = line.Trim()

                If text.Length = 0 OrElse text.StartsWith("#") Then
                    Continue For
                End If

                Dim i As Integer = text.IndexOf("="c)

                If i < 0 Then
                    Continue For
                End If

                map(text.Substring(0, i).Trim()) = text.Substring(i + 1).Trim()
            Next

            Dim count As Integer = Integer.Parse(map("layers"), CultureInfo.InvariantCulture)
            Dim dbn As New DeepBeliefNetwork()

            For i As Integer = 0 To count - 1
                Dim key As String = "layer" & i.ToString(CultureInfo.InvariantCulture)
                Dim visible As Integer = Integer.Parse(map(key & ".visible"), CultureInfo.InvariantCulture)
                Dim hidden As Integer = Integer.Parse(map(key & ".hidden"), CultureInfo.InvariantCulture)
                Dim rbm As New RBM(visible, hidden)

                rbm.VBias = parseVector(map(key & ".vbias"), visible)
                rbm.HBias = parseVector(map(key & ".hbias"), hidden)

                Dim weights As Double() = parseVector(map(key & ".weights"), visible * hidden)
                Dim offset As Integer = 0

                For a As Integer = 0 To visible - 1
                    For b As Integer = 0 To hidden - 1
                        Call rbm.Weights.set(a, b, weights(offset))
                        offset += 1
                    Next
                Next

                Call dbn.AddLayer(rbm)
            Next

            Return dbn
        End Function

        Private Shared Function joinWeights(rbm As RBM) As String
            Dim values As Double() = New Double(rbm.VisibleSize * rbm.HiddenSize - 1) {}
            Dim offset As Integer = 0

            For i As Integer = 0 To rbm.VisibleSize - 1
                For j As Integer = 0 To rbm.HiddenSize - 1
                    values(offset) = rbm.Weights.get(i, j)
                    offset += 1
                Next
            Next

            Return joinVector(values)
        End Function

        Private Shared Function joinVector(values As Double()) As String
            Dim parts As String() = New String(values.Length - 1) {}

            For i As Integer = 0 To values.Length - 1
                parts(i) = values(i).ToString("R", CultureInfo.InvariantCulture)
            Next

            Return String.Join(",", parts)
        End Function

        Private Shared Function parseVector(text As String, count As Integer) As Double()
            Dim values As Double() = New Double(count - 1) {}

            If String.IsNullOrEmpty(text) Then
                Return values
            End If

            Dim parts As String() = text.Split(","c)

            If parts.Length <> count Then
                Throw New FormatException($"expected {count} values but actually there are {parts.Length} values in the model file")
            End If

            For i As Integer = 0 To count - 1
                values(i) = Double.Parse(parts(i), CultureInfo.InvariantCulture)
            Next

            Return values
        End Function

        Public Overrides Function ToString() As String
            Dim text As New StringBuilder()

            Call text.Append(VisibleSize)

            For Each rbm As RBM In rbmLayers
                Call text.Append("-")
                Call text.Append(rbm.HiddenSize)
            Next

            Return "DBN{" & text.ToString() & "}"
        End Function

    End Class

End Namespace

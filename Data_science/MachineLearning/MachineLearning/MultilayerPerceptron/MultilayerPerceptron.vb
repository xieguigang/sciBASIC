' /********************************************************************************/
'  Microsoft.VisualBasic.MachineLearning
'  Data_science\MachineLearning\MachineLearning\MultilayerPerceptron\MultilayerPerceptron.vb
' /********************************************************************************/

Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.Activations
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports std = System.Math

Namespace MultilayerPerceptron

    ''' <summary>
    ''' 基于 <see cref="LinearAlgebra.Vector"/> 与 <see cref="NumericMatrix"/> 构建的多层感知机。
    ''' 支持分类（Softmax + 交叉熵）与回归（恒等输出 + 均方误差）两类任务，
    ''' 训练可采用带动量的 SGD 或 Adam 优化器。
    ''' </summary>
    Public Partial Class MultilayerPerceptron

        Private ReadOnly m_layers As New List(Of DenseLayer)()
        Private ReadOnly m_task As MlpTask
        Private ReadOnly m_inputSize As Integer
        Private ReadOnly m_outputSize As Integer
        Private ReadOnly m_lossHistory As New List(Of Double)()

        ''' <summary>输入维度。</summary>
        Public ReadOnly Property InputSize As Integer
            Get
                Return m_inputSize
            End Get
        End Property

        ''' <summary>输出维度（分类时等于类别数）。</summary>
        Public ReadOnly Property OutputSize As Integer
            Get
                Return m_outputSize
            End Get
        End Property

        ''' <summary>模型所执行的任务类型。</summary>
        Public ReadOnly Property Task As MlpTask
            Get
                Return m_task
            End Get
        End Property

        ''' <summary>全部层（含隐藏层与输出层）。</summary>
        Public ReadOnly Property Layers As IReadOnlyList(Of DenseLayer)
            Get
                Return m_layers
            End Get
        End Property

        ''' <summary>每一轮训练之后的损失值。</summary>
        Public ReadOnly Property LossHistory As IReadOnlyList(Of Double)
            Get
                Return m_lossHistory
            End Get
        End Property

        ''' <summary>最近一次计算的损失值。</summary>
        Public ReadOnly Property LastLoss As Double
            Get
                If m_lossHistory.Count = 0 Then
                    Return Double.NaN
                End If
                Return m_lossHistory(m_lossHistory.Count - 1)
            End Get
        End Property

        ''' <summary>
        ''' 创建一个新的多层感知机。
        ''' </summary>
        ''' <param name="inputSize">输入特征数量。</param>
        ''' <param name="hiddenSizes">各隐藏层神经元数量；空数组表示没有隐藏层。</param>
        ''' <param name="outputSize">输出维度。</param>
        ''' <param name="task">分类或回归任务。</param>
        ''' <param name="hiddenActivation">隐藏层激活函数表达式；缺省使用 ReLU()。</param>
        ''' <param name="seed">权重初始化随机种子。</param>
        Public Sub New(inputSize As Integer, hiddenSizes As Integer(), outputSize As Integer,
                       task As MlpTask,
                       Optional hiddenActivation As ActiveFunction = Nothing,
                       Optional seed As Integer = 42)
            If inputSize < 1 Then
                Throw New ArgumentOutOfRangeException(NameOf(inputSize), "输入维度必须大于零")
            End If
            If outputSize < 1 Then
                Throw New ArgumentOutOfRangeException(NameOf(outputSize), "输出维度必须大于零")
            End If

            m_inputSize = inputSize
            m_outputSize = outputSize
            m_task = task

            Dim activation As ActiveFunction = If(hiddenActivation, ActiveFunction.Parse(ActiveFunction.ReLU))
            Dim sizes As Integer() = If(hiddenSizes, New Integer() {})
            Dim previous As Integer = inputSize

            For i As Integer = 0 To sizes.Length - 1
                If sizes(i) < 1 Then
                    Throw New ArgumentOutOfRangeException(NameOf(hiddenSizes), "隐藏层神经元数量必须大于零")
                End If
                m_layers.Add(New DenseLayer(previous, sizes(i), activation.CreateFunction(), seed + i))
                previous = sizes(i)
            Next

            ' 输出层保持线性：分类的 Softmax 在 Forward 中统一施加，便于与 ONNX 图一一对应
            m_layers.Add(New DenseLayer(previous, outputSize, Nothing, seed + sizes.Length))
        End Sub

        ''' <summary>
        ''' 批量前向传播；分类任务返回每一类的概率，回归任务返回原始预测值。
        ''' </summary>
        Public Function Forward(x As NumericMatrix) As NumericMatrix
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If
            If x.ColumnDimension <> m_inputSize Then
                Throw New ArgumentException($"输入维度应为 {m_inputSize}，实际为 {x.ColumnDimension}")
            End If

            Dim current As NumericMatrix = x

            For Each layer As DenseLayer In m_layers
                current = layer.Forward(current)
            Next

            If m_task = MlpTask.Classification Then
                Call LossFunctions.Softmax(current)
            End If

            Return current
        End Function

        ''' <summary>对单个样本执行预测。</summary>
        Public Function Predict(x As Vector) As Vector
            Dim matrix As New NumericMatrix(New Double()() {x.Array})
            Return DirectCast(Forward(matrix)(0), Vector).Clone()
        End Function

        ''' <summary>对一批样本执行预测，形状为 [样本数, 输出维度]。</summary>
        Public Function Predict(x As NumericMatrix) As NumericMatrix
            Return Forward(x)
        End Function

        ''' <summary>分类任务返回概率最大的类别编号；回归任务返回首个输出值。</summary>
        Public Function PredictLabel(x As Vector) As Integer
            Dim output As Vector = Predict(x)
            Dim best As Integer = 0

            For i As Integer = 1 To output.Array.Length - 1
                If output(i) > output(best) Then
                    best = i
                End If
            Next

            Return best
        End Function

        ''' <summary>返回一批样本中每个样本的预测类别编号。</summary>
        Public Function PredictLabels(x As NumericMatrix) As Integer()
            Dim result As NumericMatrix = Forward(x)
            Dim labels As Integer() = New Integer(result.RowDimension - 1) {}

            For i As Integer = 0 To labels.Length - 1
                Dim best As Integer = 0

                For j As Integer = 1 To result.ColumnDimension - 1
                    If result(i, j) > result(i, best) Then
                        best = j
                    End If
                Next

                labels(i) = best
            Next

            Return labels
        End Function

        ''' <summary>
        ''' 使用特征矩阵与标签矩阵训练模型。
        ''' </summary>
        ''' <param name="x">形状为 [样本数, 输入维度] 的特征矩阵。</param>
        ''' <param name="y">形状为 [样本数, 输出维度] 的标签矩阵；分类时通常为 one-hot。</param>
        ''' <param name="options">训练配置；Nothing 表示使用默认配置。</param>
        ''' <returns>每一轮训练后的损失值数组。</returns>
        Public Function Fit(x As NumericMatrix, y As NumericMatrix,
                            Optional options As TrainingOptions = Nothing) As Double()
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If
            If y Is Nothing Then
                Throw New ArgumentNullException(NameOf(y))
            End If
            If x.RowDimension = 0 Then
                Throw New ArgumentException("训练集中至少需要一个样本")
            End If
            If x.ColumnDimension <> m_inputSize Then
                Throw New ArgumentException($"特征维度应为 {m_inputSize}，实际为 {x.ColumnDimension}")
            End If
            If y.RowDimension <> x.RowDimension OrElse y.ColumnDimension <> m_outputSize Then
                Throw New ArgumentException($"标签矩阵形状应为 [{x.RowDimension}, {m_outputSize}]")
            End If

            Dim opts As TrainingOptions = If(options, New TrainingOptions())
            Call opts.Validate()

            m_lossHistory.Clear()
            Dim rand As New Random(12345)
            Dim order As Integer() = Enumerable.Range(0, x.RowDimension).ToArray()
            Dim previousLoss As Double = Double.PositiveInfinity
            Dim updateStep As Integer = 0

            For epoch As Integer = 1 To opts.Epochs
                For i As Integer = order.Length - 1 To 1 Step -1
                    Dim j As Integer = rand.Next(i + 1)
                    Dim tmp As Integer = order(i)
                    order(i) = order(j)
                    order(j) = tmp
                Next

                Dim batchSize As Integer = If(opts.BatchSize > 0, opts.BatchSize, order.Length)
                Dim offset As Integer = 0

                While offset < order.Length
                    Dim count As Integer = std.Min(batchSize, order.Length - offset)
                    Dim indices As Integer() = New Integer(count - 1) {}

                    For i As Integer = 0 To count - 1
                        indices(i) = order(offset + i)
                    Next

                    Dim batchX As NumericMatrix = SelectRows(x, indices)
                    Dim batchY As NumericMatrix = SelectRows(y, indices)
                    Dim predictions As NumericMatrix = Forward(batchX)
                    Dim delta As Double()() = LossFunctions.OutputDelta(predictions, batchY, m_task)

                    For i As Integer = m_layers.Count - 1 To 0 Step -1
                        delta = m_layers(i).BackPropagation(delta)
                    Next

                    updateStep += 1

                    For Each layer As DenseLayer In m_layers
                        Call layer.Update(opts, updateStep)
                    Next

                    offset += count
                End While

                Dim allPredictions As NumericMatrix = Forward(x)
                Dim loss As Double = If(m_task = MlpTask.Classification,
                                        LossFunctions.CrossEntropy(allPredictions, y),
                                        LossFunctions.MeanSquaredError(allPredictions, y))

                m_lossHistory.Add(loss)

                If opts.Verbose AndAlso (epoch Mod std.Max(1, opts.LogEvery) = 0 OrElse epoch = 1) Then
                    Call Console.WriteLine($"epoch {epoch}/{opts.Epochs}  loss={loss:G6}")
                End If

                If std.Abs(previousLoss - loss) < opts.Tolerance Then
                    Exit For
                End If

                previousLoss = loss
            Next

            Return m_lossHistory.ToArray()
        End Function

        ''' <summary>清除训练历史与所有层的优化器状态。</summary>
        Public Sub Reset()
            m_lossHistory.Clear()

            For Each layer As DenseLayer In m_layers
                Call layer.ResetState()
            Next
        End Sub

        Public Overrides Function ToString() As String
            Return $"MLP({m_task}): {m_inputSize} -> {String.Join(" -> ", m_layers.Select(Function(l) l.OutputSize))}"
        End Function

        Private Shared Function SelectRows(matrix As NumericMatrix, indices As Integer()) As NumericMatrix
            Dim rows As Double()() = New Double(indices.Length - 1)() {}

            For i As Integer = 0 To indices.Length - 1
                rows(i) = New Double(matrix.ColumnDimension - 1) {}

                For j As Integer = 0 To rows(i).Length - 1
                    rows(i)(j) = matrix(indices(i), j)
                Next
            Next

            Return New NumericMatrix(rows)
        End Function

    End Class

End Namespace

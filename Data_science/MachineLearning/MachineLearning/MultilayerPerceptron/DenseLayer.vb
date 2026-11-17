' /********************************************************************************/
'  Microsoft.VisualBasic.MachineLearning
'  Data_science\MachineLearning\MachineLearning\MultilayerPerceptron\DenseLayer.vb
' /********************************************************************************/

Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.Activations
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports std = System.Math

Namespace MultilayerPerceptron

    ''' <summary>
    ''' 多层感知机中的一个全连接层。权重矩阵的形状为 [输入维度, 输出维度]，
    ''' 偏置向量的长度为输出维度；这种布局使前向计算可以直接调用
    ''' <c>NumericMatrix.DotProduct</c> 完成批量矩阵乘法。
    ''' </summary>
    Public Class DenseLayer

        Private ReadOnly m_inputSize As Integer
        Private ReadOnly m_outputSize As Integer
        Private m_weights As NumericMatrix
        Private m_bias As Vector

        ''' <summary>累积的权重梯度，形状 [输入维度, 输出维度]。</summary>
        Friend WeightGradient As Double()()
        ''' <summary>累积的偏置梯度，长度等于输出维度。</summary>
        Friend BiasGradient As Double()
        ''' <summary>Adam 优化器的一阶矩估计（权重）。</summary>
        Friend WeightMoment1 As Double()()
        ''' <summary>Adam 优化器的二阶矩估计（权重）。</summary>
        Friend WeightMoment2 As Double()()
        ''' <summary>Adam 优化器的一阶矩估计（偏置）。</summary>
        Friend BiasMoment1 As Double()
        ''' <summary>Adam 优化器的二阶矩估计（偏置）。</summary>
        Friend BiasMoment2 As Double()
        ''' <summary>带动量 SGD 的权重速度。</summary>
        Friend WeightVelocity As Double()()
        ''' <summary>带动量 SGD 的偏置速度。</summary>
        Friend BiasVelocity As Double()

        ''' <summary>上一次前向传播的输入缓存，用于反向传播。</summary>
        Friend LastInput As Double()()
        ''' <summary>上一次前向传播的线性输出（激活前）缓存。</summary>
        Friend LastLinear As Double()()
        ''' <summary>上一次前向传播的激活输出缓存。</summary>
        Friend LastOutput As Double()()

        ''' <summary>隐藏层使用的激活函数；输出层为 Nothing（由任务层统一处理）。</summary>
        Public Property Activation As IActivationFunction

        ''' <summary>输入维度。</summary>
        Public ReadOnly Property InputSize As Integer
            Get
                Return m_inputSize
            End Get
        End Property

        ''' <summary>输出维度。</summary>
        Public ReadOnly Property OutputSize As Integer
            Get
                Return m_outputSize
            End Get
        End Property

        ''' <summary>
        ''' 权重矩阵 [输入维度, 输出维度]。传入的矩阵必须满足该形状约定。
        ''' </summary>
        Public Property Weights As NumericMatrix
            Get
                Return m_weights
            End Get
            Set(value As NumericMatrix)
                If value Is Nothing Then
                    Throw New ArgumentNullException(NameOf(value))
                End If
                If value.RowDimension <> m_inputSize OrElse value.ColumnDimension <> m_outputSize Then
                    Throw New ArgumentException($"权重矩阵形状应为 [{m_inputSize}, {m_outputSize}]，实际为 [{value.RowDimension}, {value.ColumnDimension}]")
                End If
                m_weights = value
            End Set
        End Property

        ''' <summary>偏置向量，长度等于输出维度。</summary>
        Public Property Bias As Vector
            Get
                Return m_bias
            End Get
            Set(value As Vector)
                If value Is Nothing Then
                    Throw New ArgumentNullException(NameOf(value))
                End If
                If value.Array.Length <> m_outputSize Then
                    Throw New ArgumentException($"偏置向量长度应为 {m_outputSize}，实际为 {value.Array.Length}")
                End If
                m_bias = value
            End Set
        End Property

        ''' <summary>激活函数的可序列化表达式，可由 <c>ActiveFunction.Parse</c> 还原。</summary>
        Public ReadOnly Property ActivationExpression As String
            Get
                If Activation Is Nothing Then
                    Return "Identical()"
                End If
                Return Activation.Store.ToString()
            End Get
        End Property

        ''' <summary>
        ''' 创建一个全连接层，使用 Xavier 均匀分布初始化权重，偏置初始化为零。
        ''' </summary>
        ''' <param name="inputSize">输入维度。</param>
        ''' <param name="outputSize">输出维度。</param>
        ''' <param name="activation">激活函数；Nothing 表示线性层。</param>
        ''' <param name="seed">随机数种子，保证模型可复现。</param>
        Public Sub New(inputSize As Integer, outputSize As Integer,
                       Optional activation As IActivationFunction = Nothing,
                       Optional seed As Integer = 42)
            If inputSize < 1 Then
                Throw New ArgumentOutOfRangeException(NameOf(inputSize), "输入维度必须大于零")
            End If
            If outputSize < 1 Then
                Throw New ArgumentOutOfRangeException(NameOf(outputSize), "输出维度必须大于零")
            End If

            m_inputSize = inputSize
            m_outputSize = outputSize
            Me.Activation = activation

            Dim limit As Double = std.Sqrt(6.0 / (inputSize + outputSize))
            Dim rand As New Random(seed)
            Dim w As Double()() = Allocate(inputSize, outputSize)

            For i As Integer = 0 To inputSize - 1
                For j As Integer = 0 To outputSize - 1
                    w(i)(j) = (rand.NextDouble() * 2.0 - 1.0) * limit
                Next
            Next

            m_weights = New NumericMatrix(w)
            m_bias = New Vector(New Double(outputSize - 1) {})
            Call ResetOptimizerState()
        End Sub

        ''' <summary>
        ''' 从外部参数（例如 ONNX 初始化器）装载权重与偏置。
        ''' </summary>
        ''' <param name="weightRows">形状为 [输入维度, 输出维度] 的权重数组。</param>
        ''' <param name="biasValues">长度为输出维度的偏置数组。</param>
        Public Sub SetParameters(weightRows As Double()(), biasValues As Double())
            If weightRows Is Nothing OrElse weightRows.Length <> m_inputSize Then
                Throw New ArgumentException($"权重应当包含 {m_inputSize} 行")
            End If
            For Each row As Double() In weightRows
                If row Is Nothing OrElse row.Length <> m_outputSize Then
                    Throw New ArgumentException($"权重每行应当包含 {m_outputSize} 列")
                End If
            Next
            If biasValues Is Nothing OrElse biasValues.Length <> m_outputSize Then
                Throw New ArgumentException($"偏置应当包含 {m_outputSize} 个元素")
            End If

            m_weights = New NumericMatrix(weightRows)
            m_bias = New Vector(DirectCast(biasValues.Clone(), Double()))
            Call ResetOptimizerState()
        End Sub

        ''' <summary>
        ''' 批量前向传播：返回形状为 [批大小, 输出维度] 的矩阵。
        ''' </summary>
        Public Function Forward(x As NumericMatrix) As NumericMatrix
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If
            If x.ColumnDimension <> m_inputSize Then
                Throw New ArgumentException($"输入维度应为 {m_inputSize}，实际为 {x.ColumnDimension}")
            End If

            Dim product As NumericMatrix = DirectCast(x.DotProduct(m_weights), NumericMatrix)
            Dim z As Double()() = product.ArrayPack()
            Dim biasValues As Double() = m_bias.Array

            For i As Integer = 0 To z.Length - 1
                For j As Integer = 0 To m_outputSize - 1
                    z(i)(j) += biasValues(j)
                Next
            Next

            LastInput = x.ArrayPack()
            LastLinear = z

            If Activation IsNot Nothing Then
                For i As Integer = 0 To z.Length - 1
                    For j As Integer = 0 To m_outputSize - 1
                        z(i)(j) = Activation.Evaluate(z(i)(j))
                    Next
                Next
            End If

            LastOutput = z
            Return New NumericMatrix(z)
        End Function

        ''' <summary>
        ''' 反向传播：根据上游梯度计算本层梯度，并返回传递给前一层的梯度。
        ''' </summary>
        ''' <param name="delta">形状为 [批大小, 输出维度] 的上游梯度。</param>
        ''' <returns>形状为 [批大小, 输入维度] 的梯度。</returns>
        Public Function BackPropagation(delta As Double()()) As Double()()
            If delta Is Nothing Then
                Throw New ArgumentNullException(NameOf(delta))
            End If
            If LastInput Is Nothing OrElse LastLinear Is Nothing Then
                Throw New InvalidOperationException("必须先调用 Forward 才能执行反向传播")
            End If
            If delta.Length <> LastInput.Length Then
                Throw New ArgumentException("梯度批大小与前向缓存不一致")
            End If

            Dim batch As Integer = delta.Length
            Dim gradW As Double()() = Allocate(m_inputSize, m_outputSize)
            Dim gradB As Double() = New Double(m_outputSize - 1) {}
            Dim prev As Double()() = Allocate(batch, m_inputSize)

            For i As Integer = 0 To batch - 1
                If delta(i) Is Nothing OrElse delta(i).Length <> m_outputSize Then
                    Throw New ArgumentException($"第 {i} 行梯度长度应为 {m_outputSize}")
                End If

                For j As Integer = 0 To m_outputSize - 1
                    Dim d As Double = delta(i)(j)

                    If Activation IsNot Nothing Then
                        d *= Activation.CalculateDerivative(LastLinear(i)(j))
                    End If

                    gradB(j) += d

                    For k As Integer = 0 To m_inputSize - 1
                        gradW(k)(j) += LastInput(i)(k) * d
                        prev(i)(k) += d * m_weights(k, j)
                    Next
                Next
            Next

            WeightGradient = gradW
            BiasGradient = gradB
            Return prev
        End Function

        ''' <summary>
        ''' 使用给定的优化器配置更新权重与偏置；梯度由 <c>BackPropagation</c> 产生。
        ''' </summary>
        Public Sub Update(options As TrainingOptions, updateStep As Integer)
            If options Is Nothing Then
                Throw New ArgumentNullException(NameOf(options))
            End If
            If WeightGradient Is Nothing Then
                Throw New InvalidOperationException("没有可应用的梯度")
            End If

            Dim scale As Double = 1.0 / std.Max(1, LastInput.Length)

            Select Case options.Optimizer
                Case OptimizerKind.SGD
                    Dim momentum As Double = options.Momentum

                    For i As Integer = 0 To m_inputSize - 1
                        For j As Integer = 0 To m_outputSize - 1
                            Dim g As Double = WeightGradient(i)(j) * scale + momentum * WeightVelocity(i)(j)
                            WeightVelocity(i)(j) = g
                            m_weights(i, j) -= options.LearningRate * g
                        Next
                    Next

                    For j As Integer = 0 To m_outputSize - 1
                        Dim g As Double = BiasGradient(j) * scale + momentum * BiasVelocity(j)
                        BiasVelocity(j) = g
                        m_bias(j) -= options.LearningRate * g
                    Next

                Case OptimizerKind.Adam
                    Dim b1 As Double = options.Beta1
                    Dim b2 As Double = options.Beta2
                    Dim bc1 As Double = 1.0 - std.Pow(b1, updateStep)
                    Dim bc2 As Double = 1.0 - std.Pow(b2, updateStep)

                    For i As Integer = 0 To m_inputSize - 1
                        For j As Integer = 0 To m_outputSize - 1
                            Dim g As Double = WeightGradient(i)(j) * scale
                            WeightMoment1(i)(j) = b1 * WeightMoment1(i)(j) + (1.0 - b1) * g
                            WeightMoment2(i)(j) = b2 * WeightMoment2(i)(j) + (1.0 - b2) * g * g
                            Dim mhat As Double = WeightMoment1(i)(j) / bc1
                            Dim vhat As Double = WeightMoment2(i)(j) / bc2
                            m_weights(i, j) -= options.LearningRate * mhat / (std.Sqrt(vhat) + options.Epsilon)
                        Next
                    Next

                    For j As Integer = 0 To m_outputSize - 1
                        Dim g As Double = BiasGradient(j) * scale
                        BiasMoment1(j) = b1 * BiasMoment1(j) + (1.0 - b1) * g
                        BiasMoment2(j) = b2 * BiasMoment2(j) + (1.0 - b2) * g * g
                        Dim mhat As Double = BiasMoment1(j) / bc1
                        Dim vhat As Double = BiasMoment2(j) / bc2
                        m_bias(j) -= options.LearningRate * mhat / (std.Sqrt(vhat) + options.Epsilon)
                    Next

                Case Else
                    Throw New NotSupportedException($"不支持的优化器：{options.Optimizer}")
            End Select
        End Sub

        ''' <summary>
        ''' 以 ONNX Gemm 的约定（[输出维度, 输入维度]）导出权重数组。
        ''' </summary>
        Public Function GetOnnxWeights() As Double()
            Dim values As Double() = New Double(m_inputSize * m_outputSize - 1) {}

            For j As Integer = 0 To m_outputSize - 1
                For i As Integer = 0 To m_inputSize - 1
                    values(j * m_inputSize + i) = m_weights(i, j)
                Next
            Next

            Return values
        End Function

        ''' <summary>
        ''' 以 ONNX Gemm 的约定（[输出维度, 输入维度]）装载权重数组。
        ''' </summary>
        Public Sub SetOnnxParameters(weightValues As Double(), biasValues As Double())
            If weightValues Is Nothing OrElse weightValues.Length <> m_inputSize * m_outputSize Then
                Throw New ArgumentException($"ONNX 权重长度应为 {m_inputSize * m_outputSize}")
            End If
            If biasValues Is Nothing OrElse biasValues.Length <> m_outputSize Then
                Throw New ArgumentException($"ONNX 偏置长度应为 {m_outputSize}")
            End If

            Dim weights As Double()() = Allocate(m_inputSize, m_outputSize)

            For j As Integer = 0 To m_outputSize - 1
                For i As Integer = 0 To m_inputSize - 1
                    weights(i)(j) = weightValues(j * m_inputSize + i)
                Next
            Next

            Call SetParameters(weights, biasValues)
        End Sub

        ''' <summary>清除优化器状态与前向缓存。</summary>
        Public Sub ResetState()
            Call ResetOptimizerState()
            LastInput = Nothing
            LastLinear = Nothing
            LastOutput = Nothing
        End Sub

        Private Sub ResetOptimizerState()
            WeightGradient = Allocate(m_inputSize, m_outputSize)
            BiasGradient = New Double(m_outputSize - 1) {}
            WeightMoment1 = Allocate(m_inputSize, m_outputSize)
            WeightMoment2 = Allocate(m_inputSize, m_outputSize)
            BiasMoment1 = New Double(m_outputSize - 1) {}
            BiasMoment2 = New Double(m_outputSize - 1) {}
            WeightVelocity = Allocate(m_inputSize, m_outputSize)
            BiasVelocity = New Double(m_outputSize - 1) {}
        End Sub

        Private Shared Function Allocate(rows As Integer, cols As Integer) As Double()()
            Dim result As Double()() = New Double(rows - 1)() {}

            For i As Integer = 0 To rows - 1
                result(i) = New Double(cols - 1) {}
            Next

            Return result
        End Function

        Public Overrides Function ToString() As String
            Return $"Dense({m_inputSize} -> {m_outputSize}, {If(Activation Is Nothing, "linear", ActivationExpression)})"
        End Function

    End Class

End Namespace

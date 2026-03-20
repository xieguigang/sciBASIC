Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

Namespace PaCMAP
    ''' <summary>
    ''' PaCMAP (Pairwise Controlled Manifold Approximation Projection) 算法实现
    ''' Implementation of PaCMAP dimensionality reduction algorithm
    ''' 
    ''' 原始JavaScript代码来自 pacmapTF.js
    ''' </summary>
    Public Class PaCMAP
        Implements IDisposable
#Region "参数 / Parameters"

        ''' <summary>
        ''' 投影维度（默认2）
        ''' Number of projection dimensions (default 2)
        ''' </summary>
        Public Property NDimensions As Integer

        ''' <summary>
        ''' 近邻对数量（默认10）
        ''' Number of nearest neighbor pairs (default 10)
        ''' </summary>
        Public Property NumNeighbourPairs As Integer?

        ''' <summary>
        ''' 中近邻对比例（默认0.5）
        ''' Ratio of mid-near pairs (default 0.5)
        ''' </summary>
        Public Property RatioMidNearPairs As Double

        ''' <summary>
        ''' 远距离对比例（默认2.0）
        ''' Ratio of further pairs (default 2.0)
        ''' </summary>
        Public Property RatioFurtherPairs As Double

        ''' <summary>
        ''' 学习率（默认1）
        ''' Learning rate (default 1)
        ''' </summary>
        Public Property LearningRate As Double

        ''' <summary>
        ''' 迭代次数（默认450）
        ''' Number of iterations (default 450)
        ''' </summary>
        Public Property NumIterations As Integer

#End Region

#Region "内部状态 / Internal State"

        ''' <summary>
        ''' 输入数据
        ''' Input data
        ''' </summary>
        Private X As Tensor

        ''' <summary>
        ''' 样本数量
        ''' Number of samples
        ''' </summary>
        Private N As Integer

        ''' <summary>
        ''' 距离矩阵
        ''' Distance matrix
        ''' </summary>
        Private distances As Tensor

        ''' <summary>
        ''' 近邻对索引
        ''' Nearest neighbor pair indices
        ''' </summary>
        Private neighbourPairs As Tensor

        ''' <summary>
        ''' 中近邻对索引
        ''' Mid-near pair indices
        ''' </summary>
        Private midNearPairs As Tensor

        ''' <summary>
        ''' 远距离对索引
        ''' Further pair indices
        ''' </summary>
        Private furtherPairs As Tensor

        ''' <summary>
        ''' 投影坐标（可训练变量）
        ''' Projection coordinates (trainable variable)
        ''' </summary>
        Private Y As Tensor

        ''' <summary>
        ''' 计算得到的中近邻对数量
        ''' Calculated number of mid-near pairs
        ''' </summary>
        Private numMidNearPairs As Integer

        ''' <summary>
        ''' 计算得到的远距离对数量
        ''' Calculated number of further pairs
        ''' </summary>
        Private numFurtherPairs As Integer

        ''' <summary>
        ''' 实际的近邻对数量
        ''' Actual number of neighbor pairs
        ''' </summary>
        Private actualNumNeighbourPairs As Integer

        Private _disposed As Boolean = False
        Private _random As Random = New Random()

#End Region

#Region "构造函数 / Constructor"

        ''' <summary>
        ''' 创建PaCMAP实例
        ''' Create PaCMAP instance
        ''' </summary>
        Public Sub New(Optional nDimensions As Integer = 2, Optional numNeighbourPairs As Integer? = Nothing, Optional ratioMidNearPairs As Double = 0.5, Optional ratioFurtherPairs As Double = 2.0, Optional learningRate As Double = 1.0, Optional numIterations As Integer = 450)
            Me.NDimensions = nDimensions
            Me.NumNeighbourPairs = numNeighbourPairs
            Me.RatioMidNearPairs = ratioMidNearPairs
            Me.RatioFurtherPairs = ratioFurtherPairs
            Me.LearningRate = learningRate
            Me.NumIterations = numIterations

            If Me.NDimensions < 2 Then
                Throw New ArgumentException("The number of projection dimensions must be at least 2.")
            End If
            If Me.LearningRate <= 0 Then
                Throw New ArgumentException("The learning rate must be larger than 0.")
            End If
        End Sub

#End Region

#Region "主要方法 / Main Methods"

        ''' <summary>
        ''' 决定各类对的数量
        ''' Decide the number of each type of pairs
        ''' </summary>
        Private Sub DecideNumPairs()
            If Not NumNeighbourPairs.HasValue Then
                If N <= 10000 Then
                    actualNumNeighbourPairs = 10
                Else
                    actualNumNeighbourPairs = CInt(std.Round(10 + 15 * (std.Log10(N) - 4)))
                End If
            Else
                actualNumNeighbourPairs = NumNeighbourPairs.Value
            End If

            numMidNearPairs = CInt(std.Round(actualNumNeighbourPairs * RatioMidNearPairs))
            numFurtherPairs = CInt(std.Round(actualNumNeighbourPairs * RatioFurtherPairs))

            If actualNumNeighbourPairs < 1 Then
                Throw New ArgumentException("The number of nearest neighbors can't be less than 1")
            End If
            If numMidNearPairs < 1 Then
                Throw New ArgumentException("The number of mid-near pairs can't be less than 1")
            End If
            If numFurtherPairs < 1 Then
                Throw New ArgumentException("The number of further pairs can't be less than 1")
            End If
        End Sub

        ''' <summary>
        ''' 查找近邻对
        ''' Find nearest neighbor pairs
        ''' </summary>
        Private Function FindNeighbourPairs() As Tensor
            ' 归一化距离
            Dim normalizedDistances = distances.NormalizedDistance()

            ' 找最近的邻居（取负值后TopK相当于找最小值）
            Dim topKResult = TensorExtensions.Neg(normalizedDistances).TopK(actualNumNeighbourPairs + 1, True)

            ' 跳过第一个（自己），取后面的邻居
            Dim neighbourIndices = topKResult.Indices.Slice(New Integer() {0, 1}, New Integer() {-1, -1})

            Return neighbourIndices
        End Function

        ''' <summary>
        ''' 查找中近邻对
        ''' Find mid-near pairs
        ''' </summary>
        Private Function FindMidNearPairs() As Tensor
            ' 创建observation indices
            ' tf.range(0, N, 1).reshape([N, 1]).expandDims(2).expandDims(3).tile([1, 5, 6, 1])
            Dim observationIndices = TensorExtensions.ExpandDims(TensorExtensions.ExpandDims(TensorExtensions.Reshape(Tensor.Range(0, N, 1), N, 1), 2), 3).Tile(New Integer() {1, 5, 6, 1})

            ' 生成随机索引
            Dim samples = New List(Of Tensor)()
            For i = 0 To N - 1
                Dim indices = New Integer(29) {}
                Dim idx = 0
                For s = 0 To 4
                    For t = 0 To 5
                        Dim random As Integer
                        Do
                            random = _random.Next(N)
                        Loop While random = i
                        indices(std.Min(Threading.Interlocked.Increment(idx), idx - 1)) = random
                    Next
                Next
                samples.Add(New Tensor(indices.[Select](Function(x) CDbl(x)).ToArray(), New Integer() {5, 6, 1}))
            Next
            Dim randomIndices = samples.ToArray().Stack(0) ' [N, 5, 6, 1]

            ' 合并索引
            Dim combinedIndices = observationIndices.Concat(randomIndices, 3)

            ' 收集距离样本
            Dim distanceSamples = distances.GatherND(combinedIndices)

            ' 找第2近的邻居
            Dim topTwoResult = TensorExtensions.Neg(distanceSamples).TopK(2, True)
            Dim topTwoIndices = topTwoResult.Indices.Slice(New Integer() {0, 0, 1}, New Integer() {-1, -1, -1})

            ' 构建收集索引
            Dim observationIndicesForTopPairs = TensorExtensions.ExpandDims(TensorExtensions.ExpandDims(TensorExtensions.Reshape(Tensor.Range(0, N, 1), N, 1)), 2).Tile(New Integer() {1, 1, 5, 1})

            Dim sampleIndicesForTopPairs = TensorExtensions.ExpandDims(TensorExtensions.ExpandDims(TensorExtensions.Reshape(Tensor.Range(0, 5, 1), 5, 1))).Tile(New Integer() {1, N, 1, 1})

            Dim combinedIndicesForTopPairs = TensorExtensions.Concat(observationIndicesForTopPairs, CType(sampleIndicesForTopPairs, Tensor), 0).Concat(topTwoIndices.ExpandDims(), 3)

            ' 收集中近邻对
            Dim midNearPairs = TensorExtensions.GatherND(TensorExtensions.Squeeze(randomIndices), CType(combinedIndicesForTopPairs, Tensor)).Squeeze()

            Return midNearPairs
        End Function

        ''' <summary>
        ''' 查找远距离对
        ''' Find further pairs
        ''' </summary>
        Private Function FindFurtherPairs() As Tensor
            Dim samples = New List(Of Tensor)()

            For i = 0 To N - 1
                ' 获取第i个样本的邻居
                Dim neighbourRow = TensorExtensions.Slice(neighbourPairs, (New Integer() {i, 0}), (New Integer() {1, -1})).Squeeze()

                Dim neighbourSet = New HashSet(Of Integer)()
                For j As Integer = 0 To neighbourRow.Length - 1
                    neighbourSet.Add(CInt(neighbourRow.Data(j)))
                Next

                ' 生成不包含自己和邻居的随机索引
                Dim furtherIndices = New Double(numFurtherPairs - 1) {}
                For j = 0 To numFurtherPairs - 1
                    Dim random As Integer
                    Do
                        random = _random.Next(N)
                    Loop While random = i OrElse neighbourSet.Contains(random)
                    furtherIndices(j) = random
                Next

                samples.Add(New Tensor(furtherIndices, New Integer() {numFurtherPairs}))
            Next

            Return samples.ToArray().Stack(0)
        End Function

        ''' <summary>
        ''' 计算近邻对损失
        ''' Calculate loss for neighbor pairs
        ''' </summary>
        Private Function LossNeighbourPairs() As Tensor
            Dim J = Y.Gather(neighbourPairs)
            Dim dist = EuclideanDistance.Compute(Y, J)
            Dim numerator = TensorExtensions.Square(dist).Add(1.0)
            Dim denominator = Tensor.Scalar(10).Add(numerator)
            Dim loss = TensorExtensions.Div(numerator, CType(denominator, Tensor)).Sum()
            Return loss
        End Function

        ''' <summary>
        ''' 计算中近邻对损失
        ''' Calculate loss for mid-near pairs
        ''' </summary>
        Private Function LossMidNearPairs() As Tensor
            Dim J = Y.Gather(midNearPairs)
            Dim dist = EuclideanDistance.Compute(Y, J)
            Dim numerator = TensorExtensions.Square(dist).Add(1.0)
            Dim denominator = Tensor.Scalar(10000).Add(numerator)
            Dim loss = TensorExtensions.Div(numerator, CType(denominator, Tensor)).Sum()
            Return loss
        End Function

        ''' <summary>
        ''' 计算远距离对损失
        ''' Calculate loss for further pairs
        ''' </summary>
        Private Function LossFurtherPairs() As Tensor
            Dim J = Y.Gather(furtherPairs)
            Dim dist = EuclideanDistance.Compute(Y, J)
            Dim numerator = Tensor.Scalar(1)
            Dim denominator = Tensor.Scalar(1).Add(TensorExtensions.Square(dist).Add(1.0))
            Dim loss = TensorExtensions.Div(numerator, CType(denominator, Tensor)).Sum()
            Return loss
        End Function

        ''' <summary>
        ''' 查找所有对
        ''' Find all pairs
        ''' </summary>
        Private Sub FindPairs()
            distances = EuclideanDistance.Compute(X, X)
            neighbourPairs = FindNeighbourPairs()
            midNearPairs = FindMidNearPairs()
            furtherPairs = FindFurtherPairs()
        End Sub

        ''' <summary>
        ''' 拟合数据
        ''' Fit the model to data
        ''' </summary>
        ''' <param name="X">输入数据，形状 [N, D]</param>
        ''' <param name="init">初始化方式 ("random" 或其他)</param>
        ''' <returns>投影后的坐标，形状 [N, nDimensions]</returns>
        Public Function Fit(X As Double(,), Optional init As String = "random") As Double(,)
            ' 转换输入数据为Tensor
            Me.X = New Tensor(X)
            N = Me.X.Shape(0)

            ' 决定对的数量
            DecideNumPairs()

            ' 查找所有对
            FindPairs()

            ' 初始化投影坐标
            If Equals(init, "random") Then
                Y = Tensor.Variable(Tensor.RandomNormal(New Integer() {N, NDimensions}, 0, 1))
            End If

            ' 创建优化器
            Dim optimizer = New AdagradOptimizer(LearningRate)

            ' 迭代优化
            For i = 0 To NumIterations - 1
                ' 计算权重
                Dim weightNeighbourPairs As Double = 2
                Dim weightMidNearPairs = 1000 * (1 - (i - 1) / 100) + 3 * ((i - 1) / 100)
                Dim weightFurtherPairs As Double = 1

                If i >= 201 Then
                    weightNeighbourPairs = 1
                    weightMidNearPairs = 0
                    weightFurtherPairs = 1
                ElseIf i >= 101 Then
                    weightNeighbourPairs = 3
                    weightMidNearPairs = 3
                    weightFurtherPairs = 1
                End If

                ' 计算总损失
                Dim lossN = LossNeighbourPairs()
                Dim lossM = LossMidNearPairs()
                Dim lossF = LossFurtherPairs()

                Dim totalLoss = TensorExtensions.Add(TensorExtensions.Mul(lossN, weightNeighbourPairs), CType(TensorExtensions.Mul(lossM, weightMidNearPairs), Tensor)).Add(lossF.Mul(weightFurtherPairs))

                ' 计算梯度并更新
                Me.ComputeGradientsAndUpdate(optimizer, totalLoss)

                ' 输出进度
                If i Mod 50 = 0 OrElse i = NumIterations - 1 Then
                    Console.WriteLine($"Iteration {i + 1}/{NumIterations}, Loss: {totalLoss.Data(0):F6}")
                End If
            Next

            ' 返回结果
            Return GetResult()
        End Function

        ''' <summary>
        ''' 计算梯度并更新参数
        ''' Compute gradients and update parameters
        ''' </summary>
        Private Sub ComputeGradientsAndUpdate(optimizer As AdagradOptimizer, loss As Tensor)
            ' 数值梯度计算
            Dim gradients = Me.ComputeNumericalGradient(loss)

            ' 使用优化器更新
            optimizer.Update(Y, gradients)
        End Sub

        ''' <summary>
        ''' 计算数值梯度
        ''' Compute numerical gradient
        ''' </summary>
        Private Function ComputeNumericalGradient(loss As Tensor) As Tensor
            Const epsilon = 0.00001
            Dim gradients = New Tensor(Y.Shape)

            For i As Integer = 0 To Y.Length - 1
                ' 保存原始值
                Dim originalValue As Double = Y.Data(i)

                ' 计算 f(x + epsilon)
                Y.Data(i) = originalValue + epsilon
                Dim lossPlus = ComputeTotalLoss()

                ' 计算 f(x - epsilon)
                Y.Data(i) = originalValue - epsilon
                Dim lossMinus = ComputeTotalLoss()

                ' 恢复原始值
                Y.Data(i) = originalValue

                ' 计算梯度
                gradients.Data(i) = (lossPlus - lossMinus) / (2 * epsilon)
            Next

            Return gradients
        End Function

        ''' <summary>
        ''' 计算总损失（用于梯度计算）
        ''' Compute total loss for gradient calculation
        ''' </summary>
        Private Function ComputeTotalLoss() As Double
            Dim lossN = LossNeighbourPairs()
            Dim lossM = LossMidNearPairs()
            Dim lossF = LossFurtherPairs()

            Return lossN.Data(0) + lossM.Data(0) + lossF.Data(0)
        End Function

        ''' <summary>
        ''' 获取结果
        ''' Get the result
        ''' </summary>
        Private Function GetResult() As Double(,)
            Dim result = New Double(N - 1, NDimensions - 1) {}

            For i = 0 To N - 1
                For j = 0 To NDimensions - 1
                    result(i, j) = Y.Data(i * NDimensions + j)
                Next
            Next

            Return result
        End Function

#End Region

#Region "IDisposable"

        Public Sub Dispose() Implements IDisposable.Dispose
            If Not _disposed Then
                X?.Dispose()
                distances?.Dispose()
                neighbourPairs?.Dispose()
                midNearPairs?.Dispose()
                furtherPairs?.Dispose()
                Y?.Dispose()
                _disposed = True
            End If
        End Sub

#End Region
    End Class

    ''' <summary>
    ''' Adagrad优化器
    ''' Adagrad optimizer implementation
    ''' </summary>
    Public Class AdagradOptimizer
        Private ReadOnly _learningRate As Double
        Private _accumulatedSquaredGradients As Tensor

        Public Sub New(learningRate As Double)
            _learningRate = learningRate
        End Sub

        ''' <summary>
        ''' 更新参数
        ''' Update parameters
        ''' </summary>
        Public Sub Update(parameters As Tensor, gradients As Tensor)
            ' 初始化累积梯度
            If _accumulatedSquaredGradients Is Nothing Then
                _accumulatedSquaredGradients = New Tensor(parameters.Shape)
            End If

            ' Adagrad更新规则:
            ' accumulated_sq_grad += gradient^2
            ' parameter -= learning_rate * gradient / sqrt(accumulated_sq_grad + epsilon)
            Const epsilon = 0.00000001

            For i As Integer = 0 To parameters.Length - 1
                _accumulatedSquaredGradients.Data(i) += gradients.Data(i) * gradients.Data(i)
                parameters.Data(i) -= _learningRate * gradients.Data(i) / (std.Sqrt(_accumulatedSquaredGradients.Data(i)) + epsilon)
            Next
        End Sub
    End Class
End Namespace

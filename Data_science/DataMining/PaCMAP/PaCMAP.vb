#Region "Microsoft.VisualBasic::c024e85ae92ff9bddfdaf8bbd8c810b2, Data_science\DataMining\PaCMAP\PaCMAP.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 506
    '    Code Lines: 246 (48.62%)
    ' Comment Lines: 172 (33.99%)
    '    - Xml Docs: 79.07%
    ' 
    '   Blank Lines: 88 (17.39%)
    '     File Size: 17.26 KB


    ' Class PaCMAP
    ' 
    '     Properties: LearningRate, NDimensions, NumIterations, NumNeighbourPairs, RatioFurtherPairs
    '                 RatioMidNearPairs
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: ComputeNumericalGradient, ComputeTotalLoss, FindFurtherPairs, FindMidNearPairs, FindNeighbourPairs
    '               Fit, GetResult, LossFurtherPairs, LossMidNearPairs, LossNeighbourPairs
    ' 
    '     Sub: ComputeGradientsAndUpdate, DecideNumPairs, Dispose, FindPairs
    ' 
    ' Class AdagradOptimizer
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Update
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' PaCMAP (Pairwise Controlled Manifold Approximation Projection) 算法实现
''' Implementation of PaCMAP dimensionality reduction algorithm
''' 
''' 原始JavaScript代码来自 pacmapTF.js
''' </summary>
Public Class PaCMAP : Implements IDisposable
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

    ''' <summary>
    ''' 是否静默运行？（默认 True，不向控制台输出迭代进度）
    ''' 
    ''' 作为库在其它程序中调用时应当保持静默；只有在命令行调试场景下
    ''' 才将其设置为 False 以查看迭代收敛过程。
    ''' </summary>
    Public Property Silent As Boolean = True

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
    ''' 
    ''' 对每一个观测样本 i，随机采样 6 个候选样本并取其中第 2 近的那个作为
    ''' 中近邻；该过程重复 5 组，最终得到 ``[N, 5]`` 的中近邻索引矩阵。
    ''' 
    ''' 这里直接基于已计算好的距离矩阵实现，避免原始张量拼接实现之中的形状错误。
    ''' </summary>
    Private Function FindMidNearPairs() As Tensor
        Const groups As Integer = 5      ' 每一行重复采样的组数
        Const candidates As Integer = 6  ' 每一组随机采样的候选样本数量

        Dim result As Double() = New Double(N * groups - 1) {}

        For i As Integer = 0 To N - 1
            For s As Integer = 0 To groups - 1
                ' 采样 candidates 个不包含自身的随机索引
                Dim sample As Integer() = New Integer(candidates - 1) {}
                Dim filled As Integer = 0

                While filled < candidates
                    Dim r As Integer = _random.Next(N)

                    If r = i Then
                        Continue While
                    End If

                    sample(filled) = r
                    filled += 1
                End While

                ' 取其中距离第 2 近的候选作为中近邻
                Dim first As Integer = -1, second As Integer = -1
                Dim firstDist As Double = Double.MaxValue, secondDist As Double = Double.MaxValue

                For Each c As Integer In sample
                    Dim d As Double = distances.Data(i * N + c)

                    If d < firstDist Then
                        secondDist = firstDist
                        second = first
                        firstDist = d
                        first = c
                    ElseIf d < secondDist Then
                        secondDist = d
                        second = c
                    End If
                Next

                result(i * groups + s) = second
            Next
        Next

        Return New Tensor(result, New Integer() {N, groups})
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
        Return SumPairLoss(neighbourPairs, Function(d2) (d2 + 1.0) / (d2 + 11.0))
    End Function

    ''' <summary>
    ''' 计算中近邻对损失
    ''' Calculate loss for mid-near pairs
    ''' </summary>
    Private Function LossMidNearPairs() As Tensor
        Return SumPairLoss(midNearPairs, Function(d2) (d2 + 1.0) / (d2 + 10001.0))
    End Function

    ''' <summary>
    ''' 计算远距离对损失
    ''' Calculate loss for further pairs
    ''' </summary>
    Private Function LossFurtherPairs() As Tensor
        Return SumPairLoss(furtherPairs, Function(d2) 1.0 / (d2 + 2.0))
    End Function

    ''' <summary>
    ''' 基于低维嵌入坐标 <see cref="Y"/> 对给定的配对索引矩阵计算逐对损失之和。
    ''' 
    ''' 索引矩阵的形状为 ``[rows, pairs]``，其 data 为行主序的样本下标；
    ''' 这里直接在数值数组之上进行计算，避免张量广播带来的兼容问题。
    ''' </summary>
    ''' <param name="pairs">形如 ``[rows, pairs]`` 的配对索引矩阵</param>
    ''' <param name="loss">由成对平方距离计算该对损失的函数</param>
    Private Function SumPairLoss(pairs As Tensor, loss As Func(Of Double, Double)) As Tensor
        Dim rows As Integer = pairs.Shape(0)
        Dim count As Integer = pairs.Shape(1)
        Dim dims As Integer = NDimensions
        Dim sum As Double = 0

        For i As Integer = 0 To rows - 1
            Dim offset As Integer = i * dims

            For k As Integer = 0 To count - 1
                Dim j As Integer = CInt(pairs.Data(i * count + k))
                Dim jOffset As Integer = j * dims
                Dim d2 As Double = 0

                For d As Integer = 0 To dims - 1
                    Dim diff As Double = Y.Data(offset + d) - Y.Data(jOffset + d)
                    d2 += diff * diff
                Next

                sum += loss(d2)
            Next
        Next

        Return Tensor.Scalar(sum)
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
            If Not Silent AndAlso (i Mod 50 = 0 OrElse i = NumIterations - 1) Then
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

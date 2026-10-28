Option Strict On
Option Explicit On

Imports System
Imports Microsoft.VisualBasic.DataMining.FuzzyLogic.TSK
Imports std = System.Math

''' <summary>
''' ANFIS 的混合学习训练器。
''' </summary>
''' <remarks>
''' <para>
''' 每轮训练分两个阶段，顺序与 readme 一致：
''' </para>
''' <list type="number">
''' <item>**前向 / LSE**：前提参数固定，按行块构造设计矩阵 A 并累加 <c>A'A</c>、<c>A'y</c>，
''' 解出结论参数 <c>theta</c>。这一步对 <c>theta</c> 是全局最优的闭式解。</item>
''' <item>**反向**：结论参数固定，按行块累加前提参数梯度，再由优化器更新隶属函数的中心/宽度。</item>
''' </list>
''' <para>
''' 之所以不用 <c>CNN.trainers.TrainerAlgorithm</c>：那个训练器对"网络里所有参数块"
''' 施加同一个更新规则、按 <c>batch_size</c> 归一化梯度、并且默认带 L1/L2 衰减；
''' 而 ANFIS 的结论参数必须走闭式最小二乘、不能同时被 SGD 触碰，
''' 两者的调度模型不兼容。这里的实现把"轮级 LSE + 批级梯度下降"显式写清楚。
''' </para>
''' <para>
''' 并行化刻意没有做：最小二乘的累加顺序一变，最后几位就会变，
''' "固定种子可复现"这条验收要求会直接失效。
''' </para>
''' </remarks>
Public Class AnfisHybridTrainer

    Private ReadOnly _net As TskNetwork
    Private ReadOnly _options As AnfisOptions
    Private ReadOnly _warnings As System.Collections.Generic.List(Of String)

    ''' <summary>绑定模型与配置（配置会先做一次 <see cref="AnfisOptions.Validate"/>）。</summary>
    Public Sub New(network As TskNetwork, Optional options As AnfisOptions = Nothing)
        If network Is Nothing Then
            Throw New ArgumentNullException(NameOf(network))
        End If

        _net = network
        _options = If(options, New AnfisOptions()).Validate()
        _warnings = New System.Collections.Generic.List(Of String)()
    End Sub

    ''' <summary>训练过程中收集到的降级与告警信息（去重）。</summary>
    Public ReadOnly Property Warnings As String()
        Get
            Return _warnings.ToArray()
        End Get
    End Property

    ''' <summary>执行训练并返回拟合结果（模型是就地训练的那个实例）。</summary>
    Public Function Train(x As Double()(), y As Double()()) As AnfisFitResult
        Call AnfisData.Validate(x, y)

        Dim options As AnfisOptions = _options
        Dim sampleCount As Integer = x.Length
        Dim outputCount As Integer = y(0).Length
        Dim p As Integer = _net.ConsequentParameterCount

        Dim yAll As Double(,) = AnfisData.ToMatrix(y)

        Dim trainRows As Integer() = Nothing
        Dim validationRows As Integer() = Nothing

        Call AnfisData.Split(sampleCount, options.ValidationRatio, options.RandomSeed, trainRows, validationRows)

        Dim xTrain As Double()() = AnfisData.SelectRows(x, trainRows)
        Dim yTrain As Double(,) = AnfisData.SelectRows(yAll, trainRows)
        Dim xVal As Double()() = AnfisData.SelectRows(x, validationRows)
        Dim yVal As Double(,) = AnfisData.SelectRows(yAll, validationRows)
        Dim hasValidation As Boolean = validationRows.Length > 0

        Dim layers As New TskLayers(_net)
        Dim premise As Double() = _net.GetPremiseParameters()
        Dim optimizer As New AnfisOptimizer(premise.Length, _net.GetPremisePositiveMask(), options)

        Dim rls As RecursiveLeastSquares = Nothing

        If options.LearningMode = AnfisLearningMode.SequentialLse AndAlso Not options.FreezeConsequent Then
            rls = New RecursiveLeastSquares(p, outputCount, options.RlsForgetting, options.RlsInitialCovariance)
        End If

        Dim trainHistory As New System.Collections.Generic.List(Of Double)
        Dim validationHistory As New System.Collections.Generic.List(Of Double)
        Dim thetaHistory As New System.Collections.Generic.List(Of Double)

        Dim bestPremise As Double() = CType(premise.Clone(), Double())
        Dim bestTheta As Double(,) = CType(_net.Theta.Clone(), Double(,))
        Dim bestMetric As Double = Double.PositiveInfinity
        Dim bestEpoch As Integer = 0
        Dim stale As Integer = 0
        Dim converged As Boolean = False
        Dim epoch As Integer = 0

        While epoch < options.MaxEpoch
            Dim learningRate As Double = options.LearningRate / (1.0 + options.LearningRateDecay * epoch)

            ' ---- 阶段 1：结论参数的 (递推) 最小二乘 ----
            If Not options.FreezeConsequent Then
                Select Case options.LearningMode
                    Case AnfisLearningMode.Hybrid
                        Call SolveConsequent(layers, xTrain, yTrain)
                    Case AnfisLearningMode.GradientPlusOneShotLse
                        If epoch = 0 Then
                            Call SolveConsequent(layers, xTrain, yTrain)
                        End If
                    Case AnfisLearningMode.SequentialLse
                        Call FeedRecursive(layers, rls, xTrain, yTrain)

                        If rls.SampleCount > 0 Then
                            _net.Theta = rls.GetTheta()
                        End If
                End Select
            End If

            ' ---- 阶段 2：前提参数的反向传播 ----
            Dim trainRmse As Double

            If options.FreezePremise Then
                trainRmse = ComputeRmse(layers, xTrain, yTrain)
            Else
                trainRmse = GradientStep(layers, xTrain, yTrain, premise, optimizer, learningRate, options)
                Call _net.SetPremiseParameters(premise)
            End If

            Dim validationRmse As Double = Double.NaN

            If hasValidation Then
                validationRmse = ComputeRmse(layers, xVal, yVal)
            End If

            Call trainHistory.Add(trainRmse)
            Call thetaHistory.Add(ThetaNorm())

            If hasValidation Then
                Call validationHistory.Add(validationRmse)
            End If

            ' ---- 早停：保留验证误差最优的那一轮 ----
            Dim metric As Double = If(Double.IsNaN(validationRmse), trainRmse, validationRmse)
            Dim threshold As Double = If(Double.IsInfinity(bestMetric), Double.PositiveInfinity, bestMetric * (1.0 - options.MinImprovement))

            If metric < threshold Then
                bestMetric = metric
                bestEpoch = epoch
                stale = 0
                bestPremise = CType(premise.Clone(), Double())
                bestTheta = CType(_net.Theta.Clone(), Double(,))
            Else
                stale += 1
            End If

            epoch += 1

            If stale >= options.Patience Then
                converged = True
                Exit While
            End If
        End While

        If Not Double.IsInfinity(bestMetric) Then
            Call _net.SetPremiseParameters(bestPremise)
            _net.Theta = bestTheta
        End If

        Return BuildResult(x, yAll, layers, trainHistory, validationHistory, thetaHistory, epoch, bestEpoch, converged)
    End Function

#Region "结论参数求解"

    Private Function SolveConsequent(layers As TskLayers, x As Double()(), y As Double(,)) As Boolean
        Dim options As AnfisOptions = _options
        Dim p As Integer = _net.ConsequentParameterCount
        Dim rows As Integer = x.Length
        Dim acc As New LeastSquaresAccumulator(p, _net.OutputCount, options.Ridge)

        For start As Integer = 0 To rows - 1 Step options.ChunkRows
            Dim size As Integer = std.Min(options.ChunkRows, rows - start)
            Dim fwd As TskInferenceTrace = layers.Forward(x, start, size)
            Dim design As Double(,) = New Double(size - 1, p - 1) {}

            Call layers.BuildDesignMatrix(fwd, design)
            Call acc.AddChunk(design, AnfisData.SliceRows(y, start, size))
        Next

        Dim message As String = ""
        Dim theta As Double(,) = acc.Solve(message)

        If theta Is Nothing Then
            Call AddWarning(message)
            Return False
        End If

        _net.Theta = theta

        If Not String.IsNullOrEmpty(message) Then
            Call AddWarning(message)
        End If

        Return True
    End Function

    Private Sub FeedRecursive(layers As TskLayers, rls As RecursiveLeastSquares, x As Double()(), y As Double(,))
        Dim options As AnfisOptions = _options
        Dim rows As Integer = x.Length

        For start As Integer = 0 To rows - 1 Step options.ChunkRows
            Dim size As Integer = std.Min(options.ChunkRows, rows - start)
            Dim fwd As TskInferenceTrace = layers.Forward(x, start, size)

            For t As Integer = 0 To size - 1
                Dim design As Double() = BuildDesignRow(fwd, t)
                Dim target(_net.OutputCount - 1) As Double

                For k As Integer = 0 To _net.OutputCount - 1
                    target(k) = y(start + t, k)
                Next

                Call rls.Update(design, target)
            Next
        Next
    End Sub

    Private Function BuildDesignRow(fwd As TskInferenceTrace, t As Integer) As Double()
        Dim n As Integer = fwd.InputCount
        Dim design(_net.ConsequentParameterCount - 1) As Double

        For r As Integer = 0 To fwd.RuleCount - 1
            Dim wb As Double = fwd.Wbar(t, r)

            If wb = 0.0 Then
                Continue For
            End If

            Dim offset As Integer = r * (n + 1)

            For j As Integer = 0 To n - 1
                design(offset + j) = wb * fwd.X(t, j)
            Next

            design(offset + n) = wb
        Next

        Return design
    End Function

#End Region

#Region "梯度"

    Private Function GradientStep(layers As TskLayers,
                                  x As Double()(),
                                  y As Double(,),
                                  premise As Double(),
                                  optimizer As AnfisOptimizer,
                                  learningRate As Double,
                                  options As AnfisOptions) As Double

        Dim rows As Integer = x.Length
        Dim gradient(premise.Length - 1) As Double
        Dim updateTheta As Boolean = options.LearningMode = AnfisLearningMode.GradientOnly AndAlso Not options.FreezeConsequent
        Dim thetaGradient As Double(,) = Nothing

        If updateTheta Then
            thetaGradient = New Double(_net.ConsequentParameterCount - 1, _net.OutputCount - 1) {}
        End If

        Dim sse As Double = 0.0

        For start As Integer = 0 To rows - 1 Step options.ChunkRows
            Dim size As Integer = std.Min(options.ChunkRows, rows - start)
            Dim fwd As TskInferenceTrace = layers.Forward(x, start, size)
            Dim target As Double(,) = AnfisData.SliceRows(y, start, size)
            Dim part As TskGradient = layers.Accumulate(fwd, target, updateTheta)

            sse += part.SquaredError

            For i As Integer = 0 To gradient.Length - 1
                gradient(i) += part.Premise(i)
            Next

            If updateTheta Then
                For i As Integer = 0 To thetaGradient.GetLength(0) - 1
                    For k As Integer = 0 To thetaGradient.GetLength(1) - 1
                        thetaGradient(i, k) += part.Theta(i, k)
                    Next
                Next
            End If
        Next

        ' 按样本数归一化：否则梯度量级会随数据规模漂移，学习率就没有可比性
        For i As Integer = 0 To gradient.Length - 1
            gradient(i) /= rows
        Next

        Call optimizer.ApplyGradients(premise, gradient, learningRate, options.GradientClip)

        If updateTheta Then
            Dim theta As Double(,) = _net.Theta

            For i As Integer = 0 To theta.GetLength(0) - 1
                For k As Integer = 0 To theta.GetLength(1) - 1
                    Dim updated As Double = theta(i, k) - learningRate * thetaGradient(i, k) / rows

                    If Not updated.IsNaNImaginary Then
                        theta(i, k) = updated
                    End If
                Next
            Next
        End If

        Return std.Sqrt(sse / (rows * _net.OutputCount))
    End Function

#End Region

#Region "评估"

    Private Function ComputeRmse(layers As TskLayers, x As Double()(), y As Double(,)) As Double
        If x.Length = 0 Then
            Return Double.NaN
        End If

        Dim options As AnfisOptions = _options
        Dim sse As Double = 0.0

        For start As Integer = 0 To x.Length - 1 Step options.ChunkRows
            Dim size As Integer = std.Min(options.ChunkRows, x.Length - start)
            Dim fwd As TskInferenceTrace = layers.Forward(x, start, size)

            For t As Integer = 0 To size - 1
                For k As Integer = 0 To _net.OutputCount - 1
                    Dim d As Double = fwd.Z(t, k) - y(start + t, k)
                    sse += d * d
                Next
            Next
        Next

        Return std.Sqrt(sse / (x.Length * _net.OutputCount))
    End Function

    Private Function ComputeRuleActivations(layers As TskLayers, x As Double()()) As Double()
        Dim options As AnfisOptions = _options
        Dim result(_net.Rules.RuleCount - 1) As Double

        For start As Integer = 0 To x.Length - 1 Step options.ChunkRows
            Dim size As Integer = std.Min(options.ChunkRows, x.Length - start)
            Dim fwd As TskInferenceTrace = layers.Forward(x, start, size)

            For t As Integer = 0 To size - 1
                For r As Integer = 0 To _net.Rules.RuleCount - 1
                    result(r) += fwd.Wbar(t, r)
                Next
            Next
        Next

        If x.Length > 0 Then
            For r As Integer = 0 To result.Length - 1
                result(r) /= x.Length
            Next
        End If

        Return result
    End Function

    Private Function ThetaNorm() As Double
        Dim theta As Double(,) = _net.Theta
        Dim sum As Double = 0.0

        For i As Integer = 0 To theta.GetLength(0) - 1
            For k As Integer = 0 To theta.GetLength(1) - 1
                sum += theta(i, k) * theta(i, k)
            Next
        Next

        Return std.Sqrt(sum)
    End Function

    Private Function BuildResult(x As Double()(),
                                 y As Double(,),
                                 layers As TskLayers,
                                 trainHistory As System.Collections.Generic.List(Of Double),
                                 validationHistory As System.Collections.Generic.List(Of Double),
                                 thetaHistory As System.Collections.Generic.List(Of Double),
                                 iterations As Integer,
                                 bestEpoch As Integer,
                                 converged As Boolean) As AnfisFitResult

        Dim fitted As Double()() = _net.Predict(x)
        Dim rows As Integer = x.Length
        Dim outputCount As Integer = _net.OutputCount

        Dim residuals(rows - 1)() As Double
        Dim sse As Double = 0.0
        Dim mean As Double = 0.0

        For i As Integer = 0 To rows - 1
            For k As Integer = 0 To outputCount - 1
                mean += y(i, k)
            Next
        Next

        mean /= (rows * outputCount)

        Dim sst As Double = 0.0

        For i As Integer = 0 To rows - 1
            Dim row(outputCount - 1) As Double

            For k As Integer = 0 To outputCount - 1
                Dim d As Double = y(i, k) - fitted(i)(k)
                row(k) = d
                sse += d * d
                sst += (y(i, k) - mean) * (y(i, k) - mean)
            Next

            residuals(i) = row
        Next

        Dim r2 As Double

        If sst > 0.0 Then
            r2 = 1.0 - sse / sst
        Else
            r2 = If(sse <= 0.0, 1.0, 0.0)
        End If

        Return New AnfisFitResult With {
            .Model = _net,
            .R2 = r2,
            .RMSE = std.Sqrt(sse / (rows * outputCount)),
            .SSE = sse,
            .SST = sst,
            .FittedValues = fitted,
            .Residuals = residuals,
            .TrainRmseHistory = trainHistory.ToArray(),
            .ValidationRmseHistory = validationHistory.ToArray(),
            .ThetaNormHistory = thetaHistory.ToArray(),
            .RuleActivations = ComputeRuleActivations(layers, x),
            .PrunedRuleCount = 0,
            .Iterations = iterations,
            .BestEpoch = bestEpoch,
            .Converged = converged,
            .Warnings = _warnings.ToArray()
        }
    End Function

#End Region

    Private Sub AddWarning(message As String)
        If String.IsNullOrEmpty(message) Then
            Return
        End If
        If Not _warnings.Contains(message) Then
            Call _warnings.Add(message)
        End If
    End Sub

End Class

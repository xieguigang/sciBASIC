Imports Microsoft.VisualBasic.DataMining.FuzzyLogic.TSK
Imports std = System.Math

''' <summary>
''' ANFIS 的对外门面：结构初始化、拟合、预测、评估、剪枝与交叉验证。
''' </summary>
''' <remarks>
''' <para>
''' 推荐的最小用法：
''' <code>
''' Dim result = AnfisFitting.Fit(x, y)          ' x: Double()()（行 = 样本），y: Double()
''' Dim yhat = AnfisFitting.Predict(result.Model, xTest)
''' </code>
''' </para>
''' <para>
''' 网络结构（隶属函数、规则表、前向推理）来自 <c>Microsoft.VisualBasic.DataMining.FuzzyLogic</c>
''' 的 <c>TSK</c> 命名空间，本工程只提供训练、评估与剪枝。
''' 结构初始化用的是**网格划分**：每个输入等距放 m 个隶属函数，规则数 <c>m^n</c>。
''' 网格划分的优点是规则可解释、前向完全向量化；缺点是规则数随维数指数增长，
''' 因此 <see cref="TskStructureOptions.MaxRules"/> 与 <see cref="TskStructureOptions.MaxConsequentParams"/>
''' 会在超限时直接报错并给出可操作的提示。
''' </para>
''' </remarks>
Public Module AnfisFitting

    ''' <summary>单输出回归的便捷入口。</summary>
    Public Function Fit(x As Double()(), y As Double(), Optional options As AnfisOptions = Nothing) As AnfisFitResult
        If y Is Nothing Then
            Throw New ArgumentNullException(NameOf(y))
        End If

        Dim targets(y.Length - 1)() As Double

        For i As Integer = 0 To y.Length - 1
            targets(i) = New Double() {y(i)}
        Next

        Return Fit(x, targets, options)
    End Function

    ''' <summary>多输出回归入口。</summary>
    Public Function Fit(x As Double()(), y As Double()(), Optional options As AnfisOptions = Nothing) As AnfisFitResult
        Call AnfisData.Validate(x, y)

        Dim network As TskNetwork = CreateModel(x, y, options)
        Dim trainer As New AnfisHybridTrainer(network, options)

        Return trainer.Train(x, y)
    End Function

    ''' <summary>
    ''' 仅按数据的取值范围与配置构建模型结构（不训练）。
    ''' </summary>
    ''' <remarks>
    ''' 定义域统计**只使用传入的数据**。做交叉验证时请对训练折调用本方法，
    ''' 不要用全量数据建模再把同一结构套到每一折上（那会泄漏验证折的分布信息）。
    ''' 具体实现下沉在 <see cref="TskNetworkBuilder.CreateGrid"/>。
    ''' </remarks>
    Public Function CreateModel(x As Double()(), y As Double()(), Optional options As AnfisOptions = Nothing) As TskNetwork
        Return TskNetworkBuilder.CreateGrid(x, y, options)
    End Function

    ''' <summary>批量预测。</summary>
    Public Function Predict(model As TskNetwork, x As Double()(), Optional chunkRows As Integer = 1024) As Double()()
        If model Is Nothing Then
            Throw New ArgumentNullException(NameOf(model))
        End If

        Return model.Predict(x, chunkRows)
    End Function

    ''' <summary>批量预测并只取第一个输出（单输出模型的便利重载）。</summary>
    Public Function PredictScalar(model As TskNetwork, x As Double()()) As Double()
        Dim batch As Double()() = Predict(model, x)
        Dim result(batch.Length - 1) As Double

        For i As Integer = 0 To batch.Length - 1
            result(i) = batch(i)(0)
        Next

        Return result
    End Function

    ''' <summary>在给定数据上评估模型（不做任何训练）。</summary>
    Public Function Evaluate(model As TskNetwork, x As Double()(), y As Double()()) As AnfisFitResult
        If model Is Nothing Then
            Throw New ArgumentNullException(NameOf(model))
        End If

        Call AnfisData.Validate(x, y)

        Dim target As Double(,) = AnfisData.ToMatrix(y)
        Dim fitted As Double()() = model.Predict(x)
        Dim rows As Integer = x.Length
        Dim outputCount As Integer = model.OutputCount

        Dim residuals(rows - 1)() As Double
        Dim mean As Double = 0.0

        For i As Integer = 0 To rows - 1
            For k As Integer = 0 To outputCount - 1
                mean += target(i, k)
            Next
        Next

        mean /= (rows * outputCount)

        Dim sse As Double = 0.0
        Dim sst As Double = 0.0

        For i As Integer = 0 To rows - 1
            Dim row(outputCount - 1) As Double

            For k As Integer = 0 To outputCount - 1
                Dim d As Double = target(i, k) - fitted(i)(k)
                row(k) = d
                sse += d * d
                sst += (target(i, k) - mean) * (target(i, k) - mean)
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
            .Model = model,
            .R2 = r2,
            .RMSE = std.Sqrt(sse / (rows * outputCount)),
            .SSE = sse,
            .SST = sst,
            .FittedValues = fitted,
            .Residuals = residuals,
            .TrainRmseHistory = New Double() {},
            .ValidationRmseHistory = New Double() {},
            .ThetaNormHistory = New Double() {},
            .RuleActivations = ComputeRuleActivations(model, x),
            .PrunedRuleCount = 0,
            .Iterations = 0,
            .BestEpoch = 0,
            .Converged = False,
            .Warnings = New String() {}
        }
    End Function

    ''' <summary>
    ''' 按平均触发强度剪枝，并用剩余规则重新做一次结论参数的最小二乘拟合。
    ''' </summary>
    ''' <param name="model">待剪枝的模型</param>
    ''' <param name="x">训练输入</param>
    ''' <param name="y">训练目标</param>
    ''' <param name="threshold">平均归一化触发强度低于该值的规则被移除</param>
    ''' <param name="options">训练配置（仅使用其岭项与分块等无关紧要的参数）</param>
    ''' <remarks>
    ''' 剪枝是 ANFIS 的常规后处理：网格划分会产生大量几乎从不激活的规则，
    ''' 它们对输出没有贡献，却让结论参数矩阵白白变大、也让 LSE 更容易病态。
    ''' 前提参数在剪枝中保持不变（隶属函数的位置与形状与规则无关），
    ''' 只有结论参数需要重解。
    ''' </remarks>
    Public Function PruneRules(model As TskNetwork,
                               x As Double()(),
                               y As Double()(),
                               threshold As Double,
                               Optional options As AnfisOptions = Nothing) As AnfisFitResult

        If model Is Nothing Then
            Throw New ArgumentNullException(NameOf(model))
        End If

        Call AnfisData.Validate(x, y)

        Dim activation As Double() = ComputeRuleActivations(model, x)
        Dim keep As New System.Collections.Generic.List(Of Integer)
        Dim best As Integer = 0

        For r As Integer = 0 To activation.Length - 1
            If activation(r) >= threshold Then
                Call keep.Add(r)
            End If

            If activation(r) > activation(best) Then
                best = r
            End If
        Next

        If keep.Count = 0 Then
            ' 阈值过严：至少保留触发最强的规则，否则模型会退化成常数 0
            Call keep.Add(best)
        End If

        If keep.Count = model.Rules.RuleCount Then
            Dim unchanged As AnfisFitResult = Evaluate(model, x, y)
            unchanged.PrunedRuleCount = 0
            Return unchanged
        End If

        Dim config As AnfisOptions = If(options, New AnfisOptions()).Clone()
        config.ValidationRatio = 0.0
        config.MaxEpoch = 1
        config.Patience = 1
        config.FreezePremise = True
        config.LearningMode = AnfisLearningMode.GradientPlusOneShotLse

        Dim inputs As TskInputVariable() = model.Inputs.Select(Function(v) v.Clone()).ToArray
        Dim rules As FuzzyRuleBase = model.Rules.SelectRules(keep.ToArray)
        Dim trimmed As New TskNetwork(inputs, rules, model.OutputCount, CType(model.OutputNames.Clone(), String())) With {
            .FeatureNames = CType(model.FeatureNames.Clone(), String())
        }

        Call trimmed.SetPremiseParameters(model.GetPremiseParameters())

        Dim trainer As New AnfisHybridTrainer(trimmed, config)
        Dim result As AnfisFitResult = trainer.Train(x, y)

        result.PrunedRuleCount = model.Rules.RuleCount - rules.RuleCount

        Return result
    End Function

    ''' <summary>
    ''' 确定性 K 折交叉验证（<c>foldId(i) = i Mod folds</c>，不做随机洗牌）。
    ''' </summary>
    Public Function CrossValidate(x As Double()(),
                                  y As Double()(),
                                  Optional folds As Integer = 5,
                                  Optional options As AnfisOptions = Nothing) As AnfisCrossValidationResult

        Call AnfisData.Validate(x, y)

        If folds < 2 Then
            Throw New ArgumentOutOfRangeException(NameOf(folds), folds, "the fold count must be at least 2")
        End If

        If folds > x.Length Then
            folds = x.Length
        End If

        Dim config As AnfisOptions = If(options, New AnfisOptions()).Clone()
        Dim target As Double(,) = AnfisData.ToMatrix(y)
        Dim foldRmse(folds - 1) As Double
        Dim foldR2(folds - 1) As Double
        Dim models(folds - 1) As TskNetwork

        For fold As Integer = 0 To folds - 1
            Dim trainIndex As New System.Collections.Generic.List(Of Integer)
            Dim testIndex As New System.Collections.Generic.List(Of Integer)

            For i As Integer = 0 To x.Length - 1
                If i Mod folds = fold Then
                    Call testIndex.Add(i)
                Else
                    Call trainIndex.Add(i)
                End If
            Next

            If trainIndex.Count = 0 OrElse testIndex.Count = 0 Then
                foldRmse(fold) = Double.NaN
                foldR2(fold) = Double.NaN
                Continue For
            End If

            Dim xTrain As Double()() = AnfisData.SelectRows(x, trainIndex.ToArray())
            Dim yTrain As Double(,) = AnfisData.SelectRows(target, trainIndex.ToArray())
            Dim xTest As Double()() = AnfisData.SelectRows(x, testIndex.ToArray())
            Dim yTest As Double(,) = AnfisData.SelectRows(target, testIndex.ToArray())

            ' 内层不再划分验证集：外层已经留出了测试折
            Dim foldOptions As AnfisOptions = config.Clone()
            foldOptions.ValidationRatio = 0.0

            Dim network As TskNetwork = CreateModel(xTrain, ToJagged(yTrain), foldOptions)
            Dim trainer As New AnfisHybridTrainer(network, foldOptions)

            Call trainer.Train(xTrain, ToJagged(yTrain))

            Dim evaluation As AnfisFitResult = Evaluate(network, xTest, ToJagged(yTest))

            foldRmse(fold) = evaluation.RMSE
            foldR2(fold) = evaluation.R2
            models(fold) = network
        Next

        Return New AnfisCrossValidationResult With {
            .FoldRmse = foldRmse,
            .FoldR2 = foldR2,
            .Models = models
        }
    End Function

    Private Function ComputeRuleActivations(model As TskNetwork, x As Double()()) As Double()
        Dim layers As New TskLayers(model)
        Dim result(model.Rules.RuleCount - 1) As Double
        Dim chunk As Integer = 512

        For start As Integer = 0 To x.Length - 1 Step chunk
            Dim size As Integer = std.Min(chunk, x.Length - start)
            Dim fwd As TskInferenceTrace = layers.Forward(x, start, size)

            For t As Integer = 0 To size - 1
                For r As Integer = 0 To model.Rules.RuleCount - 1
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

    Private Function ToJagged(values As Double(,)) As Double()()
        Dim rows As Integer = values.GetLength(0)
        Dim cols As Integer = values.GetLength(1)
        Dim result(rows - 1)() As Double

        For i As Integer = 0 To rows - 1
            Dim row(cols - 1) As Double

            For j As Integer = 0 To cols - 1
                row(j) = values(i, j)
            Next

            result(i) = row
        Next

        Return result
    End Function

End Module

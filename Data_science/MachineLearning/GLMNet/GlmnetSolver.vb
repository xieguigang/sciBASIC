Imports std = System.Math

''' <summary>
''' 顶层统一入口：对应 R 端 <c>glmnet()</c> 与 <c>cv.glmnet()</c>。
''' </summary>
''' <remarks>
''' 调用形式为 <c>GlmnetSolver.Fit(x, y, "gaussian", options)</c>；
''' 模块名不使用 <c>glmnet</c> 是为了避开 VB 大小写不敏感导致的
''' 与根命名空间 <c>GLMNet</c> 的冲突。
''' </remarks>
Public Module GlmnetSolver

    ''' <summary>以 R 风格 family 名称拟合完整正则化路径。</summary>
    Public Function Fit(x As Double()(), y As Double()(), family As String,
                        Optional options As GlmnetOptions = Nothing) As GlmnetFit
        Return Fit(x, y, FamilyRegistry.Parse(family), options)
    End Function

    ''' <summary>以枚举形式拟合完整正则化路径。</summary>
    Public Function Fit(x As Double()(), y As Double()(), family As FamilyKind,
                        Optional options As GlmnetOptions = Nothing) As GlmnetFit
        Dim opts As GlmnetOptions = If(options, New GlmnetOptions()).Clone()
        Dim data As New GlmnetData(x, y, opts, family)
        Return FitCore(data, opts, family)
    End Function

    ''' <summary>稀疏设计矩阵入口（CSC 会展开为列主序数组后走同一内核）。</summary>
    Public Function Fit(x As SparseCSC, y As Double()(), family As String,
                        Optional options As GlmnetOptions = Nothing) As GlmnetFit
        If x Is Nothing Then Throw New ArgumentNullException(NameOf(x))
        Return Fit(x.ToDenseRows(), y, family, options)
    End Function

    ''' <summary>以 R 风格 family 名称执行 K 折交叉验证。</summary>
    Public Function CrossValidate(x As Double()(), y As Double()(), family As String,
                                  Optional options As GlmnetOptions = Nothing) As GlmnetCVFit
        Return CrossValidate(x, y, FamilyRegistry.Parse(family), options)
    End Function

    ''' <summary>以枚举形式执行 K 折交叉验证。</summary>
    Public Function CrossValidate(x As Double()(), y As Double()(), family As FamilyKind,
                                  Optional options As GlmnetOptions = Nothing) As GlmnetCVFit
        Dim opts As GlmnetOptions = If(options, New GlmnetOptions()).Clone()
        Dim data As New GlmnetData(x, y, opts, family)
        Call opts.Validate(data.N, data.P, data.K, family, forCrossValidation:=True)
        Return CrossValidateCore(data, opts, family)
    End Function

    ''' <summary>共用的拟合适配：参数校验 → family 分发 → 结果装配。</summary>
    Friend Function FitCore(data As GlmnetData, options As GlmnetOptions, family As FamilyKind) As GlmnetFit
        Call options.Validate(data.N, data.P, data.K, family)
        Dim solver As IGlmnetFamily = FamilyRegistry.Create(family)
        Dim path As FamilyPath = solver.Fit(data, options)
        Dim result As New GlmnetFit(path)

        If options.Relax Then
            result.RelaxedModel = RelaxedFitting.Fit(result, options)
        End If

        Return result
    End Function

    ''' <summary>
    ''' 统一 CV 流程：全量路径给出共享 lambda 网格 → 逐折拟合 → 折外指标
    ''' → 汇总均值/标准误 → lambda.min / lambda.1se。
    ''' </summary>
    Private Function CrossValidateCore(data As GlmnetData, options As GlmnetOptions, family As FamilyKind) As GlmnetCVFit
        Dim fullFit As GlmnetFit = FitCore(data, options, family)
        Dim grid As Double() = CType(fullFit.lambda.Clone(), Double())
        Dim steps As Integer = grid.Length
        Dim nfolds As Integer = options.NFolds
        Dim measure As CVMeasure = options.ResolveMeasure(family)
        Dim solver As IGlmnetFamily = FamilyRegistry.Create(family)
        Dim maximize As Boolean = solver.IsMaximize(measure)
        Dim foldId As Integer() = If(options.FoldId, MakeFolds(data, nfolds, options.Seed))
        Dim foldErrors As Double()() = New Double(nfolds - 1)() {}
        Dim preval As Double()()() = Nothing

        If options.Keep Then
            preval = New Double(steps - 1)()() {}
            For s As Integer = 0 To steps - 1
                preval(s) = New Double(data.N - 1)() {}
                For i As Integer = 0 To data.N - 1
                    preval(s)(i) = New Double(data.K - 1) {}
                Next
            Next
        End If

        For f As Integer = 0 To nfolds - 1
            Dim trainRows As New List(Of Integer)()
            Dim testRows As New List(Of Integer)()
            For i As Integer = 0 To data.N - 1
                If foldId(i) = f Then
                    testRows.Add(i)
                Else
                    trainRows.Add(i)
                End If
            Next
            If trainRows.Count = 0 OrElse testRows.Count = 0 Then
                foldErrors(f) = New Double(steps - 1) {}
                Continue For
            End If

            Dim trainData As GlmnetData = data.Subset(trainRows.ToArray())
            Dim testData As GlmnetData = data.Subset(testRows.ToArray())
            Dim foldOptions As GlmnetOptions = options.Clone()
            foldOptions.Lambda = grid
            foldOptions.NLambda = steps
            foldOptions.Weights = Nothing
            foldOptions.Offset = Nothing
            foldOptions.FoldId = Nothing

            Dim foldFit As GlmnetFit = FitCore(trainData, foldOptions, family)
            Dim predictions As Double()()() = PredictAllSteps(foldFit, testData.X, steps)
            foldErrors(f) = solver.Measure(predictions, testData, measure)

            If options.Keep Then
                For t As Integer = 0 To testRows.Count - 1
                    Dim originalIndex As Integer = testRows(t)
                    For s As Integer = 0 To steps - 1
                        For c As Integer = 0 To data.K - 1
                            preval(s)(originalIndex)(c) = predictions(s)(t)(c)
                        Next
                    Next
                Next
            End If
        Next

        Dim cvm As Double() = New Double(steps - 1) {}
        Dim cvsd As Double() = New Double(steps - 1) {}
        Dim cvup As Double() = New Double(steps - 1) {}
        Dim cvlo As Double() = New Double(steps - 1) {}

        For s As Integer = 0 To steps - 1
            Dim mean As Double = 0.0R
            For f As Integer = 0 To nfolds - 1
                mean += foldErrors(f)(s)
            Next
            mean /= CDbl(nfolds)

            Dim variance As Double = 0.0R
            For f As Integer = 0 To nfolds - 1
                Dim d As Double = foldErrors(f)(s) - mean
                variance += d * d
            Next
            Dim sd As Double = std.Sqrt(variance / CDbl(std.Max(1, nfolds - 1)))
            Dim se As Double = sd / std.Sqrt(CDbl(nfolds))

            cvm(s) = mean
            cvsd(s) = se
            cvup(s) = mean + se
            cvlo(s) = mean - se
        Next

        Dim indexMin As Integer = 0
        For s As Integer = 1 To steps - 1
            If If(maximize, cvm(s) > cvm(indexMin), cvm(s) < cvm(indexMin)) Then
                indexMin = s
            End If
        Next

        Dim threshold As Double = cvm(indexMin) + If(maximize, -1.0R, 1.0R) * cvsd(indexMin)
        Dim index1se As Integer = indexMin
        For s As Integer = 0 To indexMin
            If If(maximize, cvm(s) >= threshold, cvm(s) <= threshold) Then
                index1se = s
                Exit For
            End If
        Next

        Return New GlmnetCVFit With {
            .glmnetFit = fullFit,
            .lambda = grid,
            .cvm = cvm,
            .cvsd = cvsd,
            .cvup = cvup,
            .cvlo = cvlo,
            .nzero = fullFit.nzero,
            .lambdaMin = grid(indexMin),
            .lambda1se = grid(index1se),
            .indexMin = indexMin,
            .index1se = index1se,
            .foldid = foldId,
            .name = FamilyRegistry.MeasureName(measure),
            .measure = measure,
            .Maximize = maximize,
            .keep = options.Keep,
            .fitPreval = preval
        }
    End Function

    ''' <summary>在一条路径的全部 lambda 步骤上生成折外 link 预测。</summary>
    Private Function PredictAllSteps(fit As GlmnetFit, newx As Double()(), steps As Integer) As Double()()()
        Dim result As Double()()() = New Double(steps - 1)()() {}

        For s As Integer = 0 To steps - 1
            Dim idx As Integer = std.Min(s, fit.Count - 1)
            Dim coefficients As Double()() = fit.GetCoefficients(idx)
            result(s) = fit.LinearPredictor(newx, coefficients)
        Next

        Return result
    End Function

    ''' <summary>
    ''' 确定性分折：分类按类别、Cox 按事件状态分层，其余整体随机打乱。
    ''' 与 glmnet 的 <c>foldid</c> 语义一致，同一种子可完全复现。
    ''' </summary>
    Public Function MakeFolds(data As GlmnetData, nfolds As Integer, seed As Integer) As Integer()
        Dim n As Integer = data.N
        Dim rng As New Random(seed)
        Dim groups As New Dictionary(Of Integer, List(Of Integer))()

        For i As Integer = 0 To n - 1
            Dim key As Integer
            Select Case data.Family
                Case FamilyKind.Binomial
                    key = If(data.Y(i)(0) >= 0.5R, 1, 0)
                Case FamilyKind.Multinomial
                    key = 0
                    For c As Integer = 1 To data.K - 1
                        If data.Y(i)(c) > data.Y(i)(key) Then key = c
                    Next
                Case FamilyKind.Cox
                    key = If(data.Y(i)(1) > 0.0R, 1, 0)
                Case Else
                    key = 0
            End Select

            If Not groups.ContainsKey(key) Then groups(key) = New List(Of Integer)()
            groups(key).Add(i)
        Next

        Dim foldId As Integer() = New Integer(n - 1) {}
        For Each kv As KeyValuePair(Of Integer, List(Of Integer)) In groups
            Dim rows As List(Of Integer) = kv.Value
            For i As Integer = rows.Count - 1 To 1 Step -1
                Dim j As Integer = rng.Next(i + 1)
                Dim tmp As Integer = rows(i)
                rows(i) = rows(j)
                rows(j) = tmp
            Next
            For rank As Integer = 0 To rows.Count - 1
                foldId(rows(rank)) = rank Mod nfolds
            Next
        Next

        Return foldId
    End Function

End Module

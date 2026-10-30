Option Strict On
Option Explicit On

Imports System
Imports System.Linq
Imports Microsoft.VisualBasic.DeepLearning.ANFIS
Imports Microsoft.VisualBasic.DataMining.FuzzyLogic.TSK
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Solvers
Imports std = System.Math

''' <summary>
''' ANFIS 自检程序：把"算法是否真的按公式实现"变成可执行的断言。
''' </summary>
''' <remarks>
''' 重点不是跑出一个好看的 R2，而是守住三条数值契约：
''' <list type="number">
''' <item>前向结果必须与逐样本的标量参考实现逐位一致；</item>
''' <item>解析梯度必须与中心差分的数值梯度一致（没有自动微分，这是唯一的正确性证据）；</item>
''' <item>结论参数的 LSE 必须与 <c>OLS.LeastSquares</c> 一致。</item>
''' </list>
''' </remarks>
Module Program

    Private checks As Integer = 0
    Private failures As Integer = 0

    Function Main(args As String()) As Integer
        Call Section("1. 隶属函数的解析导数 vs 中心差分")
        Call TestMembershipGradients()

        Call Section("2. 批量前向 vs 标量参考实现")
        Call TestForwardAgainstScalar()

        Call Section("3. 前提参数梯度 vs 中心差分")
        Call TestPremiseGradient()

        Call Section("4. LSE vs OLS.LeastSquares")
        Call TestLeastSquaresAgainstOls()

        Call Section("5. 规则库枚举")
        Call TestFuzzyRuleBaseEnumeration()

        Call Section("6. 一维函数逼近 y = sin(x)")
        Call TestOneDimensional()

        Call Section("7. 二维函数逼近 y = sin(x1)*cos(x2)")
        Call TestTwoDimensional()

        Call Section("8. 混合学习 vs 纯梯度下降")
        Call TestLearningModes()

        Call Section("9. 规则剪枝")
        Call TestPruning()

        Call Section("10. 多输出与交叉验证")
        Call TestMultiOutput()
        Call TestCrossValidation()

        Call Section("11. 规模保护")
        Call TestRuleExplosionGuard()

        Call Section("12. JSON 往返")
        Call TestJsonRoundTrip()

        Call Section("13. 可复现性")
        Call TestDeterminism()

        Console.WriteLine()
        Console.WriteLine($"{checks} assertions, {failures} failure(s)")

        Return If(failures = 0, 0, 1)
    End Function

#Region "断言与工具"

    Private Sub Section(title As String)
        Console.WriteLine()
        Console.WriteLine($"=== {title} ===")
    End Sub

    Private Sub Check(name As String, condition As Boolean, Optional detail As String = "")
        checks += 1

        If condition Then
            Console.WriteLine($"  PASS  {name}  {detail}")
        Else
            failures += 1
            Console.WriteLine($"  FAIL  {name}  {detail}")
        End If
    End Sub

    ''' <summary>相对误差（分母带下限，避免接近 0 时放大噪声）。</summary>
    Private Function RelativeError(expected As Double, actual As Double) As Double
        Dim scale As Double = std.Max(1.0, std.Max(std.Abs(expected), std.Abs(actual)))
        Return std.Abs(expected - actual) / scale
    End Function

    ''' <summary>构造一个 2 输入、每输入 m 个模糊集的小网络，并随机填充结论参数。</summary>
    Private Function CreateTinyNetwork(m As Integer, seed As Integer) As TskNetwork
        Dim inputs As TskInputVariable() = {
            TskInputVariable.CreateGrid("x1", -1.0, 1.0, m, MembershipKind.Gaussian),
            TskInputVariable.CreateGrid("x2", -1.0, 1.0, m, MembershipKind.Gaussian)
        }
        Dim rules As FuzzyRuleBase = FuzzyRuleBase.CreateFullGrid({m, m})
        Dim net As New TskNetwork(inputs, rules, 1)

        Call SetRandomConsequent(net, seed)

        Return net
    End Function

    Private Sub SetRandomConsequent(net As TskNetwork, seed As Integer)
        Dim rng As New Random(seed)
        Dim theta(net.ConsequentParameterCount - 1, net.OutputCount - 1) As Double

        For i As Integer = 0 To theta.GetLength(0) - 1
            For k As Integer = 0 To theta.GetLength(1) - 1
                theta(i, k) = (rng.NextDouble() - 0.5) * 2.0
            Next
        Next

        net.Theta = theta
    End Sub

    ''' <summary>批量样本（行 = 样本）。</summary>
    Private Function SampleGrid(count As Integer, low As Double, high As Double, seed As Integer, dimension As Integer) As Double()()
        Dim rng As New Random(seed)
        Dim result(count - 1)() As Double

        For i As Integer = 0 To count - 1
            Dim row(dimension - 1) As Double

            For j As Integer = 0 To dimension - 1
                row(j) = low + rng.NextDouble() * (high - low)
            Next

            result(i) = row
        Next

        Return result
    End Function

    ''' <summary>E = 0.5 * sum((z - y)^2)，与解析梯度使用的目标函数一致。</summary>
    Private Function HalfSse(net As TskNetwork, x As Double()(), y As Double(,)) As Double
        Dim layers As New TskLayers(net)
        Dim fwd As TskInferenceTrace = layers.Forward(x, 0, x.Length)
        Dim sse As Double = 0.0

        For t As Integer = 0 To x.Length - 1
            For o As Integer = 0 To net.OutputCount - 1
                Dim d As Double = fwd.Z(t, o) - y(t, o)
                sse += d * d
            Next
        Next

        Return 0.5 * sse
    End Function

    Private Function CreateOptions() As AnfisOptions
        Return New AnfisOptions With {
            .MfKind = MembershipKind.Gaussian,
            .MfCount = 3,
            .LearningMode = AnfisLearningMode.Hybrid,
            .Optimizer = AnfisOptimizerKind.Adam,
            .LearningRate = 0.05,
            .LearningRateDecay = 0.05,
            .MaxEpoch = 40,
            .Patience = 15,
            .ValidationRatio = 0.2,
            .RandomSeed = 2024
        }
    End Function

#End Region

#Region "数值契约"

    Private Sub TestMembershipGradients()
        Dim kinds As MembershipKind() = {
            MembershipKind.Gaussian,
            MembershipKind.GeneralizedBell,
            MembershipKind.Sigmoid,
            MembershipKind.Triangular,
            MembershipKind.Trapezoidal
        }

        Dim probes As Double() = {0.2, 0.35, 0.65, 0.8}

        For Each kind As MembershipKind In kinds
            Dim mf As IMembershipFunction = MembershipFactory.CreateGrid(kind, 0.5, 0.4)
            Dim worst As Double = 0.0

            For Each x As Double In probes
                Dim analytic As Double() = mf.Gradient(x)
                Dim baseValues As Double() = mf.GetParameters()
                Dim h As Double = 0.000001

                For p As Integer = 0 To baseValues.Length - 1
                    Dim probe As Double() = CType(baseValues.Clone(), Double())

                    probe(p) = baseValues(p) + h
                    Call mf.SetParameters(probe)
                    Dim plus As Double = mf.Evaluate(x)

                    probe(p) = baseValues(p) - h
                    Call mf.SetParameters(probe)
                    Dim minus As Double = mf.Evaluate(x)

                    Call mf.SetParameters(baseValues)

                    Dim numeric As Double = (plus - minus) / (2.0 * h)
                    Dim errorValue As Double = std.Abs(analytic(p) - numeric) / std.Max(0.000001, std.Abs(numeric))

                    If errorValue > worst Then
                        worst = errorValue
                    End If
                Next
            Next

            Call Check($"{kind} 的解析导数", worst < 0.0001, $"worst relative error = {worst:E2}")
        Next
    End Sub

    Private Sub TestForwardAgainstScalar()
        Dim net As TskNetwork = CreateTinyNetwork(2, 7)
        Dim x As Double()() = SampleGrid(50, -1.0, 1.0, 11, 2)
        Dim layers As New TskLayers(net)
        Dim fwd As TskInferenceTrace = layers.Forward(x, 0, x.Length)

        Dim n As Integer = net.InputCount
        Dim worst As Double = 0.0

        For t As Integer = 0 To x.Length - 1
            ' 完全独立的标量参考实现：直接按定义连乘 + 加权平均
            Dim w(net.Rules.RuleCount - 1) As Double
            Dim total As Double = 0.0

            For r As Integer = 0 To net.Rules.RuleCount - 1
                Dim product As Double = 1.0

                For i As Integer = 0 To n - 1
                    product *= net.Inputs(i).Functions(net.Rules.Antecedent(r, i)).Evaluate(x(t)(i))
                Next

                w(r) = product
                total += product
            Next

            Dim z As Double = 0.0

            For r As Integer = 0 To net.Rules.RuleCount - 1
                Dim f As Double = net.Theta(r * (n + 1) + n, 0)

                For i As Integer = 0 To n - 1
                    f += net.Theta(r * (n + 1) + i, 0) * x(t)(i)
                Next

                z += w(r) / total * f
            Next

            Dim d As Double = std.Abs(z - fwd.Z(t, 0))

            If d > worst Then
                worst = d
            End If
        Next

        Call Check("前向输出与标量参考实现一致", worst < 0.000000000001, $"max |diff| = {worst:E2}")

        ' 归一化触发强度之和必须为 1
        Dim worstSum As Double = 0.0

        For t As Integer = 0 To x.Length - 1
            Dim sum As Double = 0.0

            For r As Integer = 0 To net.Rules.RuleCount - 1
                sum += fwd.Wbar(t, r)
            Next

            worstSum = std.Max(worstSum, std.Abs(sum - 1.0))
        Next

        Call Check("归一化触发强度之和为 1", worstSum < 0.000000000001, $"max |sum-1| = {worstSum:E2}")
    End Sub

    Private Sub TestPremiseGradient()
        Dim net As TskNetwork = CreateTinyNetwork(2, 13)
        Dim x As Double()() = SampleGrid(40, -1.0, 1.0, 17, 2)
        Dim y As Double(,) = New Double(x.Length - 1, 0) {}

        Dim rng As New Random(19)

        For i As Integer = 0 To x.Length - 1
            y(i, 0) = std.Sin(2.0 * x(i)(0)) + 0.5 * x(i)(1) * x(i)(1) + 0.05 * rng.NextDouble()
        Next

        Dim layers As New TskLayers(net)
        Dim fwd As TskInferenceTrace = layers.Forward(x, 0, x.Length)
        Dim gradient As TskGradient = layers.Accumulate(fwd, y, computeThetaGradient:=False)
        Dim baseParameters As Double() = net.GetPremiseParameters()
        Dim h As Double = 0.000001
        Dim worst As Double = 0.0

        For p As Integer = 0 To baseParameters.Length - 1
            Dim probe As Double() = CType(baseParameters.Clone(), Double())

            probe(p) = baseParameters(p) + h
            Call net.SetPremiseParameters(probe)
            Dim plus As Double = HalfSse(net, x, y)

            probe(p) = baseParameters(p) - h
            Call net.SetPremiseParameters(probe)
            Dim minus As Double = HalfSse(net, x, y)

            Call net.SetPremiseParameters(baseParameters)

            Dim numeric As Double = (plus - minus) / (2.0 * h)
            Dim errorValue As Double = std.Abs(gradient.Premise(p) - numeric) / std.Max(0.000001, std.Abs(numeric))

            If errorValue > worst Then
                worst = errorValue
            End If
        Next

        Call Check($"前提参数解析梯度（{baseParameters.Length} 个）", worst < 0.000001, $"worst relative error = {worst:E2}")

        ' 结论参数梯度同样对拍（纯梯度下降模式依赖它）
        Dim thetaGradient As TskGradient = layers.Accumulate(fwd, y, computeThetaGradient:=True)
        Dim baseTheta As Double(,) = CType(net.Theta.Clone(), Double(,))
        Dim worstTheta As Double = 0.0

        For p As Integer = 0 To baseTheta.GetLength(0) - 1
            Dim probe As Double(,) = CType(baseTheta.Clone(), Double(,))

            probe(p, 0) = baseTheta(p, 0) + h
            net.Theta = probe
            Dim plus As Double = HalfSse(net, x, y)

            probe(p, 0) = baseTheta(p, 0) - h
            net.Theta = probe
            Dim minus As Double = HalfSse(net, x, y)

            net.Theta = baseTheta

            Dim numeric As Double = (plus - minus) / (2.0 * h)
            Dim errorValue As Double = std.Abs(thetaGradient.Theta(p, 0) - numeric) / std.Max(0.000001, std.Abs(numeric))

            If errorValue > worstTheta Then
                worstTheta = errorValue
            End If
        Next

        Call Check($"结论参数解析梯度（{baseTheta.GetLength(0)} 个）", worstTheta < 0.000001, $"worst relative error = {worstTheta:E2}")
    End Sub

    Private Sub TestLeastSquaresAgainstOls()
        Dim net As TskNetwork = CreateTinyNetwork(3, 23)
        Dim x As Double()() = SampleGrid(200, -1.0, 1.0, 29, 2)
        Dim y As Double(,) = New Double(x.Length - 1, 0) {}

        For i As Integer = 0 To x.Length - 1
            y(i, 0) = std.Sin(1.5 * x(i)(0)) + 0.3 * x(i)(1)
        Next

        Dim layers As New TskLayers(net)
        Dim p As Integer = net.ConsequentParameterCount
        Dim accumulator As New LeastSquaresAccumulator(p, 1, 0.0)
        Dim full As Double(,) = New Double(x.Length - 1, p - 1) {}

        Dim start As Integer = 0

        While start < x.Length
            Dim size As Integer = std.Min(64, x.Length - start)
            Dim fwd As TskInferenceTrace = layers.Forward(x, start, size)
            Dim chunk As Double(,) = New Double(size - 1, p - 1) {}

            Call layers.BuildDesignMatrix(fwd, chunk)
            Call accumulator.AddChunk(chunk, AnfisDataSlice(y, start, size))

            For t As Integer = 0 To size - 1
                For c As Integer = 0 To p - 1
                    full(start + t, c) = chunk(t, c)
                Next
            Next

            start += size
        End While

        Dim warning As String = ""
        Dim theta As Double(,) = accumulator.Solve(warning)
        Dim reference As Double() = OLS.LeastSquares(full, Column(y))
        Dim worst As Double = 0.0

        For i As Integer = 0 To p - 1
            worst = std.Max(worst, std.Abs(theta(i, 0) - reference(i)))
        Next

        Call Check("分块 LSE 与 OLS 的解一致", worst < 0.000000001, $"max |diff| = {worst:E2}")
        Call Check("LSE 未触发降级", String.IsNullOrEmpty(warning), warning)
    End Sub

    Private Sub TestFuzzyRuleBaseEnumeration()
        Dim rules As FuzzyRuleBase = FuzzyRuleBase.CreateFullGrid({3, 2, 4})
        Call Check("m^n 规则数", rules.RuleCount = 24, $"got {rules.RuleCount}")

        Dim table As Integer(,) = rules.ToAntecedentTable()
        Dim distinct As Integer = 0

        For r As Integer = 0 To rules.RuleCount - 1
            If table(r, 0) >= 0 AndAlso table(r, 0) < 3 AndAlso table(r, 1) >= 0 AndAlso table(r, 1) < 2 AndAlso
               table(r, 2) >= 0 AndAlso table(r, 2) < 4 Then
                distinct += 1
            End If
        Next

        Call Check("前件下标都在合法范围内", distinct = 24, $"valid = {distinct}")

        Dim mask As Boolean() = New Boolean(23) {}

        For i As Integer = 0 To 5
            mask(i) = True
        Next

        Dim subset As FuzzyRuleBase = FuzzyRuleBase.CreateFullGrid({3, 2, 4}, mask)
        Call Check("启用掩膜生效", subset.RuleCount = 6, $"got {subset.RuleCount}")

        Dim pruned As FuzzyRuleBase = subset.SelectRules({5, 0})
        Call Check("规则选集", pruned.RuleCount = 2, $"got {pruned.RuleCount}")
    End Sub

#End Region

#Region "训练"

    Private Sub TestOneDimensional()
        Dim count As Integer = 200
        Dim x(count - 1)() As Double
        Dim y(count - 1) As Double

        For i As Integer = 0 To count - 1
            Dim v As Double = -std.PI + 2.0 * std.PI * i / (count - 1)
            x(i) = New Double() {v}
            y(i) = std.Sin(v)
        Next

        Dim options As AnfisOptions = CreateOptions()
        options.MfCount = 5
        options.MaxEpoch = 60

        Dim result As AnfisFitResult = AnfisFitting.Fit(x, y, options)

        Call Check("1D sin 逼近 RMSE < 0.01", result.RMSE < 0.01, $"RMSE = {result.RMSE:G6}, R2 = {result.R2:G6}, epochs = {result.Iterations}")
        Call Check("1D 末期误差低于首期", LastOf(result.TrainRmseHistory) < FirstOf(result.TrainRmseHistory),
                   $"first = {FirstOf(result.TrainRmseHistory):G6}, last = {LastOf(result.TrainRmseHistory):G6}")
    End Sub

    Private Sub TestTwoDimensional()
        Dim x As Double()() = SampleGrid(225, -2.0, 2.0, 31, 2)
        Dim y(x.Length - 1) As Double

        For i As Integer = 0 To x.Length - 1
            y(i) = std.Sin(x(i)(0)) * std.Cos(x(i)(1))
        Next

        Dim options As AnfisOptions = CreateOptions()
        Dim result As AnfisFitResult = AnfisFitting.Fit(x, y, options)

        Dim validation As Double = If(result.ValidationRmseHistory.Length > 0, result.ValidationRmseHistory.Min(), Double.NaN)

        Call Check("2D sin*cos 拟合 RMSE < 0.05", result.RMSE < 0.05, $"RMSE = {result.RMSE:G6}, R2 = {result.R2:G6}")
        Call Check("2D 最优验证 RMSE < 0.05", validation < 0.05, $"best validation RMSE = {validation:G6}")
        Call Check("规则数 = m^n", result.Model.Rules.RuleCount = 9, $"rules = {result.Model.Rules.RuleCount}")
        Call Check("每条规则的平均触发强度之和为 1", std.Abs(result.RuleActivations.Sum() - 1.0) < 0.000000001,
                   $"sum = {result.RuleActivations.Sum():G6}")
    End Sub

    Private Sub TestLearningModes()
        Dim x As Double()() = SampleGrid(225, -2.0, 2.0, 31, 2)
        Dim y(x.Length - 1) As Double

        For i As Integer = 0 To x.Length - 1
            y(i) = std.Sin(x(i)(0)) * std.Cos(x(i)(1))
        Next

        Dim hybridOptions As AnfisOptions = CreateOptions()
        hybridOptions.LearningMode = AnfisLearningMode.Hybrid
        hybridOptions.MaxEpoch = 25

        Dim gradientOptions As AnfisOptions = CreateOptions()
        gradientOptions.LearningMode = AnfisLearningMode.GradientOnly
        gradientOptions.MaxEpoch = 25

        Dim sequentialOptions As AnfisOptions = CreateOptions()
        sequentialOptions.LearningMode = AnfisLearningMode.SequentialLse
        sequentialOptions.MaxEpoch = 25

        Dim hybrid As AnfisFitResult = AnfisFitting.Fit(x, y, hybridOptions)
        Dim gradientOnly As AnfisFitResult = AnfisFitting.Fit(x, y, gradientOptions)
        Dim sequential As AnfisFitResult = AnfisFitting.Fit(x, y, sequentialOptions)

        Call Check("混合学习优于纯梯度下降", hybrid.RMSE < gradientOnly.RMSE,
                   $"hybrid = {hybrid.RMSE:G6}, gradient = {gradientOnly.RMSE:G6}")
        Call Check("递推 LSE 收敛到合理误差", sequential.RMSE < 0.2 AndAlso Not Double.IsNaN(sequential.RMSE),
                   $"sequential = {sequential.RMSE:G6}")
    End Sub

    Private Sub TestPruning()
        Dim x As Double()() = SampleGrid(225, -2.0, 2.0, 31, 2)
        Dim y(x.Length - 1) As Double

        For i As Integer = 0 To x.Length - 1
            y(i) = std.Sin(x(i)(0)) * std.Cos(x(i)(1))
        Next

        ' 冻结前提参数（只做 LSE）：隶属函数保持初始网格划分，
        ' 这样"远处的规则激活强度趋近 0"这一前提才成立。
        ' 混合训练会把高斯的宽度撑大（本自检里从 0.67 涨到 1.2 左右），
        ' 那时网格规则几乎处处有贡献，本来就没有可剪的冗余。
        Dim options As AnfisOptions = CreateOptions()
        options.FreezePremise = True
        options.LearningMode = AnfisLearningMode.GradientPlusOneShotLse
        options.MaxEpoch = 20

        Dim fit As AnfisFitResult = AnfisFitting.Fit(x, y, options)

        ' 只用"左下角"的数据做剪枝统计：远离该区域的规则平均触发强度会趋近 0，
        ' 这正是网格划分产生冗余规则的典型场景（数据分布只覆盖定义域的一角）
        Dim cornerX As New System.Collections.Generic.List(Of Double())
        Dim cornerY As New System.Collections.Generic.List(Of Double)

        For i As Integer = 0 To x.Length - 1
            If x(i)(0) < -0.5 AndAlso x(i)(1) < -0.5 Then
                Call cornerX.Add(x(i))
                Call cornerY.Add(y(i))
            End If
        Next

        Dim xs As Double()() = cornerX.ToArray
        Dim ys(xs.Length - 1) As Double

        For i As Integer = 0 To xs.Length - 1
            ys(i) = cornerY(i)
        Next

        Dim targets(xs.Length - 1)() As Double

        For i As Integer = 0 To xs.Length - 1
            targets(i) = New Double() {ys(i)}
        Next

        ' 阈值取 0.5%：9 条规则的平均触发强度上限是 11%，0.5% 已经低了一个数量级以上
        Dim baseline As AnfisFitResult = AnfisFitting.Evaluate(fit.Model, xs, targets)
        Dim pruned As AnfisFitResult = AnfisFitting.PruneRules(fit.Model, xs, targets, 0.005, options)
        Dim before As Integer = fit.Model.Rules.RuleCount
        Dim after As Integer = pruned.Model.Rules.RuleCount

        Call Check("剪枝后规则数只减不增", after <= before, $"{before} -> {after}")
        Call Check("远离数据区域的规则被识别为冗余", pruned.PrunedRuleCount > 0,
                   $"pruned = {pruned.PrunedRuleCount}, 最小平均触发强度 = {fit.RuleActivations.Min():E2}")

        Dim degradation As Double = (pruned.RMSE - baseline.RMSE) / std.Max(0.000001, baseline.RMSE)

        Call Check("剪枝后（同一区域）RMSE 退化 < 5%", degradation < 0.05,
                   $"RMSE {baseline.RMSE:G6} -> {pruned.RMSE:G6} ({degradation:P2})")
    End Sub

    Private Sub TestMultiOutput()
        Dim x As Double()() = SampleGrid(150, -1.5, 1.5, 47, 2)
        Dim y(x.Length - 1)() As Double

        For i As Integer = 0 To x.Length - 1
            y(i) = New Double() {std.Sin(x(i)(0)), std.Cos(x(i)(1))}
        Next

        Dim options As AnfisOptions = CreateOptions()
        options.MaxEpoch = 30

        Dim fit As AnfisFitResult = AnfisFitting.Fit(x, y, options)
        Dim prediction As Double()() = AnfisFitting.Predict(fit.Model, x)

        Call Check("多输出共享同一前提结构", fit.Model.OutputCount = 2, $"{fit.Model}")
        Call Check("多输出拟合 RMSE < 0.05", fit.RMSE < 0.05, $"RMSE = {fit.RMSE:G6}, R2 = {fit.R2:G6}")
        Call Check("多输出预测形状正确", prediction.Length = x.Length AndAlso prediction(0).Length = 2,
                   $"shape = {prediction.Length} x {prediction(0).Length}")
    End Sub

    Private Sub TestCrossValidation()
        Dim x As Double()() = SampleGrid(120, -1.5, 1.5, 53, 2)
        Dim y(x.Length - 1)() As Double

        For i As Integer = 0 To x.Length - 1
            y(i) = New Double() {std.Sin(x(i)(0)) * std.Cos(x(i)(1))}
        Next

        Dim options As AnfisOptions = CreateOptions()
        options.MaxEpoch = 10

        Dim cv As AnfisCrossValidationResult = AnfisFitting.CrossValidate(x, y, 3, options)

        Call Check("交叉验证折数", cv.FoldRmse.Length = 3, $"folds = {cv.FoldRmse.Length}")
        Call Check("交叉验证平均 RMSE 有限且合理", Not Double.IsNaN(cv.MeanRmse) AndAlso cv.MeanRmse < 0.2, $"{cv}")
    End Sub

    Private Sub TestRuleExplosionGuard()
        Dim options As AnfisOptions = CreateOptions()
        options.MfCount = 12

        Try
            Dim x As Double()() = SampleGrid(50, -1.0, 1.0, 59, 3)
            Dim y(x.Length - 1) As Double

            For i As Integer = 0 To x.Length - 1
                y(i) = x(i)(0) + x(i)(1) - x(i)(2)
            Next

            Call AnfisFitting.Fit(x, y, options)
            Call Check("规则数超限时抛出异常", False, "no exception was thrown")
        Catch ex As InvalidOperationException
            Call Check("规则数超限时抛出异常", ex.Message.Contains("MaxRules"), $"12^3 = 1728 > 1024; {ex.Message.Substring(0, 60)}...")
        End Try
    End Sub

    Private Sub TestJsonRoundTrip()
        Dim x As Double()() = SampleGrid(120, -2.0, 2.0, 37, 2)
        Dim y(x.Length - 1) As Double

        For i As Integer = 0 To x.Length - 1
            y(i) = std.Sin(x(i)(0)) * std.Cos(x(i)(1))
        Next

        Dim options As AnfisOptions = CreateOptions()
        options.MaxEpoch = 10

        Dim fit As AnfisFitResult = AnfisFitting.Fit(x, y, options)
        Dim json As String = fit.Model.ToJson()
        Dim restored As TskNetwork = TskNetwork.FromJson(json)

        Dim original As Double()() = AnfisFitting.Predict(fit.Model, x)
        Dim reloaded As Double()() = AnfisFitting.Predict(restored, x)
        Dim worst As Double = 0.0

        For i As Integer = 0 To x.Length - 1
            worst = std.Max(worst, std.Abs(original(i)(0) - reloaded(i)(0)))
        Next

        Call Check("JSON 往返后预测一致", worst < 0.000000000001, $"max |diff| = {worst:E2}")
        Call Check("JSON 往返后结构一致", restored.Rules.RuleCount = fit.Model.Rules.RuleCount AndAlso
                                          restored.InputCount = fit.Model.InputCount AndAlso
                                          restored.OutputCount = fit.Model.OutputCount,
                   $"{restored}")
        Call Check("FIS 文本导出非空", fit.Model.ExportFisText().Length > 100, $"length = {fit.Model.ExportFisText().Length}")
    End Sub

    Private Sub TestDeterminism()
        Dim x As Double()() = SampleGrid(150, -2.0, 2.0, 41, 2)
        Dim y(x.Length - 1) As Double

        For i As Integer = 0 To x.Length - 1
            y(i) = std.Sin(x(i)(0)) * std.Cos(x(i)(1))
        Next

        Dim first As AnfisFitResult = AnfisFitting.Fit(x, y, CreateOptions())
        Dim second As AnfisFitResult = AnfisFitting.Fit(x, y, CreateOptions())

        Dim identical As Boolean = first.RMSE = second.RMSE

        Call Check("同一配置两次训练结果逐位一致", identical, $"{first.RMSE:R} vs {second.RMSE:R}")

        Dim xTest As Double()() = SampleGrid(20, -2.0, 2.0, 43, 2)
        Dim a As Double()() = AnfisFitting.Predict(first.Model, xTest)
        Dim b As Double()() = AnfisFitting.Predict(second.Model, xTest)
        Dim worst As Double = 0.0

        For i As Integer = 0 To xTest.Length - 1
            worst = std.Max(worst, std.Abs(a(i)(0) - b(i)(0)))
        Next

        Call Check("同一配置两次训练的预测逐位一致", worst = 0.0, $"max |diff| = {worst:E2}")
    End Sub

#End Region

#Region "小工具"

    Private Function FirstOf(values As Double()) As Double
        Return If(values Is Nothing OrElse values.Length = 0, Double.NaN, values(0))
    End Function

    Private Function LastOf(values As Double()) As Double
        Return If(values Is Nothing OrElse values.Length = 0, Double.NaN, values(values.Length - 1))
    End Function

    ''' <summary>把二维目标的第 0 列取出（供 OLS 对拍）。</summary>
    Private Function Column(values As Double(,)) As Double()
        Dim result(values.GetLength(0) - 1) As Double

        For i As Integer = 0 To result.Length - 1
            result(i) = values(i, 0)
        Next

        Return result
    End Function

    ''' <summary>切出二维目标的若干行（与 <c>AnfisData.SliceRows</c> 等价，此处独立实现以免依赖 Friend 类型）。</summary>
    Private Function AnfisDataSlice(values As Double(,), start As Integer, count As Integer) As Double(,)
        Dim result(count - 1, 0) As Double

        For i As Integer = 0 To count - 1
            result(i, 0) = values(start + i, 0)
        Next

        Return result
    End Function

#End Region

End Module

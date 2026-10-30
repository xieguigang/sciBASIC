Option Strict On
Option Explicit On

Imports System
Imports System.Linq
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports std = System.Math

Namespace TSK

    ''' <summary>
    ''' 按网格划分从训练数据构建 TSK 网络结构（不做任何训练）。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 每个输入等距放 m 个隶属函数，规则数 <c>m^n</c>。网格划分的优点是规则可解释、
    ''' 前向完全向量化；缺点是规则数随维数指数增长，
    ''' 因此 <see cref="TskStructureOptions.MaxRules"/> 与 <see cref="TskStructureOptions.MaxConsequentParams"/>
    ''' 会在超限时直接报错并给出可操作的提示。
    ''' </para>
    ''' <para>
    ''' 定义域统计**只使用传入的数据**。做交叉验证时请对训练折调用本方法，
    ''' 不要用全量数据建模再把同一结构套到每一折上（那会泄漏验证折的分布信息）。
    ''' </para>
    ''' </remarks>
    Public Module TskNetworkBuilder

        ''' <summary>
        ''' 按数据的取值范围与配置构建模型结构（不训练）。
        ''' </summary>
        ''' <param name="x">样本矩阵（行 = 样本，列 = 输入）</param>
        ''' <param name="y">目标矩阵（行 = 样本，列 = 输出），只用于读取输出维数与名称</param>
        ''' <param name="options">结构配置</param>
        Public Function CreateGrid(x As Double()(), y As Double()(), Optional options As TskStructureOptions = Nothing) As TskNetwork
            If x Is Nothing OrElse x.Length = 0 Then
                Throw New ArgumentException("the feature matrix is empty", NameOf(x))
            End If
            If y Is Nothing OrElse y.Length <> x.Length Then
                Throw New ArgumentException("the target does not match the feature matrix", NameOf(y))
            End If

            Dim config As TskStructureOptions = If(options, New TskStructureOptions()).ValidateStructure()
            Dim n As Integer = x(0).Length
            Dim outputCount As Integer = y(0).Length

            Dim mfCounts(n - 1) As Integer

            If config.MfCounts IsNot Nothing Then
                If config.MfCounts.Length <> n Then
                    Throw New ArgumentException($"MfCounts has {config.MfCounts.Length} entries but there are {n} inputs", NameOf(options))
                End If

                Call Array.Copy(config.MfCounts, mfCounts, n)
            Else
                For i As Integer = 0 To n - 1
                    mfCounts(i) = config.MfCount
                Next
            End If

            Dim names As String() = config.FeatureNames

            If names Is Nothing OrElse names.Length <> n Then
                names = Enumerable.Range(1, n).Select(Function(i) $"x{i}").ToArray
            End If

            Dim inputs(n - 1) As TskInputVariable

            For i As Integer = 0 To n - 1
                Dim minimum As Double = Double.MaxValue
                Dim maximum As Double = Double.MinValue

                For r As Integer = 0 To x.Length - 1
                    Dim v As Double = x(r)(i)

                    If v < minimum Then
                        minimum = v
                    End If
                    If v > maximum Then
                        maximum = v
                    End If
                Next

                inputs(i) = TskInputVariable.CreateGrid(names(i), minimum, maximum, mfCounts(i), config.MfKind)
            Next

            Dim rules As FuzzyRuleBase = FuzzyRuleBase.CreateFullGrid(mfCounts, config.RuleEnableMask, config.MaxRules)
            Dim consequentCount As Integer = rules.RuleCount * (n + 1)

            If consequentCount > config.MaxConsequentParams Then
                Throw New InvalidOperationException(
                    $"{rules.RuleCount} rules over {n} inputs need {consequentCount} consequent parameters, " &
                    $"which exceeds MaxConsequentParams = {config.MaxConsequentParams}. " &
                    $"The least squares normal equations would need {consequentCount}^2 doubles of memory; " &
                    $"reduce MfCount, prune rules, or switch to a sequential update strategy.")
            End If

            Dim outputNames As String() = config.OutputNames

            If outputNames Is Nothing OrElse outputNames.Length <> outputCount Then
                outputNames = Nothing
            End If

            Return New TskNetwork(inputs, rules, outputCount, outputNames)
        End Function

        ''' <summary>
        ''' 按 <see cref="TskStructureOptions.RulePartition"/> 选择划分方式构建结构（不训练）。
        ''' </summary>
        ''' <param name="x">样本矩阵（行 = 样本，列 = 输入）</param>
        ''' <param name="y">目标矩阵（行 = 样本，列 = 输出）</param>
        ''' <param name="options">结构配置</param>
        ''' <remarks>
        ''' <see cref="RulePartitionKind.Auto"/> 用与预算校验同一套公式判定：
        ''' 网格装得下（<c>m^n ≤ MaxRules</c> 且 <c>m^n·(n+1) ≤ MaxConsequentParams</c>）就用
        ''' <see cref="CreateGrid"/>，否则改用 <see cref="CreateScatter"/>。
        ''' 这样高维输入（例如 18 个调控因子）不再是「静默降 K」，而是自动换一种规则生成方式。
        ''' </remarks>
        Public Function Create(x As Double()(), y As Double()(), Optional options As TskStructureOptions = Nothing) As TskNetwork
            Dim config As TskStructureOptions = If(options, New TskStructureOptions()).ValidateStructure()

            Select Case config.RulePartition
                Case RulePartitionKind.Grid
                    Return CreateGrid(x, y, config)
                Case RulePartitionKind.Scatter
                    Return CreateScatter(x, y, config)
                Case Else
                    If FitsGrid(x, config) Then
                        Return CreateGrid(x, y, config)
                    Else
                        Return CreateScatter(x, y, config)
                    End If
            End Select
        End Function

        ''' <summary>
        ''' 判断给定输入维数下网格划分是否落在规则/参数预算之内（Auto 的判定依据）。
        ''' </summary>
        ''' <param name="x">样本矩阵（只用来读取输入维数）</param>
        ''' <param name="config">结构配置</param>
        Public Function FitsGrid(x As Double()(), config As TskStructureOptions) As Boolean
            If x Is Nothing OrElse x.Length = 0 OrElse x(0) Is Nothing Then
                Return False
            End If

            Dim n As Integer = x(0).Length
            Dim rules As Double

            If config.MfCounts IsNot Nothing AndAlso config.MfCounts.Length = n Then
                rules = 1.0

                For i As Integer = 0 To n - 1
                    rules *= config.MfCounts(i)
                Next
            Else
                rules = std.Pow(config.MfCount, n)
            End If

            ' 用 Double 计算：2^18 这类规模仍在 Double 的精确整数范围内，不会溢出
            Return rules <= config.MaxRules AndAlso rules * (n + 1) <= config.MaxConsequentParams
        End Function

        ''' <summary>
        ''' 散射划分（scatter partitioning）：KMeans 聚类，每个簇生成一条规则，
        ''' 每条规则对每个输入有自己的高斯中心/宽度。
        ''' </summary>
        ''' <param name="x">样本矩阵（行 = 样本，列 = 输入）</param>
        ''' <param name="y">目标矩阵（行 = 样本，列 = 输出），只用于聚类与读取输出维数</param>
        ''' <param name="options">结构配置</param>
        ''' <remarks>
        ''' <para>
        ''' 规则数 = 簇数（<see cref="TskStructureOptions.ScatterRuleCount"/>），与输入维数**无关**，
        ''' 因此 18 个输入也能落地（网格划分下那需要 2^18 条规则）。
        ''' </para>
        ''' <para>
        ''' 流程：① 每列统计量 → ② 标准化后把 <c>[x | y]</c> 拼成行做 KMeans → ③ 每簇算原始空间的
        ''' 中心与宽度 → ④ 按「簇中心相对全局均值的 z 分数」挑出该规则的活跃输入
        ''' （下限 <see cref="TskStructureOptions.ScatterMinInputs"/>、上限
        ''' <see cref="TskStructureOptions.ScatterInputs"/>）→ ⑤ 组装规则库。
        ''' </para>
        ''' <para>
        ''' 输入变量在这里只保留**一个占位高斯**（全局均值/标准差）：散射划分的前件参数存在规则库里，
        ''' 不依赖共享模糊集；占位高斯只是为了让 <see cref="TskInputVariable"/> 的既有契约成立。
        ''' </para>
        ''' </remarks>
        Public Function CreateScatter(x As Double()(), y As Double()(), Optional options As TskStructureOptions = Nothing) As TskNetwork
            If x Is Nothing OrElse x.Length = 0 Then
                Throw New ArgumentException("the feature matrix is empty", NameOf(x))
            End If
            If y Is Nothing OrElse y.Length <> x.Length Then
                Throw New ArgumentException("the target does not match the feature matrix", NameOf(y))
            End If

            Dim config As TskStructureOptions = If(options, New TskStructureOptions()).ValidateStructure()
            Dim rows As Integer = x.Length
            Dim n As Integer = x(0).Length
            Dim outputCount As Integer = y(0).Length

            If rows < 4 Then
                Throw New InvalidOperationException(
                    $"a scatter structure needs at least 4 samples to form clusters, but only {rows} were given")
            End If

            ' ---- 1. 全局统计量（原始空间）----
            Dim means(n - 1) As Double
            Dim sds(n - 1) As Double
            Dim minimum(n - 1) As Double
            Dim maximum(n - 1) As Double

            For i As Integer = 0 To n - 1
                Dim sum As Double = 0.0
                Dim low As Double = Double.MaxValue
                Dim high As Double = Double.MinValue

                For r As Integer = 0 To rows - 1
                    Dim v As Double = x(r)(i)

                    sum += v

                    If v < low Then low = v
                    If v > high Then high = v
                Next

                means(i) = sum / rows
                minimum(i) = low
                maximum(i) = high

                Dim ss As Double = 0.0

                For r As Integer = 0 To rows - 1
                    Dim d As Double = x(r)(i) - means(i)

                    ss += d * d
                Next

                sds(i) = If(rows > 1, std.Sqrt(ss / (rows - 1)), 0.0)

                If sds(i) < 1.0E-09 Then
                    ' 常数列：给一个非零尺度，避免把该维变成 delta 函数
                    sds(i) = If(std.Abs(maximum(i)) > 1.0E-09, std.Abs(maximum(i)), 1.0)
                End If
            Next

            ' ---- 2. 标准化后聚类（把输出维也放进去，让簇在「输入 + 响应」上都有区分度）----
            Dim yMean As Double = 0.0

            For r As Integer = 0 To rows - 1
                yMean += y(r)(0)
            Next

            yMean /= rows

            Dim ySs As Double = 0.0

            For r As Integer = 0 To rows - 1
                Dim d As Double = y(r)(0) - yMean

                ySs += d * d
            Next

            Dim ySd As Double = If(rows > 1, std.Sqrt(ySs / (rows - 1)), 0.0)

            If ySd < 1.0E-09 Then
                ySd = 1.0
            End If

            Dim matrix(rows - 1)() As Double
            Dim ids(rows - 1) As String

            For r As Integer = 0 To rows - 1
                Dim line(n) As Double
                Dim sample As Double() = x(r)

                For i As Integer = 0 To n - 1
                    line(i) = (sample(i) - means(i)) / sds(i)
                Next

                line(n) = (y(r)(0) - yMean) / ySd

                matrix(r) = line
                ids(r) = "s" & r
            Next

            Dim k As Integer = std.Min(config.ScatterRuleCount, rows - 1)

            If k < 2 Then
                Throw New InvalidOperationException(
                    $"a scatter structure needs at least 2 clusters, but only {k} can be formed from {rows} samples")
            End If

            ' 固定随机种子：同一份数据 + 同一份配置必须得到同一套规则
            Call Microsoft.VisualBasic.Math.RandomExtensions.SetSeed(config.ScatterSeed)

            Dim engine As New KMeansEngine(debug:=False, max_iters:=100, n_threads:=1, auto_parallel:=False)
            Dim clusters As NumericClusterCollection = engine.ClusterDataSet(matrix, ids, k)

            ' ---- 3. 每簇 → 中心/宽度/活跃掩码 ----
            Dim centers As New List(Of Double())
            Dim sigmas As New List(Of Double())
            Dim actives As New List(Of Boolean())

            For Each cluster As NumericKMeansCluster In clusters
                If cluster Is Nothing OrElse cluster.size < 1 Then
                    Continue For
                End If

                Dim size As Integer = cluster.size
                Dim center(n - 1) As Double
                Dim sigma(n - 1) As Double
                Dim activeMask(n - 1) As Boolean

                For i As Integer = 0 To n - 1
                    Dim sum As Double = 0.0

                    For m As Integer = 0 To size - 1
                        sum += x(cluster.ClusterIndex(m))(i)
                    Next

                    Dim c As Double = sum / size
                    Dim ss As Double = 0.0

                    center(i) = c

                    For m As Integer = 0 To size - 1
                        Dim d As Double = x(cluster.ClusterIndex(m))(i) - c

                        ss += d * d
                    Next

                    Dim sd As Double = If(size > 1, std.Sqrt(ss / (size - 1)), sds(i) * 0.5)
                    Dim floor As Double = sds(i) * 0.05

                    sigma(i) = std.Max(sd, floor)
                Next

                ' 自适应活跃输入：按「簇中心偏离全局均值的 z 分数」排序
                Dim order As Integer() = Enumerable _
                    .Range(0, n) _
                    .OrderByDescending(Function(i) std.Abs(center(i) - means(i)) / sds(i)) _
                    .ToArray()

                Dim qualified As Integer = 0

                For Each i As Integer In order
                    If std.Abs(center(i) - means(i)) / sds(i) >= config.ScatterZThreshold Then
                        qualified += 1
                    End If
                Next

                Dim keep As Integer = std.Max(config.ScatterMinInputs, qualified)

                keep = std.Min(keep, config.ScatterInputs)
                keep = std.Min(keep, n)

                For j As Integer = 0 To keep - 1
                    activeMask(order(j)) = True
                Next

                Call centers.Add(center)
                Call sigmas.Add(sigma)
                Call actives.Add(activeMask)
            Next

            Dim rules As Integer = centers.Count

            If rules = 0 Then
                Throw New InvalidOperationException("the kmeans clustering produced no usable cluster")
            End If
            If rules > config.MaxRules Then
                Throw New InvalidOperationException(
                    $"{rules} cluster rules exceed MaxRules = {config.MaxRules}; reduce ScatterRuleCount")
            End If

            Dim centerTable(rules - 1, n - 1) As Double
            Dim sigmaTable(rules - 1, n - 1) As Double
            Dim activeTable(rules - 1, n - 1) As Boolean

            For r As Integer = 0 To rules - 1
                For i As Integer = 0 To n - 1
                    centerTable(r, i) = centers(r)(i)
                    sigmaTable(r, i) = sigmas(r)(i)
                    activeTable(r, i) = actives(r)(i)
                Next
            Next

            Dim ruleBase As FuzzyRuleBase = FuzzyRuleBase.CreateScatter(centerTable, sigmaTable, activeTable, config.MaxRules)
            Dim consequentCount As Integer = ruleBase.RuleCount * (n + 1)

            If consequentCount > config.MaxConsequentParams Then
                Throw New InvalidOperationException(
                    $"{ruleBase.RuleCount} cluster rules over {n} inputs need {consequentCount} consequent parameters, " &
                    $"which exceeds MaxConsequentParams = {config.MaxConsequentParams}. Reduce ScatterRuleCount.")
            End If

            ' ---- 4. 输入变量（只保留一个占位高斯）----
            Dim names As String() = config.FeatureNames

            If names Is Nothing OrElse names.Length <> n Then
                names = Enumerable.Range(1, n).Select(Function(i) $"x{i}").ToArray
            End If

            Dim inputs(n - 1) As TskInputVariable

            For i As Integer = 0 To n - 1
                Dim placeholder As IMembershipFunction = MembershipFactory.Create(
                    config.MfKind, New Double() {means(i), sds(i)})

                inputs(i) = New TskInputVariable(names(i), minimum(i), maximum(i), New IMembershipFunction() {placeholder})
            Next

            Dim outputNames As String() = config.OutputNames

            If outputNames Is Nothing OrElse outputNames.Length <> outputCount Then
                outputNames = Nothing
            End If

            Return New TskNetwork(inputs, ruleBase, outputCount, outputNames)
        End Function

        ''' <summary>
        ''' 用显式给定的输入变量与规则表直接组装网络（用于从既有 Mamdani 模型或专家规则迁移）。
        ''' </summary>
        ''' <param name="inputs">输入变量（顺序即特征列顺序）</param>
        ''' <param name="rules">规则表</param>
        ''' <param name="outputCount">输出维数</param>
        ''' <param name="outputNames">输出名称，<c>Nothing</c> 时自动生成</param>
        Public Function FromRuleTable(inputs As TskInputVariable(),
                                      rules As FuzzyRuleBase,
                                      Optional outputCount As Integer = 1,
                                      Optional outputNames As String() = Nothing) As TskNetwork

            Return New TskNetwork(inputs, rules, outputCount, outputNames)
        End Function

    End Module

End Namespace

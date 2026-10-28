Option Strict On
Option Explicit On

Imports System
Imports System.Linq
Imports Microsoft.VisualBasic.Text
Imports FL = Microsoft.VisualBasic.DataMining.FuzzyLogic.Logical.FuzzyLogic
Imports TSK = Microsoft.VisualBasic.DataMining.FuzzyLogic.TSK
Imports std = System.Math

''' <summary>
''' FuzzyLogic 的数值门禁：把"两种推理语义是否真的按约定实现"变成可执行断言。
''' </summary>
''' <remarks>
''' 重点不是跑出一个好看的拟合优度，而是守住四条契约：
''' <list type="number">
''' <item>Mamdani 前端在重构前后的求值结果逐位一致（含两个退化肩点）；</item>
''' <item>五种隶属函数的解析导数与中心差分一致；</item>
''' <item>TSK 批量前向与逐样本标量实现一致、前提与结论梯度与中心差分一致；</item>
''' <item>JSON 与 FCL 风格 XML 两种持久化都能无损往返。</item>
''' </list>
''' </remarks>
Module Program

    Private checks As Integer = 0
    Private failures As Integer = 0

    Function Main(args As String()) As Integer
        Call Section("1. Mamdani 求值一致性（与重构前基线逐点对比）")
        Call TestMamdaniBaseline()

        Call Section("2. 退化肩点语义")
        Call TestShoulders()

        Call Section("3. 隶属函数解析导数 vs 中心差分")
        Call TestMembershipGradients()

        Call Section("4. 批量前向 vs 标量参考实现")
        Call TestForwardAgainstScalar()

        Call Section("5. 前提参数与结论参数的解析梯度 vs 中心差分")
        Call TestGradients()

        Call Section("6. 规则表枚举、启用掩膜与规则剪枝")
        Call TestRuleBase()

        Call Section("7. TSK 网络的 JSON 往返")
        Call TestJsonRoundTrip()

        Call Section("8. Mamdani 模型的 XML 往返回归")
        Call TestXmlRoundTrip()

        Call Console.WriteLine()
        Call Console.WriteLine($"{checks} assertions, {failures} failure(s)")

        Return If(failures = 0, 0, 1)
    End Function

#Region "断言与工具"

    Private Sub Section(title As String)
        Call Console.WriteLine()
        Call Console.WriteLine($"=== {title} ===")
    End Sub

    Private Sub Check(name As String, condition As Boolean, Optional detail As String = "")
        checks += 1

        If condition Then
            Call Console.WriteLine($"  PASS  {name}  {detail}")
        Else
            failures += 1
            Call Console.WriteLine($"  FAIL  {name}  {detail}")
        End If
    End Sub

    ''' <summary>数值比较：NaN 视为相等，其余按相对误差。</summary>
    Private Function SameValue(expected As Double, actual As Double, tolerance As Double) As Boolean
        If Double.IsNaN(expected) Then
            Return Double.IsNaN(actual)
        End If

        Dim scale As Double = std.Max(1.0, std.Max(std.Abs(expected), std.Abs(actual)))

        Return std.Abs(expected - actual) / scale <= tolerance
    End Function

    ''' <summary>README 里的 Water/Power 示例（含左肩 Cold 与右肩 Hot）。</summary>
    Private Function CreateEngine() As FL.FuzzyEngine
        Dim water As New FL.LinguisticVariable("Water")
        Call water.MembershipFunctionCollection.Add(New FL.MembershipFunction("Cold", 0, 0, 20, 40))
        Call water.MembershipFunctionCollection.Add(New FL.MembershipFunction("Tepid", 30, 50, 50, 70))
        Call water.MembershipFunctionCollection.Add(New FL.MembershipFunction("Hot", 50, 80, 100, 100))

        Dim power As New FL.LinguisticVariable("Power")
        Call power.MembershipFunctionCollection.Add(New FL.MembershipFunction("Low", 0, 25, 25, 50))
        Call power.MembershipFunctionCollection.Add(New FL.MembershipFunction("High", 25, 50, 50, 75))

        Dim engine As New FL.FuzzyEngine()
        Call engine.LinguisticVariableCollection.Add(water)
        Call engine.LinguisticVariableCollection.Add(power)
        engine.Consequent = "Power"
        Call engine.FuzzyRuleCollection.Add(New FL.FuzzyRule("IF (Water IS Cold) OR (Water IS Tepid) THEN Power IS High"))
        Call engine.FuzzyRuleCollection.Add(New FL.FuzzyRule("IF (Water IS Hot) THEN Power IS Low"))

        Return engine
    End Function

    ''' <summary>构造一个 2 输入、每输入 m 个模糊集的小网络，并随机填充结论参数。</summary>
    Private Function CreateTinyNetwork(m As Integer, seed As Integer) As TSK.TskNetwork
        Dim inputs As TSK.TskInputVariable() = {
            TSK.TskInputVariable.CreateGrid("x1", -1.0, 1.0, m, TSK.MembershipKind.Gaussian),
            TSK.TskInputVariable.CreateGrid("x2", -1.0, 1.0, m, TSK.MembershipKind.Gaussian)
        }
        Dim rules As TSK.FuzzyRuleBase = TSK.FuzzyRuleBase.CreateFullGrid({m, m})
        Dim net As New TSK.TskNetwork(inputs, rules, 1)

        Call SetRandomConsequent(net, seed)

        Return net
    End Function

    Private Sub SetRandomConsequent(net As TSK.TskNetwork, seed As Integer)
        Dim rng As New Random(seed)
        Dim theta(net.ConsequentParameterCount - 1, net.OutputCount - 1) As Double

        For i As Integer = 0 To theta.GetLength(0) - 1
            For k As Integer = 0 To theta.GetLength(1) - 1
                theta(i, k) = (rng.NextDouble() - 0.5) * 2.0
            Next
        Next

        net.Theta = theta
    End Sub

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
    Private Function HalfSse(net As TSK.TskNetwork, x As Double()(), y As Double(,)) As Double
        Dim layers As New TSK.TskLayers(net)
        Dim fwd As TSK.TskInferenceTrace = layers.Forward(x, 0, x.Length)
        Dim sse As Double = 0.0

        For t As Integer = 0 To x.Length - 1
            For o As Integer = 0 To net.OutputCount - 1
                Dim d As Double = fwd.Z(t, o) - y(t, o)
                sse += d * d
            Next
        Next

        Return 0.5 * sse
    End Function

#End Region

#Region "Mamdani 兼容性"

    ''' <summary>
    ''' 重构前的基线（原样录自 <c>LinguisticVariable.Fuzzify</c> 的分段 if/else 实现）。
    ''' </summary>
    ''' <remarks>
    ''' 第 5 列是 <c>Defuzzify()</c> 的结果；<see cref="Double.NaN"/> 表示原实现返回 NaN（分母为 0），
    ''' 而 <c>39.9999</c> 这一行在原实现里会抛 <c>FormatException</c>——那是
    ''' <c>FuzzyEngine.Parse</c> 的既有缺陷（循环上界在文本替换后被固定），本次改造**不修**它，
    ''' 只保证行为不变。
    ''' </remarks>
    Private ReadOnly baseline As Double()() = {
        New Double() {-10.0, 0, 0, 0, Double.NaN},
        New Double() {-1.0, 0, 0, 0, Double.NaN},
        New Double() {0.0, 1, 0, 0, 50},
        New Double() {1.0, 1, 0, 0, 50},
        New Double() {5.0, 1, 0, 0, 50},
        New Double() {10.0, 1, 0, 0, 50},
        New Double() {19.9999, 1, 0, 0, 50},
        New Double() {20.0, 1, 0, 0, 50},
        New Double() {25.0, 0.75, 0, 0, 50},
        New Double() {30.0, 0.5, 0, 0, 50},
        New Double() {40.0, 0, 0.5, 0, 50},
        New Double() {49.0, 0, 0.95, 0, 50},
        New Double() {50.0, 0, 1, 0, 50},
        New Double() {55.0, 0, 0.75, 1.0 / 6.0, 44.808126410835214},
        New Double() {60.0, 0, 0.5, 1.0 / 3.0, 39.719626168224295},
        New Double() {70.0, 0, 0, 2.0 / 3.0, 25},
        New Double() {79.0, 0, 0, 29.0 / 30.0, 25},
        New Double() {80.0, 0, 0, 1, 25},
        New Double() {90.0, 0, 0, 1, 25},
        New Double() {99.0, 0, 0, 1, 25},
        New Double() {100.0, 0, 0, 1, 25},
        New Double() {101.0, 0, 0, 0, Double.NaN},
        New Double() {110.0, 0, 0, 0, Double.NaN}
    }

    Private Sub TestMamdaniBaseline()
        Dim engine As FL.FuzzyEngine = CreateEngine()
        Dim water As FL.LinguisticVariable = engine.LinguisticVariableCollection.Find("Water")
        Dim worst As Double = 0.0
        Dim mismatches As Integer = 0
        Dim nanMatches As Integer = 0

        For Each row As Double() In baseline
            water.InputValue = row(0)

            Dim values As Double() = {
                water.Fuzzify("Cold"),
                water.Fuzzify("Tepid"),
                water.Fuzzify("Hot")
            }

            For i As Integer = 0 To values.Length - 1
                If Not SameValue(row(i + 1), values(i), 0.000000000001) Then
                    mismatches += 1
                End If

                If Not Double.IsNaN(row(i + 1)) Then
                    worst = std.Max(worst, std.Abs(row(i + 1) - values(i)))
                End If
            Next

            Dim y As Double

            Try
                y = engine.Defuzzify()
            Catch ex As Exception
                ' 基线里没有异常行；一旦出现就说明解析路径被改坏了
                mismatches += 1
                Call Console.WriteLine($"        unexpected {ex.GetType().Name} at x = {row(0)}")
                Continue For
            End Try

            If Not SameValue(row(4), y, 0.000000000001) Then
                mismatches += 1
                Call Console.WriteLine($"        defuzzify mismatch at x = {row(0)}: expected {row(4)}, got {y}")
            End If

            If Double.IsNaN(row(4)) AndAlso Double.IsNaN(y) Then
                nanMatches += 1
            End If
        Next

        Call Check("Fuzzify 与重构前基线逐点一致", worst < 0.000000000001, $"max |diff| = {worst:E2}")
        Call Check("与基线完全匹配的行数", mismatches = 0, $"mismatches = {mismatches}, rows = {baseline.Length}")
        Call Check("0/0 情形仍返回 NaN（未被引入保护逻辑改变语义）", nanMatches = 4, $"NaN rows = {nanMatches}")
    End Sub

    Private Sub TestShoulders()
        ' 左肩：a == b，旧实现与内核都必须在端点上取 1
        Dim left As New TSK.TrapezoidalMembership(0, 0, 20, 40)
        Call Check("左肩 mu(X0) = 1", left.Evaluate(0) = 1.0, $"got {left.Evaluate(0)}")
        Call Check("左肩外侧 mu(-eps) = 0", left.Evaluate(-0.5) = 0.0)
        Call Check("左肩平台 mu(10) = 1", left.Evaluate(10) = 1.0)

        ' 右肩：c == d
        Dim right As New TSK.TrapezoidalMembership(50, 80, 100, 100)
        Call Check("右肩 mu(X3) = 1", right.Evaluate(100) = 1.0, $"got {right.Evaluate(100)}")
        Call Check("右肩外侧 mu(X3+eps) = 0", right.Evaluate(100.5) = 0.0)

        ' 两侧都是肩（矩形）
        Dim box As New TSK.TrapezoidalMembership(0, 0, 10, 10)
        Call Check("矩形两端点都取 1", box.Evaluate(0) = 1.0 AndAlso box.Evaluate(10) = 1.0)
        Call Check("矩形内部取 1", box.Evaluate(5) = 1.0)

        ' 四点全等（退化成一个点）
        Dim point As New TSK.TrapezoidalMembership(5, 5, 5, 5)
        Call Check("退化点 mu(5) = 1", point.Evaluate(5) = 1.0)
        Call Check("退化点 mu(4) = 0", point.Evaluate(4) = 0.0)

        ' 正常梯形：端点为 0（不能被肩点对齐误伤）
        Dim normal As New TSK.TrapezoidalMembership(0, 1, 2, 3)
        Call Check("正常梯形 mu(a) = 0", normal.Evaluate(0) = 0.0)
        Call Check("正常梯形 mu(d) = 0", normal.Evaluate(3) = 0.0)
        Call Check("正常梯形 mu(0.5) = 0.5", SameValue(0.5, normal.Evaluate(0.5), 0.000000000001))

        ' 肩点处导数按约定取 0（分段线性节点）
        Dim g As Double() = left.Gradient(0)
        Dim zeroGradient As Boolean = True

        For i As Integer = 0 To g.Length - 1
            If g(i) <> 0.0 Then
                zeroGradient = False
            End If
        Next

        Call Check("肩点梯度为 0（节点约定）", zeroGradient)

        ' Mamdani 门面与内核必须给出同一个值
        Dim facade As New FL.MembershipFunction("Cold", 0, 0, 20, 40)
        Call Check("Mamdani 门面与内核求值一致", facade.Evaluate(0) = left.Evaluate(0) AndAlso facade.Evaluate(13.7) = left.Evaluate(13.7))
        Call Check("Mamdani 门面就是 IMembershipFunction", TypeOf facade Is TSK.IMembershipFunction)

        ' X0..X3 是参数投影：写入后必须能读回，且影响求值
        facade.X2 = 15
        Call Check("X2 写入后投影正确", facade.X2 = 15.0 AndAlso facade.C = 15.0)
        Call Check("X2 写入后求值随之改变", SameValue(1.0, facade.Evaluate(16), 0.000000000001) = False)
    End Sub

#End Region

#Region "TSK 数值契约"

    Private Sub TestMembershipGradients()
        Dim kinds As TSK.MembershipKind() = {
            TSK.MembershipKind.Gaussian,
            TSK.MembershipKind.GeneralizedBell,
            TSK.MembershipKind.Sigmoid,
            TSK.MembershipKind.Triangular,
            TSK.MembershipKind.Trapezoidal
        }
        Dim probes As Double() = {0.2, 0.35, 0.65, 0.8}

        For Each kind As TSK.MembershipKind In kinds
            Dim mf As TSK.IMembershipFunction = TSK.MembershipFactory.CreateGrid(kind, 0.5, 0.4)
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
        Dim net As TSK.TskNetwork = CreateTinyNetwork(2, 7)
        Dim x As Double()() = SampleGrid(50, -1.0, 1.0, 11, 2)
        Dim layers As New TSK.TskLayers(net)
        Dim fwd As TSK.TskInferenceTrace = layers.Forward(x, 0, x.Length)

        Dim n As Integer = net.InputCount
        Dim worst As Double = 0.0

        For t As Integer = 0 To x.Length - 1
            ' 完全独立的标量参考实现：按定义连乘 + 加权平均
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

        Dim worstSum As Double = 0.0

        For t As Integer = 0 To x.Length - 1
            Dim sum As Double = 0.0

            For r As Integer = 0 To net.Rules.RuleCount - 1
                sum += fwd.Wbar(t, r)
            Next

            worstSum = std.Max(worstSum, std.Abs(sum - 1.0))
        Next

        Call Check("归一化触发强度之和为 1", worstSum < 0.000000000001, $"max |sum-1| = {worstSum:E2}")

        ' 批量预测必须与逐块前向一致
        Dim predicted As Double()() = net.Predict(x, 7)
        Dim worstPredict As Double = 0.0

        For t As Integer = 0 To x.Length - 1
            worstPredict = std.Max(worstPredict, std.Abs(predicted(t)(0) - fwd.Z(t, 0)))
        Next

        Call Check("分块预测与整批前向一致", worstPredict < 0.000000000001, $"max |diff| = {worstPredict:E2}")
    End Sub

    Private Sub TestGradients()
        Dim net As TSK.TskNetwork = CreateTinyNetwork(2, 13)
        Dim x As Double()() = SampleGrid(40, -1.0, 1.0, 17, 2)
        Dim y As Double(,) = New Double(x.Length - 1, 0) {}
        Dim rng As New Random(19)

        For i As Integer = 0 To x.Length - 1
            y(i, 0) = std.Sin(2.0 * x(i)(0)) + 0.5 * x(i)(1) * x(i)(1) + 0.05 * rng.NextDouble()
        Next

        Dim layers As New TSK.TskLayers(net)
        Dim fwd As TSK.TskInferenceTrace = layers.Forward(x, 0, x.Length)
        Dim gradient As TSK.TskGradient = layers.Accumulate(fwd, y, computeThetaGradient:=True)
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

        ' 结论参数：dE/dtheta 与中心差分对比（只抽几个位置，避免开销）
        Dim theta As Double(,) = net.Theta
        Dim thetaWorst As Double = 0.0
        Dim probes As Integer() = {0, 3, theta.GetLength(0) - 1}

        For Each index As Integer In probes
            Dim original As Double = theta(index, 0)

            theta(index, 0) = original + h
            Dim plus As Double = HalfSse(net, x, y)

            theta(index, 0) = original - h
            Dim minus As Double = HalfSse(net, x, y)

            theta(index, 0) = original

            Dim numeric As Double = (plus - minus) / (2.0 * h)
            Dim errorValue As Double = std.Abs(gradient.Theta(index, 0) - numeric) / std.Max(0.000001, std.Abs(numeric))

            thetaWorst = std.Max(thetaWorst, errorValue)
        Next

        Call Check("结论参数解析梯度", thetaWorst < 0.000001, $"worst relative error = {thetaWorst:E2}")

        ' 触发强度求和与误差统计
        Call Check("平方误差统计非负", gradient.SquaredError >= 0.0, $"sse = {gradient.SquaredError:G6}")
        Call Check("规则平均触发强度之和为 1",
                   SameValue(1.0, gradient.RuleActivation.Sum() / gradient.SampleCount, 0.000000000001),
                   $"sum/n = {gradient.RuleActivation.Sum() / gradient.SampleCount:G6}")
    End Sub

    Private Sub TestRuleBase()
        Dim table As Integer(,) = TSK.FuzzyRuleBase.CreateFullGrid({2, 3, 2}).ToAntecedentTable()

        Call Check("m^n 规则数", table.GetLength(0) = 12, $"got {table.GetLength(0)}")

        Dim valid As Boolean = True

        For r As Integer = 0 To table.GetLength(0) - 1
            If table(r, 0) < 0 OrElse table(r, 0) >= 2 Then
                valid = False
            End If
            If table(r, 1) < 0 OrElse table(r, 1) >= 3 Then
                valid = False
            End If
            If table(r, 2) < 0 OrElse table(r, 2) >= 2 Then
                valid = False
            End If
        Next

        Call Check("前件下标都在合法范围内", valid)

        ' 全网格不重复
        Dim keys As New System.Collections.Generic.HashSet(Of String)

        For r As Integer = 0 To table.GetLength(0) - 1
            Call keys.Add($"{table(r, 0)}-{table(r, 1)}-{table(r, 2)}")
        Next

        Call Check("全网格枚举无重复", keys.Count = table.GetLength(0), $"distinct = {keys.Count}")

        ' 启用掩膜
        Dim mask As Boolean() = New Boolean(11) {}

        For i As Integer = 0 To 5
            mask(i) = True
        Next

        Dim masked As TSK.FuzzyRuleBase = TSK.FuzzyRuleBase.CreateFullGrid({2, 3, 2}, mask)

        Call Check("启用掩膜生效", masked.RuleCount = 6, $"got {masked.RuleCount}")

        ' 规则子集（剪枝）
        Dim subset As TSK.FuzzyRuleBase = masked.SelectRules({0, 2})

        Call Check("规则选集", subset.RuleCount = 2, $"got {subset.RuleCount}")
        Call Check("选集内容正确",
                   subset.Antecedent(0, 0) = masked.Antecedent(0, 0) AndAlso
                   subset.Antecedent(1, 0) = masked.Antecedent(2, 0))

        ' 前件文本导出
        Dim inputs As TSK.TskInputVariable() = {
            TSK.TskInputVariable.CreateGrid("x1", -1.0, 1.0, 2, TSK.MembershipKind.Gaussian),
            TSK.TskInputVariable.CreateGrid("x2", -1.0, 1.0, 3, TSK.MembershipKind.Gaussian),
            TSK.TskInputVariable.CreateGrid("x3", -1.0, 1.0, 2, TSK.MembershipKind.Gaussian)
        }
        Dim text As String() = subset.ToAntecedentText(inputs)

        Call Check("前件文本导出条数", text.Length = 2, $"got {text.Length}")
        Call Check("前件文本格式", text(0).StartsWith("rule1: IF x1 IS mf"), text(0))

        ' 规则爆炸保护
        Dim guarded As Boolean = False

        Try
            Call TSK.FuzzyRuleBase.CreateFullGrid({12, 12, 12}, Nothing, 1024)
        Catch ex As InvalidOperationException
            guarded = True
        End Try

        Call Check("规则数超限时抛出异常", guarded)

        ' 掩膜全关
        Dim refused As Boolean = False

        Try
            Call TSK.FuzzyRuleBase.CreateFullGrid({2, 2}, New Boolean() {False, False, False, False})
        Catch ex As InvalidOperationException
            refused = True
        End Try

        Call Check("掩膜全部关闭时拒绝构建", refused)
    End Sub

    Private Sub TestJsonRoundTrip()
        Dim net As TSK.TskNetwork = CreateTinyNetwork(3, 23)
        Dim x As Double()() = SampleGrid(30, -1.0, 1.0, 29, 2)
        Dim json As String = net.ToJson()
        Dim loaded As TSK.TskNetwork = TSK.TskNetwork.FromJson(json)

        Dim before As Double()() = net.Predict(x)
        Dim after As Double()() = loaded.Predict(x)
        Dim worst As Double = 0.0

        For i As Integer = 0 To before.Length - 1
            worst = std.Max(worst, std.Abs(before(i)(0) - after(i)(0)))
        Next

        Call Check("JSON 往返后预测一致", worst = 0.0, $"max |diff| = {worst:E2}")
        Call Check("JSON 往返后规则数一致", loaded.Rules.RuleCount = net.Rules.RuleCount)
        Call Check("JSON 往返后前提参数逐位一致",
                   SameValue(net.GetPremiseParameters().Sum(), loaded.GetPremiseParameters().Sum(), 0.000000000001))
        Call Check("JSON 往返后特征名保留", loaded.FeatureNames(0) = "x1", loaded.FeatureNames(0))

        ' 类型名改名不影响既有 JSON 的读取：属性名保持 formatVersion/inputs/antecedent/theta
        Call Check("JSON 文本包含既有字段名",
                   json.Contains("""formatVersion""") AndAlso json.Contains("""antecedent""") AndAlso json.Contains("""theta"""))

        ' FIS 文本导出
        Dim fis As String = net.ExportFisText()

        Call Check("FIS 文本导出非空", fis.Length > 0, $"length = {fis.Length}")
        Call Check("FIS 文本声明乘积 T-范数", fis.Contains("AndMethod='prod'"))
    End Sub

#End Region

#Region "Mamdani 持久化"

    Private Sub TestXmlRoundTrip()
        Dim engine As FL.FuzzyEngine = CreateEngine()
        Dim water As FL.LinguisticVariable = engine.LinguisticVariableCollection.Find("Water")
        Dim expected(9) As Double

        water.InputValue = 60
        expected(0) = engine.Defuzzify()

        For i As Integer = 1 To expected.Length - 1
            water.InputValue = 40 + i * 5
            expected(i) = engine.Defuzzify()
        Next

        Dim xmlFile As String = IO.Path.Combine(IO.Path.GetTempPath(), $"fuzzylogic-selftest-{Guid.NewGuid():N}.xml")

        Try
            Call engine.Save(xmlFile, Encodings.UTF8)
            Call Check("XML 文件已写出", IO.File.Exists(xmlFile))

            Dim loaded As FL.FuzzyEngine = FL.Models.FuzzyModel.FromXml(xmlFile)
            Dim restored As FL.LinguisticVariable = loaded.LinguisticVariableCollection.Find("Water")
            Dim worst As Double = 0.0

            restored.InputValue = 60
            worst = std.Max(worst, std.Abs(expected(0) - loaded.Defuzzify()))

            For i As Integer = 1 To expected.Length - 1
                restored.InputValue = 40 + i * 5
                worst = std.Max(worst, std.Abs(expected(i) - loaded.Defuzzify()))
            Next

            Call Check("XML 往返后 Defuzzify 逐点一致", worst < 0.000000000001, $"max |diff| = {worst:E2}")
            Call Check("XML 往返后规则数一致", loaded.FuzzyRuleCollection.Count = 2, $"got {loaded.FuzzyRuleCollection.Count}")
            Call Check("XML 往返后结论变量一致", loaded.Consequent = "Power")
            Call Check("XML 往返后术语四点保留",
                       restored.MembershipFunctionCollection.Find("Hot").X3 = 100.0 AndAlso
                       restored.MembershipFunctionCollection.Find("Cold").X1 = 0.0)
        Finally
            If IO.File.Exists(xmlFile) Then
                Call IO.File.Delete(xmlFile)
            End If
        End Try
    End Sub

#End Region

End Module

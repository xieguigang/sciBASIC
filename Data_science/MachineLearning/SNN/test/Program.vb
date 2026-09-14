' ============================================================================
' Program.vb — SNN 演示与学习程序
'
' Part 0  BPTT 梯度自检    —— 数值差分 vs 解析梯度，验证反向传播实现正确性
' Part 1  替代梯度监督学习 —— 双层 LIF 网络分类 3 类高斯团簇（Rate Coding）
' Part 2  STDP 无监督学习  —— 赢者通吃 + 脉冲时序可塑性，神经元自发分化
' ============================================================================

Imports System.Text
Imports Microsoft.VisualBasic.DeepLearning.SpikingNeuralNetwork.SpikingNN
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

Module Program

    Sub Main()
        Console.WriteLine("================================================================")
        Console.WriteLine("  Spiking Neural Network  ·  LIF + SurrogateGrad + STDP")
        Console.WriteLine("  基于 Tensor 对象的第三代神经网络实现（VB.NET / BCL）")
        Console.WriteLine("================================================================")
        Console.WriteLine()

        GradientSelfCheck()
        Console.WriteLine()
        SupervisedDemo()
        Console.WriteLine()
        StdpDemo()

        Console.WriteLine()
        Console.WriteLine("Done.")
    End Sub

    ' =========================================================================
    ' Part 0：BPTT 梯度自检
    ' =========================================================================

    ''' <summary>
    ''' 把 Heaviside 阶跃替换为其光滑替代原函数（f' ≡ 替代导数）后，
    ''' BPTT 解析梯度应与中心差分数值梯度一致 → 验证反向传播链正确。
    ''' 时延编码是确定性的，保证数值差分不被编码噪声污染。
    ''' </summary>
    Private Sub GradientSelfCheck()
        Console.WriteLine("--- Part 0 · BPTT 梯度自检（数值差分 vs 解析梯度）---")
        Console.WriteLine("  方法：前向用光滑原函数替代阶跃，比较解析梯度与中心差分梯度。")
        Console.WriteLine()

        Dim net As New SpikingNetwork(8, 16, SpikeEncoding.LatencyCoding)
        net.AddLayer(8, beta:=0.9, seed:=101)
        net.AddLayer(3, beta:=0.9, seed:=102)
        For Each l In net.Layers : l.SmoothForward = True : Next

        Dim rng As New Random(7)
        Dim xf(4 * 8 - 1) As Double
        For i = 0 To xf.Length - 1
            xf(i) = 0.1 + 0.8 * rng.NextDouble()
        Next
        Dim x = New Tensor(xf, 4, 8)
        Dim y() As Integer = {0, 1, 2, 0}

        Dim eps = 0.00001
        Dim maxRel = 0.0
        Dim nChecked = 0

        For Each layer In net.Layers
            Dim w = layer.Weight
            Dim cols = w.Shape(1)
            For k = 1 To 8
                Dim fi = rng.Next(w.Length)
                Dim orig = w(fi)

                w(fi) = orig + eps
                Dim lp = net.ComputeGradients(x, y)
                w(fi) = orig - eps
                Dim lm = net.ComputeGradients(x, y)
                w(fi) = orig

                Dim gAna = layer.WeightGrad(fi)
                Dim gNum = (lp - lm) / (2.0 * eps)
                Dim denom = std.Max(std.Abs(gAna), std.Abs(gNum))
                Dim rel = If(denom < 0.000000001, 0.0, std.Abs(gAna - gNum) / denom)

                Console.WriteLine(
                    $"    {layer.Name}.W({fi \ cols},{fi Mod cols}): " &
                    $"解析={gAna,12:E4}  数值={gNum,12:E4}  相对误差={rel:E2}")

                maxRel = std.Max(maxRel, rel)
                nChecked += 1
            Next
        Next

        Console.WriteLine()
        If maxRel < 0.001 Then
            Console.WriteLine($"  [PASS] 梯度自检通过：{nChecked} 个抽样分量最大相对误差 = {maxRel:E2}（阈值 1e-3）")
        Else
            Console.WriteLine($"  [FAIL] 梯度自检失败：最大相对误差 = {maxRel:E2}")
        End If
    End Sub

    ' =========================================================================
    ' Part 1：替代梯度监督学习
    ' =========================================================================

    ''' <summary>3 个类别在 8 维空间中的中心（各维取值 [0,1]）</summary>
    Private ReadOnly ClassCenters As Double()() = {
        New Double() {0.2, 0.2, 0.75, 0.75, 0.5, 0.5, 0.2, 0.8},
        New Double() {0.75, 0.75, 0.2, 0.2, 0.8, 0.2, 0.5, 0.5},
        New Double() {0.5, 0.8, 0.5, 0.8, 0.2, 0.75, 0.8, 0.2}
    }

    Private Function Gaussian(rng As Random) As Double
        ' Box-Muller 变换
        Dim u1 = 1.0 - rng.NextDouble()
        Dim u2 = 1.0 - rng.NextDouble()
        Return std.Sqrt(-2.0 * std.Log(u1)) * std.Sin(2.0 * std.PI * u2)
    End Function

    Private Function SampleClass(rng As Random, cls As Integer) As Double()
        Dim f = ClassCenters(0).Length
        Dim v(f - 1) As Double
        For i = 0 To f - 1
            v(i) = std.Max(0.02, std.Min(0.98, ClassCenters(cls)(i) + 0.09 * Gaussian(rng)))
        Next
        Return v
    End Function

    Private Function MakeData(rng As Random, perClass As Integer) As (xs As List(Of Double()), ys As Integer())
        Dim xs As New List(Of Double())()
        Dim ys As New List(Of Integer)()
        For c = 0 To ClassCenters.Length - 1
            For n = 1 To perClass
                xs.Add(SampleClass(rng, c))
                ys.Add(c)
            Next
        Next
        Return (xs, ys.ToArray())
    End Function

    Private Function BatchTensor(data As List(Of Double()), idx As Integer(), offset As Integer, count As Integer) As Tensor
        Dim f = data(0).Length
        Dim flat(f * count - 1) As Double
        For b = 0 To count - 1
            Array.Copy(data(idx(offset + b)), 0, flat, b * f, f)
        Next
        Return New Tensor(flat, count, f)
    End Function

    Private Function AllTensor(d As (xs As List(Of Double()), ys As Integer())) As Tensor
        Dim n = d.ys.Length
        Dim idx(n - 1) As Integer
        For i = 0 To n - 1 : idx(i) = i : Next
        Return BatchTensor(d.xs, idx, 0, n)
    End Function

    Private Function Shuffle(n As Integer, rng As Random) As Integer()
        Dim idx(n - 1) As Integer
        For i = 0 To n - 1 : idx(i) = i : Next
        For i = n - 1 To 1 Step -1
            Dim j = rng.Next(i + 1)
            Dim tmp = idx(i) : idx(i) = idx(j) : idx(j) = tmp
        Next
        Return idx
    End Function

    Private Sub SupervisedDemo()
        Console.WriteLine("--- Part 1 · 替代梯度监督学习（3 类高斯团簇分类）---")

        Dim rng As New Random(2024)
        Dim train = MakeData(rng, 120)      ' 360 样本
        Dim test = MakeData(rng, 30)        ' 90 样本
        Console.WriteLine($"  数据: 3 类 × 8 维高斯团簇（σ=0.09）, 训练 {train.ys.Length}, 测试 {test.ys.Length}")

        ' ---- 展示一个样本的脉冲编码 ----
        Dim demoX = New Tensor(train.xs(0), 1, 8)
        Dim demoSeq = SpikeEncoders.RateEncode(demoX, 30, New Random(99))
        Console.WriteLine()
        Console.WriteLine("  样本 #0 的频率编码脉冲栅格（行=输入神经元, 列=时间步, █=脉冲）:")
        PrintRaster(demoSeq, 8)

        ' ---- 网络构建 ----
        Dim net As New SpikingNetwork(8, 30, SpikeEncoding.RateCoding)
        net.Rng = New Random(31415)
        net.AddLayer(64, beta:=0.85, seed:=11)
        net.AddLayer(3, beta:=0.85, seed:=22)
        Dim opt As New AdamOptimizer(0.002, 1.0)

        Console.WriteLine()
        Console.WriteLine("  网络结构: 8 → [LIF×64 β=0.85 θ=1.0] → [LIF×3 β=0.85 θ=1.0], T=30")
        Console.WriteLine("  训练配置: Adam(lr=0.002) + 梯度裁剪(1.0) + FastSigmoid 替代梯度(α=2)")
        Console.WriteLine()

        ' ---- 训练 ----
        Dim batchSize = 36
        Dim epochs = 40
        Console.WriteLine("  epoch |   loss   | test-acc")
        Dim testX = AllTensor(test)

        For epoch = 1 To epochs
            Dim order = Shuffle(train.ys.Length, rng)
            Dim lossSum = 0.0
            Dim nb = 0

            For start = 0 To order.Length - 1 Step batchSize
                Dim cnt = std.Min(batchSize, order.Length - start)
                Dim bx = BatchTensor(train.xs, order, start, cnt)
                Dim by(cnt - 1) As Integer
                For b = 0 To cnt - 1
                    by(b) = train.ys(order(start + b))
                Next
                lossSum += net.TrainStep(bx, by, opt)
                nb += 1
            Next

            If epoch = 1 OrElse epoch Mod 5 = 0 Then
                Dim acc = SpikingNetwork.Accuracy(net.Predict(testX), test.ys)
                Console.WriteLine($"   {epoch,4} | {lossSum / nb,8:F4} | {acc,8:P1}")
            End If
        Next

        ' ---- 测试评估 ----
        Dim pred = net.Predict(testX)
        Dim hit = 0
        For i = 0 To pred.Length - 1
            If pred(i) = test.ys(i) Then hit += 1
        Next
        Console.WriteLine()
        Console.WriteLine($"  [结果] 测试集准确率: {hit / CDbl(pred.Length):P1} ({hit}/{pred.Length})")

        ' ---- 单样本推理轨迹可视化 ----
        Console.WriteLine()
        Console.WriteLine("  —— 单样本推理轨迹（测试样本 #0）——")
        VizInference(net, test, 0)
    End Sub

    Private Sub VizInference(net As SpikingNetwork, test As (xs As List(Of Double()), ys As Integer()), sample As Integer)
        Dim x0 = New Tensor(test.xs(sample), 1, 8)
        Dim label = test.ys(sample)

        ' 用独立随机源复现一次前向（与训练一致的 Rate 编码）
        Dim seq = SpikeEncoders.RateEncode(x0, net.TimeSteps, New Random(777))
        For Each l In net.Layers : l.ResetState(1) : Next

        Dim outS As New List(Of Tensor)()
        For t = 0 To net.TimeSteps - 1
            Dim sig = seq(t)
            For Each l In net.Layers
                sig = l.ForwardStep(sig)
            Next
            outS.Add(sig)
        Next

        Console.WriteLine("  输入脉冲栅格:")
        PrintRaster(seq, 8)

        Console.WriteLine("  隐藏层前 4 个神经元的膜电位轨迹（▁▂▃▄▅▆▇█ 相对幅度）:")
        Dim h1 = net.Layers(0)
        For n = 0 To 3
            Dim u(net.TimeSteps - 1) As Double
            For tt = 0 To net.TimeSteps - 1
                u(tt) = h1.UHistory(tt).Data(n)
            Next
            Console.WriteLine($"    h{n}: {Sparkline(u)}")
        Next

        Console.WriteLine("  输出层脉冲栅格与计数（计数解码）:")
        Dim counts(2) As Integer
        For j = 0 To 2
            Dim line = ""
            For t = 0 To outS.Count - 1
                Dim v = outS(t).Data(j)
                If v > 0 Then counts(j) += 1
                line &= If(v > 0, "█", "·")
            Next
            Dim mark = If(j = label, " ← 真实类别", "")
            Console.WriteLine($"    out{j} |{line}| 计数={counts(j)}{mark}")
        Next

        Dim best = 0
        For j = 1 To 2
            If counts(j) > counts(best) Then best = j
        Next
        Console.WriteLine($"    → 预测类别: {best}（真实: {label}） {If(best = label, "[正确]", "[错误]")}")
    End Sub

    ' =========================================================================
    ' Part 2：STDP 无监督学习
    ' =========================================================================

    Private Sub StdpDemo()
        Console.WriteLine("--- Part 2 · STDP 无监督学习（模式分化 / 赢者通吃）---")
        Console.WriteLine("  16 个输入分两组：模式A激活前 8 个，模式B激活后 8 个。")
        Console.WriteLine("  输出层 8 个 LIF 神经元未经任何标签，通过 STDP+WTA 自发分化模式偏好。")
        Console.WriteLine()

        ' ---- 理论 STDP 学习窗口 ----
        Console.WriteLine("  理论 STDP 学习窗口（Δt = t_post − t_pre, τ=20）:")
        Dim tau = 20.0
        For dt = -5 To 5
            Dim dw = If(dt > 0, std.Exp(-dt / tau), -1.1 * std.Exp(dt / tau))
            Dim w = CInt(std.Min(24, std.Abs(dw) * 22))
            Dim bar = New String(If(dw >= 0, "+"c, "-"c), w)
            Console.WriteLine($"    Δt={dt,3}: {bar}")
        Next
        Console.WriteLine()

        Dim rng As New Random(555)
        Dim stdp As New STDPLayer(16, 8, seed:=5)
        ' 归一化目标 = 活跃输入数(8) × 期望权重(0.5)：守恒竞争，特化的关键机制
        stdp.NormalizeSum = 3.0
        stdp.APlus = 0.006
        stdp.AMinus = 0.002
        stdp.AdaptRate = 0.002

        PrintHeatmap(stdp.Weight, 16, 8, "  学习前突触权重（行=输入, 列=输出, 字符越深权重越大）:")

        ' ---- STDP 训练：120 次模式呈现，每次 12 个时间步，呈现后归一化 ----
        For present = 1 To 150
            Dim mode = rng.Next(2)
            stdp.Reset()
            For t = 1 To 12
                stdp.Step(StdpPatternInput(rng, mode))
            Next
            stdp.NormalizeWeights()
        Next

        PrintHeatmap(stdp.Weight, 16, 8, "  学习后突触权重:")

        ' ---- 响应统计 ----
        Dim respA = StdpResponse(stdp, rng, 0)
        Dim respB = StdpResponse(stdp, rng, 1)
        Console.WriteLine("  学习后各输出神经元的发放统计（各 20 次模式呈现）:")
        Console.WriteLine("  neuron |  模式A  |  模式B  | 偏好")
        For j = 0 To 7
            Dim pref = If(respA(j) > respB(j) * 2, "A", If(respB(j) > respA(j) * 2, "B", "-"))
            Console.WriteLine($"     {j}   |  {respA(j),4}   |  {respB(j),4}   |  {pref}")
        Next
        Console.WriteLine()
        Console.WriteLine("  观察要点：权重矩阵呈现清晰的列分组（某些列只对前 8 行着色），")
        Console.WriteLine("  对应输出神经元分化为模式A/B 的'专家'——无监督自组织的涌现。")
    End Sub

    ''' <summary>生成模式脉冲输入：模式A → 前 8 输入高发放率；模式B → 后 8 输入</summary>
    Private Function StdpPatternInput(rng As Random, mode As Integer) As Tensor
        Dim x = New Tensor(1, 16)
        For i = 0 To 15
            Dim active = If(mode = 0, i < 8, i >= 8)
            If rng.NextDouble() < If(active, 0.85, 0.03) Then
                x.Data(i) = 1.0
            End If
        Next
        Return x
    End Function

    Private Function StdpResponse(stdp As STDPLayer, rng As Random, mode As Integer) As Integer()
        Dim counts(7) As Integer
        For trial = 1 To 20
            stdp.Reset()
            For t = 1 To 12
                Dim s = stdp.Step(StdpPatternInput(rng, mode))
                For j = 0 To 7
                    If s.Data(j) > 0 Then counts(j) += 1
                Next
            Next
        Next
        Return counts
    End Function

    ' =========================================================================
    ' 可视化辅助
    ' =========================================================================

    Private Sub PrintRaster(seq As List(Of Tensor), rows As Integer)
        For r = 0 To rows - 1
            Dim line = ""
            For t = 0 To seq.Count - 1
                line &= If(seq(t).Data(r) > 0, "█", "·")
            Next
            Console.WriteLine($"    in{r,-2}|{line}|")
        Next
    End Sub

    Private Function Sparkline(v As Double()) As String
        Const blocks = "▁▂▃▄▅▆▇█"
        Dim mx = v.Max()
        Dim mn = v.Min()
        Dim span = std.Max(mx - mn, 0.0000001)
        Dim sb As New StringBuilder()
        For Each x In v
            Dim k = CInt(std.Floor((x - mn) / span * 7.999))
            If k < 0 Then k = 0
            If k > 7 Then k = 7
            sb.Append(blocks(k))
        Next
        Return sb.ToString()
    End Function

    Private Sub PrintHeatmap(w As Tensor, inN As Integer, outN As Integer, title As String)
        Const ramp = " .:-=+*#%@"
        Console.WriteLine(title)
        Console.Write("        ")
        For j = 0 To outN - 1
            Console.Write($"{j,-4}")
        Next
        Console.WriteLine()
        For i = 0 To inN - 1
            Console.Write($"   i{i,-3}")
            For j = 0 To outN - 1
                Dim v = std.Max(0.0, std.Min(1.0, w(i, j)))
                Console.Write($"{ramp(CInt(std.Floor(v * 9.999)))}   ")
            Next
            Console.WriteLine()
        Next
        Console.WriteLine()
    End Sub

End Module

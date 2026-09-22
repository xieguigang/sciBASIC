#Region "Microsoft.VisualBasic::081e9e173616d5bffc659aff10c49f96, Data_science\MachineLearning\SNN\test\test1.vb"

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

    '   Total Lines: 203
    '    Code Lines: 166 (81.77%)
    ' Comment Lines: 13 (6.40%)
    '    - Xml Docs: 7.69%
    ' 
    '   Blank Lines: 24 (11.82%)
    '     File Size: 8.09 KB


    ' Module test1
    ' 
    '     Function: MakeData, SampleClass, Sparkline
    ' 
    '     Sub: PrintRaster, SupervisedDemo, VizInference
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.DeepLearning.SpikingNeuralNetwork
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Module test1
    ' =========================================================================
    ' Part 1：替代梯度监督学习
    ' =========================================================================

    ''' <summary>3 个类别在 8 维空间中的中心（各维取值 [0,1]）</summary>
    Private ReadOnly ClassCenters As Double()() = {
        New Double() {0.2, 0.2, 0.75, 0.75, 0.5, 0.5, 0.2, 0.8},
        New Double() {0.75, 0.75, 0.2, 0.2, 0.8, 0.2, 0.5, 0.5},
        New Double() {0.5, 0.8, 0.5, 0.8, 0.2, 0.75, 0.8, 0.2}
    }

    Private Function SampleClass(rng As Random, cls As Integer) As Double()
        Dim f = ClassCenters(0).Length
        Dim v(f - 1) As Double
        For i = 0 To f - 1
            v(i) = std.Max(0.02, std.Min(0.98, ClassCenters(cls)(i) + 0.09 * randf.NextGaussian))
        Next
        Return v
    End Function

    Private Function MakeData(rng As Random, perClass As Integer) As NumericTable
        Dim xs As New List(Of Double())()
        Dim ys As New List(Of Integer)()
        For c As Integer = 0 To ClassCenters.Length - 1
            For n = 1 To perClass
                xs.Add(SampleClass(rng, c))
                ys.Add(c)
            Next
        Next
        Return New NumericTable(xs, ys.ToArray())
    End Function

    Public Sub SupervisedDemo()
        Console.WriteLine("  Spiking Neural Network  ·  LIF + SurrogateGrad + STDP")
        Console.WriteLine()
        Console.WriteLine("  --- 替代梯度监督学习（3 类高斯团簇分类）---")
        Console.WriteLine()

        Dim rng As New Random(2024)
        Dim train = MakeData(rng, 120)      ' 360 样本
        Dim test = MakeData(rng, 30)        ' 90 样本
        Console.WriteLine($"  数据: 3 类 × 8 维高斯团簇（σ=0.09）, 训练 {train.nsamples}, 测试 {test.nsamples}")

        ' ---- 展示一个样本的脉冲编码 ----
        Dim demoX = New Tensor(train.features(0), 1, 8)
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
            Dim order = Enumerable.Range(0, train.nsamples).Shuffle.ToArray
            Dim lossSum = 0.0
            Dim nb = 0
            Dim train_label = train.GetLabel(0)

            For start = 0 To order.Length - 1 Step batchSize
                Dim cnt = std.Min(batchSize, order.Length - start)
                Dim bx = BatchTensor(train.features, order, start, cnt)
                Dim by(cnt - 1) As Integer
                For b = 0 To cnt - 1
                    by(b) = train_label(order(start + b))
                Next
                lossSum += net.TrainStep(bx, by, opt)
                nb += 1
            Next

            If epoch = 1 OrElse epoch Mod 5 = 0 Then
                Dim acc = SpikingNetwork.Accuracy(net.Predict(testX), test.GetClassLabel(0))
                Console.WriteLine($"   {epoch,4} | {lossSum / nb,8:F4} | {acc,8:P1}")
            End If
        Next

        ' ---- 测试评估 ----
        Dim pred = net.Predict(testX)
        Dim hit = 0
        Dim test_labels = test.GetLabel(0)
        For i = 0 To pred.Length - 1
            If pred(i) = test_labels(i) Then
                hit += 1
            End If
        Next
        Console.WriteLine()
        Console.WriteLine($"  [结果] 测试集准确率: {hit / CDbl(pred.Length):P1} ({hit}/{pred.Length})")

        ' ---- 单样本推理轨迹可视化 ----
        Console.WriteLine()
        Console.WriteLine("  —— 单样本推理轨迹（测试样本 #0）——")
        VizInference(net, test, 0)
    End Sub

    Private Sub VizInference(net As SpikingNetwork, test As NumericTable, sample As Integer)
        Dim x0 = New Tensor(test.features(sample), 1, 8)
        Dim label = test.labels(sample)(0)

        ' 用独立随机源复现一次前向（与训练一致的 Rate 编码）
        Dim seq = SpikeEncoders.RateEncode(x0, net.TimeSteps, New Random(777))
        For Each l In net.Layers
            l.ResetState(1)
        Next

        Dim outS As New List(Of Tensor)()
        For t = 0 To net.TimeSteps - 1
            Dim sig = seq(t)
            For Each l In net.Layers
                sig = l.ForwardStep(sig)
            Next
            outS.Add(sig)
        Next

        Console.WriteLine()
        Console.WriteLine("  输入脉冲栅格:")
        PrintRaster(seq, 8)
        Console.WriteLine()
        Console.WriteLine("  隐藏层前 4 个神经元的膜电位轨迹（▁▂▃▄▅▆▇█ 相对幅度）:")
        Console.WriteLine()
        Dim h1 = net.Layers(0)
        For n = 0 To 3
            Dim u(net.TimeSteps - 1) As Double
            For tt = 0 To net.TimeSteps - 1
                u(tt) = h1.UHistory(tt).Data(n)
            Next
            Console.WriteLine($"    h{n}: {Sparkline(u)}")
            Console.WriteLine($"    " & New String("-"c, 36))
            Console.WriteLine()
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
End Module

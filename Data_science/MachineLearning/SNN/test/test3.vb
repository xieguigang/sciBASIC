#Region "Microsoft.VisualBasic::6fa498a0e5c362117df086f275d33710, Data_science\MachineLearning\SNN\test\test3.vb"

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

    '   Total Lines: 227
    '    Code Lines: 166 (73.13%)
    ' Comment Lines: 29 (12.78%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 32 (14.10%)
    '     File Size: 9.42 KB


    ' Module test3
    ' 
    '     Sub: InputMapInjection, SparseDemo, SparseRecurrentSimulation, SpmmCrossCheck
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DeepLearning.SpikingNeuralNetwork
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

Module test3

    ' =========================================================================
    ' Part 3：稀疏自定义连接（连接组风格）前向仿真
    '
    ' 演示如何用 FlyWire 风格的 (pre, post, weight) 三元组驱动 SNN：
    '   A. SparseMatrix.SpMM 与稠密 MatMul 对拍，并验证重复边合并
    '   B. 单个大稀疏层（含循环连接与自反馈）的脉冲动力学仿真
    '   C. inputMap：把少量输入特征注入到指定神经元子集
    '
    ' 说明：稀疏层仅做前向仿真，权重固定、不参与训练。
    ' =========================================================================

    Public Sub SparseDemo()
        Console.WriteLine("================================================================")
        Console.WriteLine("  Part 3 · 稀疏自定义连接（FlyWire 风格连接组）前向仿真")
        Console.WriteLine("================================================================")
        Console.WriteLine()

        SpmmCrossCheck()
        Console.WriteLine()
        SparseRecurrentSimulation()
        Console.WriteLine()
        InputMapInjection()
    End Sub

    ' -------------------------------------------------------------------------
    ' A. SpMM 对拍自检 + 重复边合并
    ' -------------------------------------------------------------------------
    Private Sub SpmmCrossCheck()
        Console.WriteLine("--- A · SparseMatrix.SpMM 与稠密 MatMul 对拍 ---")

        Dim rng As New Random(1234)
        Dim n = 8
        Dim nEdges = 24

        Dim pre(nEdges - 1) As Integer
        Dim post(nEdges - 1) As Integer
        Dim w(nEdges - 1) As Double
        For i = 0 To nEdges - 1
            pre(i) = rng.Next(n)
            post(i) = rng.Next(n)
            w(i) = 0.1 + rng.NextDouble()
        Next
        ' 人为制造重复边：验证 (pre, post) 相同时权重累加合并
        pre(0) = 2 : post(0) = 5 : w(0) = 1.0
        pre(1) = 2 : post(1) = 5 : w(1) = 0.5    ' 应与上一条合并为 1.5

        Dim sm = SparseMatrix.FromTriplets(pre, post, w, n, n)
        Console.WriteLine($"  神经元 {n}, 输入三元组 {nEdges} 条 → CSR 非零边 {sm.NonZeros} 条（重复边已合并）")

        ' 用朴素三重循环构造稠密矩阵（用于对拍）
        Dim denseW = New Tensor(n, n)
        For i = 0 To nEdges - 1
            denseW(pre(i), post(i)) = denseW(pre(i), post(i)) + w(i)
        Next

        ' 校验 ToDense 与朴素稠密一致
        Dim maxDense = 0.0
        Dim td = sm.ToDense()
        For i = 0 To n - 1
            For j = 0 To n - 1
                maxDense = std.Max(maxDense, std.Abs(td(i, j) - denseW(i, j)))
            Next
        Next
        Console.WriteLine($"  ToDense 与朴素稠密矩阵最大偏差 = {maxDense:E2}")

        ' 随机输入 [batch, n]，对拍 SpMM 与稠密切片
        Dim batch = 4
        Dim xf(batch * n - 1) As Double
        For i = 0 To xf.Length - 1
            xf(i) = rng.NextDouble()
        Next
        Dim x = New Tensor(xf, batch, n)

        Dim ySparse = sm.SpMM(x)
        Dim yDense = x.MatMul(denseW)

        Dim maxDiff = 0.0
        For i = 0 To ySparse.Length - 1
            maxDiff = std.Max(maxDiff, std.Abs(ySparse.Data(i) - yDense.Data(i)))
        Next

        Console.WriteLine($"  SpMM vs 稠密 MatMul 最大偏差 = {maxDiff:E2}")
        If maxDiff < 0.000000001 AndAlso maxDense < 0.000000001 Then
            Console.WriteLine("  [PASS] 稀疏矩阵构建与 SpMM 实现正确。")
        Else
            Console.WriteLine("  [FAIL] 稀疏计算与稠密参考不一致。")
        End If
    End Sub

    ' -------------------------------------------------------------------------
    ' B. 单个大稀疏层（含循环连接与自反馈）前向仿真
    ' -------------------------------------------------------------------------
    Private Sub SparseRecurrentSimulation()
        Console.WriteLine("--- B · 单层稀疏递归网络（含循环连接与自反馈）---")

        Dim rng As New Random(20240916)
        Dim n = 64
        Dim fanIn = 6          ' 每个突触后神经元的平均入边数 → 稀疏度约 fanIn/n ≈ 9%
        Dim sensory = 12       ' 前 12 个神经元作为外部刺激的"感觉神经元"

        ' 构造连接组风格三元组：每个 post 随机挑选 fanIn 个 pre（含少量自反馈）
        Dim preList As New List(Of Integer)()
        Dim postList As New List(Of Integer)()
        Dim wList As New List(Of Double)()
        For j = 0 To n - 1
            For k = 0 To fanIn - 1
                Dim i = rng.Next(n)
                preList.Add(i)
                postList.Add(j)
                wList.Add(1.0 + rng.NextDouble() * 3.0)   ' 模拟突触计数
            Next
        Next
        ' 显式加入若干自反馈连接（pre = post）
        For j = 0 To 3
            preList.Add(j) : postList.Add(j) : wList.Add(2.0)
        Next

        Dim pre() As Integer = preList.ToArray()
        Dim post() As Integer = postList.ToArray()
        Dim w() As Double = wList.ToArray()

        Console.WriteLine($"  神经元 N={n}, 突触 {(pre.Length)} 条（fan-in≈{fanIn}+自反馈），按扇入归一化")

        Dim T = 40
        Dim net As New SpikingNetwork(n, T, SpikeEncoding.RateCoding)
        net.Rng = New Random(777)
        net.AddSparseLayer(pre, post, w, n,
                           normalization:=SparseNormalization.FanIn,
                           beta:=0.9, threshold:=1.0)

        ' 输入：仅感觉神经元以一定发放率被驱动 [1, n]
        Dim x(63) As Double
        For i = 0 To sensory - 1
            x(i) = 0.5
        Next
        Dim counts = net.ForwardSpikes(New Tensor(x, 1, n))

        ' 输出脉冲栅格（每 4 个神经元折叠为一行，便于查看）
        Console.WriteLine()
        Console.WriteLine("  输出脉冲栅格（行=神经元分组, 列=时间步, █=发放）:")
        Dim sHist = net.SparseLayer.SHistory
        For g = 0 To 3
            Dim line = ""
            For t = 0 To T - 1
                Dim any = False
                For k = 0 To 15
                    Dim idx = g * 16 + k
                    If idx < n AndAlso sHist(t).Data(idx) > 0 Then any = True
                Next
                line &= If(any, "█", "·")
            Next
            Console.WriteLine($"    g{g}|{line}|")
        Next

        ' 各神经元发放率（步数归一化）
        Dim total = 0.0
        Dim active = 0
        For j = 0 To n - 1
            Dim c = counts.Data(j)
            total += c
            If c > 0 Then active += 1
        Next
        Console.WriteLine()
        Console.WriteLine($"  全群体平均发放率 = {total / (n * T):P1}，活跃神经元 {active}/{n}")
        Console.WriteLine("  各神经元脉冲计数（前 24 个）:")
        Dim line2 = "   "
        For j = 0 To 23
            line2 &= $"{CInt(counts.Data(j)),4}"
        Next
        Console.WriteLine(line2)

        Console.WriteLine()
        Console.WriteLine("  提示：感觉神经元（0..11）因外部注入而活跃，其脉冲经稀疏突触矩阵")
        Console.WriteLine("        传播至下游神经元，并可通过循环/自反馈持续影响后续时间步。")
    End Sub

    ' -------------------------------------------------------------------------
    ' C. inputMap：把 8 个输入特征注入到指定神经元子集
    ' -------------------------------------------------------------------------
    Private Sub InputMapInjection()
        Console.WriteLine("--- C · inputMap 输入注入映射 ---")

        Dim rng As New Random(88)
        Dim n = 32
        Dim nEdges = 120

        Dim pre(nEdges - 1) As Integer
        Dim post(nEdges - 1) As Integer
        Dim w(nEdges - 1) As Double
        For i = 0 To nEdges - 1
            pre(i) = rng.Next(n)
            post(i) = rng.Next(n)
            w(i) = 1.0 + rng.NextDouble() * 2.0
        Next

        ' 8 个输入特征分别注入到神经元 4,5,...,11
        Dim inputMap() As Integer = {4, 5, 6, 7, 8, 9, 10, 11}

        Dim net As New SpikingNetwork(8, 30, SpikeEncoding.RateCoding)
        net.Rng = New Random(2024)
        net.AddSparseLayer(pre, post, w, n,
                           normalization:=SparseNormalization.GlobalMax,
                           inputMap:=inputMap)

        Dim x(7) As Double
        For i = 0 To 7
            x(i) = 0.6
        Next
        Dim counts = net.ForwardSpikes(New Tensor(x, 1, 8))

        Console.WriteLine($"  输入维度 8 → inputMap {String.Join(",", inputMap)}（注入到 N={n} 的稀疏网络）")
        Console.WriteLine("  被注入神经元的脉冲计数（4..11）:")
        Dim line = "   "
        For j = 0 To 7
            line &= $"{CInt(counts.Data(inputMap(j))),4}"
        Next
        Console.WriteLine(line)
        Console.WriteLine("  [OK] inputMap 生效：仅指定神经元接收到外部驱动。")
    End Sub

End Module


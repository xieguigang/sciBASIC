#Region "Microsoft.VisualBasic::e04f9e3f47d85579e4fcbccabaaecd3c, Data_science\MachineLearning\DeepLearning\_smoketest\Program.vb"

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

    '   Total Lines: 120
    '    Code Lines: 91 (75.83%)
    ' Comment Lines: 8 (6.67%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 21 (17.50%)
    '     File Size: 5.21 KB


    ' Module Program
    ' 
    '     Sub: AssertClose, AssertFinite, Main, TestNetwork, TestNetz
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork

Module Program

    Dim rnd As New Random

    Sub Main()
        Call TestNetz()
        Call TestNetwork()
        Console.WriteLine("SMOKE TEST DONE")
    End Sub

    Private Sub TestNetz()
        Console.WriteLine("=== Netz (online SGD, CNN kernel) ===")

        Dim net As New Netz(inputNeurons:=2, hiddenNeurons:=4, hiddenLayers:=1, outputNeurons:=1,
                              activate:=Function(d) 1.0 / (1.0 + System.Math.Exp(-d)))
        net.LERNRATE = 0.05

        For i As Integer = 1 To 8000
            Dim x0 = rnd.NextDouble()
            Dim x1 = rnd.NextDouble()
            Call net.train({x0, x1}, {x0 + x1})
        Next

        Dim p = net.predict({0.3, 0.5})
        Console.WriteLine($"  predict(0.3,0.5) = {p(0):F4}  (target 0.8)  TotalError={net.TotalError:F4}")
        Call AssertFinite("Netz.predict", p(0))
        Call AssertClose("Netz.predict", p(0), 0.8, 0.3)

        Dim path = "netz_model.cnn"
        Call net.Save(path)
        Dim net2 = Netz.Load(path)
        Dim p2 = net2.predict({0.3, 0.5})
        Console.WriteLine($"  loaded predict   = {p2(0):F4}")
        Call AssertFinite("Netz.Load.predict", p2(0))
        Call AssertClose("Netz.Load.predict", p2(0), p(0), 0.05)

        ' 镜像只读属性
        Console.WriteLine($"  HiddenLayerCount={net.HiddenLayerCount} Input={net.InputNeuronCount} Output={net.OutputNeuronCount} MaxIdx={net.MaxOutputNeuronIndex}")
    End Sub

    Private Sub TestNetwork()
        Console.WriteLine("=== Network (CNN kernel) ===")

        ' 在线逐样本训练（Network.ForwardPropagate + BackPropagate -> CNN 内核）
        ' 回归网络使用 ReLU 隐藏层 + 线性输出，朴素 SGD/AdaGrad 即可稳定拟合 a+b
        Dim net As New Network(inputSize:=2, hiddenSize:={4}, outputSize:=1, learnRate:=0.1, active:="relu")
        For i As Integer = 1 To 8000
            Dim a = rnd.NextDouble()
            Dim b = rnd.NextDouble()
            Call net.ForwardPropagate({a, b}, parallel:=False)
            Call net.BackPropagate({a + b}, parallel:=False)
        Next

        Dim err = net.Compute(0.3, 0.5)
        Console.WriteLine($"  online  predict = {err(0):F4}  (target 0.8)")
        Call AssertFinite("Network.online.Compute", err(0))
        Call AssertClose("Network.online.Compute", err(0), 0.8, 0.3)

        ' 验证网络确实学到了输入->输出的映射（而非退化为常数解）
        Dim y00 = net.Compute(0, 0)(0)
        Dim y11 = net.Compute(1, 1)(0)
        Console.WriteLine($"  DBG online (0,0)={y00:F4} (1,1)={y11:F4}")
        Call AssertClose("Network.online.(1,1)>(0,0)", y11, y00, -0.5)

        ' 批量训练（Network.TrainBatch 直接驱动 CNN 内核优化器）
        Dim network2 As New Network(inputSize:=2, hiddenSize:={4}, outputSize:=1, learnRate:=0.1, active:="relu")

        Dim batchSize As Integer = 200
        Dim batch(batchSize - 1) As (input As Double(), target As Double())
        For i As Integer = 0 To batchSize - 1
            Dim a = rnd.NextDouble()
            Dim b = rnd.NextDouble()
            batch(i) = ({a, b}, {a + b})
        Next
        Call network2.TrainBatch(batch, 200)

        Dim err2 = network2.Compute(0.3, 0.5)
        Console.WriteLine($"  batch   predict = {err2(0):F4}")
        Call AssertFinite("Network.batch.Compute", err2(0))
        Call AssertClose("Network.batch.Compute", err2(0), 0.8, 0.3)

        ' 保存/加载 CNN 二进制模型
        Dim npath = "network_model.cnn"
        Call network2.Save(npath)
        Dim network3 = Network.Load(npath)
        Dim err3 = network3.Compute(0.3, 0.5)
        Console.WriteLine($"  loaded  predict = {err3(0):F4}")
        Call AssertFinite("Network.Load.Compute", err3(0))
        Call AssertClose("Network.Load.Compute", err3(0), err2(0), 0.05)

        ' 只读视图属性
        Console.WriteLine($"  Input={network3.InputLayer.Count} Hidden={network3.HiddenLayer.Count} Output={network3.OutputLayer.Count}")
    End Sub

    Private Sub AssertClose(tag As String, actual As Double, expected As Double, tol As Double)
        If System.Math.Abs(actual - expected) > System.Math.Abs(tol) Then
            ' tol<0 时表示方向相反的不等式（actual 应比 expected 大约 |tol|）
            If tol < 0 Then
                If actual < expected + System.Math.Abs(tol) Then
                    Call Console.WriteLine($"  [FAIL] {tag}: {actual:F4} not greater than {expected:F4} by {System.Math.Abs(tol):F4}")
                    Environment.Exit(1)
                End If
            Else
                Call Console.WriteLine($"  [FAIL] {tag}: {actual:F4} != {expected:F4} (tol {tol:F4})")
                Environment.Exit(1)
            End If
        End If
    End Sub

    Private Sub AssertFinite(tag As String, v As Double)
        If Double.IsNaN(v) OrElse Double.IsInfinity(v) Then
            Call Console.WriteLine($"  [FAIL] {tag} produced non-finite value: {v}")
            Environment.Exit(1)
        End If
    End Sub

End Module

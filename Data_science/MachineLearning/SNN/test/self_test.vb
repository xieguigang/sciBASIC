#Region "Microsoft.VisualBasic::2cb5cb4d9a5b8cacd708969629163314, Data_science\MachineLearning\SNN\test\self_test.vb"

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

    '   Total Lines: 73
    '    Code Lines: 53 (72.60%)
    ' Comment Lines: 8 (10.96%)
    '    - Xml Docs: 62.50%
    ' 
    '   Blank Lines: 12 (16.44%)
    '     File Size: 2.92 KB


    ' Module self_test
    ' 
    '     Sub: GradientSelfCheck
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.DeepLearning.SpikingNeuralNetwork
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

Module self_test
    ' =========================================================================
    ' Part 0：BPTT 梯度自检
    ' =========================================================================

    ''' <summary>
    ''' 把 Heaviside 阶跃替换为其光滑替代原函数（f' ≡ 替代导数）后，
    ''' BPTT 解析梯度应与中心差分数值梯度一致 → 验证反向传播链正确。
    ''' 时延编码是确定性的，保证数值差分不被编码噪声污染。
    ''' </summary>
    Public Sub GradientSelfCheck()
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

End Module


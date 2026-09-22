#Region "Microsoft.VisualBasic::638a67d0a0fba771fe92cb6fe73dc919, Data_science\MachineLearning\SNN\test\test2.vb"

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

    '   Total Lines: 110
    '    Code Lines: 88 (80.00%)
    ' Comment Lines: 8 (7.27%)
    '    - Xml Docs: 12.50%
    ' 
    '   Blank Lines: 14 (12.73%)
    '     File Size: 4.56 KB


    ' Module test2
    ' 
    '     Function: StdpPatternInput, StdpResponse
    ' 
    '     Sub: PrintHeatmap, StdpDemo
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DeepLearning.SpikingNeuralNetwork
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

Module test2

    ' =========================================================================
    ' Part 2：STDP 无监督学习
    ' =========================================================================

    Public Sub StdpDemo()
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

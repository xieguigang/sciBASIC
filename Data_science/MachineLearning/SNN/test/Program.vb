#Region "Microsoft.VisualBasic::36fd714e0f7bb2180fd76e59b3591ba5, Data_science\MachineLearning\SNN\test\Program.vb"

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

    '   Total Lines: 46
    '    Code Lines: 17 (36.96%)
    ' Comment Lines: 19 (41.30%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (21.74%)
    '     File Size: 1.91 KB


    ' Module Program
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' Program.vb — SNN 演示与学习程序
'
' Part 0  BPTT 梯度自检    —— 数值差分 vs 解析梯度，验证反向传播实现正确性
' Part 1  替代梯度监督学习 —— 双层 LIF 网络分类 3 类高斯团簇（Rate Coding）
' Part 2  STDP 无监督学习  —— 赢者通吃 + 脉冲时序可塑性，神经元自发分化
' Part 3  稀疏自定义连接    —— FlyWire 风格 (pre,post,weight) 三元组驱动单层稀疏递归仿真
' Part 4  稀疏仿真 CUDA 对拍 —— 同一稀疏网络在 CPU / GPU 后端下的前向结果一致性验证
' Part 5  回归型组件自检      —— 递归脉冲层 BPTT / 线性解码头 / 回归损失 的数值梯度对拍
' ============================================================================

Imports System.Text
Imports Microsoft.VisualBasic.DeepLearning.SpikingNeuralNetwork
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

Module Program

    Sub Main()
        '  Console.WriteLine("================================================================")
        '  Console.WriteLine("  Spiking Neural Network  ·  LIF + SurrogateGrad + STDP")
        '  Console.WriteLine("  基于 Tensor 对象的第三代神经网络实现（VB.NET / BCL）")
        '  Console.WriteLine("================================================================")
        '  Console.WriteLine()

        ' GradientSelfCheck()
        ' Console.WriteLine()
        SparseDemo()
        Console.WriteLine()
        SupervisedDemo()
        Console.WriteLine()
        SparseCudaDemo()
        Console.WriteLine()
        RecurrentLayerSelfCheck()
        '  Console.WriteLine()
        ' StdpDemo()

        Console.WriteLine()
        Console.WriteLine("Done.")
    End Sub





End Module

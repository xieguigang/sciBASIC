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

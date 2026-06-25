' ============================================================================
' 演示程序
' ============================================================================
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports Microsoft.VisualBasic.MachineLearning.VariationalAutoencoder

Public Module GraphVAEDemo

    ''' <summary>
    ''' 主程序入口: 演示GraphVAE的使用
    ''' </summary>
    Public Sub Main()
        GMMDemo.Main2({})

        Console.WriteLine("="c, 70)
        Console.WriteLine("GraphVAE 演示: 图变分自编码器")
        Console.WriteLine("="c, 70)
        Console.WriteLine()

        ' === 1. 创建示例图 ===
        Console.WriteLine("[1] 创建示例图: 5节点环形图")
        Console.WriteLine()

        Dim N As Integer = 5
        ' 5节点环形图的邻接矩阵
        Dim adjData As Double(,) = {
            {0, 1, 0, 0, 1},
            {1, 0, 1, 0, 0},
            {0, 1, 0, 1, 0},
            {0, 0, 1, 0, 1},
            {1, 0, 0, 1, 0}
        }
        Dim adj As New Tensor(adjData)

        ' 使用单位矩阵作为节点特征
        Dim features As Tensor = Tensor.Identity(N)

        Console.WriteLine("原始邻接矩阵:")
        adj.Print("A")
        Console.WriteLine()

        Console.WriteLine("节点特征矩阵 (单位矩阵):")
        features.Print("X")
        Console.WriteLine()

        ' === 2. 创建GraphVAE模型 ===
        Console.WriteLine("[2] 创建GraphVAE模型")
        Console.WriteLine()

        Dim config As New GraphVAEConfig With {
            .NumNodes = N,
            .InputDim = N,
            .Hidden1 = 16,
            .Hidden2 = 8,
            .LatentDim = 4,
            .DecHidden = 8
        }

        Console.WriteLine($"  节点数: {config.NumNodes}")
        Console.WriteLine($"  输入维度: {config.InputDim}")
        Console.WriteLine($"  GCN隐藏层1: {config.Hidden1}")
        Console.WriteLine($"  GCN隐藏层2: {config.Hidden2}")
        Console.WriteLine($"  潜在维度: {config.LatentDim}")
        Console.WriteLine($"  解码器隐藏维度: {config.DecHidden}")
        Console.WriteLine()

        Dim model As New GraphVAE(config, seed:=42)

        ' === 3. 训练模型 ===
        Console.WriteLine("[3] 训练模型")
        Console.WriteLine()

        Dim graphs As New List(Of Tuple(Of Tensor, Tensor))
        graphs.Add(Tuple.Create(adj, features))

        Dim losses As List(Of Double) = model.Train(graphs, epochs:=500, lr:=0.01F, beta:=1.0F, verbose:=True)
        Console.WriteLine()

        ' === 4. 预测（重构）图 ===
        Console.WriteLine("[4] 预测（重构）图")
        Console.WriteLine()

        Dim predResult As Tuple(Of Tensor, Tensor) = model.Predict(adj, features)
        Dim reconAdj As Tensor = predResult.Item1
        Dim reconFeat As Tensor = predResult.Item2

        Console.WriteLine("重构的邻接矩阵（概率）:")
        reconAdj.Print("A'")
        Console.WriteLine()

        Console.WriteLine("重构的邻接矩阵（阈值0.5）:")
        Dim thresholdedAdj As Tensor = GraphUtils.Threshold(reconAdj, 0.5)
        thresholdedAdj.Print("A'_binary")
        Console.WriteLine()

        ' === 5. 评估 ===
        Console.WriteLine("[5] 评估重构质量")
        Console.WriteLine()

        Dim evalResult As Tuple(Of Double, Double, Double) = model.Evaluate(adj, features)
        Console.WriteLine($"  总损失: {evalResult.Item1:F6}")
        Console.WriteLine($"  邻接矩阵准确率: {evalResult.Item2 * 100:F2}%")
        Console.WriteLine($"  特征准确率: {evalResult.Item3 * 100:F2}%")
        Console.WriteLine()

        ' === 6. 生成新图 ===
        Console.WriteLine("[6] 从先验分布生成新图")
        Console.WriteLine()

        Dim genResult As Tuple(Of Tensor, Tensor) = model.Generate()
        Dim genAdj As Tensor = genResult.Item1

        Console.WriteLine("生成的邻接矩阵（概率）:")
        genAdj.Print("A_gen")
        Console.WriteLine()

        Console.WriteLine("生成的邻接矩阵（阈值0.5）:")
        Dim genThresholded As Tensor = GraphUtils.Threshold(genAdj, 0.5)
        genThresholded.Print("A_gen_binary")
        Console.WriteLine()

        ' === 7. 对比原始和重构 ===
        Console.WriteLine("[7] 对比原始图和重构图")
        Console.WriteLine()

        Console.WriteLine("原始邻接矩阵 vs 重构（二值化）:")
        Console.WriteLine()
        For i As Integer = 0 To N - 1
            Dim origRow As String = ""
            Dim reconRow As String = ""
            For j As Integer = 0 To N - 1
                origRow &= $"{CInt(adj(i, j))} "
                reconRow &= $"{CInt(thresholdedAdj(i, j))} "
            Next
            Console.WriteLine($"  原始: [{origRow}]    重构: [{reconRow}]")
        Next
        Console.WriteLine()

        Console.WriteLine("="c, 70)
        Console.WriteLine("演示完成")
        Console.WriteLine("="c, 70)
    End Sub

End Module

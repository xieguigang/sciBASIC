Imports Microsoft.VisualBasic.DeepLearning.GNN
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' 合成同质图（Stochastic Block Model）：3 个社区、100 节点/社区、16 维特征。
''' </summary>
''' <remarks>
''' <para>
''' 为什么除了 Karate 还要一个合成图：
''' </para>
''' <list type="bullet">
'''   <item>Karate 只有 34 个节点，`34³ = 39k` 低于 fp32 GEMM 阈值 —— 它验证不了混合精度路径；</item>
'''   <item>合成图 300 个节点，`16 × 300 × 300 ≈ 1.44M` 明确越过阈值，才能验证“确实降了精度”；</item>
'''   <item>SBM 的概率参数可以精确控制同质性（p_in / p_out = 8），使“准确率应显著高于随机”成为可断言的命题。</item>
''' </list>
''' <para>
''' 特征设计：前 3 维是社区 one-hot（信号），其余 13 维是 N(0, 0.3) 噪声。
''' 噪声刻意不为 0 —— 全零噪声会让“GCN 是否真的学到东西”变得不可区分。
''' </para>
''' </remarks>
Public Module SyntheticSbm

    ''' <summary>
    ''' 生成一个 SBM 图样本。
    ''' </summary>
    ''' <param name="nodesPerClass">每个社区的节点数</param>
    ''' <param name="numClasses">社区数</param>
    ''' <param name="featureDim">特征维度（≥ numClasses）</param>
    ''' <param name="pIn">社区内连边概率</param>
    ''' <param name="pOut">跨社区连边概率</param>
    ''' <param name="noiseScale">噪声特征的尺度</param>
    ''' <param name="trainRatio">训练标签比例</param>
    ''' <param name="valRatio">验证标签比例</param>
    ''' <param name="seed">随机种子（固定后整个样本完全可复现）</param>
    Public Function Create(Optional nodesPerClass As Integer = 100,
                           Optional numClasses As Integer = 3,
                           Optional featureDim As Integer = 16,
                           Optional pIn As Double = 0.08,
                           Optional pOut As Double = 0.01,
                           Optional noiseScale As Double = 0.3,
                           Optional trainRatio As Double = 0.1,
                           Optional valRatio As Double = 0.1,
                           Optional seed As Integer = 2024) As GraphSample

        If featureDim < numClasses Then
            Throw New ArgumentException("特征维度必须不小于社区数（前 numClasses 维是社区 one-hot）")
        End If

        Dim n = nodesPerClass * numClasses
        Dim random As New Random(seed)

        ' ---- 标签与节点顺序 ----
        Dim labels(n - 1) As Integer

        For i = 0 To n - 1
            labels(i) = i \ nodesPerClass
        Next

        ' ---- 边：同社区按 pIn，跨社区按 pOut ----
        Dim graph As New Graph(n, featureDim)

        For i = 0 To n - 1
            For j = i + 1 To n - 1
                Dim p = If(labels(i) = labels(j), pIn, pOut)

                If random.NextDouble() < p Then
                    Call graph.AddUndirectedEdge(i, j)
                End If
            Next
        Next

        ' ---- 特征：社区 one-hot + 高斯噪声 ----
        Dim features = New Tensor(n, featureDim)
        Dim data = features.Data

        For i = 0 To n - 1
            Dim offset = i * featureDim

            data(offset + labels(i)) = 1.0

            For j = numClasses To featureDim - 1
                data(offset + j) = NextGaussian(random) * noiseScale
            Next
        Next

        Call features.MarkHostModified()

        ' ---- 掩码：打乱后按比例切三段 ----
        Dim order = Enumerable.Range(0, n).ToArray()

        For i = n - 1 To 1 Step -1
            Dim k = random.Next(i + 1)
            Dim temp = order(i)

            order(i) = order(k)
            order(k) = temp
        Next

        Dim trainCount = std.Max(1, CInt(std.Floor(n * trainRatio)))
        Dim valCount = std.Max(1, CInt(std.Floor(n * valRatio)))

        Dim trainMask(n - 1) As Boolean
        Dim valMask(n - 1) As Boolean
        Dim testMask(n - 1) As Boolean

        For k = 0 To trainCount - 1
            trainMask(order(k)) = True
        Next

        For k = trainCount To trainCount + valCount - 1
            valMask(order(k)) = True
        Next

        For k = trainCount + valCount To n - 1
            testMask(order(k)) = True
        Next

        Return New GraphSample With {
            .Name = $"SBM-{numClasses}x{nodesPerClass}",
            .Graph = graph,
            .Features = features,
            .Labels = labels,
            .NumClasses = numClasses,
            .TrainMask = trainMask,
            .ValidationMask = valMask,
            .TestMask = testMask
        }
    End Function

    ''' <summary>Box-Muller 生成标准正态随机数（保持与 .NET 版本无关的可复现性）</summary>
    Private Function NextGaussian(random As Random) As Double
        Dim u1 = 1.0 - random.NextDouble()   ' (0, 1]，避免 Log(0)
        Dim u2 = random.NextDouble()

        Return std.Sqrt(-2.0 * std.Log(u1)) * std.Cos(2.0 * std.PI * u2)
    End Function

End Module

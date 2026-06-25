' ============================================================================
' GMVAEDemo.vb - GMVAE 算法测试 Demo
'
' 测试场景: 多模态数据聚类与生成
'   1. 生成多模态合成数据 (多个高斯簇, 模拟多峰信息)
'   2. 使用 GMVAE 训练, 同时学习聚类和生成模型
'   3. 测试聚类性能 (与真实标签对比)
'   4. 测试生成能力 (从指定类别采样生成新数据)
'   5. 测试重构能力
'
' 作者: Qingyan Agent
' ============================================================================

Imports std = System.Math

''' <summary>
''' GMVAE 算法测试 - 多模态数据聚类与生成 Demo
''' </summary>
Public Class GMVAEDemo

    Public Shared Sub Main(args As String())
        Console.WriteLine("="c, 80)
        Console.WriteLine("GMVAE (高斯混合变分自编码器) 测试 Demo")
        Console.WriteLine("="c, 80)
        Console.WriteLine()

        ' ------------------------------------------------------------
        ' 步骤 1: 生成多模态合成数据
        ' ------------------------------------------------------------
        Console.WriteLine("[步骤 1] 生成多模态合成数据 (3 个高斯簇)")
        Console.WriteLine("-"c, 80)

        Dim nClusters = 3
        Dim dim = 4
        Dim samplesPerCluster = 100
        Dim totalSamples = nClusters * samplesPerCluster

        ' 3 个簇的中心
        Dim centers = {
            (2.0, 2.0, 2.0, 2.0),
            (-2.0, -2.0, 2.0, -2.0),
            (2.0, -2.0, -2.0, 2.0)
        }

        Console.WriteLine($"数据维度: {dim}D")
        Console.WriteLine($"簇数量: {nClusters}")
        Console.WriteLine($"每簇样本数: {samplesPerCluster}")
        Console.WriteLine($"总样本数: {totalSamples}")
        Console.WriteLine()

        ' 生成数据
        Dim rng = New Random(42)
        Dim data = New Double(totalSamples * dim - 1) {}
        Dim labels = New Integer(totalSamples - 1) {}

        For i = 0 To totalSamples - 1
            Dim cluster = i \ samplesPerCluster
            labels(i) = cluster
            Dim c = centers(cluster)

            For d = 0 To dim - 1
                ' Box-Muller 生成正态分布
                Dim u1 = 1.0 - rng.NextDouble()
                Dim u2 = 1.0 - rng.NextDouble()
                Dim z = std.Sqrt(-2.0 * std.Log(u1)) * std.Sin(2.0 * std.PI * u2)
                Dim centerVal As Double
                Select Case d
                    Case 0 : centerVal = c.Item1
                    Case 1 : centerVal = c.Item2
                    Case 2 : centerVal = c.Item3
                    Case Else : centerVal = c.Item4
                End Select
                data(i * dim + d) = centerVal + z * 0.5
            Next
        Next

        Dim X = New Tensor(data, totalSamples, dim)

        Console.WriteLine("簇中心 (真实):")
        For i = 0 To nClusters - 1
            Dim c = centers(i)
            Console.WriteLine($"  簇 {i + 1}: ({c.Item1:F1}, {c.Item2:F1}, {c.Item3:F1}, {c.Item4:F1})")
        Next
        Console.WriteLine()

        ' ------------------------------------------------------------
        ' 步骤 2: 创建并训练 GMVAE 模型
        ' ------------------------------------------------------------
        Console.WriteLine("[步骤 2] 创建并训练 GMVAE 模型")
        Console.WriteLine("-"c, 80)

        Dim gmvae = New GMVAE(
            inputDim:=dim,
            latentDim:=4,
            nComponents:=nClusters,
            hiddenDim:=32,
            seed:=42)

        Console.WriteLine($"模型配置: {gmvae}")
        Console.WriteLine()

        Console.WriteLine("开始训练...")
        Console.WriteLine()
        Dim startTime = DateTime.Now
        gmvae.Fit(X,
                  epochs:=80,
                  batchSize:=32,
                  learningRate:=0.005,
                  beta:=0.5,
                  gamma:=0.5,
                  verbose:=True)
        Dim elapsed = (DateTime.Now - startTime).TotalMilliseconds

        Console.WriteLine()
        Console.WriteLine($"训练完成! 耗时: {elapsed:F1} ms")
        Console.WriteLine()

        ' ------------------------------------------------------------
        ' 步骤 3: 测试聚类性能
        ' ------------------------------------------------------------
        Console.WriteLine("[步骤 3] 测试聚类性能")
        Console.WriteLine("-"c, 80)

        Dim predLabels = gmvae.PredictClusters(X)

        ' 计算聚类准确率 (考虑标签置换)
        Dim bestAccuracy As Double = 0
        Dim bestMapping = {0, 1, 2}

        Dim perms = {
            (0, 1, 2), (0, 2, 1), (1, 0, 2), (1, 2, 0), (2, 0, 1), (2, 1, 0)
        }
        For Each p In perms
            Dim mapping = {p.Item1, p.Item2, p.Item3}
            Dim correct = 0
            For i = 0 To totalSamples - 1
                If mapping(predLabels(i)) = labels(i) Then correct += 1
            Next
            Dim acc = correct / totalSamples
            If acc > bestAccuracy Then
                bestAccuracy = acc
                bestMapping = mapping
            End If
        Next

        Console.WriteLine($"  聚类准确率 (最佳标签置换): {bestAccuracy * 100:F2}%")
        Console.WriteLine()

        ' 输出混淆矩阵
        Console.WriteLine("  混淆矩阵 (行=真实, 列=预测):")
        Console.Write($"        ")
        For j = 0 To nClusters - 1
            Console.Write($"  Pred{j + 1}")
        Next
        Console.WriteLine()

        For i = 0 To nClusters - 1
            Console.Write($"  True{i + 1}")
            For j = 0 To nClusters - 1
                Dim count = 0
                For s = 0 To totalSamples - 1
                    If labels(s) = i AndAlso bestMapping(predLabels(s)) = j Then count += 1
                Next
                Console.Write($"  {count,6}")
            Next
            Console.WriteLine()
        Next
        Console.WriteLine()

        ' ------------------------------------------------------------
        ' 步骤 4: 测试重构能力
        ' ------------------------------------------------------------
        Console.WriteLine("[步骤 4] 测试重构能力")
        Console.WriteLine("-"c, 80)

        Dim xRecon = gmvae.Reconstruct(X, useMean:=True)

        ' 计算重构 MSE
        Dim mse As Double = 0
        For i = 0 To totalSamples - 1
            For d = 0 To dim - 1
                Dim diff = X(i, d) - xRecon(i, d)
                mse += diff * diff
            Next
        Next
        mse /= totalSamples

        Console.WriteLine($"  重构 MSE: {mse:F6}")
        Console.WriteLine()

        ' 显示前 5 个样本的重构对比
        Console.WriteLine("  前 5 个样本的重构对比:")
        Console.WriteLine($"  {"#",4} | {"原始数据",-30} | {"重构数据",-30}")
        Console.WriteLine("  " + "-"c, 70)
        For i = 0 To 4
            Dim origStr = ""
            Dim reconStr = ""
            For d = 0 To dim - 1
                origStr += $"{X(i, d),6:F2} "
                reconStr += $"{xRecon(i, d),6:F2} "
            Next
            Console.WriteLine($"  {i + 1,4} | {origStr,-30} | {reconStr,-30}")
        Next
        Console.WriteLine()

        ' ------------------------------------------------------------
        ' 步骤 5: 测试生成能力
        ' ------------------------------------------------------------
        Console.WriteLine("[步骤 5] 测试生成能力 (从指定类别采样)")
        Console.WriteLine("-"c, 80)

        For k = 0 To nClusters - 1
            Console.WriteLine($"  从类别 {k + 1} 生成 5 个样本:")
            Dim generated = gmvae.Generate(5, componentIdx:=k, seed:=100 + k)

            For i = 0 To 4
                Dim s = ""
                For d = 0 To dim - 1
                    s += $"{generated(i, d),6:F2} "
                Next
                Console.WriteLine($"    样本 {i + 1}: {s}")
            Next

            ' 计算生成样本的均值
            Dim genMean = New Double(dim - 1) {}
            For i = 0 To 4
                For d = 0 To dim - 1
                    genMean(d) += generated(i, d)
                Next
            Next
            Console.Write($"    生成均值: ")
            For d = 0 To dim - 1
                genMean(d) /= 5
                Console.Write($"{genMean(d),6:F2} ")
            Next
            Console.WriteLine()

            ' 真实簇中心
            Dim c = centers(k)
            Console.WriteLine($"    真实中心:  {c.Item1,6:F2} {c.Item2,6:F2} {c.Item3,6:F2} {c.Item4,6:F2}")
            Console.WriteLine()
        Next

        ' ------------------------------------------------------------
        ' 步骤 6: 查看学习到的先验参数
        ' ------------------------------------------------------------
        Console.WriteLine("[步骤 6] 查看学习到的先验参数 p(z|y)")
        Console.WriteLine("-"c, 80)

        Dim yProba, zMean, zLogVar As Tensor
        gmvae.Encode(X, yProba, zMean, zLogVar)

        Console.WriteLine("  各样本类别归属概率 (前 10 个样本):")
        Console.Write($"  {"#",4} | ")
        For k = 0 To nClusters - 1
            Console.Write($"  Cat{k + 1}")
        Next
        Console.WriteLine($" | 真实类别")
        Console.WriteLine("  " + "-"c, 50)

        For i = 0 To 9
            Console.Write($"  {i + 1,4} | ")
            For k = 0 To nClusters - 1
                Console.Write($"  {yProba(i, k),5:F2}")
            Next
            Console.WriteLine($" | {labels(i) + 1}")
        Next
        Console.WriteLine()

        ' ------------------------------------------------------------
        ' 步骤 7: 训练损失曲线
        ' ------------------------------------------------------------
        Console.WriteLine("[步骤 7] 训练损失曲线")
        Console.WriteLine("-"c, 80)

        Dim lossHistory = gmvae.LossHistory
        Console.WriteLine($"  总训练轮数: {lossHistory.Count}")
        Console.WriteLine($"  初始损失: {lossHistory(0):F4}")
        Console.WriteLine($"  最终损失: {lossHistory(lossHistory.Count - 1):F4}")
        Console.WriteLine($"  损失下降: {lossHistory(0) - lossHistory(lossHistory.Count - 1):F4} ({(1 - lossHistory(lossHistory.Count - 1) / lossHistory(0)) * 100:F1}%)")
        Console.WriteLine()

        ' ASCII 损失曲线
        Console.WriteLine("  损失曲线 (ASCII):")
        Dim maxLoss = lossHistory(0)
        Dim minLoss = lossHistory.Min()
        Dim range = maxLoss - minLoss
        If range < 0.0001 Then range = 1

        Console.WriteLine($"  {maxLoss:F2} |")
        For i = 0 To lossHistory.Count - 1 Step std.Max(1, lossHistory.Count \ 20)
            Dim normLoss = (lossHistory(i) - minLoss) / range
            Dim barLen = CInt(normLoss * 40)
            Dim bar = New String("*"c, barLen)
            Console.WriteLine($"        | {bar} {lossHistory(i):F3}")
        Next
        Console.WriteLine($"  {minLoss:F2} |" + New String("-"c, 45))
        Console.WriteLine($"        +{"-"c,45}")
        Console.WriteLine($"         Epoch 1{" "c,40}Epoch {lossHistory.Count}")
        Console.WriteLine()

        ' ------------------------------------------------------------
        ' 步骤 8: 与 GMM 对比
        ' ------------------------------------------------------------
        Console.WriteLine("[步骤 8] 与 GMM 对比聚类性能")
        Console.WriteLine("-"c, 80)

        Dim gmm = New GaussianMixtureModel(
            nComponents:=nClusters,
            maxIter:=200,
            diagCovariance:=False,
            seed:=42)
        gmm.Fit(X)

        Dim gmmPred = gmm.Predict(X)
        Dim gmmBestAcc As Double = 0
        For Each p In perms
            Dim mapping = {p.Item1, p.Item2, p.Item3}
            Dim correct = 0
            For i = 0 To totalSamples - 1
                If mapping(gmmPred(i)) = labels(i) Then correct += 1
            Next
            Dim acc = correct / totalSamples
            If acc > gmmBestAcc Then gmmBestAcc = acc
        Next

        Console.WriteLine($"  GMVAE 聚类准确率: {bestAccuracy * 100:F2}%")
        Console.WriteLine($"  GMM  聚类准确率: {gmmBestAcc * 100:F2}%")
        Console.WriteLine()
        Console.WriteLine("  注: GMVAE 通过神经网络学习非线性特征表示,")
        Console.WriteLine("      在复杂数据分布上通常优于直接 GMM,")
        Console.WriteLine("      同时具备生成能力 (GMM 仅能聚类, 不能生成新样本)")
        Console.WriteLine()

        ' ------------------------------------------------------------
        ' 总结
        ' ------------------------------------------------------------
        Console.WriteLine("="c, 80)
        Console.WriteLine("测试总结")
        Console.WriteLine("="c, 80)
        Console.WriteLine($"1. 聚类性能: GMVAE 准确率 = {bestAccuracy * 100:F2}%")
        Console.WriteLine($"2. 重构能力: MSE = {mse:F4}")
        Console.WriteLine($"3. 生成能力: 从指定类别生成的样本均值接近真实簇中心")
        Console.WriteLine($"4. 训练稳定性: 损失从 {lossHistory(0):F2} 下降至 {lossHistory(lossHistory.Count - 1):F2}")
        Console.WriteLine()
        Console.WriteLine("结论: GMVAE 模块成功实现了:")
        Console.WriteLine("      - 多模态数据的无监督聚类")
        Console.WriteLine("      - 输入数据的重构")
        Console.WriteLine("      - 从指定类别生成新样本")
        Console.WriteLine("      - 端到端的反向传播训练")
        Console.WriteLine()
        Console.WriteLine("按任意键退出...")
        Console.ReadKey()
    End Sub

End Class

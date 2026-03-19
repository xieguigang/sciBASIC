Imports System
Imports System.Linq

Namespace GNNSharp

    ''' <summary>
    ''' GNNSharp - 一个基于.NET基础数学函数的图神经网络库
    ''' 
    ''' 本程序演示如何使用C#从零开始实现一个完整的GNN框架
    ''' 包括：
    ''' 1. 张量运算
    ''' 2. 图数据结构
    ''' 3. GCN（图卷积网络）层
    ''' 4. 训练和评估
    ''' </summary>
    Friend Class Program
        Private Shared Sub Main(args As String())
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗")
            Console.WriteLine("║           GNNSharp - 图神经网络 C# 实现                     ║")
            Console.WriteLine("║     Graph Neural Network Implementation in C#              ║")
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝")
            Console.WriteLine()

            ' 运行所有示例
            Call RunTensorDemo()
            Call RunGraphDemo()
            Call RunNodeClassificationDemo()
            Call RunGraphClassificationDemo()

            Console.WriteLine(vbLf & "所有演示完成！按任意键退出...")
            Console.ReadKey()
        End Sub

        ''' <summary>
        ''' 演示张量的基本操作
        ''' </summary>
        Private Shared Sub RunTensorDemo()
            Console.WriteLine(vbLf & "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")
            Console.WriteLine("【示例1】张量基本操作演示")
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" & vbLf)

            ' 创建张量
            Console.WriteLine("1. 创建张量:")
            Dim a = New Tensor(3, 4)  ' 3x4的零矩阵
            Console.WriteLine($"   创建3x4零矩阵: {a}")

            ' 随机初始化
            Console.WriteLine(vbLf & "2. 随机初始化:")
            Dim b = Tensor.Random(New Integer() {3, 4}, -1, 1, seed:=42)
            Console.WriteLine("   随机矩阵 b:")
            b.Print("   ")

            ' 矩阵运算
            Console.WriteLine(vbLf & "3. 矩阵运算:")
            Dim c = Tensor.Random(New Integer() {4, 2}, -1, 1, seed:=43)
            Console.WriteLine("   矩阵 c (4x2):")
            c.Print("   ")

            Dim result = b.MatMul(c)
            Console.WriteLine(vbLf & "   矩阵乘法 b × c (3x2):")
            result.Print("   ")

            ' 逐元素操作
            Console.WriteLine(vbLf & "4. 逐元素操作:")
            Dim d = Tensor.Random(New Integer() {3, 4}, 0, 2, seed:=44)
            Console.WriteLine("   矩阵 d:")
            d.Print("   ")

            Console.WriteLine(vbLf & "   ReLU激活函数 ReLU(d):")
            Dim reluResult = d.Apply(New Func(Of Single, Single)(AddressOf ReLU))
            reluResult.Print("   ")

            ' 聚合操作
            Console.WriteLine(vbLf & "5. 聚合操作:")
            Console.WriteLine($"   所有元素之和: {d.TotalSum():F4}")
            Console.WriteLine($"   所有元素平均值: {d.Mean():F4}")
            Console.WriteLine($"   L2范数: {d.L2Norm():F4}")
        End Sub

        ''' <summary>
        ''' 演示图数据结构
        ''' </summary>
        Private Shared Sub RunGraphDemo()
            Console.WriteLine(vbLf & "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")
            Console.WriteLine("【示例2】图数据结构演示")
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" & vbLf)

            ' 创建一个简单的图
            Console.WriteLine("1. 创建图:")
            Dim numNodes = 6
            Dim featureDim = 3
            Dim graph = New Graph(numNodes, featureDim)

            ' 设置节点特征
            Console.WriteLine("   设置节点特征...")
            Dim nodeFeatures = Tensor.Random(New Integer() {numNodes, featureDim}, 0, 1, seed:=100)
            graph.NodeFeatures = nodeFeatures

            ' 添加边（创建一个简单的连通图）
            Console.WriteLine("   添加边（创建连通图）:")
            graph.AddUndirectedEdge(0, 1)
            graph.AddUndirectedEdge(0, 2)
            graph.AddUndirectedEdge(1, 2)
            graph.AddUndirectedEdge(1, 3)
            graph.AddUndirectedEdge(2, 4)
            graph.AddUndirectedEdge(3, 4)
            graph.AddUndirectedEdge(3, 5)
            graph.AddUndirectedEdge(4, 5)

            ' 打印图信息
            Console.WriteLine()
            graph.PrintInfo()

            ' 获取邻接矩阵
            Console.WriteLine(vbLf & "2. 邻接矩阵:")
            Dim adjMatrix = graph.GetAdjacencyMatrix()
            adjMatrix.Print("   ")

            ' 获取归一化邻接矩阵（GCN使用）
            Console.WriteLine(vbLf & "3. 归一化邻接矩阵 (用于GCN):")
            Dim normAdj = graph.GetNormalizedAdjacencyMatrix()
            normAdj.Print("   ")

            ' 演示邻居查询
            Console.WriteLine(vbLf & "4. 邻居查询:")
            For i = 0 To numNodes - 1
                Dim neighbors = graph.GetNeighbors(i)
                Console.WriteLine($"   节点 {i} 的邻居: [{String.Join(", ", neighbors)}]")
            Next
        End Sub

        ''' <summary>
        ''' 演示节点分类任务
        ''' 使用GCN进行半监督节点分类
        ''' </summary>
        Private Shared Sub RunNodeClassificationDemo()
            Console.WriteLine(vbLf & "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")
            Console.WriteLine("【示例3】节点分类任务 - GCN演示")
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" & vbLf)

            ' 创建一个模拟的引文网络
            Console.WriteLine("1. 创建模拟引文网络:")
            Dim numNodes = 100
            Dim featureDim = 16
            Dim numClasses = 4
            Dim hiddenDim = 32

            Dim graph = New Graph(numNodes, featureDim)

            ' 设置节点特征
            graph.NodeFeatures = Tensor.Random(New Integer() {numNodes, featureDim}, -1, 1, seed:=200)

            ' 创建随机边（模拟引文关系）
            Dim random = New Random(201)
            Dim avgDegree = 5
            For i = 0 To numNodes - 1
                Dim numNeighbors = random.Next(2, avgDegree * 2)
                For k = 0 To numNeighbors - 1
                    Dim j = random.Next(numNodes)
                    If i <> j Then
                        graph.AddUndirectedEdge(i, j)
                    End If
                Next
            Next

            graph.PrintInfo()

            ' 生成模拟标签（基于节点特征的简单规则）
            Console.WriteLine(vbLf & "2. 生成模拟标签:")
            Dim labels = New Integer(numNodes - 1) {}
            For i = 0 To numNodes - 1
                ' 简单规则：根据特征和决定类别
                Dim sum As Single = 0
                For j = 0 To featureDim - 1
                    sum += graph.NodeFeatures(i, j)
                Next
                labels(i) = CInt((sum + featureDim) / (2 * featureDim) * numClasses)
                labels(i) = Math.Clamp(labels(i), 0, numClasses - 1)
            Next
            Console.WriteLine($"   类别分布: {String.Join(", ", Enumerable.Range(0, numClasses).[Select](Function(c) $"C{c}:{labels.Count(Function(l) l = c)}"))}")

            ' 划分训练/验证/测试集
            Console.WriteLine(vbLf & "3. 划分数据集:")
            Dim trainMask = New Boolean(numNodes - 1) {}
            Dim valMask = New Boolean(numNodes - 1) {}
            Dim testMask = New Boolean(numNodes - 1) {}

            For i = 0 To numNodes - 1
                If i < numNodes * 0.1 Then  ' 10% 训练
                    trainMask(i) = True
                ElseIf i < numNodes * 0.3 Then  ' 20% 验证
                    valMask(i) = True  ' 70% 测试
                Else
                    testMask(i) = True
                End If
            Next
            Console.WriteLine($"   训练节点: {trainMask.Count(Function(x) x)}")
            Console.WriteLine($"   验证节点: {valMask.Count(Function(x) x)}")
            Console.WriteLine($"   测试节点: {testMask.Count(Function(x) x)}")

            ' 创建模型
            Console.WriteLine(vbLf & "4. 创建GCN模型:")
            Dim model = New GCNModel(featureDim, hiddenDim, numClasses)
            model.PrintModelInfo()

            ' 创建优化器和训练器
            Dim parameters = model.GetParameters()
            Dim gradients = model.GetGradients()
            Dim optimizer = New AdamOptimizer(parameters, gradients, learningRate:=0.01F)
            Dim trainer = New Trainer(model, optimizer)

            ' 训练模型
            Console.WriteLine(vbLf & "5. 开始训练:")
            trainer.Train(graph, labels, trainMask, valMask, epochs:=100, printEvery:=20)

            ' 测试模型
            Console.WriteLine(vbLf & "6. 测试结果:")
            Dim testAcc = trainer.Evaluate(graph, labels, testMask)
            Console.WriteLine($"   测试集准确率: {testAcc:P2}")

            ' 展示一些预测结果
            Console.WriteLine(vbLf & "7. 预测示例:")
            model.SetTraining(False)
            Dim probs = model.Forward(graph.NodeFeatures, graph)

            Console.WriteLine("   节点 | 真实标签 | 预测标签 | 置信度")
            Console.WriteLine("   " & New String("-"c, 45))
            For i = 0 To Math.Min(10, numNodes) - 1
                Dim pred = 0
                Dim maxProb = probs(i, 0)
                For j = 1 To numClasses - 1
                    If probs(i, j) > maxProb Then
                        maxProb = probs(i, j)
                        pred = j
                    End If
                Next
                Dim status = If(pred = labels(i), "✓", "✗")
                Console.WriteLine($"   {i,4} |    {labels(i)}     |    {pred}     |  {maxProb:P1} {status}")
            Next
        End Sub

        ''' <summary>
        ''' 演示图分类任务
        ''' </summary>
        Private Shared Sub RunGraphClassificationDemo()
            Console.WriteLine(vbLf & "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")
            Console.WriteLine("【示例4】图分类任务演示")
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" & vbLf)

            ' 创建模拟数据集
            Console.WriteLine("1. 创建模拟图数据集:")
            Dim dataset = New GraphDataset()
            Dim random = New Random(300)

            Dim numGraphs = 50
            Dim numClasses = 3
            Dim featureDim = 8
            Dim hiddenDim = 16

            For g = 0 To numGraphs - 1
                ' 随机生成图的大小
                Dim numNodes = random.Next(5, 15)
                Dim graph = New Graph(numNodes, featureDim)

                ' 设置节点特征
                graph.NodeFeatures = Tensor.Random(New Integer() {numNodes, featureDim}, -1, 1, seed:=400 + g)

                ' 添加边（随机图）
                Dim edgeProb = 0.3
                For i = 0 To numNodes - 1
                    For j = i + 1 To numNodes - 1
                        If random.NextDouble() < edgeProb Then
                            graph.AddUndirectedEdge(i, j)
                        End If
                    Next
                Next

                ' 根据图的密度分配标签
                Dim density = graph.NumEdges / (numNodes * (numNodes - 1) / 2)
                Dim label As Integer
                If density < 0.2 Then
                    label = 0      ' 稀疏图
                ElseIf density < 0.4 Then
                    label = 1 ' 中等密度
                Else
                    label = 2
                End If                     ' 密集图

                dataset.Add(graph, label)
            Next

            Console.WriteLine($"   数据集大小: {dataset.Count} 个图")
            Console.WriteLine($"   类别数量: {dataset.NumClasses}")
            Console.WriteLine($"   类别分布: {String.Join(", ", Enumerable.Range(0, numClasses).[Select](Function(c) $"C{c}:{dataset.Labels.Where(Function(l) l = c).Count}"))}")

            ' 创建模型
            Console.WriteLine(vbLf & "2. 创建图分类模型:")
            Dim model = New GraphClassificationModel(featureDim, hiddenDim, numClasses)
            model.PrintModelInfo()

            ' 创建优化器和训练器
            Dim parameters = model.GetParameters()
            Dim gradients = model.GetGradients()
            Dim optimizer = New AdamOptimizer(parameters, gradients, learningRate:=0.005F)
            Dim trainer = New GraphClassificationTrainer(model, optimizer)

            ' 训练模型
            Console.WriteLine(vbLf & "3. 开始训练:")
            trainer.Train(dataset, trainRatio:=0.8F, epochs:=50, printEvery:=10)

            ' 展示预测结果
            Console.WriteLine(vbLf & "4. 预测示例:")
            model.SetTraining(False)
            Console.WriteLine("   图ID | 真实标签 | 预测标签 | 置信度")
            Console.WriteLine("   " & New String("-"c, 45))

            For i = 0 To Math.Min(10, dataset.Count) - 1
                Dim graph = dataset.Graphs(i)
                Dim trueLabel = dataset.Labels(i)

                Dim probs = model.Forward(graph.NodeFeatures, graph)

                Dim pred = 0
                Dim maxProb = probs(0, 0)
                For j = 1 To numClasses - 1
                    If probs(0, j) > maxProb Then
                        maxProb = probs(0, j)
                        pred = j
                    End If
                Next

                Dim status = If(pred = trueLabel, "✓", "✗")
                Console.WriteLine($"   {i,4} |    {trueLabel}     |    {pred}     |  {maxProb:P1} {status}")
            Next
        End Sub
    End Class

    ''' <summary>
    ''' 数据生成工具类
    ''' 提供生成测试数据集的便捷方法
    ''' </summary>
    Public Module DataGenerator
        ''' <summary>
        ''' 生成Karate Club图（经典的社交网络数据集）
        ''' 这是一个用于测试GNN的标准数据集
        ''' </summary>
        Public Function GenerateKarateClubGraph() As Graph
            ' Zachary's Karate Club 网络
            ' 34个节点，表示一个大学空手道俱乐部的成员
            Dim graph = New Graph(34, 2)  ' 2维特征

            ' 边列表（Zachary原始数据）
            Dim edges = {
                {0, 1},
                {0, 2},
                {0, 3},
                {0, 4},
                {0, 5},
                {0, 6},
                {0, 7},
                {0, 8},
                {0, 10},
                {0, 11},
                {0, 12},
                {0, 13},
                {0, 17},
                {0, 19},
                {0, 21},
                {0, 31},
                {1, 2},
                {1, 3},
                {1, 7},
                {1, 13},
                {1, 17},
                {1, 19},
                {1, 21},
                {2, 3},
                {2, 7},
                {2, 8},
                {2, 9},
                {2, 13},
                {2, 27},
                {2, 28},
                {2, 32},
                {3, 7},
                {3, 12},
                {3, 13},
                {4, 6},
                {4, 10},
                {5, 6},
                {5, 10},
                {5, 16},
                {6, 16},
                {8, 30},
                {8, 32},
                {8, 33},
                {9, 33},
                {13, 33},
                {14, 32},
                {14, 33},
                {15, 32},
                {15, 33},
                {18, 32},
                {18, 33},
                {19, 33},
                {20, 32},
                {20, 33},
                {22, 32},
                {22, 33},
                {23, 25},
                {23, 27},
                {23, 29},
                {23, 32},
                {23, 33},
                {24, 25},
                {24, 27},
                {24, 31},
                {25, 31},
                {26, 29},
                {26, 33},
                {27, 33},
                {28, 31},
                {28, 33},
                {29, 32},
                {29, 33},
                {30, 32},
                {30, 33},
                {31, 32},
                {31, 33},
            {32, 33}}

            For i = 0 To edges.GetLength(0) - 1
                graph.AddUndirectedEdge(edges(i, 0), edges(i, 1))
            Next

            ' 设置简单的节点特征
            Dim random = New Random(42)
            For i = 0 To 33
                graph.NodeFeatures(i, 0) = CSng(random.NextDouble())
                graph.NodeFeatures(i, 1) = CSng(random.NextDouble())
            Next

            Return graph
        End Function

        ''' <summary>
        ''' 生成随机图数据集
        ''' </summary>
        Public Function GenerateRandomGraphDataset(numGraphs As Integer, minNodes As Integer, maxNodes As Integer, featureDim As Integer, numClasses As Integer, Optional seed As Integer = 42) As GraphDataset
            Dim dataset = New GraphDataset()
            Dim random = New Random(seed)

            For g = 0 To numGraphs - 1
                Dim numNodes = random.Next(minNodes, maxNodes + 1)
                Dim graph = New Graph(numNodes, featureDim)

                ' 随机节点特征
                graph.NodeFeatures = Tensor.Random(New Integer() {numNodes, featureDim}, -1, 1, seed:=seed + g)

                ' 随机边
                Dim edgeProb = 0.3
                For i = 0 To numNodes - 1
                    For j = i + 1 To numNodes - 1
                        If random.NextDouble() < edgeProb Then
                            graph.AddUndirectedEdge(i, j)
                        End If
                    Next
                Next

                ' 随机标签
                Dim label = random.Next(numClasses)
                dataset.Add(graph, label)
            Next

            Return dataset
        End Function

        ''' <summary>
        ''' 生成环形图数据集（用于测试GNN的图拓扑学习能力）
        ''' </summary>
        Public Function GenerateRingGraphs(numGraphs As Integer, ringSize As Integer, featureDim As Integer, Optional seed As Integer = 42) As GraphDataset
            Dim dataset = New GraphDataset()
            Dim random = New Random(seed)

            For g = 0 To numGraphs - 1
                Dim graph = New Graph(ringSize, featureDim)

                ' 创建环形结构
                For i = 0 To ringSize - 1
                    graph.AddUndirectedEdge(i, (i + 1) Mod ringSize)
                Next

                ' 随机节点特征
                For i = 0 To ringSize - 1
                    For j = 0 To featureDim - 1
                        graph.NodeFeatures(i, j) = CSng((random.NextDouble() * 2 - 1))
                    Next
                Next

                ' 标签：环的长度类别
                Dim label = ringSize Mod 3
                dataset.Add(graph, label)
            Next

            Return dataset
        End Function
    End Module
End Namespace

Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.nn.rbm
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.nn.rbm.deep
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.nn.rbm.learn

''' <summary>
''' RBM / 深度信念网络的控制台测试入口。
''' 
''' 依次验证：
''' 1. 伯努利采样是否服从给定的激活概率（均匀分布随机数）
''' 2. 单层RBM的对比散度训练是否收敛
''' 3. 深度信念网络的逐层贪心预训练、编码与采样生成
''' 4. 模型保存与加载的往返一致性
''' 
''' 所有的断言通过 <see cref="DebugAudit"/> 记录，最后输出JSON格式的测试报告。
''' </summary>
Module Program

    Public Function Main(args As String()) As Integer
        Dim report As String = Path.Combine(AppContext.BaseDirectory, "rbm-test-report.json")

        Call DebugAudit.Reset()
        DebugAudit.MaxEvents = 4096

        Console.WriteLine("==================================================")
        Console.WriteLine(" 受限玻尔兹曼机 / 深度信念网络 测试")
        Console.WriteLine("==================================================")

        Call runSuite("伯努利采样", AddressOf TestBernoulliSampling)
        Call runSuite("单层RBM对比散度", AddressOf TestContrastiveDivergence)
        Call runSuite("深度信念网络", AddressOf TestDeepBeliefNetwork)
        Call runSuite("模型序列化", AddressOf TestPersist)

        Call DebugAudit.SaveReport(report)

        Console.WriteLine()
        Console.WriteLine("==================================================")
        Console.WriteLine($" 失败断言数: {DebugAudit.FailureCount}")
        Console.WriteLine($" 测试报告:   {report}")
        Console.WriteLine("==================================================")

        Return DebugAudit.FailureCount
    End Function

    ''' <summary>
    ''' 执行一组测试：单个用例的异常不会打断后续的用例，但是会被记录为失败断言。
    ''' </summary>
    Private Sub runSuite(name As String, action As Action)
        Try
            Call action()
        Catch ex As Exception
            Call DebugAudit.Check(False, $"{name} 抛出异常: {ex.GetType().Name}: {ex.Message}")
            Call Console.Error.WriteLine($"[{name}] {ex}")
        End Try
    End Sub

    Private Function TrainingData() As DenseMatrix
        Dim data As Double()() = {
            New Double() {1.0, 1.0, 1.0, 0.0, 0.0, 0.0},
            New Double() {1.0, 0.0, 1.0, 0.0, 0.0, 0.0},
            New Double() {1.0, 1.0, 1.0, 0.0, 0.0, 0.0},
            New Double() {0.0, 0.0, 1.0, 1.0, 1.0, 0.0},
            New Double() {0.0, 0.0, 1.0, 1.0, 0.0, 0.0},
            New Double() {0.0, 0.0, 1.0, 1.0, 1.0, 0.0}
        }

        Return DenseMatrix.make(data)
    End Function

    ''' <summary>
    ''' 权重与偏置全部为0的时候，P(h=1|v) = 0.5，采样结果的开启比例应该接近0.5。
    ''' 如果采样使用的是高斯随机数，这个比例会偏离到 Φ(0.5) ≈ 0.69。
    ''' </summary>
    Private Sub TestBernoulliSampling()
        Dim rbm As New RBM(4, 3)

        For i As Integer = 0 To rbm.VisibleSize - 1
            For j As Integer = 0 To rbm.HiddenSize - 1
                Call rbm.Weights.set(i, j, 0)
            Next
        Next

        Dim visible As DenseMatrix = DenseMatrix.fill(1000, rbm.VisibleSize, 1)
        Dim states As DenseMatrix = rbm.SampleHidden(visible)
        Dim activated As Integer = 0

        For i As Integer = 0 To states.rows() - 1
            For j As Integer = 0 To states.columns() - 1
                If states.get(i, j) >= 0.5 Then
                    activated += 1
                End If
            Next
        Next

        Dim ratio As Double = activated / (states.rows() * states.columns())

        Call DebugAudit.Test(System.Math.Abs(ratio - 0.5) < 0.05, $"二值采样应该服从给定的激活概率0.5，实际开启比例={ratio}")
        Call DebugAudit.Test(states.rows() = 1000 AndAlso states.columns() = rbm.HiddenSize, "采样结果矩阵的尺寸应该与隐藏层单元数一致")

        Call DebugAudit.Mark("bernoulli.activationRatio", ratio)
    End Sub

    Private Sub TestContrastiveDivergence()
        Dim data As DenseMatrix = TrainingData()

        Dim rbm As New BernoulliRBM(6, 3)

        Dim before As Double = rbm.ReconstructionError(data)
        Dim after As Double = rbm.Train(data, epochs:=200, learningRate:=0.1, k:=1)

        Call DebugAudit.Test(after < before, $"训练之后的重构误差应该下降: before={before}, after={after}")
        Call DebugAudit.Test(rbm.FreeEnergy(data).Length = data.rows(), "自由能的计算结果应该按照样本数目返回")

        Call DebugAudit.Mark("rbm.reconstructionError.before", before)
        Call DebugAudit.Mark("rbm.reconstructionError.after", after)
    End Sub

    Private Sub TestDeepBeliefNetwork()
        Dim data As DenseMatrix = TrainingData()

        Dim dbn As New DeepBeliefNetwork()
        Call dbn.AddLayer(6, 4)
        Call dbn.AddLayer(4, 2)

        Call DebugAudit.Test(dbn.LayerCount = 2, "深度信念网络应该包含两层RBM")
        Call DebugAudit.Test(dbn.VisibleSize = 6 AndAlso dbn.HiddenSize = 2, "深度信念网络的输入输出尺寸不正确")

        Dim before As Double = dbn.ReconstructionError(data)
        Dim energyBefore As Double = dbn.AverageFreeEnergy(data)

        Call dbn.TrainLayerWise(data, epochs:=200, learningRate:=0.1, k:=1)

        Dim after As Double = dbn.ReconstructionError(data)
        Dim energyAfter As Double = dbn.AverageFreeEnergy(data)

        Call DebugAudit.Test(after < before, $"逐层预训练之后的重构误差应该下降: before={before}, after={after}")
        Call DebugAudit.Test(energyAfter < energyBefore, $"逐层预训练之后最底层的自由能应该下降: before={energyBefore}, after={energyAfter}")

        Dim feature As DenseMatrix = dbn.Encode(data)
        Call DebugAudit.Test(feature.rows() = data.rows() AndAlso feature.columns() = dbn.HiddenSize, "Encode的输出尺寸不正确")

        Dim restored As DenseMatrix = dbn.Reconstruct(data)
        Call DebugAudit.Test(restored.rows() = data.rows() AndAlso restored.columns() = data.columns(), "Reconstruct的输出尺寸不正确")

        Dim generated As DenseMatrix = dbn.Generate(8, gibbsSteps:=3)
        Call DebugAudit.Test(generated.rows() = 8 AndAlso generated.columns() = dbn.VisibleSize, "Generate的输出尺寸不正确")

        Call DebugAudit.Mark("dbn.reconstructionError.after", after)
        Call DebugAudit.Mark("dbn.freeEnergy.after", energyAfter)
    End Sub

    Private Sub TestPersist()
        Dim data As DenseMatrix = TrainingData()

        Dim dbn As New DeepBeliefNetwork()
        Call dbn.AddLayer(6, 4)
        Call dbn.AddLayer(4, 2)

        Call dbn.TrainLayerWise(data, epochs:=20, learningRate:=0.05)

        Dim path As String = Path.Combine(AppContext.BaseDirectory, "dbn-roundtrip.rbm")
        Call dbn.Save(path)

        Call DebugAudit.Test(File.Exists(path), "模型文件应该被成功的创建")

        Dim model As DeepBeliefNetwork = DeepBeliefNetwork.Load(path)
        Dim expected As RBM = dbn.Layers(0)
        Dim actual As RBM = model.Layers(0)

        Call DebugAudit.Test(model.LayerCount = dbn.LayerCount, "加载之后的网络层数应该与原始模型一致")
        Call DebugAudit.Test(System.Math.Abs(expected.VBias(0) - actual.VBias(0)) < 1.0E-12, "可见层偏置的往返误差过大")
        Call DebugAudit.Test(System.Math.Abs(expected.HBias(0) - actual.HBias(0)) < 1.0E-12, "隐藏层偏置的往返误差过大")
        Call DebugAudit.Test(System.Math.Abs(expected.Weights.get(0, 0) - actual.Weights.get(0, 0)) < 1.0E-12, "权重矩阵的往返误差过大")

        Call File.Delete(path)
    End Sub

End Module

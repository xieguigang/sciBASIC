Imports System
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Trainings

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
        net.LERNRATE = 0.3

        For i As Integer = 1 To 3000
            Dim x0 = rnd.NextDouble()
            Dim x1 = rnd.NextDouble()
            Call net.train({x0, x1}, {x0 + x1})
        Next

        Dim p = net.predict({0.3, 0.5})
        Console.WriteLine($"  predict(0.3,0.5) = {p(0):F4}  (target 0.8)  TotalError={net.TotalError:F4}")
        Call AssertFinite("Netz.predict", p(0))

        Dim path = "netz_model.cnn"
        Call net.Save(path)
        Dim net2 = Netz.Load(path)
        Dim p2 = net2.predict({0.3, 0.5})
        Console.WriteLine($"  loaded predict   = {p2(0):F4}")
        Call AssertFinite("Netz.Load.predict", p2(0))

        ' 镜像只读属性
        Console.WriteLine($"  HiddenLayerCount={net.HiddenLayerCount} Input={net.InputNeuronCount} Output={net.OutputNeuronCount} MaxIdx={net.MaxOutputNeuronIndex}")
    End Sub

    Private Sub TestNetwork()
        Console.WriteLine("=== Network (CNN kernel) ===")

        ' 在线逐样本训练（ANNTrainer -> Network.ForwardPropagate/BackPropagate -> CNN）
        Dim trainer As New ANNTrainer(2, {4}, 1, 0.3, 0.9)
        For i As Integer = 1 To 600
            Dim a = rnd.NextDouble()
            Dim b = rnd.NextDouble()
            Call trainer.Add({a, b}, a + b)
        Next
        Call trainer.Train(300)

        Dim err = trainer.NeuronNetwork.Compute(0.3, 0.5)
        Console.WriteLine($"  online  predict = {err(0):F4}  (target 0.8)")
        Call AssertFinite("Network.online.Compute", err(0))

        ' 批量训练（CNN.Trainer.train）
        Dim network2 As New Network(inputSize:=2, hiddenSize:={4}, outputSize:=1, learnRate:=0.3)
        Call network2.TrainBatch(trainer.TrainingSet, 200)
        Dim err2 = network2.Compute(0.3, 0.5)
        Console.WriteLine($"  batch   predict = {err2(0):F4}")
        Call AssertFinite("Network.batch.Compute", err2(0))

        ' 保存/加载 CNN 二进制模型
        Dim npath = "network_model.cnn"
        Call network2.Save(npath)
        Dim network3 = Network.Load(npath)
        Dim err3 = network3.Compute(0.3, 0.5)
        Console.WriteLine($"  loaded  predict = {err3(0):F4}")
        Call AssertFinite("Network.Load.Compute", err3(0))

        ' 镜像只读属性
        Console.WriteLine($"  Input={network3.InputLayer.Count} Hidden={network3.HiddenLayer.Count} Output={network3.OutputLayer.Count}")
    End Sub

    Private Sub AssertFinite(tag As String, v As Double)
        If Double.IsNaN(v) OrElse Double.IsInfinity(v) Then
            Call Console.WriteLine($"  [FAIL] {tag} produced non-finite value: {v}")
            Environment.Exit(1)
        End If
    End Sub

End Module

Imports System.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.CNN
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.CNN.trainers
Imports Microsoft.VisualBasic.MachineLearning.DataStorage
Imports randf2 = Microsoft.VisualBasic.Math.RandomExtensions

''' <summary>
''' 确定性的 CNN 训练基线测量器。
''' </summary>
''' <remarks>
''' <para>
''' 存在的意义：<see cref="DataBlock"/> 的权重初始化走 ``Vector.rand``，而它底层用的是
''' ``RandomExtensions.seeds`` 这个**未播种**的共享随机数发生器，因此每次运行的初始
''' 权重都不同，训练出来的准确率会在若干个百分点之间浮动，无法用来判断"某次重构是否
''' 改变了数值行为"。
''' </para>
''' <para>
''' 这里在构建网络之前调用 ``randf2.SetSeed(seed)`` 固定随机序列，并使用固定的样本集
''' 与轮数，最后输出一行机器可读的结果；同样的 (seed, passes, samples) 多次运行应当得到
''' **逐位相同**的 loss / accuracy，从而把基线钉死。
''' </para>
''' </remarks>
Public Class MnistBaseline

    ''' <summary>MNIST 训练集镜像文件</summary>
    Public Const MnistImages As String =
        "G:\GCModeller\src\R-sharp\test\demo\machineLearning\umap\mnist_dataset\train-images-idx3-ubyte"

    ''' <summary>MNIST 训练集标签文件</summary>
    Public Const MnistLabels As String =
        "G:\GCModeller\src\R-sharp\test\demo\machineLearning\umap\mnist_dataset\train-labels-idx1-ubyte"

    ''' <summary>
    ''' 构建与本仓库 MnistTest 完全一致的网络结构
    ''' （input 28x28x1 -> conv5x32 -> relu -> pool2 -> conv5x64 -> relu -> pool2 -> fc10 -> softmax）
    ''' </summary>
    Private Shared Function BuildNetwork(mr As MNIST) As ConvolutionalNN
        Dim layers As New LayerBuilder

        Call layers.buildInputLayer(New Dimension(mr.ImageSize.Width, mr.ImageSize.Height), 1)
        Call layers.buildConvLayer(5, 32, 1, 2)
        Call layers.buildReLULayer()
        Call layers.buildPoolLayer(2, 2, 0)
        Call layers.buildConvLayer(5, 64, 1, 2)
        Call layers.buildReLULayer()
        Call layers.buildPoolLayer(2, 2, 0)
        Call layers.buildFullyConnectedLayer(10)
        Call layers.buildSoftmaxLayer()

        Return New ConvolutionalNN(layers)
    End Function

    ''' <summary>
    ''' 以固定随机种子跑一次确定性的训练 + 评估，并打印单行结果。
    ''' </summary>
    ''' <param name="seed">权重初始化的随机种子</param>
    ''' <param name="passes">训练轮数（每轮遍历 <paramref name="samples"/> 张图）</param>
    ''' <param name="samples">每轮使用的样本数</param>
    Public Shared Sub Run(seed As Integer, passes As Integer, samples As Integer)
        ' 必须在构建网络(即首次调用 Vector.rand)之前播种
        Call randf2.SetSeed(seed)

        Dim mr As New MNIST(MnistImages, MnistLabels)

        ' 先把样本物化，保证每轮、每次运行看到的都是同一批、同一顺序的数据
        Dim dataset = mr.ExtractVectors.Take(samples).ToArray
        Dim net As ConvolutionalNN = BuildNetwork(mr)
        Dim trainer As TrainerAlgorithm = New AdaGradTrainer(20, 0.001F).SetKernel(net)
        Dim db As New DataBlock(mr.ImageSize.Width, mr.ImageSize.Height, 1, 0)

        Dim watch As Stopwatch = Stopwatch.StartNew()
        Dim lastLoss As Double = 0

        For p As Integer = 1 To passes
            Dim loss As Double = 0
            Dim check As New PerformanceCounter

            For Each img In dataset
                Call db.addImageData(img.value, img.value.Max)

                Dim tr As TrainResult = trainer.train(db, {Val(img.description)}, check.Set)
                loss += tr.Loss
            Next

            lastLoss = loss / dataset.Length
        Next

        Dim correct As Integer = 0

        For Each img In dataset
            Call db.addImageData(img.value, img.value.Max)

            If img.description = Which.Max(net.predict(db)) Then
                correct += 1
            End If
        Next

        watch.Stop()

        ' 机器可读的单行结果：同样的 (seed, passes, samples) 必须得到同样的这几个数值
        Call Console.WriteLine(
            $"BASELINE seed={seed} passes={passes} samples={dataset.Length} " &
            $"loss={lastLoss:R} correct={correct}/{dataset.Length} " &
            $"accuracy={CDbl(correct) / dataset.Length:R} seconds={watch.Elapsed.TotalSeconds:F3}")
    End Sub

End Class

#Region "Microsoft.VisualBasic::d705cdad78bca818f3c73a362d398452, Data_science\MachineLearning\DeepLearning\NeuralNetwork\Trainings\ANNTrainer.vb"

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

    '   Total Lines: 446
    '    Code Lines: 284 (63.68%)
    ' Comment Lines: 95 (21.30%)
    '    - Xml Docs: 87.37%
    ' 
    '   Blank Lines: 67 (15.02%)
    '     File Size: 17.41 KB


    '     Class ANNTrainer
    ' 
    '         Properties: dropOutRate, ErrorThreshold, MinError, NeuronNetwork, Selective
    '                     TrainingSet, TrainingType, Truncate, XP
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: CalculateError, errorSum, SetDropOut, SetLayerNormalize, SetOutputNames
    '                   SetSelective, SetSnapshotLocation, trainingImpl
    ' 
    '         Sub: (+2 Overloads) Add, (+2 Overloads) Corrects, RemoveLast, (+3 Overloads) Train, TrainInternal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.Utility
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Activations
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Protocols
Imports Microsoft.VisualBasic.Text
Imports stdNum = System.Math

Namespace NeuralNetwork

    ''' <summary>
    ''' 
    ''' </summary>
    Public MustInherit Class ANNTrainer : Inherits IterationReporter(Of Network)

        Public Property TrainingType As TrainingType = TrainingType.Epoch
        Public Property MinError As Double = Helpers.MinimumError

        ''' <summary>
        ''' 对<see cref="Neuron.Gradient"/>的剪裁限制阈值，小于等于零表示不进行剪裁，默认不剪裁
        ''' </summary>
        ''' <returns></returns>
        Public Property Truncate As Double
            Get
                Return network.Truncate
            End Get
            Set(value As Double)
                network.Truncate = value
            End Set
        End Property

        ''' <summary>
        ''' 是否对训练样本数据集进行选择性的训练，假若目标样本在当前所训练的模型上面
        ''' 所计算得到的预测结果和其真实结果的误差足够小的话，目标样本将不会再进行训练
        ''' </summary>
        ''' <returns></returns>
        Public Property Selective As Boolean = True
        Public Property ErrorThreshold As Double = 0.01
        ''' <summary>
        ''' [0,1]之间,建议设置一个[0.3,0.6]之间的值, 这个参数表示被随机删除的节点的数量百分比,值越高,则剩下的神经元节点越少
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property dropOutRate As Double = 0

        ''' <summary>
        ''' 最终得到的训练结果神经网络
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property NeuronNetwork As Network
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return network
            End Get
        End Property

        Public ReadOnly Property TrainingSet As TrainingSample()
            Get
                Return dataSets.ToArray
            End Get
        End Property

        Protected ReadOnly network As Network

        ''' <summary>
        ''' current errors on each classify dimension.
        ''' 
        ''' (模型当前的训练误差)
        ''' </summary>
        Protected errors As Double()
        Protected dataSets As New List(Of TrainingSample)
        Protected snapshotSaveLocation As String
        Protected outputNames As String()

        ''' <summary>
        ''' 训练所使用到的经验数量,即数据集的大小size
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property XP As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return dataSets.Count
            End Get
        End Property

        Sub New(net As Network)
            network = net
        End Sub

        ''' <summary>
        ''' 以指定的网络规模参数进行人工神经网络模型的构建
        ''' </summary>
        ''' <param name="inputSize"></param>
        ''' <param name="hiddenSize"></param>
        ''' <param name="outputSize"></param>
        ''' <param name="learnRate"></param>
        ''' <param name="momentum"></param>
        ''' <param name="active"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(inputSize As Integer, hiddenSize As Integer(), outputSize As Integer,
                       Optional learnRate As Double = 0.1,
                       Optional momentum As Double = 0.9,
                       Optional active As LayerActives = Nothing,
                       Optional weightInit As Func(Of Double) = Nothing)

            Call Me.New(New Network(
                 inputSize:=inputSize,
                 hiddenSize:=hiddenSize,
                 outputSize:=outputSize,
                 learnRate:=learnRate,
                 momentum:=momentum,
                 active:=active,
                 weightInit:=weightInit
            ))
        End Sub

        Public Sub RemoveLast()
            If Not dataSets.Count = 0 Then
                Call dataSets.RemoveLast
            End If
        End Sub

        Public Function SetSelective(opt As Boolean) As ANNTrainer
            Selective = opt
            Return Me
        End Function

        ''' <summary>
        ''' set a directory path for save the model xml files
        ''' </summary>
        ''' <param name="save">
        ''' a directory path for save model file
        ''' </param>
        ''' <returns></returns>
        Public Function SetSnapshotLocation(save As String) As ANNTrainer
            snapshotSaveLocation = save
            Return Me
        End Function

        Public Function SetOutputNames(names As String()) As ANNTrainer
            Dim maxLen As Integer = names.Select(AddressOf Strings.Len).Max

            outputNames = names _
                .Select(Function(str)
                            Return str.PadRight(maxLen * 1.25)
                        End Function) _
                .ToArray

            Return Me
        End Function

        ''' <summary>
        ''' set percentage for random drop out of the nodes in each layer
        ''' </summary>
        ''' <param name="percentage"></param>
        ''' <returns></returns>
        Public Function SetDropOut(percentage As Double) As ANNTrainer
            If percentage > 0 Then
                _dropOutRate = percentage

                For Each layer As Layer In network.HiddenLayer
                    layer.doDropOutMode = True
                Next
            End If

            Return Me
        End Function

        ''' <summary>
        ''' apply softmax normalization for each layer?
        ''' </summary>
        ''' <param name="opt"></param>
        Public Function SetLayerNormalize(opt As Boolean) As ANNTrainer
            For Each layer As Layer In network.HiddenLayer
                layer.softmaxNormalization = opt
            Next

            Return Me
        End Function

        ''' <summary>
        ''' 在这里添加训练使用的数据集
        ''' (请注意,因为ANN的output结果向量只输出``[0,1]``之间的结果,所以在训练的时候,output应该是被编码为0或者1的;
        ''' input可以接受任意实数的向量,但是这个要求所有属性都应该在同一个scale区间内,所以最好也归一化编码为0到1之间的小数)
        ''' </summary>
        ''' <param name="input"></param>
        ''' <param name="output"></param>
        Public Sub Add(input As Double(), ParamArray output As Double())
            dataSets += New TrainingSample With {
                .sample = input,
                .classify = output,
                .sampleID = App.NextTempName
            }
        End Sub

        Public Sub Add(x As Sample)
            dataSets += New TrainingSample With {
                .sampleID = x.ID,
                .classify = x.target,
                .sample = x.vector
            }
        End Sub

        ''' <summary>
        ''' 开始进行训练
        ''' </summary>
        ''' <param name="parallel">
        ''' 小型的人工神经网络的训练,并不建议使用并行化
        ''' </param>
        Public Overrides Sub Train(Optional parallel As Boolean = False)
            If TrainingType = TrainingType.Epoch Then
                Call Train(Helpers.MaxEpochs, parallel)
            Else
                Call Train(minimumError:=Helpers.MinimumError, parallel:=parallel)
            End If
        End Sub

        Protected MustOverride Sub SaveSnapshot()
        Protected MustOverride Function runTraining(parallel As Boolean) As Double()

#Region "-- Training --"
        Public Overloads Sub Train(numEpochs As Integer, Optional parallel As Boolean = False)
            Dim break As Value(Of Boolean) = False
            Dim cancelSignal As UserTaskCancelAction = Nothing
            Dim saveSignal As UserTaskSaveAction = Nothing

            If App.IsConsoleApp Then
                cancelSignal = New UserTaskCancelAction(
                    Sub()
                        Call "User cancel of the training loop...".debug
                        break.Value = True
                    End Sub)
                saveSignal = New UserTaskSaveAction(AddressOf SaveSnapshot)
            End If

            Call "training ANN network...".info
            Call TrainInternal(numEpochs, parallel, break)

            If Not cancelSignal Is Nothing Then
                Call cancelSignal.Dispose()
            End If
            If Not saveSignal Is Nothing Then
                Call saveSignal.Dispose()
            End If
        End Sub

        Private Sub TrainInternal(numEpochs As Integer, parallel As Boolean, break As Value(Of Boolean))
            Dim muErr As Double
            Dim bar As Tqdm.ProgressBar = Nothing

            For Each i As Integer In Tqdm.Range(0, numEpochs, bar:=bar)
                errors = runTraining(parallel)
                muErr = errors.Average

                Call bar.SetLabel($"iterations: [{i}/{numEpochs}], errors={muErr.ToString("G3")}{vbTab}learn_rate={network.LearnRate}")

                If muErr < ErrorThreshold Then
                    Exit For
                ElseIf muErr < ErrorThreshold * 2 Then
                    Selective = False
                Else

                End If

                If Not reporter Is Nothing Then
                    Call reporter(i, muErr, network)
                End If
                If break.Value Then
                    Exit For
                End If
            Next
        End Sub

        ''' <summary>
        ''' 在这个函数之中实现一次训练循环过程
        ''' </summary>
        ''' <param name="dataSets"></param>
        ''' <param name="parallel"></param>
        ''' <param name="selective"></param>
        ''' <returns>函数返回每一个output的误差值</returns>
        Friend Shared Function trainingImpl(network As Network,
                                            dataSets As TrainingSample(),
                                            parallel As Boolean,
                                            selective As Boolean,
                                            dropoutRate As Double,
                                            backPropagate As Boolean) As Double()

            Dim errors As New List(Of Double())()
            Dim err#()
            Dim outputSize% = dataSets(Scan0).classify.Length

            If dropoutRate > 0 Then
                Call network.DoDropOut(percentage:=dropoutRate)
            End If

            For Each dataSet As TrainingSample In dataSets
                If selective Then
                    ' 如果是只针对误差较大的训练样本进行训练的话,则在这里会首先计算
                    ' 当前的训练样本的误差,如果当前的训练样本误差比较小的话,就
                    ' 不再进行当前的样本的训练了
                    ' sum
                    err = CalculateError(network, dataSet.classify)
                    ' means
                    If errorSum(err) / outputSize <= 0.05 Then
                        ' skip current sample
                        Call errors.Add(err)
                        Continue For
                    End If
                End If

                ' 下面的两步代码调用完成一个样本的训练操作:
                ' 首先根据当前样本进行计算
                ' 然后根据误差调整响应节点的权重
                Call network.ForwardPropagate(dataSet.sample, parallel)

                If backPropagate Then
                    Call network.BackPropagate(dataSet.classify, parallel)
                End If

                Call errors.Add(CalculateError(network, dataSet.classify))
            Next

            Dim errs As Double() = New Double(outputSize - 1) {}
            Dim j As Integer

            For i As Integer = 0 To outputSize - 1
                j = i
                errs(i) = errors.Select(Function(a) a(j)).Average
            Next

            Return errs
        End Function

        Private Shared Function errorSum(errs As Double()) As Double
            Dim err As Double = errs.Sum

            Const maxErr# = 10 ^ 255

            If err.IsNaNImaginary Then
                Return maxErr
            Else
                Return err
            End If
        End Function

        Public Overloads Sub Train(minimumError As Double, Optional parallel As Boolean = False)
            Dim [error] = 1.0
            Dim numEpochs = 0
            Dim progress$
            Dim break As Boolean = False
            Dim cancelSignal As UserTaskCancelAction = Nothing

            If App.IsConsoleApp Then
                cancelSignal = New UserTaskCancelAction(
                        Sub()
                            Call "User cancel of the training loop...".debug
                            break = True
                        End Sub)
            End If

            While [error] > minimumError AndAlso numEpochs < Integer.MaxValue
                errors = runTraining(parallel)
                [error] = [errors].Average

                numEpochs += 1
                progress = ((minimumError / [error]) * 100).ToString("F2")

                If outputNames.IsNullOrEmpty Then
                    Call $"{numEpochs}{ASCII.TAB}Error:=[{[errors].Select(Function(a) a.ToString("F3")).JoinBy(", ")}]{ASCII.TAB}progress:={progress}%".debug
                Else
                    Call $"{numEpochs}{ASCII.TAB}Error:=[{[errors].Average}]{ASCII.TAB}progress:={progress}%".debug

                    For i As Integer = 0 To outputNames.Length - 1
                        Call $"    {outputNames(i)}={errors(i)}".info
                    Next
                End If

                If Not reporter Is Nothing Then
                    Call reporter(numEpochs, [error], network)
                End If
            End While

            If Not cancelSignal Is Nothing Then
                Call cancelSignal.Dispose()
            End If
        End Sub

        ''' <summary>
        ''' 预测值与目标值之间的误差的绝对值之和
        ''' </summary>
        ''' <param name="neuronNetwork"></param>
        ''' <param name="targets"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Shared Function CalculateError(neuronNetwork As Network, targets As Double()) As Double()
            Dim err#() = neuronNetwork.OutputLayer _
                .Neurons _
                .Select(Function(n, i)
                            Return stdNum.Abs(n.CalculateError(targets(i)))
                        End Function) _
                .ToArray

            Return err
        End Function
#End Region

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="input">The inputs data</param>
        ''' <param name="convertedResults">The error outputs</param>
        ''' <param name="expectedResults">The corrects output</param>
        Public Sub Corrects(input As Double(), convertedResults As Double(), expectedResults As Double(),
                            Optional train As Boolean = True,
                            Optional parallel As Boolean = False)

            Dim offendingDataSet As TrainingSample = dataSets _
                .FirstOrDefault(Function(x)
                                    Return x.sample.SequenceEqual(input) AndAlso x.classify.SequenceEqual(convertedResults)
                                End Function)

            If Not offendingDataSet.isEmpty Then
                dataSets.Remove(offendingDataSet)
            End If

            If Not dataSets.Exists(Function(x) x.sample.SequenceEqual(input) AndAlso x.classify.SequenceEqual(expectedResults)) Then
                dataSets += New TrainingSample With {
                    .sampleID = App.NextTempName,
                    .sample = input,
                    .classify = expectedResults
                }
            End If

            If train Then
                Call Me.Train(parallel)
            End If
        End Sub

        Public Sub Corrects(dataset As TrainingSample, expectedResults As Double(),
                            Optional train As Boolean = True,
                            Optional parallel As Boolean = False)
            Call Corrects(dataset.sample, dataset.classify, expectedResults, train, parallel)
        End Sub
    End Class
End Namespace

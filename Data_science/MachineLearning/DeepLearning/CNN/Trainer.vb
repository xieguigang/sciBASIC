Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.CNN.layers
Imports Microsoft.VisualBasic.MachineLearning.CNN.trainers
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
Imports Microsoft.VisualBasic.Math
Imports std = System.Math

Namespace CNN

    Public Class Trainer

        Dim log As Action(Of String) = AddressOf VBDebugger.EchoLine
        Dim alg As TrainerAlgorithm
        Dim is_generative As Boolean = False
        Dim verbose As Integer = 25

        <DebuggerStepThrough>
        Sub New(alg As TrainerAlgorithm, Optional log As Action(Of String) = Nothing, Optional verbose As Integer = 25)
            If Not log Is Nothing Then
                Me.log = log
            End If

            Me.alg = alg
            Me.verbose = verbose
        End Sub

        Private Sub TrainEpochs(trainset As SampleData(), epochsNum As Integer, ByRef right As Integer, ByRef count As Integer)
            Dim d As Integer = epochsNum / verbose
            Dim t0 = Now
            Dim randPerm As Integer()
            Dim ti As Date = Now
            Dim input As InputLayer = alg.conv_net.input
            Dim data As New DataBlock(input.dims.x, input.dims.y, input.out_depth, 0) With {.trace = Me.ToString}
            Dim tr As TrainResult = Nothing
            Dim img As SampleData
            Dim loss As New List(Of Double)
            Dim cpu As New PerformanceCounter
            Dim valid_loss As Double()
            Dim loss_sum, loss_mean As Double

            right = 0
            count = 0

            If d = 0 Then
                d = 1
            End If

            For i As Integer = 0 To epochsNum - 1
                randPerm = Util.randomPerm(trainset.Length, alg.batch_size)

                For Each index As Integer In randPerm
                    img = trainset(index)
                    ' the feature input data has already been scaled
                    data.addImageData(img.features, 1.0)
                    data.trace = $"input_data({img.id})"
                    tr = alg.train(data, img.labels, checkpoints:=cpu.Set)
                    loss += tr.Loss

                    If is_generative Then
                        If SIMD.Subtract.f64_op_subtract_f64(img.labels, alg.get_output) _
                            .Select(Function(dd) std.Abs(dd)) _
                            .Average < 0.01 Then

                            right += 1
                        End If
                    Else
                        If img.labels(0) = which.Max(alg.get_output) Then
                            right += 1
                        End If
                    End If

                    count += 1
                Next

                If i Mod d = 0 Then
                    valid_loss = loss.Where(Function(a) Not a.IsNaNImaginary).ToArray
                    loss_sum = valid_loss.Sum
                    loss_mean = loss_sum / (valid_loss.Length + 1)
                    log($"[{i + 1}/{epochsNum}; {(Now - ti).Lanudry}] {(i / epochsNum * 100).ToString("F1")}% [{valid_loss.Length}/{loss.Count}] mean_loss:{loss_mean}, total:{loss_sum}..... {(Now - t0).FormatTime(False)}")
                    ti = Now
                End If
            Next
        End Sub

        ''' <summary>
        ''' Run CNN trainer
        ''' </summary>
        ''' <param name="cnn"></param>
        ''' <param name="trainset"></param>
        ''' <param name="max_loops"></param>
        ''' <returns></returns>
        Public Function train(cnn As ConvolutionalNN, trainset As SampleData(), max_loops As Integer) As ConvolutionalNN
            Dim t As Integer = 0
            Dim stopTrain As Boolean
            Dim right = 0
            Dim count = 0

            is_generative = trainset(0).labels.Length > 1
            trainset = SampleData.TransformDataset(
                trainset:=trainset,
                is_generative:=is_generative,
                is_training:=True
            ).ToArray

            Call alg.SetKernel(cnn)

            While t < max_loops AndAlso Not stopTrain
                Dim epochsNum As Integer = trainset.Length / alg.batch_size

                If trainset.Length Mod alg.batch_size <> 0 Then
                    epochsNum += 1
                End If

                Call log("")
                Call log(t.ToString() & "th iter epochsNum: " & epochsNum.ToString())
                Call TrainEpochs(trainset, epochsNum, right, count)
                Call log("precision " & right.ToString() & "/" & count.ToString() & $"={(100 * right / count).ToString("F2")}%")

                t += 1
            End While

            Return cnn
        End Function
    End Class
End Namespace
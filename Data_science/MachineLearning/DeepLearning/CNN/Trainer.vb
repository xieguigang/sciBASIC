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
        Dim global_featureMax As Double
        Dim global_generativeMax As Double = Double.NegativeInfinity
        Dim is_generative As Boolean = False

        <DebuggerStepThrough>
        Sub New(alg As TrainerAlgorithm, Optional log As Action(Of String) = Nothing)
            If Not log Is Nothing Then
                Me.log = log
            End If

            Me.alg = alg
        End Sub

        Private Sub TrainEpochs(trainset As SampleData(), epochsNum As Integer, ByRef right As Integer, ByRef count As Integer)
            Dim d As Integer = epochsNum / 25
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
                    data.addImageData(img.features, global_featureMax)
                    data.trace = $"input_data({img.id})"
                    tr = alg.train(data, img.labels, checkpoints:=cpu.Set)
                    loss += tr.Loss

                    If is_generative Then
                        If SIMD.Subtract.f64_op_subtract_f64(img.labels, alg.get_output).Select(Function(dd) std.Abs(dd)).Average < 0.01 Then
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
                    log($"[{i + 1}/{epochsNum}; {(Now - ti).Lanudry}] {(i / epochsNum * 100).ToString("F1")}% mean_loss:{loss_mean}, total:{loss_sum}...... {(Now - t0).FormatTime(False)}")
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

            global_featureMax = Double.MinValue
            global_generativeMax = Double.MinValue
            is_generative = trainset(0).labels.Length > 1

            For Each i As SampleData In trainset
                Dim max As Double = i.features.Max

                If max > global_featureMax Then
                    global_featureMax = max
                End If

                If is_generative Then
                    max = i.labels.Max

                    If max > global_generativeMax Then
                        global_generativeMax = max
                    End If
                End If
            Next

            If is_generative Then
                For Each i As SampleData In trainset
                    For idx As Integer = 0 To i.labels.Length - 1
                        i.labels(idx) /= global_generativeMax
                    Next
                Next
            End If

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
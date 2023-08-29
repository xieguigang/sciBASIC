Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.CNN.layers
Imports Microsoft.VisualBasic.MachineLearning.CNN.trainers
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure

Namespace CNN

    Public Class Trainer

        Dim log As Action(Of String) = AddressOf VBDebugger.EchoLine
        Dim alg As TrainerAlgorithm

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
            Dim ti As Date
            Dim input As InputLayer = alg.conv_net.input
            Dim data As New DataBlock(input.dims.x, input.dims.y, 1, 0)
            Dim tr As TrainResult = Nothing
            Dim img As SampleData
            Dim loss As New List(Of Double)

            right = 0
            count = 0

            If d = 0 Then
                d = 1
            End If

            For i As Integer = 0 To epochsNum - 1
                randPerm = Util.randomPerm(trainset.Length, alg.batch_size)

                For Each index As Integer In randPerm
                    img = trainset(index)
                    data.addImageData(img.features, img.features.Max)
                    tr = alg.train(data, img.labels(0))
                    loss += tr.Loss

                    If img.labels(0) = which.Max(alg.conv_net.output.OutAct.Weights) Then
                        right += 1
                    End If

                    count += 1
                Next

                If i Mod d = 0 Then
                    log($"[{i + 1}/{epochsNum};  {(Now - ti).Lanudry}] {(i / epochsNum * 100).ToString("F1")}% ...... {(Now - t0).FormatTime(False)}")
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
#Region "Microsoft.VisualBasic::609393eaa0dd28bad19de979fecfefde, Data_science\MachineLearning\DeepLearning\CNN\Trainer.vb"

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

    '   Total Lines: 141
    '    Code Lines: 109
    ' Comment Lines: 8
    '   Blank Lines: 24
    '     File Size: 5.55 KB


    '     Class Trainer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: train
    ' 
    '         Sub: TrainEpochs
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
        Dim action As Action(Of Integer, ConvolutionalNN)
        Dim precision_cutoff As Double = 0.05

        <DebuggerStepThrough>
        Sub New(alg As TrainerAlgorithm,
                Optional log As Action(Of String) = Nothing,
                Optional action As Action(Of Integer, ConvolutionalNN) = Nothing,
                Optional verbose As Integer = 25)

            If Not log Is Nothing Then
                Me.log = log
            End If

            Me.alg = alg
            Me.verbose = verbose
            Me.action = action
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
            Dim mean_errors As Double

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
                        mean_errors = SIMD.Subtract.f64_op_subtract_f64(img.labels, alg.get_output) _
                            .Select(Function(dd) std.Abs(dd)) _
                            .Average

                        If mean_errors < precision_cutoff Then
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
                    log($"[{i + 1}/{epochsNum} {(Now - ti).Lanudry}] {(i / epochsNum * 100).ToString("F1")}% [{valid_loss.Length}/{loss.Count}] mean_loss:{loss_mean.ToString("G3")}, total:{loss_sum.ToString("G4")}, mean_errors={mean_errors.ToString("F5")}..... {(Now - t0).FormatTime(False)}")
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
                Call log("(" & t.ToString() & $"th/{max_loops}) iter epochsNum: " & epochsNum.ToString())
                Call TrainEpochs(trainset, epochsNum, right, count)
                Call log("precision " & right.ToString() & "/" & count.ToString() & $"={(100 * right / count).ToString("F2")}%")

                If Not action Is Nothing Then
                    Call action(t, cnn)
                End If

                t += 1
            End While

            Return cnn
        End Function
    End Class
End Namespace

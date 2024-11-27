#Region "Microsoft.VisualBasic::505a3cfa68b0d35bd9c5ebf9ba16f98e, Data_science\MachineLearning\DeepLearning\CNN\Trainer.vb"

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

    '   Total Lines: 158
    '    Code Lines: 123 (77.85%)
    ' Comment Lines: 8 (5.06%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 27 (17.09%)
    '     File Size: 6.49 KB


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
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.CNN.layers
Imports Microsoft.VisualBasic.MachineLearning.CNN.trainers
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Serialization.JSON
Imports std = System.Math

Namespace CNN

    Public Class Trainer

        Dim log As Action(Of String) = AddressOf VBDebugger.EchoLine
        Dim alg As TrainerAlgorithm
        Dim is_generative As Boolean = False
        Dim verbose As Boolean
        Dim action As Action(Of Integer, ConvolutionalNN)
        Dim precision_cutoff As Double = 0.05

        <DebuggerStepThrough>
        Sub New(alg As TrainerAlgorithm,
                Optional log As Action(Of String) = Nothing,
                Optional action As Action(Of Integer, ConvolutionalNN) = Nothing,
                Optional verbose As Boolean = True)

            If Not log Is Nothing Then
                Me.log = log
            End If

            Me.alg = alg
            Me.verbose = verbose
            Me.action = action
        End Sub

        Private Sub TrainEpochs(trainset As SampleData(), epochsNum As Integer, ByRef right As Integer, ByRef count As Integer)
            Dim randPerm As Integer()
            Dim input As InputLayer = alg.conv_net.input
            Dim data As New DataBlock(input.dims.x, input.dims.y, input.out_depth, 0) With {.trace = Me.ToString}
            Dim tr As TrainResult = Nothing
            Dim img As SampleData
            Dim loss As New List(Of Double)
            Dim cpu As New PerformanceCounter
            Dim valid_loss As Double()
            Dim loss_sum, loss_mean As Double
            Dim mean_errors As Double
            Dim progress As Tqdm.ProgressBar = Nothing
            Dim report_txt As String

            right = 0
            count = 0

            For Each i As Integer In Tqdm.Range(0, epochsNum, bar:=progress)
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

                If verbose Then
                    valid_loss = loss.AsParallel.Where(Function(a) Not a.IsNaNImaginary).ToArray
                    loss_sum = valid_loss.Sum
                    loss_mean = loss_sum / (valid_loss.Length + 1)
                    report_txt = $"[{valid_loss.Length}/{loss.Count}] mean_loss:{loss_mean.ToString("G3")} total:{loss_sum.ToString("G4")} mean_errors={mean_errors.ToString("F5")}"
                    progress.SetLabel(report_txt)
                End If
            Next
        End Sub

        ''' <summary>
        ''' Run CNN trainer
        ''' </summary>
        ''' <param name="trainset"></param>
        ''' <param name="max_loops"></param>
        ''' <returns></returns>
        Public Function train(trainset As SampleData(), max_loops As Integer) As ConvolutionalNN
            Dim t As Integer = 0
            Dim stopTrain As Boolean
            Dim right = 0
            Dim count = 0

            is_generative = trainset(0).labels.Length > 1

            ' make dataset normalization
            trainset = SampleData.TransformDataset(
                trainset:=trainset,
                is_generative:=is_generative,
                is_training:=True
            ).ToArray

            Dim checkInvalids As SampleData() = trainset _
                .AsParallel _
                .Where(Function(d) d.CheckInvalidNaN) _
                .ToArray

            If Not checkInvalids.IsNullOrEmpty Then
                Dim message As String = $"There are {checkInvalids.Length} invalid NaN samples data was found: {checkInvalids.Select(Function(d) d.id).GetJson}, these invalid sample data will be ignored in the training steps!"

                Call message.Warning
                Call VBDebugger.EchoLine(message)

                trainset = trainset _
                    .AsParallel _
                    .Where(Function(d) Not d.CheckInvalidNaN) _
                    .ToArray
            End If
            If alg.conv_net Is Nothing Then
                Throw New InvalidProgramException("no neuron network model inside the trainer algorithm module, call SetKernel method before call this train method!")
            End If

            While t < max_loops AndAlso Not stopTrain
                Dim epochsNum As Integer = trainset.Length / alg.batch_size

                If trainset.Length Mod alg.batch_size <> 0 Then
                    epochsNum += 1
                End If

                Call log("")
                Call log("(" & t.ToString() & $"th/{max_loops}) iter epochsNum: " & epochsNum.ToString())
                Call TrainEpochs(trainset, epochsNum, right, count)
                Call log("")
                Call log("precision " & right.ToString() & "/" & count.ToString() & $"={(100 * right / count).ToString("F2")}%")

                If Not action Is Nothing Then
                    Call action(t, alg.conv_net)
                End If

                t += 1
            End While

            Return alg.conv_net
        End Function
    End Class
End Namespace

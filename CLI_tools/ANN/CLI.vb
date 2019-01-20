#Region "Microsoft.VisualBasic::748308d6cc55dd5c318770db251e36f0, CLI_tools\ANN\CLI.vb"

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

    ' Module CLI
    ' 
    '     Function: ConfigTemplate, Encourage, Train
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Settings.Inf
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Accelerator
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure

Module CLI

    <ExportAPI("/config.template")>
    <Usage("/config.template [/save <default=./config.ini>]")>
    Public Function ConfigTemplate(args As CommandLine) As Integer
        Return New Config().WriteProfile(args("/save") Or "./config.ini")
    End Function

    ''' <summary>
    ''' 这个函数会输出训练好的模型, 训练集测试结果, 错误率变化曲线图, 训练日志
    ''' 配置文件不存在的画，则使用默认的配置数据
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <ExportAPI("/training")>
    <Usage("/training /samples <sample_matrix.Xml> [/config <config.ini> /parallel /GA.optimize /out <ANN.Xml>]")>
    Public Function Train(args As CommandLine) As Integer
        Dim in$ = args <= "/samples"
        Dim parallel As Boolean = args("/parallel")
        Dim out$ = args("/out") Or $"{[in].TrimSuffix}_ANN_trained.Xml"
        Dim samples As DataSet = [in].LoadXml(Of DataSet)
        Dim config As Config = (args <= "/config").LoadProfile(Of Config) Or Config.Default
        Dim hiddenSize As Integer()

        If config.hidden_size.StringEmpty Then
            hiddenSize = {
                CInt(samples.Size.Width * 1.25),
                CInt((samples.OutputSize + samples.Size.Width) / 2),
                CInt(samples.OutputSize * 1.25)
            }
        Else
            hiddenSize = config.hidden_size _
                .Split(","c) _
                .Select(Function(s) CInt(Val(s))) _
                .ToArray
        End If

        Dim trainingHelper As New TrainingUtils(
            samples.Size.Width, hiddenSize,
            samples.OutputSize,
            config.learnRate,
            config.momentum
        )

        For Each sample As Sample In samples.PopulateNormalizedSamples
            Call trainingHelper.Add(sample.status, sample.target)
        Next

        Helpers.MaxEpochs = config.iterations

        Call Console.WriteLine(trainingHelper.NeuronNetwork.ToString)

        If Not args("/GA.optimize").IsTrue Then
            Using log As StreamWriter = $"{out.TrimSuffix}.log".OpenWriter
                Dim synapses = trainingHelper.NeuronNetwork.GetSynapseGroups

                Call log.WriteLine(synapses.Keys.JoinBy(vbCrLf))
                Call trainingHelper _
                    .AttachReporter(Sub(i, e, g)
                                        Call $"[{i}] errors={e}, learn={g.LearnRate}".__DEBUG_ECHO
                                        Call log.WriteLine((New Double() {i, e, g.LearnRate}.AsList + synapses.Select(Function(s) s.First.Weight)).JoinBy(vbTab))
                                    End Sub) _
                    .Train(parallel)
            End Using
        Else
            Call trainingHelper _
                .NeuronNetwork _
                .RunGAAccelerator(
                    trainingSet:=trainingHelper.TrainingSet,
                    iterations:=config.iterations
                 )
        End If

        Return trainingHelper _
            .TakeSnapshot _
            .GetXml _
            .SaveTo(out) _
            .CLICode
    End Function

    ''' <summary>
    ''' 使用测试训练数据集继续训练人工神经网络模型
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("/encourage")>
    <Usage("/encourage /model <ANN.xml> /samples <samples.Xml> [/parallel /iterations <default=10000> /out <out.Xml>]")>
    Public Function Encourage(args As CommandLine) As Integer
        Dim in$ = args <= "/model"
        Dim samples$ = args <= "/samples"
        Dim out$ = args("/out") Or $"{[in].TrimSuffix}.encouraged.Xml"
        Dim parallel As Boolean = args("/parallel")
        Dim network As Network = [in].LoadXml(Of NeuralNetwork).LoadModel
        Dim training As New TrainingUtils(network)
        Dim logs$ = out.TrimSuffix & ".logs/"

        Helpers.MaxEpochs = args("/iterations") Or 10000

        For Each sample As Sample In samples.LoadXml(Of DataSet).PopulateNormalizedSamples
            Call training.Add(sample.status, sample.target)
        Next

        Dim synapses = training _
            .NeuronNetwork _
            .GetSynapseGroups _
            .Select(Function(g) g.First) _
            .ToArray
        Dim synapsesWeights As New Dictionary(Of String, List(Of Double))
        Dim errors As New List(Of Double)
        Dim index As New List(Of Integer)

        For Each s In synapses
            synapsesWeights.Add(s.ToString, New List(Of Double))
        Next

        Call Console.WriteLine(network.ToString)
        Call training _
            .AttachReporter(Sub(i, err, model)
                                Call index.Add(i)
                                Call errors.Add(err)
                                Call synapses.DoEach(Sub(s) synapsesWeights(s.ToString).Add(s.Weight))
                            End Sub) _
            .Train(parallel)
        Call Debugger.WriteCDF(network, out.TrimSuffix & ".debugger.CDF", synapses, errors, index, synapsesWeights)

        Return training.TakeSnapshot _
            .GetXml _
            .SaveTo(out) _
            .CLICode
    End Function
End Module

#Region "Microsoft.VisualBasic::ce75948f50eb07f3958071402aa033b0, CLI_tools\ANN\CLI.vb"

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
'     Function: ConfigTemplate, Encourage, runTrainingCommon, Train
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Settings.Inf
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Accelerator
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure
Imports Microsoft.VisualBasic.MIME.application.netCDF
Imports DataFrame = Microsoft.VisualBasic.Data.csv.IO.DataFrame
Imports VisualBasic = Microsoft.VisualBasic.Language.Runtime

Module CLI

    <ExportAPI("/Summary.Debugger.Dump")>
    <Usage("/Summary.Debugger.Dump /in <debugger_out.cdf>")>
    Public Sub SummaryDebuggerDump(args As CommandLine)
        Call New netCDFReader(args <= "/in").Print()
    End Sub

    ''' <summary>
    ''' 导出误差率曲线数据
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("/Export.Errors.Curve")>
    <Usage("/Export.Errors.Curve /in <debugger_out.cdf> [/out <errors.csv>]")>
    Public Function ExportErrorCurve(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim out$ = args("/out") Or $"{[in].TrimSuffix}.errors.csv"
        Dim cdf As New netCDFReader([in])
        Dim errors = cdf.getDataVariable("fitness").numerics
        Dim index = cdf.getDataVariable("iterations").integers

        With New VisualBasic
            Return New DataFrame(!iterations = index, !fitness = errors) _
                .csv _
                .Save(out) _
                .CLICode
        End With
    End Function

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

        Dim defaultActive As DefaultValue(Of String) = config.default_active Or ActiveFunction.Sigmoid
        Dim actives As New Activations.LayerActives With {
            .hiddens = ActiveFunction.Parse(config.hiddens_active Or defaultActive),
            .input = ActiveFunction.Parse(config.input_active Or defaultActive),
            .output = ActiveFunction.Parse(config.output_active Or defaultActive)
        }
        Dim trainingHelper As New TrainingUtils(
            samples.Size.Width, hiddenSize,
            samples.OutputSize,
            config.learnRate,
            config.momentum,
            actives
        )

        trainingHelper.NeuronNetwork.LearnRateDecay = config.learnRateDecay

        For Each sample As Sample In samples.PopulateNormalizedSamples
            Call trainingHelper.Add(sample.status, sample.target)
        Next

        Helpers.MaxEpochs = config.iterations

        Call Console.WriteLine(trainingHelper.NeuronNetwork.ToString)

        If Not args("/GA.optimize").IsTrue Then
            Call trainingHelper.runTrainingCommon(out.TrimSuffix & ".debugger.CDF", parallel)
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

    <Extension>
    Private Function runTrainingCommon(trainer As TrainingUtils, debugCDF$, parallel As Boolean) As TrainingUtils
        Dim synapses = trainer _
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

        Call Console.WriteLine(trainer.NeuronNetwork.ToString)
        Call trainer _
            .AttachReporter(Sub(i, err, model)
                                Call index.Add(i)
                                Call errors.Add(err)
                                Call synapses.DoEach(Sub(s) synapsesWeights(s.ToString).Add(s.Weight))
                            End Sub) _
            .Train(parallel)

        Call Debugger.WriteCDF(trainer.NeuronNetwork, debugCDF, synapses, errors, index, synapsesWeights)

        Return trainer
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

        Helpers.MaxEpochs = args("/iterations") Or 10000

        For Each sample As Sample In samples.LoadXml(Of DataSet).PopulateNormalizedSamples
            Call training.Add(sample.status, sample.target)
        Next

        Return training _
            .runTrainingCommon(out.TrimSuffix & ".debugger.CDF", parallel) _
            .TakeSnapshot _
            .GetXml _
            .SaveTo(out) _
            .CLICode
    End Function
End Module

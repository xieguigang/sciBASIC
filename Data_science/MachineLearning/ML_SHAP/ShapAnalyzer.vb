#Region "Microsoft.VisualBasic::7bb3c3396ce3eefeef1ae4d5a17f57a2, Data_science\MachineLearning\ML_SHAP\ShapAnalyzer.vb"

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

'   Total Lines: 144
'    Code Lines: 73 (50.69%)
' Comment Lines: 45 (31.25%)
'    - Xml Docs: 100.00%
' 
'   Blank Lines: 26 (18.06%)
'     File Size: 5.31 KB


' Class ShapAnalysisResult
' 
'     Properties: Baseline, DatasetName, Explanations, FeatureNames, IsClassification
'                 MeanAbsShap, MeanShap, ModelName
' 
'     Function: GlobalImportance
' 
' Module ShapAnalyzer
' 
'     Function: Analyze
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Statistics.ShapleyValue

''' <summary>
''' SHAP 分析的门面：把任意 <see cref="IShapExplainer"/> 与数据集组合起来，
''' 计算逐样本的 SHAP 矩阵以及全局特征重要性。
''' </summary>
Public Module ShapAnalyzer

    ''' <summary>
    ''' 对数据集之中的样本执行 SHAP 分析。
    ''' </summary>
    ''' <param name="explainer">已经针对某个模型构建好的 SHAP 解释器</param>
    ''' <param name="dataset">用于计算 SHAP 的样本数据</param>
    ''' <param name="modelName">模型名字（用于报告）</param>
    ''' <param name="maxSamples">
    ''' 最多解释的样本数量，小于等于 0 表示解释全部样本
    ''' </param>
    ''' <param name="output">
    ''' 可选的模型输出函数，用于记录模型的原始输出
    ''' （对于逻辑回归是类别概率）。为空时使用加性值。
    ''' </param>
    ''' <returns></returns>
    Public Function Analyze(explainer As IShapExplainer,
                            dataset As ShapDataset,
                            modelName As String,
                            Optional maxSamples As Integer = 0,
                            Optional output As Func(Of Double(), Double) = Nothing) As ShapAnalysisResult

        Dim count As Integer = dataset.Size

        If maxSamples > 0 AndAlso maxSamples < count Then
            count = maxSamples
        End If

        Dim width As Integer = dataset.Width
        Dim explanations As New List(Of ShapExplanation)()
        Dim absoluteSum As Double() = New Double(width - 1) {}
        Dim signedSum As Double() = New Double(width - 1) {}

        For i As Integer = 0 To count - 1
            Dim x As Double() = dataset.Features(i)
            Dim contributions As Double() = explainer.Explain(x)
            Dim additive As Double = explainer.Baseline

            For j As Integer = 0 To width - 1
                Dim value As Double = If(j < contributions.Length, contributions(j), 0)

                absoluteSum(j) += System.Math.Abs(value)
                signedSum(j) += value
                additive += value
            Next

            Dim rawOutput As Double = additive

            If output IsNot Nothing Then
                rawOutput = output(x)
            End If

            explanations.Add(New ShapExplanation With {
                .ID = dataset.IDs(i),
                .FeatureNames = dataset.FeatureNames,
                .Contributions = contributions,
                .Baseline = explainer.Baseline,
                .AdditiveValue = additive,
                .Output = rawOutput
            })
        Next

        Dim normalizer As Double = System.Math.Max(1, explanations.Count)

        Return New ShapAnalysisResult With {
            .ModelName = modelName,
            .DatasetName = dataset.Name,
            .IsClassification = dataset.IsCategorical,
            .FeatureNames = dataset.FeatureNames,
            .Baseline = explainer.Baseline,
            .Explanations = explanations,
            .MeanAbsShap = absoluteSum.Select(Function(v) v / normalizer).ToArray,
            .MeanShap = signedSum.Select(Function(v) v / normalizer).ToArray
        }
    End Function

End Module

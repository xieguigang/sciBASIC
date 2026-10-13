#Region "Microsoft.VisualBasic::010f7c69979777920437784421cfa7e7, Data_science\DataMining\DataMining\Evaluation\ModelEvaluation.vb"

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

    '   Total Lines: 67
    '    Code Lines: 25 (37.31%)
    ' Comment Lines: 32 (47.76%)
    '    - Xml Docs: 96.88%
    ' 
    '   Blank Lines: 10 (14.93%)
    '     File Size: 2.70 KB


    '     Module ModelEvaluation
    ' 
    '         Function: (+4 Overloads) Evaluate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Evaluation

    ''' <summary>
    ''' 统一评估框架的入口（Facade）。
    ''' 
    ''' 对「机器学习分类结果 / 回归结果 / 聚类结果」三类结果数据，调用
    ''' <see cref="Evaluate(ClassificationResult, String)"/> 等重载即可得到统一的
    ''' <see cref="EvaluationReport"/>，其中包含该结果种类下所有已注册指标
    ''' （参见 <see cref="MetricRegistry"/>）以及可选的 ROC 曲线。
    ''' </summary>
    Public Module ModelEvaluation

        ''' <summary>
        ''' 评估机器学习分类结果。
        ''' </summary>
        ''' <param name="result"></param>
        ''' <param name="name">可选的结果名字，覆盖 <see cref="ClassificationResult.Name"/></param>
        ''' <returns></returns>
        Public Function Evaluate(result As ClassificationResult, Optional name As String = Nothing) As EvaluationReport
            If Not String.IsNullOrEmpty(name) Then
                result.Name = name
            End If

            Return MetricRegistry.Compute(result)
        End Function

        ''' <summary>
        ''' 评估回归结果。
        ''' </summary>
        ''' <param name="result"></param>
        ''' <param name="name">可选的结果名字，覆盖 <see cref="RegressionResult.Name"/></param>
        ''' <returns></returns>
        Public Function Evaluate(result As RegressionResult, Optional name As String = Nothing) As EvaluationReport
            If Not String.IsNullOrEmpty(name) Then
                result.Name = name
            End If

            Return MetricRegistry.Compute(result)
        End Function

        ''' <summary>
        ''' 评估聚类结果。
        ''' </summary>
        ''' <param name="result"></param>
        ''' <param name="name">可选的结果名字，覆盖 <see cref="ClusteringResult.Name"/></param>
        ''' <returns></returns>
        Public Function Evaluate(result As ClusteringResult, Optional name As String = Nothing) As EvaluationReport
            If Not String.IsNullOrEmpty(name) Then
                result.Name = name
            End If

            Return MetricRegistry.Compute(result)
        End Function

        ''' <summary>
        ''' 评估任意实现了 <see cref="IEvaluationResult"/> 的结果数据
        ''' （例如用户自定义的结果类型）。
        ''' </summary>
        ''' <param name="result"></param>
        ''' <returns></returns>
        Public Function Evaluate(result As IEvaluationResult) As EvaluationReport
            Return MetricRegistry.Compute(result)
        End Function

    End Module

End Namespace

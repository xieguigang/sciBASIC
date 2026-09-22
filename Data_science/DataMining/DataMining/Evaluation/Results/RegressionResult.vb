#Region "Microsoft.VisualBasic::, Data_science\DataMining\DataMining\Evaluation\Results\RegressionResult.vb"

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

'   Total Lines: 0
'    Code Lines: 0 (NaN%)
' Comment Lines: 0 (NaN%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 0 (NaN%)
'     File Size: 0 B


' /********************************************************************************/

#End Region

Imports System.Linq
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model

Namespace Evaluation

    ''' <summary>
    ''' 回归结果的统一数据模型：``(预测值, 连续真值)``。
    ''' 
    ''' 回归模型同样可以评估 ROC/AUC：通过把连续真值按照阈值切分为二分类问题
    ''' （参见 <see cref="RegressionROC"/>），这正是原先 <see cref="RegressionClassify"/>
    ''' 所做的事情，现在统一由本类型承载。
    ''' </summary>
    Public Class RegressionResult : Implements IEvaluationResult
        Implements IRocResult

        Public Property Name As String Implements IEvaluationResult.Name

        ''' <summary>
        ''' 模型的回归预测值。
        ''' </summary>
        ''' <returns></returns>
        Public Property Predictions As Double()

        ''' <summary>
        ''' 连续的观测真值。
        ''' </summary>
        ''' <returns></returns>
        Public Property Actuals As Double()

        ''' <summary>
        ''' 判定一个预测是否「命中」真值的误差容差，默认 0.1。
        ''' </summary>
        ''' <returns></returns>
        Public Property Eps As Double = 0.1

        ''' <summary>
        ''' ROC 阈值扫描的步数，默认 25。
        ''' </summary>
        ''' <returns></returns>
        Public Property SweepSteps As Integer = 25

        ''' <summary>
        ''' 真值的取值范围（为空时自动从 <see cref="Actuals"/> 推断）。
        ''' </summary>
        ''' <returns></returns>
        Public Property LabelRange As DoubleRange

        Private _curve As RocCurve

        Public ReadOnly Property Kind As ResultKinds Implements IEvaluationResult.Kind
            Get
                Return ResultKinds.regression
            End Get
        End Property

        Public ReadOnly Property Size As Integer
            Get
                Return If(Predictions Is Nothing, 0, Predictions.Length)
            End Get
        End Property

        Public Shared Function Create(predictions As Double(),
                                      actuals As Double(),
                                      Optional name As String = Nothing,
                                      Optional eps As Double = 0.1,
                                      Optional sweepSteps As Integer = 25) As RegressionResult

            If predictions Is Nothing OrElse actuals Is Nothing Then
                Throw New ArgumentNullException("the predictions and actuals vectors can not be nothing!")
            End If

            If predictions.Length <> actuals.Length Then
                Throw New DataMisalignedException($"the predictions({predictions.Length}) and actuals({actuals.Length}) vectors must have the same length!")
            End If

            Return New RegressionResult With {
                .Name = If(String.IsNullOrEmpty(name), "regression", name),
                .Predictions = predictions,
                .Actuals = actuals,
                .Eps = eps,
                .SweepSteps = sweepSteps
            }
        End Function

        ''' <summary>
        ''' 构建回归结果的 ROC 曲线（统一委托到 <see cref="RegressionROC"/>，其内部再使用
        ''' <see cref="RocBuilder.SweepThresholds(Of T)"/>）。
        ''' </summary>
        ''' <returns></returns>
        Public Function Curve() As RocCurve Implements IRocResult.Curve
            If _curve Is Nothing Then
                _curve = RocCurve.Create(RegressionROC.ROC(Predictions, Actuals, LabelRange, Eps, SweepSteps))
            End If

            Return _curve
        End Function

        ''' <summary>
        ''' 把当前结果转换为逐样本的 <see cref="RegressionClassify"/> 序列（兼容旧接口）。
        ''' </summary>
        ''' <returns></returns>
        Public Function ToClassify() As RegressionClassify()
            Return Predictions _
                .Select(Function(fx, i)
                            Return New RegressionClassify With {
                                .predicts = fx,
                                .actual = Actuals(i),
                                .sampleID = $"v_{i + 1}"
                            }
                        End Function) _
                .ToArray
        End Function

    End Class

End Namespace


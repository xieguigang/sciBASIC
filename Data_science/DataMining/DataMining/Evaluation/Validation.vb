#Region "Microsoft.VisualBasic::3e339734d11457ad2a405bbd3fd625f6, Data_science\DataMining\DataMining\Evaluation\Validation.vb"

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

    '   Total Lines: 261
    '    Code Lines: 136 (52.11%)
    ' Comment Lines: 93 (35.63%)
    '    - Xml Docs: 87.10%
    ' 
    '   Blank Lines: 32 (12.26%)
    '     File Size: 10.51 KB


    '     Structure Validation
    ' 
    '         Properties: F1Score, FbetaScore, FPR, NPV
    ' 
    '         Function: AUC, Calc, FromConfusion, ROC, ToDataSet
    '                   ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace Evaluation

    ''' <summary>
    ''' 验证结果描述：ROC 曲线之上的一个阈值点。
    ''' 
    ''' ``灵敏度 = 真阳性人数 / (真阳性人数 + 假阴性人数)``
    ''' ``特异度 = 真阴性人数 / (真阴性人数 + 假阳性人数)``
    ''' 
    ''' 注意：本类型是整个 <see cref="Evaluation"/> 模块的**规范 ROC 点**，
    ''' 其中 <see cref="Sensibility"/> / <see cref="Specificity"/> / <see cref="Accuracy"/> /
    ''' <see cref="Precision"/> 等所有比率字段统一采用 ``[0, 1]`` 的**分数**表示
    ''' （旧版本使用的是 ``[0, 100]`` 的百分数，已在本轮重构中规范化）。
    ''' </summary>
    ''' <remarks>
    ''' https://www.jianshu.com/p/f0c7c1ad9091
    ''' </remarks>
    Public Structure Validation

        ''' <summary>
        ''' TNR：特异度（真阴性率），``[0, 1]``。
        ''' </summary>
        Public Specificity As Double

        ''' <summary>
        ''' Recall / TPR：灵敏度（真阳性率），``[0, 1]``。
        ''' </summary>
        Public Sensibility As Double

        ''' <summary>
        ''' 准确率，``[0, 1]``。
        ''' </summary>
        Public Accuracy As Double

        ''' <summary>
        ''' PPV：精确率，``[0, 1]``。
        ''' </summary>
        Public Precision As Double

        ''' <summary>
        ''' balanced error rate，``[0, 1]``。
        ''' </summary>
        Public BER As Double

        ''' <summary>
        ''' 假阳性率（FPR = 1 - 特异度），``[0, 1]``。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property FPR As Double
            Get
                Return FP / (FP + TN)
            End Get
        End Property

        ''' <summary>
        ''' Negative predictive value
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property NPV As Double
            Get
                Return TN / (FN + TN)
            End Get
        End Property

        Public All As Integer
        Public TP As Integer
        Public FP As Integer
        Public TN As Integer
        Public FN As Integer

        ''' <summary>
        ''' 进行当前的预测鉴定分析的阈值等级
        ''' </summary>
        Public Threshold As Double

        Public ReadOnly Property F1Score As Double
            Get
                Return FbetaScore(beta:=1)
            End Get
        End Property

        Public ReadOnly Property FbetaScore(Optional beta# = 1) As Double
            Get
                Return (1 + beta ^ 2) * Precision * Sensibility / ((beta ^ 2) * Precision + Sensibility)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Function ToDataSet() As Dictionary(Of String, Double)
            Return New Dictionary(Of String, Double) From {
                {NameOf(Specificity), Specificity},
                {NameOf(Sensibility), Sensibility},
                {NameOf(Accuracy), Accuracy},
                {NameOf(Precision), Precision},
                {NameOf(FPR), FPR},
                {NameOf(NPV), NPV},
                {NameOf(F1Score), F1Score},
                {"F2Score", FbetaScore(beta:=2)},
                {NameOf(BER), BER},
                {NameOf(All), All},
                {"True Positive", TP},
                {"False Positive", FP},
                {"True Negative", TN},
                {"False Negative", FN}
            }
        End Function

        ''' <summary>
        ''' 由混淆矩阵直接构造 ROC 点。这是整个模块之中**唯一**的 ROC 点构造原语，
        ''' 所有比率字段统一归一化为 ``[0, 1]`` 的分数。
        ''' </summary>
        ''' <param name="tp">真阳性人数</param>
        ''' <param name="fp">假阳性人数</param>
        ''' <param name="tn">真阴性人数</param>
        ''' <param name="fn">假阴性人数</param>
        ''' <param name="threshold">当前阈值</param>
        ''' <returns></returns>
        Public Shared Function FromConfusion(tp As Integer,
                                             fp As Integer,
                                             tn As Integer,
                                             fn As Integer,
                                             Optional threshold# = 0.5) As Validation

            Dim all As Integer = tp + fp + tn + fn
            Dim sensitivity As Double = If(tp + fn = 0, Double.NaN, tp / (tp + fn))
            Dim specificity As Double = If(tn + fp = 0, Double.NaN, tn / (tn + fp))
            Dim accuracy As Double = If(all = 0, Double.NaN, (tp + tn) / all)
            Dim precision As Double = If(tp + fp = 0, Double.NaN, tp / (tp + fp))
            Dim fpr As Double = If(fp + tn = 0, Double.NaN, fp / (fp + tn))

            Return New Validation With {
                .Sensibility = sensitivity,
                .Specificity = specificity,
                .Accuracy = accuracy,
                .Precision = precision,
                .All = all,
                .FN = fn,
                .FP = fp,
                .TN = tn,
                .TP = tp,
                .Threshold = threshold,
                .BER = 0.5 * (fpr + If(fn + tp = 0, Double.NaN, fn / (fn + tp)))
            }
        End Function

        ''' <summary>
        ''' 统计样本集合在给定判定规则下的混淆矩阵，并构造 ROC 点。
        ''' </summary>
        ''' <typeparam name="T">
        ''' + ``true`` 表示阳性
        ''' + ``false`` 表示阴性
        ''' </typeparam>
        ''' <param name="entity"></param>
        ''' <param name="getValidate">得到实际的分类结果</param>
        ''' <param name="getPredict">得到预测的分类结果</param>
        ''' <param name="percentile">当前的阈值</param>
        ''' <returns></returns>
        Public Shared Function Calc(Of T)(entity As IEnumerable(Of T),
                                          getValidate As Func(Of T, Boolean),
                                          getPredict As Func(Of T, Boolean),
                                          Optional percentile# = 0.5) As Validation

            ' 真阳性人数
            Dim TP As Integer
            ' 假阳性人数
            Dim FP As Integer
            ' 真阴性人数
            Dim TN As Integer
            ' 假阴性人数
            Dim FN As Integer
            Dim All%

            For Each n As T In entity
                Dim validate = getValidate(n)
                Dim predict = getPredict(n)

                If validate = True Then
                    ' 真实的结果为阳性
                    If predict = True Then
                        ' 预测与真实情况一致
                        TP += 1
                    Else
                        ' 但是预测为阴性
                        FN += 1
                    End If
                Else
                    ' 真实结果为阴性
                    If predict = True Then
                        ' 但是预测结果为阳性
                        FP += 1
                    Else
                        ' 预测与真实情况一致
                        TN += 1
                    End If
                End If

                All += 1
            Next

            Return FromConfusion(TP, FP, TN, FN, percentile)
        End Function

        Shared ReadOnly normalRange As [Default](Of Sequence) = New Sequence(0, 1, 10000)

        ''' <summary>
        ''' ROC 曲线下面积（梯形法）。统一委托到 <see cref="RocAuc.Trapezoid(IEnumerable(Of Validation))"/>。
        ''' </summary>
        ''' <param name="validates"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function AUC(validates As IEnumerable(Of Validation)) As Double
            Return RocAuc.Trapezoid(validates)
        End Function

        ''' <summary>
        ''' 生ROC曲线的绘制数据（统一委托到 <see cref="RocBuilder.SweepThresholds(Of T)"/>）
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="entity"></param>
        ''' <param name="getValidate">``func x, threshold => yes/no``</param>
        ''' <param name="getPredict"></param>
        ''' <param name="threshold"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 在一个二分类模型中，对于所得到的连续结果，假设已确定一个阈值，比如说 0.6，
        ''' 大于这个值的实例划归为正类，小于这个值则划到负类中。如果减小阈值，减到0.5，
        ''' 固然能识别出更多的正类，也就是提高了识别出的正例占所有正例的比例，即TPR，
        ''' 但同时也将更多的负实例当作了正实例，即提高了FPR。为了形象化这一变化，
        ''' 在此引入ROC。
        ''' </remarks>
        Public Shared Function ROC(Of T)(entity As IEnumerable(Of T),
                                         getValidate As Func(Of T, Double, Boolean),
                                         getPredict As Func(Of T, Double, Boolean),
                                         Optional threshold As [Variant](Of Sequence, Func(Of T, Double)) = Nothing) As IEnumerable(Of Validation)

            Dim dataArray As T() = entity.ToArray
            Dim cutoffs As Double()

            If threshold Is Nothing Then
                cutoffs = normalRange.DefaultValue.AsEnumerable.ToArray
            ElseIf threshold Like GetType(Sequence) Then
                cutoffs = threshold.TryCast(Of Sequence).AsEnumerable.ToArray
            Else
                cutoffs = dataArray.Select(threshold.TryCast(Of Func(Of T, Double))).ToArray
            End If

            Return RocBuilder.SweepThresholds(dataArray, getValidate, getPredict, cutoffs)
        End Function

    End Structure

End Namespace

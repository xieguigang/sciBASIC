#Region "Microsoft.VisualBasic::ed7a0e75360c457b7918d168374ce624, Data_science\DataMining\DataMining\Evaluation\Validation.vb"

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

    '   Total Lines: 218
    '    Code Lines: 132
    ' Comment Lines: 64
    '   Blank Lines: 22
    '     File Size: 8.46 KB


    '     Structure Validation
    ' 
    '         Properties: F1Score, FbetaScore, FPR, NPV
    ' 
    '         Function: AUC, Calc, ROC, ToDataSet, ToString
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
    ''' 验证结果描述
    ''' 
    ''' ``灵敏度 = 真阳性人数 / (真阳性人数 + 假阴性人数) * 100%``
    ''' ``特异度 = 真阴性人数 / (真阴性人数 + 假阳性人数) * 100%``
    ''' </summary>
    ''' <remarks>
    ''' https://www.jianshu.com/p/f0c7c1ad9091
    ''' </remarks>
    Public Structure Validation

        ''' <summary>
        ''' TNR
        ''' </summary>
        Dim Specificity As Double
        ''' <summary>
        ''' Recall, TPR
        ''' </summary>
        Dim Sensibility As Double
        Dim Accuracy As Double
        ''' <summary>
        ''' PPV
        ''' </summary>
        Dim Precision As Double
        ''' <summary>
        ''' balanced error rate
        ''' </summary>
        Dim BER As Double

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

        Dim All As Integer
        Dim TP As Integer
        Dim FP As Integer
        Dim TN As Integer
        Dim FN As Integer

        ''' <summary>
        ''' 进行当前的预测鉴定分析的百分比等级，默认是0.5，即 50%
        ''' </summary>
        Dim Threshold As Double

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
        ''' 
        ''' </summary>
        ''' <typeparam name="T">
        ''' + ``true`` 表示阳性
        ''' + ``false`` 表示阴性
        ''' </typeparam>
        ''' <param name="entity"></param>
        ''' <param name="getValidate">得到实际的分类结果</param>
        ''' <param name="getPredict">得到预测的分类结果</param>
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

            Return New Validation With {
                .Sensibility = TP / (TP + FN) * 100,
                .Specificity = TN / (TN + FP) * 100,
                .Accuracy = (TP + TN) / All * 100,
                .Precision = TP / (TP + FP) * 100,
                .All = All,
                .FN = FN,
                .FP = FP,
                .TN = TN,
                .TP = TP,
                .Threshold = percentile,
                .BER = 1 / 2 * (.FPR + FN / (FN + TP))
            }
        End Function

        Shared ReadOnly normalRange As [Default](Of Sequence) = New Sequence(0, 1, 10000)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function AUC(validates As IEnumerable(Of Validation)) As Double
            Return validates.AUC
        End Function

        ''' <summary>
        ''' 生ROC曲线的绘制数据(这个函数产生的曲线默认是阈值在[0,1]之间的)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="entity"></param>
        ''' <param name="getValidate">``func x, threshold => yes/no``</param>
        ''' <param name="getPredict"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 在一个二分类模型中，对于所得到的连续结果，假设已确定一个阈值，比如说 0.6，
        ''' 大于这个值的实例划归为正类，小于这个值则划到负类中。如果减小阈值，减到0.5，
        ''' 固然能识别出更多的正类，也就是提高了识别出的正例占所有正例的比例，即TPR，
        ''' 但同时也将更多的负实例当作了正实例，即提高了FPR。为了形象化这一变化，
        ''' 在此引入ROC。
        ''' </remarks>
        Public Shared Iterator Function ROC(Of T)(entity As IEnumerable(Of T),
                                                  getValidate As Func(Of T, Double, Boolean),
                                                  getPredict As Func(Of T, Double, Boolean),
                                                  Optional threshold As [Variant](Of Sequence, Func(Of T, Double)) = Nothing) As IEnumerable(Of Validation)

            Dim validate As Func(Of T, Boolean)
            Dim predict As Func(Of T, Boolean)
            Dim dataArray As T() = entity.ToArray
            Dim thresholdPopulator = Function() As IEnumerable(Of Double)
                                         If threshold Is Nothing Then
                                             Return normalRange.DefaultValue.AsEnumerable
                                         ElseIf threshold Like GetType(Sequence) Then
                                             Return threshold.TryCast(Of Sequence).AsEnumerable
                                         Else
                                             Return dataArray.Select(threshold.TryCast(Of Func(Of T, Double)))
                                         End If
                                     End Function

            For Each cutoff As Double In thresholdPopulator()
#Disable Warning
                validate = Function(x) getValidate(x, cutoff)
                predict = Function(x) getPredict(x, cutoff)

                Yield Validation.Calc(dataArray, validate, predict, percentile:=cutoff)
#Enable Warning
            Next
        End Function
    End Structure
End Namespace

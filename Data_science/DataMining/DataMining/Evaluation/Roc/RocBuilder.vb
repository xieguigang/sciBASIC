#Region "Microsoft.VisualBasic::f9f111c2abcd1be101076307939149ac, Data_science\DataMining\DataMining\Evaluation\Roc\RocBuilder.vb"

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

    '   Total Lines: 151
    '    Code Lines: 73 (48.34%)
    ' Comment Lines: 55 (36.42%)
    '    - Xml Docs: 90.91%
    ' 
    '   Blank Lines: 23 (15.23%)
    '     File Size: 7.01 KB


    '     Module RocBuilder
    ' 
    '         Function: FromData, (+2 Overloads) FromScores, SweepCurve, SweepThresholds
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Linq
Imports System.Security.Cryptography

Namespace Evaluation

    ''' <summary>
    ''' 统一的 ROC 曲线构建器。
    ''' 
    ''' 这是 <see cref="Evaluation"/> 模块之中**唯一**的曲线生成实现，提供三种入口：
    ''' 
    ''' 1. <see cref="FromScores(Double(), Boolean())"/>：由「分数 + 标签」直接生成曲线，
    '''    内部按分数降序扫描，并列分数（ties）会被合并为同一个阈值点，复杂度 ``O(n log n)``。
    ''' 2. <see cref="SweepThresholds(Of T)"/>：通用阈值扫描（等价于原 ``Validation.ROC``），
    '''    对每一个阈值分别统计混淆矩阵。
    ''' 3. <see cref="FromData(Of T)"/>：由泛型回调抽取分数与标签后调用 <see cref="FromScores(Double(), Boolean())"/>。
    ''' </summary>
    Public Module RocBuilder

        ''' <summary>
        ''' 由「分数 + 布尔标签」构建 ROC 曲线。
        ''' 
        ''' 约定：分数越大越倾向于正类；阈值 ``t`` 的判定规则为 ``score >= t``。
        ''' </summary>
        ''' <param name="scores">模型打分</param>
        ''' <param name="labels">真实标签，``True`` 为正类</param>
        ''' <returns></returns>
        Public Function FromScores(scores As Double(), labels As Boolean()) As RocCurve
            If scores Is Nothing OrElse labels Is Nothing OrElse scores.Length <> labels.Length Then
                Throw New DataMisalignedException("the scores and labels vectors must have the same non-empty length!")
            End If

            Dim n As Integer = scores.Length
            Dim positive As Integer = 0

            For i As Integer = 0 To n - 1
                If labels(i) Then
                    positive += 1
                End If
            Next

            Dim negative As Integer = n - positive
            Dim points As New List(Of Validation)()

            If n > 0 Then
                Dim order As Integer() = Enumerable.Range(0, n) _
                    .OrderByDescending(Function(i) scores(i)) _
                    .ToArray
                Dim tp As Integer = 0
                Dim fp As Integer = 0
                Dim index As Integer = 0

                ' 初始点：所有样本都被判定为负类
                points.Add(Validation.FromConfusion(0, 0, negative, positive, Double.MaxValue))

                While index < n
                    Dim current As Double = scores(order(index))

                    ' 并列分数合并为同一个阈值点
                    While index < n AndAlso scores(order(index)) = current
                        If labels(order(index)) Then
                            tp += 1
                        Else
                            fp += 1
                        End If

                        index += 1
                    End While

                    points.Add(Validation.FromConfusion(tp, fp, negative - fp, positive - tp, current))
                End While
            End If

            Return RocCurve.Create(points)
        End Function

        ''' <summary>
        ''' 由「分数 + 连续标签」构建 ROC 曲线，标签按照 <paramref name="cutoff"/> 二值化。
        ''' </summary>
        ''' <param name="scores">模型打分</param>
        ''' <param name="labels">标签向量，``>= cutoff`` 判定为正类</param>
        ''' <param name="cutoff">判定正类的阈值，默认为 0.5</param>
        ''' <returns></returns>
        Public Function FromScores(scores As Double(), labels As Double(), Optional cutoff As Double = 0.5) As RocCurve
            Return FromScores(scores, labels.Select(Function(v) v >= cutoff).ToArray)
        End Function

        ''' <summary>
        ''' 由泛型回调构建 ROC 曲线。
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="entity">样本集合</param>
        ''' <param name="getScore">从样本之中读取模型打分</param>
        ''' <param name="getLabel">从样本之中读取真实标签</param>
        ''' <returns></returns>
        Public Function FromData(Of T)(entity As IEnumerable(Of T),
                                       getScore As Func(Of T, Double),
                                       getLabel As Func(Of T, Boolean)) As RocCurve
            Dim data As T() = entity.ToArray
            Dim scores As Double() = data.Select(getScore).ToArray
            Dim labels As Boolean() = data.Select(getLabel).ToArray

            Return FromScores(scores, labels)
        End Function

        ''' <summary>
        ''' 通用阈值扫描：对每一个阈值分别调用 <see cref="Validation.Calc(Of T)"/> 统计混淆矩阵，
        ''' 产生 ROC 曲线之上的阈值点序列。该函数等价于原 ``Validation.ROC``。
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="entity">样本集合</param>
        ''' <param name="getValidate">``func (sample, cutoff) => 是否为阳性``</param>
        ''' <param name="getPredict">``func (sample, cutoff) => 是否预测为阳性``</param>
        ''' <param name="cutoffs">需要扫描的阈值序列</param>
        ''' <returns></returns>
        Public Iterator Function SweepThresholds(Of T)(entity As IEnumerable(Of T),
                                                       getValidate As Func(Of T, Double, Boolean),
                                                       getPredict As Func(Of T, Double, Boolean),
                                                       cutoffs As IEnumerable(Of Double)) As IEnumerable(Of Validation)

            Dim data As T() = entity.ToArray

            For Each cutoff As Double In cutoffs
                Yield Validation.Calc(
                    entity:=data,
                    getValidate:=Function(x) getValidate(x, cutoff),
                    getPredict:=Function(x) getPredict(x, cutoff),
                    percentile:=cutoff
                )
            Next
        End Function

        ''' <summary>
        ''' 通用阈值扫描并直接构建 <see cref="RocCurve"/>。
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="entity"></param>
        ''' <param name="getValidate"></param>
        ''' <param name="getPredict"></param>
        ''' <param name="cutoffs"></param>
        ''' <returns></returns>
        Public Function SweepCurve(Of T)(entity As IEnumerable(Of T),
                                         getValidate As Func(Of T, Double, Boolean),
                                         getPredict As Func(Of T, Double, Boolean),
                                         cutoffs As IEnumerable(Of Double)) As RocCurve

            Return RocCurve.Create(SweepThresholds(entity, getValidate, getPredict, cutoffs))
        End Function

    End Module

End Namespace

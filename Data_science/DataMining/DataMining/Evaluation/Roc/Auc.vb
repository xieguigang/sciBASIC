#Region "Microsoft.VisualBasic::, Data_science\DataMining\DataMining\Evaluation\Roc\Auc.vb"

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
Imports System.Security.Cryptography
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Evaluation

    ''' <summary>
    ''' 统一的 AUC（Area Under the ROC Curve）计算核心。
    ''' 
    ''' 这是整个 <see cref="Evaluation"/> 模块之中**唯一**的 AUC 实现：
    ''' <see cref="Metric.auc"/>、<see cref="ROC.AUC(Double(), Double())"/>、
    ''' <see cref="ROC.SimpleAUC(Vector, Vector)"/>、<see cref="Validation.AUC(IEnumerable(Of Validation))"/>
    ''' 以及 <see cref="PerformanceEvaluator"/> 的曲线面积全部委托到这里，
    ''' 从而保证同一份数据在所有调用路径之上得到一致的数值结果。
    ''' 
    ''' 模块提供两种语义的 AUC：
    ''' 
    ''' 1. <see cref="RankAUC(Double(), Boolean())"/>：对原始分数直接计算的精确秩和 AUC
    '''    （Mann–Whitney U 统计量），并列分数使用平均秩，复杂度 ``O(n log n)``。
    ''' 2. <see cref="Trapezoid(IEnumerable(Of Validation))"/>：由 ROC 曲线（阈值扫描结果）
    '''    按照梯形面积法积分得到的 AUC。
    ''' 
    ''' 两者共享同一套并列分数（ties）处理与归一化约定，因此在分数取值唯一时数值相等。
    ''' </summary>
    Public Module RocAuc

        ''' <summary>
        ''' 精确的秩和 AUC（Mann–Whitney U 统计量）。
        ''' 
        ''' 计算公式：``AUC = (Σ rank(positive) - n_pos * (n_pos + 1) / 2) / (n_pos * n_neg)``，
        ''' 其中并列分数使用平均秩（average rank）处理。
        ''' </summary>
        ''' <param name="scores">模型的打分（分数越大越倾向于正类）</param>
        ''' <param name="labels">样本的真实类别，``True`` 表示正类</param>
        ''' <returns>
        ''' ``[0, 1]`` 区间内的 AUC 值；当样本只有一个类别（正类或负类数量为 0）时返回
        ''' <see cref="Double.NaN"/>。
        ''' </returns>
        Public Function RankAUC(scores As Double(), labels As Boolean()) As Double
            If scores Is Nothing OrElse labels Is Nothing Then
                Return Double.NaN
            End If

            If scores.Length <> labels.Length Then
                Throw New DataMisalignedException($"the scores({scores.Length}) and labels({labels.Length}) vectors must have the same length!")
            End If

            Dim n As Integer = scores.Length

            If n = 0 Then
                Return Double.NaN
            End If

            Dim positive As Integer = 0

            For i As Integer = 0 To n - 1
                If labels(i) Then
                    positive += 1
                End If
            Next

            Dim negative As Integer = n - positive

            If positive = 0 OrElse negative = 0 Then
                ' 只有单一类别的时候 AUC 没有定义
                Return Double.NaN
            End If

            Dim order As Integer() = Enumerable.Range(0, n) _
                .OrderBy(Function(i) scores(i)) _
                .ToArray
            Dim ranks As Double() = New Double(n - 1) {}
            Dim start As Integer = 0

            While start < n
                Dim [stop] As Integer = start

                While [stop] + 1 < n AndAlso scores(order([stop] + 1)) = scores(order(start))
                    [stop] += 1
                End While

                ' 并列分数取平均秩（1-based）
                Dim averageRank As Double = (start + [stop]) / 2.0 + 1.0

                For k As Integer = start To [stop]
                    ranks(order(k)) = averageRank
                Next

                start = [stop] + 1
            End While

            Dim rankSum As Double = 0

            For i As Integer = 0 To n - 1
                If labels(i) Then
                    rankSum += ranks(i)
                End If
            Next

            Dim u As Double = rankSum - positive * (positive + 1) / 2.0

            Return u / (positive * negative)
        End Function

        ''' <summary>
        ''' 把连续的标签向量按照 <paramref name="cutoff"/> 二值化之后计算秩和 AUC。
        ''' </summary>
        ''' <param name="scores">模型的打分</param>
        ''' <param name="labels">标签向量，``>= cutoff`` 判定为正类</param>
        ''' <param name="cutoff">判定正类的阈值，默认为 0.5</param>
        ''' <returns></returns>
        Public Function RankAUC(scores As Double(), labels As Double(), Optional cutoff As Double = 0.5) As Double
            If labels Is Nothing Then
                Return Double.NaN
            End If

            Return RankAUC(scores, labels.Select(Function(v) v >= cutoff).ToArray)
        End Function

        ''' <summary>
        ''' 曲线梯形面积法的**核心实现**：对已经按照 ``FPR`` 升序排列的
        ''' ``(TPR, FPR)`` 序列做梯形积分。
        ''' 
        ''' 所有其它 ``Trapezoid`` 重载最终都调用这个函数，从而保证模块内只有一份积分代码。
        ''' </summary>
        ''' <param name="TPR">真阳性率序列（必须已按 FPR 升序排列）</param>
        ''' <param name="FPR">假阳性率序列（升序）</param>
        ''' <returns></returns>
        Public Function Trapezoid(TPR As Double(), FPR As Double()) As Double
            If TPR Is Nothing OrElse FPR Is Nothing Then
                Return Double.NaN
            End If

            Dim n As Integer = System.Math.Min(TPR.Length, FPR.Length)

            If n < 2 Then
                Return 0
            End If

            Dim area As Double = 0

            For i As Integer = 1 To n - 1
                area += (FPR(i) - FPR(i - 1)) * (TPR(i) + TPR(i - 1)) / 2.0
            Next

            Return area
        End Function

        ''' <summary>
        ''' 曲线梯形面积法计算 AUC。
        ''' 
        ''' 输入曲线会先按照 ``FPR`` 升序排序，并自动忽略包含 ``NaN`` 的无效点。
        ''' </summary>
        ''' <param name="curve">ROC 曲线（阈值扫描产生的 <see cref="Validation"/> 点集合）</param>
        ''' <returns>``[0, 1]`` 区间内的曲线下面积</returns>
        Public Function Trapezoid(curve As IEnumerable(Of Validation)) As Double
            If curve Is Nothing Then
                Return Double.NaN
            End If

            Dim points As Validation() = curve _
                .Where(Function(p) Not p.FPR.IsNaNImaginary AndAlso Not p.Sensibility.IsNaNImaginary) _
                .OrderBy(Function(p) p.FPR) _
                .ToArray

            If points.Length < 2 Then
                Return 0
            End If

            Return Trapezoid(points.Select(Function(p) p.Sensibility).ToArray,
                             points.Select(Function(p) p.FPR).ToArray)
        End Function

        ''' <summary>
        ''' 直接取 <see cref="RocCurve"/> 上已经计算好的曲线面积。
        ''' </summary>
        ''' <param name="curve"></param>
        ''' <returns></returns>
        Public Function Trapezoid(curve As RocCurve) As Double
            If curve Is Nothing Then
                Return Double.NaN
            Else
                Return curve.AUC
            End If
        End Function

        ''' <summary>
        ''' 在 ROC 曲线之上寻找最佳分类阈值：距离理想点 ``(FPR=0, TPR=1)`` 欧氏距离最小的那个点。
        ''' 
        ''' 返回的是曲线点数组之中的下标；当曲线为空的时候返回 ``-1``。
        ''' </summary>
        ''' <param name="curve">ROC 曲线</param>
        ''' <returns></returns>
        Public Function BestThreshold(curve As IEnumerable(Of Validation)) As Integer
            If curve Is Nothing Then
                Return -1
            End If

            Dim points As Validation() = curve.ToArray
            Dim best As Integer = -1
            Dim minDist As Double = Double.MaxValue

            For i As Integer = 0 To points.Length - 1
                Dim distance As Double = DistanceToIdealPoint(points(i))

                If Not distance.IsNaNImaginary AndAlso distance < minDist Then
                    minDist = distance
                    best = i
                End If
            Next

            Return best
        End Function

        ''' <summary>
        ''' 到理想点 ``(FPR=0, TPR=1)`` 的欧氏距离：``sqrt((1 - TPR)^2 + FPR^2)``。
        ''' </summary>
        ''' <param name="point"></param>
        ''' <returns></returns>
        Public Function DistanceToIdealPoint(point As Validation) As Double
            Return System.Math.Sqrt((1 - point.Sensibility) ^ 2 + point.FPR ^ 2)
        End Function

        ''' <summary>
        ''' 兼容旧签名：现由 <see cref="BestThreshold(IEnumerable(Of Validation))"/> 统一实现。
        ''' （注意：本模块命名为 <c>RocAuc</c> 而不是 <c>Auc</c>，以避免与既有公开成员
        ''' ``Metric.auc`` / ``ROC.AUC`` / ``Validation.AUC`` 发生 VB 不区分大小写的名称冲突。）
        ''' </summary>
        ''' <param name="TPR">真阳性率（Sensibility）序列</param>
        ''' <param name="FPR">假阳性率序列</param>
        ''' <returns></returns>
        <Obsolete("请改用 BestThreshold(curve As IEnumerable(Of Validation))。")>
        Public Function BestThreshold(TPR As Vector, FPR As Vector) As Integer
            If TPR Is Nothing OrElse FPR Is Nothing Then
                Return -1
            End If

            Dim n As Integer = System.Math.Min(TPR.Length, FPR.Length)

            If n = 0 Then
                Return -1
            End If

            Dim best As Integer = -1
            Dim minDist As Double = Double.MaxValue

            For i As Integer = 0 To n - 1
                Dim distance As Double = System.Math.Sqrt((1 - TPR(i)) ^ 2 + FPR(i) ^ 2)

                If Not distance.IsNaNImaginary AndAlso distance < minDist Then
                    minDist = distance
                    best = i
                End If
            Next

            Return best
        End Function

    End Module

End Namespace


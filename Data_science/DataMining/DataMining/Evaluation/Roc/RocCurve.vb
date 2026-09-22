#Region "Microsoft.VisualBasic::RocCurve, Data_science\DataMining\DataMining\Evaluation\Roc\RocCurve.vb"

Imports System.Linq

Namespace Evaluation

    ''' <summary>
    ''' 统一的 ROC 曲线模型：<see cref="Evaluation"/> 模块之中唯一的曲线结果载体。
    ''' 
    ''' 一条 ROC 曲线由若干 <see cref="Validation"/> 阈值点组成（按 ``FPR`` 升序排列），
    ''' 并附带由 <see cref="RocAuc.Trapezoid(IEnumerable(Of Validation))"/> 计算出来的曲线下面积
    ''' 以及 <see cref="RocAuc.BestThreshold(IEnumerable(Of Validation))"/> 给出的最佳阈值下标。
    ''' </summary>
    Public Class RocCurve

        ''' <summary>
        ''' 曲线上的阈值点，已经按照 ``FPR`` 升序排列。
        ''' </summary>
        ''' <returns></returns>
        Public Property Points As Validation()

        ''' <summary>
        ''' 曲线下面积（梯形法），取值范围 ``[0, 1]``。
        ''' </summary>
        ''' <returns></returns>
        Public Property AUC As Double

        ''' <summary>
        ''' 最佳阈值点在 <see cref="Points"/> 之中的下标；空曲线时为 ``-1``。
        ''' </summary>
        ''' <returns></returns>
        Public Property BestIndex As Integer

        ''' <summary>
        ''' 正类样本的数量。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Positive As Integer
            Get
                If Points Is Nothing OrElse Points.Length = 0 Then
                    Return 0
                Else
                    Return Points(0).TP + Points(0).FN
                End If
            End Get
        End Property

        ''' <summary>
        ''' 负类样本的数量。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Negative As Integer
            Get
                If Points Is Nothing OrElse Points.Length = 0 Then
                    Return 0
                Else
                    Return Points(0).FP + Points(0).TN
                End If
            End Get
        End Property

        ''' <summary>
        ''' 最佳阈值点（距离理想点 ``(FPR=0, TPR=1)`` 最近）。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property BestThreshold As Validation
            Get
                If BestIndex >= 0 AndAlso Points IsNot Nothing AndAlso BestIndex < Points.Length Then
                    Return Points(BestIndex)
                Else
                    Return New Validation()
                End If
            End Get
        End Property

        ''' <summary>
        ''' 由一组阈值点构建 ROC 曲线（自动排序并计算 AUC 与最佳阈值）。
        ''' </summary>
        ''' <param name="points"></param>
        ''' <returns></returns>
        Public Shared Function Create(points As IEnumerable(Of Validation)) As RocCurve
            Dim array As Validation() = If(points Is Nothing, New Validation() {}, points.ToArray())

            array = array _
                .OrderBy(Function(p) p.FPR) _
                .ToArray

            Return New RocCurve With {
                .Points = array,
                .AUC = RocAuc.Trapezoid(array),
                .BestIndex = RocAuc.BestThreshold(array)
            }
        End Function

        ''' <summary>
        ''' 以 ``(阈值, TPR, FPR)`` 的形式枚举当前曲线的绘制点。
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function EnumeratePoints() As IEnumerable(Of (threshold As Double, TPR As Double, FPR As Double))
            If Points IsNot Nothing Then
                For Each point As Validation In Points
                    Yield (point.Threshold, point.Sensibility, point.FPR)
                Next
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"ROC(auc={AUC.ToString("F4")}, points={If(Points Is Nothing, 0, Points.Length)}, +{Positive}/-{Negative})"
        End Function

    End Class

End Namespace

#End Region

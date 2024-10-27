#Region "Microsoft.VisualBasic::ea32fe8e1509a8d1bac5a2bf1d8ab6dc, Data_science\DataMining\DataMining\Evaluation\ROC.vb"

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

    '   Total Lines: 139
    '    Code Lines: 73 (52.52%)
    ' Comment Lines: 51 (36.69%)
    '    - Xml Docs: 86.27%
    ' 
    '   Blank Lines: 15 (10.79%)
    '     File Size: 5.33 KB


    '     Module ROC
    ' 
    '         Function: (+3 Overloads) AUC, BestThreshold, SimpleAUC
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Evaluation

    ''' <summary>
    ''' The ROC math module
    ''' </summary>
    Public Module ROC

        ''' <summary>
        ''' 使用梯形面积法计算AUC的结果值
        ''' </summary>
        ''' <param name="validates"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' https://blog.revolutionanalytics.com/2016/11/calculating-auc.html
        ''' 
        ''' ```r
        ''' simple_auc &lt;- function(TPR, FPR){
        '''    # inputs already sorted, best scores first 
        '''    dFPR &lt;- c(diff(FPR), 0)
        '''    dTPR &lt;- c(diff(TPR), 0)
        '''    sum(TPR * dFPR) + sum(dTPR * dFPR) / 2;
        ''' }
        '''
        ''' with(roc_df, simple_auc(TPR, FPR))
        ''' ```
        ''' </remarks>
        <Extension>
        Public Function AUC(validates As IEnumerable(Of Validation)) As Double
            With validates.OrderBy(Function(x) x.Sensibility).ToArray
                Dim TPR As Vector = .Select(Function(v) v.Sensibility).AsVector
                Dim FPR As Vector = .Select(Function(v) v.FPR).AsVector

                Return SimpleAUC(TPR, FPR)
            End With
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="TPR"></param>
        ''' <param name="FPR"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' two input vector must be sorted
        ''' </remarks>
        Public Function SimpleAUC(TPR As Vector, FPR As Vector) As Double
            Dim auc As Double = 0.0

            For i As Integer = 1 To FPR.Length - 1
                auc += (FPR(i) - FPR(i - 1)) * (TPR(i) + TPR(i - 1)) / 2.0
            Next

            Return auc
        End Function

        ''' <summary>
        ''' get the index of best threshold 
        ''' </summary>
        ''' <param name="TPR">sensibility</param>
        ''' <param name="FPR"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' Calculate the distance to the ideal point (0,1) for each row
        ''' We use the euclidean distance: sqrt((1-TPR)^2 + (FPR)^2)
        ''' 
        ''' NA value will be ignored from this function automatically.
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function BestThreshold(TPR As Vector, FPR As Vector) As Integer
            Return which.Min(Vector.Sqrt((1 - TPR) ^ 2 + (FPR) ^ 2).Select(Function(vi) If(vi.IsNaNImaginary, Double.MaxValue, vi)))
        End Function

        ''' <summary>
        ''' Rank排序法计算AUC面积
        ''' </summary>
        ''' <param name="validates"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function AUC(validates As IEnumerable(Of Validate), Optional names$() = Nothing) As IEnumerable(Of NamedValue(Of Double))
            Dim validateVector = validates.SafeQuery.ToArray
            Dim width% = validateVector(Scan0).width

            If names.IsNullOrEmpty Then
                names = width.SeqIterator _
                    .Select(Function(i) $"output_{i}") _
                    .ToArray
            End If

            Dim predicts As Double()
            Dim actuals As Double()
            Dim aucValue As Double

#Disable Warning
            For i As Integer = 0 To width - 1
                predicts = validateVector.Select(Function(test) test.predicts(i)).ToArray
                actuals = validateVector.Select(Function(test) test.actuals(i)).ToArray
                aucValue = AUC(predicts, actuals)

                Yield New NamedValue(Of Double) With {
                    .Name = names(i),
                    .Value = aucValue
                }
            Next
#Enable Warning
        End Function

        Public Function AUC(predicts As Double(), actuals As Double()) As Double
            ' 创建一个包含预测值、标签和索引的元组数组
            ' 按预测值降序排序
            Dim labeledPredictions = Enumerable.Range(0, predicts.Length) _
                .Select(Function(i) (prediction:=predicts(i), label:=actuals(i) > 0, i)) _
                .OrderByDescending(Function(a) a.prediction) _
                .ToArray()

            ' 计算TPR和FPR
            Dim aucVal As Double = 0
            Dim prevFPR As Double = 0
            Dim tprSum As Double = 0
            Dim fprSum As Double = 0
            Dim posCount As Double = labeledPredictions.Count(Function(l) l.label)
            Dim negCount = actuals.Length - posCount

            For i As Integer = 0 To labeledPredictions.Length - 1
                If labeledPredictions(i).label Then ' 真阳性
                    tprSum += 1 ' 假阳性
                Else
                    fprSum += 1
                End If

                Dim tpr = tprSum / posCount
                Dim fpr = fprSum / negCount

                ' 累加面积
                aucVal += (tpr + prevFPR) * (fpr - prevFPR) / 2
                prevFPR = fpr
            Next

            ' 处理最后一个点（FPR=0）
            aucVal += tprSum / posCount * prevFPR / 2

            Return aucVal
        End Function
    End Module
End Namespace

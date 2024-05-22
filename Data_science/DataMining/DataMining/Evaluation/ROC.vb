#Region "Microsoft.VisualBasic::1117607db00b863b282064ff55ef5162, Data_science\DataMining\DataMining\Evaluation\ROC.vb"

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

    '   Total Lines: 111
    '    Code Lines: 68 (61.26%)
    ' Comment Lines: 30 (27.03%)
    '    - Xml Docs: 83.33%
    ' 
    '   Blank Lines: 13 (11.71%)
    '     File Size: 4.31 KB


    '     Module ROC
    ' 
    '         Function: (+3 Overloads) AUC, SimpleAUC
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
Imports Microsoft.VisualBasic.Math.Scripting.Rscript

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

        Public Function SimpleAUC(TPR As Vector, FPR As Vector) As Double
            Dim dFPR As Vector = diff(FPR).AppendAfter(0).ToArray
            Dim dTPR As Vector = diff(TPR).AppendAfter(0).ToArray

            Return (TPR * dFPR).Sum + (dTPR * dFPR).Sum / 2
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
            Dim validateVector = predicts _
                .Select(Function(a, i) (predicts:=a, actual:=actuals(i))) _
                .ToArray
            ' 首先对score从大到小排序
            Dim orderScoreDesc = validateVector _
                .OrderByDescending(Function(test) test.predicts) _
                .ToArray
            ' 然后按照score进行ranking的计算
            Dim ranks = orderScoreDesc _
                .Select(Function(test) test.predicts) _
                .Ranking(, desc:=False) _
                .AsVector
            ' 然后把所有的正类样本的rank相加
            Dim positiveRankSum = which _
                .IsTrue(orderScoreDesc.Select(Function(test) test.actual > 0)) _
                .DoCall(Function(indices) ranks(indices)) _
                .Sum
            Dim M = orderScoreDesc.Count(Function(test) test.actual > 0)
            Dim N = validateVector.Length - M
            Dim aucValue = (positiveRankSum - M * (1 + M) / 2) / (M * N)

            Return aucValue
        End Function
    End Module
End Namespace

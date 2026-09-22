#Region "Microsoft.VisualBasic::4f94e38efbf937df67e5daf2bd5bc63e, Data_science\DataMining\DataMining\Evaluation\ROC.vb"

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

    '   Total Lines: 91
    '    Code Lines: 45 (49.45%)
    ' Comment Lines: 35 (38.46%)
    '    - Xml Docs: 97.14%
    ' 
    '   Blank Lines: 11 (12.09%)
    '     File Size: 3.65 KB


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
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Evaluation

    ''' <summary>
    ''' ROC / AUC 的兼容薄封装层。
    ''' 
    ''' 本模块不再包含任何独立的 ROC 曲线或 AUC 计算逻辑，
    ''' 全部委托到统一核心 <see cref="RocBuilder"/>（曲线构建）与 <see cref="Auc"/>（面积计算）。
    ''' </summary>
    Public Module ROC

        ''' <summary>
        ''' 由 ROC 曲线点集计算曲线下面积（梯形法）。
        ''' </summary>
        ''' <param name="validates"></param>
        ''' <returns></returns>
        <Extension>
        Public Function AUC(validates As IEnumerable(Of Validation)) As Double
            Return RocAuc.Trapezoid(validates)
        End Function

        ''' <summary>
        ''' 由已经按照 FPR 升序排序的 ``(TPR, FPR)`` 序列计算曲线下面积。
        ''' </summary>
        ''' <param name="TPR"></param>
        ''' <param name="FPR"></param>
        ''' <returns></returns>
        Public Function SimpleAUC(TPR As Vector, FPR As Vector) As Double
            If TPR Is Nothing OrElse FPR Is Nothing Then
                Return Double.NaN
            End If

            Return RocAuc.Trapezoid(TPR.Array, FPR.Array)
        End Function

        ''' <summary>
        ''' 由 ``(分数, 标签)`` 直接计算精确的秩和 AUC。
        ''' </summary>
        ''' <param name="predicts"></param>
        ''' <param name="actuals"></param>
        ''' <returns></returns>
        Public Function AUC(predicts As Double(), actuals As Double()) As Double
            Return RocAuc.RankAUC(predicts, actuals)
        End Function

        ''' <summary>
        ''' 获取最靠近理想点 ``(FPR=0, TPR=1)`` 的曲线点下标。
        ''' </summary>
        ''' <param name="TPR">真阳性率序列</param>
        ''' <param name="FPR">假阳性率序列</param>
        ''' <returns></returns>
        <Obsolete("请改用 RocAuc.BestThreshold(curve As IEnumerable(Of Validation))。")>
        Public Function BestThreshold(TPR As Vector, FPR As Vector) As Integer
            Return RocAuc.BestThreshold(TPR, FPR)
        End Function

        ''' <summary>
        ''' 多输出维度结果的 AUC（每一个输出维度产生一个命名指标）。
        ''' </summary>
        ''' <param name="validates"></param>
        ''' <param name="names"></param>
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

#Disable Warning
            For i As Integer = 0 To width - 1
                Dim predicts As Double() = validateVector.Select(Function(test) test.predicts(i)).ToArray
                Dim actuals As Double() = validateVector.Select(Function(test) test.actuals(i)).ToArray

                Yield New NamedValue(Of Double) With {
                    .Name = names(i),
                    .Value = RocAuc.RankAUC(predicts, actuals)
                }
            Next
#Enable Warning
        End Function
    End Module
End Namespace

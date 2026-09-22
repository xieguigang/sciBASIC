#Region "Microsoft.VisualBasic::b80a74bcc7917e841de43dd3808a69ef, Data_science\DataMining\DataMining\Evaluation\Metric.vb"

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

    '   Total Lines: 168
    '    Code Lines: 99 (58.93%)
    ' Comment Lines: 41 (24.40%)
    '    - Xml Docs: 92.68%
    ' 
    '   Blank Lines: 28 (16.67%)
    '     File Size: 5.89 KB


    '     Enum Metrics
    ' 
    '         [error], acc, auc, mae, mse
    '         none
    ' 
    '  
    ' 
    ' 
    ' 
    '     Delegate Function
    ' 
    ' 
    '     Class Metric
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: [error], accuracy, auc, crossEntropyLoss, GetMetric
    '                   mean_absolute_error, mean_square_error, Parse, r2_score, root_mean_square_error
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Linq
Imports std = System.Math

Namespace Evaluation

    Public Enum Metrics
        none
        ''' <summary>
        ''' <see cref="Metric.accuracy"/> 
        ''' </summary>
        acc
        ''' <summary>
        ''' <see cref="Metric.error"/> 
        ''' </summary>
        [error]
        ''' <summary>
        ''' <see cref="Metric.mean_square_error"/> 
        ''' </summary>
        mse
        ''' <summary>
        ''' <see cref="Metric.mean_absolute_error"/> 
        ''' </summary>
        mae
        ''' <summary>
        ''' <see cref="Metric.auc"/> 
        ''' </summary>
        auc
    End Enum

    ''' <summary>
    ''' 统一的「``(预测值, 标签)`` → 指标值」委托。
    ''' 该签名属于公开契约（例如 ``xgboost`` 项目通过 <see cref="Metric.GetMetric"/> 使用），
    ''' 因此在本轮重构之中保持不变。
    ''' </summary>
    ''' <param name="pred"></param>
    ''' <param name="label"></param>
    ''' <returns></returns>
    Public Delegate Function IMetric(pred As Double(), label As Double()) As Double

    ''' <summary>
    ''' 经典模型评估指标的兼容入口。
    ''' 
    ''' 数值实现全部委托到统一核心（<see cref="Auc"/> 等），本类型只负责名称解析与
    ''' 保持既有的公开签名不变。
    ''' </summary>
    Public NotInheritable Class Metric

        Private Sub New()
        End Sub

        Public Shared Function Parse(metric As String) As Metrics
            Select Case Strings.LCase(metric)
                Case NameOf(Metrics.acc) : Return Metrics.acc
                Case NameOf(Metrics.auc) : Return Metrics.auc
                Case NameOf(Metrics.error) : Return Metrics.error
                Case NameOf(Metrics.mae) : Return Metrics.mae
                Case NameOf(Metrics.mse) : Return Metrics.mse
                Case Else
                    Return Metrics.mse
            End Select
        End Function

        Public Shared Function GetMetric(metric As Metrics) As IMetric
            Select Case metric
                Case Metrics.acc : Return AddressOf accuracy
                Case Metrics.error : Return AddressOf [error]
                Case Metrics.mse : Return AddressOf mean_square_error
                Case Metrics.mae : Return AddressOf mean_absolute_error
                Case Metrics.auc : Return AddressOf auc
                Case Else
                    Throw New NotImplementedException()
            End Select
        End Function

        Public Shared Function accuracy(pred As Double(), label As Double()) As Double
            Dim hit = 0.0

            For i = 0 To pred.Length - 1

                If label(i) = 0 AndAlso pred(i) < 0.5 OrElse label(i) = 1 AndAlso pred(i) > 0.5 Then
                    hit += 1
                End If
            Next

            Return hit / pred.Length
        End Function

        Public Shared Function [error](pred As Double(), label As Double()) As Double
            Return 1.0 - Metric.accuracy(pred, label)
        End Function

        Public Shared Function mean_square_error(pred As Double(), label As Double()) As Double
            Dim sum = 0.0

            For i = 0 To pred.Length - 1
                sum += std.Pow(pred(i) - label(i), 2.0)
            Next

            Return sum / pred.Length
        End Function

        Public Shared Function root_mean_square_error(pred As Double(), label As Double()) As Double
            Return std.Sqrt(mean_square_error(pred, label))
        End Function

        Public Shared Function mean_absolute_error(pred As Double(), label As Double()) As Double
            Dim sum = 0.0

            For i = 0 To pred.Length - 1
                sum += std.Abs(pred(i) - label(i))
            Next

            Return sum / pred.Length
        End Function

        ''' <summary>
        ''' 决定系数 R²：``1 - SSE / SST``。
        ''' </summary>
        Public Shared Function r2_score(pred As Double(), label As Double()) As Double
            Dim mean As Double = label.Average()
            Dim sse As Double = 0
            Dim sst As Double = 0

            For i = 0 To pred.Length - 1
                sse += std.Pow(pred(i) - label(i), 2.0)
                sst += std.Pow(label(i) - mean, 2.0)
            Next

            If sst = 0 Then
                Return 0
            Else
                Return 1.0 - sse / sst
            End If
        End Function

        ''' <summary>
        ''' 精确的秩和 AUC（委托到统一核心 <see cref="RocAuc.RankAUC(Double(), Double(), Double)"/>）。
        ''' </summary>
        Public Shared Function auc(pred As Double(), label As Double()) As Double
            Return RocAuc.RankAUC(pred, label)
        End Function

        ''' <summary>
        ''' 交叉熵损失。
        ''' 
        ''' 与旧实现不同，本函数**不会**修改调用方传入的 <paramref name="predictions"/> 数组。
        ''' </summary>
        Public Shared Function crossEntropyLoss(predictions As Double(), labels As Double()) As Double
            Dim loss As Double = 0
            Dim epsilon As Double = 0.000000000000001

            For i As Integer = 0 To predictions.Length - 1
                ' 使用局部变量做数值截断，避免破坏调用方的输入数据
                Dim p As Double = predictions(i)

                If p = 0.0 Then
                    p = epsilon
                ElseIf p = 1.0 Then
                    p = 1 - epsilon
                End If

                loss -= labels(i) * std.Log(p) + (1 - labels(i)) * std.Log(1 - p)
            Next

            Return loss / predictions.Length
        End Function
    End Class
End Namespace

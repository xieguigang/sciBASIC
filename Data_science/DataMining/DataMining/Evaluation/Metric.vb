#Region "Microsoft.VisualBasic::56caaaed13fc8170e7d29409e3c11962, Data_science\DataMining\DataMining\Evaluation\Metric.vb"

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
    '    Code Lines: 95 (68.35%)
    ' Comment Lines: 15 (10.79%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 29 (20.86%)
    '     File Size: 4.54 KB


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
    '         Function: [error], accuracy, auc, GetMetric, mean_absolute_error
    '                   mean_square_error, Parse
    '         Class labelComparer
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: compare
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports stdNum = System.Math

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

    Public Delegate Function IMetric(pred As Double(), label As Double()) As Double

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
                sum += stdNum.Pow(pred(i) - label(i), 2.0)
            Next

            Return sum / pred.Length
        End Function

        Public Shared Function mean_absolute_error(pred As Double(), label As Double()) As Double
            Dim sum = 0.0

            For i = 0 To pred.Length - 1
                sum += stdNum.Abs(pred(i) - label(i))
            Next

            Return sum / pred.Length
        End Function

        Public Shared Function auc(pred As Double(), label As Double()) As Double
            Dim n_pos As Double = 0

            For Each v In label
                n_pos += v
            Next

            Dim n_neg = pred.Length - n_pos
            Dim label_pred As Double()() = RectangularArray.Matrix(Of Double)(pred.Length, 2)

            For i = 0 To pred.Length - 1
                label_pred(i)(0) = label(i)
                label_pred(i)(1) = pred(i)
            Next

            Array.Sort(label_pred, New Metric.labelComparer())
            Dim accumulated_neg As Double = 0
            Dim satisfied_pair As Double = 0

            For i = 0 To label_pred.Length - 1

                If label_pred(i)(0) = 1 Then
                    satisfied_pair += accumulated_neg
                Else
                    accumulated_neg += 1
                End If
            Next

            Return satisfied_pair / n_neg / n_pos
        End Function

        Private Class labelComparer : Implements IComparer(Of Double())

            Public Sub New()
            End Sub

            Public Overridable Function compare(a As Double(), b As Double()) As Integer Implements IComparer(Of Double()).Compare
                Return a(1).CompareTo(b(1))
            End Function
        End Class
    End Class
End Namespace

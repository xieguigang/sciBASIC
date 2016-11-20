#Region "Microsoft.VisualBasic::28f2b29b674dac877aa0510dccf958d1, ..\sciBASIC#\Data_science\Mathematical\Math\Quantile\Extensions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Linq

Namespace Quantile

    Public Module Extensions

        Public Const epsilon As Double = 0.001

        ''' <summary>
        ''' Example Usage:
        ''' 
        ''' ```vbnet
        ''' Dim shuffle As Long() = New Long(window_size - 1) {}
        '''
        ''' For i As Integer = 0 To shuffle.Length - 1
        '''     shuffle(i) = i
        ''' Next
        '''
        ''' shuffle = shuffle.Shuffles
        '''
        ''' Dim estimator As QuantileEstimationGK = Shuffle.GKQuantile
        ''' Dim quantiles As Double() = {0.5, 0.9, 0.95, 0.99, 1.0}
        '''
        ''' For Each q As Double In quantiles
        '''     Dim estimate As Long = estimator.query(q)
        '''     Dim actual As Long = Shuffle.actually(q)
        '''     Dim out As String = String.Format("Estimated {0:F2} quantile as {1:D} (actually {2:D})", q, estimate, actual)
        '''
        '''     Call out.__DEBUG_ECHO
        ''' Next
        ''' ```
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="epsilon"></param>
        ''' <param name="compact_size"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GKQuantile(source As IEnumerable(Of Long),
                                   Optional epsilon# = Extensions.epsilon,
                                   Optional compact_size% = 1000) As QuantileEstimationGK

            Dim estimator As New QuantileEstimationGK(epsilon, compact_size)

            For Each x As Long In source
                Call estimator.Insert(x)
            Next

            Return estimator
        End Function

        Const window_size As Integer = 10000

        ''' <summary>
        ''' Selector for object sequence that by using quantile calculation.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="data"></param>
        ''' <param name="getValue">
        ''' Object in the input sequence that can be measuring as a numeric value by using this function pointer.
        ''' (通过这个函数指针可以将序列之中的对象转换为可计算quantile的数值)
        ''' </param>
        ''' <param name="quantiles#"></param>
        ''' <param name="epsilon#"></param>
        ''' <param name="compact_size%"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function SelectByQuantile(Of T)(data As IEnumerable(Of T),
                                                        getValue As Func(Of T, Long),
                                                        quantiles#(),
                                                        Optional epsilon# = Extensions.epsilon,
                                                        Optional compact_size% = 1000) As IEnumerable(Of DoubleTagged(Of T()))
            Dim cache = (From x As T
                         In data
                         Select x,
                             v = getValue(x)).ToArray
            Dim vals As Long() = cache.ToArray(Function(x) x.v)
            Dim estimator As QuantileEstimationGK = vals.GKQuantile(epsilon, compact_size)

            For Each q As Double In quantiles
                Dim estimate As Long = estimator.Query(q)
                Dim up As IEnumerable(Of T) = From x
                                              In cache
                                              Where x.v >= estimate
                                              Select x.x

                Yield New DoubleTagged(Of T()) With {
                    .Tag = q,
                    .value = up.ToArray
                }
            Next
        End Function

        ''' <summary>
        ''' 使用示例
        ''' </summary>
        Private Sub Test()
            Dim shuffle As Long() = New Long(window_size - 1) {}

            For i As Integer = 0 To shuffle.Length - 1
                shuffle(i) = i
            Next

            shuffle = shuffle.Shuffles

            Dim estimator As QuantileEstimationGK = shuffle.GKQuantile
            Dim quantiles As Double() = {0.5, 0.9, 0.95, 0.99, 1.0}

            For Each q As Double In quantiles
                Dim estimate As Long = estimator.Query(q)
                Dim actual As Long = shuffle.actually(q)
                Dim out As String = String.Format("Estimated {0:F2} quantile as {1:D} (actually {2:D})", q, estimate, actual)

                Call out.__DEBUG_ECHO
            Next
        End Sub

        <Extension>
        Public Function actually(source As Long(), q As Double) As Long
            Dim actual As Long = CLng(Fix((q) * (source.Length - 1)))
            Return actual
        End Function
    End Module
End Namespace

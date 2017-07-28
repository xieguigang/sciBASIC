#Region "Microsoft.VisualBasic::9f2ef44620e5f0716a11cdc2f8b9a254, ..\sciBASIC#\Data_science\Mathematical\Math\Quantile\Extensions.vb"

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

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Quantile

    ''' <summary>
    ''' GK quantile extensions method
    ''' </summary>
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

        <Extension>
        Public Function GKQuantile(source As IEnumerable(Of Double),
                                   Optional epsilon# = Extensions.epsilon,
                                   Optional compact_size% = 1000) As QuantileEstimationGK
            Dim estimator As New QuantileEstimationGK(epsilon, compact_size)

            For Each x As Long In source
                Call estimator.Insert(x)
            Next

            Return estimator
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="quantile#">数量的百分比，值位于0-1之间</param>
        ''' <param name="epsilon#"></param>
        ''' <param name="compact_size%"></param>
        ''' <returns></returns>
        <Extension>
        Public Function QuantileThreshold(data As IEnumerable(Of Double),
                                          quantile#,
                                          Optional epsilon# = Extensions.epsilon,
                                          Optional compact_size% = 1000) As Double
            Dim q As QuantileEstimationGK = data.GKQuantile(epsilon, compact_size)
            Return q.Query(quantile)
        End Function

        ''' <summary>
        ''' 将数值转化为相对应的quantile水平等级
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="steps#"></param>
        ''' <param name="epsilon#"></param>
        ''' <param name="compact_size%"></param>
        ''' <returns></returns>
        <Extension>
        Public Function QuantileLevels(source As IEnumerable(Of Double),
                                       Optional steps# = 0.005,
                                       Optional epsilon# = Extensions.epsilon,
                                       Optional compact_size% = 1000) As Double()

            Dim array#() = source.ToArray
            Dim estimator As New QuantileEstimationGK(epsilon, compact_size, array)
            Dim cuts As New List(Of Double)
            Dim levels As New List(Of Double)  ' 需要返回的是这个相对应的quantile水平

            For q As Double = 0 To 1 Step steps
                cuts += estimator.Query(q)
                levels += q
            Next

            Dim index As New OrderSelector(Of Double)(cuts)

            For i As Integer = 0 To array.Length - 1
                ' 在这里将实际的数据转换为quantile水平
                array(i) = levels(index.FirstGreaterThan(array(i)))
            Next

            Return array
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

        '''' <summary>
        '''' 使用示例
        '''' </summary>
        'Private Sub Test()
        '    Dim shuffle As Long() = New Long(window_size - 1) {}

        '    For i As Integer = 0 To shuffle.Length - 1
        '        shuffle(i) = i
        '    Next

        '    shuffle = shuffle.Shuffles

        '    Dim estimator As QuantileEstimationGK = shuffle.GKQuantile
        '    Dim quantiles As Double() = {0.5, 0.9, 0.95, 0.99, 1.0}

        '    For Each q As Double In quantiles
        '        Dim estimate As Long = estimator.Query(q)
        '        Dim actual As Long = shuffle.actually(q)
        '        Dim out As String = String.Format("Estimated {0:F2} quantile as {1:D} (actually {2:D})", q, estimate, actual)

        '        Call out.__DEBUG_ECHO
        '    Next
        'End Sub

        <Extension>
        Public Function Summary(data As IEnumerable(Of Double)) As String
            Dim v#() = data.ToArray
            Dim q As QuantileEstimationGK = v.GKQuantile
            Dim sb As New StringBuilder

            For Each quantile# In {0.25, 0.5, 0.75, 0.9, 0.95, 0.99, 1}
                Dim estimate# = q.Query(quantile)
                Dim out As String = String.Format("Estimated {0:F2}% quantile as {1}", quantile * 100, estimate)
                sb.AppendLine(out)
            Next

            Return sb.ToString
        End Function
    End Module
End Namespace

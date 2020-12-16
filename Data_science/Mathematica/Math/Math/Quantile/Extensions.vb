#Region "Microsoft.VisualBasic::d5e661546a8baad03991d59967f979ad, Data_science\Mathematica\Math\Math\Quantile\Extensions.vb"

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

    '     Module Extensions
    ' 
    '         Function: debugView, (+2 Overloads) GKQuantile, QuantileLevels, SelectByQuantile, Threshold
    ' 
    '         Sub: Summary
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Quantile

    ''' <summary>
    ''' GK quantile extensions method
    ''' </summary>
    Public Module Extensions

        Public Const epsilon As Double = 0.001

        <Extension>
        Friend Function debugView(q As QuantileQuery) As String
            Return seq(0, 1, 0.25) _
                .ToDictionary(Function(pct) (100 * pct).ToString("F2") & "%",
                              Function(pct)
                                  Return q.Query(pct).ToString("F2")
                              End Function) _
                .GetJson
        End Function

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

        ''' <summary>
        ''' <see cref="QuantileEstimationGK"/> for numeric vector.
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="epsilon#"></param>
        ''' <param name="compact_size%"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GKQuantile(source As IEnumerable(Of Double),
                                   Optional epsilon# = Extensions.epsilon,
                                   Optional compact_size% = 1000) As QuantileEstimationGK
            Dim estimator As New QuantileEstimationGK(epsilon, compact_size)

            For Each x As Double In source
                Call estimator.Insert(x)
            Next

            Return estimator
        End Function

        ''' <summary>
        ''' Measure sample threshold data by a given quantile level.
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="quantile">数量的百分比，值位于0-1之间</param>
        ''' <param name="epsilon"></param>
        ''' <param name="compact_size"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Threshold(data As IEnumerable(Of Double),
                                  quantile#,
                                  Optional epsilon# = Extensions.epsilon,
                                  Optional compact_size% = 1000) As Double

            Return data.GKQuantile(epsilon, compact_size).Query(quantile)
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
                                       Optional steps# = 0.01,
                                       Optional epsilon# = Extensions.epsilon,
                                       Optional compact_size% = 1000,
                                       Optional fast As Boolean = False) As Double()

            Dim array#() = source.ToArray
            Dim estimator As QuantileQuery

            If fast Then
                estimator = New FastRankQuantile(array)
            Else
                estimator = New QuantileEstimationGK(epsilon, compact_size, array)
            End If

            Dim cuts As New List(Of Double)
            Dim levels As New List(Of Double)

            ' 需要返回的是这个相对应的quantile水平
            For q As Double = 0 To 1 Step steps
                cuts += estimator.Query(q)
                levels += q
            Next

            Dim index As New OrderSelector(Of Double)(cuts)

            If fast Then
                array = array.SeqIterator.ToArray _
                    .AsParallel _
                    .Select(Function(xi)
                                Return (levels(index.FirstGreaterThan(xi.value)), xi.i)
                            End Function) _
                    .OrderBy(Function(xi) xi.i) _
                    .Select(Function(xi) xi.Item1) _
                    .ToArray
            Else
                For i As Integer = 0 To array.Length - 1
                    ' 在这里将实际的数据转换为quantile水平
                    array(i) = levels(index.FirstGreaterThan(array(i)))
                Next
            End If

            Return array
        End Function

        Const window_size As Integer = 10000

        ''' <summary>
        ''' Selector for object sequence that by using quantile calculation.(对指定的序列按照所给定的quantile值进行分块)
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

            Dim cache = (From x As T In data Select x, v = getValue(x)).ToArray
            Dim vals As Long() = cache.Select(Function(x) x.v).ToArray
            Dim estimator As QuantileEstimationGK = vals.GKQuantile(epsilon, compact_size)

            For Each q As Double In quantiles
                Dim estimate As Long = estimator.Query(q)
                Dim up As IEnumerable(Of T) = From x
                                              In cache
                                              Where x.v >= estimate
                                              Select x.x

                Yield New DoubleTagged(Of T()) With {
                    .Tag = q,
                    .Value = up.ToArray
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

        Const SummaryTemplate$ = "Estimated {0:F2}% quantile as {1} with {2} sample data (mean:={3}, std:={4})."

        ''' <summary>
        ''' 默认是输出到标准输出上的
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="dev"></param>
        <Extension>
        Public Sub Summary(data As IEnumerable(Of Double), Optional dev As TextWriter = Nothing)
            Dim v As Vector = data.ToArray
            Dim q As QuantileEstimationGK = v.GKQuantile

            With dev Or App.StdOut
                Call .WriteLine()
                Call .WriteLine("# Data summary")
                Call .WriteLine()
                Call .WriteLine($"Total={v.Length}")

                For Each quantile As Double In {0.1, 0.25, 0.5, 0.75, 0.9, 0.95, 0.99, 1}
                    Dim estimate# = q.Query(quantile)
                    Dim lessthan = v(v <= estimate)
                    Dim out$ = String.Format(SummaryTemplate, quantile * 100, estimate, lessthan.Length, lessthan.Average, lessthan.SD)

                    .WriteLine(out)
                Next

                Call .Flush()
            End With
        End Sub
    End Module
End Namespace

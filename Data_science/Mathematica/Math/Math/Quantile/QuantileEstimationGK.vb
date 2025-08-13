#Region "Microsoft.VisualBasic::12719dc7463c2db2a892c8a5935e06ce, Data_science\Mathematica\Math\Math\Quantile\QuantileEstimationGK.vb"

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

    '   Total Lines: 157
    '    Code Lines: 78 (49.68%)
    ' Comment Lines: 53 (33.76%)
    '    - Xml Docs: 60.38%
    ' 
    '   Blank Lines: 26 (16.56%)
    '     File Size: 5.20 KB


    '     Class QuantileEstimationGK
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Query, ToString
    ' 
    '         Sub: compress, (+2 Overloads) Insert
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports std = System.Math

'
'   Copyright 2012 Andrew Wang (andrew@umbrant.com)
'
'   Licensed under the Apache License, Version 2.0 (the "License");
'   you may not use this file except in compliance with the License.
'   You may obtain a copy of the License at
'
'       http://www.apache.org/licenses/LICENSE-2.0
'
'   Unless required by applicable law or agreed to in writing, software
'   distributed under the License is distributed on an "AS IS" BASIS,
'   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
'   See the License for the specific language governing permissions and
'   limitations under the License.
'

Namespace Quantile

    ''' <summary>
    ''' Implementation of the Greenwald and Khanna algorithm for streaming
    ''' calculation of epsilon-approximate quantiles.
    ''' 
    ''' See: 
    ''' 
    ''' > Greenwald and Khanna, "Space-efficient online computation of quantile summaries" in SIGMOD 2001
    ''' </summary>
    Public Class QuantileEstimationGK : Implements QuantileQuery, IEnumerable(Of QuantileThreshold)

        ''' <summary>
        ''' Acceptable % error in percentile estimate
        ''' </summary>
        ReadOnly epsilon As Double
        ''' <summary>
        ''' Total number of items in stream
        ''' </summary>
        Dim count As Integer = 0
        ''' <summary>
        ''' Threshold to trigger a compaction
        ''' </summary>
        ReadOnly compact_size As Integer

        Dim sample As New List(Of X)

        ''' <summary>
        ''' Implementation of the Greenwald and Khanna algorithm for streaming
        ''' calculation of epsilon-approximate quantiles.
        ''' </summary>
        ''' <param name="epsilon">Acceptable % error in percentile estimate</param>
        ''' <param name="compact_size">Threshold to trigger a compaction</param>
        Public Sub New(epsilon#, compact_size%, Optional data As IEnumerable(Of Double) = Nothing)
            Me.compact_size = compact_size
            Me.epsilon = epsilon

            If Not data Is Nothing Then
                For Each x As Double In data
                    Call Insert(x)
                Next
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return Me.debugView
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Insert(v&)
            Call Insert(CDbl(v))
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="v"></param>
        ''' <remarks>
        ''' 对这个函数的调用无法被并行化
        ''' </remarks>
        Public Sub Insert(v#)
            Dim idx As Integer = 0

            For Each i As X In sample
                If i.value > v Then
                    Exit For
                Else
                    idx += 1
                End If
            Next

            Dim delta As Integer

            If idx = 0 OrElse idx = sample.Count Then
                delta = 0
            Else
                delta = CInt(Fix(std.Floor(2 * epsilon * count)))
            End If

            Call sample.Insert(idx, New X(v, 1, delta))

            If sample.Count > compact_size Then
                Call compress()
            End If

            Me.count += 1
        End Sub

        Private Sub compress()
            Dim removed As Integer = 0

            For i As Integer = 0 To sample.Count - 2
                If i = sample.Count OrElse i + 1 = sample.Count Then
                    Exit For
                End If

                Dim x As X = sample(i)
                Dim x1 As X = sample(i + 1)

                ' Merge the items together if we don't need it to maintain the
                ' error bound
                If x.g + x1.g + x1.delta <= std.Floor(2 * epsilon * count) Then
                    x1.g += x.g
                    sample.RemoveAt(i)
                    removed += 1
                End If
            Next
        End Sub

        ''' <summary>
        ''' 使用数量百分比来获取得到对应的阈值，<paramref name="quantile"/>为``[0,1]``之间的百分比值
        ''' </summary>
        ''' <param name="quantile#">``[0,1]``之间的百分比值</param>
        ''' <returns>阈值</returns>
        Public Function Query(quantile#) As Double Implements QuantileQuery.Query
            Dim rankMin As Integer = 0
            Dim desired As Integer = CInt(Fix(quantile * count))

            If sample.Count = 0 Then
                Return 0
            End If

            For i As Integer = 1 To sample.Count - 1
                Dim prev As X = sample(i - 1)
                Dim cur As X = sample(i)

                rankMin += prev.g

                If rankMin + cur.g + cur.delta > desired + (2 * epsilon * count) Then
                    Return prev.value
                End If
            Next

            ' edge case of wanting max value
            Return sample(sample.Count - 1).value
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of QuantileThreshold) Implements IEnumerable(Of QuantileThreshold).GetEnumerator
            For q As Double = 0 To 1 Step 0.1
                Yield New QuantileThreshold With {
                    .quantile = q,
                    .sample = Query(q)
                }
            Next
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function
    End Class
End Namespace

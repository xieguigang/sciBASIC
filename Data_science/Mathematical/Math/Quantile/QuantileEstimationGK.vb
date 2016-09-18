#Region "Microsoft.VisualBasic::18086d353812414eae7396025b805440, ..\visualbasic_App\Scripting\Math\Math\Quantile\QuantileEstimationGK.vb"

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
Imports System.Text
Imports Microsoft.VisualBasic

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
    Public Class QuantileEstimationGK

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

        ''' <summary>
        ''' Implementation of the Greenwald and Khanna algorithm for streaming
        ''' calculation of epsilon-approximate quantiles.
        ''' </summary>
        ''' <param name="epsilon">Acceptable % error in percentile estimate</param>
        ''' <param name="compact_size">Threshold to trigger a compaction</param>
        Public Sub New(epsilon As Double, compact_size As Integer)
            Me.compact_size = compact_size
            Me.epsilon = epsilon
        End Sub

        Dim sample As List(Of X) = New List(Of X)

        Private Sub printList()
            Dim buf As New StringBuilder

            For Each i As X In sample
                buf.Append(String.Format("({0:D} {1:D} {2:D}),", i.value, i.g, i.delta))
            Next

            Call buf.ToString.__DEBUG_ECHO
        End Sub

        Public Overridable Sub insert(v As Long)
            Dim idx As Integer = 0

            For Each i As X In sample
                If i.value > v Then Exit For
                idx += 1
            Next

            Dim delta As Integer

            If idx = 0 OrElse idx = sample.Count Then
                delta = 0
            Else
                delta = CInt(Fix(Math.Floor(2 * epsilon * count)))
            End If

            Dim newItem As New X(v, 1, delta)

            sample.Insert(idx, newItem)

            If sample.Count > compact_size Then
                ' printList()
                compress()
                ' printList()
            End If

            Me.count += 1
        End Sub

        Public Overridable Sub compress()
            Dim removed As Integer = 0

            For i As Integer = 0 To sample.Count - 2
                If i = sample.Count OrElse i + 1 = sample.Count Then
                    Exit For
                End If

                Dim x As X = sample(i)
                Dim x1 As X = sample(i + 1)

                ' Merge the items together if we don't need it to maintain the
                ' error bound
                If x.g + x1.g + x1.delta <= Math.Floor(2 * epsilon * count) Then
                    x1.g += x.g
                    sample.RemoveAt(i)
                    removed += 1
                End If
            Next
        End Sub

        Public Overridable Function query(quantile As Double) As Long
            Dim rankMin As Integer = 0
            Dim desired As Integer = CInt(Fix(quantile * count))

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
    End Class
End Namespace

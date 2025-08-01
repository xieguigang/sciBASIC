#Region "Microsoft.VisualBasic::5f5f3dff42fbd09c2ab26b694f0f0f23, Data_science\Mathematica\Math\Math\DownSampling\TimeGap\TimeGapAlgorithm.vb"

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

    '   Total Lines: 66
    '    Code Lines: 53 (80.30%)
    ' Comment Lines: 3 (4.55%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (15.15%)
    '     File Size: 2.50 KB


    '     Class TimeGapAlgorithm
    ' 
    '         Function: process, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports std = System.Math

Namespace DownSampling.TimeGap

    ''' <summary>
    ''' Find out big gaps between events and select events at both ends of those gaps.
    ''' </summary>
    Public Class TimeGapAlgorithm
        Implements DownSamplingAlgorithm

        Private rate As Double = 1

        Public Overridable Function process(data As IList(Of ITimeSignal), threshold As Integer) As IList(Of ITimeSignal) Implements DownSamplingAlgorithm.process

            If data.Count = 0 OrElse threshold >= data.Count Then
                Return data
            End If
            Dim result As New List(Of ITimeSignal)()

            Dim weighted As IList(Of WeightedEvent) = New List(Of WeightedEvent)()
            Dim avg As Double = (data(data.Count - 1).time - data(0).time) * 1.0 / (data.Count - 1)
            For i As Integer = 0 To data.Count - 1
                Dim we As New WeightedEvent(data(i))
                If i < data.Count - 1 Then
                    Dim delta As Long = data(i + 1).time - data(i).time
                    we.Weight = delta - avg
                End If
                weighted.Add(we)
            Next i

            Dim [set] As ISet(Of ITimeSignal) = New HashSet(Of ITimeSignal)()
            Dim max As Integer = CInt(std.Truncate(threshold * rate))
            Dim multiple As Integer = 1024
            Dim limit As Integer = Integer.MaxValue
            Do While multiple > 2
                For i As Integer = 0 To weighted.Count - 1
                    Dim e As WeightedEvent = weighted(i)
                    Dim m As Double = e.Weight / avg
                    If m > multiple AndAlso m <= limit Then
                        [set].Add(e.Event)
                        If i + 1 < weighted.Count Then
                            [set].Add(weighted(i + 1).Event)
                        End If
                    End If
                    If [set].Count >= max Then
                        GoTo ABreak
                    End If
                Next i
                limit = multiple
                multiple >>= 2
AContinue:
            Loop
ABreak:
            result.AddRange([set])
            result.Sort(AddressOf EventOrder.BY_TIME_ASC)
            Return result
        End Function

        Public Overrides Function ToString() As String
            Return "TIMEGAP"
        End Function

    End Class

End Namespace

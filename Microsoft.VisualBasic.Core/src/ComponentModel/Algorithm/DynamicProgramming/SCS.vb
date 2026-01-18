#Region "Microsoft.VisualBasic::ee7ae9e9c706b6b711041aabcd15093c, Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\DynamicProgramming\SCS.vb"

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

    '   Total Lines: 151
    '    Code Lines: 106 (70.20%)
    ' Comment Lines: 21 (13.91%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 24 (15.89%)
    '     File Size: 5.62 KB


    '     Module SCS
    ' 
    '         Function: Coverage, MaxPrefixLength, runIteration, ShortestCommonSuperString
    ' 
    '         Sub: TableView
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.Algorithm.DynamicProgramming

    ''' <summary>
    ''' shortest_common_superstring
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/aakash01/codebase/blob/60394bf92eb09410c07eec1c4d3c81cf0fc72a70/src/com/aakash/practice/interviewbit_may2017/dynamic_programming/ShortestCommonSuperString.java
    ''' </remarks>
    Public Module SCS

        <Extension>
        Public Sub TableView(fragments As IEnumerable(Of String), SCS$, ByRef print As TextWriter, Optional empty As Char = "."c)
            Dim lines As New List(Of String)

            Call print.WriteLine(SCS)

            For Each str As String In fragments
                Dim start% = InStr(SCS, str) - 1
                Dim ends% = start + str.Length
                Dim lefts = SCS.Length - ends
                Dim view$ = New String(empty, start) & str & New String(empty, lefts)

                lines += view
            Next

            Call print.WriteLine($"#Coverage={Coverage(lines, blank:=empty)}")
            Call print.WriteLine(lines.JoinBy(print.NewLine))
            Call print.Flush()
        End Sub

        ''' <summary>
        ''' 使用重叠程度最高的片段作为统计的标准
        ''' </summary>
        ''' <param name="table"></param>
        ''' <returns></returns>
        Public Function Coverage(table As IEnumerable(Of String), Optional blank As Char = "."c) As Integer
            ' 重叠程度最高，意味着blank是最少的
            Dim lines As Char()() = table.Select(Function(s) s.ToArray).ToArray
            ' 因为都是等长的，所以直接使用第一条作为标准了
            Dim length% = lines(Scan0).Length
            Dim coverages As New List(Of Integer)
            Dim index%

            For i As Integer = 0 To length - 1
                index = i
                coverages += lines _
                    .Where(Function(seq)
                               Return seq(index) <> blank
                           End Function) _
                    .Count
            Next

            Return coverages.Max
        End Function

        ''' <summary>
        ''' Solve using Greedy. For all string find the max common prefix/suffix. Merge those two strings
        ''' and continue it.
        ''' </summary>
        ''' <remarks>
        ''' 当这个函数遇到完全没有重叠的序列片段的时候，是会直接将这个不重叠的片段接到SCS的最末尾的
        ''' </remarks>
        <Extension>
        Public Function ShortestCommonSuperString(strs As IEnumerable(Of String)) As String
            Dim seqs As String() = strs.ToArray
            Dim l As Integer = seqs.Length
            Dim p As Integer
            Dim q As Integer
            Dim finalStr As String

            Do While l > 1
                p = -1
                q = -1
                finalStr = RunIteration(l, seqs, p, q)
                l -= 1
                seqs(p) = finalStr
                seqs(q) = seqs(l)
            Loop

            Return seqs.ElementAtOrDefault(Scan0)
        End Function

        Private Function RunIteration(l As Integer, seqs As String(), ByRef p%, ByRef q%) As String
            Dim currMax As Integer = Integer.MinValue
            Dim finalStr As String = Nothing

            For j As Integer = 0 To l - 1
                For k As Integer = j + 1 To l - 1
                    Dim str As String = seqs(j)
                    Dim b As String = seqs(k)

                    If str.Contains(b) Then
                        If b.Length > currMax Then
                            finalStr = str
                            currMax = b.Length
                            p = j
                            q = k
                        End If
                    ElseIf b.Contains(str) Then
                        If str.Length > currMax Then
                            finalStr = b
                            currMax = str.Length
                            p = j
                            q = k
                        End If
                    Else
                        ' find max common prefix and suffix
                        Dim maxPrefixMatch = MaxPrefixLength(str, b)

                        If maxPrefixMatch > currMax Then
                            finalStr = str & b.Substring(maxPrefixMatch)
                            currMax = maxPrefixMatch
                            p = j
                            q = k
                        End If

                        Dim maxSuffixMatch = MaxPrefixLength(b, str)

                        If maxSuffixMatch > currMax Then
                            finalStr = b & str.Substring(maxSuffixMatch)
                            currMax = maxSuffixMatch
                            p = j
                            q = k
                        End If
                    End If
                Next
            Next

            Return finalStr
        End Function

        Private Function MaxPrefixLength(a As String, b As String) As Integer
            Dim max As Integer = 0
            Dim prefix$

            For i As Integer = 0 To b.Length - 1
                prefix = b.Substring(0, i + 1)

                If a.EndsWith(prefix) Then
                    max = i + 1
                End If
            Next

            Return max
        End Function
    End Module
End Namespace

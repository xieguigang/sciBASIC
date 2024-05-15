#Region "Microsoft.VisualBasic::713273939ca40df842bbd86f21753aea, Data\Trinity\Bigram.vb"

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

    '   Total Lines: 82
    '    Code Lines: 55
    ' Comment Lines: 14
    '   Blank Lines: 13
    '     File Size: 2.58 KB


    ' Class Bigram
    ' 
    '     Properties: count, i, j
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: ParseLine, (+2 Overloads) ParseText, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.Data.NLP.Model
Imports Microsoft.VisualBasic.Linq

Public Class Bigram

    Public Property i As String
    Public Property j As String
    Public Property count As Integer

    Sub New()
    End Sub

    Sub New(tuple As SlideWindow(Of String))
        i = tuple.First
        j = tuple.Last
        count = 1
    End Sub

    Public Overrides Function ToString() As String
        Return $"[{i} - {j}] = {count}"
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="words">a collection of the words, should be segmentation and 
    ''' keeps in the original input orders</param>
    ''' <returns></returns>
    Public Shared Function ParseLine(words As String()) As IEnumerable(Of Bigram)
        Dim hist As New Dictionary(Of String, Bigram)
        Dim key As String

        For Each tuple As SlideWindow(Of String) In words.SlideWindows(2, offset:=1)
            key = tuple.First & "," & tuple.Last

            If Not hist.ContainsKey(key) Then
                ' default count = 1
                hist.Add(key, New Bigram(tuple))
            Else
                hist(key).count += 1
            End If
        Next

        Return hist.Values
    End Function

    ''' <summary>
    ''' implements the bigram parser
    ''' </summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function ParseText(text As String) As IEnumerable(Of Bigram)
        Return ParseText(Paragraph.Segmentation(text))
    End Function

    Public Shared Iterator Function ParseText(data As IEnumerable(Of Paragraph)) As IEnumerable(Of Bigram)
        Dim lines = data.Select(Function(p) p.sentences) _
            .IteratesALL _
            .AsParallel _
            .Select(Function(s)
                        Return ParseLine(s.GetWords.ToArray).ToArray
                    End Function) _
            .ToArray
        ' union counts
        Dim union = lines.IteratesALL.GroupBy(Function(a) $"{a.i},{a.j}")

        For Each bi As IGrouping(Of String, Bigram) In union
            Yield New Bigram With {
                .i = bi.First.i,
                .j = bi.First.j,
                .count = Aggregate t As Bigram
                         In bi
                         Into Sum(t.count)
            }
        Next
    End Function

End Class

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

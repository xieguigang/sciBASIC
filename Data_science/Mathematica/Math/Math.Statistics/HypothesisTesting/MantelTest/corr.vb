Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Hypothesis.Mantel

    Public Module corr

        <Extension>
        Public Function GetCorrelations(mat As Double()(), cor As Func(Of Double(), Double(), (cor As Double, pvalue As Double))) As (cor As Double()(), pvalue As Double()())
            Dim evalData = mat _
                .SeqIterator _
                .AsParallel _
                .Select(Function(d)
                            Dim vec As New List(Of Double)
                            Dim vec2 As New List(Of Double)
                            Dim x As Double() = d.value

                            For Each y As Double() In mat
                                vec += cor(x, y).cor
                                vec2 += cor(x, y).pvalue
                            Next

                            Return (d.i, vec.ToArray, vec2.ToArray)
                        End Function) _
                .OrderBy(Function(d) d.i) _
                .ToArray
            Dim corr As Double()() = evalData.Select(Function(i) i.Item2).ToArray
            Dim pvalue As Double()() = evalData.Select(Function(i) i.Item3).ToArray

            Return (corr, pvalue)
        End Function

        Public Function Pearson(x As Double(), y As Double()) As (cor#, pvalue#)
            Dim pvalue As Double
            Dim corVal = Correlations.GetPearson(x, y, prob:=pvalue)

            Return (corVal, pvalue)
        End Function
    End Module
End Namespace
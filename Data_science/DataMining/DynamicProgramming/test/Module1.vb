Imports Microsoft.VisualBasic.DataMining.DynamicProgramming
Imports Microsoft.VisualBasic.DataMining.DynamicProgramming.NeedlemanWunsch

Module Module1
    Dim q = "AGTCGCCCCGTCCC"
    Dim S = "AGTCGCCCCGTCGG"
    Dim s2 = "AGTCGCCCCGTCGGAAAAAAAAA"
    Dim q1 = "GTCCC"
    Dim q2 = "AGTCGCTCCC"
    Dim q3 = "AGTCGCCCCCCC"

    Sub Main()
        Call scoreTest()
    End Sub

    Sub scoreTest()
        Dim nw As New NeedlemanWunsch(Of Char)(q, q, Function(x, y) Char.ToUpper(x) = Char.ToUpper(y), "-"c, Function(x) x)
        Call nw.compute()
        Dim l = nw.PopulateAlignments.ToArray

        nw = New NeedlemanWunsch(Of Char)(q1, q1, Function(x, y) Char.ToUpper(x) = Char.ToUpper(y), "-"c, Function(x) x)
        Call nw.compute()
        Dim l1 = nw.PopulateAlignments.ToArray

        nw = New NeedlemanWunsch(Of Char)(q2, q2, Function(x, y) Char.ToUpper(x) = Char.ToUpper(y), "-"c, Function(x) x)
        Call nw.compute()
        Dim l2 = nw.PopulateAlignments.ToArray

        nw = New NeedlemanWunsch(Of Char)(q3, q3, Function(x, y) Char.ToUpper(x) = Char.ToUpper(y), "-"c, Function(x) x)
        Call nw.compute()
        Dim l3 = nw.PopulateAlignments.ToArray

        nw = New NeedlemanWunsch(Of Char)(q, S, Function(x, y) Char.ToUpper(x) = Char.ToUpper(y), "-"c, Function(x) x)
        Call nw.compute()
        Dim qs = nw.PopulateAlignments.ToArray

        nw = New NeedlemanWunsch(Of Char)(q, s2, Function(x, y) Char.ToUpper(x) = Char.ToUpper(y), "-"c, Function(x) x)
        Call nw.compute()
        Dim qs2 = nw.PopulateAlignments.ToArray

        Pause()
    End Sub

End Module

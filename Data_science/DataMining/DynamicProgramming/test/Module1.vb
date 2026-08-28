#Region "Microsoft.VisualBasic::1315b2ebc0434929d7963011e68a9241, Data_science\DataMining\DynamicProgramming\test\Module1.vb"

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

    '   Total Lines: 44
    '    Code Lines: 16 (36.36%)
    ' Comment Lines: 18 (40.91%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (22.73%)
    '     File Size: 1.67 KB


    ' Module Module1
    ' 
    '     Sub: Main, scoreTest
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming
Imports Microsoft.VisualBasic.DataMining.DynamicProgramming
Imports Microsoft.VisualBasic.DataMining.DynamicProgramming.NeedlemanWunsch
Imports std = System.Math

Module Module1
    Dim q = "AGTCGCCCCGTCCC"
    Dim S = "AGTCGCCCCGTCGG"
    Dim s2 = "AGTCGCCCCGTCGGAAAAAAAAA"
    Dim q1 = "GTCCC"
    Dim q2 = "AGTCGCTCCC"
    Dim q3 = "AGTCGCCCCCCC"

    ''' <summary>
    ''' 0 - Match, 1 - Missmatch, 2 - Indel
    ''' </summary>
    Private Class SimpleScore : Implements IScore(Of Char)

        Public Function GetSimilarityScore(a As Char, b As Char) As Double Implements IScore(Of Char).GetSimilarityScore
            If a = CenterStar.GapChar AndAlso b = CenterStar.GapChar Then
                Return 0
            ElseIf a = CenterStar.GapChar OrElse b = CenterStar.GapChar Then
                Return 2
            Else
                Return If(a = b, 0, 1)
            End If
        End Function
    End Class

    ''' <summary>
    ''' 不带任何带宽限制的参考实现
    ''' </summary>
    Private Function EditDistance(a As String, b As String) As Integer
        Dim l1 As Integer = a.Length
        Dim l2 As Integer = b.Length
        Dim d As Integer(,) = New Integer(l1, l2) {}

        For i As Integer = 0 To l1
            d(i, 0) = i
        Next
        For j As Integer = 0 To l2
            d(0, j) = j
        Next

        For i As Integer = 1 To l1
            For j As Integer = 1 To l2
                Dim cost As Integer = If(a(i - 1) = b(j - 1), 0, 1)

                d(i, j) = std.Min(std.Min(d(i - 1, j) + 1, d(i, j - 1) + 1), d(i - 1, j - 1) + cost)
            Next
        Next

        Return d(l1, l2)
    End Function

    ''' <summary>
    ''' O(n^2 * L) 的 SP 得分参考实现
    ''' </summary>
    Private Function BruteForceSP(align As String(), score As IScore(Of Char)) As Double
        Dim n As Integer = align.Length
        Dim L As Integer = align(0).Length
        Dim total# = 0

        For i As Integer = 0 To n - 1
            For j As Integer = i + 1 To n - 1
                For k As Integer = 0 To L - 1
                    total += score.GetSimilarityScore(align(i)(k), align(j)(k))
                Next
            Next
        Next

        Return total
    End Function

    ''' <summary>
    ''' 返回 Nothing 表示通过，否则返回失败原因
    ''' </summary>
    Private Function CheckAlignment(input As String(), align As String()) As String
        Dim L As Integer = align(0).Length

        For i As Integer = 0 To align.Length - 1
            If align(i).Length <> L Then
                Return $"row #{i} length {align(i).Length} <> {L}"
            End If

            Dim raw As String = New String(align(i).Where(Function(c) c <> CenterStar.GapChar).ToArray())

            If raw <> input(i) Then
                Return $"row #{i} does not restore its input sequence"
            End If
        Next

        Return Nothing
    End Function

    Private Function RandomSeq(rnd As Random, len As Integer) As String
        Dim bases As String = "ACGT"
        Dim sb As New StringBuilder()

        For i As Integer = 1 To len
            sb.Append(bases(rnd.Next(4)))
        Next

        Return sb.ToString()
    End Function

    Private Function Mutate(rnd As Random, seq As String) As String
        Dim bases As String = "ACGT"
        Dim sb As New StringBuilder()

        For Each c As Char In seq
            Dim r As Integer = rnd.Next(100)

            If r < 4 Then
                sb.Append(bases(rnd.Next(4)))
                sb.Append(c)
            ElseIf r < 8 Then
                ' deletion
            ElseIf r < 14 Then
                sb.Append(bases(rnd.Next(4)))
            Else
                sb.Append(c)
            End If
        Next

        Return sb.ToString()
    End Function

    Private failures As Integer = 0

    Private Sub Check(condition As Boolean, message As String)
        If condition Then
            Call Console.WriteLine($"  [ OK ] {message}")
        Else
            failures += 1
            Call Console.WriteLine($"  [FAIL] {message}")
        End If
    End Sub

    Sub Main()
        Call scoreTest()
        Call kbandTest()
        Call centerStarTest()

        Call Console.WriteLine()
        Call Console.WriteLine(If(failures = 0, "ALL TESTS PASSED", $"{failures} TEST(S) FAILED"))
        Call Pause()
    End Sub

    ''' <summary>
    ''' KBandSearch 与不带带宽限制的参考实现对照，
    ''' 同时覆盖 K 足够大与 K 需要自动扩大两种情形
    ''' </summary>
    Sub kbandTest()
        Call Console.WriteLine("== KBandSearch vs. full DP ==")

        Dim rnd As New Random(20260829)
        Dim distMismatch As Integer = 0
        Dim score As New SimpleScore()

        For t As Integer = 1 To 300
            Dim a As String = RandomSeq(rnd, rnd.Next(0, 120))
            Dim b As String = RandomSeq(rnd, rnd.Next(0, 120))

            If t Mod 3 = 0 Then
                b = Mutate(rnd, a)
            End If

            Dim expected As Integer = EditDistance(a, b)

            For Each k As Integer In New Integer() {1, 2, 8, 32, 1000}
                Dim buf As String() = New String(2) {}
                Dim machine As New KBandSearch(globalAlign:=buf, k)
                Dim actual As Integer = machine.CalculateEditDistance(a, b)

                If actual <> expected Then
                    distMismatch += 1
                    Call Console.WriteLine($"  [FAIL] k={k}: {actual} <> {expected}")
                End If

                ' globalAlign 是 Friend 成员，这里借助 CenterStar 的两序列比对
                ' 间接校验回溯结果能够还原输入
                Dim pair As String() = {a, b}
                Dim aligned As String() = Nothing
                Dim edits As Integer() = Nothing

                Call New CenterStar(pair, kband:=k).Compute(score, aligned, edits)

                Dim err As String = CheckAlignment(pair, aligned)

                If err IsNot Nothing Then
                    distMismatch += 1
                    Call Console.WriteLine($"  [FAIL] k={k}: {err}")
                End If
            Next
        Next

        Call Check(distMismatch = 0, $"edit distance matches the full DP and the traceback restores the inputs ({distMismatch} mismatches)")
        Call Console.WriteLine()
    End Sub

    Sub centerStarTest()
        Call Console.WriteLine("== CenterStar MSA ==")

        Dim score As New SimpleScore()

        ' 1. 教科书样例
        Dim input1 As String() = {"ACG", "ATCG", "AG"}
        Dim align1 As String() = Nothing
        Dim edits1 As Integer() = Nothing
        Dim cost1 As Double = New CenterStar(input1).Compute(score, align1, edits1)

        Call Check(CheckAlignment(input1, align1) Is Nothing, $"sample ACG/ATCG/AG: {CheckAlignment(input1, align1)}")
        Call Check(std.Abs(cost1 - BruteForceSP(align1, score)) < 0.0000001, $"sample SP score {cost1} = brute force {BruteForceSP(align1, score)}")

        ' 2. 随机数据集：精确中心选择
        Dim rnd As New Random(1234567)

        For round As Integer = 1 To 8
            Dim n As Integer = 4 + rnd.Next(20)
            Dim ancestor As String = RandomSeq(rnd, rnd.Next(80, 400))
            Dim seqs As String() = New String(n - 1) {}

            For i As Integer = 0 To n - 1
                seqs(i) = Mutate(rnd, ancestor)
            Next

            Dim align As String() = Nothing
            Dim edits As Integer() = Nothing
            Dim cost As Double = New CenterStar(seqs, kband:=32, exactCenterLimit:=1024).Compute(score, align, edits)
            Dim err As String = CheckAlignment(seqs, align)
            Dim brute As Double = BruteForceSP(align, score)

            Call Check(err Is Nothing, $"round {round} (n={n}): {err}")
            Call Check(std.Abs(cost - brute) < 0.0000001, $"round {round} SP score {cost} = brute force {brute}")
        Next

        ' 3. 随机数据集：采样近似的中心选择
        For round As Integer = 1 To 4
            Dim n As Integer = 33 + rnd.Next(40)
            Dim ancestor As String = RandomSeq(rnd, rnd.Next(80, 300))
            Dim seqs As String() = New String(n - 1) {}

            For i As Integer = 0 To n - 1
                seqs(i) = Mutate(rnd, ancestor)
            Next

            Dim align As String() = Nothing
            Dim edits As Integer() = Nothing
            Dim cost As Double = New CenterStar(seqs, kband:=32, exactCenterLimit:=32).Compute(score, align, edits)
            Dim err As String = CheckAlignment(seqs, align)
            Dim brute As Double = BruteForceSP(align, score)

            Call Check(err Is Nothing, $"sampled round {round} (n={n}): {err}")
            Call Check(std.Abs(cost - brute) < 0.0000001, $"sampled round {round} SP score {cost} = brute force {brute}")
        Next

        ' 4. 边界情形
        Dim empty As String() = Nothing
        Dim cost0 As Double = New CenterStar(New String() {}).Compute(score, empty)
        Call Check(cost0 = 0 AndAlso empty IsNot Nothing AndAlso empty.Length = 0, "empty input set")

        Dim single1 As String() = Nothing
        Dim costS As Double = New CenterStar({"ACGT"}).Compute(score, single1)
        Call Check(costS = 0 AndAlso single1.Length = 1 AndAlso single1(0) = "ACGT", "single sequence")

        Dim same As String() = Nothing
        Dim costSame As Double = New CenterStar({"ACGT", "ACGT", "ACGT"}).Compute(score, same)
        Call Check(costSame = 0 AndAlso same.Length = 3, "identical sequences")

        Dim blank As String() = Nothing
        Dim input4 As String() = {"", "ACGT", "AC"}
        Dim costB As Double = New CenterStar(input4).Compute(score, blank)
        Call Check(CheckAlignment(input4, blank) Is Nothing, $"empty string member: {CheckAlignment(input4, blank)}")

        Call Console.WriteLine()
    End Sub

    Sub scoreTest()
        'Dim nw As New NeedlemanWunsch(Of Char)(q, q, Function(x, y) Char.ToUpper(x) = Char.ToUpper(y), "-"c, Function(x) x)
        'Call nw.compute()
        'Dim l = nw.PopulateAlignments.ToArray

        'nw = New NeedlemanWunsch(Of Char)(q1, q1, Function(x, y) Char.ToUpper(x) = Char.ToUpper(y), "-"c, Function(x) x)
        'Call nw.compute()
        'Dim l1 = nw.PopulateAlignments.ToArray

        'nw = New NeedlemanWunsch(Of Char)(q2, q2, Function(x, y) Char.ToUpper(x) = Char.ToUpper(y), "-"c, Function(x) x)
        'Call nw.compute()
        'Dim l2 = nw.PopulateAlignments.ToArray

        'nw = New NeedlemanWunsch(Of Char)(q3, q3, Function(x, y) Char.ToUpper(x) = Char.ToUpper(y), "-"c, Function(x) x)
        'Call nw.compute()
        'Dim l3 = nw.PopulateAlignments.ToArray

        'nw = New NeedlemanWunsch(Of Char)(q, S, Function(x, y) Char.ToUpper(x) = Char.ToUpper(y), "-"c, Function(x) x)
        'Call nw.compute()
        'Dim qs = nw.PopulateAlignments.ToArray

        'nw = New NeedlemanWunsch(Of Char)(q, s2, Function(x, y) Char.ToUpper(x) = Char.ToUpper(y), "-"c, Function(x) x)
        'Call nw.compute()
        'Dim qs2 = nw.PopulateAlignments.ToArray

        Pause()
    End Sub

End Module

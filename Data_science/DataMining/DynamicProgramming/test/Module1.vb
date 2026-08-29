#Region "Microsoft.VisualBasic::a7d46fb211c6b1748b4091e59fa041e0, Data_science\DataMining\DynamicProgramming\test\Module1.vb"

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

    '   Total Lines: 196
    '    Code Lines: 156 (79.59%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 40 (20.41%)
    '     File Size: 7.00 KB


    ' Module Module1
    ' 
    '     Function: BruteForceSP, CheckAlignment, EditDistance, Mutate, RandomSeq
    ' 
    '     Sub: Check, Main
    '     Class SimpleScore
    ' 
    '         Function: GetSimilarityScore
    ' 
    ' 
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
                d(i, j) = std.Min(std.Min(d(i - 1, j) + 1, d(i, j - 1) + 1), d(i - 1, j - 1) + If(a(i - 1) = b(j - 1), 0, 1))
            Next
        Next

        Return d(l1, l2)
    End Function

    Private Function BruteForceSP(align As String(), score As IScore(Of Char)) As Double
        Dim total# = 0

        For i As Integer = 0 To align.Length - 1
            For j As Integer = i + 1 To align.Length - 1
                For k As Integer = 0 To align(0).Length - 1
                    total += score.GetSimilarityScore(align(i)(k), align(j)(k))
                Next
            Next
        Next

        Return total
    End Function

    Private Function CheckAlignment(input As String(), align As String()) As String
        For i As Integer = 0 To align.Length - 1
            If align(i).Length <> align(0).Length Then
                Return $"row #{i} length mismatch"
            End If

            Dim raw As String = New String(align(i).Where(Function(c) c <> CenterStar.GapChar).ToArray())

            If raw <> input(i) Then
                Return $"row #{i} does not restore its input"
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
        Dim score As New SimpleScore()
        Dim rnd As New Random(20260829)
        Dim distMismatch As Integer = 0

        For t As Integer = 1 To 200
            Dim a As String = RandomSeq(rnd, rnd.Next(0, 120))
            Dim b As String = RandomSeq(rnd, rnd.Next(0, 120))

            If t Mod 3 = 0 Then
                b = Mutate(rnd, a)
            End If

            Dim expected As Integer = EditDistance(a, b)
            Dim full As Integer = New KBandSearch(globalAlign:=New String(2) {}, 1000).CalculateEditDistance(a, b)

            If full <> expected Then
                distMismatch += 1
            End If

            For Each k As Integer In New Integer() {1, 2, 8, 32}
                If New KBandSearch(globalAlign:=New String(2) {}, k).CalculateEditDistance(a, b) < expected Then
                    distMismatch += 1
                End If
            Next
        Next

        Call Check(distMismatch = 0, $"KBandSearch matches the full DP / stays an upper bound ({distMismatch} mismatches)")

        Dim sample As String() = {"ACG", "ATCG", "AG"}
        Dim a1 As String() = Nothing
        Dim e1 As Integer() = Nothing
        Dim c1 As Double = New CenterStar(sample).Compute(score, a1, e1)

        Call Check(CheckAlignment(sample, a1) Is Nothing, $"sample: {CheckAlignment(sample, a1)}")
        Call Check(std.Abs(c1 - BruteForceSP(a1, score)) < 0.0000001, $"sample SP {c1} = {BruteForceSP(a1, score)}")

        For round As Integer = 1 To 6
            Dim n As Integer = 4 + rnd.Next(60)
            Dim ancestor As String = RandomSeq(rnd, rnd.Next(80, 400))
            Dim seqs As String() = New String(n - 1) {}

            For i As Integer = 0 To n - 1
                seqs(i) = Mutate(rnd, ancestor)
            Next

            Dim align As String() = Nothing
            Dim edits As Integer() = Nothing
            Dim cost As Double = New CenterStar(seqs, kband:=32).Compute(score, align, edits)
            Dim err As String = CheckAlignment(seqs, align)
            Dim brute As Double = BruteForceSP(align, score)

            Call Check(err Is Nothing, $"n={n}: {err}")
            Call Check(std.Abs(cost - brute) < 0.0000001, $"n={n} SP {cost} = {brute}")
        Next

        For Each spec As Integer() In New Integer()() {
            New Integer() {100, 300},
            New Integer() {200, 500},
            New Integer() {400, 1000}
        }
            Dim n As Integer = spec(0)
            Dim ancestor As String = RandomSeq(rnd, spec(1))
            Dim seqs As String() = New String(n - 1) {}

            For i As Integer = 0 To n - 1
                seqs(i) = Mutate(rnd, ancestor)
            Next

            Dim align As String() = Nothing
            Dim edits As Integer() = Nothing
            Dim watch As Stopwatch = Stopwatch.StartNew()
            Dim cost As Double = New CenterStar(seqs, kband:=32).Compute(score, align, edits)

            watch.Stop()

            Call Check(CheckAlignment(seqs, align) Is Nothing, $"n={n}, len~{spec(1)}: {watch.ElapsedMilliseconds} ms, columns={align(0).Length}, SP={cost}")
        Next

        Call Console.WriteLine(If(failures = 0, "ALL TESTS PASSED", $"{failures} TEST(S) FAILED"))
    End Sub
End Module

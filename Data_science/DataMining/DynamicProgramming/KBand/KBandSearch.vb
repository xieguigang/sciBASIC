#Region "Microsoft.VisualBasic::7b0b8647e0c7a8a81aa327c40b35e874, Data_science\DataMining\DynamicProgramming\KBand\KBandSearch.vb"

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

    '   Total Lines: 195
    '    Code Lines: 114 (58.46%)
    ' Comment Lines: 47 (24.10%)
    '    - Xml Docs: 40.43%
    ' 
    '   Blank Lines: 34 (17.44%)
    '     File Size: 6.50 KB


    ' Class KBandSearch
    ' 
    '     Properties: K, globalAlign
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: AlignBanded, Backtrace, CalculateEditDistance
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports std = System.Math

''' <summary>
''' Global alignment (Needleman-Wunsch) with a k-band heuristic.
''' 
''' Only cells with |i - j| &lt;= k are evaluated, which turns the O(l1*l2) 
''' dynamic programming into O(l1*min(l2, 2k)) time.
''' 
''' The implementation keeps two rolling row buffers instead of the full 
''' score matrix, so that the working set is O(k) rather than O(l1*l2).
''' </summary>
Public Class KBandSearch

    ''' <summary>
    ''' The k-band width requested by the caller.
    ''' 
    ''' The effective width used internally may be enlarged automatically when 
    ''' the requested one is too small to make the target cell reachable, see 
    ''' <see cref="CalculateEditDistance(String, String)"/>.
    ''' </summary>
    ''' <returns></returns>
    Friend ReadOnly K As Integer
    ''' <summary>
    ''' The output buffer of the last alignment, slot 0 is the aligned first 
    ''' sequence and slot 1 is the aligned second sequence.
    ''' </summary>
    ''' <remarks>
    ''' This buffer is shared by all of the alignments made by this instance, 
    ''' i.e. the class is NOT thread safe, each worker thread requires its own 
    ''' instance.
    ''' </remarks>
    Friend ReadOnly globalAlign As String()

    ''' <summary>
    ''' A cell that is out of the k-band, or that has not been computed yet.
    ''' 
    ''' NOTE: this sentinel must never take part in any arithmetic expression, 
    ''' adding one to it overflows into <see cref="Integer.MinValue"/> and would 
    ''' make an unreachable direction look like the cheapest one.
    ''' </summary>
    Private Const UNREACHABLE As Integer = Integer.MaxValue

    ''' <summary>
    ''' open the k-band alignment search engine
    ''' </summary>
    ''' <param name="globalAlign">A buffer of at least two slots for receiving 
    ''' the alignment result of the last <see cref="CalculateEditDistance(String, String)"/> 
    ''' call.</param>
    ''' <param name="k">The k-band width, a small value runs faster but may miss 
    ''' the optimal path when the two sequences are highly divergent.</param>
    Sub New(ByRef globalAlign$(), k As Integer)
        Me.K = If(k > 0, k, 1)
        Me.globalAlign = globalAlign
    End Sub

    ''' <summary>
    ''' Global alignment and function to calculate the edit distances
    ''' 
    ''' + 0   diagonal
    ''' + 1   left
    ''' + 2   up
    ''' 
    ''' </summary>
    ''' <param name="seq1$"></param>
    ''' <param name="seq2$"></param>
    ''' <returns>The edit distance between <paramref name="seq1"/> and 
    ''' <paramref name="seq2"/>, the two aligned strings are written into the 
    ''' <see cref="globalAlign"/> buffer.</returns>
    ''' <remarks>
    ''' A k-band that is narrower than the length difference |l1 - l2| can never 
    ''' reach the target cell. Instead of throwing, the band is enlarged 
    ''' automatically here (in the worst case up to the full matrix, i.e. an exact 
    ''' Needleman-Wunsch alignment).
    ''' </remarks>
    Public Function CalculateEditDistance(seq1$, seq2$) As Integer
        Dim l1 As Integer = seq1.Length
        Dim l2 As Integer = seq2.Length

        If seq1 = seq2 Then
            globalAlign(0) = seq1
            globalAlign(1) = seq2

            Return 0
        End If

        Dim k As Integer = Me.K
        Dim required As Integer = std.Abs(l1 - l2)
        Dim limit As Integer = std.Max(l1, l2)

        ' the band should at least cover the length difference, otherwise
        ' the cell (l1, l2) is located outside of the band and unreachable
        If k < required Then
            k = required
        End If
        If k > limit Then
            k = limit
        End If

        ' k >= |l1 - l2| already guarantees reachability, the retry loop below
        ' is only a defensive net for pathological inputs
        Do
            Dim dist As Integer = AlignBanded(seq1, seq2, l1, l2, k)

            If dist >= 0 Then
                Return dist
            End If
            If k >= limit Then
                Exit Do
            End If

            k = std.Min(limit, std.Max(k * 2, k + 1))
        Loop

        Return AlignBanded(seq1, seq2, l1, l2, limit)
    End Function

    ''' <summary>
    ''' Fill the k-band and backtrace the optimal global alignment.
    ''' </summary>
    ''' <returns>
    ''' The edit distance, or a negative value when the target cell (l1, l2) 
    ''' turns out to be unreachable inside the band of width <paramref name="k"/>.
    ''' </returns>
    ''' 
    ''' <remarks>
    ''' Row buffers are addressed by the column window of the current row: row i 
    ''' covers the columns [jStart, jEnd] and stores cell (i, j) at the slot 
    ''' (j - jStart) + 1. Slot 0 and slot (w + 1) are permanent sentinels that 
    ''' always hold <see cref="UNREACHABLE"/>, so that the inner loop needs no 
    ''' boundary check at all.
    ''' 
    ''' As the window of row i-1 is shifted by at most one column against the 
    ''' window of row i, the neighbouring cells are located at:
    ''' 
    '''   diag (i-1, j-1) => prev[cj + delta]
    '''   up   (i-1, j)   => prev[cj + delta + 1]
    '''   left (i, j-1)   => cur[cj]
    '''   
    ''' in which delta = jStart(i) - jStart(i-1) is either 0 or 1.
    ''' </remarks>
    Private Function AlignBanded(seq1$, seq2$, l1 As Integer, l2 As Integer, k As Integer) As Integer
        Dim maxW As Integer

        If k >= l2 Then
            ' the band is wider than the whole matrix, no restriction at all
            maxW = l2 + 1
        Else
            maxW = std.Min(l2 + 1, 2 * k + 1)
        End If

        Dim stride As Integer = maxW + 2
        Dim traceSize As Long = CLng(l1 + 1) * stride

        If traceSize > Integer.MaxValue Then
            Throw New OutOfMemoryException($"k-band alignment of {l1} x {l2} requires {traceSize} bytes of traceback buffer, which is too large to be allocated.")
        End If

        Dim prev As Integer() = New Integer(stride - 1) {}
        Dim cur As Integer() = New Integer(stride - 1) {}
        Dim trace As Byte() = New Byte(CInt(traceSize) - 1) {}

        For t As Integer = 0 To stride - 1
            prev(t) = UNREACHABLE
            cur(t) = UNREACHABLE
        Next

        ' row 0: jStart is 0 here, so that only the columns j <= k are in band
        Dim prevJStart As Integer = 0
        Dim prevW As Integer = std.Min(l2, k) + 1

        For j As Integer = 0 To prevW - 1
            cur(j + 1) = j
            trace(j + 1) = 1 ' Left
        Next

        Dim w As Integer = prevW

        For i As Integer = 1 To l1
            Dim swap As Integer() = prev
            prev = cur
            cur = swap

            Dim jStart As Integer = std.Max(0, i - k)
            Dim jEnd As Integer = std.Min(l2, i + k)
            Dim delta As Integer = jStart - prevJStart

            w = jEnd - jStart + 1

            Dim rowBase As Integer = i * stride

            For cj As Integer = 0 To w - 1
                Dim j As Integer = jStart + cj
                Dim idx As Integer = cj + 1
                Dim best As Integer
                Dim dir As Integer = 0

                ' Diagonal (i-1, j-1)
                Dim diag As Integer = prev(cj + delta)

                If diag = UNREACHABLE Then
                    best = UNREACHABLE
                Else
                    best = diag + If(seq1(i - 1) = seq2(j - 1), 0, 1)
                End If

                ' Up (i-1, j): only valid when the source cell is inside the 
                ' window of the previous row
                Dim upIdx As Integer = cj + delta + 1

                If upIdx <= prevW Then
                    Dim up As Integer = prev(upIdx)

                    ' the guard also keeps UNREACHABLE + 1 from overflowing
                    If up <> UNREACHABLE AndAlso up + 1 < best Then
                        best = up + 1
                        dir = 2 ' Up
                    End If
                End If

                ' Left (i, j-1)
                Dim left As Integer = cur(cj)

                If left <> UNREACHABLE AndAlso left + 1 < best Then
                    best = left + 1
                    dir = 1 ' Left
                End If

                cur(idx) = best
                trace(rowBase + idx) = CByte(dir)
            Next

            ' the right sentinel: the window shrinks near the end of the matrix, 
            ' which would leave a stale score of a previous row in that slot
            cur(w + 1) = UNREACHABLE

            prevJStart = jStart
            prevW = w
        Next

        ' cur holds row l1, or row 0 when l1 is zero
        Dim lastIdx As Integer = (l2 - std.Max(0, l1 - k)) + 1

        If lastIdx > w OrElse cur(lastIdx) = UNREACHABLE Then
            Return -1
        Else
            Return Backtrace(trace, stride, k, l1, l2, seq1, seq2, cur(lastIdx))
        End If
    End Function

    ''' <summary>
    ''' Walk the traceback buffer back from (l1, l2) to (0, 0) and build the two 
    ''' aligned strings.
    ''' </summary>
    Private Function Backtrace(trace As Byte(), stride As Integer, k As Integer,
                               l1 As Integer, l2 As Integer,
                               seq1$, seq2$, dist As Integer) As Integer

        Dim i As Integer = l1
        Dim j As Integer = l2
        Dim len As Integer = std.Max(1, l1 + l2)
        Dim align1 As Char() = New Char(len - 1) {}
        Dim align2 As Char() = New Char(len - 1) {}
        Dim pos As Integer = 0

        While i > 0 OrElse j > 0
            Dim jStart As Integer = std.Max(0, i - k)
            Dim t As Integer = trace(i * stride + (j - jStart) + 1)

            If t = 1 Then ' Left
                align1(pos) = CenterStar.GapChar
                align2(pos) = seq2(j - 1)
                j -= 1
            ElseIf t = 2 Then ' Up
                align1(pos) = seq1(i - 1)
                align2(pos) = CenterStar.GapChar
                i -= 1
            Else ' Diagonal
                align1(pos) = seq1(i - 1)
                align2(pos) = seq2(j - 1)
                i -= 1
                j -= 1
            End If

            pos += 1
        End While

        ' 反转字符串
        Dim sb1 As New StringBuilder(len)
        Dim sb2 As New StringBuilder(len)

        For p As Integer = pos - 1 To 0 Step -1
            sb1.Append(align1(p))
            sb2.Append(align2(p))
        Next

        globalAlign(0) = sb1.ToString()
        globalAlign(1) = sb2.ToString()

        Return dist
    End Function
End Class

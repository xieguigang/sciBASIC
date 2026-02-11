#Region "Microsoft.VisualBasic::032ca55c7864f86b1957c69283413ebc, Data_science\DataMining\DynamicProgramming\KBand\KBandSearch.vb"

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
    '     File Size: 6.47 KB


    ' Class KBandSearch
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Backtrace, CalculateEditDistance
    ' 
    '     Sub: FillBand
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports std = System.Math

Public Class KBandSearch

    ''' <summary>
    ''' k-band 带宽
    ''' </summary>
    Friend ReadOnly K As Integer
    Friend ReadOnly globalAlign As String()

    Sub New(ByRef globalAlign$(), k As Integer)
        Me.K = k
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
    ''' <returns></returns>
    Public Function CalculateEditDistance(seq1$, seq2$) As Integer
        Dim l1 = seq1.Length
        Dim l2 = seq2.Length

        If seq1 = seq2 Then
            globalAlign(0) = seq1
            globalAlign(1) = seq2

            Return 0
        End If

        ' K-Band 只计算 |i - j| <= K 的区域
        ' 为了代码简单，我们仍然分配 l1+1 * l2+1 的矩阵，但只更新带内区域
        ' 如果追求极致空间优化，可以使用偏移量映射的二维数组，但实现较复杂

        Dim score(l1, l2) As Integer
        Dim trace(l1, l2) As Integer

        ' 初始化为最大值，表示不可达
        For i As Integer = 0 To l1
            For j As Integer = 0 To l2
                score(i, j) = Integer.MaxValue
            Next
        Next

        ' 原点
        score(0, 0) = 0

        ' 初始化边界
        ' 只有在带内的边界才需要赋值
        For i As Integer = 1 To l1
            ' j=0, 必须满足 |i - 0| <= K，即 i <= K
            If i <= K Then
                score(i, 0) = i
                trace(i, 0) = 2 ' Up
            End If
        Next

        For j As Integer = 1 To l2
            ' i=0, 必须满足 |0 - j| <= K，即 j <= K
            If j <= K Then
                score(0, j) = j
                trace(0, j) = 1 ' Left
            End If
        Next

        ' 填充带状区域
        Call FillBand(seq1, seq2, score, trace)

        ' 回溯
        ' 从 (l1, l2) 开始。如果最优路径跑出了带宽，这个单元格可能是 MaxValue。
        ' 简单处理：如果 (l1,l2) 是 MaxValue，说明 K 太小了。
        ' 实际应用中可能需要动态增大 K 重算，这里为了简单，如果不可达就抛出异常或回退到最近的可行点（不推荐）。
        ' 我们假设 K 足够大以至于 (l1, l2) 可达。

        If score(l1, l2) = Integer.MaxValue Then
            Throw New Exception("K-Band width is too small to align these sequences.")
        Else
            Return Backtrace(score, trace, l1, l2, seq1, seq2)
        End If
    End Function

    ''' <summary>
    ''' 填充带状区域
    ''' </summary>
    ''' <param name="seq1"></param>
    ''' <param name="seq2"></param>
    ''' <param name="score"></param>
    ''' <param name="trace"></param>
    Private Sub FillBand(seq1$, seq2$, ByRef score As Integer(,), ByRef trace As Integer(,))
        Dim l1 = seq1.Length
        Dim l2 = seq2.Length

        For i As Integer = 1 To l1
            ' 确定 j 的范围: [max(1, i-K), min(l2, i+K)]
            Dim jStart As Integer = std.Max(1, i - K)
            Dim jEnd As Integer = std.Min(l2, i + K)

            For j As Integer = jStart To jEnd
                Dim matchCost As Integer = If(seq1(i - 1) = seq2(j - 1), 0, 1)

                ' 计算三个方向的代价，如果来源在带外（值为 MaxValue），则忽略该方向

                ' Diagonal (i-1, j-1)
                ' (i-1) - (j-1) = i - j，所以在带内肯定有效，只要 score 有效
                Dim diagScore As Integer = score(i - 1, j - 1)

                ' Up (i-1, j)
                ' 检查 (i-1) - j 是否在带内 => |(i-1) - j| <= K
                Dim upScore As Integer = Integer.MaxValue
                If std.Abs((i - 1) - j) <= K Then
                    upScore = score(i - 1, j)
                End If

                ' Left (i, j-1)
                ' 检查 i - (j-1) 是否在带内 => |i - (j-1)| <= K
                Dim leftScore As Integer = Integer.MaxValue

                If std.Abs(i - (j - 1)) <= K Then
                    leftScore = score(i, j - 1)
                End If

                ' 取最小值
                Dim minScore As Integer = diagScore + matchCost
                Dim direction As Integer = 0 ' Diagonal

                If upScore + 1 < minScore Then
                    minScore = upScore + 1
                    direction = 2 ' Up
                End If
                If leftScore + 1 < minScore Then
                    minScore = leftScore + 1
                    direction = 1 ' Left
                End If

                score(i, j) = minScore
                trace(i, j) = direction
            Next
        Next
    End Sub

    Private Function Backtrace(score As Integer(,), trace As Integer(,), l1 As Integer, l2 As Integer, seq1$, seq2$) As Integer
        Dim i As Integer = l1
        Dim j As Integer = l2
        Dim len As Integer = l1 + l2 ' 最大可能长度
        Dim align1(len - 1) As Char
        Dim align2(len - 1) As Char
        Dim pos As Integer = 0

        While i > 0 OrElse j > 0
            Dim t As Integer = trace(i, j)

            If t = 0 Then ' Diagonal
                align1(pos) = seq1(i - 1)
                align2(pos) = seq2(j - 1)
                i -= 1
                j -= 1
            ElseIf t = 1 Then ' Left
                align1(pos) = CenterStar.GapChar
                align2(pos) = seq2(j - 1)
                j -= 1
            ElseIf t = 2 Then ' Up
                align1(pos) = seq1(i - 1)
                align2(pos) = CenterStar.GapChar
                i -= 1
            Else
                ' 不应该发生，除非从错误的地方开始回溯
                Exit While
            End If

            pos += 1
        End While

        ' 反转字符串
        Dim sb1 As New StringBuilder()
        Dim sb2 As New StringBuilder()

        For k As Integer = pos - 1 To 0 Step -1
            sb1.Append(align1(k))
            sb2.Append(align2(k))
        Next

        globalAlign(0) = sb1.ToString()
        globalAlign(1) = sb2.ToString()

        Return score(l1, l2)
    End Function
End Class

#Region "Microsoft.VisualBasic::b37c27bee3b0864129d05b834e7c0b50, Data_science\Mathematica\Math\mHG\HG_row_n\HG_row_ncalc.vb"

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

    '   Total Lines: 160
    '    Code Lines: 62 (38.75%)
    ' Comment Lines: 82 (51.25%)
    '    - Xml Docs: 57.32%
    ' 
    '   Blank Lines: 16 (10.00%)
    '     File Size: 7.25 KB


    ' Class HG_row_ncalc
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: iter, recur
    ' 
    '     Sub: HG_row_ncalcrecurinner
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports std = System.Math

''' <summary>
''' The helper class for the hypergeometric row calculation.
'''
''' NOTE about the indexing convention:
''' The 0-based ``Vector`` here stores ``HG_row(i) = Prob(X == i)``, while the R reference
''' implementation stores the same value in ``HG_row[i + 1]``. Hence every ``HG_row_m[X]``
''' access of the R code is translated into ``HG_row_m(X - 1)`` in this file.
''' </summary>
Public Class HG_row_ncalc

    Dim HG_row_m As Vector, m%, ni%, b_n#, N%, B%

    Private Sub New()
    End Sub

    ''' <summary>
    ''' The code works directly On HG_row_m - updating it recursively, filling the right
    ''' values from right To left. Filling this row In a directed manner allow us To update a cell
    ''' In a recursive manner according To the one left To it, without being concerned that the cell
    ''' To the left was already updated To a "too big" number Of tries.
    '''
    ''' The code work On the subtree induced by the limits:
    ''' Rows (m_start, n) And columns (b_n_start, b_n_end),
    ''' updating the subtree In a recursive manner until row_n.
    ''' 
    ''' The code assumes that:
    '''   ``HG_row_m(b_n_start)`` has the correct hypergeometric probability value For number Of
    '''   tries (row) ``m_start``.
    ''' </summary>
    ''' <param name="b_n_start%"></param>
    ''' <param name="b_n_end%"></param>
    ''' <param name="m_start%"></param>
    Private Sub HG_row_ncalcrecurinner(b_n_start%, b_n_end%, m_start%)
        ' # Stop condition
        ' R: if ((n - m_start) == 0)
        If ((ni - m_start) = 0) Then
            Return
        Else
            ' split the tree To two subtrees To be evaluated separately.
            ' R_tree Will be used To calculate the HG_row_n entries corresponding To (b_n_start:b_n_start + r_split),
            ' And L_tree will be used To calculate the rest.
            Dim r_split% = CInt(std.Floor((b_n_end - b_n_start + 1) / 2))
            Dim l_split% = (b_n_end - b_n_start + 1) - r_split

            'If m Is above the root Of the tree we are working On - Then some rows were already
            ' calculated And we can take this To our advantage.
            Dim rows_already_calc% = std.Max(m - m_start, 0)

            ' Go diagonally (increasing both b_n_start And m) until we Get To the root Of the right tree.
            ' R: HG_row_m[b_n_start + i + 1] <<- HG_row_m[b_n_start + i] * d_ratio(m_start + i, b_n_start + i, N, B)
            Dim i% = rows_already_calc
            Do While (i < l_split) ' Needs To occur sequentially
                i = i + 1
                HG_row_m(b_n_start + i) = HG_row_m(b_n_start + i - 1) * d_ratio(m_start + i, b_n_start + i, N, B)
            Loop
            ' Calculate the right tree.
            Call HG_row_ncalcrecurinner(b_n_start + l_split, b_n_end, m_start + l_split)

            ' Go upwards (increasing only m) until we Get To the root Of the left tree.      
            ' R: HG_row_m[b_n_start + 1] <<- HG_row_m[b_n_start + 1] * v_ratio(m_start + i, b_n_start, N, B)
            i = rows_already_calc
            Do While (i < (ni + 1 - l_split - m_start)) ' Needs To occur sequentially
                i = i + 1
                HG_row_m(b_n_start) = HG_row_m(b_n_start) * v_ratio(m_start + i, b_n_start, N, B)
            Loop
            ' Calculate the left tree.
            Call HG_row_ncalcrecurinner(b_n_start, b_n_start + l_split - 1, ni + 1 - l_split)
        End If
    End Sub

    ''' <summary>
    ''' Calculate HG row n recursively.
    ''' See Function documentation For "HG_row_n.calc", To gain insight On input And outputs. 
    '''
    ''' NOTE: This implementation Is my interpretation Of a very unclear statement In Eden's thesis that a row can be
    ''' calculated In O(B) without considering previous rows. I did this In O(B * log(B)).
    ''' </summary>
    ''' <param name="HG_row_m"></param>
    ''' <param name="m"></param>
    ''' <param name="ni"></param>
    ''' <param name="b_n"></param>
    ''' <param name="N"></param>
    ''' <param name="B"></param>
    ''' <returns></returns>
    Public Shared Function recur(HG_row_m As Vector, m%, ni%, b_n#, N%, B%) As Vector
        '# HG_row_n Is calculated from a tree, For which the root Is located In
        '# b_n rows beneath - we will mark this As m_start.
        '#If row m Is "beneath" this root, We "go up" And calculate it.
        '#If we are "above" this root, we will ignore this For now, And use the fact that
        '# we have some rows calculated above m_start, In the recursion.
        '# NOTE: We can technically initialize HG_row_m[2:] To 0, but knowing that we will
        '# only use the cell In index 1, we Do Not bother.
        Dim m_start% = ni - CInt(b_n)

        ' R: while (m < m_start) { m <- m + 1; HG_row_m[1] <- HG_row_m[1] * v_ratio(m, 0, N, B) }
        Do While (m < m_start)
            m = m + 1
            HG_row_m(0) = HG_row_m(0) * v_ratio(m, 0, N, B)
        Loop

        ' NOTE: the object must be created after the loop above, so that the captured ``m``
        ' is the final value of the loop counter (just like the R closure does).
        Dim HG_row As New HG_row_ncalc() With {
            .B = B,
            .b_n = b_n,
            .HG_row_m = HG_row_m,
            .m = m,
            .N = N,
            .ni = ni
        }

        ' NOTE: this code works On HG_row_m directly.
        Call HG_row.HG_row_ncalcrecurinner(0, CInt(b_n), m_start)

        Return HG_row_m
    End Function

    ''' <summary>
    ''' Calculate HG row n iteratively.
    ''' See function documentation for "HG_row_n.calc", to gain insight on input And outputs. 
    '''
    ''' NOTE: The code works directly on HG_row_m, m - increasing m until it becomes n.
    ''' </summary>
    ''' <param name="HG_row_m"></param>
    ''' <param name="m"></param>
    ''' <param name="ni"></param>
    ''' <param name="b_n"></param>
    ''' <param name="N"></param>
    ''' <param name="B"></param>
    ''' <returns></returns>
    Public Shared Function iter(HG_row_m As Vector, m%, ni%, b_n#, N%, B%) As Vector
        '# Go upwards (increasing only m) until we get to row n-1.
        '# R: b_to_update <- 0:(b_n - 1)
        Dim bn% = CInt(b_n)

        Do While (m < (ni - 1))
            m = m + 1
            ' R: HG_row_m[b_to_update + 1] <- HG_row_m[b_to_update + 1] * v_ratio(m, b_to_update, N, B)
            For bi As Integer = 0 To bn - 1
                HG_row_m(bi) = HG_row_m(bi) * v_ratio(m, bi, N, B)
            Next
        Loop

        m = m + 1
        ' Last row To go - first update b_n from the diagonal, 
        ' Then the rest vertically
        ' R: HG_row_m[b_n + 1] <- HG_row_m[b_n] * d_ratio(m, b_n, N, B)
        HG_row_m(bn) = HG_row_m(bn - 1) * d_ratio(m, bn, N, B)
        For bi As Integer = 0 To bn - 1
            HG_row_m(bi) = HG_row_m(bi) * v_ratio(m, bi, N, B)
        Next

        Return HG_row_m
    End Function
End Class

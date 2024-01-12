Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports std = System.Math

Public Class HG_row_ncalc

    Dim HG_row_m As Vector, m%, ni%, b_n#, N%, B%

    Private Sub New()
    End Sub

    Private Sub HG_row_ncalcrecurinner(b_n_start%, b_n_end%, m_start%)
        '# The code works directly On HG_row_m - updating it recursively, filling the right
        '# values from right To left. Filling this row In a directed manner allow us To update a cell
        '# In a recursive manner according To the one left To it, without being concerned that the cell
        '# To the left was already updated To a "too big" number Of tries.
        '#
        '# The code work On the subtree induced by the limits:
        '# Rows (m_start, n) And columns (b_n_start, b_n_end),
        '# updating the subtree In a recursive manner until row_n.
        '# 
        '# The code assumes that:
        '#   HG_row_m[b_n_start] has the correct hypergeometric probability value For number Of tries (row) 
        '#   m_start.

        ' # Stop condition
        If ((N - m_start) = 0) Then
            Return
        Else
            ' split the tree To two subtrees To be evaluated separately.
            ' R_tree Will be used To calculate the HG_row_n entries corresponding To (b_n_start:b_n_start + r_split),
            ' And L_tree will be used To calculate the rest.
            Dim r_split = std.Floor((b_n_end - b_n_start + 1) / 2)
            Dim l_split = (b_n_end - b_n_start + 1) - r_split

            'If m Is above the root Of the tree we are working On - Then some rows were already
            ' calculated And we can take this To our advantage.
            Dim rows_already_calc = std.Max(m - m_start, 0)

            ' Go diagonally (increasing both b_n_start And m) until we Get To the root Of the right tree.
            Dim i = rows_already_calc
            Do While (i < l_split) ' Needs To occur sequentially
                i = i + 1
                HG_row_m(b_n_start + i + 1) = HG_row_m(b_n_start + i) * d_ratio(m_start + i, b_n_start + i, N, B)
            Loop
            ' Calculate the right tree.
            HG_row_ncalcrecurinner(b_n_start + l_split, b_n_end, m_start + l_split)

            ' Go upwards (increasing only m) until we Get To the root Of the left tree.      
            i = rows_already_calc
            Do While (i < (N + 1 - l_split - m_start)) ' Needs To occur sequentially
                i = i + 1
                HG_row_m(b_n_start + 1) = HG_row_m(b_n_start + 1) * v_ratio(m_start + i, b_n_start, N, B)
            Loop
            ' Calculate the left tree.
            HG_row_ncalcrecurinner(b_n_start, b_n_start + l_split - 1, N + 1 - l_split)
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
        Dim HG_row As New HG_row_ncalc() With {
            .B = B,
            .b_n = b_n,
            .HG_row_m = HG_row_m,
            .m = m,
            .N = N,
            .ni = ni
        }

        '# HG_row_n Is calculated from a tree, For which the root Is located In
        '# b_n rows beneath - we will mark this As m_start.
        '#If rowThenThen m Is "beneath" this root, We "go up" And calculate it.
        '#If weThenThen are "above" this root, we will ignore this For now, And use the fact that
        '# we have some rows calculated above m_start, In the recursion.
        '# NOTE: We can technically initialize HG_row_m[2:] To 0, but knowing that we will
        '# only use the cell In index 1, we Do Not bother.
        Dim i_start = N - b_n

        Do While (m < i_start)
            m = m + 1
            HG_row_m(1) = HG_row_m(1) * v_ratio(m, 0, N, B)
        Loop

        ' NOTE: this code works On HG_row_m directly.
        Call HG_row.HG_row_ncalcrecurinner(0, b_n, i_start)

        Return HG_row_m
    End Function

    ''' <summary>
    ''' Calculate HG row n iteratively.
    ''' See function documentation for "HG_row_n.calc", to gain insight on input And outputs. 
    '''
    ''' NOTE: The code works directly on HG_row_m, m - increasing m until it becomes n.
    ''' </summary>
    ''' <param name="HG_row_m"></param>
    ''' <param name="m#"></param>
    ''' <param name="ni#"></param>
    ''' <param name="b_n#"></param>
    ''' <param name="N#"></param>
    ''' <param name="B#"></param>
    ''' <returns></returns>
    Public Shared Function iter(HG_row_m As Vector, m%, ni%, b_n#, N%, B%) As Vector
        '# Go upwards (increasing only m) until we get to row n-1.    
        Dim b_to_update As Vector = seq(0, b_n - 1)

        Do While (m < (N - 1))
            m = m + 1
            HG_row_m(b_to_update + 1) = HG_row_m(b_to_update + 1) * v_ratio(m, b_to_update, N, B)
        Loop

        m = m + 1
        ' Last row To go - first update b_n from the diagonal, 
        ' Then the rest vertically
        HG_row_m(b_n + 1) = HG_row_m(b_n) * d_ratio(m, b_n, N, B)
        HG_row_m(b_to_update + 1) = HG_row_m(b_to_update + 1) * v_ratio(m, b_to_update, N, B)

        Return HG_row_m
    End Function
End Class
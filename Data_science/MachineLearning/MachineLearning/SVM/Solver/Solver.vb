#Region "Microsoft.VisualBasic::2ff48882a61c09b7999152b3a23f07be, Data_science\MachineLearning\MachineLearning\SVM\Solver\Solver.vb"

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

    '     Class Solver
    ' 
    '         Function: be_shrunk, calculate_rho, get_C, is_free, is_lower_bound
    '                   is_upper_bound, select_working_set
    ' 
    '         Sub: do_shrinking, reconstruct_gradient, Solve, swap_index, update_alpha_status
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' 
' * SVM.NET Library
' * Copyright (C) 2008 Matthew Johnson
' * 
' * This program is free software: you can redistribute it and/or modify
' * it under the terms of the GNU General Public License as published by
' * the Free Software Foundation, either version 3 of the License, or
' * (at your option) any later version.
' * 
' * This program is distributed in the hope that it will be useful,
' * but WITHOUT ANY WARRANTY; without even the implied warranty of
' * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' * GNU General Public License for more details.
' * 
' * You should have received a copy of the GNU General Public License
' * along with this program.  If not, see <http://www.gnu.org/licenses/>.


Imports Microsoft.VisualBasic.Text
Imports stdNum = System.Math

Namespace SVM

    ''' <summary>
    ''' An SMO algorithm in Fan et al., JMLR 6(2005), p. 1889--1918
    ''' Solves:
    '''
    ''' ```
    '''	min 0.5(\alpha^T Q \alpha) + p^T \alpha
    '''
    '''		y^T \alpha = \delta
    '''		y_i = +1 or -1
    '''		0 &lt;= alpha_i &lt;= Cp for y_i = 1
    '''		0 &lt;= alpha_i &lt;= Cn for y_i = -1
    ''' ```
    ''' 
    ''' Given:
    '''
    '''	Q, p, y, Cp, Cn, and an initial feasible point \alpha
    '''	l is the size of vectors and matrices
    '''	eps is the stopping tolerance
    '''
    ''' solution will be put in \alpha, objective value will be put in obj
    '''
    ''' </summary>
    Friend Class Solver

        Protected active_size As Integer
        Protected y As SByte()

        ''' <summary>
        ''' gradient of objective function
        ''' </summary>
        Protected G As Double()
        Protected Const LOWER_BOUND As Byte = 0
        Protected Const UPPER_BOUND As Byte = 1
        Protected Const FREE As Byte = 2
        Protected alpha_status As Byte()    ' LOWER_BOUND, UPPER_BOUND, FREE
        Protected alpha As Double()
        Protected Q As IQMatrix
        Protected QD As Double()
        Protected eps As Double
        Protected Cp, Cn As Double
        Protected p As Double()
        Protected active_set As Integer()
        Protected G_bar As Double()     ' gradient, if we treat free variables as 0
        Protected l As Integer
        Protected unshrink As Boolean   ' XXX
        Protected Const INF As Double = Double.PositiveInfinity

        Private Function get_C(i As Integer) As Double
            Return If(y(i) > 0, Cp, Cn)
        End Function

        Private Sub update_alpha_status(i As Integer)
            If alpha(i) >= get_C(i) Then
                alpha_status(i) = UPPER_BOUND
            ElseIf alpha(i) <= 0 Then
                alpha_status(i) = LOWER_BOUND
            Else
                alpha_status(i) = FREE
            End If
        End Sub

        Protected Function is_upper_bound(i As Integer) As Boolean
            Return alpha_status(i) = UPPER_BOUND
        End Function

        Protected Function is_lower_bound(i As Integer) As Boolean
            Return alpha_status(i) = LOWER_BOUND
        End Function

        Protected Function is_free(i As Integer) As Boolean
            Return alpha_status(i) = FREE
        End Function

        Protected Sub swap_index(i As Integer, j As Integer)
            Q.SwapIndex(i, j)

            Do
                Dim __ = y(i)
                y(i) = y(j)
                y(j) = __
            Loop While False

            Do
                Dim __ = G(i)
                G(i) = G(j)
                G(j) = __
            Loop While False

            Do
                Dim __ = alpha_status(i)
                alpha_status(i) = alpha_status(j)
                alpha_status(j) = __
            Loop While False

            Do
                Dim __ = alpha(i)
                alpha(i) = alpha(j)
                alpha(j) = __
            Loop While False

            Do
                Dim __ = p(i)
                p(i) = p(j)
                p(j) = __
            Loop While False

            Do
                Dim __ = active_set(i)
                active_set(i) = active_set(j)
                active_set(j) = __
            Loop While False

            Do
                Dim __ = G_bar(i)
                G_bar(i) = G_bar(j)
                G_bar(j) = __
            Loop While False
        End Sub

        ''' <summary>
        ''' reconstruct inactive elements of G from G_bar 
        ''' and free variables
        ''' </summary>
        Protected Sub reconstruct_gradient()
            If active_size = l Then
                Return
            ElseIf active_size <= -1 Then
                Logging.info($"unsure for active_size index is negatuve value?")
                Return
            End If

            Dim i, j As Integer
            Dim nr_free = 0

            For j = active_size To l - 1
                G(j) = G_bar(j) + p(j)
            Next

            For j = 0 To active_size - 1
                If is_free(j) Then nr_free += 1
            Next

            If 2 * nr_free < active_size Then
                Logging.info(ASCII.LF & "WARNING: using -h 0 may be faster" & ASCII.LF)
            End If

            If nr_free * l > 2 * active_size * (l - active_size) Then
                For i = active_size To l - 1
                    Dim Q_i = Q.GetQ(i, active_size)

                    For j = 0 To active_size - 1
                        If is_free(j) Then
                            G(i) += alpha(j) * Q_i(j)
                        End If
                    Next
                Next
            Else

                For i = 0 To active_size - 1

                    If is_free(i) Then
                        Dim Q_i = Q.GetQ(i, l)
                        Dim alpha_i = alpha(i)

                        For j = active_size To l - 1
                            G(j) += alpha_i * Q_i(j)
                        Next
                    End If
                Next
            End If
        End Sub

        Public Overridable Sub Solve(l As Integer, Q As IQMatrix, p_ As Double(), y_ As SByte(), alpha_ As Double(), Cp As Double, Cn As Double, eps As Double, si As SolutionInfo, shrinking As Boolean)
            Me.l = l
            Me.Q = Q
            QD = Q.GetQD()
            p = CType(p_.Clone(), Double())
            y = CType(y_.Clone(), SByte())
            alpha = CType(alpha_.Clone(), Double())
            Me.Cp = Cp
            Me.Cn = Cn
            Me.eps = eps

            unshrink = False

            ' initialize alpha_status
            If True Then
                alpha_status = New Byte(l - 1) {}

                For i = 0 To l - 1
                    update_alpha_status(i)
                Next
            End If

            ' initialize active set (for shrinking)
            If True Then
                active_set = New Integer(l - 1) {}

                For i = 0 To l - 1
                    active_set(i) = i
                Next

                active_size = l
            End If

            ' initialize gradient
            If True Then
                G = New Double(l - 1) {}
                G_bar = New Double(l - 1) {}
                Dim i As Integer

                For i = 0 To l - 1
                    G(i) = p(i)
                    G_bar(i) = 0
                Next

                For i = 0 To l - 1

                    If Not is_lower_bound(i) Then
                        Dim Q_i = Q.GetQ(i, l)
                        Dim alpha_i = alpha(i)
                        Dim j As Integer

                        For j = 0 To l - 1
                            G(j) += alpha_i * Q_i(j)
                        Next

                        If is_upper_bound(i) Then
                            For j = 0 To l - 1
                                G_bar(j) += get_C(i) * Q_i(j)
                            Next
                        End If
                    End If
                Next
            End If

            ' optimization step

            Dim iter = 0
            Dim max_iter = stdNum.Max(10000000, If(l > Integer.MaxValue / 100, Integer.MaxValue, 100 * l))
            Dim counter = stdNum.Min(l, 1000) + 1
            Dim working_set = New Integer(1) {}

            While iter < max_iter
                ' show progress and do shrinking

                If Threading.Interlocked.Decrement(counter) = 0 Then
                    counter = stdNum.Min(l, 1000)

                    If shrinking Then
                        do_shrinking()
                    End If
                End If

                If select_working_set(working_set) <> 0 Then
                    ' reconstruct the whole gradient
                    reconstruct_gradient()
                    ' reset active set size and check
                    active_size = l

                    If select_working_set(working_set) <> 0 Then
                        Exit While
                    Else
                        counter = 1
                    End If  ' do shrinking next iteration
                End If

                Dim i = working_set(0)
                Dim j = working_set(1)

                iter += 1

                ' update alpha[i] and alpha[j], handle bounds carefully

                Dim Q_i = Q.GetQ(i, active_size)
                Dim Q_j = Q.GetQ(j, active_size)
                Dim C_i = get_C(i)
                Dim C_j = get_C(j)
                Dim old_alpha_i = alpha(i)
                Dim old_alpha_j = alpha(j)

                If y(i) <> y(j) Then
                    Dim quad_coef = QD(i) + QD(j) + 2 * Q_i(j)
                    If quad_coef <= 0 Then quad_coef = 0.000000000001
                    Dim delta = (-G(i) - G(j)) / quad_coef
                    Dim diff = alpha(i) - alpha(j)
                    alpha(i) += delta
                    alpha(j) += delta

                    If diff > 0 Then
                        If alpha(j) < 0 Then
                            alpha(j) = 0
                            alpha(i) = diff
                        End If
                    Else

                        If alpha(i) < 0 Then
                            alpha(i) = 0
                            alpha(j) = -diff
                        End If
                    End If

                    If diff > C_i - C_j Then
                        If alpha(i) > C_i Then
                            alpha(i) = C_i
                            alpha(j) = C_i - diff
                        End If
                    Else

                        If alpha(j) > C_j Then
                            alpha(j) = C_j
                            alpha(i) = C_j + diff
                        End If
                    End If
                Else
                    Dim quad_coef = QD(i) + QD(j) - 2 * Q_i(j)
                    If quad_coef <= 0 Then quad_coef = 0.000000000001
                    Dim delta = (G(i) - G(j)) / quad_coef
                    Dim sum = alpha(i) + alpha(j)
                    alpha(i) -= delta
                    alpha(j) += delta

                    If sum > C_i Then
                        If alpha(i) > C_i Then
                            alpha(i) = C_i
                            alpha(j) = sum - C_i
                        End If
                    Else

                        If alpha(j) < 0 Then
                            alpha(j) = 0
                            alpha(i) = sum
                        End If
                    End If

                    If sum > C_j Then
                        If alpha(j) > C_j Then
                            alpha(j) = C_j
                            alpha(i) = sum - C_j
                        End If
                    Else

                        If alpha(i) < 0 Then
                            alpha(i) = 0
                            alpha(j) = sum
                        End If
                    End If
                End If

                ' update G

                Dim delta_alpha_i = alpha(i) - old_alpha_i
                Dim delta_alpha_j = alpha(j) - old_alpha_j

                For k = 0 To active_size - 1
                    G(k) += Q_i(k) * delta_alpha_i + Q_j(k) * delta_alpha_j
                Next

                ' update alpha_status and G_bar

                If True Then
                    Dim ui = is_upper_bound(i)
                    Dim uj = is_upper_bound(j)
                    update_alpha_status(i)
                    update_alpha_status(j)
                    Dim k As Integer

                    If ui <> is_upper_bound(i) Then
                        Q_i = Q.GetQ(i, l)

                        If ui Then
                            For k = 0 To l - 1
                                G_bar(k) -= C_i * Q_i(k)
                            Next
                        Else

                            For k = 0 To l - 1
                                G_bar(k) += C_i * Q_i(k)
                            Next
                        End If
                    End If

                    If uj <> is_upper_bound(j) Then
                        Q_j = Q.GetQ(j, l)

                        If uj Then
                            For k = 0 To l - 1
                                G_bar(k) -= C_j * Q_j(k)
                            Next
                        Else

                            For k = 0 To l - 1
                                G_bar(k) += C_j * Q_j(k)
                            Next
                        End If
                    End If
                End If
            End While

            If iter >= max_iter Then
                If active_size < l Then
                    ' reconstruct the whole gradient to calculate objective value
                    reconstruct_gradient()

                    active_size = l
                End If

                Console.Error.Write(ASCII.LF & "WARNING: reaching max number of iterations" & ASCII.LF)
            End If

            ' calculate rho

            si.rho = calculate_rho()

            ' calculate objective value
            If True Then
                Dim v As Double = 0
                Dim i As Integer

                For i = 0 To l - 1
                    v += alpha(i) * (G(i) + p(i))
                Next

                si.obj = v / 2
            End If

            ' put back the solution
            If True Then
                For i = 0 To l - 1
                    alpha_(active_set(i)) = alpha(i)
                Next
            End If

            si.upper_bound_p = Cp
            si.upper_bound_n = Cn

            Logging.info(ASCII.LF & "optimization finished, #iter = " & iter & ASCII.LF)
        End Sub

        ''' <summary>
        ''' return 1 if already optimal, return 0 otherwise
        ''' </summary>
        ''' <param name="working_set"></param>
        ''' <returns></returns>
        Protected Overridable Function select_working_set(working_set As Integer()) As Integer
            ' return i,j such that
            ' i: maximizes -y_i * grad(f)_i, i in I_up(\alpha)
            ' j: mimimizes the decrease of obj value
            '    (if quadratic coefficeint <= 0, replace it with tau)
            '    -y_j*grad(f)_j < -y_i*grad(f)_i, j in I_low(\alpha)

            Dim Gmax = -INF
            Dim Gmax2 = -INF
            Dim Gmax_idx = -1
            Dim Gmin_idx = -1
            Dim obj_diff_min = INF

            For t = 0 To active_size - 1

                If y(t) = +1 Then
                    If Not is_upper_bound(t) Then
                        If -G(t) >= Gmax Then
                            Gmax = -G(t)
                            Gmax_idx = t
                        End If
                    End If
                Else

                    If Not is_lower_bound(t) Then
                        If G(t) >= Gmax Then
                            Gmax = G(t)
                            Gmax_idx = t
                        End If
                    End If
                End If
            Next

            Dim i = Gmax_idx
            Dim Q_i As Single() = Nothing
            If i <> -1 Then Q_i = Q.GetQ(i, active_size) ' null Q_i not accessed: Gmax=-INF if i=-1

            For j = 0 To active_size - 1

                If y(j) = +1 Then
                    If Not is_lower_bound(j) Then
                        Dim grad_diff = Gmax + G(j)
                        If G(j) >= Gmax2 Then Gmax2 = G(j)

                        If grad_diff > 0 Then
                            Dim obj_diff As Double
                            Dim quad_coef = QD(i) + QD(j) - 2.0 * y(i) * Q_i(j)

                            If quad_coef > 0 Then
                                obj_diff = -(grad_diff * grad_diff) / quad_coef
                            Else
                                obj_diff = -(grad_diff * grad_diff) / 0.000000000001
                            End If

                            If obj_diff <= obj_diff_min Then
                                Gmin_idx = j
                                obj_diff_min = obj_diff
                            End If
                        End If
                    End If
                Else

                    If Not is_upper_bound(j) Then
                        Dim grad_diff = Gmax - G(j)
                        If -G(j) >= Gmax2 Then Gmax2 = -G(j)

                        If grad_diff > 0 Then
                            Dim obj_diff As Double
                            Dim quad_coef = QD(i) + QD(j) + 2.0 * y(i) * Q_i(j)

                            If quad_coef > 0 Then
                                obj_diff = -(grad_diff * grad_diff) / quad_coef
                            Else
                                obj_diff = -(grad_diff * grad_diff) / 0.000000000001
                            End If

                            If obj_diff <= obj_diff_min Then
                                Gmin_idx = j
                                obj_diff_min = obj_diff
                            End If
                        End If
                    End If
                End If
            Next

            If Gmax + Gmax2 < eps Then Return 1
            working_set(0) = Gmax_idx
            working_set(1) = Gmin_idx
            Return 0
        End Function

        Private Function be_shrunk(i As Integer, Gmax1 As Double, Gmax2 As Double) As Boolean
            If is_upper_bound(i) Then
                If y(i) = +1 Then
                    Return -G(i) > Gmax1
                Else
                    Return -G(i) > Gmax2
                End If
            ElseIf is_lower_bound(i) Then

                If y(i) = +1 Then
                    Return G(i) > Gmax2
                Else
                    Return G(i) > Gmax1
                End If
            Else
                Return False
            End If
        End Function

        Protected Overridable Sub do_shrinking()
            Dim i As Integer
            Dim Gmax1 = -INF        ' max { -y_i * grad(f)_i | i in I_up(\alpha) }
            Dim Gmax2 = -INF        ' max { y_i * grad(f)_i | i in I_low(\alpha) }

            ' find maximal violating pair first
            For i = 0 To active_size - 1

                If y(i) = +1 Then
                    If Not is_upper_bound(i) Then
                        If -G(i) >= Gmax1 Then Gmax1 = -G(i)
                    End If

                    If Not is_lower_bound(i) Then
                        If G(i) >= Gmax2 Then Gmax2 = G(i)
                    End If
                Else

                    If Not is_upper_bound(i) Then
                        If -G(i) >= Gmax2 Then Gmax2 = -G(i)
                    End If

                    If Not is_lower_bound(i) Then
                        If G(i) >= Gmax1 Then Gmax1 = G(i)
                    End If
                End If
            Next

            If unshrink = False AndAlso Gmax1 + Gmax2 <= eps * 10 Then
                unshrink = True
                reconstruct_gradient()
                active_size = l
            End If

            For i = 0 To active_size - 1

                If be_shrunk(i, Gmax1, Gmax2) Then
                    active_size -= 1

                    While active_size > i

                        If Not be_shrunk(active_size, Gmax1, Gmax2) Then
                            swap_index(i, active_size)
                            Exit While
                        End If

                        active_size -= 1
                    End While
                End If
            Next
        End Sub

        Protected Overridable Function calculate_rho() As Double
            Dim r As Double
            Dim nr_free = 0
            Dim ub = INF, lb = -INF, sum_free As Double = 0

            For i = 0 To active_size - 1
                Dim yG = y(i) * G(i)

                If is_lower_bound(i) Then
                    If y(i) > 0 Then
                        ub = stdNum.Min(ub, yG)
                    Else
                        lb = stdNum.Max(lb, yG)
                    End If
                ElseIf is_upper_bound(i) Then

                    If y(i) < 0 Then
                        ub = stdNum.Min(ub, yG)
                    Else
                        lb = stdNum.Max(lb, yG)
                    End If
                Else
                    nr_free += 1
                    sum_free += yG
                End If
            Next

            If nr_free > 0 Then
                r = sum_free / nr_free
            Else
                r = (ub + lb) / 2
            End If

            Return r
        End Function
    End Class

End Namespace

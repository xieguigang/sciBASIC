#Region "Microsoft.VisualBasic::c368d045b42e6ba591b59075ba446c2b, Data_science\Mathematica\Math\Math\Numerics\Optimization\LBFGSB\BFGSMat.vb"

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

    '   Total Lines: 555
    '    Code Lines: 447 (80.54%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 108 (19.46%)
    '     File Size: 17.98 KB


    '     Class BFGSMat
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) apply_PtBQv, (+2 Overloads) apply_PtWMv, (+2 Overloads) apply_WtPv, (+2 Overloads) Wb
    ' 
    '         Sub: add_correction, apply_Mv, apply_Wtv, compute_FtBAb, reset
    '              solve_PtBP
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Framework.Optimization.LBFGSB


    Public NotInheritable Class BFGSMat

        Public m_m As Integer
        Public m_theta As Double
        Public m_s As Matrix
        Public m_y As Matrix
        Public m_ys As Double()
        Public m_alpha As Double()
        Public m_ncorr As Integer
        Public m_ptr As Integer

        Public permMinv As Matrix
        Public permMsolver As BKLDLT

        Public Sub reset(n As Integer, m As Integer)
            m_m = m
            m_theta = 1.0
            m_s = Matrix.resize(m_s, n, m)
            m_y = Matrix.resize(m_y, n, m)
            m_ys = Vector.resize(m_ys, m)
            m_alpha = Vector.resize(m_alpha, m)
            m_ncorr = 0
            m_ptr = m

            permMinv = Matrix.resize(permMinv, 2 * m, 2 * m)
            permMinv.All = 0.0
            permMinv.Diag = 1.0 ' sets diagonal to 1.0
            permMsolver = New BKLDLT()
        End Sub

        Public Sub New()
        End Sub

        Public Sub New(n As Integer, m As Integer)
            reset(n, m)
        End Sub

        Public Sub add_correction(s As Double(), y As Double())
            If Debug.flag Then
                Debug.debug("-"c, "add correction")
                Debug.debug("s: ", s)
                Debug.debug("y: ", y)
            End If

            Dim loc = m_ptr Mod m_m

            m_s.setCol(loc, s)
            m_y.setCol(loc, y)

            Dim ys = Vector.dot(s, y)
            m_ys(loc) = ys

            m_theta = m_y.colSquaredNorm(loc) / ys

            If m_ncorr < m_m Then
                m_ncorr += 1
            End If

            m_ptr = loc + 1

            permMinv.set(loc, loc, -ys)

            For i = 0 To m_ncorr - 1
                Dim Ss = m_s.colDot(i, s)
                permMinv.set(m_m + loc, m_m + i, Ss)
                permMinv.set(m_m + i, m_m + loc, Ss)
            Next

            Dim len = m_ncorr - 1
            If m_ncorr >= m_m Then
                For i = 0 To m_m - 1
                    permMinv.set(m_m + i, loc, 0.0)
                Next
            End If

            Dim yloc = (loc + m_m - 1) Mod m_m
            Dim mloc = m_m + loc
            For i = 0 To len - 1
                permMinv.set(mloc, yloc, m_y.colDot(yloc, s))
                yloc = (yloc + m_m - 1) Mod m_m
            Next

            For i = 0 To m_m - 1
                For j = 0 To m_m - 1
                    permMinv.set(m_m + i, m_m + j, permMinv.get(m_m + i, m_m + j) * m_theta)
                Next
            Next

            permMsolver.compute(permMinv)

            For i = 0 To m_m - 1
                For j = 0 To m_m - 1
                    permMinv.set(m_m + i, m_m + j, permMinv.get(m_m + i, m_m + j) / m_theta)
                Next
            Next

            If Debug.flag Then
                Debug.debug("-"c, "add correction - end")
            End If
        End Sub

        Public Sub apply_Wtv(v As Double(), res As Double())
            If Debug.flag Then
                Debug.debug("-"c, "apply_Wtv")
                Debug.debug("v:  ", v)
            End If

            For i = 0 To m_ncorr - 1
                res(i) = m_y.colDot(i, v)
            Next

            For i = 0 To m_ncorr - 1
                res(i + m_ncorr) = m_theta * m_s.colDot(i, v)
            Next

            If Debug.flag Then
                Debug.debug("res: ", res)
                Debug.debug("-"c, "apply_Wtv - end")
            End If
        End Sub

        Public Sub apply_Mv(v As Double(), res As Double())
            If Debug.flag Then
                Debug.debug("-"c, "apply Mv")
                Debug.debug("v:  ", v)
            End If

            If m_ncorr < 1 Then
                If Debug.flag Then
                    Debug.debug("-"c, "leaving apply_Mv, m_ncorr < 1")
                End If
                Return
            End If

            Dim vpadding = New Double(2 * m_m - 1) {}
            For i = 0 To m_ncorr - 1
                vpadding(i) = v(i)
                vpadding(m_m + i) = v(m_ncorr + i)
            Next

            permMsolver.solve_inplace(vpadding)

            For i = 0 To m_ncorr - 1
                res(i) = vpadding(i)
                res(i + m_ncorr) = vpadding(m_m + i)
            Next

            If Debug.flag Then
                Debug.debug("res: ", res)
                Debug.debug("-"c, "apply Mv - end")
            End If
        End Sub

        Public Function Wb(b As Integer) As Double()
            If Debug.flag Then
                Debug.debug("-"c, "Wb")
                Call Debug.debug("b: " & b.ToString())
            End If

            Dim res = New Double(2 * m_ncorr - 1) {}

            For j = 0 To m_ncorr - 1
                res(j) = m_y.get(b, j)
                res(m_ncorr + j) = m_theta * m_s.get(b, j)
            Next

            If Debug.flag Then
                Debug.debug("res: ", res)
                Debug.debug("-"c, "Wb - end")
            End If

            Return res
        End Function

        Public Function Wb(b As List(Of Integer)) As Matrix
            If Debug.flag Then
                Debug.debug("-"c, "Wb")
                Call Debug.debug("b: " & b.ToString())
            End If

            Dim nb = b.Count
            Dim res As Matrix = New Matrix(nb, 2 * m_ncorr)

            For j = 0 To m_ncorr - 1 ' col
                For i = 0 To nb - 1 ' row
                    Dim row = b(i)
                    res.set(i, j, m_y.get(row, j))
                    res.set(i, j + m_ncorr, m_s.get(row, j))
                Next
            Next

            If Debug.flag Then
                Debug.debug("res: ", res)
                Debug.debug("-"c, "Wb - end")
            End If

            Return res
        End Function

        Public Function apply_WtPv(P_set As List(Of Integer), v As Double(), res As Double()) As Boolean
            Return apply_WtPv(P_set, v, res, False)
        End Function

        Public Function apply_WtPv(P_set As List(Of Integer), v As Double(), res As Double(), test_zero As Boolean) As Boolean
            If Debug.flag Then
                Call Debug.debug("-"c, "apply_WtPv, test_zero=" & test_zero.ToString())
                Call Debug.debug("P_set: " & P_set.ToString())
                Debug.debug("v: ", v)
            End If

            Dim Pptr = P_set
            Dim vptr = v
            Dim nP = P_set.Count

            If test_zero Then
                Dim P_reduced As List(Of Integer) = New List(Of Integer)(nP)
                Dim v_reduced As List(Of Double) = New List(Of Double)(nP)

                For i = 0 To nP - 1
                    If vptr(i) <> 0.0 Then
                        P_reduced.Add(Pptr(i))
                        v_reduced.Add(v(i))
                    End If
                Next

                nP = P_reduced.Count
                Pptr = P_reduced
                vptr = New Double(nP - 1) {}
                For i = 0 To nP - 1
                    vptr(i) = v_reduced(i)
                Next
            End If

            If m_ncorr < 1 OrElse nP < 1 Then
                If Debug.flag Then
                    Debug.debug("-"c, "leaving apply_WtPv")
                End If
                Vector.setAll(res, 0.0)
                Return False
            End If

            For j = 0 To m_ncorr - 1
                Dim resy = 0.0
                Dim ress = 0.0
                For i = 0 To nP - 1
                    Dim row = Pptr(i)
                    resy += m_y.get(row, j) * vptr(i)
                    ress += m_s.get(row, j) * vptr(i)
                Next
                res(j) = resy
                res(m_ncorr + j) = m_theta * ress
            Next

            If Debug.flag Then
                Debug.debug("res: ", res)
                Debug.debug("-"c, "apply_WtPv - end")
            End If

            Return True
        End Function

        Public Function apply_PtWMv(P_set As List(Of Integer), v As Double(), res As Double(), scale As Double) As Boolean
            If Debug.flag Then
                Call Debug.debug("-"c, "apply_PtWMv, scale=" & scale.ToString())
                Call Debug.debug("P_set: " & P_set.ToString())
                Debug.debug("v: ", v)
            End If

            Dim nP = P_set.Count
            Vector.setAll(res, 0.0)
            If m_ncorr < 1 OrElse nP < 1 Then
                If Debug.flag Then
                    Debug.debug("-"c, "leaving apply_PTWMv, m_ncorr < 1 || np < 1")
                End If
                Return False
            End If

            Dim Mv = New Double(2 * m_ncorr - 1) {}
            apply_Mv(v, Mv)
            For i = 0 To m_ncorr - 1
                Mv(i + m_ncorr) *= m_theta
            Next

            For j = 0 To m_ncorr - 1
                Dim Mvy = Mv(j)
                Dim Mvs = Mv(m_ncorr + j)
                For i = 0 To nP - 1
                    Dim row = P_set(i)
                    res(i) += Mvy * m_y.get(row, j) + Mvs * m_s.get(row, j)
                Next
            Next

            For i = 0 To res.Length - 1
                res(i) *= scale
            Next

            If Debug.flag Then
                Debug.debug("res: ", res)
                Debug.debug("-"c, "apply_PtWMv - end")
            End If

            Return True
        End Function

        Public Function apply_PtWMv(WP As Matrix, v As Double(), res As Double(), scale As Double) As Boolean
            If Debug.flag Then
                Call Debug.debug("-"c, "apply_PtWMv, scale=" & scale.ToString())
                Debug.debug("WP: ", WP)
                Debug.debug("v:", v)
            End If

            Dim nP = WP.rows

            If m_ncorr < 1 OrElse nP < 1 Then
                If Debug.flag Then
                    Debug.debug("-"c, "leaving apply_PtWMv, m_ncorr < 1 || nP < 1")
                End If
                Vector.setAll(res, 0.0)
                Return False
            End If

            Dim Mv = New Double(2 * m_ncorr - 1) {}
            apply_Mv(v, Mv)

            For i = 0 To m_ncorr - 1
                Mv(i + m_ncorr) *= m_theta
            Next

            For i = 0 To res.Length - 1
                Dim dot = 0.0
                For k = 0 To Mv.Length - 1
                    dot += WP.get(i, k) * Mv(k)
                Next
                res(i) = scale * dot
            Next

            If Debug.flag Then
                Debug.debug("res: ", res)
                Debug.debug("-"c, "apply_PtWMv - end")
            End If

            Return True
        End Function

        Public Sub compute_FtBAb(WF As Matrix, fv_set As List(Of Integer), newact_set As List(Of Integer), Wd As Double(), drt As Double(), res As Double())

            If Debug.flag Then
                Debug.debug("-"c, "compute_FtBAb")
                Debug.debug("WF: ", WF)
                Call Debug.debug("fv_set: " & fv_set.ToString())
                Call Debug.debug("newact_set: " & newact_set.ToString())
                Debug.debug("Wd: ", Wd)
                Debug.debug("drt: ", drt)
            End If

            Dim nact = newact_set.Count
            Dim nfree = WF.rows

            If m_ncorr < 1 OrElse nact < 1 OrElse nfree < 1 Then
                If Debug.flag Then
                    Debug.debug("-"c, "leaving compute_FtBAb, m_ncorr < 1 || nact < 1 || nfree < 1")
                End If
                Vector.setAll(res, 0.0)
                Return
            End If

            Dim rhs = New Double(2 * m_ncorr - 1) {}
            If nact <= nfree Then
                Dim Ad = New Double(nfree - 1) {}
                For i = 0 To nact - 1
                    Ad(i) = drt(newact_set(i))
                Next
                apply_WtPv(newact_set, Ad, rhs)
            Else
                Dim Fd = New Double(nfree - 1) {}
                For i = 0 To nfree - 1
                    Fd(i) = drt(fv_set(i))
                Next

                For i = 0 To rhs.Length - 1
                    Dim dot = 0.0
                    For k = 0 To nfree - 1
                        dot += WF.get(k, i) * Fd(k)
                    Next
                    rhs(i) = dot
                Next

                For i = 0 To m_ncorr - 1
                    rhs(i + m_ncorr) *= m_theta
                Next

                Vector.sub(Wd, rhs, rhs)
            End If

            apply_PtWMv(WF, rhs, res, -1.0)

            If Debug.flag Then
                Debug.debug("res: ", res)
                Debug.debug("-"c, "compute_FtBAb - end")
            End If
        End Sub

        Public Function apply_PtBQv(WP As Matrix, Q_set As List(Of Integer), v As Double(), res As Double()) As Boolean
            Return apply_PtBQv(WP, Q_set, v, res, False)
        End Function

        Public Function apply_PtBQv(WP As Matrix, Q_set As List(Of Integer), v As Double(), res As Double(), test_zero As Boolean) As Boolean

            If Debug.flag Then
                Call Debug.debug("-"c, "PtBQv, test_zero=" & test_zero.ToString())
                Debug.debug("WP: ", WP)
                Call Debug.debug("Q_set: " & Q_set.ToString())
                Debug.debug("v: ", v)
            End If

            Dim nP = WP.rows
            Dim nQ = Q_set.Count

            If m_ncorr < 1 OrElse nP < 1 OrElse nQ < 1 Then
                If Debug.flag Then
                    Debug.debug("-"c, "leaving PtBQv, m_ncorr < 1 || nP < 1 || nQ < 1")
                End If

                Vector.setAll(res, 0.0)
                Return False
            End If

            Dim WQtv = New Double(2 * m_ncorr - 1) {}
            Dim nonzero = apply_WtPv(Q_set, v, WQtv, test_zero)
            If Not nonzero Then
                If Debug.flag Then
                    Debug.debug("-"c, "leaving PtBQv, !nonzero")
                End If
                Vector.setAll(res, 0.0)
                Return False
            End If

            Dim MWQtv = New Double(2 * m_ncorr - 1) {}
            apply_Mv(WQtv, MWQtv)

            For i = 0 To m_ncorr - 1
                MWQtv(i + m_ncorr) *= m_theta
            Next

            For row = 0 To WP.rows - 1
                Dim dot = 0.0
                For col = 0 To WP.cols - 1
                    dot += WP.get(row, col) * MWQtv(col)
                Next
                res(row) = -dot
            Next

            If Debug.flag Then
                Debug.debug("res: ", res)
                Debug.debug("-"c, "PtBQv - end")
            End If

            Return True
        End Function

        Public Sub solve_PtBP(WP As Matrix, v As Double(), res As Double())
            If Debug.flag Then
                Debug.debug("-"c, "solve_PtBP")
                Debug.debug("WP: ", WP)
                Debug.debug("v: ", v)
            End If

            Dim nP = WP.rows
            If m_ncorr < 1 OrElse nP < 1 Then
                For i = 0 To res.Length - 1
                    res(i) = v(i) / m_theta
                Next

                If Debug.flag Then
                    Debug.debug("res: ", res)
                    Debug.debug("-"c, "leaving PtBQv, m_ncorr < 1 || nP < 1")
                End If

                Return
            End If

            Dim mid As Matrix = New Matrix(2 * m_ncorr, 2 * m_ncorr)
            For j = 0 To m_ncorr - 1
                For i = 0 To m_ncorr - j - 1
                    Dim dot = 0.0
                    For k = 0 To nP - 1
                        dot += WP.get(k, i + j) * WP.get(k, j)
                    Next

                    mid.set(i + j, j, permMinv.get(i + j, j) - dot / m_theta)
                Next
            Next

            For i = 0 To m_ncorr - 1
                For j = 0 To m_ncorr - 1
                    Dim dot = 0.0
                    For k = 0 To nP - 1
                        dot += WP.get(k, i + m_ncorr) * WP.get(k, j)
                    Next
                    mid.set(i + m_ncorr, j, permMinv.get(i + m_m, j) - dot)
                Next
            Next

            For j = 0 To m_ncorr - 1
                For i = 0 To m_ncorr - j - 1
                    Dim dot = 0.0
                    Dim right = WP.cols - (m_ncorr - j) + i

                    For k = 0 To WP.rows - 1
                        dot += WP.get(k, right) * WP.get(k, m_ncorr + j)
                    Next
                    mid.set(i + m_ncorr + j, m_ncorr + j, m_theta * (permMinv.get(i + m_m + j, m_m + j) - dot))
                Next
            Next

            Dim midsolver As BKLDLT = New BKLDLT(mid)

            Dim WPv = New Double(WP.cols - 1) {}
            For i = 0 To WP.cols - 1
                For j = 0 To WP.rows - 1
                    WPv(i) += WP.get(j, i) * v(j)
                Next
            Next

            For i = WPv.Length - 1 To m_ncorr Step -1
                WPv(i) *= m_theta
            Next

            midsolver.solve_inplace(WPv)

            For i = WPv.Length - 1 To m_ncorr Step -1
                WPv(i) *= m_theta
            Next

            Dim t2 = m_theta * m_theta

            For i = 0 To res.Length - 1
                Dim dot = 0.0
                For k = 0 To WPv.Length - 1
                    dot += WP.get(i, k) * WPv(k)
                Next
                res(i) = v(i) / m_theta + dot / t2
            Next

            If Debug.flag Then
                Debug.debug("res: ", res)
                Debug.debug("-"c, "PtBP - end")
            End If
        End Sub
    End Class

End Namespace

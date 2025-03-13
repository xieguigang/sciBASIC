Imports std = System.Math

Namespace Framework.Optimization.LBFGSB

    Public NotInheritable Class SubspaceMin

        Public Shared ReadOnly meps As Double = -Microsoft.VisualBasic.Math.Ulp(1.0)

        Public Shared Sub subvec_assign(v As Double(), ind As List(Of Integer), rhs As Double())
            Dim nsub = ind.Count
            For i = 0 To nsub - 1
                v(ind(i)) = rhs(i)
            Next
        End Sub

        Public Shared Function subvec(v As Double(), ind As List(Of Integer)) As Double()
            Dim nsub = ind.Count
            Dim res = New Double(nsub - 1) {}
            For i = 0 To nsub - 1
                res(i) = v(ind(i))
            Next
            Return res
        End Function

        Public Shared Function in_bounds(x As Double(), lb As Double(), ub As Double()) As Boolean
            Dim n = x.Length
            For i = 0 To n - 1
                If x(i) < lb(i) OrElse x(i) > ub(i) Then
                    Return False
                End If
            Next
            Return True
        End Function

        Public Shared Function P_converged(yP_set As List(Of Integer), vecy As Double(), vecl As Double(), vecu As Double()) As Boolean
            Dim nP = yP_set.Count
            For i = 0 To nP - 1
                Dim coord = yP_set(i)
                If vecy(coord) < vecl(coord) OrElse vecy(coord) > vecu(coord) Then
                    Return False
                End If
            Next
            Return True
        End Function

        Public Shared Function L_converged(yL_set As List(Of Integer), lambda As Double()) As Boolean
            Dim nL = yL_set.Count
            For i = 0 To nL - 1
                Dim coord = yL_set(i)
                If lambda(coord) < 0.0 Then
                    Return False
                End If
            Next
            Return True
        End Function

        Public Shared Function U_converged(yU_set As List(Of Integer), mu As Double()) As Boolean
            Dim nU = yU_set.Count
            For i = 0 To nU - 1
                Dim coord = yU_set(i)
                If mu(coord) < 0.0 Then
                    Return False
                End If
            Next
            Return True
        End Function

        Public Shared Sub subspace_minimize(bfgs As BFGSMat, x0 As Double(), g As Double(), lb As Double(), ub As Double(), cauchy As Cauchy, maxit As Integer, drt As Double())
            If Debug.flag Then
                Debug.debug("-"c, "subspace minimize")
            End If

            Dim Wd = cauchy.vecc

            Vector.sub(cauchy.xcp, x0, drt)

            Dim nfree = cauchy.fv_set.Count

            If nfree < 1 Then
                If Debug.flag Then
                    Debug.debug("-"c, "leaving subspace_minimize, nfree<1")
                End If
                Return
            End If

            Dim WF = bfgs.Wb(cauchy.fv_set)

            Dim vecc = New Double(nfree - 1) {}
            bfgs.compute_FtBAb(WF, cauchy.fv_set, cauchy.newact_set, Wd, drt, vecc)
            Dim vecl = New Double(nfree - 1) {}
            Dim vecu = New Double(nfree - 1) {}

            For i = 0 To nfree - 1
                Dim coord = cauchy.fv_set(i)
                vecl(i) = lb(coord) - x0(coord)
                vecu(i) = ub(coord) - x0(coord)
                vecc(i) += g(coord)
            Next

            Dim vecy = New Double(nfree - 1) {}
            Dim veccm As Double() = CType(vecc.Clone(), Double())
            For i = 0 To veccm.Length - 1
                veccm(i) *= -1.0
            Next

            bfgs.solve_PtBP(WF, veccm, vecy)

            If in_bounds(vecy, vecl, vecu) Then
                subvec_assign(drt, cauchy.fv_set, vecy)
                If Debug.flag Then
                    Debug.debug("-"c, "leaving subspace_minimize, solution has been found")
                End If
                Return
            End If

            Dim yfallback As Double() = CType(vecy.Clone(), Double())

            Dim lambda = New Double(nfree - 1) {}
            Dim mu = New Double(nfree - 1) {}

            Dim L_set As List(Of Integer) = New List(Of Integer)(nfree / 3)
            Dim U_set As List(Of Integer) = New List(Of Integer)(nfree / 3)
            Dim yL_set As List(Of Integer) = New List(Of Integer)(nfree / 3)
            Dim yU_set As List(Of Integer) = New List(Of Integer)(nfree / 3)
            Dim P_set As List(Of Integer) = New List(Of Integer)(nfree)
            Dim yP_set As List(Of Integer) = New List(Of Integer)(nfree)

            Dim k As Integer
            For k = 0 To maxit - 1
                L_set.Clear()
                U_set.Clear()
                P_set.Clear()
                yL_set.Clear()
                yU_set.Clear()
                yP_set.Clear()

                For i = 0 To nfree - 1
                    Dim coord = cauchy.fv_set(i)
                    Dim li = vecl(i)
                    Dim ui = vecu(i)

                    If vecy(i) < li OrElse vecy(i) = li AndAlso lambda(i) >= 0.0 Then
                        L_set.Add(coord)
                        yL_set.Add(i)
                        vecy(i) = li
                        mu(i) = 0.0
                    ElseIf vecy(i) > ui OrElse vecy(i) = ui AndAlso mu(i) >= 0.0 Then
                        U_set.Add(coord)
                        yU_set.Add(i)
                        vecy(i) = ui
                        lambda(i) = 0.0
                    Else
                        P_set.Add(coord)
                        yP_set.Add(i)
                        lambda(i) = 0.0
                        mu(i) = 0.0
                    End If
                Next

                Dim WP = bfgs.Wb(P_set)
                Dim nP = P_set.Count

                If nP > 0 Then
                    Dim rhs = subvec(vecc, yP_set)
                    Dim lL = subvec(vecl, yL_set)
                    Dim uU = subvec(vecu, yU_set)
                    Dim tmp = New Double(nP - 1) {}
                    Dim nonzero = bfgs.apply_PtBQv(WP, L_set, lL, tmp, True)
                    If nonzero Then
                        For i = 0 To rhs.Length - 1
                            rhs(i) += tmp(i)
                        Next
                    End If
                    nonzero = bfgs.apply_PtBQv(WP, U_set, uU, tmp, True)
                    If nonzero Then
                        For i = 0 To rhs.Length - 1
                            rhs(i) += tmp(i)
                        Next
                    End If

                    For i = 0 To rhs.Length - 1
                        rhs(i) *= -1.0
                    Next

                    bfgs.solve_PtBP(WP, rhs, tmp)
                    subvec_assign(vecy, yP_set, tmp)
                End If

                Dim nL = L_set.Count
                Dim nU = U_set.Count
                Dim Fy = New Double(2 * bfgs.m_ncorr - 1) {}
                If nL > 0 OrElse nU > 0 Then
                    bfgs.apply_WtPv(cauchy.fv_set, vecy, Fy)
                End If

                If nL > 0 Then
                    Dim res = New Double(L_set.Count - 1) {}
                    bfgs.apply_PtWMv(L_set, Fy, res, -1.0)
                    Dim svc = subvec(vecc, yL_set)
                    Dim svy = subvec(vecy, yL_set)
                    For i = 0 To res.Length - 1
                        res(i) += svc(i) + bfgs.m_theta * svy(i)
                    Next
                    subvec_assign(lambda, yL_set, res)
                End If

                If nU > 0 Then
                    Dim negRes = New Double(U_set.Count - 1) {}
                    bfgs.apply_PtWMv(U_set, Fy, negRes, -1.0)
                    Dim svc = subvec(vecc, yU_set)
                    Dim svy = subvec(vecy, yU_set)
                    For i = 0 To negRes.Length - 1
                        negRes(i) += svc(i) + bfgs.m_theta * svy(i)
                        negRes(i) *= -1.0
                    Next
                    subvec_assign(mu, yU_set, negRes)
                End If

                If L_converged(yL_set, lambda) AndAlso U_converged(yU_set, mu) AndAlso P_converged(yP_set, vecy, vecl, vecu) Then
                    Exit For
                End If
            Next

            If k >= maxit Then
                For i = 0 To vecy.Length - 1
                    vecy(i) = std.Max(std.Min(vecy(i), vecu(i)), vecl(i))
                Next
                subvec_assign(drt, cauchy.fv_set, vecy)

                Dim dg = Vector.dot(drt, g)

                If dg <= meps Then
                    If Debug.flag Then
                        Debug.debug("-"c, "leaving subspace_minimize, projected")
                    End If
                    Return
                End If

                For i = 0 To vecy.Length - 1
                    vecy(i) = std.Max(std.Min(yfallback(i), vecu(i)), vecl(i))
                Next

                subvec_assign(drt, cauchy.fv_set, vecy)
                dg = Vector.dot(drt, g)

                If dg <= meps Then
                    If Debug.flag Then
                        Debug.debug("-"c, "leaving subspace_minimize, projected unconstrained")
                    End If
                    Return
                End If

                ' If still not, fall back to the unconstrained solution

                subvec_assign(drt, cauchy.fv_set, yfallback)

                If Debug.flag Then
                    Debug.debug("-"c, "leaving subspace_minimize, projected unconstrained")
                End If

                Return
            End If

            subvec_assign(drt, cauchy.fv_set, vecy)

            If Debug.flag Then
                Debug.debug("-"c, "leaving subspace_minimize, converged")
            End If

        End Sub
    End Class

End Namespace

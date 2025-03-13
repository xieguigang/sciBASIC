Imports Microsoft.VisualBasic.Serialization.JSON
Imports std = System.Math

Namespace Framework.Optimization.LBFGSB

    Public NotInheritable Class Cauchy

        Public xcp As Double()
        Public vecc As Double()
        Public newact_set As List(Of Integer)
        Public fv_set As List(Of Integer)

        Public Shared ReadOnly eps As Double = Microsoft.VisualBasic.Math.Ulp(1.0)

        Public Shared Function search_greater(brk As Double(), ord As List(Of Integer), t As Double, start As Integer) As Integer
            Dim nord = ord.Count
            Dim i As Integer
            For i = start To nord - 1
                If brk(ord(i)) > t Then
                    Exit For
                End If
            Next
            Return i
        End Function

        Public Sub New(bfgs As BFGSMat, x0 As Double(), g As Double(), lb As Double(), ub As Double())
            If Debug.flag Then
                Debug.debug("="c, "Cauchy")
                Debug.debug("x0: ", x0)
                Debug.debug(" g: ", g)
            End If

            Dim n = x0.Length
            xcp = CType(x0.Clone(), Double())
            vecc = New Double(2 * bfgs.m_ncorr - 1) {}
            newact_set = New List(Of Integer)(n)
            fv_set = New List(Of Integer)(n)

            Dim brk = New Double(n - 1) {}
            Dim vecd = New Double(n - 1) {}

            Dim ord As List(Of Integer) = New List(Of Integer)(n)

            For i = 0 To n - 1
                If g(i) < 0.0 Then
                    brk(i) = (x0(i) - ub(i)) / g(i)
                ElseIf g(i) > 0.0 Then
                    brk(i) = (x0(i) - lb(i)) / g(i)
                Else
                    brk(i) = Double.PositiveInfinity
                End If

                Dim iszero = brk(i) = 0.0
                vecd(i) = If(iszero, 0.0, -g(i))

                If Double.IsInfinity(brk(i)) Then
                    fv_set.Add(i)
                ElseIf Not iszero Then
                    ord.Add(i)
                End If
            Next

            ord.Sort(Function(a, bb) brk(a).CompareTo(brk(bb)))

            Dim nord = ord.Count
            Dim nfree = fv_set.Count

            If Debug.flag Then
                Debug.debug("    brk: ", brk)
                Debug.debug("   vecd: ", vecd)
                Call Debug.debug("   nord: " & nord.ToString())
                Call Debug.debug("    ord: " & ord.GetJson())
                Call Debug.debug("  nfree: " & nfree.ToString())
                Call Debug.debug(" fv_set: " & fv_set.GetJson())
            End If

            If nfree < 1 AndAlso nord < 1 Then
                If Debug.flag Then
                    Debug.debug("="c, "leaving Cauchy, nfree < 1 && nord < 1")
                End If
                Return
            End If

            Dim vecp = New Double(2 * bfgs.m_ncorr - 1) {}
            bfgs.apply_Wtv(vecd, vecp)
            Dim fp = -Vector.squaredNorm(vecd)

            Dim cache = New Double(2 * bfgs.m_ncorr - 1) {}
            bfgs.apply_Mv(vecp, cache)
            Dim fpp = -bfgs.m_theta * fp - Vector.dot(vecp, cache)

            Dim deltatmin = -fp / fpp

            Dim il = 0.0
            Dim b = 0
            Dim iu = If(nord < 1, Double.PositiveInfinity, brk(ord(b)))
            Dim deltat = iu - il

            Dim crossed_all = False
            Dim wact = New Double(2 * bfgs.m_ncorr - 1) {}

            While deltatmin >= deltat
                For i = 0 To vecc.Length - 1
                    vecc(i) += deltat * vecp(i)
                Next

                Dim act_begin = b
                Dim act_end = search_greater(brk, ord, iu, b) - 1

                If nfree = 0 AndAlso act_end = nord - 1 Then
                    For i = act_begin To act_end
                        Dim act = ord(i)
                        xcp(act) = If(vecd(act) > 0.0, ub(act), lb(act))
                        newact_set.Add(act)
                    Next
                    crossed_all = True
                    Exit While
                End If

                fp += deltat * fpp

                For i = act_begin To act_end
                    Dim act = ord(i)
                    xcp(act) = If(vecd(act) > 0.0, ub(act), lb(act))
                    Dim zact = xcp(act) - x0(act)
                    Dim gact = g(act)
                    Dim ggact = gact * gact
                    wact = bfgs.Wb(act)
                    bfgs.apply_Mv(wact, cache)
                    fp += ggact + bfgs.m_theta * gact * zact - gact * Vector.dot(cache, vecc)
                    fpp -= bfgs.m_theta * ggact + 2 * gact * Vector.dot(cache, vecp) + ggact * Vector.dot(cache, wact)
                    For j = 0 To vecp.Length - 1
                        vecp(j) += gact * wact(j)
                    Next
                    vecd(act) = 0.0
                    newact_set.Add(act)
                Next

                deltatmin = -fp / fpp
                il = iu
                b = act_end + 1
                If b >= nord Then
                    Exit While
                End If
                iu = brk(ord(b))
                deltat = iu - il
            End While

            If fpp < eps Then
                deltatmin = -fp / eps
            End If

            If Not crossed_all Then
                deltatmin = std.Max(deltatmin, 0.0)
                For j = 0 To vecc.Length - 1
                    vecc(j) += deltatmin * vecp(j)
                Next
                Dim tfinal = il + deltatmin
                For i = 0 To nfree - 1
                    Dim coord = fv_set(i)
                    xcp(coord) = x0(coord) + tfinal * vecd(coord)
                Next
                For i = b To nord - 1
                    Dim coord = ord(i)
                    xcp(coord) = x0(coord) + tfinal * vecd(coord)
                    fv_set.Add(coord)
                Next
            End If

            If Debug.flag Then
                Debug.debug("vecc: ", vecc)
                Debug.debug("xcp: ", xcp)
                Call Debug.debug("newact_set: " & newact_set.GetJson())
                Call Debug.debug("fv_set: " & fv_set.GetJson())
                Debug.debug("="c, "Cauchy - end")
            End If
        End Sub
    End Class

End Namespace

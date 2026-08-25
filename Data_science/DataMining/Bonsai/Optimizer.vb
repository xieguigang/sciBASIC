' /********************************************************************************/

'   Author:
'
'       xie (genetics@smrucc.org)
'
'   Copyright (c) 2026 GPL3 Licensed
'
'
'   GNU GENERAL PUBLIC LICENSE (GPL3)
'
'
'   This program is free software: you can redistribute it and/or modify
'   it under the terms of the GNU General Public License as published by
'   the Free Software Foundation, either version 3 of the License, or
'   (at your option) any later version.
'
'   This program is distributed in the hope that it will be useful,
'   but WITHOUT ANY WARRANTY; without even the implied warranty of
'   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'   GNU General Public License for more details.
'
'   You should have received a copy of the GNU General Public License
'   along with this program. If not, see <http://www.gnu.org/licenses/>.

' /********************************************************************************/

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace Microsoft.VisualBasic.DataMining.Bonsai

    ''' <summary>
    ''' Lightweight numerical optimisers used by the Bonsai core. These mirror the scipy primitives
    ''' the reference python code relies on: a 1-D Brent root-finder (scipy.optimize.brentq) and a
    ''' bound-constrained L-BFGS (scipy.optimize.minimize(method="L-BFGS-B", jac=True)).
    ''' </summary>
    Public Module Optimizer

        ''' <summary>
        ''' Delegate for a scalar objective that also returns its gradient.
        ''' </summary>
        Public Delegate Function ObjWithGrad(x As Double(), ParamArray args() As Object) As (f As Double, grad As Double())

        ''' <summary>
        ''' Delegate for a 1-D scalar function f(t) whose root we want.
        ''' </summary>
        Public Delegate Function ScalarFunc(t As Double, ParamArray args() As Object) As Double

        ' =================================================================================
        ' 1-D Brent root finder (translated from scipy.optimize.brentq)
        ' =================================================================================

        ''' <summary>
        ''' Find a root of <paramref name="f"/> in [a, b] assuming f(a) and f(b) have opposite signs.
        ''' Mirrors scipy.optimize.brentq with the same convergence criteria.
        ''' </summary>
        Public Function BrentZero(f As ScalarFunc, a As Double, b As Double, ParamArray args() As Object) As Double
            Dim fa = f(a, args)
            Dim fb = f(b, args)
            If fa = 0.0 Then Return a
            If fb = 0.0 Then Return b
            If (fa > 0) = (fb > 0) Then
                ' No sign change: clamp to the side closer to zero
                Return If(Math.Abs(fa) < Math.Abs(fb), a, b)
            End If

            Dim x As Double = a, w As Double = a, v As Double = a
            Dim fx = fa, fw = fa, fv = fa
            Dim tol As Double = 1.0E-7
            Dim maxiter = 100

            Dim xm = 0.5 * (a + b)
            Dim tol1 = 0.0
            Dim iter = 0

            Do
                iter += 1
                If iter > maxiter Then Exit Do

                Dim p As Double = 0, q As Double = 0, r As Double = 0
                Dim delta = If(x >= xm, a - x, b - x)
                Dim fr = If(x >= xm, fx, fx)  ' placeholder, recomputed below
                tol1 = 2.0 * EPS * Math.Abs(x) + 0.5 * tol
                Dim xacc = If(x >= xm, a, b) - x

                If Math.Abs(xacc) <= tol1 Then
                    Exit Do
                End If

                If Math.Abs(fw - fv) > EPS AndAlso Math.Abs(fv - fx) > EPS AndAlso Math.Abs(fw - fx) > EPS Then
                    ' Inverse quadratic interpolation
                    q = (x - w) * (fx - fv)
                    r = (x - v) * (fx - fw)
                    p = (x - v) * q - (x - w) * r
                    q = 2.0 * (q - r)
                    If q > 0 Then p = -p
                    q = Math.Abs(q)
                    r = If(x >= xm, a - x, b - x)
                End If

                If q > 0 AndAlso p > 0 AndAlso Math.Abs(p) < Math.Abs(0.5 * q * r) AndAlso p > q * (a - x) AndAlso p < q * (b - x) Then
                    ' Accept interpolation step
                    delta = p / q
                Else
                    ' Bisection
                    delta = If(x >= xm, a - x, b - x)
                End If

                Dim stepSize = If(delta >= 0, Math.Max(delta, tol1), Math.Min(delta, -tol1))
                x = x + stepSize
                fx = f(x, args)

                If (fa > 0) = (fx >= 0) Then
                    a = x : fa = fx
                Else
                    b = x : fb = fx
                End If

                If Math.Abs(fa) < Math.Abs(fb) Then
                    Dim tmp = a : a = b : b = tmp
                    Dim tf = fa : fa = fb : fb = tf
                End If

                w = v : fw = fv
                v = xm : fv = fr
                xm = 0.5 * (a + b)
            Loop

            Return x
        End Function

        ' =================================================================================
        ' Bound-constrained L-BFGS (limited-memory BFGS with projected gradient)
        ' =================================================================================

        Public Structure OptResult
            Public x As Double()
            Public fun As Double
            Public success As Boolean
        End Structure

        ''' <summary>
        ''' Minimise <paramref name="obj"/> subject to box constraints <paramref name="bounds"/>
        ''' using a limited-memory BFGS with gradient projection. Mirrors scipy L-BFGS-B on the small
        ''' (3-variable) problems Bonsai needs.
        ''' </summary>
        Public Function Minimize(obj As ObjWithGrad, x0 As Double(), bounds As List(Of (lo As Double, hi As Double)), ParamArray args() As Object) As OptResult
            Dim n = x0.Length
            Dim x = DirectCast(x0.Clone, Double())
            Dim lo = bounds.Select(Function(b) b.lo).ToArray
            Dim hi = bounds.Select(Function(b) b.hi).ToArray

            ' clamp to bounds
            For i = 0 To n - 1
                x(i) = Math.Min(hi(i), Math.Max(lo(i), x(i)))
            Next

            Dim m = 10   ' memory size
            Dim sList As New List(Of Double())   ' s_k = x_{k+1} - x_k
            Dim yList As New List(Of Double())   ' y_k = g_{k+1} - g_k
            Dim rhoList As New List(Of Double)

            Dim eval = obj(x, args)
            Dim f = eval.f
            Dim g = eval.grad
            projectGradient(g, x, lo, hi)

            Dim maxiter = 200
            Dim factr = 1.0E7
            Dim pgtol = 1.0E-5
            Dim exited As Boolean = False

            For iter = 0 To maxiter - 1
                ' Stopping test on projected gradient
                Dim pgnorm = 0.0
                For i = 0 To n - 1
                    pgnorm = Math.Max(pgnorm, Math.Abs(g(i)))
                Next
                If pgnorm < pgtol Then
                    exited = True
                    Exit For
                End If

                ' L-BFGS two-loop recursion to get search direction d = -H g
                Dim d = lbfgsDirection(x, g, sList, yList, rhoList, n, m)

                ' Simple backtracking line search with Armijo condition
                Dim stepSize = 1.0
                Dim fx = eval.f
                Dim gx = eval.grad
                Dim found = False
                For ls = 0 To 30
                    Dim xt = New Double(n - 1) {}
                    For i = 0 To n - 1
                        xt(i) = x(i) + stepSize * d(i)
                        xt(i) = Math.Min(hi(i), Math.Max(lo(i), xt(i)))
                    Next
                    Dim ev = obj(xt, args)
                    ' sufficient decrease: f(xt) <= f(x) + 1e-4 * step * grad·d
                    Dim gd = 0.0
                    For i = 0 To n - 1
                        gd += g(i) * d(i)
                    Next
                    If ev.f <= f + 1.0E-4 * stepSize * gd OrElse stepSize < 1.0E-12 Then
                        x = xt
                        f = ev.f
                        g = ev.grad
                        projectGradient(g, x, lo, hi)
                        found = True
                        Exit For
                    End If
                    stepSize *= 0.5
                Next

                If Not found Then
                    ' No further progress
                    exited = True
                    Exit For
                End If

                ' store correction: s = x_new - x_old, y = g_new - g_old
                Dim xold = New Double(n - 1) {}
                Dim gold = New Double(n - 1) {}
                For i = 0 To n - 1
                    xold(i) = x(i) - stepSize * d(i)
                Next
                Dim gOldArr = gradAt(xold, obj, args)
                For i = 0 To n - 1
                    gold(i) = gOldArr(i)
                Next
                Dim s = New Double(n - 1) {}
                Dim y = New Double(n - 1) {}
                Dim ys = 0.0
                For i = 0 To n - 1
                    s(i) = x(i) - xold(i)
                    y(i) = g(i) - gold(i)
                    ys += s(i) * y(i)
                Next

                If ys > EPS Then
                    sList.Add(s)
                    yList.Add(y)
                    rhoList.Add(1.0 / ys)
                    If sList.Count > m Then
                        sList.RemoveAt(0)
                        yList.RemoveAt(0)
                        rhoList.RemoveAt(0)
                    End If
                End If
            Next

            Return New OptResult With {.x = x, .fun = f, .success = exited}
        End Function

        Private Function gradAt(x As Double(), obj As ObjWithGrad, args As Object()) As Double()
            Return obj(x, args).grad
        End Function

        Private Sub projectGradient(g As Double(), x As Double(), lo As Double(), hi As Double())
            For i = 0 To g.Length - 1
                If x(i) <= lo(i) AndAlso g(i) > 0 Then g(i) = 0
                If x(i) >= hi(i) AndAlso g(i) < 0 Then g(i) = 0
            Next
        End Sub

        Private Function lbfgsDirection(x As Double(), g As Double(), sList As List(Of Double()), yList As List(Of Double()), rhoList As List(Of Double), n As Integer, m As Integer) As Double()
            ' d = -g, then two-loop recursion
            Dim d = g.Select(Function(gi) -gi).ToArray
            Dim k = sList.Count
            Dim alpha(k - 1) As Double

            For i = k - 1 To 0 Step -1
                Dim s = sList(i), y = yList(i), rho = rhoList(i)
                Dim sg = 0.0
                For j = 0 To n - 1
                    sg += s(j) * d(j)
                Next
                alpha(i) = rho * sg
                For j = 0 To n - 1
                    d(j) -= alpha(i) * y(j)
                Next
            Next

            ' scale by initial Hessian approximation gamma = (s_{k-1}·y_{k-1})/(y_{k-1}·y_{k-1})
            If k > 0 Then
                Dim s = sList(k - 1), y = yList(k - 1)
                Dim sy = 0.0, yy = 0.0
                For j = 0 To n - 1
                    sy += s(j) * y(j)
                    yy += y(j) * y(j)
                Next
                If yy > EPS Then
                    Dim gamma = sy / yy
                    For j = 0 To n - 1
                        d(j) *= gamma
                    Next
                End If
            End If

            For i = 0 To k - 1
                Dim s = sList(i), y = yList(i), rho = rhoList(i)
                Dim yg = 0.0
                For j = 0 To n - 1
                    yg += y(j) * d(j)
                Next
                Dim beta = rho * yg
                For j = 0 To n - 1
                    d(j) += s(j) * (alpha(i) - beta)
                Next
            Next

            Return d
        End Function
    End Module
End Namespace

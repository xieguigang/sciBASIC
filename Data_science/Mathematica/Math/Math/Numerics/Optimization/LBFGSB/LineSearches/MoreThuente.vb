#Region "Microsoft.VisualBasic::1ac62a5944d5e1ed62e8f96751d6a827, Data_science\Mathematica\Math\Math\Numerics\Optimization\LBFGSB\LineSearches\MoreThuente.vb"

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

    '   Total Lines: 481
    '    Code Lines: 408 (84.82%)
    ' Comment Lines: 2 (0.42%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 71 (14.76%)
    '     File Size: 18.38 KB


    '     Class MoreThuente
    ' 
    ' 
    '         Enum RESULT
    ' 
    '             CONVERGED, MAX_ITERS, NONE, OUT_RANGE, STPMAX
    '             STPMIN, XTOL, ZERODG
    ' 
    ' 
    ' 
    '         Class Bool
    ' 
    '             Constructor: (+2 Overloads) Sub New
    ' 
    '         Class CStepType
    ' 
    ' 
    ' 
    '         Class PhiDPhi
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: evaluate
    ' 
    '  
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: cstep
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace Framework.Optimization.LBFGSB.LineSearches


    ' https://github.com/JuliaNLSolvers/LineSearches.jl/blob/master/src/morethuente.jl
    ' https://github.com/SurajGupta/r-source/blob/master/src/appl/lbfgsb.c#L2976

    Public Class MoreThuente : Inherits AbstractLineSearch

        Public Enum RESULT
            NONE
            CONVERGED
            OUT_RANGE
            XTOL
            STPMAX
            STPMIN
            MAX_ITERS
            ZERODG
        End Enum

        Public Class Bool
            Friend b As Boolean

            Public Sub New()
                Me.New(False)
            End Sub

            Public Sub New(b As Boolean)
                Me.b = b
            End Sub
        End Class

        Public Class CStepType
            Public stx, fx, dx, sty, fy, dy, stp, fp, dp As Double
            Public bracketed As Boolean
        End Class

        Public Class PhiDPhi
            Friend f As IGradFunction
            Friend drt As Double() ' search direction (cauchy point - x)
            Friend xp As Double() ' previous x
            Friend x As Double() ' current x
            Friend grad As Double() ' gradient
            Public dg As Double ' dot(grad,drt)

            Public Sub New(f As IGradFunction, x As Double(), grad As Double(), xp As Double(), drt As Double())
                Me.f = f
                Me.x = x
                Me.grad = grad
                Me.xp = xp
                Me.drt = drt
            End Sub

            Public Overridable Function evaluate(alpha As Double) As Double
                For i = 0 To x.Length - 1
                    x(i) = xp(i) + alpha * drt(i)
                Next

                Dim f = Me.f.eval(x, grad)
                dg = Vector.dot(grad, drt)
                Return f
            End Function
        End Class

        Public info As RESULT

        Public Shared ReadOnly eps As Double = Microsoft.VisualBasic.Math.Ulp(1.0)
        Public Shared ReadOnly iterfinitemax As Integer = -std.Log(eps) / std.Log(2.0)

        Public Sub New(fun As IGradFunction, param As Parameters, xp As Double(), drt As Double(), step_max As Double, _step As Double, _fx As Double, grad As Double(), _dg As Double, x As Double(), weak_wolfe As Boolean)
            If Debugger.flag Then
                Debugger.debug("-"c, "line search")
                Debugger.debug("      xp: ", xp)
                Debugger.debug("       x: ", x)
                Call Debugger.debug("      fx: " & _fx.ToString())
                Debugger.debug("    grad: ", grad)
                Call Debugger.debug("      dg: " & _dg.ToString())
                Call Debugger.debug("    step: " & _step.ToString())
                Call Debugger.debug("step_max: " & step_max.ToString())
                Debugger.debug("     drt: ", drt)
            End If

            MyBase._fx = _fx
            MyBase._step = _step
            MyBase._dg = _dg

            If MyBase.dg >= 0.0 Then
                Throw New LBFGSBException("the moving direction does not decrease the objective function value, dg=" & MyBase.dg.ToString())
            End If

            Dim stp = MyBase.step
            Dim stpmin = param.min_step
            Dim stpmax = step_max

            Dim phidphi As PhiDPhi = New PhiDPhi(fun, x, grad, xp, drt)

            info = RESULT.NONE
            Dim bracketed As Bool = New Bool()
            Dim stage1 = True

            Dim finit = MyBase._fx
            Dim ginit = MyBase._dg
            Dim gtest = param.ftol * MyBase.dg ' armijo condition
            Dim ctest = param.wolfe * MyBase.dg ' curvature test, wolfe condition
            Dim width = stpmax - stpmin
            Dim width1 = 2.0 * width

            Dim stx = 0.0
            Dim fx = finit
            Dim gx = ginit
            Dim sty = 0.0
            Dim fy = finit
            Dim gy = ginit

            Dim stmin = 0.0
            Dim stmax = stp + stp * 4.0

            Dim cs As CStepType = New CStepType()

            Dim f = phidphi.evaluate(stp)
            MyBase._dg = phidphi.dg

            Dim iterfinite = 0
            While (Double.IsInfinity(fx) OrElse Double.IsInfinity(MyBase.dg) OrElse Double.IsNaN(fx) OrElse Double.IsNaN(MyBase.dg)) AndAlso iterfinite < iterfinitemax
                stp = stp / 2.0

                f = phidphi.evaluate(stp)
                MyBase._dg = phidphi.dg

                stx = stp * 7.0 / 8.0
                iterfinite += 1
            End While

            If Debugger.flag Then
                Debugger.debug(">"c, "entering loop")
                Call Debugger.debug("       stp: " & stp.ToString())
                Call Debugger.debug("       stx: " & stx.ToString())
            End If

            Dim iter = 0
            While True
                If Debugger.flag Then
                    Call Debugger.debug("  line search iter:" & iter.ToString())
                End If

                f = phidphi.evaluate(stp)
                MyBase._dg = phidphi.dg

                If std.Abs(MyBase.dg) < eps Then
                    info = RESULT.ZERODG
                    Me._step = stp
                    Me._fx = f

                    If Debugger.flag Then
                        Call Debugger.debug("    step: " & stp.ToString())
                        Call Debugger.debug("      fx: " & f.ToString())
                        Debugger.debug("       x: ", x)
                        Debugger.debug("    grad: ", grad)
                        Call Debugger.debug("-"c, "leaving line search, dg = " & MyBase.dg.ToString())
                    End If
                    Return
                End If

                Dim ftest = finit + stp * gtest

                If stage1 AndAlso f < ftest AndAlso MyBase.dg >= 0.0 Then
                    stage1 = False
                End If

                If bracketed.b AndAlso (stp <= stmin OrElse stp >= stmax) Then
                    info = RESULT.OUT_RANGE
                End If
                If stp = stpmax AndAlso f <= ftest AndAlso MyBase.dg <= gtest Then
                    info = RESULT.STPMAX
                End If
                If stp = stpmin AndAlso (f > ftest OrElse MyBase.dg >= gtest) Then
                    info = RESULT.STPMIN
                End If
                If iter >= param.max_linesearch Then
                    info = RESULT.MAX_ITERS
                End If
                If bracketed.b AndAlso stmax - stmin <= param.xtol * stmax Then
                    info = RESULT.XTOL
                End If
                If Not weak_wolfe AndAlso f <= ftest AndAlso std.Abs(MyBase.dg) <= -ctest OrElse weak_wolfe AndAlso f <= ftest AndAlso MyBase.dg >= ctest Then
                    info = RESULT.CONVERGED
                End If

                If info <> RESULT.NONE Then
                    Me._step = stp
                    Me._fx = f

                    If Debugger.flag Then
                        Call Debugger.debug("    step: " & stp.ToString())
                        Call Debugger.debug("      fx: " & f.ToString())
                        Debugger.debug("       x: ", x)
                        Debugger.debug("    grad: ", grad)
                        Call Debugger.debug("-"c, "leaving line search, info = " & info.ToString())
                    End If
                    Return
                End If

                If stage1 AndAlso f < fx AndAlso f > ftest Then
                    Dim fm = f - stp * gtest
                    Dim fxm = fx - stx * gtest
                    Dim fym = fy - sty * gtest
                    Dim dgm = MyBase.dg - gtest
                    Dim gxm = gx - gtest
                    Dim gym = gy - gtest

                    cs.stx = stx
                    cs.fx = fxm
                    cs.dx = gxm
                    cs.sty = sty
                    cs.fy = fym
                    cs.dy = gym
                    cs.stp = stp
                    cs.fp = fm
                    cs.dp = dgm
                    cs.bracketed = bracketed.b

                    cstep(cs, stmin, stmax)

                    stx = cs.stx
                    fxm = cs.fx
                    gxm = cs.dx
                    sty = cs.sty
                    fym = cs.fy
                    gym = cs.dy
                    stp = cs.stp
                    fm = cs.fp
                    bracketed.b = cs.bracketed

                    fx = fxm + stx * gtest
                    fy = fym + sty * gtest
                    gx = gxm + gtest
                    gy = gym + gtest
                Else
                    cs.stx = stx
                    cs.fx = fx
                    cs.dx = gx
                    cs.sty = sty
                    cs.fy = fy
                    cs.dy = gy
                    cs.stp = stp
                    cs.fp = f
                    cs.dp = MyBase.dg
                    cs.bracketed = bracketed.b

                    cstep(cs, stmin, stmax)

                    stx = cs.stx
                    fx = cs.fx
                    gx = cs.dx
                    sty = cs.sty
                    fy = cs.fy
                    gy = cs.dy
                    stp = cs.stp
                    f = cs.fp
                    MyBase._dg = cs.dp
                    bracketed.b = cs.bracketed
                End If

                If bracketed.b Then
                    Dim adiff = std.Abs(sty - stx)
                    If adiff >= width1 * 0.66 Then
                        stp = stx + (sty - stx) / 2.0
                    End If
                    width1 = width
                    width = adiff
                End If

                If bracketed.b Then
                    stmin = std.Min(stx, sty)
                    stmax = std.Max(stx, sty)
                Else
                    stmin = stp + 1.1 * (stp - stx)
                    stmax = stp + 4.0 * (stp - stx)
                End If

                stp = If(stp < stpmin, stpmin, stp)
                stp = If(stp > stpmax, stpmax, stp)

                If Debugger.flag Then
                    Call Debugger.debug("  stmin: " & stmin.ToString())
                    Call Debugger.debug("  stmax: " & stmax.ToString())
                    Call Debugger.debug("  stp: " & stp.ToString())
                End If

                iter += 1

                If bracketed.b AndAlso (stp <= stmin OrElse stp >= stmax) OrElse bracketed.b AndAlso stmax - stmin <= param.xtol * stmax OrElse iter >= param.max_linesearch Then
                    If Debugger.flag Then
                        Call Debugger.debug("  fallback to stx: " & stx.ToString())
                    End If
                    stp = stx
                End If

            End While
        End Sub

        Private Sub cstep(cs As CStepType, stpmin As Double, stpmax As Double)
            Dim sgnd = cs.dp * (cs.dx / std.Abs(cs.dx))

            If Debugger.flag Then
                Debugger.debug("<< cstep")
                Call Debugger.debug(" stx=" & cs.stx.ToString())
                Call Debugger.debug("  fx=" & cs.fx.ToString())
                Call Debugger.debug("  dx=" & cs.dx.ToString())
                Call Debugger.debug(" sty=" & cs.sty.ToString())
                Call Debugger.debug("  fy=" & cs.fy.ToString())
                Call Debugger.debug("  dy=" & cs.dy.ToString())
                Call Debugger.debug(" stp=" & cs.stp.ToString())
                Call Debugger.debug("  fp=" & cs.fp.ToString())
                Call Debugger.debug("  dp=" & cs.dp.ToString())
                Call Debugger.debug("  br=" & cs.bracketed.ToString())
                Call Debugger.debug(" min=" & stpmin.ToString())
                Call Debugger.debug(" max=" & stpmax.ToString())
                Call Debugger.debug("sgnd=" & sgnd.ToString())
            End If

            Dim stpf As Double

            If cs.fp > cs.fx Then

                If Debugger.flag Then
                    Debugger.debug("= Case 1")
                End If

                Dim theta = 3.0 * (cs.fx - cs.fp) / (cs.stp - cs.stx) + cs.dx + cs.dp
                Dim s = std.Max(std.Max(std.Abs(theta), std.Abs(cs.dx)), std.Abs(cs.dp))
                Dim d1 = theta / s
                Dim gamm = s * std.Sqrt(d1 * d1 - cs.dx / s * (cs.dp / s))
                If cs.stp < cs.stx Then
                    gamm = -gamm
                End If
                Dim p = gamm - cs.dx + theta
                Dim q = gamm - cs.dx + gamm + cs.dp
                Dim r = p / q
                Dim stpc = cs.stx + r * (cs.stp - cs.stx)
                Dim stpq = cs.stx + cs.dx / ((cs.fx - cs.fp) / (cs.stp - cs.stx) + cs.dx) / 2.0 * (cs.stp - cs.stx)
                If std.Abs(stpc - cs.stx) < std.Abs(stpq - cs.stx) Then
                    stpf = stpc
                Else
                    stpf = (stpc + stpq) / 2.0
                End If
                If Debugger.flag Then
                    Call Debugger.debug("= stpf: " & stpf.ToString())
                End If
                cs.bracketed = True
            ElseIf sgnd < 0.0 Then
                If Debugger.flag Then
                    Debugger.debug("= Case 2")
                End If

                Dim theta = 3.0 * (cs.fx - cs.fp) / (cs.stp - cs.stx) + cs.dx + cs.dp
                Dim s = std.Max(std.Max(std.Abs(theta), std.Abs(cs.dx)), std.Abs(cs.dp))
                Dim d1 = theta / s
                Dim gamm = s * std.Sqrt(d1 * d1 - cs.dx / s * (cs.dp / s))
                If cs.stp > cs.stx Then
                    gamm = -gamm
                End If
                Dim p = gamm - cs.dp + theta
                Dim q = gamm - cs.dp + gamm + cs.dx
                Dim r = p / q
                Dim stpc = cs.stp + r * (cs.stx - cs.stp)
                Dim stpq = cs.stp + cs.dp / (cs.dp - cs.dx) * (cs.stx - cs.stp)
                If std.Abs(stpc - cs.stp) < std.Abs(stpq - cs.stp) Then
                    stpf = stpc
                Else
                    stpf = stpq
                End If
                If Debugger.flag Then
                    Call Debugger.debug("= stpf: " & stpf.ToString())
                End If
                cs.bracketed = True
            ElseIf std.Abs(cs.dp) < std.Abs(cs.dx) Then
                If Debugger.flag Then
                    Debugger.debug("= Case 3")
                End If

                Dim theta = 3.0 * (cs.fx - cs.fp) / (cs.stp - cs.stx) + cs.dx + cs.dp
                Dim s = std.Max(std.Max(std.Abs(theta), std.Abs(cs.dx)), std.Abs(cs.dp))

                Dim d1 = theta / s
                d1 = d1 * d1 - cs.dx / s * (cs.dp / s)
                Dim gamm = If(d1 <= 0.0, 0.0, s * std.Sqrt(d1))

                If cs.stp > cs.stx Then
                    gamm = -gamm
                End If

                Dim p = gamm - cs.dp + theta
                Dim q = gamm + (cs.dx - cs.dp) + gamm
                Dim r = p / q

                Dim stpc As Double
                If r < 0.0 AndAlso gamm <> 0.0 Then
                    stpc = cs.stp + r * (cs.stx - cs.stp)
                ElseIf cs.stp > cs.stx Then
                    stpc = stpmax
                Else
                    stpc = stpmin
                End If

                Dim stpq = cs.stp + cs.dp / (cs.dp - cs.dx) * (cs.stx - cs.stp)
                If cs.bracketed Then
                    If std.Abs(stpc - cs.stp) < std.Abs(stpq - cs.stp) Then
                        stpf = stpc
                    Else
                        stpf = stpq
                    End If
                    d1 = cs.stp + (cs.sty - cs.stp) * 0.66
                    If cs.stp > cs.stx Then
                        stpf = std.Min(d1, stpf)
                    Else
                        stpf = std.Max(d1, stpf)
                    End If
                Else
                    If std.Abs(stpc - cs.stp) > std.Abs(stpq - cs.stp) Then
                        stpf = stpc
                    Else
                        stpf = stpq
                    End If
                    stpf = std.Min(std.Max(stpmin, stpf), stpmax)
                End If
                If Debugger.flag Then
                    Call Debugger.debug("= stpf: " & stpf.ToString())
                End If
            Else
                If Debugger.flag Then
                    Debugger.debug("= Case 4")
                End If
                If cs.bracketed Then
                    Dim theta = 3.0 * (cs.fp - cs.fy) / (cs.sty - cs.stp) + cs.dy + cs.dp
                    Dim s = std.Max(std.Max(std.Abs(theta), std.Abs(cs.dy)), std.Abs(cs.dp))
                    Dim d1 = theta / s
                    Dim gamm = s * std.Sqrt(d1 * d1 - cs.dy / s * (cs.dp / s))
                    If cs.stp > cs.sty Then
                        gamm = -gamm
                    End If
                    Dim p = cs.stp - cs.dp + theta
                    Dim q = gamm - cs.dp + gamm + cs.dy
                    Dim r = p / q
                    stpf = cs.stp + r * (cs.sty - cs.stp)
                ElseIf cs.stp > cs.stx Then
                    stpf = stpmax
                Else
                    stpf = stpmin
                End If

                If Debugger.flag Then
                    Call Debugger.debug("= stpf: " & stpf.ToString())
                End If
            End If

            If cs.fp > cs.fx Then
                cs.sty = cs.stp
                cs.fy = cs.fp
                cs.dy = cs.dp
            Else
                If sgnd < 0.0 Then
                    cs.sty = cs.stx
                    cs.fy = cs.fx
                    cs.dy = cs.dx
                End If
                cs.stx = cs.stp
                cs.fx = cs.fp
                cs.dx = cs.dp
            End If

            cs.stp = stpf

            If Debugger.flag Then
                Call Debugger.debug("= [2]stp: " & cs.stp.ToString())
            End If
        End Sub
    End Class

End Namespace

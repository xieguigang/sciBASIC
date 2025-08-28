#Region "Microsoft.VisualBasic::16de0512171627ab228638faa6364931, Data_science\Mathematica\Math\Math\Numerics\Optimization\LBFGSB\LineSearches\LineSearch.vb"

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

    '   Total Lines: 307
    '    Code Lines: 229 (74.59%)
    ' Comment Lines: 20 (6.51%)
    '    - Xml Docs: 15.00%
    ' 
    '   Blank Lines: 58 (18.89%)
    '     File Size: 13.16 KB


    '     Class LineSearch
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: cubic_minimizer, (+2 Overloads) quadratic_minimizer, step_selection
    '         Class Bool
    ' 
    '             Constructor: (+2 Overloads) Sub New
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace Framework.Optimization.LBFGSB.LineSearches

    ''' <summary>
    ''' Line search methods for optimization and root-finding
    ''' </summary>
    Public NotInheritable Class LineSearch
        Inherits AbstractLineSearch

        Public Class Bool
            Friend b As Boolean

            Public Sub New()
                Me.New(False)
            End Sub

            Public Sub New(b As Boolean)
                Me.b = b
            End Sub
        End Class

        Public Shared ReadOnly eps As Double = Microsoft.VisualBasic.Math.Ulp(1.0)

        Public Shared Function quadratic_minimizer(a As Double, b As Double, fa As Double, ga As Double, fb As Double) As Double
            Dim ba = b - a
            Return a + 0.5 * ba * ba * ga / (fa - fb + ba * ga)
        End Function

        Public Shared Function quadratic_minimizer(a As Double, b As Double, ga As Double, gb As Double) As Double
            Return a + (b - a) * ga / (ga - gb)
        End Function

        Public Shared Function cubic_minimizer(a As Double, b As Double, fa As Double, fb As Double, ga As Double, gb As Double, exists As Bool) As Double
            Dim apb = a + b
            Dim ba = b - a
            Dim ba2 = ba * ba
            Dim fba = fb - fa
            Dim gba = gb - ga

            Dim z3 = (ga + gb) * ba - 2.0 * fba
            Dim z2 = 0.5 * (gba * ba2 - 3.0 * apb * z3)
            Dim z1 = fba * ba2 - apb * z2 - (a * apb + b * b) * z3

            If std.Abs(z3) < eps * std.Abs(z2) OrElse std.Abs(z3) < eps * std.Abs(z1) Then
                ' Minimizer exists if c2 > 0
                exists.b = z2 * ba > 0.0
                ' Return the end point if the minimizer does not exist
                Return If(exists.b, -0.5 * z1 / z2, b)
            End If

            ' Now we can assume z3 > 0
            ' The minimizer is a solution to the equation c1 + 2*c2 * x + 3*c3 * x^2 = 0
            ' roots = -(z2/z3) / 3 (+-) sqrt((z2/z3)^2 - 3 * (z1/z3)) / 3
            '
            ' Let u = z2/(3z3) and v = z1/z2
            ' The minimizer exists if v/u <= 1
            Dim u = z2 / (3.0 * z3), v = z1 / z2
            Dim vu = v / u
            exists.b = vu <= 1.0
            If Not exists.b Then
                Return b
            End If

            ' We need to find a numerically stable way to compute the roots, as z3 may
            ' still be small
            '
            ' If |u| >= |v|, let w = 1 + sqrt(1-v/u), and then
            ' r1 = -u * w, r2 = -v / w, r1 does not need to be the smaller one
            '
            ' If |u| < |v|, we must have uv <= 0, and then
            ' r = -u (+-) sqrt(delta), where
            ' sqrt(delta) = sqrt(|u|) * sqrt(|v|) * sqrt(1-u/v)
            Dim r1 = 0.0, r2 = 0.0
            If std.Abs(u) >= std.Abs(v) Then
                Dim w = 1.0 + std.Sqrt(1.0 - vu)
                r1 = -u * w
                r2 = -v / w
            Else
                Dim sqrtd = std.Sqrt(std.Abs(u)) * std.Sqrt(std.Abs(v)) * std.Sqrt(1 - u / v)
                r1 = -u - sqrtd
                r2 = -u + sqrtd
            End If
            Return If(z3 * ba > 0.0, std.Max(r1, r2), std.Min(r1, r2))

        End Function

        Public Const deltal As Double = 1.1
        Public Const deltau As Double = 0.66

        Public Shared Function step_selection(al As Double, au As Double, at As Double, fl As Double, fu As Double, ft As Double, gl As Double, gu As Double, gt As Double) As Double
            If al = au Then
                Return al
            End If

            If Double.IsInfinity(ft) OrElse Double.IsInfinity(gt) Then
                Return (al + at) / 2.0
            End If

            Dim ac_exists As Bool = New Bool()
            Dim ac = cubic_minimizer(al, at, fl, ft, gl, gt, ac_exists)
            Dim aq = quadratic_minimizer(al, at, fl, gl, ft)

            If ft > fl Then
                If Not ac_exists.b Then
                    Return aq
                End If

                Return If(std.Abs(ac - al) < std.Abs(aq - al), ac, (aq + ac) / 2.0)

            End If

            Dim [as] = quadratic_minimizer(al, at, gl, gt)
            If gt * gl < 0.0 Then
                Return If(std.Abs(ac - at) >= std.Abs([as] - at), ac, [as])
            End If

            If std.Abs(gt) < std.Abs(gl) Then
                Dim res = If(ac_exists.b AndAlso (ac - at) * (at - al) > 0.0 AndAlso std.Abs(ac - at) < std.Abs([as] - at), ac, [as])
                Return If(at > al, std.Min(at + deltau * (au - at), res), std.Max(at + deltau * (au - at), res))
            End If

            If Double.IsInfinity(au) OrElse Double.IsInfinity(fu) OrElse Double.IsInfinity(gu) Then
                Return at + deltal * (at - al)
            End If

            Dim ae_exists As Bool = New Bool()
            Dim ae = cubic_minimizer(at, au, ft, fu, gt, gu, ae_exists)
            Return If(at > al, std.Min(at + deltau * (au - at), ae), std.Max(at + deltau * (au - at), ae))
        End Function

        Public Const delta As Double = 1.1

        Public Sub New(f As IGradFunction, param As Parameters, xp As Double(), drt As Double(), step_max As Double, _step As Double, _fx As Double, grad As Double(), _dg As Double, x As Double(), weak_wolfe As Boolean)
            If Debugger.flag Then
                Call Debugger.debug("-"c, "line search")
                Call Debugger.debug("      xp: ", xp)
                Call Debugger.debug("       x: ", x)
                Call Debugger.debug("      fx: " & _fx.ToString())
                Call Debugger.debug("    grad: ", grad)
                Call Debugger.debug("      dg: " & _dg.ToString())
                Call Debugger.debug("    step: " & _step.ToString())
                Call Debugger.debug("step_max: " & step_max.ToString())
                Call Debugger.debug("     drt: ", drt)
            End If

            MyBase._fx = _fx
            MyBase._step = _step
            MyBase._dg = _dg

            If [step] <= 0.0 Then
                Throw New LBFGSBException("step must be positive, step=" & [step].ToString())
            End If
            If [step] > step_max Then
                Throw New LBFGSBException("step exceeds max step, step=" & [step].ToString() & ", step_max=" & step_max.ToString())
            End If

            Dim fx_init = fx
            Dim dg_init = dg

            Dim test_decr = param.ftol * dg_init
            Dim test_curv = param.wolfe * dg_init
            Dim I_lo = 0.0, I_hi = Double.PositiveInfinity
            Dim fI_lo = 0.0, fI_hi = Double.PositiveInfinity
            Dim gI_lo = (1.0 - param.ftol) * dg_init, gI_hi = Double.PositiveInfinity

            For i = 0 To x.Length - 1
                x(i) = xp(i) + [step] * drt(i)
            Next

            MyBase._fx = f.eval(x, grad)
            MyBase._dg = Vector.dot(grad, drt)

            If Debugger.flag Then
                Call Debugger.debug("-> before wolfe and loop")
                Call Debugger.debug("test_decr: " & test_decr.ToString())
                Call Debugger.debug("test_curv: " & test_curv.ToString())
                Call Debugger.debug("        x: ", x)
                Call Debugger.debug("       fx: " & fx.ToString())
                Call Debugger.debug("     grad: ", grad)
                Call Debugger.debug("       dg: " & dg.ToString())
                Call Debugger.debug("wolfe cond 1: " & fx.ToString() & " <= " & (fx_init + [step] * test_decr).ToString() & " == " & (fx <= fx_init + [step] * test_decr).ToString())
                Call Debugger.debug("wolfe cond 2: " & std.Abs(dg).ToString() & " <= " & test_curv.ToString() & " == " & (std.Abs(dg) <= test_curv).ToString())
            End If

            If fx <= fx_init + [step] * test_decr AndAlso std.Abs(dg) <= test_curv Then
                If Debugger.flag Then
                    Debugger.debug("-"c, "leaving line search, criteria met")
                End If
                Return
            End If

            If Debugger.flag Then
                Debugger.debug("-> entering loop")
            End If

            Dim iter As Integer
            For iter = 0 To param.max_linesearch - 1
                Dim ft = fx - fx_init - [step] * test_decr
                Dim gt = dg - param.ftol * dg_init

                If Debugger.flag Then
                    Call Debugger.debug("iter: " & iter.ToString())
                    Call Debugger.debug("  ft: " & ft.ToString())
                    Call Debugger.debug("  gt: " & gt.ToString())
                End If

                Dim new_step As Double
                If ft > fI_lo Then
                    new_step = step_selection(I_lo, I_hi, [step], fI_lo, fI_hi, ft, gI_lo, gI_hi, gt)

                    If new_step <= param.min_step Then
                        new_step = (I_lo + [step]) / 2.0
                    End If

                    I_hi = [step]
                    fI_hi = ft
                    gI_hi = gt

                    If Debugger.flag Then
                        Call Debugger.debug("-- case 1, " & ft.ToString() & " > " & fI_lo.ToString())
                        Call Debugger.debug("-- new_step: " & new_step.ToString())
                    End If
                ElseIf gt * (I_lo - [step]) > 0.0 Then
                    new_step = std.Min(step_max, [step] + delta * ([step] - I_lo))

                    I_lo = [step]
                    fI_lo = ft
                    gI_lo = gt

                    If Debugger.flag Then
                        Call Debugger.debug("-- case 2, " & (gt * (I_lo - [step])).ToString() & " > 0.0")
                        Call Debugger.debug("-- new_step: " & new_step.ToString())
                    End If
                Else
                    new_step = step_selection(I_lo, I_hi, [step], fI_lo, fI_hi, ft, gI_lo, gI_hi, gt)

                    I_hi = I_lo
                    fI_hi = fI_lo
                    gI_hi = gI_lo

                    I_lo = [step]
                    fI_lo = ft
                    gI_lo = gt

                    If Debugger.flag Then
                        Debugger.debug("-- case 3")
                        Call Debugger.debug("-- new_step: " & new_step.ToString())
                    End If
                End If

                If [step] = step_max AndAlso new_step >= step_max Then
                    If Debugger.flag Then
                        Debugger.debug("-"c, "leaving line search, maximum step size reached")
                    End If
                    Return
                End If

                MyBase._step = new_step

                If [step] < param.min_step Then
                    Throw New LBFGSBException("the line search step became smaller than the minimum value allowed")
                End If

                If [step] > param.max_step Then
                    Throw New LBFGSBException("the line search step became larger than the maximum value allowed")
                End If

                For i = 0 To x.Length - 1
                    x(i) = xp(i) + [step] * drt(i)
                Next
                MyBase._fx = f.eval(x, grad)
                MyBase._dg = Vector.dot(grad, drt)

                If Debugger.flag Then
                    Call Debugger.debug("     x: ", x)
                    Call Debugger.debug("    fx: " & fx.ToString())
                    Call Debugger.debug("  grad: ", grad)
                    Call Debugger.debug("    dg: " & dg.ToString())
                    Call Debugger.debug("  wolfe cond 1: " & fx.ToString() & " <= " & (fx_init + [step] * test_decr).ToString() & " == " & (fx <= fx_init + [step] * test_decr).ToString())
                    Call Debugger.debug("  wolfe cond 2: " & std.Abs(dg).ToString() & " <= " & test_curv.ToString() & " == " & (std.Abs(dg) <= test_curv).ToString())
                End If

                If Not weak_wolfe AndAlso fx <= fx_init + [step] * test_decr AndAlso std.Abs(dg) <= -test_curv OrElse weak_wolfe AndAlso fx <= fx_init + [step] * test_decr AndAlso std.Abs(dg) >= test_curv Then
                    If Debugger.flag Then
                        Debugger.debug("-"c, "leaving line search, criteria met (2)")
                    End If
                    Return
                End If

                If [step] >= step_max Then
                    Dim ft2 = fx - fx_init - [step] * test_decr
                    If ft2 <= fI_lo Then
                        If Debugger.flag Then
                            Debugger.debug("-"c, "leaving line search, maximum step size reached (2)")
                        End If

                        Return
                    End If
                End If
            Next

            Throw New LBFGSBException("the line search routine reached the maximum number of iterations")
        End Sub
    End Class

End Namespace

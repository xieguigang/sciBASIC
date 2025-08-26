#Region "Microsoft.VisualBasic::8ea3db0c09f604ce912a70196b59ac6c, Data_science\Mathematica\Math\Math\Numerics\Optimization\LBFGSB\LineSearches\LewisOverton.vb"

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

'   Total Lines: 196
'    Code Lines: 143 (72.96%)
' Comment Lines: 16 (8.16%)
'    - Xml Docs: 93.75%
' 
'   Blank Lines: 37 (18.88%)
'     File Size: 6.34 KB


'     Class LewisOverton
' 
' 
'         Enum RESULT
' 
'             CONVERGED, MAX_ITERS, NONE, STEPTOL, STPMAX
'             STPMIN, ZERODG
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
'     Sub: finish
' 
' 
' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace Framework.Optimization.LBFGSB.LineSearches


    ' https://github.com/ZJU-FAST-Lab/LBFGS-Lite/blob/master/include/lbfgs.hpp#L276
    Public Class LewisOverton
        Inherits AbstractLineSearch

        Public Enum RESULT
            NONE
            CONVERGED
            STPMAX
            STPMIN
            MAX_ITERS
            ZERODG
            STEPTOL
        End Enum

        Public Class PhiDPhi

            Friend f As IGradFunction

            ''' <summary>
            ''' search direction (cauchy point - x)
            ''' </summary>
            Friend drt As Double()
            ''' <summary>
            ''' previous x
            ''' </summary>
            Friend xp As Double()
            ''' <summary>
            ''' current x
            ''' </summary>
            Friend x As Double()
            ''' <summary>
            ''' gradient
            ''' </summary>
            Friend grad As Double()
            ''' <summary>
            ''' dot(grad,drt)
            ''' </summary>
            Public dg As Double

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

        Private Sub finish(info As RESULT, f As Double, stp As Double)
            _fx = f
            _step = stp
            Me.info = info

            If Debugger.flag Then
                Call Debugger.debug("    step: " & stp.ToString())
                Call Debugger.debug("      fx: " & f.ToString())
                Call Debugger.debug("-"c, "leaving line search, dg = " & MyBase.dg.ToString())
            End If
        End Sub

        Public Sub New(fun As IGradFunction, param As Parameters, xp As Double(), drt As Double(), step_max As Double, _step As Double, _fx As Double, grad As Double(), _dg As Double, x As Double())
            If Debugger.flag Then
                Debugger.debug("-"c, "LewisOverton line search")
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

            info = RESULT.NONE

            Dim brackt = False
            Dim touched = False

            Dim finit = MyBase.fx
            Dim dgtest = param.ftol * MyBase.dg
            Dim dstest = param.wolfe * MyBase.dg
            Dim mu = 0.0
            Dim nu = stpmax

            Dim phidphi As PhiDPhi = New PhiDPhi(fun, x, grad, xp, drt)

            Dim f = phidphi.evaluate(stp)
            MyBase._dg = phidphi.dg

            Dim iterfinite = 0
            While (Double.IsInfinity(MyBase.fx) OrElse Double.IsInfinity(MyBase.dg)) AndAlso iterfinite < iterfinitemax
                stp = stp / 2.0

                f = phidphi.evaluate(stp)
                MyBase._dg = phidphi.dg
            End While

            If Debugger.flag Then
                Debugger.debug(">"c, "entering loop")
                Call Debugger.debug("       stp: " & stp.ToString())
            End If

            Dim iter = 0
            While True
                If Debugger.flag Then
                    Call Debugger.debug("  line search iter:" & iter.ToString())
                End If

                f = phidphi.evaluate(stp)
                MyBase._dg = phidphi.dg

                If std.Abs(MyBase.dg) < eps Then
                    finish(RESULT.ZERODG, f, stp)
                    Return
                End If

                If f > finit + stp * dgtest Then
                    mu = stp
                    brackt = True
                Else
                    If MyBase.dg < dstest Then
                        mu = stp
                    Else
                        finish(RESULT.CONVERGED, f, stp)
                        Return
                    End If
                End If

                If iter >= param.max_linesearch Then
                    finish(RESULT.MAX_ITERS, f, stp)
                    Return
                End If

                If brackt AndAlso nu - mu < eps * nu Then
                    finish(RESULT.STEPTOL, f, stp)
                    Return
                End If

                If brackt Then
                    stp = 0.5 * (mu + nu)
                Else
                    stp *= 2.0
                End If

                If stp < stpmin Then
                    finish(RESULT.STPMIN, f, stpmin)
                    Return
                Else
                    If touched Then
                        finish(RESULT.STPMAX, f, stpmax)
                        Return
                    Else
                        touched = True
                        stp = stpmax
                    End If
                End If

                iter += 1

            End While
        End Sub
    End Class

End Namespace

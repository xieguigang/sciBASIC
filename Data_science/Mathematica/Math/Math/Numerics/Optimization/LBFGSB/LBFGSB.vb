#Region "Microsoft.VisualBasic::388ce9f9f23f1b28940155f84accba54, Data_science\Mathematica\Math\Math\Numerics\Optimization\LBFGSB\LBFGSB.vb"

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

    '   Total Lines: 261
    '    Code Lines: 179 (68.58%)
    ' Comment Lines: 37 (14.18%)
    '    - Xml Docs: 64.86%
    ' 
    '   Blank Lines: 45 (17.24%)
    '     File Size: 10.23 KB


    '     Class LBFGSB
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: max_step_size, minimize, proj_grad_norm
    ' 
    '         Sub: force_bounds, reset
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB.LineSearches
Imports std = System.Math

Namespace Framework.Optimization.LBFGSB

    ''' <summary>
    ''' L-BFGS-B optimizer
    ''' 
    ''' # LBFGSBJava
    ''' 
    ''' L-BFGS-B box constrained optimizer 
    ''' 
    ''' * Most of the code ported from [LBFGS++](https://lbfgspp.statr.me/) code.
    ''' * MoreThuente line search ported from [R source](https://github.com/SurajGupta/r-source/blob/master/src/appl/lbfgsb.c#L2976) and 
    '''       [Julia LineSearches.jl](https://github.com/JuliaNLSolvers/LineSearches.jl/blob/master/src/morethuente.jl)
    ''' * MoreThuente can be used with strong or weak (default) Wolfe condition 
    ''' * LewisOverton (experimental, don't use) line search ported from [LBFGS-Lite](https://github.com/ZJU-FAST-Lab/LBFGS-Lite/blob/master/include/lbfgs.hpp)
    ''' 
    ''' LBFGSB is implemented without any dependencies.
    ''' 
    ''' See `org.generateme.lbfgsb.examples` for usage cases.
    ''' 
    ''' ## Target function
    ''' 
    ''' Each target function should implement `IGradFunction` interface.
    ''' 
    ''' * default implementation of `gradient` uses finite differences method
    ''' * implementing only `evaluate(x)` will use finite differences
    ''' * there is a possibility to calculate function value and gradient in one call, implement `gradient(x,grad)` and 
    '''   implement `in_place_gradient` to return `true`. See `Rosenbrock` in examples.
    ''' 
    ''' ## Licence
    ''' 
    ''' Copyright (c) 2023 GenerateMe
    ''' 
    ''' The MIT Licence
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/genmeblog/LBFGSBJava
    ''' </remarks>
    Public NotInheritable Class LBFGSB
        Public m_param As Parameters
        Public m_bfgs As BFGSMat
        Public m_fx As Double()
        Public m_xp As Double()
        Public m_grad As Double()
        Public m_gradp As Double()
        Public m_drt As Double()
        Public m_projgnorm As Double
        Public fx As Double
        Public k As Integer

        Public Sub reset(n As Integer)
            m_bfgs.reset(n, m_param.m)
            m_xp = Vector.resize(m_xp, n)
            m_grad = Vector.resize(m_grad, n)
            m_gradp = Vector.resize(m_gradp, n)
            m_drt = Vector.resize(m_drt, n)
            If m_param.past > 0 Then
                m_fx = Vector.resize(m_fx, m_param.past)
            End If
        End Sub

        Public Sub New()
            Me.New(New Parameters())
        End Sub

        Public Sub New(param As Parameters)
            m_param = param
            m_bfgs = New BFGSMat()
        End Sub

        Public Function maxit(n As Integer) As LBFGSB
            m_param.max_iterations = n
            Return Me
        End Function

        Public Shared Sub force_bounds(x As Double(), lb As Double(), ub As Double())
            For i = 0 To x.Length - 1
                x(i) = std.Max(std.Min(x(i), ub(i)), lb(i))
            Next
        End Sub

        Public Shared Function proj_grad_norm(x As Double(), g As Double(), lb As Double(), ub As Double()) As Double
            Dim res = 0.0

            For i = 0 To x.Length - 1
                Dim proj = std.Max(std.Min(x(i) - g(i), ub(i)), lb(i))
                res = std.Max(res, std.Abs(proj - x(i)))
            Next

            Return res
        End Function

        Public Shared Function max_step_size(x0 As Double(), drt As Double(), lb As Double(), ub As Double()) As Double
            Dim [step] = Double.PositiveInfinity

            For i = 0 To x0.Length - 1
                If drt(i) > 0.0 Then
                    [step] = std.Min([step], (ub(i) - x0(i)) / drt(i))
                ElseIf drt(i) < 0 Then
                    [step] = std.Min([step], (lb(i) - x0(i)) / drt(i))
                End If
            Next

            Return [step]
        End Function

        Public Shared ReadOnly eps As Double = Microsoft.VisualBasic.Math.Ulp(1.0)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="f">function</param>
        ''' <param name="[in]">initial and final value</param>
        ''' <param name="lb">lower bounds</param>
        ''' <param name="ub">upper bounds</param>
        ''' <returns></returns>
        Public Function minimize(f As IGradFunction, [in] As Double(), lb As Double(), ub As Double()) As Double()
            If Debugger.flag Then
                Debugger.debug("="c, "entering minimization")
            End If

            Dim x As Double() = CType([in].Clone(), Double())

            Dim n = x.Length

            If lb.Length <> n OrElse ub.Length <> n Then
                Throw New LBFGSBException("lb and ub must have the same size as x")
            End If

            force_bounds(x, lb, ub)

            reset(n)
            Dim fpast = m_param.past

            fx = f.eval(x, m_grad)
            m_projgnorm = proj_grad_norm(x, m_grad, lb, ub)

            If Debugger.flag Then
                Debugger.debug("initial")
            End If
            If Debugger.flag Then
                Call Debugger.debug("  fx:        " & fx.ToString())
            End If
            If Debugger.flag Then
                Call Debugger.debug("  projgnorm: " & m_projgnorm.ToString())
            End If
            If Debugger.flag Then
                Debugger.debug("  x:         ", x)
            End If
            If Debugger.flag Then
                Debugger.debug("  grad:      ", m_grad)
            End If

            If fpast > 0 Then
                m_fx(0) = fx
            End If

            If m_projgnorm <= m_param.epsilon OrElse m_projgnorm <= m_param.epsilon_rel * Vector.norm(x) Then
                If Debugger.flag Then
                    Call Debugger.debug("="c, "leaving minimization, projgnorm less than epsilon, projgnorm = " & m_projgnorm.ToString())
                End If
                Return x
            End If

            ' Cauchy stores xcp, vecc, newact_set and fv_set
            Dim cauchy As Cauchy = New Cauchy(m_bfgs, x, m_grad, lb, ub)

            Vector.sub(cauchy.xcp, x, m_drt)
            If m_param.linesearch = LINESEARCH.MORETHUENTE_LBFGSPP Then
                Vector.normalize(m_drt) ' problematic
            End If

            Dim vecs = New Double(n - 1) {}
            Dim vecy = New Double(n - 1) {}

            k = 1

            While True
                If Debugger.flag Then
                    Call Debugger.debug("#"c, "K = " & k.ToString())
                End If

                m_xp = CType(x.Clone(), Double())
                m_gradp = CType(m_grad.Clone(), Double())

                Dim dg = Vector.dot(m_grad, m_drt)
                Dim step_max = max_step_size(x, m_drt, lb, ub)

                If dg >= 0.0 OrElse step_max <= m_param.min_step Then
                    Vector.sub(cauchy.xcp, x, m_drt)
                    m_bfgs.reset(n, m_param.m)
                    dg = Vector.dot(m_grad, m_drt)
                    step_max = max_step_size(x, m_drt, lb, ub)
                End If

                step_max = std.Min(step_max, m_param.max_step)
                Dim [step] = std.Min(1.0, step_max)

                Dim ls As AbstractLineSearch
                Select Case m_param.linesearch
                    Case LINESEARCH.MORETHUENTE_LBFGSPP
                        ls = New LineSearches.LineSearch(f, m_param, m_xp, m_drt, step_max, [step], fx, m_grad, dg, x, m_param.weak_wolfe)
                    Case LINESEARCH.LEWISOVERTON
                        ls = New LewisOverton(f, m_param, m_xp, m_drt, step_max, [step], fx, m_grad, dg, x)
                    Case Else
                        ls = New MoreThuente(f, m_param, m_xp, m_drt, step_max, [step], fx, m_grad, dg, x, m_param.weak_wolfe)
                End Select

                fx = ls.fx
                [step] = ls.step
                dg = ls.dg

                m_projgnorm = proj_grad_norm(x, m_grad, lb, ub)

                If Debugger.flag Then
                    Call Debugger.debug("  fx:        " & fx.ToString())
                End If
                If Debugger.flag Then
                    Call Debugger.debug("  projgnorm: " & m_projgnorm.ToString())
                End If
                If Debugger.flag Then
                    Debugger.debug("  x:         ", x)
                End If
                If Debugger.flag Then
                    Debugger.debug("  grad:      ", m_grad)
                End If

                If m_projgnorm <= m_param.epsilon OrElse m_projgnorm <= m_param.epsilon_rel * Vector.norm(x) Then
                    If Debugger.flag Then
                        Call Debugger.debug("="c, "leaving minimization, projgnorm less than epsilons, projgnorm = " & m_projgnorm.ToString())
                    End If
                    Return x
                End If

                If fpast > 0 Then
                    Dim fxd = m_fx(k Mod fpast)
                    If k >= fpast AndAlso std.Abs(fxd - fx) <= m_param.delta * std.Max(std.Max(std.Abs(fx), std.Abs(fxd)), 1.0) Then
                        If Debugger.flag Then
                            Debugger.debug("="c, "leaving minimization, past results less than delta")
                        End If
                        Return x
                    End If
                    m_fx(k Mod fpast) = fx
                End If

                If m_param.max_iterations <> 0 AndAlso k >= m_param.max_iterations Then
                    If Debugger.flag Then
                        Debugger.debug("="c, "leaving minimization, max iterations reached")
                    End If
                    Return x
                End If

                Vector.sub(x, m_xp, vecs)
                Vector.sub(m_grad, m_gradp, vecy)

                If Vector.dot(vecs, vecy) > eps * Vector.squaredNorm(vecy) Then
                    m_bfgs.add_correction(vecs, vecy)
                End If

                force_bounds(x, lb, ub)
                cauchy = New Cauchy(m_bfgs, x, m_grad, lb, ub)

                SubspaceMin.subspace_minimize(m_bfgs, x, m_grad, lb, ub, cauchy, m_param.max_submin, m_drt)

                k += 1
            End While

            Throw New InvalidProgramException("invalid loop!")
        End Function
    End Class

End Namespace

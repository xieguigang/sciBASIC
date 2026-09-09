#Region "Microsoft.VisualBasic::97a2d9f9d97b2cc2fa0598d559784967, Data_science\Mathematica\Math\Math\Algebra\LP\IPMCrossover\LppSolver.vb"

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

    '   Total Lines: 321
    '    Code Lines: 215 (66.98%)
    ' Comment Lines: 40 (12.46%)
    '    - Xml Docs: 22.50%
    ' 
    '   Blank Lines: 66 (20.56%)
    '     File Size: 13.28 KB


    '     Module LppSolver
    ' 
    '         Function: AppendBoundRows, Fail, HasFiniteUpper, ObjOrig, Ok
    '                   SnapToBounds, Solve, SolveStandard
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' LppSolver.vb — 完整流水线组装 → LPPSolution [readme §六 流水线图]
' ----------------------------------------------------------------------------
'   LppProblem / CSR → 标准形（含下界平移与上下界）
'     → 原始-对偶内点法(Mehrotra, 有界变量)
'     → [无上界的小规模问题] Crossover → 最优基可行解
'     → [有上界或大规模] 内点解直接输出（吸附到边界）
'     → [IPM 失败且规模允许] 上界转行 + 纯单纯形兜底（不可行/无界证书）
'     → 映射回问题空间 → LPPSolution
'
' 规模分派：
'   · 稀疏标准形（IsSparse，基因组规模）：跳过 crossover 与单纯形兜底
'     —— Crossover 的 EchelonRank 与单纯形每次主元重构 LU 在万级规模下不可用，
'    而 FBA 只需要通量与目标值，不需要顶点解。
'   · 有上界问题同样跳过 crossover（crossover 目前只覆盖 x ≥ 0 情形），
'     改为把内点解按容差吸附到最近的界上。
'
' 报告量（问题空间）：
'   obj = c_origᵀv + ObjOffset；slack_i = b_i − A_i·v；
'   shadowPrice_i = σ·flipSign_i·y_i = ∂(原始目标)/∂b_i；
'   reducedCost_j = c_j − Σ_i A_ij·shadowPrice_i
' ============================================================================

Imports std = System.Math
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Linq

Namespace LinearAlgebra.LinearProgramming.IPMCrossover

    Public Module LppSolver

        ''' <summary>主入口：求解 LppProblem → LPPSolution（稠密路径）</summary>
        Public Function Solve(prob As LppProblem, Optional decimalFormat As String = "G5") As LPPSolution
            Return SolveStandard(StandardForm.FromProblem(prob), decimalFormat)
        End Function

        ''' <summary>
        ''' 标准形入口：FBA 等大规模问题直接构造好 StandardForm 后走这里
        ''' </summary>
        Public Function SolveStandard(sf As StandardForm,
                                      Optional decimalFormat As String = "G5",
                                      Optional maxIter As Int32 = 200) As LPPSolution
            Dim sw = Stopwatch.StartNew()
            Dim feasMs As Long = -1
            Dim log As New List(Of String)()
            Dim n As Int32 = sf.N
            Dim m As Int32 = sf.M
            Dim nc As Int32 = sf.N + sf.NSlack
            Dim anyBound As Boolean = HasFiniteUpper(sf.U)

            log.Add($"LppSolver: {If(sf.Sigma < 0, "max", "min")} {n} vars × {m} constraints " &
                    $"[IPM{(If(sf.IsSparse, " + sparse", ""))}{(If(anyBound, " + bounds", ""))}]")

            ' ---- 平凡情形：无约束 ----
            If m = 0 Then
                Dim x0(nc - 1) As Double

                For j As Int32 = 0 To nc - 1
                    If sf.c(j) < -0.000000000001 AndAlso (Not anyBound OrElse Double.IsInfinity(sf.U(j))) Then
                        log.Add("无约束且存在负成本方向 → 无界")
                        Return Fail("LP 目标无界（无约束且存在可无限增大的下降方向）", log, sw.ElapsedMilliseconds)
                    End If
                    If sf.c(j) < 0 Then x0(j) = sf.U(j)
                Next

                feasMs = sw.ElapsedMilliseconds

                Return Ok(sf, x0, Nothing, ObjOrig(sf, x0), -1, log, sw, feasMs, decimalFormat)
            End If

            ' ---- 内点法 ----
            Dim ipmLog As New List(Of String)()
            Dim ipm As New InteriorPointSolver(sf.Mat, sf.b, sf.c, If(anyBound, sf.U, Nothing), 0.00000001, maxIter, ipmLog)
            Dim r = ipm.Solve()

            log.AddRange(ipmLog)
            log.Add($"  IPM 状态: {r.Status}（{r.Iters} 次迭代）")

            If r.Status = "optimal" Then
                feasMs = sw.ElapsedMilliseconds
            End If

            Dim finalX As Double() = Nothing
            Dim finalY As Double() = Nothing
            Dim pivots As Int32 = -1
            Dim sxResult As SimplexResult = Nothing

            If r.Status = "optimal" Then
                If (Not anyBound) AndAlso (Not sf.IsSparse) Then
                    ' ---- Crossover（仅无上界的中小规模问题）----
                    Dim cx As New CrossoverSolver(sf.A, sf.b, sf.c, log)
                    Dim cr = cx.Run(r.X, r.S)

                    If cr.Status = "optimal" Then
                        finalX = cr.X
                        finalY = cr.Y
                        pivots = cr.Pivots
                    Else
                        log.Add($"  Crossover 失败({cr.Status}) → 直接使用内点解")
                    End If
                End If

                If finalX Is Nothing Then
                    ' 有上界或大规模：内点解直接输出，并把贴近界的分量吸附到界上
                    finalX = SnapToBounds(r.X, sf.U, log)
                    finalY = r.Y
                    log.Add("  输出内点解（已吸附到边界）")
                End If
            ElseIf Not sf.IsSparse Then
                ' ---- 单纯形兜底（仅在规模允许时；有界变量先转成显式行）----
                Dim A2 As Double(,) = sf.A
                Dim b2 As Double() = sf.b
                Dim c2 As Double() = sf.c

                If anyBound Then
                    A2 = AppendBoundRows(sf.A, sf.U, b2, c2)
                End If

                Dim sx As New SimplexSolver(A2, b2, c2, log)

                sxResult = sx.Solve()
                log.Add($"  单纯形状态: {sxResult.Status}（{sxResult.Iters} 次迭代，冗余行 {sxResult.DropRows.Count}）")

                If sxResult.Status = "optimal" Then
                    finalX = New Double(nc - 1) {}
                    Array.Copy(sxResult.X, finalX, std.Min(sxResult.X.Length, nc))
                    finalY = sxResult.Y
                    If feasMs < 0 Then feasMs = sw.ElapsedMilliseconds
                End If
            End If

            sw.Stop()

            Dim totalMs = sw.ElapsedMilliseconds

            If feasMs < 0 Then feasMs = totalMs

            ' ---- 输出 ----
            If finalX Is Nothing Then
                Dim msg As String

                Select Case sxResult?.Status
                    Case "infeasible"
                        msg = "LP 不可行（Phase 1 人造基目标 > tol 的证书）"
                    Case "unbounded"
                        msg = "LP 目标无界（存在无阻挡下降射线证书）"
                    Case "numeric_fail"
                        msg = "数值失败（LU 分解奇异）"
                    Case "max_iter"
                        msg = "迭代数超限未收敛"
                    Case Else
                        msg = If(sf.IsSparse,
                                 $"求解失败: {r.Status}（IPM 未收敛，且问题规模过大无法用单纯形兜底）",
                                 $"求解失败: {r.Status}")
                End Select

                Return New LPPSolution(msg, String.Join(vbLf, log), feasMs)
            End If

            Dim objVal = ObjOrig(sf, finalX)

            Return Ok(sf, finalX, finalY, objVal, pivots, log, sw, feasMs, decimalFormat)
        End Function

        ''' <summary>把有限上界 x_j + t_j = u_j 追加为显式约束行（单纯形兜底专用）</summary>
        Private Function AppendBoundRows(A As Double(,), U As Double(),
                                         ByRef b As Double(), ByRef c As Double()) As Double(,)
            Dim m As Int32 = A.GetLength(0)
            Dim nc As Int32 = A.GetLength(1)
            Dim idx As New List(Of Int32)()

            For j As Int32 = 0 To nc - 1
                If Not Double.IsInfinity(U(j)) Then idx.Add(j)
            Next

            Dim nB As Int32 = idx.Count

            If nB = 0 Then Return A

            Dim A2(m + nB - 1, nc + nB - 1) As Double
            Dim b2(m + nB - 1) As Double
            Dim c2(nc + nB - 1) As Double

            For i As Int32 = 0 To m - 1
                For j As Int32 = 0 To nc - 1
                    A2(i, j) = A(i, j)
                Next
                b2(i) = b(i)
            Next

            For j As Int32 = 0 To nc - 1
                c2(j) = c(j)
            Next

            For k As Int32 = 0 To nB - 1
                Dim j As Int32 = idx(k)
                Dim row As Int32 = m + k

                A2(row, j) = 1.0
                A2(row, nc + k) = 1.0
                b2(row) = U(j)
            Next

            b = b2
            c = c2

            Return A2
        End Function

        ''' <summary>是否存在有限上界</summary>
        Public Function HasFiniteUpper(U As Double()) As Boolean
            If U Is Nothing Then Return False

            For j As Int32 = 0 To U.Length - 1
                If Not Double.IsInfinity(U(j)) Then Return True
            Next

            Return False
        End Function

        ''' <summary>把贴近界的分量吸附到界上（内点解总有 1e-8 量级的内部间隙）</summary>
        Private Function SnapToBounds(x As Double(), U As Double(), log As List(Of String)) As Double()
            Dim y As Double() = New Double(x.Length - 1) {}
            Dim snapped As Int32 = 0

            Array.Copy(x, y, x.Length)

            If U IsNot Nothing Then
                For j As Int32 = 0 To y.Length - 1
                    If Double.IsInfinity(U(j)) Then Continue For

                    Dim tol As Double = 0.00000001 * std.Max(1.0, U(j))

                    If y(j) <= tol Then
                        y(j) = 0.0
                        snapped += 1
                    ElseIf y(j) >= U(j) - tol Then
                        y(j) = U(j)
                        snapped += 1
                    ElseIf y(j) < 0.0 Then
                        y(j) = 0.0
                    ElseIf y(j) > U(j) Then
                        y(j) = U(j)
                    End If
                Next
            Else
                For j As Int32 = 0 To y.Length - 1
                    If y(j) < 0.0 Then y(j) = 0.0
                Next
            End If

            If log IsNot Nothing AndAlso snapped > 0 Then
                log.Add($"  边界吸附: {snapped} / {y.Length} 个变量落在界上")
            End If

            Return y
        End Function

        ''' <summary>原始目标值 c_origᵀv + ObjOffset</summary>
        Private Function ObjOrig(sf As StandardForm, xStd As Double()) As Double
            Dim s As Double = sf.ObjOffset

            For j As Int32 = 0 To sf.N - 1
                s += sf.COrig(j) * (sf.LbShift(j) + xStd(j))
            Next

            Return s
        End Function

        Private Function Fail(msg As String, log As List(Of String), feasMs As Long) As LPPSolution
            Return New LPPSolution(msg, String.Join(vbLf, log), feasMs)
        End Function

        ''' <summary>从标准形解提取问题空间报告量并构造 LPPSolution</summary>
        Private Function Ok(sf As StandardForm, xStd As Double(), yStd As Double(),
                            objVal As Double, pivots As Int32, log As List(Of String),
                            sw As Stopwatch, feasMs As Long, decimalFormat As String) As LPPSolution
            Dim n = sf.N
            Dim m = sf.M

            ' 原始解：v = lb + x
            Dim xOrig As Double() = sf.ToOriginal(xStd)

            ' 影子价（原始方向）
            Dim shadow(m - 1) As Double

            If yStd IsNot Nothing AndAlso yStd.Length >= m Then
                For i As Int32 = 0 To m - 1
                    shadow(i) = sf.MapShadowPrice(i, yStd)
                Next
            End If

            ' slack / reduced cost（原始空间直算，全部走稀疏友好的 Mv / Mtv）
            Dim lhs As Double() = sf.MatOrig.Mv(xOrig)
            Dim slackArr(m - 1) As Double

            For i As Int32 = 0 To m - 1
                slackArr(i) = sf.BOrig(i) - lhs(i)
                If std.Abs(slackArr(i)) < 0.000000001 * (1.0 + std.Abs(sf.BOrig(i))) Then slackArr(i) = 0.0
            Next

            Dim aty As Double() = sf.MatOrig.Mtv(shadow)
            Dim reduced(n - 1) As Double

            For j As Int32 = 0 To n - 1
                reduced(j) = sf.COrig(j) - aty(j)
                If std.Abs(reduced(j)) < 0.000000001 * (1.0 + std.Abs(sf.COrig(j))) Then reduced(j) = 0.0
            Next

            log.Add($"  完成: 目标值 = {objVal:G10}")

            Return New LPPSolution(xOrig, objVal, sf.VarNames, sf.ConstraintTypeList,
                                   slackArr, shadow, reduced,
                                   sw.ElapsedMilliseconds, feasMs,
                                   String.Join(vbLf, log), decimalFormat)
        End Function

    End Module

End Namespace


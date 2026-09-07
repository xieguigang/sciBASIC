' ============================================================================
' LppSolver.vb — 完整流水线组装 → LPPSolution [readme §六 流水线图]
' ----------------------------------------------------------------------------
'   LppProblem → 标准形 → 原始-对偶内点法(Mehrotra)
'     → 近似最优内点解 → Crossover(划分/构基/消超基本/单纯形收尾)
'     → 最优基可行解 → 映射回原始空间 → LPPSolution
' 兜底链：IPM 任意状态失败 → 纯单纯形(Phase1/2) 独立求解并给出
'   不可行/无界证书；crossover 失败同理。
' 报告量（原始方向）：
'   obj = c_origᵀx；slack_i = b_i − A_i·x（≤ 为正 slack、≥ 为负= surplus、= 绑定 0）；
'   shadowPrice_i = σ·flipSign_i·y_i = ∂(原始目标)/∂b_i；
'   reducedCost_j = c_j − Σ_i A_ij·shadowPrice_i。
' ============================================================================

Imports std = System.Math
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Linq

Namespace LinearAlgebra.LinearProgramming.IPMCrossover

    Public Module LppSolver

        ''' <summary>主入口：求解 LppProblem → LPPSolution</summary>
        Public Function Solve(prob As LppProblem, Optional decimalFormat As String = "G5") As LPPSolution
            Dim sw = Stopwatch.StartNew()
            Dim feasMs As Long = -1
            Dim log As New List(Of String)()
            log.Add($"LppSolver: {prob.ObjectiveSense} {prob.Variables.Count} vars × {prob.Constraints.Count} constraints [IPM + Crossover]")

            ' ---- 标准形 ----
            Dim sf = StandardForm.FromProblem(prob)
            Dim n = sf.N
            Dim m = sf.M
            Dim nc = n + sf.NSlack

            ' ---- 平凡情形：无约束 ----
            If m = 0 Then
                For j = 0 To n - 1
                    If sf.c(j) < -0.000000000001 Then
                        log.Add("无约束且存在负成本方向 → 无界")
                        Return Fail("LP 目标无界（无约束且存在可无限增大的下降方向）", log, sw.ElapsedMilliseconds)
                    End If
                Next
                Dim x0(n - 1) As Double
                Dim obj0 = ObjOrig(sf, x0)
                feasMs = sw.ElapsedMilliseconds
                Return Ok(sf, x0, New Double(n + sf.NSlack - 1) {}, obj0, 0, log, sw, feasMs, decimalFormat)
            End If

            ' ---- 内点法 ----
            Dim ipmLog As New List(Of String)()
            Dim ipm As New InteriorPointSolver(sf.A, sf.b, sf.c, 0.00000001, 200, ipmLog)
            Dim r = ipm.Solve()
            log.AddRange(ipmLog)
            log.Add($"  IPM 状态: {r.Status}（{r.Iters} 次迭代）")
            If r.Status = "optimal" Then
                feasMs = sw.ElapsedMilliseconds
            End If

            Dim sxResult As SimplexResult = Nothing
            Dim finalX As Double() = Nothing
            Dim finalY As Double() = Nothing
            Dim pivots As Int32 = -1
            Dim usedCrossover As Boolean = False

            If r.Status = "optimal" Then
                ' ---- Crossover ----
                Dim cx As New CrossoverSolver(sf.A, sf.b, sf.c, log)
                Dim cr = cx.Run(r.X, r.S)
                If cr.Status = "optimal" Then
                    finalX = cr.X
                    finalY = cr.Y
                    pivots = cr.Pivots
                    usedCrossover = True
                    If feasMs < 0 Then feasMs = sw.ElapsedMilliseconds
                Else
                    log.Add($"  Crossover 失败({cr.Status}) → 纯单纯形兜底")
                End If
            End If

            If finalX Is Nothing Then
                ' ---- 纯单纯形兜底（证书权威来源）----
                Dim sx As New SimplexSolver(sf.A, sf.b, sf.c, log)
                sxResult = sx.Solve()
                log.Add($"  单纯形状态: {sxResult.Status}（{sxResult.Iters} 次迭代，冗余行 {sxResult.DropRows.Count}）")
                If sxResult.Status = "optimal" Then
                    finalX = sxResult.X
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
                        msg = $"求解失败: {r.Status}"
                End Select
                Return New LPPSolution(msg, String.Join(vbLf, log), feasMs)
            End If

            Dim objVal = ObjOrig(sf, finalX)
            Return Ok(sf, finalX, finalY, objVal, pivots, log, sw, feasMs, decimalFormat)
        End Function

        ''' <summary>原始目标值 c_origᵀx</summary>
        Private Function ObjOrig(sf As StandardForm, xStd As Double()) As Double
            Dim s As Double = 0
            For j = 0 To sf.N - 1
                s += sf.Sigma * sf.c(j) * xStd(j)
            Next
            Return s
        End Function

        Private Function Fail(msg As String, log As List(Of String), feasMs As Long) As LPPSolution
            Return New LPPSolution(msg, String.Join(vbLf, log), feasMs)
        End Function

        ''' <summary>从标准形解提取原始空间报告量并构造 LPPSolution</summary>
        Private Function Ok(sf As StandardForm, xStd As Double(), yStd As Double(),
                            objVal As Double, pivots As Int32, log As List(Of String),
                            sw As Stopwatch, feasMs As Long, decimalFormat As String) As LPPSolution
            Dim n = sf.N
            Dim m = sf.M
            ' 原始解
            Dim xOrig(n - 1) As Double
            For j = 0 To n - 1
                xOrig(j) = xStd(j)
            Next
            ' 影子价（原始方向）
            Dim shadow(m - 1) As Double
            For i = 0 To m - 1
                shadow(i) = sf.MapShadowPrice(i, yStd)
            Next
            ' slack / reduced cost（原始数据直算，绑定判定做容差归零）
            Dim slackArr(m - 1) As Double
            For i = 0 To m - 1
                Dim lhs As Double = 0
                For j = 0 To n - 1
                    lhs += sf.AOriginal(i, j) * xOrig(j)
                Next
                slackArr(i) = sf.BOriginal(i) - lhs
                If std.Abs(slackArr(i)) < 0.000000001 * (1.0 + std.Abs(sf.BOriginal(i))) Then slackArr(i) = 0.0
            Next
            Dim reduced(n - 1) As Double
            For j = 0 To n - 1
                Dim s = sf.COriginal(j)
                For i = 0 To m - 1
                    s -= sf.AOriginal(i, j) * shadow(i)
                Next
                reduced(j) = s
                If std.Abs(reduced(j)) < 0.000000001 * (1.0 + std.Abs(sf.COriginal(j))) Then reduced(j) = 0.0
            Next
            Dim summary = If(pivots >= 0, $"IPM + crossover(主元 {pivots} 次)", "纯单纯形兜底")
            log.Add($"  完成: {summary}，目标值 = {objVal:G10}")
            Return New LPPSolution(xOrig, objVal, sf.VarNames, sf.ConstraintTypeList,
                                   slackArr, shadow, reduced,
                                   sw.ElapsedMilliseconds, feasMs,
                                   String.Join(vbLf, log), decimalFormat)
        End Function

    End Module

End Namespace

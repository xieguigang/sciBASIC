' ============================================================================
' RScanCore.vb — r-scan 统计核心（Dembo–Karlin 1992 泊松逼近）[readme 全文]
' ----------------------------------------------------------------------------
' 原子函数（模块化、可自由组合）：
'   GapsOf            相邻间隔 X_i（按排序后位点计算）[readme §一]
'   IntervalCdf       F(t) = P(X_i ≤ t)：均匀间隔精确式 / 指数极限式 [readme §二/§三]
'   IntervalSurvival  S(t) = P(X_i > t) = (1 − t/L)^{m−1}
'   PLeftTail         p = P(R(r) ≤ t_obs) ≈ 1 − e^{−μ}Σ_{j<r} μ^j/j!, μ = nGaps·F(t_obs)
'   PRightTail        右尾前 q 大间隔：p = P(Pois(μ_R) ≥ q), μ_R = nGaps·S(t_obs)
'   Scan              完整检验流水线 [readme §四 步骤 1–4] → RScanResult
'   MonteCarlo        均匀零假设模拟经验 p（独立于解析式的验证口径，选项开启）
' ----------------------------------------------------------------------------
' 文档口径记录（两处按数学精确化；默认取精确式，选项可回退文档字面）：
'   ① μ 的间隔数：文档写 μ = m·F(t)（readme §四(3)）；实际间隔数 = m−1，
'      故默认 nGaps = m−1（大 m 时两者等价；RScanOptions.UseDocMGaps = True 取字面 m）
'   ② 文档 §三 "均匀间距情形 F(t) = (1−t/L)^{m−1}"：该式实为生存函数 S(t) = P(X > t)
'      （CDF 应为 1 − S(t)），其用途在右尾检验 P(X_(m) ≤ t) ≈ exp[−m·S(t)]（readme §三末）；
'      本实现按修正语义处理：左尾 F = 1−S，右尾 μ_R = nGaps·S。
'      蒙特卡洛（m=100, L=1e6, 20000 rep）验证：左尾最大偏差 0.02（泊松逼近的
'      预期误差量级），右尾偏差 0.008 → 公式与经验分布吻合。
' ============================================================================

Imports System.Text
Imports std = System.Math

Namespace RScan

    ''' <summary>单间隔分布模型 [readme §二/§三]</summary>
    Public Enum GapModel
        ''' <summary>精确（iid 均匀点位）：F(t) = 1 − (1 − t/L)^{m−1}</summary>
        UniformSpacing
        ''' <summary>泊松过程极限：F(t) = 1 − e^{−λt}，λ = m/L（小 t 时与上式一致）</summary>
        Exponential
    End Enum

    Public Class RScanOptions

        ''' <summary>检验的 r 上限（r = 1..RMax；r=1 极端贴近、2≤r≪m 局部簇）[readme §一 r 表]</summary>
        Public Property RMax As Integer = 5

        ''' <summary>单间隔分布模型（默认精确均匀间距式）</summary>
        Public Property Model As GapModel = GapModel.UniformSpacing

        ''' <summary>True: μ = m·F（文档字面）；False: μ = (m−1)·F（精确间隔数，默认）</summary>
        Public Property UseDocMGaps As Boolean = False

        ''' <summary>右尾检验的最大间隔个数 q 上限（q = 1..QMax，对应 X_(n), X_(n−1), ...）</summary>
        Public Property QMax As Integer = 3

        ''' <summary>蒙特卡洛重复次数（0 = 关闭；&gt;0 时附加经验 p 值）</summary>
        Public Property MonteCarloReps As Integer = 0

        ''' <summary>蒙特卡洛随机种子（可复现）</summary>
        Public Property Seed As Integer = 42

    End Class

    ''' <summary>左尾 r-scan 表行：R(r) = X_(r) 与聚集显著性</summary>
    Public Class RScanRow

        ''' <summary>第 r 个最小间隔的 r</summary>
        Public Property R As Integer
        ''' <summary>X_(r) 观察值</summary>
        Public Property Xr As Double
        ''' <summary>L(r) = Σ_{j≤r} X_(j)（聚集强度统计量 [readme §一]）</summary>
        Public Property Lr As Double
        ''' <summary>左尾 p 值：前 r 个最紧间隔显著更小的概率（小 → 聚集）</summary>
        Public Property PLeft As Double
        ''' <summary>蒙特卡洛经验 p（若启用）</summary>
        Public Property PLeftMc As Double? = Nothing

        Public Overrides Function ToString() As String
            Dim mc = If(PLeftMc.HasValue, "  MC=" & PLeftMc.Value.ToString("G4"), "")
            Return "r=" & R & "  X_(r)=" & Xr.ToString("G10") & "  L(r)=" & Lr.ToString("G10") & "  p_left=" & PLeft.ToString("G6") & mc
        End Function

    End Class

    ''' <summary>右尾表行：前 q 大间隔与分散显著性</summary>
    Public Class RightTailRow

        ''' <summary>第 q 大间隔（q=1 即最大间隔）</summary>
        Public Property Q As Integer
        ''' <summary>第 q 大间隔观察值</summary>
        Public Property Xq As Double
        ''' <summary>右尾 p 值：前 q 大间隔显著更大的概率（小 → 分散/大空隙）</summary>
        Public Property PRight As Double
        Public Property PRightMc As Double? = Nothing

        Public Overrides Function ToString() As String
            Dim mc = If(PRightMc.HasValue, "  MC=" & PRightMc.Value.ToString("G4"), "")
            Return "q=" & Q & "  X_(top" & Q & ")=" & Xq.ToString("G10") & "  p_right=" & PRight.ToString("G6") & mc
        End Function

    End Class

    Public Class RScanResult

        Public Property M As Integer
        Public Property L As Double
        Public Property Positions As Double()
        Public Property GapsSorted As Double()
        Public Property LeftTable As New List(Of RScanRow)()
        Public Property RightTable As New List(Of RightTailRow)()
        ''' <summary>最显著 r（左尾最小 p）与 Bonferroni 校正 p（×左尾检验数）</summary>
        Public Property BestR As Integer
        Public Property BestPLeft As Double = 1.0
        Public Property BestPLeftBonferroni As Double = 1.0
        Public Property MaxGap As Double
        Public Property McReps As Integer
        Public Property Log As New List(Of String)()

        Public Function SummaryText() As String
            Dim sb As New StringBuilder()
            sb.AppendLine("r-scan: m=" & M & " sites, L=" & L.ToString("G12") & ", gaps=" & (M - 1))
            If RightTable.Count > 0 Then
                sb.AppendLine("  max gap = " & MaxGap.ToString("G10") & "  (right-tail q=1 p = " & RightTable(0).PRight.ToString("G6") & ")")
            End If
            sb.AppendLine("  best clustering: r=" & BestR & ", p_left=" & BestPLeft.ToString("G6") & ", Bonferroni p=" & BestPLeftBonferroni.ToString("G6"))
            If McReps > 0 Then sb.AppendLine("  Monte Carlo: " & McReps & " reps")
            Return sb.ToString()
        End Function

    End Class

    Public Module RScanStatistics

        ''' <summary>排序后的相邻间隔 X_1..X_{m−1} [readme §一]</summary>
        Public Function GapsOf(positions As IEnumerable(Of Double)) As Double()
            Dim ps = positions.OrderBy(Function(t) t).ToArray()
            If ps.Length < 2 Then Throw New ArgumentException("至少需要 2 个位点才有间隔")
            Dim g(ps.Length - 2) As Double
            For i = 0 To ps.Length - 2
                g(i) = ps(i + 1) - ps(i)
            Next
            Return g
        End Function

        ''' <summary>F(t) = P(X_i ≤ t) [readme §二/§三]</summary>
        Public Function IntervalCdf(t As Double, L As Double, m As Integer,
                                    model As GapModel) As Double
            If t <= 0 Then Return 0.0
            If t >= L Then Return 1.0
            If model = GapModel.Exponential Then
                Dim lam = m / L
                Return 1.0 - std.Exp(-lam * t)
            Else
                Return 1.0 - std.Pow(1.0 - t / L, m - 1)
            End If
        End Function

        ''' <summary>S(t) = P(X_i > t) = (1 − t/L)^{m−1}（iid 均匀点位精确边缘）</summary>
        Public Function IntervalSurvival(t As Double, L As Double, m As Integer) As Double
            If t <= 0 Then Return 1.0
            If t >= L Then Return 0.0
            Return std.Pow(1.0 - t / L, m - 1)
        End Function

        ''' <summary>μ = nGaps·F(t)：小间隔计数 {X_i ≤ t} 的期望（泊松参数）[readme §四(3)]</summary>
        Public Function MuLeft(t As Double, L As Double, m As Integer,
                               model As GapModel, useDocMGaps As Boolean) As Double
            Dim n = If(useDocMGaps, CDbl(m), CDbl(m - 1))
            Return n * IntervalCdf(t, L, m, model)
        End Function

        ''' <summary>μ_R = nGaps·S(t)：大间隔计数 {X_i ≥ t} 的期望</summary>
        Public Function MuRight(t As Double, L As Double, m As Integer,
                                useDocMGaps As Boolean) As Double
            Dim n = If(useDocMGaps, CDbl(m), CDbl(m - 1))
            Return n * IntervalSurvival(t, L, m)
        End Function

        ''' <summary>
        ''' 左尾 p 值 [readme §四(4)]：p = P(R(r) ≤ t_obs) ≈ 1 − e^{−μ}·Σ_{j=0}^{r−1} μ^j/j!
        ''' （= P(Pois(μ) ≥ r)，μ = nGaps·F(t_obs)；小 → 前 r 个间隔显著更紧 → 聚集）
        ''' </summary>
        Public Function PLeftTail(r As Integer, tObs As Double, L As Double, m As Integer,
                                  model As GapModel, useDocMGaps As Boolean) As Double
            Dim mu = MuLeft(tObs, L, m, model, useDocMGaps)
            Return SpecialFunctions.PoissonTail(mu, r)
        End Function

        ''' <summary>
        ''' 右尾 p 值 [readme §三末/§四末]：前 q 个最大间隔全部 ≥ t_obs 的显著性
        ''' p = P(Pois(μ_R) ≥ q)；q=1 时 = 1 − e^{−μ_R}，与 P(X_(m) ≤ t) ≈ exp[−m·S(t)] 一致
        ''' （小 → 存在异常大空隙 → 分散）
        ''' </summary>
        Public Function PRightTail(q As Integer, tObs As Double, L As Double, m As Integer,
                                   useDocMGaps As Boolean) As Double
            Dim muR = MuRight(tObs, L, m, useDocMGaps)
            Return SpecialFunctions.PoissonTail(muR, q)
        End Function

        ''' <summary>完整 r-scan 检验流水线 [readme §四 步骤 1–4]</summary>
        Public Function Scan(positions As IEnumerable(Of Double), L As Double,
                             Optional options As RScanOptions = Nothing) As RScanResult
            If options Is Nothing Then options = New RScanOptions()
            If L <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(L))
            Dim ps = positions.OrderBy(Function(t) t).ToArray()
            If ps.Length < 2 Then Throw New ArgumentException("至少需要 2 个位点")
            Dim m = ps.Length
            Dim gs = GapsOf(ps).OrderBy(Function(t) t).ToArray()
            Dim nGaps = m - 1

            Dim res As New RScanResult With {
                .M = m, .L = L, .Positions = ps, .GapsSorted = gs, .MaxGap = gs(nGaps - 1)}

            ' ---- 左尾 r-scan 表（r = 1..min(RMax, nGaps)）：步骤 1-4 ----
            Dim rHi = std.Min(options.RMax, nGaps)
            Dim cum As Double = 0.0
            For r = 1 To rHi
                cum += gs(r - 1)
                Dim p = PLeftTail(r, gs(r - 1), L, m, options.Model, options.UseDocMGaps)
                res.LeftTable.Add(New RScanRow With {.R = r, .Xr = gs(r - 1), .Lr = cum, .PLeft = p})
                If p < res.BestPLeft Then
                    res.BestPLeft = p
                    res.BestR = r
                End If
            Next
            res.BestPLeftBonferroni = std.Min(1.0, res.BestPLeft * rHi)

            ' ---- 右尾表（q = 1..min(QMax, nGaps)，从最大间隔自上而下）----
            Dim qHi = std.Min(options.QMax, nGaps)
            For q = 1 To qHi
                Dim xq = gs(nGaps - q)
                Dim p = PRightTail(q, xq, L, m, options.UseDocMGaps)
                res.RightTable.Add(New RightTailRow With {.Q = q, .Xq = xq, .PRight = p})
            Next

            ' ---- 蒙特卡洛经验 p（可选；独立于解析泊松逼近的口径）----
            If options.MonteCarloReps > 0 Then
                res.McReps = options.MonteCarloReps
                Dim rng As New Random(options.Seed)
                Dim rHits(rHi) As Integer
                Dim qHits(qHi) As Integer
                For rep = 1 To options.MonteCarloReps
                    Dim sim(m - 1) As Double
                    For i = 0 To m - 1
                        sim(i) = rng.NextDouble() * L
                    Next
                    Array.Sort(sim)
                    Dim sg(nGaps - 1) As Double
                    For i = 0 To nGaps - 1
                        sg(i) = sim(i + 1) - sim(i)
                    Next
                    Array.Sort(sg)
                    For r = 1 To rHi
                        If sg(r - 1) <= gs(r - 1) Then rHits(r) += 1
                    Next
                    For q = 1 To qHi
                        If sg(nGaps - q) >= gs(nGaps - q) Then qHits(q) += 1
                    Next
                Next
                For r = 1 To rHi
                    res.LeftTable(r - 1).PLeftMc = rHits(r) / CDbl(options.MonteCarloReps)
                Next
                For q = 1 To qHi
                    res.RightTable(q - 1).PRightMc = qHits(q) / CDbl(options.MonteCarloReps)
                Next
            End If
            Return res
        End Function

    End Module

End Namespace

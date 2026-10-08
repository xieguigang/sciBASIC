#Region "Microsoft.VisualBasic::LuUpdate.vb, Data_science\Mathematica\Math\Math\Algebra\LP\IPMCrossover\LuUpdate.vb"

' Copyright (c) 2018 GPL3 Licensed
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

#End Region

' ============================================================================
' LuUpdate.vb — 基矩阵 LU 的产品形式（η）增量更新
' ----------------------------------------------------------------------------
' 动机：单纯形每轮迭代都要解 B·x = b 与 Bᵀ·y = c_B。既有实现（BoundedSimplex.Refresh
' 与 Simplex.LoopPhase1/2）在**每一次迭代**都重新装配基矩阵并做一次完整 LU 分解，
' 即 O(m³)/迭代；而一次迭代真正改变的只是基的一列，本可用 O(m²) 的秩 1 修正表达。
'
' 数学形式（产品形式更新 / Product-Form Update）：
'   换基（第 p 列被换为 a_q）时
'       B' = B + (a_q − B·e_p)·e_pᵀ = B·(I + w·e_pᵀ)，  w = B⁻¹a_q − e_p
'   由 Sherman–Morrison
'       (I + w·e_pᵀ)⁻¹ = I − w·e_pᵀ/(1 + w_p)
'   记 1 + w_p = z_p（z = B⁻¹a_q），于是求解只需在**基 LU 的一次三角回代**之上
'   叠加 O(m) 修正：
'       正解 B'x = b   : z = B⁻¹b;        x = z − w·(z_p / (1 + w_p))
'       转置 B'ᵀx = b  : v = b;  v_p −= (w·v)/(1 + w_p);  x = B⁻ᵀv
'   连续 k 次换基后 B_k = B₀·Π_{j=1..k}(I + w_j·e_{p_j}ᵀ)，故
'       B_k⁻¹  = T_1 … T_k·B₀⁻¹   （T_j = I − w_j e_{p_j}ᵀ/(1+w_{j,p_j})，按 j 升序施加）
'       B_k⁻ᵀ  = B₀⁻ᵀ·T_kᵀ … T_1ᵀ （按 j 降序施加）
'
' 为什么选 PFU 而不是 Forrest–Tomlin 原地 U 更新：
'   PFU 能 100% 复用既有 LinAlg.LuSolve / LuSolveT（两者均为纯函数：只读 fac、
'   不改入参、可返回 Nothing），语义零改动、正确性极易对拍；代价是单次求解随 η
'   条数 k 线性增长 O(m² + k·m)，由 MaxUpdates 封顶（超上限即强制重构）。
'   FT 原地 U 更新性能上限更高（O(m²) 更新 + 恒定 O(m²) 求解），但需处理 spike
'   与列重排序，归入第 ② 期（稀疏 η + Markowitz 阈值主元）一并做。
'
' 线程安全约定（重要）：
'   Solve / SolveT 是**纯读**操作，只读基分解与 η 序列，可被多个线程并发调用
'   （GomoryCut.Generate 对候选割行并行调用 LuSolveT 依赖此性质）。
'   只有 ApplyUpdate / SetBase / Refactor 会写入，**调用方必须保证它们不与
'   Solve / SolveT 并发**。实现上 η 条数先于数组引用读取，即便发生增长也不会越界。
'
' Copyright (c) 2018 GPL3 Licensed — sciBASIC.NET Foundation
' ============================================================================

Imports std = System.Math

Namespace LinearAlgebra.LinearProgramming.IPMCrossover

    ''' <summary>
    ''' LU 增量更新的控制参数与稳定性阈值。
    ''' </summary>
    Public Class LuUpdateOptions

        ''' <summary>是否启用增量更新；False 时调用方应每次完整重构（等价于优化前行为）。</summary>
        Public Property Enabled As Boolean = True

        ''' <summary>
        ''' 两轮完整重构之间允许的最大 η 条数。达到上限即标记 <c>NeedsRefactor</c>。
        ''' 该值同时决定了「周期性强制重构」的节奏，是防止数值漂移累积的主闸门。
        ''' </summary>
        Public Property MaxUpdates As Integer = 60

        ''' <summary>主元分母 |1 + w_p| = |z_p| 的下限；低于该值视为数值不可靠并拒绝本次更新。</summary>
        Public Property PivotTolerance As Double = 0.000000000001

        ''' <summary>η 向量无穷范数上限；超过该值说明基条件数恶化，拒绝本次更新。</summary>
        Public Property MaxEtaNorm As Double = 1000000.0

        ''' <summary>复制一份参数。</summary>
        Public Function Clone() As LuUpdateOptions
            Return New LuUpdateOptions With {
                .Enabled = Enabled,
                .MaxUpdates = MaxUpdates,
                .PivotTolerance = PivotTolerance,
                .MaxEtaNorm = MaxEtaNorm
            }
        End Function

    End Class

    ''' <summary>
    ''' 基矩阵的产品形式（η）更新器：持有一个完整的 <see cref="LuFactorization"/>
    ''' 与若干 Sherman–Morrison 秩 1 修正，把「每次换基 O(m³) 重构」降为
    ''' 「换基 O(m²) 更新 + 每 K 次一次 O(m³) 重构」。
    ''' </summary>
    ''' <remarks>
    ''' <b>不变量</b>：<see cref="Solve"/> / <see cref="SolveT"/> 的结果恒等于
    ''' 「当前基矩阵完整 LU 分解后 <see cref="LinAlg.LuSolve"/> / <see cref="LinAlg.LuSolveT"/>」
    ''' 的结果（浮点舍入路径不同，但数学等价，误差由稳定性闸门与周期性重构封顶）。
    ''' </remarks>
    Public NotInheritable Class LuUpdater

        ' ---- 配置 ----
        Private ReadOnly _opts As LuUpdateOptions

        ' ---- 基分解（唯一的全量因子）----
        Private _base As LuFactorization

        ' ---- η 序列：位置 p_j / 向量 w_j / 分母 (1 + w_{j,p_j}) ----
        Private _etaP() As Integer
        Private _etaW()() As Double
        Private _etaD() As Double
        Private _etaCount As Integer
        Private _etaCap As Integer

        ' ---- 统计 ----
        Private _refactors As Integer = 0
        Private _updates As Integer = 0
        Private _gateRejects As Integer = 0

        ''' <summary>
        ''' 最近一次 <see cref="ApplyUpdate"/> 是否被闸门拒绝。
        ''' 拒绝意味着 η 序列与"已被调用方换掉的基"脱节（基已变但修正未记入），
        ''' 此时必须重构 —— 由 <see cref="NeedsRefactor"/> 反映。
        ''' </summary>
        Private _lastRejected As Boolean = False

        ''' <summary>η 序列增长前的初始容量。</summary>
        Private Const InitialEtaCapacity As Integer = 16

        Public Sub New(Optional options As LuUpdateOptions = Nothing)
            _opts = If(options, New LuUpdateOptions())
            _etaCap = InitialEtaCapacity
            _etaP = New Integer(_etaCap - 1) {}
            _etaW = New Double(_etaCap - 1)() {}
            _etaD = New Double(_etaCap - 1) {}
            _etaCount = 0
        End Sub

        ''' <summary>当前生效的更新参数（只读引用；构造后不应再改动字段）。</summary>
        Public ReadOnly Property Options As LuUpdateOptions
            Get
                Return _opts
            End Get
        End Property

        ''' <summary>当前的基分解；尚未装载时为 Nothing。</summary>
        Public ReadOnly Property BaseFactorization As LuFactorization
            Get
                Return _base
            End Get
        End Property

        ' ================================================================
        ' 生命周期：装载 / 重构
        ' ================================================================

        ''' <summary>
        ''' 装载一次完整分解并清空 η 序列（唯一的全量因子入口）。
        ''' </summary>
        ''' <param name="base">基矩阵的完整 LU 分解；Nothing 表示分解失败（奇异）。</param>
        ''' <returns>装载成功为 True。</returns>
        Public Function SetBase(base As LuFactorization) As Boolean
            _refactors += 1

            If base Is Nothing Then Return False

            _base = base
            _etaCount = 0
            _lastRejected = False

            Return True
        End Function

        ''' <summary>
        ''' 作废当前的基分解与 η 序列（不改动统计计数）。
        ''' </summary>
        ''' <remarks>
        ''' 用于「基与工作矩阵已不再对应」的场景：MILP 并行 worker 经
        ''' <c>BoundedSimplex.SetBounds</c> 替换界数组后热启动基来自别的节点、
        ''' 割平面追加行列导致基结构变化等。
        ''' 此后 <see cref="NeedsRefactor"/> 恒为 True，下一次求解前必然完整重构。
        ''' </remarks>
        Public Sub Invalidate()
            _base = Nothing
            _etaCount = 0
            _lastRejected = False
        End Sub

        ''' <summary>
        ''' 完整重构：对给定方阵做部分主元 LU 分解并清空 η 序列。
        ''' </summary>
        ''' <returns>分解成功为 True（奇异返回 False 且不改变既有状态）。</returns>
        Public Function Refactor(A As Double(,)) As Boolean
            If A Is Nothing Then Return False

            Dim fac As LuFactorization = Nothing

            Try
                fac = LinAlg.LuFactor(A)
            Catch ex As Exception
                ' 非方阵等契约外输入：按分解失败处理，不向上抛
                fac = Nothing
            End Try

            Return SetBase(fac)
        End Function

        ' ================================================================
        ' 唯一的写入路径：换基
        ' ================================================================

        ''' <summary>
        ''' 应用一次换基：基矩阵第 <paramref name="leavePos"/> 列被替换为
        ''' <paramref name="enteringCol"/>。复杂度 O(m² + k·m)（一次正解求 w）。
        ''' </summary>
        ''' <param name="leavePos">离开列在基中的位置（0-based）。</param>
        ''' <param name="enteringCol">进入列的完整列向量（长度 = m）。</param>
        ''' <param name="preSolved">
        ''' 可选：调用方已经算好的 <c>B⁻¹·enteringCol</c>（例如单纯形方向 α）。
        ''' 传入可省去一次 O(m²) 正解；为 Nothing 时内部现算。
        ''' </param>
        ''' <returns>
        ''' True 表示已追加 η；False 表示被稳定性闸门拒绝或尚无基分解 —— 
        ''' 此时 <see cref="NeedsRefactor"/> 为 True，调用方应完整重构。
        ''' </returns>
        Public Function ApplyUpdate(leavePos As Integer, enteringCol As Double(),
                                    Optional preSolved As Double() = Nothing) As Boolean

            If _base Is Nothing OrElse enteringCol Is Nothing Then
                Return False
            End If

            ' ---- 闸门 0：未启用 → 一律要求重构（等价于优化前的每迭代全量分解）----
            If Not _opts.Enabled Then
                _gateRejects += 1
                _lastRejected = True
                Return False
            End If

            ' ---- 闸门 1：η 条数上限（同时充当周期性强制重构）----
            If _etaCount >= _opts.MaxUpdates Then
                _gateRejects += 1
                _lastRejected = True
                Return False
            End If

            Dim m As Integer = _base.n

            If leavePos < 0 OrElse leavePos >= m OrElse enteringCol.Length < m Then
                Return False
            End If

            ' ---- z = B⁻¹a_q（当前基，含已累积的 η）----
            Dim z As Double() = If(preSolved, Solve(enteringCol))

            If z Is Nothing OrElse z.Length < m Then
                _lastRejected = True
                Return False
            End If

            ' ---- w = z − e_p；分母 1 + w_p = z_p ----
            Dim denom As Double = z(leavePos)

            ' 闸门 2：主元分母下限（|1 + w_p| 过小 → 换基本身数值不可靠）
            If std.Abs(denom) < _opts.PivotTolerance Then
                _gateRejects += 1
                _lastRejected = True
                Return False
            End If

            Dim w(m - 1) As Double

            Array.Copy(z, w, m)
            w(leavePos) = z(leavePos) - 1.0

            ' 闸门 3：η 范数上限（基条件数恶化的代理指标）
            Dim norm As Double = 0.0

            For i As Integer = 0 To m - 1
                Dim a As Double = std.Abs(w(i))

                If a > norm Then norm = a
            Next

            If norm > _opts.MaxEtaNorm Then
                _gateRejects += 1
                _lastRejected = True
                Return False
            End If

            ' ---- 追加 η（先写槽位，再递增条数）----
            If _etaCount = _etaCap Then GrowEta()

            _etaP(_etaCount) = leavePos
            _etaW(_etaCount) = w
            _etaD(_etaCount) = denom
            _etaCount += 1
            _updates += 1
            _lastRejected = False

            Return True
        End Function

        Private Sub GrowEta()
            Dim cap As Integer = _etaCap * 2
            Dim p(cap - 1) As Integer
            Dim w(cap - 1)() As Double
            Dim d(cap - 1) As Double

            Array.Copy(_etaP, p, _etaCount)
            Array.Copy(_etaW, w, _etaCount)
            Array.Copy(_etaD, d, _etaCount)

            _etaP = p
            _etaW = w
            _etaD = d
            _etaCap = cap
        End Sub

        ' ================================================================
        ' 求解（纯读，可并发）
        ' ================================================================

        ''' <summary>
        ''' 解 B·x = rhs，等价于「当前基完整分解后 <see cref="LinAlg.LuSolve"/>」。
        ''' 纯读操作，可并发调用。
        ''' </summary>
        Public Function Solve(rhs As Double()) As Double()
            If _base Is Nothing OrElse rhs Is Nothing OrElse rhs.Length < _base.n Then
                Return Nothing
            End If

            Dim z As Double() = LinAlg.LuSolve(_base, rhs)

            If z Is Nothing Then Return Nothing

            ' 先读条数、再取数组引用：即便此刻发生容量增长也不会读到越界槽位
            Dim k As Integer = _etaCount
            Dim pArr As Integer() = _etaP
            Dim wArr As Double()() = _etaW
            Dim dArr As Double() = _etaD
            Dim m As Integer = z.Length

            For t As Integer = 0 To k - 1
                Dim pj As Integer = pArr(t)
                Dim wj As Double() = wArr(t)
                Dim s As Double = z(pj) / dArr(t)

                ' z -= w_j · s（s 已在改写 z(pj) 之前取值）
                If s <> 0.0 Then
                    For i As Integer = 0 To m - 1
                        z(i) -= wj(i) * s
                    Next
                End If
            Next

            Return z
        End Function

        ''' <summary>
        ''' 解 Bᵀ·x = rhs，等价于「当前基完整分解后 <see cref="LinAlg.LuSolveT"/>」。
        ''' 纯读操作，可并发调用（GomoryCut 的并行割行生成依赖此性质）。
        ''' </summary>
        Public Function SolveT(rhs As Double()) As Double()
            If _base Is Nothing OrElse rhs Is Nothing OrElse rhs.Length < _base.n Then
                Return Nothing
            End If

            Dim n As Integer = _base.n
            Dim v(n - 1) As Double

            Array.Copy(rhs, v, n)

            Dim k As Integer = _etaCount
            Dim pArr As Integer() = _etaP
            Dim wArr As Double()() = _etaW
            Dim dArr As Double() = _etaD

            ' B_k⁻ᵀ = B₀⁻ᵀ·T_kᵀ…T_1ᵀ ⇒ 按 j 降序施加，最后再做基的转置回代
            For t As Integer = k - 1 To 0 Step -1
                Dim pj As Integer = pArr(t)
                Dim wj As Double() = wArr(t)
                Dim s As Double = 0.0

                For i As Integer = 0 To n - 1
                    s += wj(i) * v(i)
                Next

                v(pj) -= s / dArr(t)
            Next

            Return LinAlg.LuSolveT(_base, v)
        End Function

        ' ================================================================
        ' 状态与统计
        ' ================================================================

        ''' <summary>是否存在待应用的换基（η 条数 &gt; 0）；为 0 时基分解即精确分解。</summary>
        Public ReadOnly Property PendingUpdates As Integer
            Get
                Return _etaCount
            End Get
        End Property

        ''' <summary>
        ''' 是否应完整重构。满足以下任一即为 True：尚未装载基分解、
        ''' 上一次 <see cref="ApplyUpdate"/> 被闸门拒绝、或 η 条数已达上限。
        ''' </summary>
        Public ReadOnly Property NeedsRefactor As Boolean
            Get
                If _base Is Nothing Then Return True
                If Not _opts.Enabled Then Return True
                If _lastRejected Then Return True
                Return _etaCount >= _opts.MaxUpdates
            End Get
        End Property

        Public ReadOnly Property RefactorCount As Integer
            Get
                Return _refactors
            End Get
        End Property

        Public ReadOnly Property UpdateCount As Integer
            Get
                Return _updates
            End Get
        End Property

        Public ReadOnly Property GateRejectCount As Integer
            Get
                Return _gateRejects
            End Get
        End Property

        ''' <summary>把三项统计归零（跨问题 / 跨节点复用实例时用）。</summary>
        Public Sub ResetStatistics()
            _refactors = 0
            _updates = 0
            _gateRejects = 0
        End Sub

        ' ================================================================
        ' 诊断（非热路径，供自检与 Verbose 使用）
        ' ================================================================

        ''' <summary>
        ''' 残差抽查：计算 ‖B·x − rhs‖∞，其中 x = <see cref="Solve"/>(rhs)。
        ''' </summary>
        ''' <param name="rows">当前基矩阵（jagged 行主序，每行长度 = m）。</param>
        ''' <param name="rhs">右端项。</param>
        ''' <returns>残差无穷范数；求解失败返回 <see cref="Double.NaN"/>。</returns>
        ''' <remarks>
        ''' O(m²)，<b>不在热路径调用</b>（每次迭代抽查会抵消增量更新的收益）。
        ''' 该项作为第四道闸门由自检（T14）与诊断入口驱动：把「每次全量重构的结果」
        ''' 与「η 更新后的结果」对拍，残差 ≤ 1e-9 即通过。
        ''' </remarks>
        Public Function ResidualNorm(rows As Double()(), rhs As Double()) As Double
            Dim x As Double() = Solve(rhs)

            If x Is Nothing OrElse rows Is Nothing Then Return Double.NaN

            Dim m As Integer = rows.Length

            If m <> x.Length Then Return Double.NaN

            Dim worst As Double = 0.0

            For i As Integer = 0 To m - 1
                Dim row As Double() = rows(i)

                If row Is Nothing OrElse row.Length <> m Then Return Double.NaN

                Dim s As Double = 0.0

                For j As Integer = 0 To m - 1
                    s += row(j) * x(j)
                Next

                Dim d As Double = std.Abs(s - rhs(i))

                If d > worst Then worst = d
            Next

            Return worst
        End Function

        Public Overrides Function ToString() As String
            Return $"LuUpdater(base={If(_base Is Nothing, "n/a", _base.n & "x" & _base.n)}, " &
                   $"eta={_etaCount}/{_opts.MaxUpdates}, 重构 {_refactors}, 更新 {_updates}, 闸门拒绝 {_gateRejects})"
        End Function

    End Class

End Namespace

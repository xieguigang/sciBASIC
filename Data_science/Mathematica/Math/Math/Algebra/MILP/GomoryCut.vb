#Region "Microsoft.VisualBasic::d1f83bb6c5a69d1d9e8a1ab8bdf0c9fd, Data_science\Mathematica\Math\Math\Algebra\MILP\GomoryCut.vb"

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

'   Total Lines: 208
'    Code Lines: 112 (53.85%)
' Comment Lines: 46 (22.12%)
'    - Xml Docs: 39.13%
' 
'   Blank Lines: 50 (24.04%)
'     File Size: 8.36 KB


'     Class CutRow
' 
'         Properties: Coefficients, Op, Rhs, SourceColumn, Violation
' 
'     Module GomoryCut
' 
'         Function: Generate, GmiDelta
' 
' 
' /********************************************************************************/

#End Region

' ============================================================================
' GomoryCut.vb — Gomory 混合整数割（GMI）生成
' ----------------------------------------------------------------------------
' 推导（工作形式 min cᵀx, Ax=b, l ≤ x ≤ u 的某个最优基）：
'   对基本整数变量 x_Bi，tableau 行为
'       x_Bi + Σ_{j∈N} ā_ij x_j = b̄_i,      ā_ij = (B⁻¹A)_ij
'   非基本变量在界上：j 在下界记 y_j = x_j − l_j ≥ 0；在丄界记 y_j = u_j − x_j ≥ 0。
'   代入并令 a_j = ā_ij（下界）/ −ā_ij（上界），f0 = frac(x_Bi) ∈ (0,1)，得
'       x_Bi + Σ_j a_j y_j = f̄,   x_Bi ∈ ℤ,  y_j ≥ 0
'   GMI 割：
'       Σ_j δ_j y_j ≥ f0,
'       δ_j = frac(a_j)                 (整数变量, frac ≤ f0)
'       δ_j = f0·(1−frac(a_j))/(1−f0)   (整数变量, frac > f0)
'       δ_j = a_j                       (连续变量, a_j ≥ 0)
'       δ_j = −f0·a_j/(1−f0)            (连续变量, a_j < 0)
'   由 δ_j ≥ 0 与 y_j ≥ 0 知该不等式有效且割去当前分数顶点（违反量 ≈ f0）。
'
' 换回工作变量空间（Σ γ_k x_k ≥ rhs）：
'   γ_j = +δ_j, rhs += δ_j·l_j（下界侧）；γ_j = −δ_j, rhs −= δ_j·u_j（上界侧）。
'
' 数值处理：割系数按 max|γ| 归一；超过 CutCoefficientLimit 或密度上限的割丢弃；
' 仅保留对当前 LP 最优解违反量 > 容差的割，并按违反量降序取前 maxCuts 条。
'
' Copyright (c) 2018 GPL3 Licensed — sciBASIC.NET Foundation
' ============================================================================

Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming.IPMCrossover
Imports std = System.Math

Namespace LinearAlgebra.LinearProgramming.MILP

    ''' <summary>一条待追加的割平面（工作变量空间）。</summary>
    Public Class CutRow

        ''' <summary>工作列系数（长度 = MilpLpForm.Cols）</summary>
        Public Property Coefficients As Double()
        ''' <summary>"&lt;=" 或 "&gt;="</summary>
        Public Property Op As String
        Public Property Rhs As Double
        ''' <summary>对生成时 LP 最优解的违反量（&gt; 0 表示确实割去该点）</summary>
        Public Property Violation As Double
        ''' <summary>生成该割的基本整数变量（工作列索引），用于日志</summary>
        Public Property SourceColumn As Integer

    End Class

    ''' <summary>
    ''' Gomory 混合整数割生成器。
    ''' </summary>
    Public Module GomoryCut

        ''' <summary>
        ''' 由当前最优基生成 GMI 割。
        ''' </summary>
        ''' <param name="form">节点 LP 工作形式</param>
        ''' <param name="simplex">刚完成 <c>Solve</c> 的单纯形（提供 LU 分解）</param>
        ''' <param name="result">单纯形结果（基、非基本状态、解）</param>
        ''' <param name="options">求解选项（割的尺度/密度限制）</param>
        ''' <param name="maxCuts">本轮的割数上限</param>
        ''' <param name="log">日志</param>
        Public Function Generate(form As MilpLpForm, simplex As BoundedSimplex, result As BsResult,
                                 options As MilpOptions, maxCuts As Integer,
                                 log As List(Of String)) As List(Of CutRow)

            Dim cuts As New List(Of CutRow)()
            Dim baseFac As LuFactorization = simplex.Updater.BaseFactorization

            If baseFac Is Nothing Then Return cuts
            If result Is Nothing OrElse Not result.IsOptimal Then Return cuts

            Dim m As Integer = form.Rows
            Dim n As Integer = form.Cols
            Dim tol As Double = options.IntegerTolerance
            Dim basic As New HashSet(Of Integer)()

            For k As Integer = 0 To m - 1
                If result.Basis(k) >= 0 Then basic.Add(result.Basis(k))
            Next

            Dim i1 As Double = 1.0 - tol

            ' ---- 预筛选候选基本整数行（小数部分落在 (tol, 1−tol) 内）----
            Dim cand As New List(Of Integer)()
            Dim fracs As New List(Of Double)()

            For k As Integer = 0 To m - 1
                Dim bj As Integer = result.Basis(k)

                If bj < 0 Then Continue For                              ' 人工列（冗余行）
                If Not form.IsIntegerColumn(bj) Then Continue For        ' 仅对基本整数变量生成

                Dim xbi As Double = result.X(bj)
                Dim f0 As Double = xbi - std.Floor(xbi)

                If f0 <= tol OrElse f0 >= i1 Then Continue For           ' 已经整数

                cand.Add(k)
                fracs.Add(f0)
            Next

            If cand.Count = 0 Then Return cuts

            ' ---- AT 列主序缓存：tableau 行 α = Aᵀw 变为行批量点积（连续访存 + SIMD）----
            Dim AT As Double()() = form.TransposeCache()

            ' ---- 逐候选行生成割：行间完全独立（只读共享 fac/form/result）----
            ' 旧版按 k 顺序遇 maxCuts 即停；并行版生成全部候选后按违反量排序取前
            ' maxCuts 条（Top-K），语义更强且与行生成顺序无关。
            Dim generated(cand.Count - 1) As CutRow

            Call MilpKernels.ForParallel(cand.Count, m * n,
                Sub(idx)
                    generated(idx) = GenerateOne(form, simplex, result, basic, AT,
                                                 cand(idx), fracs(idx), options, tol)
                End Sub)

            For Each cut As CutRow In generated
                If cut IsNot Nothing Then cuts.Add(cut)
            Next

            If cuts.Count > 1 Then
                cuts.Sort(Function(p, q) q.Violation.CompareTo(p.Violation))
            End If

            If cuts.Count > maxCuts Then
                cuts.RemoveRange(maxCuts, cuts.Count - maxCuts)
            End If

            If log IsNot Nothing AndAlso cuts.Count > 0 Then
                log.Add($"  割平面: 生成 {cuts.Count} 条 GMI 割（最大违反量 {cuts(0).Violation:G4}）")
            End If

            Return cuts
        End Function

        ''' <summary>
        ''' 由一条候选基本行生成 GMI 割；数值不可靠 / 无违反时返回 Nothing。
        ''' </summary>
        ''' <remarks>
        ''' tableau 行改走 <see cref="BoundedSimplex.SolveBasisT"/>（LU 更新器的纯读
        ''' 接口），以便在基含有 η 修正时也拿到精确的 B⁻ᵀe_k。候选行之间并行，
        ''' 该接口只读内部状态、可并发调用，线程安全语义与原先的
        ''' <c>LinAlg.LuSolveT(fac, …)</c> 完全一致。
        ''' </remarks>
        Private Function GenerateOne(form As MilpLpForm, simplex As BoundedSimplex, result As BsResult,
                                     basic As HashSet(Of Integer), AT As Double()(),
                                     k As Integer, f0 As Double,
                                     options As MilpOptions, tol As Double) As CutRow

            Dim m As Integer = form.Rows
            Dim n As Integer = form.Cols
            Dim bj As Integer = result.Basis(k)

            ' ---- tableau 行：w = B⁻ᵀe_k（BoundedSimplex.SolveBasisT 只读，线程安全）----
            Dim e(m - 1) As Double
            e(k) = 1.0

            Dim w As Double() = simplex.SolveBasisT(e)

            If w Is Nothing Then Return Nothing

            ' α = Aᵀw：一次行批量点积算全行（替代旧的逐列跨步循环）
            Dim aq(n - 1) As Double

            Call MilpKernels.MatVecRows(AT, w, aq)

            Dim gamma(n - 1) As Double
            Dim rhs As Double = f0
            Dim nonZero As Integer = 0

            For j As Integer = 0 To n - 1
                If basic.Contains(j) Then Continue For

                Dim range As Double = form.u(j) - form.l(j)

                If range <= 1.0E-09 Then Continue For

                Dim atUpper As Boolean = result.AtUpper(j)
                Dim ap As Double = If(atUpper, -aq(j), aq(j))
                Dim delta As Double = GmiDelta(ap, form.IsIntegerColumn(j), f0)

                If delta <= 0.0 Then Continue For

                If atUpper Then
                    gamma(j) = -delta
                    rhs -= delta * form.u(j)
                Else
                    gamma(j) = delta
                    rhs += delta * form.l(j)
                End If

                nonZero += 1
            Next

            If nonZero = 0 Then Return Nothing

            ' ---- 尺度与密度校验 ----
            Dim scale As Double = 0.0

            For j As Integer = 0 To n - 1
                scale = std.Max(scale, std.Abs(gamma(j)))
            Next

            If scale <= 1.0E-09 OrElse scale > options.CutCoefficientLimit Then Return Nothing
            If nonZero / CDbl(n) > options.CutDensityLimit * 1.5 Then Return Nothing

            If std.Abs(scale - 1.0) > 1.0E-12 Then
                For j As Integer = 0 To n - 1
                    gamma(j) /= scale
                Next

                rhs /= scale
            End If

            ' ---- 违反量：cut 为 Σ γ x ≥ rhs ----
            Dim lhs As Double = 0.0

            For j As Integer = 0 To n - 1
                If gamma(j) <> 0.0 Then lhs += gamma(j) * result.X(j)
            Next

            Dim violation As Double = rhs - lhs

            If violation <= tol * (1.0 + std.Abs(rhs)) Then Return Nothing

            Return New CutRow With {
                .Coefficients = gamma,
                .Op = ">=",
                .Rhs = rhs,
                .Violation = violation,
                .SourceColumn = bj
            }
        End Function

        ''' <summary>GMI 割系数 δ_j。</summary>
        Friend Function GmiDelta(ap As Double, isInteger As Boolean, f0 As Double) As Double
            If isInteger Then
                Dim fj As Double = ap - std.Floor(ap)

                If fj <= f0 Then Return fj

                Return f0 * (1.0 - fj) / (1.0 - f0)
            Else
                If ap >= 0.0 Then Return ap

                Return -f0 * ap / (1.0 - f0)
            End If
        End Function

    End Module

End Namespace

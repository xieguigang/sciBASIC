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

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports std = System.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming.IPMCrossover

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
            Dim fac As LuFactorization = simplex.Factorization

            If fac Is Nothing Then Return cuts
            If result Is Nothing OrElse Not result.IsOptimal Then Return cuts

            Dim m As Integer = form.Rows
            Dim n As Integer = form.Cols
            Dim tol As Double = options.IntegerTolerance
            Dim basic As New HashSet(Of Integer)()

            For k As Integer = 0 To m - 1
                If result.Basis(k) >= 0 Then basic.Add(result.Basis(k))
            Next

            Dim i1 As Double = 1.0 - tol

            For k As Integer = 0 To m - 1
                If cuts.Count >= maxCuts Then Exit For

                Dim bj As Integer = result.Basis(k)

                If bj < 0 Then Continue For                              ' 人工列（冗余行）
                If Not form.IsIntegerColumn(bj) Then Continue For        ' 仅对基本整数变量生成

                Dim xbi As Double = result.X(bj)
                Dim f0 As Double = xbi - std.Floor(xbi)

                If f0 <= tol OrElse f0 >= i1 Then Continue For           ' 已经整数

                ' ---- tableau 行：w = B⁻ᵀe_k ----
                Dim e(m - 1) As Double
                e(k) = 1.0

                Dim w As Double() = LinAlg.LuSolveT(fac, e)

                If w Is Nothing Then Continue For

                Dim gamma(n - 1) As Double
                Dim rhs As Double = f0
                Dim nonZero As Integer = 0

                For j As Integer = 0 To n - 1
                    If basic.Contains(j) Then Continue For

                    Dim range As Double = form.u(j) - form.l(j)

                    If range <= 1.0E-09 Then Continue For

                    Dim aq As Double = 0.0

                    For i As Integer = 0 To m - 1
                        aq += w(i) * form.A(i, j)
                    Next

                    Dim atUpper As Boolean = result.AtUpper(j)
                    Dim ap As Double = If(atUpper, -aq, aq)
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

                If nonZero = 0 Then Continue For

                ' ---- 尺度与密度校验 ----
                Dim scale As Double = 0.0

                For j As Integer = 0 To n - 1
                    scale = std.Max(scale, std.Abs(gamma(j)))
                Next

                If scale <= 1.0E-09 OrElse scale > options.CutCoefficientLimit Then Continue For
                If nonZero / CDbl(n) > options.CutDensityLimit * 1.5 Then Continue For

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

                If violation <= tol * (1.0 + std.Abs(rhs)) Then Continue For

                cuts.Add(New CutRow With {
                    .Coefficients = gamma,
                    .Op = ">=",
                    .Rhs = rhs,
                    .Violation = violation,
                    .SourceColumn = bj
                })
            Next

            If cuts.Count > 1 Then
                cuts.Sort(Function(p, q) q.Violation.CompareTo(p.Violation))
            End If

            If log IsNot Nothing AndAlso cuts.Count > 0 Then
                log.Add($"  割平面: 生成 {cuts.Count} 条 GMI 割（最大违反量 {cuts(0).Violation:G4}）")
            End If

            Return cuts
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

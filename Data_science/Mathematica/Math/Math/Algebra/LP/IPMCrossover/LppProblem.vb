#Region "Microsoft.VisualBasic::e2e17181313a1b71ac7cf9ac978e288c, Data_science\Mathematica\Math\Math\Algebra\LP\IPMCrossover\LppProblem.vb"

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

    '   Total Lines: 441
    '    Code Lines: 291 (65.99%)
    ' Comment Lines: 85 (19.27%)
    '    - Xml Docs: 38.82%
    ' 
    '   Blank Lines: 65 (14.74%)
    '     File Size: 18.75 KB


    '     Class LppConstraint
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class LppProblem
    ' 
    '         Function: ConstraintTypes, VariableNames
    ' 
    '     Class StandardForm
    ' 
    '         Properties: IsSparse
    ' 
    '         Function: Clone, FromProblem, FromSparse, HasNonZero, MapShadowPrice
    '                   ToDense, ToOriginal
    ' 
    '         Sub: NegateRow
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' LppProblem.vb — LP 输入模型 + 标准形转换
' ----------------------------------------------------------------------------
' 输入（问题空间）：min/max cᵀv，约束 A_i·v {<=,>=,=} b_i，lb ≤ v ≤ ub
'
' 标准形（IPM 的工作对象）[readme §一]：
'   min c̃ᵀx̃，Ãx̃ = b̃，**0 ≤ x̃ ≤ ũ**，x̃ 含松弛列
'   · max → min：c̃ = −c_orig（σ = −1），报告量乘 σ 映回原始方向；
'   · ≤ 行加 +松弛、≥ 行加 −松弛、= 行不加（松弛列 ub = +∞）；
'   · b̃_i < 0 的行整体翻转（flipSign = −1，保证 b̃ ≥ 0）；
'   · **有限下界用平移 v = lb + x 消掉**（b̃ ← b − A·lb，ũ = ub − lb，
'     ObjOffset = cᵀlb），因此支持 lb/ub 不增加任何约束行或列 —— 这是
'     FBA（可逆反应 lb = −1000）能够保持规模的前提。
'
' 两个构造入口：
'   FromProblem  —— 稠密路径（中小规模，LppProblem 字典式输入）
'   FromSparse   —— 稀疏路径（基因组规模，CSR + 整数索引直接构造，
'                   绕开 Array.IndexOf 变量名的 O(nnz·n) 匹配）
'
' 映射回问题空间：v = lb + x；slack_i = b_i − A_i·v；
'                 shadowPrice_i = σ·flipSign_i·y_i = ∂(原始目标)/∂b_i
' ============================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports std = System.Math

Namespace LinearAlgebra.LinearProgramming.IPMCrossover

    Public Class LppConstraint

        ''' <summary>变量名 → 系数（缺省为 0）</summary>
        Public Coefficients As Dictionary(Of String, Double)
        ''' <summary>"&lt;=" / ">=" / "="</summary>
        Public Op As String
        Public Rhs As Double

        Public Sub New(coefficients As Dictionary(Of String, Double), op As String, rhs As Double)
            Me.Coefficients = coefficients
            Me.Op = op
            Me.Rhs = rhs
        End Sub

    End Class

    Public Class LppProblem

        ''' <summary>"min" / "max"</summary>
        Public ObjectiveSense As String = "min"
        Public Variables As New List(Of LppVariable)()
        Public Constraints As New List(Of LppConstraint)()

        Public Function VariableNames() As String()
            Return Variables.Select(Function(v) v.symbol).ToArray()
        End Function

        Public Function ConstraintTypes() As String()
            Return Constraints.Select(Function(k) k.Op).ToArray()
        End Function

    End Class

    ''' <summary>标准形（IPM/单纯形的工作对象）</summary>
    Public Class StandardForm

        ' ---------- 工作空间 ----------
        ''' <summary>稠密工作矩阵（含松弛列、已翻转）；稀疏路径为 Nothing</summary>
        Public A As Double(,)
        ''' <summary>矩阵抽象：IPM 只通过该接口访问 A（稠密或稀疏）</summary>
        Public Mat As ILpMatrix
        ''' <summary>m（已保证 ≥ 0）</summary>
        Public b As Double()
        ''' <summary>n + nSlack（内部 min 方向）</summary>
        Public c As Double()
        ''' <summary>工作变量上界，+∞ 表示无上界</summary>
        Public U As Double()
        Public M As Int32
        Public N As Int32
        Public NSlack As Int32
        Public Sigma As Int32          ' +1: 原为 min；−1: 原为 max（c̃ = σ·c_orig）
        Public FlipSign As Double()    ' 每行翻转符号（±1）
        Public VarNames As String()
        Public ConstraintTypeList As String()

        ' ---------- 原始空间（slack / reduced cost 报告用）----------
        ''' <summary>原始 A（未平移、未翻转、不含松弛列）</summary>
        Public MatOrig As ILpMatrix
        Public BOrig As Double()
        Public COrig As Double()
        ''' <summary>下界平移量：v = LbShift + ColScale∘x</summary>
        Public LbShift As Double()
        ''' <summary>
        ''' 列缩放：x（工作变量）→ 问题空间时乘的因子，默认全 1。
        ''' 稀疏入口用它把变量盒归一化到 [0,1]，避免 Θ = x/s 跨越多达 1e7 的量级。
        ''' </summary>
        Public ColScale As Double()
        ''' <summary>平移引入的目标常数 cᵀlb</summary>
        Public ObjOffset As Double

        ''' <summary>稀疏路径（无稠密矩阵，跳过 crossover / 单纯形收尾）</summary>
        Public ReadOnly Property IsSparse As Boolean
            Get
                Return A Is Nothing
            End Get
        End Property

        ''' <summary>把标准形解映射回问题空间：v = lb + x</summary>
        Public Function ToOriginal(xStd As Double()) As Double()
            Dim v(N - 1) As Double

            For j As Int32 = 0 To N - 1
                v(j) = LbShift(j) + xStd(j)
            Next

            Return v
        End Function

        ''' <summary>影子价映射回原始方向：σ·flipSign_i·y_i = ∂(原始目标)/∂b_i</summary>
        Public Function MapShadowPrice(i As Int32, y As Double()) As Double
            Return Sigma * FlipSign(i) * y(i)
        End Function

        ' ====================================================================
        ' 稠密入口
        ' ====================================================================
        ''' <summary>由 LppProblem 构造标准形（中小规模）</summary>
        Public Shared Function FromProblem(prob As LppProblem) As StandardForm
            Dim sf As New StandardForm()
            sf.N = prob.Variables.Count
            sf.M = prob.Constraints.Count
            sf.Sigma = If(prob.ObjectiveSense.ToLowerInvariant().StartsWith("max"), -1, 1)
            sf.VarNames = prob.VariableNames()
            sf.ConstraintTypeList = prob.ConstraintTypes()
            sf.FlipSign = New Double(sf.M - 1) {}
            sf.NSlack = 0
            Dim slackOfRow(sf.M - 1) As Int32      ' −1 = 无；否则 +1(≤) / −1(≥)
            For i = 0 To sf.M - 1
                Dim op = prob.Constraints(i).Op
                If op = "<=" Then
                    slackOfRow(i) = 1
                    sf.NSlack += 1
                ElseIf op = ">=" Then
                    slackOfRow(i) = -1
                    sf.NSlack += 1
                ElseIf op = "=" Then
                    slackOfRow(i) = -1
                Else
                    Throw New ArgumentException($"未知约束类型: {op}")
                End If
            Next
            ' 先数一遍真正的松弛个数（= 行无松弛）
            Dim totalSlack As Int32 = 0
            For i = 0 To sf.M - 1
                If prob.Constraints(i).Op <> "=" Then totalSlack += 1
            Next
            sf.NSlack = totalSlack
            Dim ncols = sf.N + sf.NSlack
            sf.A = New Double(sf.M - 1, ncols - 1) {}
            sf.b = New Double(sf.M - 1) {}
            sf.c = New Double(ncols - 1) {}
            ' 系数与 RHS
            Dim slackPtr As Int32 = sf.N
            For i = 0 To sf.M - 1
                Dim con = prob.Constraints(i)
                For Each kvp In con.Coefficients
                    Dim j = Array.IndexOf(sf.VarNames, kvp.Key)
                    If j < 0 Then Throw New ArgumentException($"约束引用了未知变量: {kvp.Key}")
                    sf.A(i, j) += kvp.Value
                Next
                sf.b(i) = con.Rhs
                ' 松弛列
                If con.Op = "<=" Then
                    sf.A(i, slackPtr) = 1.0
                    slackPtr += 1
                ElseIf con.Op = ">=" Then
                    sf.A(i, slackPtr) = -1.0
                    slackPtr += 1
                End If
            Next

            ' ---------- 原始空间数据（平移/翻转之前）----------
            sf.BOrig = New Double(sf.M - 1) {}
            For i = 0 To sf.M - 1
                sf.BOrig(i) = prob.Constraints(i).Rhs
            Next
            sf.COrig = New Double(sf.N - 1) {}
            sf.LbShift = New Double(sf.N - 1) {}
            Dim AOrig(sf.M - 1, sf.N - 1) As Double
            For i = 0 To sf.M - 1
                For j = 0 To sf.N - 1
                    AOrig(i, j) = sf.A(i, j)
                Next
            Next
            For j = 0 To sf.N - 1
                sf.COrig(j) = prob.Variables(j).coefficient
                sf.LbShift(j) = prob.Variables(j).LowerBound
            Next

            ' ---------- 下界平移：v = lb + x ----------
            sf.U = New Double(ncols - 1) {}
            For j = 0 To sf.N - 1
                Dim v = prob.Variables(j)
                Dim lb As Double = v.LowerBound
                Dim ub As Double = v.UpperBound

                If ub < lb Then
                    Throw New ArgumentException($"变量 {v.symbol} 的上下界矛盾: [{lb}, {ub}]")
                End If

                sf.U(j) = std.Max(ub - lb, 0.000000001)

                If lb <> 0.0 Then
                    For i = 0 To sf.M - 1
                        sf.b(i) -= sf.A(i, j) * lb
                    Next
                    sf.ObjOffset += v.coefficient * lb
                End If
            Next
            ' 松弛列无上界
            For j = sf.N To ncols - 1
                sf.U(j) = Double.PositiveInfinity
            Next

            ' 行翻转（b ≥ 0）
            For i = 0 To sf.M - 1
                If sf.b(i) < 0 Then
                    sf.FlipSign(i) = -1.0
                    For j = 0 To ncols - 1
                        sf.A(i, j) = -sf.A(i, j)
                    Next
                    sf.b(i) = -sf.b(i)
                Else
                    sf.FlipSign(i) = 1.0
                End If
            Next
            ' 内部目标：σ·c_orig（松弛列成本 0）
            For j = 0 To sf.N - 1
                sf.c(j) = sf.Sigma * prob.Variables(j).coefficient
            Next

            sf.Mat = New DenseLpMatrix(sf.A)
            sf.MatOrig = New DenseLpMatrix(AOrig)

            Return sf
        End Function

        ' ====================================================================
        ' 稀疏入口（基因组规模）
        ' ====================================================================
        ''' <summary>
        ''' 由 CSR 化学计量矩阵直接构造标准形（不做任何变量名匹配、不物化稠密矩阵）
        ''' </summary>
        ''' <param name="csr">约束矩阵（m 行 × n 列）</param>
        ''' <param name="rhs">约束右端项（FBA 的稳态方程右端为 0）</param>
        ''' <param name="obj">目标系数（原始方向）</param>
        ''' <param name="lb">变量下界（Nothing 视为全 0）</param>
        ''' <param name="ub">变量上界（Nothing 视为全 +∞）</param>
        ''' <param name="sense">"min" / "max"</param>
        ''' <remarks>
        ''' 目前只支持全 "=" 约束（FBA 的质量平衡约束即为此形式）；
        ''' 混合 &lt;= / &gt;= 的问题请走 <see cref="FromProblem"/>。
        ''' </remarks>
        Public Shared Function FromSparse(csr As LpSparseMatrix,
                                          rhs As Double(),
                                          obj As Double(),
                                          lb As Double(),
                                          ub As Double(),
                                          varNames As String(),
                                          sense As String,
                                          Optional constraintTypes As String() = Nothing,
                                          Optional forceDense As Boolean = False) As StandardForm
            If csr Is Nothing Then Throw New ArgumentNullException(NameOf(csr))

            ' 注意：局部名不能与 StandardForm 的 M / N 字段同音（VB 标识符大小写不敏感）
            Dim rowsN As Int32 = csr.Rows
            Dim colsN As Int32 = csr.Columns

            If varNames Is Nothing OrElse varNames.Length <> colsN Then
                Throw New ArgumentException("变量名称数量必须与矩阵列数一致")
            End If

            Dim sf As New StandardForm()

            sf.M = rowsN
            sf.N = colsN
            sf.NSlack = 0
            sf.Sigma = If(sense.ToLowerInvariant().StartsWith("max"), -1, 1)
            sf.VarNames = varNames
            sf.ConstraintTypeList = New String(rowsN - 1) {}

            If constraintTypes Is Nothing OrElse constraintTypes.Length <> rowsN Then
                For i = 0 To rowsN - 1
                    sf.ConstraintTypeList(i) = "="
                Next
            Else
                Array.Copy(constraintTypes, sf.ConstraintTypeList, rowsN)
            End If

            sf.FlipSign = New Double(rowsN - 1) {}
            sf.A = Nothing

            For i = 0 To rowsN - 1
                If sf.ConstraintTypeList(i) <> "=" Then
                    Throw New ArgumentException("稀疏入口当前仅支持全 '=' 约束（FBA 形式）")
                End If
            Next

            ' ---------- 原始空间 ----------
            sf.BOrig = New Double(rowsN - 1) {}
            If rhs IsNot Nothing Then Array.Copy(rhs, sf.BOrig, std.Min(rhs.Length, rowsN))
            sf.COrig = New Double(colsN - 1) {}
            If obj IsNot Nothing Then Array.Copy(obj, sf.COrig, std.Min(obj.Length, colsN))
            sf.LbShift = New Double(colsN - 1) {}
            If lb IsNot Nothing Then Array.Copy(lb, sf.LbShift, std.Min(lb.Length, colsN))
            sf.MatOrig = New SparseLpMatrix(csr)

            ' ---------- 工作矩阵（先复制一份，随后就地翻转行）----------
            Dim work As LpSparseMatrix = Clone(csr)

            ' ---------- 下界平移 ----------
            sf.b = New Double(rowsN - 1) {}
            Array.Copy(sf.BOrig, sf.b, rowsN)
            sf.U = New Double(colsN - 1) {}

            For j = 0 To colsN - 1
                Dim lbj As Double = sf.LbShift(j)
                Dim ubj As Double = If(ub Is Nothing OrElse j >= ub.Length, Double.PositiveInfinity, ub(j))

                If ubj < lbj Then
                    Throw New ArgumentException($"变量 {varNames(j)} 的上下界矛盾: [{lbj}, {ubj}]")
                End If

                sf.U(j) = std.Max(ubj - lbj, 0.000000001)

                If lbj <> 0.0 Then
                    sf.ObjOffset += sf.COrig(j) * lbj
                End If
            Next

            ' b ← b − A·lb
            If HasNonZero(sf.LbShift) Then
                Dim Alb As Double() = sf.MatOrig.Mv(sf.LbShift)

                For i = 0 To rowsN - 1
                    sf.b(i) -= Alb(i)
                Next
            End If

            ' ---------- 行翻转（b ≥ 0）----------
            For i = 0 To rowsN - 1
                If sf.b(i) < 0 Then
                    sf.FlipSign(i) = -1.0
                    sf.b(i) = -sf.b(i)
                    NegateRow(work, i)
                Else
                    sf.FlipSign(i) = 1.0
                End If
            Next

            ' ---------- 行均衡 ----------
            ' 化学计量矩阵各行的量级可能相差 2~3 个数量级（本 GEM 的行 ‖A_i‖² ∈ [0.25, 5e3]），
            ' 未做均衡时正规方程 A·Θ·Aᵀ 条件数可达 1e12+，解出的 Δy 量级 1e8~1e11，
            ' fraction-to-boundary 步长退化到 1e-8，内点法完全走不动。
            ' 行缩放 Ã = R·A、b̃ = R·b（R 对角且正定）不改变可行域与最优解，
            ' 只改变收敛性质，且报告量仍用未缩放的 MatOrig/BOrig 计算。
            For i As Int32 = 0 To rowsN - 1
                Dim norm2 As Double = 0.0

                For p As Int32 = work.RowPtr(i) To work.RowPtr(i + 1) - 1
                    Dim a As Double = work.Values(p)

                    norm2 += a * a
                Next

                Dim r As Double = If(norm2 > 0.0, 1.0 / std.Sqrt(norm2), 1.0)

                For p As Int32 = work.RowPtr(i) To work.RowPtr(i + 1) - 1
                    work.Values(p) *= r
                Next

                sf.b(i) *= r
            Next

            ' ---------- 内部目标 ----------
            sf.c = New Double(colsN - 1) {}
            For j = 0 To colsN - 1
                sf.c(j) = sf.Sigma * sf.COrig(j)
            Next

            If forceDense Then
                ' 稠密回退：仅用于中小规模的诊断/对比（基因组规模会 OOM）
                sf.A = ToDense(work)
                sf.Mat = New DenseLpMatrix(sf.A)
            Else
                sf.Mat = New SparseLpMatrix(work)
            End If

            Return sf
        End Function

        Private Shared Function ToDense(csr As LpSparseMatrix) As Double(,)
            Dim A(csr.Rows - 1, csr.Columns - 1) As Double

            For i As Int32 = 0 To csr.Rows - 1
                For p As Int32 = csr.RowPtr(i) To csr.RowPtr(i + 1) - 1
                    A(i, csr.ColIdx(p)) = csr.Values(p)
                Next
            Next

            Return A
        End Function

        Private Shared Function Clone(csr As LpSparseMatrix) As LpSparseMatrix
            Dim rp As Int32() = New Int32(csr.RowPtr.Length - 1) {}
            Dim ci As Int32() = New Int32(std.Max(csr.ColIdx.Length, 1) - 1) {}
            Dim vx As Double() = New Double(std.Max(csr.Values.Length, 1) - 1) {}

            Array.Copy(csr.RowPtr, rp, csr.RowPtr.Length)
            If csr.ColIdx.Length > 0 Then Array.Copy(csr.ColIdx, ci, csr.ColIdx.Length)
            If csr.Values.Length > 0 Then Array.Copy(csr.Values, vx, csr.Values.Length)

            Return New LpSparseMatrix(csr.Rows, csr.Columns, rp, ci, vx)
        End Function

        Private Shared Sub NegateRow(csr As LpSparseMatrix, i As Int32)
            For p As Int32 = csr.RowPtr(i) To csr.RowPtr(i + 1) - 1
                csr.Values(p) = -csr.Values(p)
            Next
        End Sub

        Private Shared Function HasNonZero(v As Double()) As Boolean
            For i As Int32 = 0 To v.Length - 1
                If v(i) <> 0.0 Then Return True
            Next
            Return False
        End Function

    End Class

End Namespace

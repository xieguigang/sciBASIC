#Region "Microsoft.VisualBasic::3bf7e4117bf36695369e3bfa3de998a2, Data_science\Mathematica\Math\Math\Algebra\LP\IPMCrossover\LpMatrix.vb"

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

    '   Total Lines: 549
    '    Code Lines: 377 (68.67%)
    ' Comment Lines: 61 (11.11%)
    '    - Xml Docs: 59.02%
    ' 
    '   Blank Lines: 111 (20.22%)
    '     File Size: 19.11 KB


    '     Interface INormalFactor
    ' 
    '         Properties: Converged, IsExact, Method, NonZeros
    ' 
    '         Function: Mv, Solve
    ' 
    '     Interface ILpMatrix
    ' 
    '         Properties: Columns, NonZeros, Rows
    ' 
    '         Function: Column, FactorNormal, Mtv, Mv, NormalDiag
    ' 
    '     Class DenseLpMatrix
    ' 
    '         Properties: Columns, NonZeros, Rows
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Column, FactorNormal, Mtv, Mv, NormalDiag
    '                   NormalMatrix
    ' 
    '     Class DenseNormalFactor
    ' 
    '         Properties: Converged, IsExact, Method, NonZeros
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Mv, Solve
    ' 
    '     Class SparseLpMatrix
    ' 
    '         Properties: CholRowLimit, Columns, MaxCholNnz, NonZeros, Rows
    '                     UsingIterative
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Column, DisableChol, FactorNormal, Mtv, Mv
    '                   NormalDiag, NormalMatrix
    ' 
    '     Class SparseNormalFactor
    ' 
    '         Properties: Converged, IsExact, Method, NonZeros
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Mv, Solve
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' LpMatrix.vb — 矩阵抽象层：稠密 / 稀疏双实现 [plan step 1]
' ----------------------------------------------------------------------------
' 目的：把"算法"与"存储"解耦，让 InteriorPoint 只依赖 ILpMatrix，
'       避免同一套内点法逻辑写两遍（稠密版 + 稀疏版）。
'
'   ILpMatrix     : Mv(A·x) / Mtv(Aᵀ·y) / Column(j) / FactorNormal(θ, reg)
'   INormalFactor : 正规方程 (A·Θ·Aᵀ + reg·I) 的求解器，含一次迭代精化所需 Mv
'
'   DenseLpMatrix  : 包 Double(,)，复用已验证的 LinAlg.Cholesky（中小规模）
'   SparseLpMatrix : CSR + CSC 副本，正规方程稀疏装配 + 稀疏 LDLᵀ / PCG（基因组规模）
'
' 关键的规模账（GEM: 14777 × 16107, 71949 非零元）：
'   稠密 A      → 1.9 GB   （必然 OOM）
'   稠密 AΘAᵀ   → 1.75 GB  （必然 OOM）
'   CSR A       → ~0.9 MB
'   稀疏 AΘAᵀ   → ~3e5 非零元
' ============================================================================

Imports System
Imports std = System.Math

Namespace LinearAlgebra.LinearProgramming.IPMCrossover

    ''' <summary>
    ''' 正规方程 (A·Θ·Aᵀ + reg·I)·z = rhs 的求解器
    ''' </summary>
    Public Interface INormalFactor

        ''' <summary>求解；失败返回 Nothing</summary>
        Function Solve(rhs As Double()) As Double()

        ''' <summary>(A·Θ·Aᵀ + reg·I)·v，迭代精化与残差校验用</summary>
        Function Mv(v As Double()) As Double()

        ''' <summary>true = 直接法（可做迭代精化）；false = 迭代法（Krylov）</summary>
        ReadOnly Property IsExact As Boolean

        ''' <summary>线性系统是否解到容差内（迭代法收敛失败时上层应提升正则化重试）</summary>
        ReadOnly Property Converged As Boolean

        ReadOnly Property NonZeros As Long

        ReadOnly Property Method As String

    End Interface

    ''' <summary>
    ''' 约束矩阵抽象：内点法只依赖这四个运算，因此稠密/ sparse 可自由切换
    ''' </summary>
    Public Interface ILpMatrix

        ReadOnly Property Rows As Int32
        ReadOnly Property Columns As Int32
        ''' <summary>非零元个数（稠密实现为 m*n）</summary>
        ReadOnly Property NonZeros As Long

        ''' <summary>A·x</summary>
        Function Mv(x As Double()) As Double()

        ''' <summary>Aᵀ·y</summary>
        Function Mtv(y As Double()) As Double()

        ''' <summary>取第 j 列的稠密向量（crossover / 单纯形收尾用）</summary>
        Function Column(j As Int32) As Double()

        ''' <summary>分解 (A·Θ·Aᵀ + reg·I)；失败返回 Nothing</summary>
        Function FactorNormal(theta As Double(), reg As Double) As INormalFactor

        ''' <summary>
        ''' A·Θ·Aᵀ 的对角元（Σ_j A(i,j)²·θ_j），用作正则化量级基准；
        ''' 成本远低于完整装配，可每次迭代调用
        ''' </summary>
        Function NormalDiag(theta As Double()) As Double()

    End Interface

    ''' <summary>稠密实现：包一层 Double(,)，用于中小规模（已验证路径）</summary>
    Public Class DenseLpMatrix
        Implements ILpMatrix

        Private ReadOnly A As Double(,)
        Private ReadOnly m As Int32
        Private ReadOnly n As Int32
        ' 正规矩阵缓存：正则化阶梯重试时 θ 不变，无需重复 O(m²n) 装配
        Private cacheTheta As Double() = Nothing
        Private cacheM As Double(,) = Nothing

        Public Sub New(A As Double(,))
            Me.A = A
            Me.m = A.GetLength(0)
            Me.n = A.GetLength(1)
        End Sub

        Public ReadOnly Property Rows As Int32 Implements ILpMatrix.Rows
            Get
                Return m
            End Get
        End Property

        Public ReadOnly Property Columns As Int32 Implements ILpMatrix.Columns
            Get
                Return n
            End Get
        End Property

        Public ReadOnly Property NonZeros As Long Implements ILpMatrix.NonZeros
            Get
                Return CLng(m) * CLng(n)
            End Get
        End Property

        Public Function Mv(x As Double()) As Double() Implements ILpMatrix.Mv
            Dim y(m - 1) As Double
            For i As Int32 = 0 To m - 1
                Dim s As Double = 0
                For j As Int32 = 0 To n - 1
                    s += A(i, j) * x(j)
                Next
                y(i) = s
            Next
            Return y
        End Function

        Public Function Mtv(y As Double()) As Double() Implements ILpMatrix.Mtv
            Dim x(n - 1) As Double
            For i As Int32 = 0 To m - 1
                Dim yi As Double = y(i)
                If yi = 0.0 Then Continue For
                For j As Int32 = 0 To n - 1
                    x(j) += A(i, j) * yi
                Next
            Next
            Return x
        End Function

        Public Function Column(j As Int32) As Double() Implements ILpMatrix.Column
            Dim v(m - 1) As Double
            For i As Int32 = 0 To m - 1
                v(i) = A(i, j)
            Next
            Return v
        End Function

        Public Function FactorNormal(theta As Double(), reg As Double) As INormalFactor Implements ILpMatrix.FactorNormal
            Dim M0 As Double(,) = NormalMatrix(theta)
            Dim Mreg(m - 1, m - 1) As Double

            For i As Int32 = 0 To m - 1
                For k As Int32 = 0 To m - 1
                    Mreg(i, k) = M0(i, k)
                Next
                Mreg(i, i) += reg
            Next

            Dim L = LinAlg.Cholesky(Mreg)

            If L Is Nothing Then Return Nothing

            Return New DenseNormalFactor(L, Mreg, 0.0)
        End Function

        ''' <summary>A·Θ·Aᵀ（按 θ 引用缓存，正则化阶梯重试时不重算）</summary>
        Private Function NormalMatrix(theta As Double()) As Double(,)
            If cacheM IsNot Nothing AndAlso Object.ReferenceEquals(cacheTheta, theta) Then
                Return cacheM
            End If

            Dim M0(m - 1, m - 1) As Double

            For i As Int32 = 0 To m - 1
                For k As Int32 = i To m - 1
                    Dim s As Double = 0
                    For j As Int32 = 0 To n - 1
                        s += A(i, j) * theta(j) * A(k, j)
                    Next
                    M0(i, k) = s
                    M0(k, i) = s
                Next
            Next

            cacheTheta = theta
            cacheM = M0

            Return M0
        End Function

        Public Function NormalDiag(theta As Double()) As Double() Implements ILpMatrix.NormalDiag
            Dim d(m - 1) As Double

            For i As Int32 = 0 To m - 1
                Dim s As Double = 0
                For j As Int32 = 0 To n - 1
                    Dim aij As Double = A(i, j)
                    s += aij * aij * theta(j)
                Next
                d(i) = s
            Next

            Return d
        End Function

    End Class

    ''' <summary>稠密正规方程因子：Cholesky 两段回代 + 一次迭代精化</summary>
    Public Class DenseNormalFactor
        Implements INormalFactor

        Private ReadOnly L As Double(,)
        Private ReadOnly M As Double(,)
        Private ReadOnly reg As Double
        Private ReadOnly size As Int32

        Public Sub New(L As Double(,), M As Double(,), reg As Double)
            Me.L = L
            Me.M = M
            Me.reg = reg
            Me.size = M.GetLength(0)
        End Sub

        Public ReadOnly Property IsExact As Boolean Implements INormalFactor.IsExact
            Get
                Return True
            End Get
        End Property

        Public ReadOnly Property Converged As Boolean Implements INormalFactor.Converged
            Get
                Return True
            End Get
        End Property

        Public ReadOnly Property NonZeros As Long Implements INormalFactor.NonZeros
            Get
                Return CLng(size) * CLng(size)
            End Get
        End Property

        Public ReadOnly Property Method As String Implements INormalFactor.Method
            Get
                Return "dense-cholesky"
            End Get
        End Property

        Public Function Solve(rhs As Double()) As Double() Implements INormalFactor.Solve
            Return LinAlg.CholSolve(L, rhs)
        End Function

        Public Function Mv(v As Double()) As Double() Implements INormalFactor.Mv
            Dim y(size - 1) As Double
            For i As Int32 = 0 To size - 1
                Dim s As Double = reg * v(i)
                For j As Int32 = 0 To size - 1
                    s += M(i, j) * v(j)
                Next
                y(i) = s
            Next
            Return y
        End Function

    End Class

    ''' <summary>
    ''' 稀疏实现：CSR(行) + CSC(列) 双副本。
    ''' 正规方程 A·Θ·Aᵀ 稀疏装配后优先走稀疏 LDLᵀ，
    ''' fill-in 超阈值或分解失败时自动降级为雅可比预条件 PCG（零 fill-in，无 OOM 风险）。
    ''' </summary>
    Public Class SparseLpMatrix
        Implements ILpMatrix

        Private ReadOnly m As Int32
        Private ReadOnly n As Int32
        ' CSR
        Private ReadOnly rp As Int32()
        Private ReadOnly ci As Int32()
        Private ReadOnly vx As Double()
        ' CSC（A 的转置，按列扫描装配正规方程需要）
        Private ReadOnly cp As Int32()
        Private ReadOnly ri As Int32()
        Private ReadOnly rv As Double()

        Private cholPerm As Int32() = Nothing
        Private cholDisabled As Boolean = False
        ' 正规方程缓存：正则化阶梯重试时 θ 不变，只需换 reg 后重新分解
        Private cacheTheta As Double() = Nothing
        Private cacheM As SymCsr = Nothing

        ''' <summary>稀疏 LDLᵀ 的 fill-in 上限（条目数），超过则永久降级为 PCG</summary>
        Public Shared Property MaxCholNnz As Long = 6000000
        ''' <summary>规模小于该行数时优先尝试稀疏 LDLᵀ</summary>
        Public Shared Property CholRowLimit As Int32 = 40000

        Public Sub New(csr As LpSparseMatrix)
            Me.m = If(csr Is Nothing, 0, csr.Rows)
            Me.n = If(csr Is Nothing, 0, csr.Columns)

            If csr Is Nothing OrElse m <= 0 OrElse n <= 0 Then
                Me.rp = New Int32(0) {}
                Me.ci = New Int32() {}
                Me.vx = New Double() {}
                Me.cp = New Int32(0) {}
                Me.ri = New Int32() {}
                Me.rv = New Double() {}
                Return
            End If

            Me.rp = csr.RowPtr
            Me.ci = csr.ColIdx
            Me.vx = csr.Values

            ' ---- 构造 CSC 副本 ----
            Dim colCount(n - 1) As Int32

            For p As Int32 = 0 To ci.Length - 1
                colCount(ci(p)) += 1
            Next

            Dim cpt(n) As Int32

            For j As Int32 = 0 To n - 1
                cpt(j + 1) = cpt(j) + colCount(j)
            Next

            Dim ridx(ci.Length - 1) As Int32
            Dim rval(ci.Length - 1) As Double
            Dim cursor(n - 1) As Int32

            Array.Copy(cpt, 0, cursor, 0, n)

            For i As Int32 = 0 To m - 1
                For p As Int32 = rp(i) To rp(i + 1) - 1
                    Dim j As Int32 = ci(p)
                    Dim q As Int32 = cursor(j)

                    ridx(q) = i
                    rval(q) = vx(p)
                    cursor(j) = q + 1
                Next
            Next

            Me.cp = cpt
            Me.ri = ridx
            Me.rv = rval
        End Sub

        ''' <summary>直接从 CSR 三分量构造（避免中间对象）</summary>
        Public Sub New(rows As Int32, cols As Int32, rowPtr As Int32(), colIdx As Int32(), vals As Double())
            Me.New(New LpSparseMatrix(rows, cols, rowPtr, colIdx, vals))
        End Sub

        Public ReadOnly Property Rows As Int32 Implements ILpMatrix.Rows
            Get
                Return m
            End Get
        End Property

        Public ReadOnly Property Columns As Int32 Implements ILpMatrix.Columns
            Get
                Return n
            End Get
        End Property

        Public ReadOnly Property NonZeros As Long Implements ILpMatrix.NonZeros
            Get
                Return If(vx Is Nothing, 0, CLng(vx.Length))
            End Get
        End Property

        Public Function Mv(x As Double()) As Double() Implements ILpMatrix.Mv
            Dim y(m - 1) As Double

            For i As Int32 = 0 To m - 1
                Dim s As Double = 0
                For p As Int32 = rp(i) To rp(i + 1) - 1
                    s += vx(p) * x(ci(p))
                Next
                y(i) = s
            Next

            Return y
        End Function

        Public Function Mtv(y As Double()) As Double() Implements ILpMatrix.Mtv
            Dim x(n - 1) As Double

            For j As Int32 = 0 To n - 1
                Dim s As Double = 0
                For p As Int32 = cp(j) To cp(j + 1) - 1
                    s += rv(p) * y(ri(p))
                Next
                x(j) = s
            Next

            Return x
        End Function

        Public Function Column(j As Int32) As Double() Implements ILpMatrix.Column
            Dim v(m - 1) As Double

            For p As Int32 = cp(j) To cp(j + 1) - 1
                v(ri(p)) = rv(p)
            Next

            Return v
        End Function

        Public Function FactorNormal(theta As Double(), reg As Double) As INormalFactor Implements ILpMatrix.FactorNormal
            Dim base0 As SymCsr = NormalMatrix(theta)
            Dim Mx As SymCsr = base0.Clone()

            Mx.AddDiagonal(reg)

            Dim chol As SparseChol.SparseCholFactor = Nothing

            If Not cholDisabled AndAlso m <= CholRowLimit Then
                If cholPerm Is Nothing Then
                    cholPerm = SparseChol.MinDegreeOrder(Mx)
                End If

                chol = SparseChol.Factor(Mx, cholPerm, MaxCholNnz)

                If chol Is Nothing Then
                    ' fill-in 爆炸或非正定：本次迭代与后续迭代都改用 PCG
                    cholDisabled = True
                End If
            End If

            Return New SparseNormalFactor(Mx, chol, AddressOf DisableChol)
        End Function

        ''' <summary>A·Θ·Aᵀ（按 θ 引用缓存）</summary>
        Private Function NormalMatrix(theta As Double()) As SymCsr
            If cacheM IsNot Nothing AndAlso Object.ReferenceEquals(cacheTheta, theta) Then
                Return cacheM
            End If

            cacheM = SparseNormal.Assemble(rp, ci, vx, cp, ri, rv, m, n, theta)
            cacheTheta = theta

            Return cacheM
        End Function

        Public Function NormalDiag(theta As Double()) As Double() Implements ILpMatrix.NormalDiag
            Dim d(m - 1) As Double

            For i As Int32 = 0 To m - 1
                Dim s As Double = 0
                For p As Int32 = rp(i) To rp(i + 1) - 1
                    Dim a As Double = vx(p)
                    s += a * a * theta(ci(p))
                Next
                d(i) = s
            Next

            Return d
        End Function

        ''' <summary>把"改用 PCG"的决定回写到矩阵对象（后续迭代不再尝试 LDLᵀ）</summary>
        Private Function DisableChol() As Boolean
            cholDisabled = True
            Return True
        End Function

        Public ReadOnly Property UsingIterative As Boolean
            Get
                Return cholDisabled
            End Get
        End Property

    End Class

    ''' <summary>
    ''' 稀疏正规方程因子：优先稀疏 LDLᵀ，解出后做一次残差校验；
    ''' 残差不达标（排序/数值问题）则本次改用 PCG 并永久降级。
    ''' </summary>
    Public Class SparseNormalFactor
        Implements INormalFactor

        Private ReadOnly M As SymCsr
        Private ReadOnly chol As SparseChol.SparseCholFactor
        Private ReadOnly iterative As Pcg.PcgSolver
        Private ReadOnly onCholFail As Func(Of Boolean)
        Private cholOk As Boolean

        Public Sub New(M As SymCsr, chol As SparseChol.SparseCholFactor, onCholFail As Func(Of Boolean))
            Me.M = M
            Me.chol = chol
            Me.cholOk = (chol IsNot Nothing)
            Me.iterative = New Pcg.PcgSolver(M)
            Me.onCholFail = onCholFail
        End Sub

        Public ReadOnly Property IsExact As Boolean Implements INormalFactor.IsExact
            Get
                Return cholOk
            End Get
        End Property

        Public ReadOnly Property NonZeros As Long Implements INormalFactor.NonZeros
            Get
                If cholOk Then Return chol.NonZeros
                Return M.NonZeros
            End Get
        End Property

        Public ReadOnly Property Converged As Boolean Implements INormalFactor.Converged
            Get
                If cholOk Then Return True
                Return iterative.Converged
            End Get
        End Property

        Public ReadOnly Property Method As String Implements INormalFactor.Method
            Get
                If cholOk Then Return $"sparse-ldlt(nnz(L)={chol.NonZeros})"
                Return "pcg"
            End Get
        End Property

        Public Function Solve(rhs As Double()) As Double() Implements INormalFactor.Solve
            If cholOk Then
                Dim z As Double() = chol.Solve(rhs)

                If z IsNot Nothing Then
                    ' 残差校验：排序或数值问题会让 LDLᵀ 失真，此时降级 PCG
                    Dim ok As Boolean = Pcg.ResidualOk(M, z, rhs, 0.000001)

                    If ok Then
                        Return z
                    End If

                    cholOk = False
                    If onCholFail IsNot Nothing Then onCholFail()
                Else
                    cholOk = False
                    If onCholFail IsNot Nothing Then onCholFail()
                End If
            End If

            Return iterative.Solve(rhs)
        End Function

        Public Function Mv(v As Double()) As Double() Implements INormalFactor.Mv
            Return M.Mv(v)
        End Function

    End Class

End Namespace


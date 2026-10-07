#Region "Microsoft.VisualBasic::MilpKernels.vb, Data_science\Mathematica\Math\Math\Algebra\MILP\MilpKernels.vb"

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
' MilpKernels.vb — MILP 求解器的 SIMD / 多线程数值内核门面
' ----------------------------------------------------------------------------
' 为什么单独建一层门面：
'   1. 单纯形 / 割平面的行向量长度普遍只有几十 ~ 几千，远低于
'      SimdParallel 的 65536 自动并行门槛，因此"按总工作量 m·n 自适应
'      调度"的多线程决策必须放在本层，而不是依赖通用门面的阈值。
'   2. 割平面 / Refresh / LU 的矩阵以行主序 Double(,) 存储，VB 无法把
'      二维数组的行切片直接交给 Vector API，因此内部统一转为
'      jagged rows（Double()()，每行连续），行级 SIMD 才能落地。
'   3. LU 分解复用既有 LuFactorization 结果类与 LinAlg.LuSolve/LuSolveT，
'      只替换分解内核 —— 行更新是逐元素 AXPY（无归约），结果与标量版
'      LinAlg.LuFactor 逐位一致，主元选择语义完全保持。
'
' SIMD 依赖：Microsoft.VisualBasic.Core/src/Math/SIMD/
'   · SIMDIntrinsics.DotFma / AxpyInPlace（FMA 融合乘加，自动回退）
'   · SimdParallel.Dot（大数据量分块并行归约）
'   · SIMDEnvironment.IsEnabled（全局 SIMD 逃生开关）
'
' Copyright (c) 2018 GPL3 Licensed — sciBASIC.NET Foundation
' ============================================================================

Imports System.Numerics
Imports System.Runtime.Intrinsics
Imports System.Runtime.Intrinsics.X86
Imports System.Threading.Tasks
Imports Microsoft.VisualBasic.Math.SIMD
Imports std = System.Math

Namespace LinearAlgebra.LinearProgramming.MILP

    ''' <summary>
    ''' MILP 数值内核门面：SIMD 快路径 + 标量回退 + 按工作量自适应的多线程调度。
    ''' </summary>
    ''' <remarks>
    ''' 并行开关（<see cref="EnableParallel"/> / <see cref="MaxThreads"/>）由
    ''' <see cref="MilpSolver.Solve"/> 入口根据 <see cref="MilpOptions"/> 设置；
    ''' SIMD 本身始终启用（受 <see cref="SIMDEnvironment.IsEnabled"/> 全局开关控制）。
    ''' </remarks>
    Friend Module MilpKernels

        ''' <summary>是否启用多线程并行（False 时全部内核走单线程 SIMD）。</summary>
        Public Property EnableParallel As Boolean = False

        ''' <summary>并行最大线程数；0 = 自动（CPU 逻辑核心数）。</summary>
        Public Property MaxThreads As Integer = 0

        ''' <summary>
        ''' 触发多线程的最小工作量（元素操作数，约为 m·n）。
        ''' 低于该值时并行调度开销会反噬收益，直接走单线程 SIMD。
        ''' </summary>
        Public Property MinParallelWork As Integer = 65536

        ' ================================================================
        ' 布局转换：Double(,) <=> jagged rows
        ' ================================================================

        ''' <summary>行主序矩阵 → jagged 行（每行独立拷贝，连续内存）。</summary>
        Public Function ToRows(A As Double(,)) As Double()()
            Dim m As Integer = A.GetLength(0)
            Dim n As Integer = A.GetLength(1)
            Dim rows(m - 1) As Double()

            For i As Integer = 0 To m - 1
                Dim row As Double() = New Double(n - 1) {}

                For j As Integer = 0 To n - 1
                    row(j) = A(i, j)
                Next

                rows(i) = row
            Next

            Return rows
        End Function

        ''' <summary>
        ''' 行主序矩阵的转置，以 jagged rows 返回（第 j 行 = 原矩阵第 j 列）。
        ''' 列提取 / 列点积从此变为连续访存。
        ''' </summary>
        Public Function TransposeRows(A As Double(,)) As Double()()
            Dim m As Integer = A.GetLength(0)
            Dim n As Integer = A.GetLength(1)
            Dim rows(n - 1) As Double()

            For j As Integer = 0 To n - 1
                rows(j) = New Double(m - 1) {}
            Next

            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    rows(j)(i) = A(i, j)
                Next
            Next

            Return rows
        End Function

        ''' <summary>jagged rows → 行主序矩阵。</summary>
        Public Function ToMatrix(rows As Double()()) As Double(,)
            Dim m As Integer = rows.Length
            Dim n As Integer = If(m = 0, 0, rows(0).Length)
            Dim A(m - 1, n - 1) As Double

            For i As Integer = 0 To m - 1
                Dim row As Double() = rows(i)

                For j As Integer = 0 To n - 1
                    A(i, j) = row(j)
                Next
            Next

            Return A
        End Function

        ' ================================================================
        ' 向量内核
        ' ================================================================

        ''' <summary>
        ''' 点积：长度足够大时转调 <see cref="SimdParallel.Dot"/>（分块并行归约），
        ''' 否则走 FMA 内核（无 FMA 硬件时自动回退 <see cref="SimdReduce.Dot"/>）。
        ''' </summary>
        Public Function Dot(a As Double(), b As Double()) As Double
            If a.Length >= MinParallelWork Then
                Return SimdParallel.Dot(a, b)
            End If

            Return SIMDIntrinsics.DotFma(a, b)
        End Function

        ''' <summary>就地 AXPY：<c>y(i) += alpha * x(i)</c>（全长度，FMA）。</summary>
        Public Sub AxpyInPlace(y As Double(), alpha As Double, x As Double())
            Call SIMDIntrinsics.AxpyInPlace(alpha, x, y)
        End Sub

        ''' <summary>
        ''' 区段就地 AXPY：<c>y(i) += alpha * x(i)</c>，仅作用于
        ''' <c>[start, ends)</c> 区段（LU 消元的行更新只需要 U 区段）。
        ''' </summary>
        Public Sub AxpyRange(y As Double(), x As Double(), alpha As Double, start As Integer, ends As Integer)
            Dim len As Integer = ends - start

            If len <= 0 Then Return

            If Fma.IsSupported AndAlso SIMDEnvironment.IsEnabled Then
                Dim w As Integer = Vector256(Of Double).Count
                Dim av As Vector256(Of Double) = Vector256.Create(Of Double)(alpha)
                Dim i As Integer = start

                Do While i <= ends - w
                    Dim yv As Vector256(Of Double) = Vector256.LoadUnsafe(Of Double)(y(i))
                    Dim xv As Vector256(Of Double) = Vector256.LoadUnsafe(Of Double)(x(i))

                    Call Vector256.StoreUnsafe(Fma.MultiplyAdd(av, xv, yv), y(i))
                    i += w
                Loop

                Do While i < ends
                    y(i) += alpha * x(i)
                    i += 1
                Loop
            Else
                ' 跨平台回退：System.Numerics.Vector 硬件加/乘（或软件模拟）
                Dim w As Integer = Vector(Of Double).Count
                Dim av As New Vector(Of Double)(alpha)
                Dim i As Integer = start

                Do While i <= ends - w
                    Dim yv As New Vector(Of Double)(y, i)
                    Dim xv As New Vector(Of Double)(x, i)

                    Call (yv + av * xv).CopyTo(y, i)
                    i += w
                Loop

                Do While i < ends
                    y(i) += alpha * x(i)
                    i += 1
                Loop
            End If
        End Sub

        ''' <summary>
        ''' 行批量点积（jagged 矩阵 × 向量）：<c>dst(i) = rows(i) · v</c>。
        ''' tableau 行 α = Aᵀw、约简成本 d = c − Aᵀy 等热点都落在该内核。
        ''' 行间完全独立，工作量达到阈值时按行多线程并行。
        ''' </summary>
        Public Sub MatVecRows(rows As Double()(), v As Double(), dst As Double())
            Dim nRows As Integer = rows.Length

            If nRows = 0 Then Return

            Dim workPerRow As Integer = If(rows(0) Is Nothing, 0, rows(0).Length)

            If ShouldParallelize(nRows, workPerRow) Then
                Dim po As ParallelOptions = MakeOptions()

                Call Parallel.For(0, nRows, po,
                    Sub(i)
                        dst(i) = SIMDIntrinsics.DotFma(rows(i), v)
                    End Sub)
            Else
                For i As Integer = 0 To nRows - 1
                    dst(i) = SIMDIntrinsics.DotFma(rows(i), v)
                Next
            End If
        End Sub

        ''' <summary>
        ''' 行主序 jagged 矩阵-向量乘别名：<c>dst(i) = rows(i) · v</c>
        ''' （供 rhs = b − A·v 一类调用，语义同 <see cref="MatVecRows"/>）。
        ''' </summary>
        Public Sub MatVec(rows As Double()(), v As Double(), dst As Double())
            Call MatVecRows(rows, v, dst)
        End Sub

        ' ================================================================
        ' LU 分解（复用既有 LuFactorization 结果类）
        ' ================================================================

        ''' <summary>
        ''' 部分主元 LU 分解（jagged rows 版本，<b>就地消费</b> rows）。
        ''' </summary>
        ''' <remarks>
        ''' 与 <c>LinAlg.LuFactor</c> 语义完全一致：P·A = L·U，LU 原地合并
        ''' （|L| 对角 = 1），Piv(k) = 位置 k 处的原始行号；主元 &lt; 1e-13 判奇异。
        ''' 行更新 <c>U(i, k+1:) -= lik * U(k, k+1:)</c> 是逐元素 AXPY（无归约），
        ''' 因此结果与标量版<b>逐位一致</b>。剩余消元工作量达到阈值时行级并行。
        ''' </remarks>
        Public Function LuFactorRows(rows As Double()()) As LuFactorization
            Dim n As Integer = rows.Length
            Dim piv(n - 1) As Int32

            For i As Integer = 0 To n - 1
                piv(i) = i
            Next

            For k As Integer = 0 To n - 1
                ' ---- 选主元（部分主元）----
                Dim p As Integer = k
                Dim best As Double = std.Abs(rows(k)(k))

                For i As Integer = k + 1 To n - 1
                    Dim v As Double = std.Abs(rows(i)(k))

                    If v > best Then
                        best = v
                        p = i
                    End If
                Next

                If best < 0.0000000000001 Then Return Nothing

                If p <> k Then
                    Dim tmp As Double() = rows(k)
                    rows(k) = rows(p)
                    rows(p) = tmp

                    Dim ti As Int32 = piv(k)
                    piv(k) = piv(p)
                    piv(p) = ti
                End If

                Dim pivotRow As Double() = rows(k)
                Dim pivotVal As Double = pivotRow(k)
                Dim remaining As Integer = (n - k - 1) * (n - k - 1)

                ' ---- L 因子 + 行更新（U 区段连续 AXPY，可并行）----
                If ShouldParallelizeByWork(remaining) Then
                    Dim po As ParallelOptions = MakeOptions()

                    Call Parallel.For(k + 1, n, po,
                        Sub(i)
                            Dim row As Double() = rows(i)
                            Dim lik As Double = row(k) / pivotVal

                            row(k) = lik
                            Call AxpyRange(row, pivotRow, -lik, k + 1, n)
                        End Sub)
                Else
                    For i As Integer = k + 1 To n - 1
                        Dim row As Double() = rows(i)
                        Dim lik As Double = row(k) / pivotVal

                        row(k) = lik
                        Call AxpyRange(row, pivotRow, -lik, k + 1, n)
                    Next
                End If
            Next

            Return New LuFactorization(ToMatrix(rows), piv)
        End Function

        ''' <summary>
        ''' 部分主元 LU 分解（Double(,) 版本）：内部转 jagged 消元（SIMD），
        ''' 结束后转回 <see cref="LuFactorization"/> 所需的行主序 Double(,)。
        ''' 输入矩阵不被修改。
        ''' </summary>
        Public Function LuFactorSimd(A As Double(,)) As LuFactorization
            Return LuFactorRows(ToRows(A))
        End Function

        ' ================================================================
        ' 并行调度
        ' ================================================================

        ''' <summary>按 行数 × 每行工作量 判断是否值得多线程。</summary>
        <System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)>
        Public Function ShouldParallelize(count As Integer, workPerRow As Integer) As Boolean
            Return EnableParallel AndAlso count >= 2 AndAlso CLng(count) * workPerRow >= MinParallelWork
        End Function

        ''' <summary>
        ''' 按工作量自适应的并行 for：达到阈值走 <see cref="Parallel.For"/>，
        ''' 否则顺序执行。迭代间必须相互独立（无共享可变状态）。
        ''' </summary>
        Public Sub ForParallel(count As Integer, workPerItem As Integer, body As Action(Of Integer))
            If ShouldParallelize(count, workPerItem) Then
                Call Parallel.For(0, count, MakeOptions(), body)
            Else
                For i As Integer = 0 To count - 1
                    Call body(i)
                Next
            End If
        End Sub

        ''' <summary>按总工作量（元素操作数）判断是否值得多线程。</summary>
        <System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)>
        Private Function ShouldParallelizeByWork(work As Integer) As Boolean
            Return EnableParallel AndAlso work >= MinParallelWork
        End Function

        Private Function MakeOptions() As ParallelOptions
            Dim po As New ParallelOptions()

            If MaxThreads > 0 Then
                po.MaxDegreeOfParallelism = MaxThreads
            End If

            Return po
        End Function

    End Module

End Namespace

#Region "Microsoft.VisualBasic::9bc56fa453a38383278d4344afca51dd, Data_science\Mathematica\Math\Math\Algebra\LP\IPMCrossover\SparseNormal.vb"

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

    '   Total Lines: 232
    '    Code Lines: 149 (64.22%)
    ' Comment Lines: 38 (16.38%)
    '    - Xml Docs: 60.53%
    ' 
    '   Blank Lines: 45 (19.40%)
    '     File Size: 8.75 KB


    '     Class SymCsr
    ' 
    '         Properties: NonZeros
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Clone, Diagonal, Mv, RowDegrees
    ' 
    '         Sub: AddDiagonal, MvInto
    ' 
    '     Module SparseNormal
    ' 
    '         Function: Assemble
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' SparseNormal.vb — 正规方程 A·Θ·Aᵀ 的稀疏装配 [readme §2.2 / plan step 1]
' ----------------------------------------------------------------------------
' A·Θ·Aᵀ = Σ_j θ_j · a_j · a_jᵀ（a_j 为 A 的第 j 列）
'   ⇒ 按列扫描：第 j 列的 k 个非零行两两配对，产生 k² 个累加项。
'   复杂度 O(Σ_j k_j²)，与 A 的列非零元分布有关、与 m·n 无关。
'   GEM（71949 非零元 / 16107 列，平均 4.5/列）≈ 3.3e5 次乘加，可忽略。
'
' 两遍法（先数模式、再填值）+ marker 数组去重，产出对称 CSR（含对角、两个三角全存）。
' 模式在迭代过程中不变（θ 恒 > 0），但每遍成本极低，故不做模式缓存。
' ============================================================================

Imports System
Imports std = System.Math

Namespace LinearAlgebra.LinearProgramming.IPMCrossover

    ''' <summary>对称稀疏矩阵（CSR，两个三角都存，含对角）</summary>
    Public Class SymCsr

        Public ReadOnly N As Int32
        Public ReadOnly RowPtr As Int32()
        Public ReadOnly ColIdx As Int32()
        Public ReadOnly Values As Double()

        Public Sub New(n As Int32, rowPtr As Int32(), colIdx As Int32(), values As Double())
            Me.N = n
            Me.RowPtr = rowPtr
            Me.ColIdx = colIdx
            Me.Values = values
        End Sub

        Public ReadOnly Property NonZeros As Long
            Get
                Return If(ColIdx Is Nothing, 0, CLng(ColIdx.Length))
            End Get
        End Property

        Public Function Mv(x As Double()) As Double()
            Dim y(N - 1) As Double

            MvInto(x, y)

            Return y
        End Function

        ''' <summary>无分配版本的 A·x（PCG 内循环每步都调用，避免 GC 抖动）</summary>
        Public Sub MvInto(x As Double(), y As Double())
            For i As Int32 = 0 To N - 1
                Dim s As Double = 0
                For p As Int32 = RowPtr(i) To RowPtr(i + 1) - 1
                    s += Values(p) * x(ColIdx(p))
                Next
                y(i) = s
            Next
        End Sub

        ''' <summary>取对角元（缺失视为 0）</summary>
        Public Function Diagonal() As Double()
            Dim d(N - 1) As Double

            For i As Int32 = 0 To N - 1
                For p As Int32 = RowPtr(i) To RowPtr(i + 1) - 1
                    If ColIdx(p) = i Then
                        d(i) = Values(p)
                        Exit For
                    End If
                Next
            Next

            Return d
        End Function

        ''' <summary>对角加 reg（M + reg·I）；保证每行对角元存在</summary>
        Public Sub AddDiagonal(reg As Double)
            For i As Int32 = 0 To N - 1
                Dim found As Boolean = False

                For p As Int32 = RowPtr(i) To RowPtr(i + 1) - 1
                    If ColIdx(p) = i Then
                        Values(p) += reg
                        found = True
                        Exit For
                    End If
                Next

                If Not found Then
                    Throw New InvalidOperationException("SymCsr: 第 " & i & " 行缺少对角元")
                End If
            Next
        End Sub

        ''' <summary>深拷贝（正则化阶梯重试时保留未加 reg 的原始版本）</summary>
        Public Function Clone() As SymCsr
            Dim rp As Int32() = New Int32(RowPtr.Length - 1) {}
            Dim ci As Int32() = New Int32(std.Max(ColIdx.Length, 1) - 1) {}
            Dim vx As Double() = New Double(std.Max(Values.Length, 1) - 1) {}

            Array.Copy(RowPtr, rp, RowPtr.Length)
            If ColIdx.Length > 0 Then Array.Copy(ColIdx, ci, ColIdx.Length)
            If Values.Length > 0 Then Array.Copy(Values, vx, Values.Length)

            Return New SymCsr(N, rp, ci, vx)
        End Function

        ''' <summary>每行非零元个数（排序启发式的度数）</summary>
        Public Function RowDegrees() As Int32()
            Dim deg(N - 1) As Int32

            For i As Int32 = 0 To N - 1
                deg(i) = RowPtr(i + 1) - RowPtr(i)
            Next

            Return deg
        End Function

    End Class

    Public Module SparseNormal

        ''' <summary>
        ''' 稀疏装配 A·Θ·Aᵀ（对称 CSR）
        ''' </summary>
        ''' <param name="rp">A 的 CSR 行指针（长度 m+1）</param>
        ''' <param name="ci">A 的 CSR 列索引</param>
        ''' <param name="vx">A 的 CSR 值</param>
        ''' <param name="cp">A 的 CSC 列指针（长度 n+1）</param>
        ''' <param name="ri">A 的 CSC 行索引</param>
        ''' <param name="rv">A 的 CSC 值</param>
        ''' <param name="m">A 的行数</param>
        ''' <param name="n">A 的列数</param>
        ''' <param name="theta">对角缩放（长度 n）</param>
        ''' <remarks>
        ''' 按行做外层循环：第 i 行的模式 = ∪_{j: A(i,j)≠0} {k : A(k,j)≠0}。
        ''' marker 的戳必须用**行号**（而不是列号）——同一列 j 会同时贡献给多个行 i，
        ''' 用列号做戳会让第 i2 行误判"该列位置已存在"，把值写进第 i1 行的槽位。
        ''' </remarks>
        Public Function Assemble(rp As Int32(), ci As Int32(), vx As Double(),
                                 cp As Int32(), ri As Int32(), rv As Double(),
                                 m As Int32, n As Int32, theta As Double()) As SymCsr
            If m <= 0 Then
                Return New SymCsr(0, New Int32(m + 1) {}, New Int32() {}, New Double() {})
            End If

            Dim marker(m - 1) As Int32
            Dim mpos(m - 1) As Int32
            Dim cnt(m - 1) As Int32

            ' ---- pass 1：数模式 ----
            For i As Int32 = 0 To m - 1
                Dim stamp As Int32 = i + 1

                For p As Int32 = rp(i) To rp(i + 1) - 1
                    Dim j As Int32 = ci(p)

                    For q As Int32 = cp(j) To cp(j + 1) - 1
                        Dim k As Int32 = ri(q)

                        If marker(k) <> stamp Then
                            marker(k) = stamp
                            cnt(i) += 1
                        End If
                    Next
                Next
            Next

            ' 空行补一个对角槽位，保证 M+reg·I 的对角严格正
            For i As Int32 = 0 To m - 1
                If cnt(i) = 0 Then cnt(i) = 1
            Next

            Dim mr(m) As Int32

            For i As Int32 = 0 To m - 1
                mr(i + 1) = mr(i) + cnt(i)
            Next

            Dim nnz As Int32 = mr(m)
            Dim outIdx(nnz - 1) As Int32
            Dim outVal(nnz - 1) As Double
            Dim fill(m - 1) As Int32

            For i As Int32 = 0 To m - 1
                marker(i) = 0
            Next

            ' ---- pass 2：填值 ----
            For i As Int32 = 0 To m - 1
                Dim stamp As Int32 = i + 1

                For p As Int32 = rp(i) To rp(i + 1) - 1
                    Dim j As Int32 = ci(p)
                    Dim aij As Double = vx(p)
                    Dim scaled As Double = aij * theta(j)

                    For q As Int32 = cp(j) To cp(j + 1) - 1
                        Dim k As Int32 = ri(q)
                        Dim pos As Int32

                        If marker(k) <> stamp Then
                            marker(k) = stamp
                            pos = mr(i) + fill(i)
                            outIdx(pos) = k
                            outVal(pos) = 0.0
                            mpos(k) = pos
                            fill(i) += 1
                        Else
                            pos = mpos(k)
                        End If

                        outVal(pos) += scaled * rv(q)
                    Next
                Next
            Next

            ' 空行补对角（值为 0，随后 AddDiagonal 会加上 reg）
            For i As Int32 = 0 To m - 1
                If fill(i) < cnt(i) Then
                    Dim pos As Int32 = mr(i) + fill(i)

                    outIdx(pos) = i
                    outVal(pos) = 0.0
                    fill(i) += 1
                End If
            Next

            Return New SymCsr(m, mr, outIdx, outVal)
        End Function

    End Module

End Namespace


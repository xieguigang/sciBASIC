#Region "Microsoft.VisualBasic::cb73f5f808198ed96ad08351ba4cec77, Data_science\Mathematica\Math\Math\Algebra\Matrix.NET\Decomposition\SingularValueDecomposition.vb"

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

    '   Total Lines: 566
    '    Code Lines: 400 (70.67%)
    ' Comment Lines: 91 (16.08%)
    '    - Xml Docs: 54.95%
    ' 
    '   Blank Lines: 75 (13.25%)
    '     File Size: 21.92 KB


    '     Class SingularValueDecomposition
    ' 
    '         Properties: Condition, Norm2, Rank, S, SingularValues
    '                     U, V
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports stdf = System.Math
Imports SIMDIntrinsics = Microsoft.VisualBasic.Math.SIMD.SIMDIntrinsics

Namespace LinearAlgebra.Matrix

    ''' <summary>Singular Value Decomposition.
    ''' <P>
    ''' For an m-by-n matrix A with m >= n, the singular value decomposition is
    ''' an m-by-n orthogonal matrix U, an n-by-n diagonal matrix S, and
    ''' an n-by-n orthogonal matrix V so that A = U*S*V'.</P>
    ''' <P>
    ''' The singular values, sigma[k] = S[k][k], are ordered so that
    ''' sigma[0] >= sigma[1] >= ... >= sigma[n-1].</P>
    ''' <P>
    ''' The singular value decompostion always exists, so the constructor will
    ''' never fail.  The matrix condition number and the effective numerical
    ''' rank can be computed from this decomposition.</P>
    ''' </summary>
    Public Class SingularValueDecomposition

#Region "Class variables"

        ''' <summary>Arrays for internal storage of U and V.
        ''' @serial internal storage of U.
        ''' @serial internal storage of V.
        ''' </summary>
        Dim valueU As Double()(), valueV As Double()()

        ''' <summary>Array for internal storage of singular values.
        ''' @serial internal storage of singular values.
        ''' </summary>
        Dim m_s As Double()

        ''' <summary>Row and column dimensions.
        ''' @serial row dimension.
        ''' @serial column dimension.
        ''' </summary>
        Dim m As Integer, n As Integer

#End Region

#Region "Constructor"

        Sub New(A As Double(,))
            Call Me.New(New NumericMatrix(A))
        End Sub

        ''' <summary>
        ''' Construct the singular value decomposition, returns Structure to access U, S and V.
        ''' </summary>
        ''' <param name="Arg">   Rectangular matrix
        ''' </param>
        Public Sub New(Arg As GeneralMatrix)
            ' Derived from LINPACK code.
            ' Initialize.
            Dim A As Double()() = Arg.ArrayPack(deepcopy:=True)
            Dim U, V As Double()()

            m = Arg.RowDimension
            n = Arg.ColumnDimension
            Dim nu As Integer = System.Math.Min(m, n)
            m_s = New Double(System.Math.Min(m + 1, n) - 1) {}
            U = New Double(m - 1)() {}
            For i As Integer = 0 To m - 1
                U(i) = New Double(nu - 1) {}
            Next
            V = New Double(n - 1)() {}
            For i2 As Integer = 0 To n - 1
                V(i2) = New Double(n - 1) {}
            Next
            Dim e As Double() = New Double(n - 1) {}
            Dim work As Double() = New Double(m - 1) {}
            Dim wantu As Boolean = True
            Dim wantv As Boolean = True

            ' Reduce A to bidiagonal form, storing the diagonal elements
            ' in s and the super-diagonal elements in e.

            Dim nct As Integer = System.Math.Min(m - 1, n)
            Dim nrt As Integer = System.Math.Max(0, System.Math.Min(n - 2, m))
            For k As Integer = 0 To System.Math.Max(nct, nrt) - 1
                If k < nct Then

                    ' Compute the transformation for the k-th column and
                    ' place the k-th diagonal in s[k].
                    ' Compute 2-norm of k-th column without under/overflow.
                    m_s(k) = 0
                    For i As Integer = k To m - 1
                        m_s(k) = Hypot(m_s(k), A(i)(k))
                    Next
                    If m_s(k) <> 0.0 Then
                        If A(k)(k) < 0.0 Then
                            m_s(k) = -m_s(k)
                        End If
                        For i As Integer = k To m - 1
                            A(i)(k) /= m_s(k)
                        Next
                        A(k)(k) += 1.0
                    End If
                    m_s(k) = -m_s(k)
                End If

                ' SIMD 化：抽取 Householder 列片段 [k, m) 为连续数组，
                ' 随后对每一列的点积与 AXPY 走 FMA 内核
                Dim house As Double() = Nothing
                Dim colJ As Double() = Nothing

                If (k < nct) AndAlso (m_s(k) <> 0.0) Then
                    Dim len As Integer = m - k
                    house = New Double(len - 1) {}
                    colJ = New Double(len - 1) {}

                    For i As Integer = k To m - 1
                        house(i - k) = A(i)(k)
                    Next
                End If

                For j As Integer = k + 1 To n - 1
                    If house IsNot Nothing Then

                        ' Apply the transformation.

                        Call ExtractColumnFragment(A, j, k, m, colJ)

                        Dim t As Double = SIMDIntrinsics.DotFma(house, colJ)
                        t = (-t) / A(k)(k)

                        Call ApplyColumnAxpy(A, j, k, m, house, colJ, t)
                    End If

                    ' Place the k-th row of A into e for the
                    ' subsequent calculation of the row transformation.

                    e(j) = A(k)(j)
                Next
                If wantu And (k < nct) Then

                    ' Place the transformation in U for subsequent back
                    ' multiplication.

                    For i As Integer = k To m - 1
                        U(i)(k) = A(i)(k)
                    Next
                End If
                If k < nrt Then

                    ' Compute the k-th row transformation and place the
                    ' k-th super-diagonal in e[k].
                    ' Compute 2-norm without under/overflow.
                    e(k) = 0
                    For i As Integer = k + 1 To n - 1
                        e(k) = Hypot(e(k), e(i))
                    Next
                    If e(k) <> 0.0 Then
                        If e(k + 1) < 0.0 Then
                            e(k) = -e(k)
                        End If
                        For i As Integer = k + 1 To n - 1
                            e(i) /= e(k)
                        Next
                        e(k + 1) += 1.0
                    End If
                    e(k) = -e(k)
                    If (k + 1 < m) And (e(k) <> 0.0) Then

                        ' Apply the transformation.
                        ' SIMD 化：work 的累加与回写改为列片段 + FMA 内核
                        ' （workFrag 即旧实现中的 work(i) 片段，新分配即零初始化）
                        Dim lenW As Integer = m - k - 1
                        Dim workFrag As Double() = New Double(lenW - 1) {}
                        Dim colJA As Double() = New Double(lenW - 1) {}

                        For j As Integer = k + 1 To n - 1
                            Call ExtractColumnFragment(A, j, k + 1, m, colJA)
                            Call SIMDIntrinsics.AxpyInPlace(e(j), colJA, workFrag)
                        Next

                        For j As Integer = k + 1 To n - 1
                            Call ExtractColumnFragment(A, j, k + 1, m, colJA)

                            Dim t As Double = (-e(j)) / e(k + 1)
                            Call ApplyColumnAxpy(A, j, k + 1, m, workFrag, colJA, t)
                        Next
                    End If
                    If wantv Then

                        ' Place the transformation in V for subsequent
                        ' back multiplication.

                        For i As Integer = k + 1 To n - 1
                            V(i)(k) = e(i)
                        Next
                    End If
                End If
            Next

            ' Set up the final bidiagonal matrix or order p.

            Dim p As Integer = System.Math.Min(n, m + 1)
            If nct < n Then
                m_s(nct) = A(nct)(nct)
            End If
            If m < p Then
                m_s(p - 1) = 0.0
            End If
            If nrt + 1 < p Then
                e(nrt) = A(nrt)(p - 1)
            End If
            e(p - 1) = 0.0

            ' If required, generate U.

            If wantu Then
                For j As Integer = nct To nu - 1
                    For i As Integer = 0 To m - 1
                        U(i)(j) = 0.0
                    Next
                    U(j)(j) = 1.0
                Next
                For k As Integer = nct - 1 To 0 Step -1
                    If m_s(k) <> 0.0 Then
                        ' SIMD 化：抽取 U 的 Householder 列片段 [k, m)，
                        ' 每列的点积与 AXPY 走 FMA 内核
                        Dim lenU As Integer = m - k
                        Dim houseU As Double() = New Double(lenU - 1) {}
                        Dim colJU As Double() = New Double(lenU - 1) {}

                        For i As Integer = k To m - 1
                            houseU(i - k) = U(i)(k)
                        Next

                        For j As Integer = k + 1 To nu - 1
                            Call ExtractColumnFragment(U, j, k, m, colJU)

                            Dim t As Double = SIMDIntrinsics.DotFma(houseU, colJU)
                            t = (-t) / U(k)(k)

                            Call ApplyColumnAxpy(U, j, k, m, houseU, colJU, t)
                        Next
                        For i As Integer = k To m - 1
                            U(i)(k) = -U(i)(k)
                        Next
                        U(k)(k) = 1.0 + U(k)(k)
                        For i As Integer = 0 To k - 2
                            U(i)(k) = 0.0
                        Next
                    Else
                        For i As Integer = 0 To m - 1
                            U(i)(k) = 0.0
                        Next
                        U(k)(k) = 1.0
                    End If
                Next
            End If

            ' If required, generate V.

            If wantv Then
                For k As Integer = n - 1 To 0 Step -1
                    If (k < nrt) And (e(k) <> 0.0) Then
                        ' SIMD 化：抽取 V 的 Householder 列片段 [k+1, n)
                        Dim lenV As Integer = n - k - 1
                        Dim houseV As Double() = New Double(lenV - 1) {}
                        Dim colJV As Double() = New Double(lenV - 1) {}

                        For i As Integer = k + 1 To n - 1
                            houseV(i - k - 1) = V(i)(k)
                        Next

                        For j As Integer = k + 1 To nu - 1
                            Call ExtractColumnFragment(V, j, k + 1, n, colJV)

                            Dim t As Double = SIMDIntrinsics.DotFma(houseV, colJV)
                            t = (-t) / V(k + 1)(k)

                            Call ApplyColumnAxpy(V, j, k + 1, n, houseV, colJV, t)
                        Next
                    End If
                    For i As Integer = 0 To n - 1
                        V(i)(k) = 0.0
                    Next
                    V(k)(k) = 1.0
                Next
            End If

            ' Main iteration loop for the singular values.

            Dim pp As Integer = p - 1
            Dim iter As Integer = 0
            Dim eps As Double = System.Math.Pow(2.0, -52.0)
            ' 准确度修正：增加总迭代次数上限。旧实现中 iter 只在 kase=4 时归零，
            ' 病态输入可能使主循环永远无法收敛（JAMA 已知缺陷，原注释自认
            ' "Here is where a test for too many iterations would go"）。
            Dim totalIter As Integer = 0
            Dim maxTotalIterations As Integer = 1000 + 100 * System.Math.Max(m, n)
            While p > 0
                Dim k As Integer, kase As Integer

                totalIter += 1

                If totalIter > maxTotalIterations Then
                    Throw New ApplicationException(
                        $"SVD does not converge after {maxTotalIterations} iterations of the QR step.")
                End If

                ' This section of the program inspects for
                ' negligible elements in the s and e arrays.  On
                ' completion the variables kase and k are set as follows.

                ' kase = 1     if s(p) and e[k-1] are negligible and k<p
                ' kase = 2     if s(k) is negligible and k<p
                ' kase = 3     if e[k-1] is negligible, k<p, and
                '              s(k), ..., s(p) are not negligible (qr step).
                ' kase = 4     if e(p-1) is negligible (convergence).

                For k = p - 2 To -1 Step -1
                    If k = -1 Then
                        Exit For
                    End If
                    If stdf.Abs(e(k)) <= eps * (stdf.Abs(m_s(k)) + stdf.Abs(m_s(k + 1))) Then
                        e(k) = 0.0
                        Exit For
                    End If
                Next
                If k = p - 2 Then
                    kase = 4
                Else
                    Dim ks As Integer
                    For ks = p - 1 To k Step -1
                        If ks = k Then
                            Exit For
                        End If
                        Dim t As Double = (If(ks <> p, stdf.Abs(e(ks)), 0.0)) + (If(ks <> k + 1, stdf.Abs(e(ks - 1)), 0.0))
                        If stdf.Abs(m_s(ks)) <= eps * t Then
                            m_s(ks) = 0.0
                            Exit For
                        End If
                    Next
                    If ks = k Then
                        kase = 3
                    ElseIf ks = p - 1 Then
                        kase = 1
                    Else
                        kase = 2
                        k = ks
                    End If
                End If
                k += 1

                ' Perform the task indicated by kase.

                Select Case kase


                ' Deflate negligible s(p).
                    Case 1

                        Dim f As Double = e(p - 2)
                        e(p - 2) = 0.0
                        For j As Integer = p - 2 To k Step -1
                            Dim t As Double = Hypot(m_s(j), f)
                            Dim cs As Double = m_s(j) / t
                            Dim sn As Double = f / t
                            m_s(j) = t
                            If j <> k Then
                                f = (-sn) * e(j - 1)
                                e(j - 1) = cs * e(j - 1)
                            End If
                            If wantv Then
                                For i As Integer = 0 To n - 1
                                    t = cs * V(i)(j) + sn * V(i)(p - 1)
                                    V(i)(p - 1) = (-sn) * V(i)(j) + cs * V(i)(p - 1)
                                    V(i)(j) = t
                                Next
                            End If
                        Next



                ' Split at negligible s(k).


                    Case 2

                        Dim f As Double = e(k - 1)
                        e(k - 1) = 0.0
                        For j As Integer = k To p - 1
                            Dim t As Double = Hypot(m_s(j), f)
                            Dim cs As Double = m_s(j) / t
                            Dim sn As Double = f / t
                            m_s(j) = t
                            f = (-sn) * e(j)
                            e(j) = cs * e(j)
                            If wantu Then
                                For i As Integer = 0 To m - 1
                                    t = cs * U(i)(j) + sn * U(i)(k - 1)
                                    U(i)(k - 1) = (-sn) * U(i)(j) + cs * U(i)(k - 1)
                                    U(i)(j) = t
                                Next
                            End If
                        Next

                ' Perform one qr step.


                    Case 3

                        ' Calculate the shift.

                        Dim scale As Double = stdf.Max(stdf.Max(stdf.Max(stdf.Max(stdf.Abs(m_s(p - 1)), stdf.Abs(m_s(p - 2))), stdf.Abs(e(p - 2))), stdf.Abs(m_s(k))), stdf.Abs(e(k)))
                        Dim sp As Double = m_s(p - 1) / scale
                        Dim spm1 As Double = m_s(p - 2) / scale
                        Dim epm1 As Double = e(p - 2) / scale
                        Dim sk As Double = m_s(k) / scale
                        Dim ek As Double = e(k) / scale
                        Dim b As Double = ((spm1 + sp) * (spm1 - sp) + epm1 * epm1) / 2.0
                        Dim c As Double = (sp * epm1) * (sp * epm1)
                        Dim shift As Double = 0.0
                        If (b <> 0.0) Or (c <> 0.0) Then
                            shift = stdf.Sqrt(b * b + c)
                            If b < 0.0 Then
                                shift = -shift
                            End If
                            shift = c / (b + shift)
                        End If
                        Dim f As Double = (sk + sp) * (sk - sp) + shift
                        Dim g As Double = sk * ek

                        ' Chase zeros.

                        For j As Integer = k To p - 2
                            Dim t As Double = Hypot(f, g)
                            Dim cs As Double = f / t
                            Dim sn As Double = g / t
                            If j <> k Then
                                e(j - 1) = t
                            End If
                            f = cs * m_s(j) + sn * e(j)
                            e(j) = cs * e(j) - sn * m_s(j)
                            g = sn * m_s(j + 1)
                            m_s(j + 1) = cs * m_s(j + 1)
                            If wantv Then
                                For i As Integer = 0 To n - 1
                                    t = cs * V(i)(j) + sn * V(i)(j + 1)
                                    V(i)(j + 1) = (-sn) * V(i)(j) + cs * V(i)(j + 1)
                                    V(i)(j) = t
                                Next
                            End If
                            t = Hypot(f, g)
                            cs = f / t
                            sn = g / t
                            m_s(j) = t
                            f = cs * e(j) + sn * m_s(j + 1)
                            m_s(j + 1) = (-sn) * e(j) + cs * m_s(j + 1)
                            g = sn * e(j + 1)
                            e(j + 1) = cs * e(j + 1)
                            If wantu AndAlso (j < m - 1) Then
                                For i As Integer = 0 To m - 1
                                    t = cs * U(i)(j) + sn * U(i)(j + 1)
                                    U(i)(j + 1) = (-sn) * U(i)(j) + cs * U(i)(j + 1)
                                    U(i)(j) = t
                                Next
                            End If
                        Next
                        e(p - 2) = f
                        iter = iter + 1

                ' Convergence.


                    Case 4

                        ' Make the singular values positive.

                        If m_s(k) <= 0.0 Then
                            m_s(k) = (If(m_s(k) < 0.0, -m_s(k), 0.0))
                            If wantv Then
                                For i As Integer = 0 To pp
                                    V(i)(k) = -V(i)(k)
                                Next
                            End If
                        End If

                        ' Order the singular values.

                        While k < pp
                            If m_s(k) >= m_s(k + 1) Then
                                Exit While
                            End If
                            Dim t As Double = m_s(k)
                            m_s(k) = m_s(k + 1)
                            m_s(k + 1) = t
                            If wantv AndAlso (k < n - 1) Then
                                For i As Integer = 0 To n - 1
                                    t = V(i)(k + 1)
                                    V(i)(k + 1) = V(i)(k)
                                    V(i)(k) = t
                                Next
                            End If
                            If wantu AndAlso (k < m - 1) Then
                                For i As Integer = 0 To m - 1
                                    t = U(i)(k + 1)
                                    U(i)(k + 1) = U(i)(k)
                                    U(i)(k) = t
                                Next
                            End If
                            k += 1
                        End While
                        iter = 0
                        p -= 1


                End Select
            End While

            valueU = U
            valueV = V
        End Sub

        ''' <summary>
        ''' SIMD 化辅助：把矩阵 M 的列 j 的行片段 [rowStart, rowEnd) 抽取为连续数组
        ''' </summary>
        Private Shared Sub ExtractColumnFragment(M As Double()(), j As Integer,
                                                 rowStart As Integer, rowEnd As Integer,
                                                 colJ As Double())

            For i As Integer = rowStart To rowEnd - 1
                colJ(i - rowStart) = M(i)(j)
            Next
        End Sub

        ''' <summary>
        ''' SIMD 化辅助：colJ += t * house（就地 FMA AXPY），并把结果写回 M 的列 j
        ''' </summary>
        Private Shared Sub ApplyColumnAxpy(M As Double()(), j As Integer,
                                           rowStart As Integer, rowEnd As Integer,
                                           house As Double(), colJ As Double(), t As Double)

            Call SIMDIntrinsics.AxpyInPlace(t, house, colJ)

            For i As Integer = rowStart To rowEnd - 1
                M(i)(j) = colJ(i - rowStart)
            Next
        End Sub
#End Region

#Region "Public Properties"
        ''' <summary>Return the one-dimensional array of singular values</summary>
        ''' <returns>     diagonal of S.
        ''' </returns>
        Public Overridable ReadOnly Property SingularValues() As Vector
            Get
                Return New Vector(m_s)
            End Get
        End Property

        ''' <summary>Return the diagonal matrix of singular values</summary>
        ''' <returns>     S
        ''' </returns>
        Public Overridable ReadOnly Property S() As GeneralMatrix
            Get
                Dim X As New NumericMatrix(n, n)
                Dim Sa As Double()() = X.Array
                For i As Integer = 0 To n - 1
                    For j As Integer = 0 To n - 1
                        Sa(i)(j) = 0.0
                    Next
                    Sa(i)(i) = Me.m_s(i)
                Next
                Return X
            End Get
        End Property
#End Region

#Region "Public Methods"

        ''' <summary>Return the left singular vectors</summary>
        ''' <returns>     U
        ''' </returns>
        Public ReadOnly Property U() As GeneralMatrix
            Get
                Return New NumericMatrix(valueU, m, System.Math.Min(m + 1, n))
            End Get
        End Property

        ''' <summary>Return the right singular vectors</summary>
        ''' <returns>     V
        ''' </returns>
        Public ReadOnly Property V() As GeneralMatrix
            Get
                Return New NumericMatrix(valueV, n, n)
            End Get
        End Property

        ''' <summary>Two norm</summary>
        ''' <returns>     max(S)
        ''' </returns>
        ''' 
        Public ReadOnly Property Norm2() As Double
            Get
                Return m_s(0)
            End Get
        End Property

        ''' <summary>Two norm condition number</summary>
        ''' <returns>     max(S)/min(S)
        ''' </returns>
        ''' <remarks>
        ''' 准确度修正：最小奇异值为 0（秩亏矩阵）时返回
        ''' <see cref="Double.PositiveInfinity"/> 而非产生除零 NaN。
        ''' </remarks>
        Public ReadOnly Property Condition() As Double
            Get
                Dim sMin As Double = m_s(System.Math.Min(m, n) - 1)

                If sMin = 0 Then
                    Return Double.PositiveInfinity
                End If

                Return m_s(0) / sMin
            End Get
        End Property

        ''' <summary>Effective numerical matrix rank</summary>
        ''' <returns>     Number of nonnegligible singular values.
        ''' </returns>

        Public ReadOnly Property Rank() As Integer
            Get
                Dim eps As Double = System.Math.Pow(2.0, -52.0)
                Dim tol As Double = System.Math.Max(m, n) * m_s(0) * eps
                Dim r As Integer = 0
                For i As Integer = 0 To m_s.Length - 1
                    If m_s(i) > tol Then
                        r += 1
                    End If
                Next
                Return r
            End Get
        End Property
#End Region

    End Class
End Namespace

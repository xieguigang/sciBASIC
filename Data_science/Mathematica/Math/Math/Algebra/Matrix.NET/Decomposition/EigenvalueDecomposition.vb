#Region "Microsoft.VisualBasic::abbc32540d27ea08b6a36ce96b7cf598, Data_science\Mathematica\Math\Math\Algebra\Matrix.NET\Decomposition\EigenvalueDecomposition.vb"

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

    '   Total Lines: 980
    '    Code Lines: 707 (72.14%)
    ' Comment Lines: 140 (14.29%)
    '    - Xml Docs: 42.86%
    ' 
    '   Blank Lines: 133 (13.57%)
    '     File Size: 36.83 KB


    '     Class EigenvalueDecomposition
    ' 
    '         Properties: D, ImagEigenvalues, RealEigenvalues, V
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: cdiv, hqr2, orthes, tql2, tred2
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.Serialization
Imports stdnum = System.Math

Namespace LinearAlgebra.Matrix

    ''' <summary>
    ''' Eigenvalues and eigenvectors of a real matrix. 
    ''' If A is symmetric, then A = V*D*V' where the eigenvalue matrix D is
    ''' diagonal and the eigenvector matrix V is orthogonal.
    ''' I.e. A = V.Multiply(D.Multiply(V.Transpose())) and 
    ''' V.Multiply(V.Transpose()) equals the identity matrix.
    ''' If A is not symmetric, then the eigenvalue matrix D is block diagonal
    ''' with the real eigenvalues in 1-by-1 blocks and any complex eigenvalues,
    ''' lambda + i*mu, in 2-by-2 blocks, [lambda, mu; -mu, lambda].  The
    ''' columns of V represent the eigenvectors in the sense that A*V = V*D,
    ''' i.e. A.Multiply(V) equals V.Multiply(D).  The matrix V may be badly
    ''' conditioned, or even singular, so the validity of the equation
    ''' A = V*D*Inverse(V) depends upon V.cond().
    ''' 
    ''' </summary>
    Public Class EigenvalueDecomposition

#Region "Class variables"

        ''' <summary>Row and column dimension (square matrix).
        ''' @serial matrix dimension.
        ''' </summary>
        Private n As Integer

        ''' <summary>Symmetry flag.
        ''' @serial internal symmetry flag.
        ''' </summary>
        Private issymmetric As Boolean

        ''' <summary>Arrays for internal storage of eigenvalues.
        ''' @serial internal storage of eigenvalues.
        ''' </summary>
        Private m_d As Double(), m_e As Double()

        ''' <summary>Array for internal storage of eigenvectors.
        ''' @serial internal storage of eigenvectors.
        ''' </summary>
        Private _V As Double()()

        ''' <summary>Array for internal storage of nonsymmetric Hessenberg form.
        ''' @serial internal storage of nonsymmetric Hessenberg form.
        ''' </summary>
        Private H As Double()()

        ''' <summary>Working storage for nonsymmetric algorithm.
        ''' @serial working storage for nonsymmetric algorithm.
        ''' </summary>
        Private ort As Double()

#End Region

#Region "Private Methods"

        ''' <summary>
        ''' Symmetric Householder reduction to tridiagonal form.
        ''' </summary>
        Private Sub tred2()
            '  This is derived from the Algol procedures tred2 by
            '  Bowdler, Martin, Reinsch, and Wilkinson, Handbook for
            '  Auto. Comp., Vol.ii-Linear Algebra, and the corresponding
            '  Fortran subroutine in EISPACK.
            Dim V = _V

            For j As Integer = 0 To n - 1
                m_d(j) = V(n - 1)(j)
            Next

            ' Householder reduction to tridiagonal form.

            For i As Integer = n - 1 To 1 Step -1
                ' Scale to avoid under/overflow.

                Dim scale As Double = 0.0
                Dim h As Double = 0.0
                For k As Integer = 0 To i - 1
                    scale = scale + stdnum.Abs(m_d(k))
                Next
                If scale = 0.0 Then
                    m_e(i) = m_d(i - 1)
                    For j As Integer = 0 To i - 1
                        m_d(j) = V(i - 1)(j)
                        V(i)(j) = 0.0
                        V(j)(i) = 0.0
                    Next
                Else
                    ' Generate Householder vector.

                    For k As Integer = 0 To i - 1
                        m_d(k) /= scale
                        h += m_d(k) * m_d(k)
                    Next
                    Dim f As Double = m_d(i - 1)
                    Dim g As Double = System.Math.Sqrt(h)
                    If f > 0 Then
                        g = -g
                    End If
                    m_e(i) = scale * g
                    h = h - f * g
                    m_d(i - 1) = f - g
                    For j As Integer = 0 To i - 1
                        m_e(j) = 0.0
                    Next

                    ' Apply similarity transformation to remaining columns.

                    For j As Integer = 0 To i - 1
                        f = m_d(j)
                        V(j)(i) = f
                        g = m_e(j) + V(j)(j) * f
                        For k As Integer = j + 1 To i - 1
                            g += V(k)(j) * m_d(k)
                            m_e(k) += V(k)(j) * f
                        Next
                        m_e(j) = g
                    Next
                    f = 0.0
                    For j As Integer = 0 To i - 1
                        m_e(j) /= h
                        f += m_e(j) * m_d(j)
                    Next
                    Dim hh As Double = f / (h + h)
                    For j As Integer = 0 To i - 1
                        m_e(j) -= hh * m_d(j)
                    Next
                    For j As Integer = 0 To i - 1
                        f = m_d(j)
                        g = m_e(j)
                        For k As Integer = j To i - 1
                            V(k)(j) -= (f * m_e(k) + g * m_d(k))
                        Next
                        m_d(j) = V(i - 1)(j)
                        V(i)(j) = 0.0
                    Next
                End If
                m_d(i) = h
            Next

            ' Accumulate transformations.

            For i As Integer = 0 To n - 2
                V(n - 1)(i) = V(i)(i)
                V(i)(i) = 1.0
                Dim h As Double = m_d(i + 1)
                If h <> 0.0 Then
                    For k As Integer = 0 To i
                        m_d(k) = V(k)(i + 1) / h
                    Next
                    For j As Integer = 0 To i
                        Dim g As Double = 0.0
                        For k As Integer = 0 To i
                            g += V(k)(i + 1) * V(k)(j)
                        Next
                        For k As Integer = 0 To i
                            V(k)(j) -= g * m_d(k)
                        Next
                    Next
                End If
                For k As Integer = 0 To i
                    V(k)(i + 1) = 0.0
                Next
            Next
            For j As Integer = 0 To n - 1
                m_d(j) = V(n - 1)(j)
                V(n - 1)(j) = 0.0
            Next
            V(n - 1)(n - 1) = 1.0
            m_e(0) = 0.0
        End Sub

        ' Symmetric tridiagonal QL algorithm.

        Private Sub tql2()
            '  This is derived from the Algol procedures tql2, by
            '  Bowdler, Martin, Reinsch, and Wilkinson, Handbook for
            '  Auto. Comp., Vol.ii-Linear Algebra, and the corresponding
            '  Fortran subroutine in EISPACK.

            Dim V = _V

            For i As Integer = 1 To n - 1
                m_e(i - 1) = m_e(i)
            Next
            m_e(n - 1) = 0.0

            Dim f As Double = 0.0
            Dim tst1 As Double = 0.0
            Dim eps As Double = System.Math.Pow(2.0, -52.0)
            For l As Integer = 0 To n - 1
                ' Find small subdiagonal element

                tst1 = System.Math.Max(tst1, stdnum.Abs(m_d(l)) + stdnum.Abs(m_e(l)))
                Dim m As Integer = l
                While m < n
                    If stdnum.Abs(m_e(m)) <= eps * tst1 Then
                        Exit While
                    End If
                    m += 1
                End While

                ' If m == l, d[l] is an eigenvalue,
                ' otherwise, iterate.

                If m > l Then
                    Dim iter As Integer = 0
                    Do
                        iter = iter + 1
                        ' (Could check iteration count here.)
                        ' Compute implicit shift

                        Dim g As Double = m_d(l)
                        Dim p As Double = (m_d(l + 1) - g) / (2.0 * m_e(l))
                        Dim r As Double = Hypot(p, 1.0)
                        If p < 0 Then
                            r = -r
                        End If
                        m_d(l) = m_e(l) / (p + r)
                        m_d(l + 1) = m_e(l) * (p + r)
                        Dim dl1 As Double = m_d(l + 1)
                        Dim h As Double = g - m_d(l)
                        For i As Integer = l + 2 To n - 1
                            m_d(i) -= h
                        Next
                        f = f + h

                        ' Implicit QL transformation.

                        p = m_d(m)
                        Dim c As Double = 1.0
                        Dim c2 As Double = c
                        Dim c3 As Double = c
                        Dim el1 As Double = m_e(l + 1)
                        Dim s As Double = 0.0
                        Dim s2 As Double = 0.0
                        For i As Integer = m - 1 To l Step -1
                            c3 = c2
                            c2 = c
                            s2 = s
                            g = c * m_e(i)
                            h = c * p
                            r = Hypot(p, m_e(i))
                            m_e(i + 1) = s * r
                            s = m_e(i) / r
                            c = p / r
                            p = c * m_d(i) - s * g
                            m_d(i + 1) = h + s * (c * g + s * m_d(i))

                            ' Accumulate transformation.

                            For k As Integer = 0 To n - 1
                                h = V(k)(i + 1)
                                V(k)(i + 1) = s * V(k)(i) + c * h
                                V(k)(i) = c * V(k)(i) - s * h
                            Next
                        Next
                        p = (-s) * s2 * c3 * el1 * m_e(l) / dl1
                        m_e(l) = s * p

                        ' Check for convergence.
                        m_d(l) = c * p
                    Loop While stdnum.Abs(m_e(l)) > eps * tst1
                End If
                m_d(l) = m_d(l) + f
                m_e(l) = 0.0
            Next

            ' Sort eigenvalues and corresponding vectors.

            For i As Integer = 0 To n - 2
                Dim k As Integer = i
                Dim p As Double = m_d(i)
                For j As Integer = i + 1 To n - 1
                    If m_d(j) < p Then
                        k = j
                        p = m_d(j)
                    End If
                Next
                If k <> i Then
                    m_d(k) = m_d(i)
                    m_d(i) = p
                    For j As Integer = 0 To n - 1
                        p = V(j)(i)
                        V(j)(i) = V(j)(k)
                        V(j)(k) = p
                    Next
                End If
            Next
        End Sub

        ' Nonsymmetric reduction to Hessenberg form.

        Private Sub orthes()
            '  This is derived from the Algol procedures orthes and ortran,
            '  by Martin and Wilkinson, Handbook for Auto. Comp.,
            '  Vol.ii-Linear Algebra, and the corresponding
            '  Fortran subroutines in EISPACK.

            Dim low As Integer = 0
            Dim high As Integer = n - 1
            Dim V = _V

            For m As Integer = low + 1 To high - 1

                ' Scale column.

                Dim scale As Double = 0.0
                For i As Integer = m To high
                    scale = scale + stdnum.Abs(H(i)(m - 1))
                Next
                If scale <> 0.0 Then

                    ' Compute Householder transformation.

                    Dim h__1 As Double = 0.0
                    For i As Integer = high To m Step -1
                        ort(i) = H(i)(m - 1) / scale
                        h__1 += ort(i) * ort(i)
                    Next
                    Dim g As Double = System.Math.Sqrt(h__1)
                    If ort(m) > 0 Then
                        g = -g
                    End If
                    h__1 = h__1 - ort(m) * g
                    ort(m) = ort(m) - g

                    ' Apply Householder similarity transformation
                    ' H = (I-u*u'/h)*H*(I-u*u')/h)

                    For j As Integer = m To n - 1
                        Dim f As Double = 0.0
                        For i As Integer = high To m Step -1
                            f += ort(i) * H(i)(j)
                        Next
                        f = f / h__1
                        For i As Integer = m To high
                            H(i)(j) -= f * ort(i)
                        Next
                    Next

                    For i As Integer = 0 To high
                        Dim f As Double = 0.0
                        For j As Integer = high To m Step -1
                            f += ort(j) * H(i)(j)
                        Next
                        f = f / h__1
                        For j As Integer = m To high
                            H(i)(j) -= f * ort(j)
                        Next
                    Next
                    ort(m) = scale * ort(m)
                    H(m)(m - 1) = scale * g
                End If
            Next

            ' Accumulate transformations (Algol's ortran).

            For i As Integer = 0 To n - 1
                For j As Integer = 0 To n - 1
                    V(i)(j) = (If(i = j, 1.0, 0.0))
                Next
            Next

            For m As Integer = high - 1 To low + 1 Step -1
                If H(m)(m - 1) <> 0.0 Then
                    For i As Integer = m + 1 To high
                        ort(i) = H(i)(m - 1)
                    Next
                    For j As Integer = m To high
                        Dim g As Double = 0.0
                        For i As Integer = m To high
                            g += ort(i) * V(i)(j)
                        Next
                        ' Double division avoids possible underflow
                        g = (g / ort(m)) / H(m)(m - 1)
                        For i As Integer = m To high
                            V(i)(j) += g * ort(i)
                        Next
                    Next
                End If
            Next
        End Sub

        ''' <summary>
        ''' Complex scalar division.
        ''' </summary>
        Dim cdivr As Double
        ''' <summary>
        ''' Complex scalar division.
        ''' </summary>
        Dim cdivi As Double

        Private Sub cdiv(xr As Double, xi As Double, yr As Double, yi As Double)
            Dim r As Double, d As Double
            If stdnum.Abs(yr) > stdnum.Abs(yi) Then
                r = yi / yr
                d = yr + r * yi
                cdivr = (xr + r * xi) / d
                cdivi = (xi - r * xr) / d
            Else
                r = yr / yi
                d = yi + r * yr
                cdivr = (r * xr + xi) / d
                cdivi = (r * xi - xr) / d
            End If
        End Sub


        ''' <summary>
        ''' Nonsymmetric reduction from Hessenberg to real Schur form.
        ''' </summary>
        Private Sub hqr2()
            '  This is derived from the Algol procedure hqr2,
            '  by Martin and Wilkinson, Handbook for Auto. Comp.,
            '  Vol.ii-Linear Algebra, and the corresponding
            '  Fortran subroutine in EISPACK.

            ' Initialize

            Dim V = _V
            Dim nn As Integer = Me.n
            Dim n As Integer = nn - 1
            Dim low As Integer = 0
            Dim high As Integer = nn - 1
            Dim eps As Double = System.Math.Pow(2.0, -52.0)
            Dim exshift As Double = 0.0
            Dim p As Double = 0, q As Double = 0, r As Double = 0, s As Double = 0, z As Double = 0, t As Double,
            w As Double, x As Double, y As Double

            ' Store roots isolated by balanc and compute matrix norm

            Dim norm As Double = 0.0
            For i As Integer = 0 To nn - 1
                If i < low Or i > high Then
                    m_d(i) = H(i)(i)
                    m_e(i) = 0.0
                End If
                For j As Integer = stdnum.Max(i - 1, 0) To nn - 1
                    norm = norm + stdnum.Abs(H(i)(j))
                Next
            Next

            ' Outer loop over eigenvalue index

            Dim iter As Integer = 0
            While n >= low

                ' Look for single small sub-diagonal element

                Dim l As Integer = n
                While l > low
                    s = stdnum.Abs(H(l - 1)(l - 1)) + stdnum.Abs(H(l)(l))
                    If s = 0.0 Then
                        s = norm
                    End If
                    If stdnum.Abs(H(l)(l - 1)) < eps * s Then
                        Exit While
                    End If
                    l -= 1
                End While

                ' Check for convergence
                ' One root found

                If l = n Then
                    H(n)(n) = H(n)(n) + exshift
                    m_d(n) = H(n)(n)
                    m_e(n) = 0.0
                    n -= 1

                    ' Two roots found
                    iter = 0
                ElseIf l = n - 1 Then
                    w = H(n)(n - 1) * H(n - 1)(n)
                    p = (H(n - 1)(n - 1) - H(n)(n)) / 2.0
                    q = p * p + w
                    z = stdnum.Sqrt(stdnum.Abs(q))
                    H(n)(n) = H(n)(n) + exshift
                    H(n - 1)(n - 1) = H(n - 1)(n - 1) + exshift
                    x = H(n)(n)

                    ' Real pair

                    If q >= 0 Then
                        If p >= 0 Then
                            z = p + z
                        Else
                            z = p - z
                        End If
                        m_d(n - 1) = x + z
                        m_d(n) = m_d(n - 1)
                        If z <> 0.0 Then
                            m_d(n) = x - w / z
                        End If
                        m_e(n - 1) = 0.0
                        m_e(n) = 0.0
                        x = H(n)(n - 1)
                        s = stdnum.Abs(x) + stdnum.Abs(z)
                        p = x / s
                        q = z / s
                        r = System.Math.Sqrt(p * p + q * q)
                        p = p / r
                        q = q / r

                        ' Row modification

                        For j As Integer = n - 1 To nn - 1
                            z = H(n - 1)(j)
                            H(n - 1)(j) = q * z + p * H(n)(j)
                            H(n)(j) = q * H(n)(j) - p * z
                        Next

                        ' Column modification

                        For i As Integer = 0 To n
                            z = H(i)(n - 1)
                            H(i)(n - 1) = q * z + p * H(i)(n)
                            H(i)(n) = q * H(i)(n) - p * z
                        Next

                        ' Accumulate transformations

                        For i As Integer = low To high
                            z = V(i)(n - 1)
                            V(i)(n - 1) = q * z + p * V(i)(n)
                            V(i)(n) = q * V(i)(n) - p * z

                            ' Complex pair
                        Next
                    Else
                        m_d(n - 1) = x + p
                        m_d(n) = x + p
                        m_e(n - 1) = z
                        m_e(n) = -z
                    End If
                    n = n - 2

                    ' No convergence yet
                    iter = 0
                Else

                    ' Form shift

                    x = H(n)(n)
                    y = 0.0
                    w = 0.0
                    If l < n Then
                        y = H(n - 1)(n - 1)
                        w = H(n)(n - 1) * H(n - 1)(n)
                    End If

                    ' Wilkinson's original ad hoc shift

                    If iter = 10 Then
                        exshift += x
                        For i As Integer = low To n
                            H(i)(i) -= x
                        Next
                        s = stdnum.Abs(H(n)(n - 1)) + stdnum.Abs(H(n - 1)(n - 2))
                        y = 0.75 * s
                        x = y
                        w = (-0.4375) * s * s
                    End If

                    ' MATLAB's new ad hoc shift

                    If iter = 30 Then
                        s = (y - x) / 2.0
                        s = s * s + w
                        If s > 0 Then
                            s = System.Math.Sqrt(s)
                            If y < x Then
                                s = -s
                            End If
                            s = x - w / ((y - x) / 2.0 + s)
                            For i As Integer = low To n
                                H(i)(i) -= s
                            Next
                            exshift += s
                            w = 0.964
                            y = w
                            x = y
                        End If
                    End If

                    iter = iter + 1
                    ' (Could check iteration count here.)
                    ' Look for two consecutive small sub-diagonal elements

                    Dim m As Integer = n - 2
                    While m >= l
                        z = H(m)(m)
                        r = x - z
                        s = y - z
                        p = (r * s - w) / H(m + 1)(m) + H(m)(m + 1)
                        q = H(m + 1)(m + 1) - z - r - s
                        r = H(m + 2)(m + 1)
                        s = stdnum.Abs(p) + stdnum.Abs(q) + stdnum.Abs(r)
                        p = p / s
                        q = q / s
                        r = r / s
                        If m = l Then
                            Exit While
                        End If
                        If stdnum.Abs(H(m)(m - 1)) * (stdnum.Abs(q) + stdnum.Abs(r)) < eps * (stdnum.Abs(p) * (stdnum.Abs(H(m - 1)(m - 1)) + stdnum.Abs(z) + stdnum.Abs(H(m + 1)(m + 1)))) Then
                            Exit While
                        End If
                        m -= 1
                    End While

                    For i As Integer = m + 2 To n
                        H(i)(i - 2) = 0.0
                        If i > m + 2 Then
                            H(i)(i - 3) = 0.0
                        End If
                    Next

                    ' Double QR step involving rows l:n and columns m:n

                    For k As Integer = m To n - 1
                        Dim notlast As Boolean = (k <> n - 1)
                        If k <> m Then
                            p = H(k)(k - 1)
                            q = H(k + 1)(k - 1)
                            r = (If(notlast, H(k + 2)(k - 1), 0.0))
                            x = stdnum.Abs(p) + stdnum.Abs(q) + stdnum.Abs(r)
                            If x <> 0.0 Then
                                p = p / x
                                q = q / x
                                r = r / x
                            End If
                        End If
                        If x = 0.0 Then
                            Exit For
                        End If
                        s = System.Math.Sqrt(p * p + q * q + r * r)
                        If p < 0 Then
                            s = -s
                        End If
                        If s <> 0 Then
                            If k <> m Then
                                H(k)(k - 1) = (-s) * x
                            ElseIf l <> m Then
                                H(k)(k - 1) = -H(k)(k - 1)
                            End If
                            p = p + s
                            x = p / s
                            y = q / s
                            z = r / s
                            q = q / p
                            r = r / p

                            ' Row modification

                            For j As Integer = k To nn - 1
                                p = H(k)(j) + q * H(k + 1)(j)
                                If notlast Then
                                    p = p + r * H(k + 2)(j)
                                    H(k + 2)(j) = H(k + 2)(j) - p * z
                                End If
                                H(k)(j) = H(k)(j) - p * x
                                H(k + 1)(j) = H(k + 1)(j) - p * y
                            Next

                            ' Column modification

                            For i As Integer = 0 To System.Math.Min(n, k + 3)
                                p = x * H(i)(k) + y * H(i)(k + 1)
                                If notlast Then
                                    p = p + z * H(i)(k + 2)
                                    H(i)(k + 2) = H(i)(k + 2) - p * r
                                End If
                                H(i)(k) = H(i)(k) - p
                                H(i)(k + 1) = H(i)(k + 1) - p * q
                            Next

                            ' Accumulate transformations

                            For i As Integer = low To high
                                p = x * V(i)(k) + y * V(i)(k + 1)
                                If notlast Then
                                    p = p + z * V(i)(k + 2)
                                    V(i)(k + 2) = V(i)(k + 2) - p * r
                                End If
                                V(i)(k) = V(i)(k) - p
                                V(i)(k + 1) = V(i)(k + 1) - p * q
                            Next
                            ' (s != 0)
                        End If
                        ' k loop
                    Next
                    ' check convergence
                End If
            End While
            ' while (n >= low)
            ' Backsubstitute to find vectors of upper triangular form

            If norm = 0.0 Then
                Return
            End If

            For n = nn - 1 To 0 Step -1
                p = m_d(n)
                q = m_e(n)

                ' Real vector

                If q = 0 Then
                    Dim l As Integer = n
                    H(n)(n) = 1.0
                    For i As Integer = n - 1 To 0 Step -1
                        w = H(i)(i) - p
                        r = 0.0
                        For j As Integer = l To n
                            r = r + H(i)(j) * H(j)(n)
                        Next
                        If m_e(i) < 0.0 Then
                            z = w
                            s = r
                        Else
                            l = i
                            If m_e(i) = 0.0 Then
                                If w <> 0.0 Then
                                    H(i)(n) = (-r) / w
                                Else
                                    H(i)(n) = (-r) / (eps * norm)

                                    ' Solve real equations
                                End If
                            Else
                                x = H(i)(i + 1)
                                y = H(i + 1)(i)
                                q = (m_d(i) - p) * (m_d(i) - p) + m_e(i) * m_e(i)
                                t = (x * s - z * r) / q
                                H(i)(n) = t
                                If stdnum.Abs(x) > stdnum.Abs(z) Then
                                    H(i + 1)(n) = (-r - w * t) / x
                                Else
                                    H(i + 1)(n) = (-s - y * t) / z
                                End If
                            End If

                            ' Overflow control

                            t = stdnum.Abs(H(i)(n))
                            If (eps * t) * t > 1 Then
                                For j As Integer = i To n
                                    H(j)(n) = H(j)(n) / t
                                Next
                            End If
                        End If

                        ' Complex vector
                    Next
                ElseIf q < 0 Then
                    Dim l As Integer = n - 1

                    ' Last vector component imaginary so matrix is triangular

                    If stdnum.Abs(H(n)(n - 1)) > stdnum.Abs(H(n - 1)(n)) Then
                        H(n - 1)(n - 1) = q / H(n)(n - 1)
                        H(n - 1)(n) = (-(H(n)(n) - p)) / H(n)(n - 1)
                    Else
                        cdiv(0.0, -H(n - 1)(n), H(n - 1)(n - 1) - p, q)
                        H(n - 1)(n - 1) = cdivr
                        H(n - 1)(n) = cdivi
                    End If
                    H(n)(n - 1) = 0.0
                    H(n)(n) = 1.0
                    For i As Integer = n - 2 To 0 Step -1
                        Dim ra As Double, sa As Double, vr As Double, vi As Double
                        ra = 0.0
                        sa = 0.0
                        For j As Integer = l To n
                            ra = ra + H(i)(j) * H(j)(n - 1)
                            sa = sa + H(i)(j) * H(j)(n)
                        Next
                        w = H(i)(i) - p

                        If m_e(i) < 0.0 Then
                            z = w
                            r = ra
                            s = sa
                        Else
                            l = i
                            If m_e(i) = 0 Then
                                cdiv(-ra, -sa, w, q)
                                H(i)(n - 1) = cdivr
                                H(i)(n) = cdivi
                            Else

                                ' Solve complex equations

                                x = H(i)(i + 1)
                                y = H(i + 1)(i)
                                vr = (m_d(i) - p) * (m_d(i) - p) + m_e(i) * m_e(i) - q * q
                                vi = (m_d(i) - p) * 2.0 * q
                                If vr = 0.0 And vi = 0.0 Then
                                    vr = eps * norm * (stdnum.Abs(w) + stdnum.Abs(q) + stdnum.Abs(x) + stdnum.Abs(y) + stdnum.Abs(z))
                                End If
                                cdiv(x * r - z * ra + q * sa, x * s - z * sa - q * ra, vr, vi)
                                H(i)(n - 1) = cdivr
                                H(i)(n) = cdivi
                                If stdnum.Abs(x) > (stdnum.Abs(z) + stdnum.Abs(q)) Then
                                    H(i + 1)(n - 1) = (-ra - w * H(i)(n - 1) + q * H(i)(n)) / x
                                    H(i + 1)(n) = (-sa - w * H(i)(n) - q * H(i)(n - 1)) / x
                                Else
                                    cdiv(-r - y * H(i)(n - 1), -s - y * H(i)(n), z, q)
                                    H(i + 1)(n - 1) = cdivr
                                    H(i + 1)(n) = cdivi
                                End If
                            End If

                            ' Overflow control

                            t = stdnum.Max(stdnum.Abs(H(i)(n - 1)), stdnum.Abs(H(i)(n)))
                            If (eps * t) * t > 1 Then
                                For j As Integer = i To n
                                    H(j)(n - 1) = H(j)(n - 1) / t
                                    H(j)(n) = H(j)(n) / t
                                Next
                            End If
                        End If
                    Next
                End If
            Next

            ' Vectors of isolated roots

            For i As Integer = 0 To nn - 1
                If i < low Or i > high Then
                    For j As Integer = i To nn - 1
                        V(i)(j) = H(i)(j)
                    Next
                End If
            Next

            ' Back transformation to get eigenvectors of original matrix

            For j As Integer = nn - 1 To low Step -1
                For i As Integer = low To high
                    z = 0.0
                    For k As Integer = low To System.Math.Min(j, high)
                        z = z + V(i)(k) * H(k)(j)
                    Next
                    V(i)(j) = z
                Next
            Next
        End Sub

#End Region


#Region "Constructor"

        ''' <summary>
        ''' Check for symmetry, then construct the eigenvalue decomposition, returns Structure to access D and V.
        ''' </summary>
        ''' <param name="Arg">Square matrix</param>
        Public Sub New(Arg As GeneralMatrix)
            Dim A As Double()() = Arg.ArrayPack
            n = Arg.ColumnDimension
            Dim V = New Double(n - 1)() {}
            For i As Integer = 0 To n - 1
                V(i) = New Double(n - 1) {}
            Next
            m_d = New Double(n - 1) {}
            m_e = New Double(n - 1) {}
            _V = V

            issymmetric = True
            Dim j As Integer = 0
            While (j < n) And issymmetric
                Dim i As Integer = 0
                While (i < n) And issymmetric
                    issymmetric = (A(i)(j) = A(j)(i))
                    i += 1
                End While
                j += 1
            End While

            If issymmetric Then
                For i As Integer = 0 To n - 1
                    For j = 0 To n - 1
                        V(i)(j) = A(i)(j)
                    Next
                Next

                ' Tridiagonalize.
                tred2()

                ' Diagonalize.
                tql2()
            Else
                H = New Double(n - 1)() {}
                For i2 As Integer = 0 To n - 1
                    H(i2) = New Double(n - 1) {}
                Next
                ort = New Double(n - 1) {}

                For j = 0 To n - 1
                    For i As Integer = 0 To n - 1
                        H(i)(j) = A(i)(j)
                    Next
                Next

                ' Reduce to Hessenberg form.
                orthes()

                ' Reduce Hessenberg to real Schur form.
                hqr2()
            End If

            _V = V
        End Sub

#End Region

#Region "Public Properties"

        ' return { lambda:R.getDiag(), E:E };

        ''' <summary>Return the real parts of the eigenvalues</summary>
        ''' <returns>     real(diag(D))
        ''' </returns>
        Public Overridable ReadOnly Property RealEigenvalues() As Double()
            Get
                Return m_d
            End Get
        End Property

        ''' <summary>Return the imaginary parts of the eigenvalues</summary>
        ''' <returns>     imag(diag(D))
        ''' </returns>
        Public Overridable ReadOnly Property ImagEigenvalues() As Double()
            Get
                Return m_e
            End Get
        End Property

        ''' <summary>Return the block diagonal eigenvalue matrix</summary>
        ''' <returns>     D
        ''' </returns>
        Public Overridable ReadOnly Property D() As NumericMatrix
            Get
                Dim X As New NumericMatrix(n, n)
                Dim Da As Double()() = X.Array
                For i As Integer = 0 To n - 1
                    For j As Integer = 0 To n - 1
                        Da(i)(j) = 0.0
                    Next
                    Da(i)(i) = m_d(i)
                    If m_e(i) > 0 Then
                        Da(i)(i + 1) = m_e(i)
                    ElseIf m_e(i) < 0 Then
                        Da(i)(i - 1) = m_e(i)
                    End If
                Next
                Return X
            End Get
        End Property
#End Region

#Region "Public Methods"

        ''' <summary>Return the eigenvector matrix</summary>
        ''' <returns>     V
        ''' </returns>
        Public ReadOnly Property V() As NumericMatrix
            Get
                Return New NumericMatrix(_V, n, n)
            End Get
        End Property
#End Region

    End Class
End Namespace

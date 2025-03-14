#Region "Microsoft.VisualBasic::b25bd2fb6ec6891ff1d736118cc5ba06, Data_science\Mathematica\Math\Math\Numerics\Optimization\LBFGSB\BKLDLT.vb"

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

    '   Total Lines: 464
    '    Code Lines: 351 (75.65%)
    ' Comment Lines: 24 (5.17%)
    '    - Xml Docs: 50.00%
    ' 
    '   Blank Lines: 89 (19.18%)
    '     File Size: 14.35 KB


    '     Class BKLDLT
    ' 
    ' 
    '         Enum Info
    ' 
    '             NOT_COMPUTED, NUMERICAL_ISSUE, SUCCESSFUL
    ' 
    ' 
    ' 
    '         Class Pair
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: ToString
    ' 
    '         Class IndexType
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    '  
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: coeff, diag_coeff, find_lambda, find_sigma, gaussian_elimination_1x1
    '               gaussian_elimination_2x2, index, permutate_mat, solve
    ' 
    '     Sub: compress_permutation, compute, compute_pointer, copy_data, interchange_rows
    '          pivoting_1x1, pivoting_2x2, solve_inplace, swap, swap_ranges
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace Framework.Optimization.LBFGSB

    ''' <summary>
    ''' Bunch-Kaufman LDLT decomposition
    ''' </summary>
	''' <remarks>
	''' https://lbfgspp.statr.me/doc/BKLDLT_8h_source.html
	''' </remarks>
    Public NotInheritable Class BKLDLT

        Friend Shared ReadOnly alpha As Double = (1.0 + std.Sqrt(17.0)) / 8.0

        Public Enum Info
            SUCCESSFUL
            NOT_COMPUTED
            NUMERICAL_ISSUE
        End Enum

        ''' <summary>
        ''' Compressed permutations
        ''' </summary>
        Public Class Pair
            Private ReadOnly outerInstance As BKLDLT

            Friend a, b As Integer

            Public Sub New(outerInstance As BKLDLT, a As Integer, b As Integer)
                Me.outerInstance = outerInstance
                Me.a = a
                Me.b = b
            End Sub

            Public Overrides Function ToString() As String
                Return "[" & a.ToString() & ", " & b.ToString() & "]"
            End Function
        End Class

        ''' <summary>
        ''' Index reference, mutable
        ''' </summary>		
        Public Class IndexType
            Private ReadOnly outerInstance As BKLDLT

            Friend v As Integer

            Public Sub New(outerInstance As BKLDLT, v As Integer)
                Me.outerInstance = outerInstance
                Me.v = v
            End Sub
        End Class

        Public Shared Sub swap(a As Double(), i As Integer, j As Integer)
            Dim t = a(i)
            a(i) = a(j)
            a(j) = t
        End Sub

        Public Shared Sub swap_ranges(a As Double(), start As Integer, [end] As Integer, target As Integer)
            Dim i = start, j = target

            While i < [end]
                swap(a, i, j)
                i += 1
                j += 1
            End While
        End Sub

        Public computed As Boolean = False
        Public infoField As Info = Info.NOT_COMPUTED

        Public n As Integer = 0
        Public data As Double() ' lower triangular matrix, column-wise
        Public colptr As Integer() ' indices of columns
        Public perm As Integer() ' permutations
        Public permc As List(Of Pair) ' compressed permutations

        ' index of value
        Public Function index(i As Integer, j As Integer) As Integer
            Return colptr(j) + (i - j)
        End Function

        ' value from matrix
        Public Function coeff(i As Integer, j As Integer) As Double
            Return data(colptr(j) + (i - j))
        End Function

        ' diagonal value
        Public Function diag_coeff(i As Integer) As Double
            Return data(colptr(i))
        End Function

        ' find and store column pointers
        Private Sub compute_pointer()
            colptr = New Integer(n - 1) {}

            Dim head = 0
            For i = 0 To n - 1
                colptr(i) = head
                head += n - i
            Next
        End Sub

        ' copy matrix data
        Private Sub copy_data(mat As Matrix)
            For j = 0 To n - 1
                Dim begin = mat.index(j, j)
                Dim len = n - j
                Array.Copy(mat.mat, begin, data, colptr(j), len)
            Next
        End Sub

        Private Sub compress_permutation()
            permc = New List(Of Pair)(n)

            For i = 0 To n - 1
                Dim idx = If(perm(i) >= 0, perm(i), -perm(i) - 1)
                If idx <> i Then
                    permc.Add(New Pair(Me, i, idx))
                End If
            Next
        End Sub

        Public Function find_lambda(k As Integer, r As IndexType) As Double
            Dim head = colptr(k)
            Dim [end] = colptr(k + 1)
            r.v = k + 1
            Dim lambda = std.Abs(data(head + 1))

            For ptr = head + 2 To [end] - 1
                Dim abs_elem = std.Abs(data(ptr))
                If lambda < abs_elem Then
                    lambda = abs_elem
                    r.v = k + (ptr - head)
                End If
            Next

            Return lambda
        End Function

        Public Function find_sigma(k As Integer, r As Integer, p As IndexType) As Double
            Dim sigma = -1.0

            If r < n - 1 Then
                sigma = find_lambda(r, p)
            End If

            For j = k To r - 1
                Dim abs_elem = std.Abs(coeff(r, j))
                If sigma < abs_elem Then
                    sigma = abs_elem
                    p.v = j
                End If
            Next

            Return sigma
        End Function

        Public Sub pivoting_1x1(k As Integer, r As Integer)
            If k <> r Then
                swap(data, colptr(k), colptr(r))
                swap_ranges(data, index(r + 1, k), colptr(k + 1), index(r + 1, r))
                Dim src = index(k + 1, k)
                Dim j = k + 1

                While j < r
                    swap(data, src, index(r, j))
                    j += 1
                    src += 1
                End While
            End If
            perm(k) = r
        End Sub

        Public Sub pivoting_2x2(k As Integer, r As Integer, p As Integer)
            pivoting_1x1(k, p)
            pivoting_1x1(k + 1, r)
            swap(data, index(k + 1, k), index(r, k))
            perm(k) = -perm(k) - 1
            perm(k + 1) = -perm(k + 1) - 1
        End Sub

        Public Sub interchange_rows(r1 As Integer, r2 As Integer, c1 As Integer, c2 As Integer)
            If r1 <> r2 Then
                For j = c1 To c2
                    swap(data, index(r1, j), index(r2, j))
                Next
            End If
        End Sub

        Public Function permutate_mat(k As Integer) As Boolean
            Dim r As IndexType = New IndexType(Me, k)
            Dim p As IndexType = New IndexType(Me, k)

            Dim lambda = find_lambda(k, r)

            If lambda > 0 Then

                Dim abs_akk = std.Abs(diag_coeff(k))
                Dim alambda = alpha * lambda

                If abs_akk < alambda Then

                    Dim sigma = find_sigma(k, r.v, p)

                    If sigma * abs_akk < alambda * lambda Then

                        If abs_akk >= alpha * sigma Then
                            pivoting_1x1(k, r.v)
                            interchange_rows(k, r.v, 0, k - 1)
                            Return True
                        Else
                            p = New IndexType(Me, k)
                            pivoting_2x2(k, r.v, p.v)
                            interchange_rows(k, p.v, 0, k - 1)
                            interchange_rows(k + 1, r.v, 0, k - 1)
                            Return False
                        End If

                    End If
                End If
            End If

            Return True
        End Function

        Public Function gaussian_elimination_1x1(k As Integer) As Info

            Dim akk = diag_coeff(k)

            If akk = 0.0 Then
                Return Info.NUMERICAL_ISSUE
            End If

            data(colptr(k)) = 1.0 / akk

            Dim lptr = colptr(k) + 1
            Dim ldim = n - k - 1

            '		MapVec l(lptr, ldim);
            '    for (Index j = 0; j < ldim; j++)
            '    {
            '        MapVec(col_pointer(j + k + 1), ldim - j).noalias() -= (lptr[j] / akk) * l.tail(ldim - j);
            '    }
            For j = 0 To ldim - 1
                Dim col = colptr(j + k + 1)
                Dim v1 = data(lptr + j) / akk
                For i = 0 To ldim - j - 1
                    Dim v = v1 * data(lptr + j + i) ' tail, idx of starting column = ldim-(ldim-j) = j
                    data(col + i) -= v
                Next
            Next

            For i = 0 To ldim - 1
                data(lptr + i) /= akk
            Next

            Return Info.SUCCESSFUL
        End Function

        Public Function gaussian_elimination_2x2(k As Integer) As Info

            Dim e11 = colptr(k)
            Dim e21 = index(k + 1, k)
            Dim e22 = colptr(k + 1)

            Dim de11 = data(e11)
            Dim de21 = data(e21)
            Dim de22 = data(e22)
            Dim delta = de11 * de22 - de21 * de21

            If delta = 0.0 Then
                Return Info.NUMERICAL_ISSUE
            End If

            ' inverse_inplace_2x2
            data(e11) = de22 / delta
            data(e21) = -de21 / delta
            data(e22) = de11 / delta

            de11 = data(e11)
            de21 = data(e21)
            de22 = data(e22)

            Dim l1ptr = index(k + 2, k)
            Dim l2ptr = index(k + 2, k + 1)
            Dim ldim = n - k - 2

            If ldim <> 0 Then
                Dim X0 = New Double(ldim - 1) {}
                Dim X1 = New Double(ldim - 1) {}

                For i = 0 To ldim - 1
                    X0(i) = data(l1ptr + i) * de11 + data(l2ptr + i) * de21
                    X1(i) = data(l1ptr + i) * de21 + data(l2ptr + i) * de22
                Next

                For j = 0 To ldim - 1
                    Dim col = colptr(j + k + 2)
                    For i = 0 To ldim - j - 1
                        Dim v = X0(j + i) * data(l1ptr + j) + X1(j + i) * data(l2ptr + j)
                        data(col + i) -= v
                    Next
                Next

                For i = 0 To ldim - 1
                    data(l1ptr + i) = X0(i)
                    data(l2ptr + i) = X1(i)
                Next
            End If

            Return Info.SUCCESSFUL
        End Function

        Public Sub compute(mat As Matrix)
            If Debug.flag Then
                Debug.debug("-"c, "compute BKLDLT")
            End If
            If Debug.flag Then
                Debug.debug("mat: ", mat)
            End If

            n = mat.rows
            perm = New Integer(n - 1) {}

            ' setLinSpaced
            For i = 0 To n - 1
                perm(i) = i
            Next

            data = New Double(n * (n + 1) / 2 - 1) {}
            compute_pointer()
            copy_data(mat)

            Dim k As Integer
            For k = 0 To n - 1 - 1
                Dim is_1x1 = permutate_mat(k)
                If is_1x1 Then
                    infoField = gaussian_elimination_1x1(k)
                Else
                    infoField = gaussian_elimination_2x2(k)
                    k += 1
                End If

                If infoField <> Info.SUCCESSFUL Then
                    Exit For
                End If
            Next

            If k = n - 1 Then
                Dim akk = diag_coeff(k)
                If akk = 0.0 Then
                    infoField = Info.NUMERICAL_ISSUE
                    data(colptr(k)) = Double.NaN
                Else
                    data(colptr(k)) = 1.0 / data(colptr(k))
                End If
            End If

            compress_permutation()

            computed = True

            If Debug.flag Then
                Debug.debug("-"c, "compute BKLDLT - end")
            End If
        End Sub

        Public Sub solve_inplace(b As Double())
            If Debug.flag Then
                Debug.debug("-"c, "solve BKLDLT")
            End If
            If Debug.flag Then
                Debug.debug("b: ", b)
            End If

            For Each p In permc
                swap(b, p.a, p.b)
            Next

            Dim [end] = If(perm(n - 1) < 0, n - 3, n - 2)
            Dim i As Integer
            For i = 0 To [end]
                Dim b1size = n - i - 1
                Dim b2size = b1size - 1
                If perm(i) >= 0 Then
                    Dim l = index(i + 1, i)
                    For k = 0 To b1size - 1
                        b(i + 1 + k) -= data(l + k) * b(i)
                    Next
                Else
                    Dim l1 = index(i + 2, i)
                    Dim l2 = index(i + 2, i + 1)
                    For k = 0 To b2size - 1
                        b(i + 2 + k) -= data(l1 + k) * b(i) + data(l2 + k) * b(i + 1)
                    Next
                    i += 1
                End If
            Next

            For i = 0 To n - 1
                Dim e11 = diag_coeff(i)

                If perm(i) >= 0 Then
                    b(i) *= e11
                Else
                    Dim e21 = coeff(i + 1, i)
                    Dim e22 = diag_coeff(i + 1)
                    Dim wi = b(i) * e11 + b(i + 1) * e21
                    b(i + 1) = b(i) * e21 + b(i + 1) * e22
                    b(i) = wi
                    i += 1
                End If
            Next

            i = If(perm(n - 1) < 0, n - 3, n - 2)
            While i >= 0
                Dim ldim = n - i - 1
                Dim l = index(i + 1, i)
                Dim dot1 = 0.0
                For k = 0 To ldim - 1
                    dot1 += b(i + 1 + k) * data(l + k)
                Next
                b(i) -= dot1

                If perm(i) < 0 Then
                    Dim l2 = index(i + 1, i - 1)
                    Dim dot2 = 0.0
                    For k = 0 To ldim - 1
                        dot2 += b(i + 1 + k) * data(l2 + k)
                    Next
                    b(i - 1) -= dot2
                    i -= 1
                End If

                i -= 1
            End While

            For i = permc.Count - 1 To 0 Step -1
                Dim p = permc(i)
                swap(b, p.a, p.b)
            Next

            If Debug.flag Then
                Debug.debug("-"c, "solve BKLDLT - end")
            End If
        End Sub

        Public Function solve(b As Double()) As Double()
            Dim copy As Double() = CType(b.Clone(), Double())
            solve_inplace(copy)
            Return copy
        End Function

        Public Sub New(mat As Matrix)
            compute(mat)
        End Sub

        Public Sub New()
        End Sub
    End Class

End Namespace

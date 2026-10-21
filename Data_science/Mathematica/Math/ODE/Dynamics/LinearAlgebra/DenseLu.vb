Namespace Dynamics

    ''' <summary>
    ''' A pre-allocated dense LU decomposition with partial pivoting, it is used by the implicit
    ''' integrators for solving the linear system ``W Δ = −G`` of the Newton iterations.
    ''' (隐式求解器的 Newton 迭代所使用的预分配稠密 LU 分解)
    ''' </summary>
    ''' <remarks>
    ''' 分解与回代全部在原位缓冲上进行，除了构造的时候分配的 ``N × N`` 矩阵之外不会产生
    ''' 任何额外的内存分配；状态变量的个数 ``N`` 在生物动力学模型之中通常不超过 50，
    ''' 因此稠密 LU 完全足够。
    ''' 
    ''' 使用方式：
    ''' 
    ''' ```vbnet
    ''' If lu.Factor(iterationMatrix) Then
    '''     Call lu.Solve(rhs, delta)   ' delta = W⁻¹ rhs
    ''' End If
    ''' ```
    ''' </remarks>
    Friend NotInheritable Class DenseLu

        ''' <summary>
        ''' The dimension of the linear system.(线性系统的维度)
        ''' </summary>
        Friend ReadOnly Size As Integer

        ''' <summary>
        ''' The in-place LU factors of the last factorized matrix.(最近一次分解的 LU 因子)
        ''' </summary>
        Friend ReadOnly lu As Double(,)

        ''' <summary>
        ''' The row permutation which is produced by the partial pivoting.(部分主元选择产生的行置换)
        ''' </summary>
        Friend ReadOnly pivot As Integer()

        ''' <summary>
        ''' Create the LU workspace of a ``size × size`` linear system.
        ''' </summary>
        ''' <param name="size"></param>
        Friend Sub New(size As Integer)
            If size < 1 Then
                Throw New ArgumentOutOfRangeException(NameOf(size), "The dimension of the linear system must be positive!")
            End If

            Me.Size = size
            Me.lu = New Double(size - 1, size - 1) {}
            Me.pivot = New Integer(size - 1) {}
        End Sub

        ''' <summary>
        ''' Factor the given matrix into the internal <see cref="lu"/> buffer by the partial
        ''' pivoting Gaussian elimination.
        ''' </summary>
        ''' <param name="a">The square matrix which will not be modified.</param>
        ''' <returns>
        ''' ``False`` when the matrix is singular (or numerically singular), the caller should
        ''' reduce the step size in that case.
        ''' </returns>
        Friend Function Factor(a As Double(,)) As Boolean
            Dim n As Integer = Size

            Call System.Array.Copy(a, lu, a.Length)

            For i As Integer = 0 To n - 1
                pivot(i) = i
            Next

            For k As Integer = 0 To n - 1
                Dim maxRow As Integer = k
                Dim maxValue As Double = System.Math.Abs(lu(k, k))

                For i As Integer = k + 1 To n - 1
                    Dim value As Double = System.Math.Abs(lu(i, k))

                    If value > maxValue Then
                        maxValue = value
                        maxRow = i
                    End If
                Next

                If maxValue <= 1.0E-300 Then
                    ' 数值奇异：该矩阵不可分解
                    Return False
                End If

                If maxRow <> k Then
                    For j As Integer = 0 To n - 1
                        Dim tmp As Double = lu(k, j)

                        lu(k, j) = lu(maxRow, j)
                        lu(maxRow, j) = tmp
                    Next

                    Dim p As Integer = pivot(k)

                    pivot(k) = pivot(maxRow)
                    pivot(maxRow) = p
                End If

                Dim diagonal As Double = lu(k, k)

                For i As Integer = k + 1 To n - 1
                    Dim multiplier As Double = lu(i, k) / diagonal

                    lu(i, k) = multiplier

                    If multiplier <> 0.0 Then
                        For j As Integer = k + 1 To n - 1
                            lu(i, j) -= multiplier * lu(k, j)
                        Next
                    End If
                Next
            Next

            Return True
        End Function

        ''' <summary>
        ''' Solve ``A x = b`` with the factorized matrix.
        ''' </summary>
        ''' <param name="rhs">The right hand side vector, it will not be modified.</param>
        ''' <param name="x">The solution buffer, it should not be the same buffer as <paramref name="rhs"/>.</param>
        Friend Sub Solve(rhs As Double(), x As Double())
            Dim n As Integer = Size

            For i As Integer = 0 To n - 1
                x(i) = rhs(pivot(i))
            Next

            ' 前向替换：L 为单位下三角
            For i As Integer = 0 To n - 1
                Dim sum As Double = x(i)

                For j As Integer = 0 To i - 1
                    sum -= lu(i, j) * x(j)
                Next

                x(i) = sum
            Next

            ' 后向替换
            For i As Integer = n - 1 To 0 Step -1
                Dim sum As Double = x(i)

                For j As Integer = i + 1 To n - 1
                    sum -= lu(i, j) * x(j)
                Next

                x(i) = sum / lu(i, i)
            Next
        End Sub
    End Class
End Namespace

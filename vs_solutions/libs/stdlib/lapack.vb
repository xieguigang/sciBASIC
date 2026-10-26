Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis
Imports TTest = Microsoft.VisualBasic.Math.Statistics.Hypothesis.t
Imports AltHypothesis = Microsoft.VisualBasic.Math.Statistics.Hypothesis.Hypothesis

''' <summary>
''' 线性代数模块：转置、乘法、线性方程组求解、求逆、行列式，以及保留的 t 检验入口。
''' <para>
''' 矩阵统一使用行主序的交错数组（<c>Double()()</c>），与二维表、绘图等接口保持一致。
''' </para>
''' </summary>
Public Module lapack

    ''' <summary>矩阵转置。</summary>
    Public Function transpose(m As Double()()) As Double()()
        If m Is Nothing OrElse m.Length = 0 Then
            Throw New ArgumentException("the matrix is empty", NameOf(m))
        End If

        Dim rows As Integer = m.Length
        Dim cols As Integer = m(0).Length
        Dim out(cols - 1)() As Double

        For j As Integer = 0 To cols - 1
            Dim column(rows - 1) As Double

            For i As Integer = 0 To rows - 1
                column(i) = m(i)(j)
            Next

            out(j) = column
        Next

        Return out
    End Function

    ''' <summary>矩阵乘法 A(m×k) · B(k×n)。</summary>
    Public Function multiply(a As Double()(), b As Double()()) As Double()()
        Dim m As Integer = a.Length
        Dim k As Integer = a(0).Length
        Dim n As Integer = b(0).Length

        If b.Length <> k Then
            Throw New ArgumentException($"the matrix dimensions do not agree: {m}x{k} times {b.Length}x{n}")
        End If

        Dim out(m - 1)() As Double

        For i As Integer = 0 To m - 1
            Dim row(n - 1) As Double

            For j As Integer = 0 To n - 1
                Dim acc As Double = 0

                For t As Integer = 0 To k - 1
                    acc += a(i)(t) * b(t)(j)
                Next

                row(j) = acc
            Next

            out(i) = row
        Next

        Return out
    End Function

    ''' <summary>解线性方程组 A · x = b（高斯消元 + 部分主元），返回解向量。</summary>
    Public Function solve(a As Double()(), b As Double()) As Double()
        Dim n As Integer = a.Length

        If b.Length <> n Then
            Throw New ArgumentException("the right hand side length should equals the matrix rank", NameOf(b))
        End If

        Dim m(n - 1)() As Double
        Dim rhs(n - 1) As Double

        For i As Integer = 0 To n - 1
            m(i) = CType(a(i).Clone(), Double())
            rhs(i) = b(i)
        Next

        For col As Integer = 0 To n - 1
            Dim pivot As Integer = col
            Dim maxValue As Double = System.Math.Abs(m(col)(col))

            For r As Integer = col + 1 To n - 1
                Dim v As Double = System.Math.Abs(m(r)(col))

                If v > maxValue Then
                    maxValue = v
                    pivot = r
                End If
            Next

            If maxValue < 1.0E-14 Then
                Throw New InvalidOperationException("the matrix is singular or nearly singular")
            End If

            If pivot <> col Then
                Dim tmpRow As Double() = m(col)
                m(col) = m(pivot)
                m(pivot) = tmpRow

                Dim tmpValue As Double = rhs(col)
                rhs(col) = rhs(pivot)
                rhs(pivot) = tmpValue
            End If

            For r As Integer = col + 1 To n - 1
                Dim factor As Double = m(r)(col) / m(col)(col)

                If factor = 0 Then
                    Continue For
                End If

                For c As Integer = col To n - 1
                    m(r)(c) -= factor * m(col)(c)
                Next

                rhs(r) -= factor * rhs(col)
            Next
        Next

        Dim x(n - 1) As Double

        For i As Integer = n - 1 To 0 Step -1
            Dim acc As Double = rhs(i)

            For j As Integer = i + 1 To n - 1
                acc -= m(i)(j) * x(j)
            Next

            x(i) = acc / m(i)(i)
        Next

        Return x
    End Function

    ''' <summary>矩阵求逆（逐列求解线性方程组）。</summary>
    Public Function inverse(a As Double()()) As Double()()
        Dim n As Integer = a.Length
        Dim columns(n - 1)() As Double

        For j As Integer = 0 To n - 1
            Dim unit(n - 1) As Double
            unit(j) = 1

            columns(j) = solve(a, unit)
        Next

        Return transpose(columns)
    End Function

    ''' <summary>行列式（高斯消元，主元乘积）。</summary>
    Public Function determinant(a As Double()()) As Double
        Dim n As Integer = a.Length
        Dim m(n - 1)() As Double

        For i As Integer = 0 To n - 1
            m(i) = CType(a(i).Clone(), Double())
        Next

        Dim det As Double = 1

        For col As Integer = 0 To n - 1
            Dim pivot As Integer = col
            Dim maxValue As Double = System.Math.Abs(m(col)(col))

            For r As Integer = col + 1 To n - 1
                Dim v As Double = System.Math.Abs(m(r)(col))

                If v > maxValue Then
                    maxValue = v
                    pivot = r
                End If
            Next

            If maxValue < 1.0E-14 Then
                Return 0
            End If

            If pivot <> col Then
                Dim tmpRow As Double() = m(col)
                m(col) = m(pivot)
                m(pivot) = tmpRow
                det = -det
            End If

            det *= m(col)(col)

            For r As Integer = col + 1 To n - 1
                Dim factor As Double = m(r)(col) / m(col)(col)

                For c As Integer = col To n - 1
                    m(r)(c) -= factor * m(col)(c)
                Next
            Next
        Next

        Return det
    End Function

    ''' <summary>单位矩阵。</summary>
    Public Function identity(n As Integer) As Double()()
        Dim out(n - 1)() As Double

        For i As Integer = 0 To n - 1
            Dim row(n - 1) As Double
            row(i) = 1
            out(i) = row
        Next

        Return out
    End Function

    ''' <summary>双样本 t 检验（保留的兼容入口，与 <c>stats.t_test</c> 等价）。</summary>
    Public Function t_test(x As Double(), y As Double(), Optional alpha As Double = 0.05) As TwoSampleResult
        Return TTest.Test(x, y, alternative:=AltHypothesis.TwoSided, alpha:=alpha)
    End Function
End Module

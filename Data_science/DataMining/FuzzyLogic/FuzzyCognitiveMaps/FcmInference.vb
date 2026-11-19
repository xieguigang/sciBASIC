Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports std = System.Math

Namespace FuzzyCognitiveMaps

    ''' <summary>FCM 权重学习：Hebbian 增量与岭回归最小二乘两条路径。</summary>
    ''' <remarks>
    ''' 输入是一段连续激活序列 <c>c(0), c(1), ...</c>，其中 <c>c(t+1)</c> 由 <c>c(t)</c> 驱动：
    ''' <c>c_i(t+1) = Σ_j w_ji · c_j(t)</c>。缺失样本以 <c>Nothing</c> 表示并跳过。
    ''' </remarks>
    Public NotInheritable Class FcmInference

        Private ReadOnly _ridge As Double

        ''' <summary>以岭回归正则化系数构造推断器。</summary>
        Public Sub New(Optional ridge As Double = 1e-6)
            If ridge < 0.0 OrElse Double.IsNaN(ridge) Then
                Throw New ArgumentOutOfRangeException(NameOf(ridge), ridge, "ridge must be non-negative")
            End If

            _ridge = ridge
        End Sub

        ''' <summary>岭回归系数。</summary>
        Public ReadOnly Property Ridge As Double
            Get
                Return _ridge
            End Get
        End Property

        ''' <summary>
        ''' Hebbian 学习：<c>w_ij += rate · c_i(t) · c_j(t-1)</c>，随后按最大绝对值归一化。
        ''' </summary>
        ''' <returns>学习到的权重矩阵（source × target）。</returns>
        Public Function FitHebbian(sequence As List(Of Double()), conceptCount As Integer,
                                   Optional rate As Double = 0.1,
                                   Optional maxWeight As Double = 1.0) As Double(,)
            Call ValidateSequence(sequence, conceptCount)
            If rate <= 0.0 Then
                Throw New ArgumentOutOfRangeException(NameOf(rate), rate, "rate must be positive")
            End If
            If maxWeight <= 0.0 Then
                Throw New ArgumentOutOfRangeException(NameOf(maxWeight), maxWeight, "max weight must be positive")
            End If

            Dim weights(conceptCount - 1, conceptCount - 1) As Double

            For t As Integer = 1 To sequence.Count - 1
                Dim before As Double() = sequence(t - 1)
                Dim after As Double() = sequence(t)

                If before Is Nothing OrElse after Is Nothing Then
                    Continue For
                End If

                For source As Integer = 0 To conceptCount - 1
                    For target As Integer = 0 To conceptCount - 1
                        If source = target Then
                            Continue For
                        End If

                        weights(source, target) += rate * before(source) * after(target)
                    Next
                Next
            Next

            Dim peak As Double = 0.0

            For i As Integer = 0 To conceptCount - 1
                For j As Integer = 0 To conceptCount - 1
                    peak = std.Max(peak, std.Abs(weights(i, j)))
                Next
            Next

            If peak > maxWeight Then
                Dim scale As Double = maxWeight / peak

                For i As Integer = 0 To conceptCount - 1
                    For j As Integer = 0 To conceptCount - 1
                        weights(i, j) *= scale
                    Next
                Next
            End If

            Return weights
        End Function

        ''' <summary>
        ''' 岭回归最小二乘：对每个目标概念求解 <c>c_i(t+1) = Σ_j w_ji · c_j(t)</c>。
        ''' </summary>
        ''' <returns>学习到的权重矩阵（source × target）。</returns>
        Public Function FitLeastSquares(sequence As List(Of Double()), conceptCount As Integer) As Double(,)
            Call ValidateSequence(sequence, conceptCount)

            Dim weights(conceptCount - 1, conceptCount - 1) As Double
            Dim samples As New List(Of Tuple(Of Double(), Double()))()

            For t As Integer = 1 To sequence.Count - 1
                Dim before As Double() = sequence(t - 1)
                Dim after As Double() = sequence(t)

                If before Is Nothing OrElse after Is Nothing Then
                    Continue For
                End If

                Call samples.Add(Tuple.Create(before, after))
            Next

            For target As Integer = 0 To conceptCount - 1
                Dim solution As Double() = SolveRidge(samples, target, conceptCount)

                For source As Integer = 0 To conceptCount - 1
                    weights(source, target) = solution(source)
                Next
            Next

            Return weights
        End Function

        ''' <summary>对单个目标求解岭回归正规方程（高斯消元 + 部分主元）。</summary>
        Private Function SolveRidge(samples As List(Of Tuple(Of Double(), Double())),
                                    target As Integer, dimension As Integer) As Double()
            Dim ata(dimension - 1, dimension - 1) As Double
            Dim atb(dimension - 1) As Double

            For Each sample As Tuple(Of Double(), Double()) In samples
                Dim x As Double() = sample.Item1
                Dim y As Double = sample.Item2(target)

                For i As Integer = 0 To dimension - 1
                    atb(i) += x(i) * y

                    For j As Integer = 0 To dimension - 1
                        ata(i, j) += x(i) * x(j)
                    Next
                Next
            Next

            For i As Integer = 0 To dimension - 1
                ata(i, i) += _ridge
            Next

            Return GaussianSolve(ata, atb)
        End Function

        ''' <summary>高斯消元求解稠密线性方程组。</summary>
        Private Shared Function GaussianSolve(a As Double(,), b As Double()) As Double()
            Dim n As Integer = b.Length
            Dim m As Double(,) = DirectCast(a.Clone(), Double(,))
            Dim rhs(n - 1) As Double

            Array.Copy(b, rhs, n)

            For col As Integer = 0 To n - 1
                Dim pivot As Integer = col

                For row As Integer = col + 1 To n - 1
                    If std.Abs(m(row, col)) > std.Abs(m(pivot, col)) Then
                        pivot = row
                    End If
                Next

                If std.Abs(m(pivot, col)) < 1e-12 Then
                    Continue For
                End If

                If pivot <> col Then
                    For j As Integer = 0 To n - 1
                        Dim temp As Double = m(col, j)
                        m(col, j) = m(pivot, j)
                        m(pivot, j) = temp
                    Next

                    Dim t As Double = rhs(col)
                    rhs(col) = rhs(pivot)
                    rhs(pivot) = t
                End If

                For row As Integer = col + 1 To n - 1
                    Dim factor As Double = m(row, col) / m(col, col)

                    For j As Integer = col To n - 1
                        m(row, j) -= factor * m(col, j)
                    Next

                    rhs(row) -= factor * rhs(col)
                Next
            Next

            Dim result(n - 1) As Double

            For i As Integer = n - 1 To 0 Step -1
                If std.Abs(m(i, i)) < 1e-12 Then
                    result(i) = 0.0
                    Continue For
                End If

                Dim sum As Double = rhs(i)

                For j As Integer = i + 1 To n - 1
                    sum -= m(i, j) * result(j)
                Next

                result(i) = sum / m(i, i)
            Next

            Return result
        End Function

        Private Shared Sub ValidateSequence(sequence As List(Of Double()), conceptCount As Integer)
            If sequence Is Nothing Then
                Throw New ArgumentNullException(NameOf(sequence))
            End If
            If conceptCount <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(conceptCount))
            End If
            If sequence.Count < 2 Then
                Throw New ArgumentException("at least two activation vectors are required", NameOf(sequence))
            End If

            For Each vector As Double() In sequence
                If vector IsNot Nothing AndAlso vector.Length <> conceptCount Then
                    Throw New ArgumentException($"activation vector length must be {conceptCount}", NameOf(sequence))
                End If
            Next
        End Sub
    End Class

End Namespace

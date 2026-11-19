#Region "Microsoft.VisualBasic::d6e5b4a2f7c9481ab0c3e5f42a9d7103, Data_science\Mathematica\Math\Math\Algebra\Matrix.NET\Decomposition\DNMF.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU GENERAL PUBLIC LICENSE for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.Tqdm

Namespace LinearAlgebra.Matrix

    ''' <summary>
    ''' Discriminative Non-negative Matrix Factorization (DNMF) with Fisher constraints.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 目标函数：<c>J = 1/2||A - WH||_F^2 + lambdaW/2*tr(W^T Sw W) - lambdaB/2*tr(W^T Sb W)</c>。
    ''' 其中 <c>Sw</c> 为类内散度矩阵，<c>Sb</c> 为类间散度矩阵，二者由当前样本表示 W
    ''' 按类别标签计算，并按迹归一化以避免尺度退化。
    ''' </para>
    ''' <para>
    ''' 与 <see cref="NMF"/> 保持一致的数据约定：输入 <c>A</c> 为 m 行 n 列（行 = 样本，列 = 特征），
    ''' 输出 <c>W</c> 为 m×k 的样本系数矩阵，<c>H</c> 为 k×n 的基矩阵。散度矩阵定义在
    ''' k 维表示空间上，因此对 W 的判别梯度项为 <c>W * S</c>（m×k）。
    ''' </para>
    ''' <para>
    ''' 散度矩阵按元素拆分为正部与负部：<c>S = S+ - S-</c>。更新公式：
    ''' <c>W &lt;- W * (AH^T + lambdaW*W*Sw- + lambdaB*W*Sb+ + eps) / (WHH^T + lambdaW*W*Sw+ + lambdaB*W*Sb- + eps)</c>，
    ''' <c>H &lt;- H * (W^T A + eps) / (W^T W H + eps)</c>。所有项非负，因而因子保持非负。
    ''' </para>
    ''' </remarks>
    Public Class DNMF

        Public Property W As NumericMatrix
        Public Property H As NumericMatrix
        Public Property cost As Double
        Public Property errors As Double()
        Public Property rank As Integer

        ''' <summary>
        ''' Factorize a non-negative matrix under Fisher discriminative constraints.
        ''' </summary>
        ''' <param name="A">m×n non-negative data matrix, rows are samples.</param>
        ''' <param name="labels">Length-m class labels, one label per sample (row).</param>
        ''' <param name="k">Number of latent components.</param>
        ''' <param name="max_iterations">Maximum number of multiplicative update iterations.</param>
        ''' <param name="tolerance">Relative cost change threshold for early stopping.</param>
        ''' <param name="epsilon">Positive stabilizer added to numerators and denominators.</param>
        ''' <param name="lambdaWithin">Weight of the within-class scatter penalty.</param>
        ''' <param name="lambdaBetween">Weight of the between-class scatter bonus.</param>
        ''' <param name="randomSeed">Optional seed for reproducible initialization.</param>
        ''' <param name="tqdm">Whether to display a progress bar.</param>
        ''' <returns>The fitted DNMF model.</returns>
        Public Shared Function Factorisation(A As NumericMatrix,
                                             labels As Integer(),
                                             Optional k As Integer = 2,
                                             Optional max_iterations As Integer = 1000,
                                             Optional tolerance As Double = 0.001,
                                             Optional epsilon As Double = 0.0001,
                                             Optional lambdaWithin As Double = 0.1,
                                             Optional lambdaBetween As Double = 0.1,
                                             Optional randomSeed As Integer? = Nothing,
                                             Optional tqdm As Boolean = True) As DNMF

            Call ValidateArguments(A, labels, k, max_iterations, tolerance, epsilon, lambdaWithin, lambdaBetween)

            Dim m As Integer = A.RowDimension
            Dim n As Integer = A.ColumnDimension
            Dim rnd As New Random(If(randomSeed.HasValue, randomSeed.Value, Environment.TickCount))
            Dim W As NumericMatrix = InitializePositive(m, k, rnd)
            Dim H As NumericMatrix = InitializePositive(k, n, rnd)
            Dim groups As Dictionary(Of Integer, List(Of Integer)) = GroupLabels(labels)
            Dim errors As New List(Of Double)
            Dim cost As Double = 0
            Dim previousCost As Double = Double.PositiveInfinity
            Dim bar As Tqdm.ProgressBar = Nothing

            If tqdm Then
                For Each i As Integer In TqdmWrapper.Range(0, max_iterations, bar:=bar, wrap_console:=App.EnableTqdm)
                    Call UpdateOnce(W, H, A, groups, lambdaWithin, lambdaBetween, epsilon, cost)
                    Call errors.Add(cost)

                    Dim converged As Boolean = Double.IsFinite(previousCost) AndAlso
                                                   Global.System.Math.Abs(cost - previousCost) <= tolerance * (1 + Global.System.Math.Abs(previousCost))
                    previousCost = cost

                    If converged Then
                        Exit For
                    End If

                    If bar IsNot Nothing Then
                        Call bar.SetLabel(cost)
                    End If
                Next
            Else
                For i As Integer = 1 To max_iterations
                    Call UpdateOnce(W, H, A, groups, lambdaWithin, lambdaBetween, epsilon, cost)
                    Call errors.Add(cost)

                    Dim converged As Boolean = Double.IsFinite(previousCost) AndAlso
                                                   Global.System.Math.Abs(cost - previousCost) <= tolerance * (1 + Global.System.Math.Abs(previousCost))
                    previousCost = cost

                    If converged Then
                        Exit For
                    End If
                Next
            End If

            Return New DNMF With {
                .cost = cost,
                .errors = errors.ToArray,
                .H = H,
                .rank = k,
                .W = W
            }
        End Function

        Private Shared Sub ValidateArguments(A As NumericMatrix,
                                             labels As Integer(),
                                             k As Integer,
                                             max_iterations As Integer,
                                             tolerance As Double,
                                             epsilon As Double,
                                             lambdaWithin As Double,
                                             lambdaBetween As Double)

            If A Is Nothing Then
                Throw New ArgumentNullException(NameOf(A))
            End If

            If labels Is Nothing Then
                Throw New ArgumentNullException(NameOf(labels))
            End If

            If labels.Length <> A.RowDimension Then
                Throw New ArgumentException($"labels length ({labels.Length}) must match the number of samples ({A.RowDimension}).", NameOf(labels))
            End If

            If k < 1 OrElse k > Global.System.Math.Min(A.RowDimension, A.ColumnDimension) Then
                Throw New ArgumentException($"k must be in [1, min(rows, columns)] = [1, {Global.System.Math.Min(A.RowDimension, A.ColumnDimension)}].", NameOf(k))
            End If

            If max_iterations < 1 Then
                Throw New ArgumentException("max_iterations must be positive.", NameOf(max_iterations))
            End If

            If tolerance < 0 OrElse Not Double.IsFinite(tolerance) Then
                Throw New ArgumentException("tolerance must be a finite non-negative value.", NameOf(tolerance))
            End If

            If epsilon <= 0 OrElse Not Double.IsFinite(epsilon) Then
                Throw New ArgumentException("epsilon must be a finite positive value.", NameOf(epsilon))
            End If

            If lambdaWithin < 0 OrElse Not Double.IsFinite(lambdaWithin) Then
                Throw New ArgumentException("lambdaWithin must be a finite non-negative value.", NameOf(lambdaWithin))
            End If

            If lambdaBetween < 0 OrElse Not Double.IsFinite(lambdaBetween) Then
                Throw New ArgumentException("lambdaBetween must be a finite non-negative value.", NameOf(lambdaBetween))
            End If

            For i As Integer = 0 To A.RowDimension - 1
                For j As Integer = 0 To A.ColumnDimension - 1
                    Dim value As Double = A(i, j)

                    If value < 0 OrElse Not Double.IsFinite(value) Then
                        Throw New ArgumentException($"A({i},{j}) must be finite and non-negative.", NameOf(A))
                    End If
                Next
            Next
        End Sub

        Private Shared Function GroupLabels(labels As Integer()) As Dictionary(Of Integer, List(Of Integer))
            Dim groups As New Dictionary(Of Integer, List(Of Integer))

            For i As Integer = 0 To labels.Length - 1
                Dim label As Integer = labels(i)

                If Not groups.ContainsKey(label) Then
                    Call groups.Add(label, New List(Of Integer))
                End If

                Call groups(label).Add(i)
            Next

            If groups.Count < 2 Then
                Throw New ArgumentException("DNMF requires at least two distinct class labels.", NameOf(labels))
            End If

            Return groups
        End Function

        Private Shared Function InitializePositive(rows As Integer, columns As Integer, rnd As Random) As NumericMatrix
            Dim matrix As New NumericMatrix(rows, columns)

            For i As Integer = 0 To rows - 1
                For j As Integer = 0 To columns - 1
                    matrix(i, j) = 0.5 + rnd.NextDouble() * 0.5
                Next
            Next

            Return matrix
        End Function

        Private Shared Sub UpdateOnce(ByRef W As NumericMatrix,
                                      ByRef H As NumericMatrix,
                                      A As NumericMatrix,
                                      groups As Dictionary(Of Integer, List(Of Integer)),
                                      lambdaWithin As Double,
                                      lambdaBetween As Double,
                                      epsilon As Double,
                                      ByRef cost As Double)

            Dim within As NumericMatrix = Nothing
            Dim between As NumericMatrix = Nothing

            Call BuildScatter(W, groups, within, between)

            Dim withinPositive As NumericMatrix = PositivePart(within, negated:=False)
            Dim withinNegative As NumericMatrix = PositivePart(within, negated:=True)
            Dim betweenPositive As NumericMatrix = PositivePart(between, negated:=False)
            Dim betweenNegative As NumericMatrix = PositivePart(between, negated:=True)

            Dim Ht As NumericMatrix = DirectCast(H.Transpose, NumericMatrix)
            Dim AHt As NumericMatrix = DirectCast(A.DotProduct(Ht), NumericMatrix)
            Dim HHt As NumericMatrix = DirectCast(H.DotProduct(Ht), NumericMatrix)
            Dim WHHt As NumericMatrix = DirectCast(W.DotProduct(HHt), NumericMatrix)
            Dim WWithinPositive As NumericMatrix = DirectCast(W.DotProduct(withinPositive), NumericMatrix)
            Dim WWithinNegative As NumericMatrix = DirectCast(W.DotProduct(withinNegative), NumericMatrix)
            Dim WBetweenPositive As NumericMatrix = DirectCast(W.DotProduct(betweenPositive), NumericMatrix)
            Dim WBetweenNegative As NumericMatrix = DirectCast(W.DotProduct(betweenNegative), NumericMatrix)

            For i As Integer = 0 To W.RowDimension - 1
                For j As Integer = 0 To W.ColumnDimension - 1
                    Dim numerator As Double = AHt(i, j) +
                                              lambdaWithin * WWithinNegative(i, j) +
                                              lambdaBetween * WBetweenPositive(i, j) +
                                              epsilon
                    Dim denominator As Double = WHHt(i, j) +
                                                lambdaWithin * WWithinPositive(i, j) +
                                                lambdaBetween * WBetweenNegative(i, j) +
                                                epsilon

                    W(i, j) *= numerator / denominator
                Next
            Next

            Dim Wt As NumericMatrix = DirectCast(W.Transpose, NumericMatrix)
            Dim WtA As NumericMatrix = DirectCast(Wt.DotProduct(A), NumericMatrix)
            Dim WtW As NumericMatrix = DirectCast(Wt.DotProduct(W), NumericMatrix)
            Dim WtWH As NumericMatrix = DirectCast(WtW.DotProduct(H), NumericMatrix)

            For i As Integer = 0 To H.RowDimension - 1
                For j As Integer = 0 To H.ColumnDimension - 1
                    H(i, j) *= (WtA(i, j) + epsilon) / (WtWH(i, j) + epsilon)
                Next
            Next

            Dim V As NumericMatrix = DirectCast(W.DotProduct(H), NumericMatrix)
            Dim sumSquaredError As Double = 0

            For i As Integer = 0 To A.RowDimension - 1
                For j As Integer = 0 To A.ColumnDimension - 1
                    Dim difference As Double = A(i, j) - V(i, j)
                    sumSquaredError += difference * difference
                Next
            Next

            cost = sumSquaredError
        End Sub

        Private Shared Sub BuildScatter(W As NumericMatrix,
                                        groups As Dictionary(Of Integer, List(Of Integer)),
                                        ByRef within As NumericMatrix,
                                        ByRef between As NumericMatrix)

            Dim k As Integer = W.ColumnDimension
            Dim total As Integer = W.RowDimension
            Dim overallMean(k - 1) As Double
            Dim classMean(k - 1) As Double
            Dim deviation(k - 1) As Double

            within = New NumericMatrix(k, k)
            between = New NumericMatrix(k, k)

            For i As Integer = 0 To total - 1
                For j As Integer = 0 To k - 1
                    overallMean(j) += W(i, j)
                Next
            Next

            For j As Integer = 0 To k - 1
                overallMean(j) /= total
            Next

            For Each group As List(Of Integer) In groups.Values
                For Each index As Integer In group
                    For j As Integer = 0 To k - 1
                        classMean(j) += W(index, j)
                    Next
                Next

                For j As Integer = 0 To k - 1
                    classMean(j) /= group.Count
                    deviation(j) = classMean(j) - overallMean(j)
                Next

                For Each index As Integer In group
                    For a As Integer = 0 To k - 1
                        Dim withinDeviation As Double = W(index, a) - classMean(a)

                        For b As Integer = 0 To k - 1
                            within(a, b) += withinDeviation * (W(index, b) - classMean(b))
                        Next
                    Next
                Next

                For a As Integer = 0 To k - 1
                    For b As Integer = 0 To k - 1
                        between(a, b) += group.Count * deviation(a) * deviation(b)
                    Next
                Next

                For j As Integer = 0 To k - 1
                    classMean(j) = 0
                    deviation(j) = 0
                Next
            Next

            Call NormalizeByTrace(within)
            Call NormalizeByTrace(between)
        End Sub

        Private Shared Function PositivePart(matrix As NumericMatrix, negated As Boolean) As NumericMatrix
            Dim result As New NumericMatrix(matrix.RowDimension, matrix.ColumnDimension)

            For i As Integer = 0 To matrix.RowDimension - 1
                For j As Integer = 0 To matrix.ColumnDimension - 1
                    Dim value As Double = matrix(i, j)

                    If negated Then
                        value = -value
                    End If

                    result(i, j) = If(value > 0, value, 0.0)
                Next
            Next

            Return result
        End Function

        Private Shared Sub NormalizeByTrace(matrix As NumericMatrix)
            Dim trace As Double = 0

            For i As Integer = 0 To Global.System.Math.Min(matrix.RowDimension, matrix.ColumnDimension) - 1
                trace += matrix(i, i)
            Next

            If trace <= 0 OrElse Not Double.IsFinite(trace) Then
                Return
            End If

            For i As Integer = 0 To matrix.RowDimension - 1
                For j As Integer = 0 To matrix.ColumnDimension - 1
                    matrix(i, j) /= trace
                Next
            Next
        End Sub

    End Class
End Namespace

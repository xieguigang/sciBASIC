Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Math.Statistics.Distributions

''' <summary>
''' 
''' </summary>
''' <remarks>
''' 卡方检验
''' 
''' 卡方检验用于确定预期频数（expected frequencies）和观察频数（observed frequencies）
''' 在一个或多个类别中是否存在显著差异。预期频数是指在假设两个变量独立的情况下，根据边际
''' 总数计算出的理论频数。观察频数则是在实际数据中观察到的频数。
''' </remarks>
Public Class ChiSquareTest

    Public Property observed As Double()()
    Public Property expected As Double()()
    Public Property chi_square As Double
    Public Property pvalue As Double

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="observed">
    ''' shoudl be matrix with two rows and two columns.
    ''' </param>
    ''' <returns></returns>
    Public Shared Function Test(observed As Double()(), expected As Double()()) As ChiSquareTest
        Dim chiSquareStat = (observed(0)(0) - expected(0)(0)) ^ 2 / expected(0)(0) +
                       (observed(0)(1) - expected(0)(1)) ^ 2 / expected(0)(1) +
                       (observed(1)(0) - expected(1)(0)) ^ 2 / expected(1)(0) +
                       (observed(1)(1) - expected(1)(1)) ^ 2 / expected(1)(1)

        ' 自由度（对于二联表始终为1）
        Const freedom As Integer = 1

        Dim p As Double = Distribution.ChiSquare(chiSquareStat, freedom)

        Return New ChiSquareTest With {
            .chi_square = chiSquareStat,
            .expected = expected,
            .observed = observed,
            .pvalue = p
        }
    End Function

    Public Shared Function Test(observed As Double()()) As ChiSquareTest
        Dim expected As Double()() = RectangularArray.Matrix(Of Double)(2, 2)
        Dim row_sums = observed.Select(Function(r) r.Sum).ToArray
        Dim col_sums = New Double(1) {}
        Dim total As Integer = row_sums.Sum

        col_sums(0) = observed.Select(Function(r) r(0)).Sum
        col_sums(1) = observed.Select(Function(r) r(1)).Sum

        For i As Integer = 0 To 1
            For j As Integer = 0 To 1
                expected(i)(j) = (row_sums(i) * col_sums(j)) / total
            Next
        Next

        Return Test(observed, expected)
    End Function
End Class

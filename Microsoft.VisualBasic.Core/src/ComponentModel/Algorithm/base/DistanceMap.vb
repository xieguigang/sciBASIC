Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.Algorithm.base

    Public Class DistanceMap(Of T)

        ReadOnly distanceMap As Double()()

        Default Public ReadOnly Property Distance(i As Integer, j As Integer) As Double
            Get
                Return distanceMap(i)(j)
            End Get
        End Property

        Sub New(points As IEnumerable(Of T), metric As Func(Of T, T, Double))
            Dim pool As T() = points.SafeQuery.ToArray
            Dim n As Integer = pool.Length
            Dim matrix As Double()() = RectangularArray.Matrix(Of Double)(n, n)

            For i As Integer = 0 To n - 1
                ' 对角线元素：点到自身的距离（通常为0，仍需计算一次）
                matrix(i)(i) = metric(points(i), points(i))

                ' 仅计算上三角部分（j > i），避免重复计算
                For j As Integer = i + 1 To n - 1
                    Dim dist As Double = metric(points(i), points(j))

                    matrix(i)(j) = dist
                    matrix(j)(i) = dist  ' 利用对称性直接赋值
                Next
            Next

            distanceMap = matrix
        End Sub
    End Class
End Namespace
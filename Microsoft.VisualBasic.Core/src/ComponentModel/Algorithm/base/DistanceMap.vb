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

        Default Public ReadOnly Property Distance(i As IIndexOf(Of Integer), j As IIndexOf(Of Integer)) As Double
            Get
                Return distanceMap(i.Address)(j.Address)
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="points"></param>
        ''' <param name="metric">
        ''' 具有对称性的距离计算公式，通常为欧几里得距离
        ''' </param>
        Sub New(points As IEnumerable(Of T), metric As Func(Of T, T, Double))
            Dim pool As T() = points.SafeQuery.ToArray
            Dim n As Integer = pool.Length
            ' create NxN matrix
            Dim matrix As Double()() = RectangularArray.Matrix(Of Double)(n, n)

            ' 使用 Parallel.For 并行化外层循环（遍历行索引 i）
            System.Threading.Tasks.Parallel.For(0, n,
                Sub(i)
                    Dim dist As Double = 0
                    Dim vec As Double() = matrix(i)

                    ' 对角线元素：点到自身的距离（每个线程独立计算自己的 i）
                    vec(i) = metric(pool(i), pool(i))

                    ' 上三角部分（j > i）：仅计算当前行 i 的对应列
                    For j As Integer = i + 1 To n - 1
                        dist = metric(pool(i), pool(j))
                        vec(j) = dist
                        matrix(j)(i) = dist ' 利用对称性赋值下三角
                    Next
                End Sub)

            distanceMap = matrix
        End Sub
    End Class
End Namespace
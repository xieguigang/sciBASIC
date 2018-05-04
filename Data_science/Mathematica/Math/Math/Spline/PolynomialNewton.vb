Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Interpolation

    ''' <summary>
    ''' 似乎对三角函数不起作用
    ''' </summary>
    Public Module PolynomialNewton

        ''' <summary>
        ''' PolynomialNewton Spline
        ''' </summary>
        ''' <param name="points">序列之中的<see cref="PointF.X"/>必须要是单调递增或者递减的</param>
        ''' <param name="divided%"></param>
        ''' <returns></returns>
        <Extension>
        Public Function NewtonPolynomial(points As IEnumerable(Of PointF), Optional divided% = 10) As PointF()
            With points.ToArray
                Dim X As Vector = .X
                ' 在这里不可以使用Linq，因为需要将原始序列之中的X点插进入向量之中
                Dim xi As New List(Of Double)
                Dim i As int = Scan0
                Dim xn# = X(++i)
                Dim previous = X(0)

                For Each xj As Double In seq(X.First, X.Last, (X.Last - X.First) / divided)
                    If xn >= previous Then
                        ' 需要插入一个原始序列之中的X
                        xi += xn
                        xn = X(++i)
                    End If

                    xi += xj
                    previous = xj
                Next

                Return xi.AsVector.NewtonPolynomial(.ByRef)
            End With
        End Function

        <Extension>
        Public Function NewtonPolynomial(x As Vector, points As IEnumerable(Of PointF)) As PointF()
            With points.ToArray
                Return Newton(x, .X, .Y) _
                    .Select(Function(y, i)
                                Return New PointF(x(i), y)
                            End Function) _
                    .ToArray
            End With
        End Function

        ''' <summary>
        ''' 使用牛顿多项式插值计算出给定的X点<paramref name="x1"/>所对应的Y值
        ''' <paramref name="x"/>和<paramref name="y"/>是已知的实验值点
        ''' </summary>
        ''' <param name="x1#"></param>
        ''' <param name="x#"></param>
        ''' <param name="y#"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' <paramref name="x"/> and <paramref name="y"/> should be length equals
        ''' </remarks>
        ''' 
        <Extension>
        Public Function Newton(x1 As Vector, x#(), y#()) As Vector
            Dim result As New Vector(y(0), x1.Dim)
            Dim node As New Vector(x1.Dim)
            Dim denominator# = 1.0
            Dim n% = x.Length

            If x.Length <> y.Length OrElse x.Length = 0 Then
                Throw New ArgumentException("Invalid vector length value!")
            End If

            For i As Integer = 1 To n - 1
                node *= 0.0

                For j As Integer = 0 To i

                    ' f[x_0,…,x_n ]
                    denominator = 1.0

                    For k As Integer = 0 To i
                        If k <> j Then
                            denominator *= (x(j) - x(k))
                        End If
                    Next

                    node += y(j) / denominator
                Next

                For k As Integer = 0 To i - 1
                    ' (x-x_0) … (x-x_(i-1) )
                    node *= (x1 - x(k))
                Next

                result += node
            Next

            Return result
        End Function
    End Module
End Namespace
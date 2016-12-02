Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Interpolation

    ''' <summary>
    ''' 三次样条插值的核心代码模块
    ''' </summary>
    Public Class CubicSplineVector : Implements IEnumerable(Of Single)

        Dim _points As New List(Of Single)
        Dim _cubics As New List(Of Cubic)

        Public ReadOnly Property Count As Integer
            Get
                Return _points.Count
            End Get
        End Property

        Sub New(data As IEnumerable(Of Single))
            Call _points.AddRange(data)
        End Sub

        Sub New()
        End Sub

        Sub Add(x!)
            Call _points.Add(x!)
        End Sub

        Public Sub CalcSpline()
            Call CalcNaturalCubic(_points, _cubics)
        End Sub

        Public Overrides Function ToString() As String
            Return _points.GetJson
        End Function

        Public Shared Sub CalcNaturalCubic(values As IList(Of Single), cubics As ICollection(Of Cubic))
            Dim num As Integer = values.Count - 1

            Dim gamma As Double() = New Double(num) {}
            Dim delta As Double() = New Double(num) {}
            Dim D As Double() = New Double(num) {}

            Dim i As Integer
            '		
            '               We solve the equation
            '	          [2 1       ] [D[0]]   [3(x[1] - x[0])  ]
            '	          |1 4 1     | |D[1]|   |3(x[2] - x[0])  |
            '	          |  1 4 1   | | .  | = |      .         |
            '	          |    ..... | | .  |   |      .         |
            '	          |     1 4 1| | .  |   |3(x[n] - x[n-2])|
            '	          [       1 2] [D[n]]   [3(x[n] - x[n-1])]
            '
            '	          by using row operations to convert the matrix to upper triangular
            '	          and then back substitution.  The D[i] are the derivatives at the knots.
            '		 
            gamma(0) = 1.0F / 2.0F
            For i = 1 To num - 1
                gamma(i) = 1.0F / (4.0F - gamma(i - 1))
            Next
            gamma(num) = 1.0F / (2.0F - gamma(num - 1))

            Dim p0 As Single = values(0)
            Dim p1 As Single = values(1)

            delta(0) = 3.0F * (p1 - p0) * gamma(0)
            For i = 1 To num - 1
                p0 = values(i - 1)
                p1 = values(i + 1)
                delta(i) = (3.0F * (p1 - p0) - delta(i - 1)) * gamma(i)
            Next

            p0 = values(num - 1)
            p1 = values(num)

            delta(num) = (3.0F * (p1 - p0) - delta(num - 1)) * gamma(num)

            D(num) = delta(num)
            For i = num - 1 To 0 Step -1
                D(i) = delta(i) - gamma(i) * D(i + 1)
            Next

            'now compute the coefficients of the cubics
            cubics.Clear()

            For i = 0 To num - 1
                p0 = values(i)
                p1 = values(i + 1)

                cubics.Add(New Cubic(p0, D(i), 3 * (p1 - p0) - 2 * D(i) - D(i + 1), 2 * (p0 - p1) + D(i) + D(i + 1)))
            Next
        End Sub

        Public Function GetPoint(position As Single) As Single
            position = position * _cubics.Count

            Dim cubicNum As Integer = CInt(Fix(Math.Min(_cubics.Count - 1, position)))
            Dim cubicPos As Single = (position - cubicNum)

            Return _cubics(cubicNum).Eval(cubicPos)
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of Single) Implements IEnumerable(Of Single).GetEnumerator
            For f As Single = 0 To 1 Step 0.01
                Yield GetPoint(f)
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
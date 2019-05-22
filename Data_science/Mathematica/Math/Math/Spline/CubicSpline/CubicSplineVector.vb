#Region "Microsoft.VisualBasic::a70a60e0f96dc80af4b696710ae48295, Data_science\Mathematica\Math\Math\Spline\CubicSpline\CubicSplineVector.vb"

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

    '     Class CubicSplineVector
    ' 
    '         Properties: Count
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: GetEnumerator, GetPoint, IEnumerable_GetEnumerator, ToString
    ' 
    '         Sub: Add, CalcNaturalCubic, CalcSpline
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.JSON
Imports sys = System.Math

Namespace Interpolation

    ''' <summary>
    ''' 三次样条插值的核心代码模块
    ''' </summary>
    Public Class CubicSplineVector : Implements IEnumerable(Of Single)

        Dim _points As New List(Of Single)
        Dim _cubics As New List(Of Cubic)

        Public ReadOnly Property Count As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return _points.Count
            End Get
        End Property

        Sub New(data As IEnumerable(Of Single))
            Call _points.AddRange(data)
        End Sub

        Sub New()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub Add(x!)
            Call _points.Add(x!)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
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
            Dim cubicNum%, cubicPos!

            position = position * _cubics.Count
            cubicNum = CInt(Fix(sys.Min(_cubics.Count - 1, position)))
            cubicPos = (position - cubicNum)

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

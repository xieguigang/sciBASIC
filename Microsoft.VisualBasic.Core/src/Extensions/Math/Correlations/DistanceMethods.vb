#Region "Microsoft.VisualBasic::52c98e95e2daf60299f56380568c7106, Microsoft.VisualBasic.Core\src\Extensions\Math\Correlations\DistanceMethods.vb"

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


    ' Code Statistics:

    '   Total Lines: 258
    '    Code Lines: 128
    ' Comment Lines: 97
    '   Blank Lines: 33
    '     File Size: 9.66 KB


    '     Module DistanceMethods
    ' 
    '         Function: chebyshev_distance, Distance, (+2 Overloads) DistanceTo, (+6 Overloads) EuclideanDistance, fidelity_distance
    '                   harmonic_mean_distance, Mahalanobis, (+2 Overloads) ManhattanDistance, MinkowskiDistance, (+2 Overloads) SquareDistance
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports std = System.Math

Namespace Math.Correlations

    Public Module DistanceMethods

        ''' <summary>
        ''' Chebyshev distance:
        ''' 
        ''' ```py
        ''' # \underset{i}{\max}{(|P_{i}\ -\ Q_{i}|)}
        ''' np.max(np.abs(p - q))
        ''' ```
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="q"></param>
        ''' <returns></returns>
        Public Function chebyshev_distance(p As Double(), q As Double()) As Double
            Return Aggregate xi As Double
                   In SIMD.Subtract.f64_op_subtract_f64(p, q)
                   Into Max(std.Abs(xi))
        End Function

        ''' <summary>
        ''' Fidelity distance:
        ''' 
        ''' ```py
        ''' # 1-\sum\sqrt{P_{i}Q_{i}}
        ''' 1 - np.sum(np.sqrt(p * q))
        ''' ```
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="q"></param>
        ''' <returns></returns>
        Public Function fidelity_distance(p As Double(), q As Double()) As Double
            Return 1 - (
                Aggregate xi As Double
                In SIMD.Multiply.f64_op_multiply_f64(p, q)
                Into Sum(std.Sqrt(xi))
            )
        End Function

        ''' <summary>
        ''' Harmonic mean distance:
        ''' 
        ''' ```py
        ''' # 1-2\sum(\frac{P_{i}Q_{i}}{P_{i}+Q_{i}})
        ''' 1 - 2 * np.sum(p * q / (p + q))
        ''' ```
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="q"></param>
        ''' <returns></returns>
        Public Function harmonic_mean_distance(p As Double(), q As Double()) As Double
            Dim pxq = SIMD.Multiply.f64_op_multiply_f64(p, q)
            Dim paq = SIMD.Add.f64_op_add_f64(p, q)
            Dim pdq = SIMD.Divide.f64_op_divide_f64(pxq, paq)

            Return 1 - 2 * pdq.Sum
        End Function

        Public Function MinkowskiDistance(X As Double(), Y As Double(), q As Double) As Double
            Dim dq As Double

            For i As Integer = 0 To X.Length - 1
                dq += (X(i) - Y(i)) ^ q
            Next

            Return dq ^ (1 / q)
        End Function

        Public Function Mahalanobis(X As Double(), Y As Double(), W As Double(), q As Double) As Double
            Dim dq As Double

            For i As Integer = 0 To X.Length - 1
                dq += W(i) * (X(i) - Y(i)) ^ q
            Next

            Return dq ^ (1 / q)
        End Function

        ''' <summary>
        ''' 多位坐标的欧几里得距离，与坐标点0进行比较
        ''' </summary>
        ''' <param name="vector"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function EuclideanDistance(vector As IEnumerable(Of Double)) As Double
            ' 由于是和令进行比较，减零仍然为原来的数，所以这里直接使用n^2了
            Return std.Sqrt((From n In vector Select n ^ 2).Sum)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function EuclideanDistance(Vector As IEnumerable(Of Integer)) As Double
            Return std.Sqrt((From n In Vector Select n ^ 2).Sum)
        End Function

        <Extension>
        Public Function EuclideanDistance(a As IEnumerable(Of Integer), b As IEnumerable(Of Integer)) As Double
            If a.Count <> b.Count Then
                Return -1
            Else
                Return std.Sqrt((From i As Integer In a.Sequence Select (a(i) - b(i)) ^ 2).Sum)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function EuclideanDistance(a As IEnumerable(Of Double), b As IEnumerable(Of Double)) As Double
            Return EuclideanDistance(a.ToArray, b.ToArray)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="a">Point A</param>
        ''' <param name="b">Point B</param>
        ''' <returns></returns>
        <Extension>
        Public Function EuclideanDistance(a As Byte(), b As Byte()) As Double
            If a.Length <> b.Length Then
                Return -1.0R
            Else
                Dim sum As Double = 0

                For i As Integer = 0 To a.Length - 1
                    sum += (a(i) - b(i)) ^ 2
                Next

                Return std.Sqrt(sum)
            End If
        End Function

        ''' <summary>
        ''' Calculates the Euclidean Distance Measure between two data points
        ''' </summary>
        ''' <param name="X">An array with the values of an object or datapoint</param>
        ''' <param name="Y">An array with the values of an object or datapoint</param>
        ''' <returns>Returns the Euclidean Distance Measure Between Points X and Points Y</returns>
        ''' 
        <Extension>
        Public Function EuclideanDistance(X As Double(), Y As Double()) As Double
            If X.Length <> Y.Length Then
                Throw New ArgumentException(DimNotAgree)
            Else
                Dim v = SIMD.Exponent.f64_op_exponent_f64_scalar(SIMD.Subtract.f64_op_subtract_f64(X, Y), 2)
                Dim sum As Double = v.Sum
                Dim distance As Double = std.Sqrt(sum)

                Return distance
            End If
        End Function

        Const DimNotAgree As String = "The number of elements in X must match the number of elements in Y!"

        ''' <summary>
        ''' Calculates the Manhattan Distance Measure between two data points
        ''' </summary>
        ''' <param name="X">An array with the values of an object or datapoint</param>
        ''' <param name="Y">An array with the values of an object or datapoint</param>
        ''' <returns>Returns the Manhattan Distance Measure Between Points X and Points Y</returns>
        ''' <remarks>
        ''' Manhattan 距离：是Minkowski, q=1时的特例
        ''' </remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ManhattanDistance(X#(), Y#()) As Double
            Return MinkowskiDistance(X, Y, 1)
        End Function

        ''' <summary>
        ''' Calculates the Manhattan Distance Measure between two data points
        ''' </summary>
        ''' <param name="X">An array with the values of an object or datapoint</param>
        ''' <param name="Y">An array with the values of an object or datapoint</param>
        ''' <returns>Returns the Manhattan Distance Measure Between Points X and Points Y</returns>
        ''' <remarks>
        ''' Manhattan 距离：是Minkowski, q=1时的特例
        ''' </remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ManhattanDistance(x As IVector, y As IVector) As Double
            Return MinkowskiDistance(x.Data, y.Data, 1)
        End Function

#If NET_48 = 1 Or NETCOREAPP Then

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Distance(pt As (X#, Y#), x#, y#) As Double
            Return {pt.X, pt.Y}.EuclideanDistance({x, y})
        End Function

#End If

        ''' <summary>
        ''' implements via <see cref="EuclideanDistance"/>
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function DistanceTo(a As IVector, b As IVector) As Double
            Return EuclideanDistance(a.Data, b.Data)
        End Function

        ''' <summary>
        ''' implements via <see cref="EuclideanDistance"/>
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="v"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function DistanceTo(a As IVector, v As Double()) As Double
            Return EuclideanDistance(a.Data, v)
        End Function

        ''' <summary>
        ''' SUM((a - v) ^ 2)
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="v"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function SquareDistance(a As IVector, v As Double()) As Double
            Return SIMD.Exponent.f64_op_exponent_f64_scalar(SIMD.Subtract.f64_op_subtract_f64(a.Data, v), 2).Sum
        End Function

        ''' <summary>
        ''' Reduced Euclidean distance
        ''' </summary>
        ''' <remarks>
        ''' SUM((x - y) ^ 2)
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function SquareDistance(x As Span(Of Double), y As Span(Of Double)) As Double
            Dim len As Integer = x.Length
            Dim v As Double() = New Double(len - 1) {}

            For i As Integer = 0 To len - 1
                v(i) = x(i) - y(i)
            Next

            Return SIMD.Exponent.f64_op_exponent_f64_scalar(v, 2).Sum
        End Function
    End Module
End Namespace

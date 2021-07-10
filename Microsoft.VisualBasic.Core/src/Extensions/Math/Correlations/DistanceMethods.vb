Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports stdNum = System.Math

Namespace Math.Correlations

    Public Module DistanceMethods

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
            Return stdNum.Sqrt((From n In vector Select n ^ 2).Sum)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function EuclideanDistance(Vector As IEnumerable(Of Integer)) As Double
            Return stdNum.Sqrt((From n In Vector Select n ^ 2).Sum)
        End Function

        <Extension>
        Public Function EuclideanDistance(a As IEnumerable(Of Integer), b As IEnumerable(Of Integer)) As Double
            If a.Count <> b.Count Then
                Return -1
            Else
                Return stdNum.Sqrt((From i As Integer In a.Sequence Select (a(i) - b(i)) ^ 2).Sum)
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
                Return stdNum.Sqrt((From i As Integer In a.Sequence Select (CInt(a(i)) - CInt(b(i))) ^ 2).Sum)
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
            End If

            Dim count As Integer = X.Length
            Dim sum As Double = 0.0

            For i As Integer = 0 To count - 1
                sum += stdNum.Pow(stdNum.Abs(X(i) - Y(i)), 2)
            Next

            Dim distance As Double = stdNum.Sqrt(sum)

            Return distance
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

#If NET_48 = 1 Or netcore5 = 1 Then

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Distance(pt As (X#, Y#), x#, y#) As Double
            Return {pt.X, pt.Y}.EuclideanDistance({x, y})
        End Function

#End If
    End Module
End Namespace
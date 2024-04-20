﻿Namespace mr.go.sgfilter
    'JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
    '	import static Math.abs;
    'JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
    '	import static Math.pow;
    'JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
    '	import static Math.sqrt;

    ''' <summary>
    ''' Filters data using Ramer-Douglas-Peucker algorithm with specified tolerance
    ''' 
    ''' @author Rzeźnik </summary>
    ''' <seealsocref=""/> <ahref="http://en.wikipedia.org/wiki/Ramer-Douglas-Peucker_algorithm">Ramer-Douglas-Peucker algorithm</a> </seealso>
    Public Class RamerDouglasPeuckerFilter
        Implements DataFilter

        Private epsilonField As Double

        ''' 
        ''' <paramname="epsilon">
        '''            epsilon in Ramer-Douglas-Peucker algorithm (maximum distance
        '''            of a point in data between original curve and simplified
        '''            curve) </param>
        ''' <exceptioncref="IllegalArgumentException">
        '''             when {@code epsilon </> </exception>
        Public Sub New(epsilon As Double)
            If epsilon <= 0 Then
                Throw New ArgumentException("Epsilon nust be > 0")
            End If
            epsilonField = epsilon
        End Sub

        Public Overridable Function filter(data As Double()) As Double() Implements DataFilter.filter
            Return ramerDouglasPeuckerFunction(data, 0, data.Length - 1)
        End Function

        ''' 
        ''' <returns> {@code epsilon} </returns>
        Public Overridable Property Epsilon As Double
            Get
                Return epsilonField
            End Get
            Set(value As Double)
                If value <= 0 Then
                    Throw New ArgumentException("Epsilon nust be > 0")
                End If
                epsilonField = value
            End Set
        End Property

        Protected Friend Overridable Function ramerDouglasPeuckerFunction(points As Double(), startIndex As Integer, endIndex As Integer) As Double()
            Dim dmax As Double = 0
            Dim idx = 0
            Dim a As Double = endIndex - startIndex
            Dim b = points(endIndex) - points(startIndex)
            Dim c = -(b * startIndex - a * points(startIndex))
            Dim norm = Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2))
            For i = startIndex + 1 To endIndex - 1
                Dim distance = Math.Abs(b * i - a * points(i) + c) / norm
                If distance > dmax Then
                    idx = i
                    dmax = distance
                End If
            Next
            If dmax >= epsilonField Then
                Dim recursiveResult1 = ramerDouglasPeuckerFunction(points, startIndex, idx)
                Dim recursiveResult2 = ramerDouglasPeuckerFunction(points, idx, endIndex)
                Dim result = New Double(recursiveResult1.Length - 1 + recursiveResult2.Length - 1) {}
                Array.Copy(recursiveResult1, 0, result, 0, recursiveResult1.Length - 1)
                Array.Copy(recursiveResult2, 0, result, recursiveResult1.Length - 1, recursiveResult2.Length)
                Return result
            Else
                Return New Double() {points(startIndex), points(endIndex)}
            End If
        End Function


    End Class

End Namespace

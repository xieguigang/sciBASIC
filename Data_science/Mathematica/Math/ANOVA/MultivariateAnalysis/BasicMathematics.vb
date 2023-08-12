Imports std = System.Math

Public Class BasicMathematics

    Public Shared Function ErrorOfSquareVs2(ByVal x As Double(), ByVal y As Double()) As Double
        Dim sum As Double = 0
        For i = 0 To x.Length - 1
            sum += std.Pow(x(i) - y(i), 2)
        Next
        Return sum
    End Function

    Public Shared Function Stdev(ByVal array As Double()) As Double
        Dim sum As Double = 0, mean As Double = 0
        For i = 0 To array.Length - 1
            sum += array(i)
        Next
        mean = sum / array.Length

        sum = 0
        For i = 0 To array.Length - 1
            sum += std.Pow(array(i) - mean, 2)
        Next
        Return std.Sqrt(sum / (array.Length - 1))
    End Function

    Public Shared Function SumOfSquare(ByVal array As Double()) As Double
        Dim sum As Double = 0
        For i = 0 To array.Length - 1
            sum += std.Pow(array(i), 2)
        Next
        Return sum
    End Function

    Public Shared Function RootSumOfSquare(ByVal array As Double()) As Double
        Dim sum As Double = 0
        For i = 0 To array.Length - 1
            sum += std.Pow(array(i), 2)
        Next
        Return std.Sqrt(sum)
    End Function

    Public Shared Function Var(ByVal array As Double()) As Double
        Dim sum As Double = 0, mean As Double = 0
        For i = 0 To array.Length - 1
            sum += array(i)
        Next
        mean = sum / array.Length

        sum = 0
        For i = 0 To array.Length - 1
            sum += std.Pow(array(i) - mean, 2)
        Next
        Return sum / (array.Length - 1)
    End Function

    Public Shared Function InnerProduct(ByVal array1 As Double(), ByVal array2 As Double()) As Double
        Dim sum As Double = 0
        For i = 0 To array1.Length - 1
            sum += array1(i) * array2(i)
        Next
        Return sum
    End Function
End Class

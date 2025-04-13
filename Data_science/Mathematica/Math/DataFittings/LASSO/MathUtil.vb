Namespace LASSO

    ''' <summary>
    ''' Utility Math functions that are used by other classes.
    ''' 
    ''' @author Yasser Ganjisaffar (http://www.ics.uci.edu/~yganjisa/)
    ''' 
    ''' </summary>
    Public Class MathUtil


        Public Shared Function getStg(arr As Double()) As Double
            Return getStd(arr, arr.Average())
        End Function

        Public Shared Function getStg(arr As IList(Of Double)) As Double
            Return getStd(arr, arr.Average())
        End Function

        Public Shared Function getStd(arr As Double(), avg As Double) As Double
            Dim sum As Double = 0
            For Each item In arr
                sum += Math.Pow(item - avg, 2)
            Next
            Return Math.Sqrt(sum / arr.Length)
        End Function

        Public Shared Function getStd(arr As IList(Of Double), avg As Double) As Double
            Dim sum As Double = 0
            For Each item In arr
                sum += Math.Pow(item - avg, 2)
            Next
            Return Math.Sqrt(sum / arr.Count)
        End Function

        Public Shared Function getDotProduct(vector1 As Single(), vector2 As Single(), length As Integer) As Double
            Dim product As Double = 0
            For i = 0 To length - 1
                product += vector1(i) * vector2(i)
            Next
            Return product
        End Function

        Public Shared Function getDotProduct(vector1 As Double(), vector2 As Double(), length As Integer) As Double
            Dim product As Double = 0
            For i = 0 To length - 1
                product += vector1(i) * vector2(i)
            Next
            Return product
        End Function

        Public Shared Function getDotProduct(vector1 As Single(), vector2 As Single()) As Double
            Return getDotProduct(vector1, vector2, vector1.Length)
        End Function

        ' Divides the second vector from the first one (vector1[i] /= val)
        Public Shared Sub divideInPlace(vector As Single(), val As Single)
            Dim length = vector.Length
            For i = 0 To length - 1
                vector(i) /= val
            Next
        End Sub

        Public Shared Function allocateDoubleMatrix(m As Integer, n As Integer) As Double()()
            Dim mat = New Double(m - 1)() {}
            For i = 0 To m - 1
                mat(i) = New Double(n - 1) {}
            Next
            Return mat
        End Function

        Public Shared Function getFormattedDouble(val As Double, decimalPoints As Integer) As String
            Dim format = "#."
            For i = 0 To decimalPoints - 1
                format += "#"
            Next
            Return val.ToString(format)
        End Function
    End Class

End Namespace

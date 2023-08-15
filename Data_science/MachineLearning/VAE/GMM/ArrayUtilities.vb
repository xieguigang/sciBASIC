Imports ext = Microsoft.VisualBasic.Linq.Extensions
Imports std = System.Math

Namespace GMM.EMGaussianMixtureModel

    Public Module ArrayUtilities

        Public Function sumList(X As IList(Of Double), Y As IList(Of Double)) As IList(Of Double)
            If X Is Nothing OrElse Y Is Nothing Then
                Throw New NullReferenceException("one of the submitted Lists is null, cannot add Lists")
            End If
            ' Debug.Assert(X.Count == Y.Count);
            Dim results As IList(Of Double) = New List(Of Double)()
            For i = 0 To X.Count - 1
                results.Add(X(i) + Y(i))
            Next
            Return results
        End Function

        Public Function multiplicationScalar(X As IList(Of Double), j As Double) As IList(Of Double)
            Dim results As IList(Of Double) = New List(Of Double)()
            For Each xi In X
                results.Add(xi * j)
            Next

            Return results
        End Function

        Public Function multiplicationScalar(X As Double(), j As Double) As Double()
            Dim results = New Double(X.Length - 1) {}
            For i = 0 To X.Length - 1
                results(i) = X(i) * j
            Next
            Return results
        End Function


        Public Function divisionScalar(X As IList(Of Double), j As Double) As IList(Of Double)
            If j = 0.0 Then
                Throw New ArithmeticException("divisionScalar failed, scalar cannot be 0.0")
            Else
                Return multiplicationScalar(X, 1 / j)
            End If
        End Function

        'idea is that we have a list of lists (inner list is like a row), and we sum over all columns.
        Public Function columnSum(input As IList(Of IList(Of Double))) As IList(Of Double)
            Dim results As IList(Of Double) = New List(Of Double)(ext.Repeats(0.0, input(0).Count)) '(Collections.nCopies(input[0].Count, 0.0));
            For Each i In input
                results = sumList(results, i)
            Next
            Return results
        End Function

        Public Function l1Norm(inVector As IList(Of Double)) As IList(Of Double)
            Dim sum = 0.0
            For Each vi In inVector
                sum += std.Abs(vi)
            Next
            Return divisionScalar(inVector, sum)
        End Function

        Public Function distToCenterL1(x As Double, centers As IList(Of Double)) As IList(Of Double)
            Dim results As IList(Of Double) = New List(Of Double)()
            For Each ci In centers
                results.Add(std.Abs(x - ci))
            Next
            Return l1Norm(results)
        End Function

        Public Function euclidDist(x As Double(), y As Double()) As Double
            Debug.Assert(x.Length = y.Length)
            Dim result = 0.0
            For i = 0 To x.Length - 1
                result += std.Pow(x(i) - y(i), 2)
            Next
            Return std.Sqrt(result)
        End Function

        Public Function distToCenterL1(x As Double(), centers As IList(Of Double())) As IList(Of Double)
            Dim results As IList(Of Double) = New List(Of Double)()
            For Each center In centers
                results.Add(euclidDist(x, center))
            Next
            Return l1Norm(results)
        End Function
    End Module

End Namespace

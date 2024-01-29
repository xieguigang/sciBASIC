Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

Namespace GMM.EMGaussianMixtureModel

    Public Module Math

        Public Function divisionScalar(X As IList(Of Double), j As Double) As Double()
            If j = 0.0 Then
                Throw New ArithmeticException("divisionScalar failed, scalar cannot be 0.0")
            Else
                Return New Vector(X) * (1 / j)
            End If
        End Function

        ''' <summary>
        ''' idea is that we have a list of lists (inner list is like a row), and we sum over all columns.
        ''' </summary>
        ''' <param name="input"></param>
        ''' <returns></returns>
        Public Function columnSum(input As IList(Of IList(Of Double))) As IList(Of Double)
            Dim results As Vector = Vector.Zero([Dim]:=input(0).Count)

            For Each i As IList(Of Double) In input
                results = results + New Vector(i)
            Next

            Return results.AsList
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

        Public Function distToCenterL1(x As Double(), centers As IList(Of Double())) As IList(Of Double)
            Dim results As IList(Of Double) = New List(Of Double)()
            For Each center As Double() In centers
                results.Add(x.EuclideanDistance(center))
            Next
            Return l1Norm(results)
        End Function
    End Module

End Namespace

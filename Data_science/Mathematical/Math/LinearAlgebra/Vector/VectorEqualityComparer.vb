Imports System.Collections.Generic

Namespace LinearAlgebra

    Public Class VectorEqualityComparer
        Implements IEqualityComparer(Of Vector)

        Public Shared Function VectorEqualsToAnother(v1 As List(Of Double), v2 As List(Of Double)) As Boolean
            If v1.Count <> v2.Count Then
                Return False
            End If

            For i As Integer = 0 To v1.Count - 1
                If v1(i) <> v2(i) Then
                    Return False
                End If
            Next

            Return True
        End Function

        Public Shared Function VectorEqualsToAnother(v1#(), v2#()) As Boolean
            If v1.Length <> v2.Length Then
                Return False
            End If

            For i As Integer = 0 To v1.Length - 1
                If v1(i) <> v2(i) Then
                    Return False
                End If
            Next

            Return True
        End Function

        Public Overloads Function Equals(x As Vector, y As Vector) As Boolean Implements IEqualityComparer(Of Vector).Equals
            Return VectorEqualsToAnother(x, y)
        End Function

        Public Overloads Function GetHashCode(v As Vector) As Integer Implements IEqualityComparer(Of Vector).GetHashCode
            Dim sum As Integer = v.Sum(Function(x) x.GetHashCode)
            Return sum
        End Function
    End Class
End Namespace
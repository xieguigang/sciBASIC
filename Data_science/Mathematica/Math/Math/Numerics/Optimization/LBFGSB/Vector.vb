Imports std = System.Math

Namespace Framework.Optimization.LBFGSB

    Public NotInheritable Class Vector

        ' Eigen resize functionality
        Public Shared Function resize(a As Double(), n As Integer) As Double()
            If a Is Nothing Then
                Return New Double(n - 1) {}
            Else
                If a.Length = n Then
                    Return a
                Else
                    Dim res = New Double(n - 1) {}
                    Array.Copy(a, 0, res, 0, n)
                    Return res
                End If
            End If
        End Function

        Public Shared Function dot(a As Double(), b As Double()) As Double
            Dim res = 0.0
            For i = 0 To a.Length - 1
                res += a(i) * b(i)
            Next
            Return res
        End Function

        Public Shared Function squaredNorm(x As Double()) As Double
            Dim res = 0.0
            For i = 0 To x.Length - 1
                res += x(i) * x(i)
            Next
            Return res
        End Function

        Public Shared Function norm(x As Double()) As Double
            Return std.Sqrt(squaredNorm(x))
        End Function

        Public Shared Sub normalize(x As Double())
            Dim n = norm(x)
            For i = 0 To x.Length - 1
                x(i) /= n
            Next
        End Sub

        Public Shared Sub setAll(x As Double(), v As Double)
            For i = 0 To x.Length - 1
                x(i) = v
            Next
        End Sub

        Public Shared Sub [sub](a As Double(), b As Double(), res As Double())
            For i = 0 To res.Length - 1
                res(i) = a(i) - b(i)
            Next
        End Sub
    End Class

End Namespace

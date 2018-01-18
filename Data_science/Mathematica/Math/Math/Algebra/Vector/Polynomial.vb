Namespace LinearAlgebra

    Public Class Polynomial

        Public Property Factors As Double()

        Public ReadOnly Property F(x#) As Double
            Get
                Dim ans As Double = 0

                For i As Integer = 0 To Factors.Length - 1
                    ans += Factors(i) * (x ^ i)
                Next

                Return ans
            End Get
        End Property

        Public Overrides Function ToString() As String
            Dim items = Factors _
                .Select(Function(a, i)
                            If i = 0 Then
                                Return a.ToString("F2")
                            ElseIf i = 1 Then
                                Return $"{a.ToString("F2")}*x"
                            Else
                                Return $"{a.ToString("F2")}*x^{i}"
                            End If
                        End Function) _
                .ToArray
            Dim Y$ = items.JoinBy(" + ")

            Return Y
        End Function
    End Class
End Namespace
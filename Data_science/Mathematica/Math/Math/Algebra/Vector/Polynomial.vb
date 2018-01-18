Imports System.Runtime.CompilerServices

Namespace LinearAlgebra

    Public Class Polynomial

        Public Property Factors As Double()

        Default Public ReadOnly Property F(x#) As Double
            Get
                Dim ans As Double = 0

                For i As Integer = 0 To Factors.Length - 1
                    ans += Factors(i) * (x ^ i)
                Next

                Return ans
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return ToString(format:="F2")
        End Function

        Public Overloads Function ToString(format As String) As String
            Dim items = Factors _
                .Select(Function(a, i)
                            If i = 0 Then
                                Return a.ToString(format)
                            ElseIf i = 1 Then
                                Return $"{a.ToString(format)}*X"
                            Else
                                Return $"{a.ToString(format)}*X^{i}"
                            End If
                        End Function) _
                .ToArray
            Dim Y$ = items.JoinBy(" + ")

            Return Y
        End Function
    End Class
End Namespace
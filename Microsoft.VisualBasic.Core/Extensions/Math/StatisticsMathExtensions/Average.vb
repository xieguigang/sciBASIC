Imports System.Runtime.CompilerServices

Namespace Math.Statistics

    Public Class Average

        Public Sum#, N%

        Public ReadOnly Property Average As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Sum / N
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(data As IEnumerable(Of Double))
            With data.ToArray
                Sum = .Sum
                N = .Length
            End With
        End Sub

        Public Overrides Function ToString() As String
            Return $"Means of {N} samples = {Average}"
        End Function

        Public Shared Operator +(avg As Average, x#) As Average
            avg.Sum += x
            avg.N += 1
            Return avg
        End Operator

        Public Shared Widening Operator CType(avg As Double) As Average
            Return New Average() + avg
        End Operator
    End Class
End Namespace
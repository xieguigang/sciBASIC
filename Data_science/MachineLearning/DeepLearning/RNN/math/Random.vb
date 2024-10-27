Imports rand = Microsoft.VisualBasic.Math.RandomExtensions

Namespace RNN

    ''' <summary>
    ''' Helper functions for randomness.
    ''' </summary>
    Public Class Random

        ' Random matrix

        ' Returns an MxN matrix filled with numbers drawn from a standard normal
        ' distribution.
        ' Requires that M > 0 and N > 0.
        Public Shared Function randn(M As Integer, N As Integer) As Matrix
            Dim lM = Matrix.zeros(M, N)
            lM.apply(Function(d) rand.NextDouble())
            Return lM
        End Function

        ' Returns an k-dimensional vector filled with numbers drawn from a standard
        ' normal distribution.
        ' Requires that k > 0.
        Public Shared Function randn(k As Integer) As Matrix
            Return randn(1, k)
        End Function

        ' Returns a matrix shaped like m filled with numbers drawn from a standard
        ' normal distribution.
        ' Requires that m != null.
        Public Shared Function randomLike(m As Matrix) As Matrix
            Return randn(m.M, m.N)
        End Function

        ' Random choice 

        ' Samples an index from a random distribution with the given probabilities
        ' p. Will work properly, if the sum of probabilities is 1.0.
        ' Requires that p != null.
        Public Shared Function randomChoice(p As Double()) As Integer
            Dim random As Double = rand.NextDouble()
            Dim cumulative = 0.0

            For i = 0 To p.Length - 1
                cumulative += p(i)
                If cumulative > random Then
                    Return i
                End If
            Next
            Return p.Length - 1 ' Fallback: probabilities did not sum up to a 1.0;
        End Function
    End Class

End Namespace
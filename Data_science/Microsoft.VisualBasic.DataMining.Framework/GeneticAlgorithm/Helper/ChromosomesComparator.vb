Namespace GAF.Helper

    Friend Class ChromosomesComparator(Of C As Chromosome(Of C), T As IComparable(Of T))
        Implements IComparer(Of C)

        ReadOnly outerInstance As GeneticAlgorithm(Of C, T)
        ReadOnly cache As New Dictionary(Of C, T)

        Public Sub New(GA As GeneticAlgorithm(Of C, T))
            outerInstance = GA
        End Sub

        Public Function compare(chr1 As C, chr2 As C) As Integer Implements IComparer(Of C).Compare
            Dim fit1 As T = Me.fit(chr1)
            Dim fit2 As T = Me.fit(chr2)
            Dim ret As Integer = fit1.CompareTo(fit2)
            Return ret
        End Function

        Public Function fit(chr As C) As T
            If cache.ContainsKey(chr) Then
                Return cache(chr)
            Else
                Dim f As T = outerInstance _
                    ._fitnessFunc _
                    .Calculate(chr)
                cache(chr) = f

                Return f
            End If
        End Function

        Public Sub clearCache()
            Me.cache.Clear()
        End Sub
    End Class
End Namespace
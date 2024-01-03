Namespace evolution

    Public Interface Individual
        Inherits IComparable(Of Individual)

        ReadOnly Property Expression As Expression

        ReadOnly Property Fitness As Double

        Function computeFitness(dataTuples As IList(Of Tuple)) As Double

    End Interface

End Namespace

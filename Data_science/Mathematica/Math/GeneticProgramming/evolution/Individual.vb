Imports Microsoft.VisualBasic.Math.Scripting
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.model

Namespace evolution

    Public Interface Individual
        Inherits IComparable(Of Individual)

        ReadOnly Property Expression As Expression

        ReadOnly Property Fitness As Double

        Function computeFitness(dataTuples As IList(Of DataPoint)) As Double

    End Interface

End Namespace

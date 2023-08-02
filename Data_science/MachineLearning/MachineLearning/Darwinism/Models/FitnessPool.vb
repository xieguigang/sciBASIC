Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF

Namespace Darwinism.Models

    ''' <summary>
    ''' Compute fitness and cache the result data in this pool.
    ''' </summary>
    ''' <typeparam name="Individual"></typeparam>
    ''' <remarks>
    ''' this fitness calculation pool is works for the genetic algorithm module
    ''' </remarks>
    Public Class FitnessPool(Of Individual As {Class, Chromosome(Of Individual)}) : Inherits GeneralFitnessPool(Of Individual)

        Sub New(cacl As Fitness(Of Individual), capacity%)
            Call MyBase.New(cacl, capacity, Function(a) a.Identity)
        End Sub
    End Class
End Namespace
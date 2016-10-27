Imports Microsoft.VisualBasic.DataMining.Darwinism.Models

Namespace Darwinism.GAF.Helper

    ''' <summary>
    ''' 缓存的Key是染色体的ToString的计算值
    ''' </summary>
    ''' <typeparam name="C"></typeparam>
    ''' <typeparam name="T"></typeparam>
    Friend Class ChromosomesComparator(Of C As Chromosome(Of C), T As IComparable(Of T)) : Inherits FitnessPool(Of C, T)
        Implements IComparer(Of C)

        Public Sub New(GA As GeneticAlgorithm(Of C, T))
            caclFitness = AddressOf GA._fitnessFunc.Calculate
        End Sub

        Public Function compare(chr1 As C, chr2 As C) As Integer Implements IComparer(Of C).Compare
            Dim fit1 As T = Fitness(chr1)
            Dim fit2 As T = Fitness(chr2)
            Dim ret As Integer = fit1.CompareTo(fit2)
            Return ret
        End Function

        Public Sub clearCache()
            Call cache.Clear()
        End Sub
    End Class
End Namespace
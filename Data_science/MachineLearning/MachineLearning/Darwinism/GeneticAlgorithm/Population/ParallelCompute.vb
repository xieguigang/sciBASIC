Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models

Namespace Darwinism.GAF.Population

    ''' <summary>
    ''' implements this interface for create custom parallel 
    ''' compute api function for run the genetic algorithm
    ''' </summary>
    ''' <typeparam name="chr"></typeparam>
    ''' <remarks>
    ''' 遗传算法的主要限速步骤是在fitness的计算之上
    ''' </remarks>
    Public MustInherit Class ParallelComputeFitness(Of chr As {Class, Chromosome(Of chr)})

        Public MustOverride Function ComputeFitness(comparator As FitnessPool(Of chr), source As PopulationCollection(Of chr)) As IEnumerable(Of NamedValue(Of Double))

    End Class

    Public Class ParallelPopulationCompute(Of chr As {Class, Chromosome(Of chr)}) : Inherits ParallelComputeFitness(Of chr)

        Public Overrides Function ComputeFitness(comparator As FitnessPool(Of chr), source As PopulationCollection(Of chr)) As IEnumerable(Of NamedValue(Of Double))
            Return From c As chr
                   In source.GetCollection.AsParallel
                   Let fit As Double = comparator.Fitness(c, parallel:=False)
                   Let key As String = c.Identity
                   Select New NamedValue(Of Double) With {
                      .Name = key,
                      .Value = fit
                   }
        End Function
    End Class

    Public Class ParallelDataSetCompute(Of chr As {Class, Chromosome(Of chr)}) : Inherits ParallelComputeFitness(Of chr)

        Public Overrides Function ComputeFitness(comparator As FitnessPool(Of chr), source As PopulationCollection(Of chr)) As IEnumerable(Of NamedValue(Of Double))
            Return From c As chr
                   In source.GetCollection()
                   Let fit As Double = comparator.Fitness(c, parallel:=True)
                   Let key As String = c.Identity
                   Select New NamedValue(Of Double) With {
                       .Name = key,
                       .Value = fit
                   }
        End Function
    End Class
End Namespace
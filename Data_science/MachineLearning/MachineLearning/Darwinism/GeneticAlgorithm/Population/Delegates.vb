Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models

Namespace Darwinism.GAF

    ''' <summary>
    ''' 遗传算法的主要限速步骤是在fitness的计算之上
    ''' </summary>
    ''' <typeparam name="chr"></typeparam>
    ''' <param name="source"></param>
    ''' <returns></returns>
    Public Delegate Function ParallelComputeFitness(Of chr As {Class, Chromosome(Of chr)})(comparator As FitnessPool(Of chr), source As PopulationCollection(Of chr)) As IEnumerable(Of NamedValue(Of Double))
    Public Delegate Function PopulationCollectionCreator(Of Chr As {Class, Chromosome(Of Chr)})() As PopulationCollection(Of Chr)

End Namespace

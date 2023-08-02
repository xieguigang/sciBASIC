Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models

Namespace Darwinism.GAF.Population.SubstitutionStrategy

    ''' <summary>
    ''' 种群的精英杂交更替策略
    ''' </summary>
    ''' <typeparam name="Chr"></typeparam>
    Public Class EliteReplacement(Of Chr As {Class, Chromosome(Of Chr)})
        Implements IStrategy(Of Chr)

        ReadOnly ranf As Random = Math.seeds

        Public ReadOnly Property type As Strategies Implements IStrategy(Of Chr).type
            Get
                Return Strategies.EliteCrossbreed
            End Get
        End Property

        ''' <summary>
        ''' 只保留10%的个体,然后这些个体杂交补充到种群的大小
        ''' </summary>
        ''' <param name="newPop"></param>
        ''' <param name="GA"></param>
        ''' <returns></returns>
        Public Function newPopulation(newPop As Population(Of Chr), GA As GeneticAlgorithm(Of Chr)) As Population(Of Chr) Implements IStrategy(Of Chr).newPopulation
            Dim x, y As Chr

            ' 通过fitness排序来进行择优
            ' 只选择最好的前10个染色体
            ' 然后剩余的成员通过这些被保留下来的染色体间的交叉来生成
            Call newPop.SortPopulationByFitness(GA.chromosomesComparator)
            Call newPop.Trim(newPop.capacitySize * 0.1)

            ' 对剩下的精英个体进行杂交,补充种群的成员
            Do While newPop.Size < newPop.capacitySize
                x = newPop.Random(ranf)
                y = newPop.Random(ranf)

                For Each newIndividual As Chr In x.Crossover(y)
                    Call newPop.Add(newIndividual)
                Next
            Loop

            Return newPop
        End Function
    End Class
End Namespace
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF.Population
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models
Imports Microsoft.VisualBasic.ValueTypes

Namespace Darwinism.GAF.Helper

    Public Module InitializationHelper

        ''' <summary>
        ''' The simplest strategy for creating initial population <br/>
        ''' in real life it could be more complex.
        ''' </summary>
        <Extension>
        Public Function InitialPopulation(Of T As {Class, Chromosome(Of T)})(base As T, popSize%,
                                                                             Optional parallel As Boolean = True,
                                                                             Optional addBase As Boolean = True,
                                                                             Optional parallelInitialize As Boolean = True) As Population(Of T)
            Return base.InitialPopulation(
                population:=New Population(Of T)(New PopulationList(Of T), parallel) With {.capacitySize = popSize},
                addBase:=addBase,
                parallelInitialize:=parallelInitialize
            )
        End Function

        ''' <summary>
        ''' The simplest strategy for creating initial population <br/>
        ''' in real life it could be more complex.
        ''' 
        ''' (如果<paramref name="population"/>对象的构造函数所传递的fitness计算函数是False，则整个GA的计算过程为串行计算过程)
        ''' </summary>
        <Extension>
        Public Function InitialPopulation(Of T As {Class, Chromosome(Of T)})(base As T, population As IPopulation(Of T),
                                                                             Optional addBase As Boolean = True,
                                                                             Optional parallelInitialize As Boolean = True) As Population(Of T)
            Dim time As Double = App.ElapsedMilliseconds
            Dim populationSize% = population.capacitySize

            If addBase Then
                ' 20190722
                ' 如果这个base是来自于已经训练好的模型,那么会需要将其添加进入
                ' 现在的这个种群之中
                ' 否则程序会需要额外的几个循环来进行训练至best
                Call population.Add(base)
            End If

            Call "Start to create the initial population...".__DEBUG_ECHO

            ' Each member of initial population
            ' is mutated clone of base chromosome
            Dim mutations As IEnumerable(Of T) = From i As Integer
                                                 In populationSize _
                                                     .SeqRandom _
                                                     .Populate(parallelInitialize)
                                                 Select base.Mutate
            ' 使用并行化, 在处理大型的数据集的时候可以在这里比较明显的提升计算性能
            For Each chr As T In mutations
                Call population.Add(chr)
            Next

            Call $"Takes {DateTimeHelper.ReadableElapsedTime(App.ElapsedMilliseconds - time)} for intialize population.".__DEBUG_ECHO

            Return population
        End Function
    End Module
End Namespace
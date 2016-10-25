Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo
Imports Microsoft.VisualBasic.DataMining.GAF
Imports Microsoft.VisualBasic.DataMining.GAF.Helper
Imports Microsoft.VisualBasic.Mathematical.diffEq

Namespace GAF

    ''' <summary>
    ''' 参数拟合的方法
    ''' </summary>
    Public Module Protocol

        ''' <summary>
        ''' 测试用
        ''' </summary>
        ''' <param name="model"></param>
        ''' <param name="n%"></param>
        ''' <param name="a#"></param>
        ''' <param name="b#"></param>
        ''' <param name="popSize%"></param>
        ''' <param name="evolIterations%"></param>
        ''' <param name="outPrint"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Fitting(model As Model, n%, a#, b#,
                                Optional popSize% = 1000%,
                                Optional evolIterations% = 5000%,
                                Optional ByRef outPrint As List(Of outPrint) = Nothing) As Dictionary(Of String, Double)

            Dim population As Population(Of ParameterVector) =
                New ParameterVector() With {
                    .vars = Model _
                        .GetParameters(GetType(Model)) _
                        .Select(Function(s) New var) _
                        .ToArray
            }.InitialPopulation(popSize%)
            Dim fitness As Fitness(Of ParameterVector, Double) =
                New GAFfitness(New Dictionary(Of String, Double), model, n, a, b)
            Dim ga As New GeneticAlgorithm(Of ParameterVector, Double)(population, fitness)
            Dim out As New List(Of outPrint)

            Call ga.AddDefaultListener(Sub(x) Call out.Add(x))
            Call ga.Evolve(evolIterations%)

            outPrint = out

            Return ga.Best.vars _
                .ToDictionary(Function(x) x.Name,
                              Function(x) x.value)
        End Function
    End Module
End Namespace
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo
Imports Microsoft.VisualBasic.DataMining.GAF
Imports Microsoft.VisualBasic.DataMining.GAF.Helper
Imports Microsoft.VisualBasic.Mathematical.diffEq
Imports Microsoft.VisualBasic.Serialization.JSON

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
                                Optional popSize% = 100%,
                                Optional evolIterations% = 5000%,
                                Optional ByRef outPrint As List(Of outPrint) = Nothing) As VariableModel()

            Dim vars As VariableModel() = model.params _
                .Join(model.yinit) _
                .ToArray
            Dim population As Population(Of ParameterVector) =
                New ParameterVector() With {
                    .vars = vars
            }.InitialPopulation(popSize%)
            Dim obs As Dictionary(Of String, Double) =
                vars _
                .ToDictionary(Function(x) x.Name,
                              Function(x) x.GetValue)
            Dim fitness As Fitness(Of ParameterVector, Double) =
                New GAFfitness(obs, model, n, a, b)
            Dim ga As New GeneticAlgorithm(Of ParameterVector, Double)(population, fitness)
            Dim out As New List(Of outPrint)

            Call ga.AddDefaultListener(Sub(x)
                                           Call out.Add(x)
#If DEBUG Then
                                           Call x.ToString.__DEBUG_ECHO
#End If
                                       End Sub)
            Call ga.Evolve(evolIterations%)

            outPrint = out

#If DEBUG Then
            Call Console.WriteLine("Observation:")
            Call Console.WriteLine(obs.GetJson)
            Call Console.WriteLine("GAF fitting:")
            Call Console.WriteLine(ga.Best.vars.GetJson)
#End If

            Return ga.Best.vars
        End Function
    End Module
End Namespace
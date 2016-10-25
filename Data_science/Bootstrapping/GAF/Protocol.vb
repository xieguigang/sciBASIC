Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo
Imports Microsoft.VisualBasic.DataMining.GAF
Imports Microsoft.VisualBasic.DataMining.GAF.Helper
Imports Microsoft.VisualBasic.Mathematical.diffEq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical

Namespace GAF

    ''' <summary>
    ''' 参数拟合的方法
    ''' </summary>
    Public Module Protocol

        <Extension> Public Sub Mutate(ByRef array#(), rnd As Random)
            Dim i% = rnd.Next(array.Length)
            Dim n# = array(i)
            Dim power# = Math.Log10(n#) - 1
            Dim sign% = If(rnd.NextBoolean, 1, -1)

            n += sign * (rnd.Next(10) * (10 ^ power))
            If n.Is_NA_UHandle Then
                n = Short.MaxValue
            End If

            array(i) = n
        End Sub

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
                                Optional ByRef outPrint As List(Of outPrint) = Nothing,
                                Optional threshold# = 1,
                                Optional obs As Dictionary(Of String, Double) = Nothing,
                                Optional log10Fit As Boolean = False) As var()

            Dim getVars As Func(Of var()) =
                Function() model.params _
                    .Select(Function(x) New var(x.Name, x.GetValue)) _
                    .Join(model.yinit _
                    .Select(Function(x) New var(x.Name, x.GetValue))) _
                    .ToArray
            Dim population As Population(Of ParameterVector) =
                New ParameterVector() With {
                    .vars = getVars() _
                    .ToArray(Function(x) New var(x.Name, x.value + 10 * Rnd()))
            }.InitialPopulation(popSize%)

            If obs.IsNullOrEmpty Then
                obs = getVars() _
                    .ToDictionary(Function(x) x.Name,
                                  Function(x) 1.0#)
            Else
                Console.Title = obs.GetJson
            End If

            Dim fitness As Fitness(Of ParameterVector, Double) =
                New GAFfitness(obs, model, n, a, b) With {
                    .log10Fitness = log10Fit
            }
            Dim ga As New GeneticAlgorithm(Of ParameterVector, Double)(
                population, fitness)
            Dim out As New List(Of outPrint)

#If DEBUG Then
            Call ga.addIterationListener(
                New Dump With {
                    .a = a,
                    .b = b,
                    .n = n,
                    .model = model.GetType
                })
#End If
            Call ga.AddDefaultListener(Sub(x)
                                           Call out.Add(x)
                                           Call x.ToString.__DEBUG_ECHO
                                       End Sub, threshold)
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
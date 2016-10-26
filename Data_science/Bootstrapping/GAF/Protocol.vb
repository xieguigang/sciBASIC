Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo
Imports Microsoft.VisualBasic.DataMining.GAF
Imports Microsoft.VisualBasic.DataMining.GAF.Helper
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.Calculus
Imports Microsoft.VisualBasic.Serialization.JSON

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
        ''' Using for model testing debug.(测试用)
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
        Public Function Fitting(model As MonteCarlo.Model, n%, a#, b#,
                                Optional popSize% = 100%,
                                Optional evolIterations% = 5000%,
                                Optional ByRef outPrint As List(Of outPrint) = Nothing,
                                Optional threshold# = 0.5,
                                Optional obs As Dictionary(Of String, Double) = Nothing,
                                Optional log10Fit As Boolean = False) As var()

            Dim vars$() = ODEs.GetParameters(model.GetType) _
                .Join(ODEs.GetVariables(model.GetType)) _
                .ToArray

            If obs.IsNullOrEmpty Then
                obs = vars.ToDictionary(
                    Function(x) x,
                    Function(x) 1.0#)
            Else
                Console.Title = obs.GetJson
            End If

            Dim fitness As New GAFfitness(obs, model, n, a, b) With {
                .log10Fitness = log10Fit
            }

            Return vars.__runInternal(
                popSize:=popSize,
                evolIterations:=evolIterations,
                fitness:=fitness,
                outPrint:=outPrint,
                threshold:=threshold)
        End Function

        <Extension>
        Private Function __runInternal(vars$(), popSize%, threshold#, evolIterations%,
                                       fitness As GAFfitness,
                                       ByRef outPrint As List(Of outPrint)) As var()

            Dim population As Population(Of ParameterVector) =
                New ParameterVector() With {
                    .vars = vars.ToArray(
                        Function(x) New var(x, (2 ^ x.Length) * (100000 * New Random().NextDouble)))
            }.InitialPopulation(popSize%)

            Dim ga As New GeneticAlgorithm(Of ParameterVector, Double)(population, fitness)
            Dim out As New List(Of outPrint)
#If DEBUG Then
            Call ga.addIterationListener(
                New Dump With {
                    .a = fitness.a,
                    .b = fitness.b,
                    .n = fitness.n,
                    .model = fitness.Model,
                    .y0 = fitness.y0
                })
#End If
            Call ga.AddDefaultListener(Sub(x)
                                           Call out.Add(x)
                                           Call x.ToString.__DEBUG_ECHO
                                       End Sub, threshold)
            Call ga.Evolve(evolIterations%)

            outPrint = out
#If DEBUG Then
            Call Console.WriteLine("GAF fitting:")
            Call Console.WriteLine(ga.Best.vars.GetJson)
#End If
            Return ga.Best.vars
        End Function

        ''' <summary>
        ''' 用于实际分析的GAF工具
        ''' </summary>
        ''' <param name="observation">用于进行拟合的目标真实的实验数据，模型计算所使用的y0初值从这里面来</param>
        ''' <param name="popSize%"></param>
        ''' <param name="evolIterations%"></param>
        ''' <param name="outPrint"></param>
        ''' <param name="threshold#"></param>
        ''' <param name="log10Fit"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Fitting(Of T As MonteCarlo.Model)(
                         observation As ODEsOut,
                         Optional popSize% = 100%,
                         Optional evolIterations% = 5000%,
                         Optional ByRef outPrint As List(Of outPrint) = Nothing,
                         Optional threshold# = 0.5,
                         Optional log10Fit As Boolean = True,
                         Optional ignores$() = Nothing,
                         Optional initOverrides As Dictionary(Of String, Double) = Nothing) As var()

            Dim vars$() = Model.GetParameters(GetType(T)) _
                .Join(Model.GetVariables(GetType(T))) _
                .ToArray
            Dim fitness As New GAFfitness(GetType(T), observation, initOverrides) With {
                .log10Fitness = log10Fit,
                .Ignores = If(ignores Is Nothing, {}, ignores)
            }

            Return vars.__runInternal(
                popSize:=popSize,
                evolIterations:=evolIterations,
                fitness:=fitness,
                outPrint:=outPrint,
                threshold:=threshold)
        End Function
    End Module
End Namespace
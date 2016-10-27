Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo
Imports Microsoft.VisualBasic.DataMining.Darwinism.GAF
Imports Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper
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

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="array#">需要被拟合的参数列表，在这个函数里面会被修改一点产生突变</param>
        ''' <param name="rnd"></param>
        <Extension> Public Sub Mutate(ByRef array#(), rnd As Random)
            Dim i% = rnd.Next(array.Length)  ' 得到需要被突变的位点在数组中的下标
            Dim n# = Math.Abs(array(i))      ' 得到元素值，由于负数取位数的时候回出错，所以这里取绝对值，因为只需要取位数
            Dim power# = Math.Log10(n#) - 1  ' 取位数
            Dim sign% =
                If(rnd.NextBoolean, 1, -1)

            n += sign * (rnd.Next(1000) * (1000 ^ power))
            If n.IsNaNImaginary Then
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

            Dim vars$() = ODEs.GetParameters(model.GetType).ToArray

            If obs.IsNullOrEmpty Then
                obs = vars.ToDictionary(
                    Function(x) x,
                    Function(x) 1.0#)
            Else
                Console.Title = obs.GetJson
            End If

            Dim fitness As New GAFFitness(obs, model, n, a, b) With {
                .log10Fitness = log10Fit
            }

            Return vars.__runInternal(
                popSize:=popSize,
                evolIterations:=evolIterations,
                fitness:=fitness,
                outPrint:=outPrint,
                threshold:=threshold,
                argsInit:=Nothing)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="vars$">从模型内部定义所解析出来的需要进行拟合的参数的名称列表</param>
        ''' <param name="popSize%"></param>
        ''' <param name="threshold#"></param>
        ''' <param name="evolIterations%"></param>
        ''' <param name="fitness"></param>
        ''' <param name="outPrint"></param>
        ''' <param name="argsInit"></param>
        ''' <returns></returns>
        <Extension>
        Private Function __runInternal(vars$(), popSize%, threshold#, evolIterations%,
                                       fitness As GAFFitness,
                                       ByRef outPrint As List(Of outPrint),
                                       argsInit As Dictionary(Of String, Double)) As var()

            Dim estArgs As var()

            If argsInit.IsNullOrEmpty Then
                estArgs = vars.ToArray(
                    Function(x) New var With {
                        .Name = x,
                        .value = (2 ^ x.Length) * (100000 * New Random().NextDouble)
                    })
            Else
                estArgs = LinqAPI.Exec(Of var) <= From x
                                                  In argsInit
                                                  Where Array.IndexOf(vars, x.Key) > -1
                                                  Select New var With {
                                                      .Name = x.Key,
                                                      .value = x.Value
                                                  }
                Dim varsData As Dictionary(Of var) =
                    estArgs.ToDictionary
                For Each name$ In vars
                    If Not varsData.ContainsKey(name$) Then
                        varsData += New var With {
                            .Name = name,
                            .value = (2 ^ name.Length) * (100 * New Random().NextDouble)
                        }
                    End If
                Next
            End If

            Dim population As Population(Of ParameterVector) =
                New ParameterVector() With {
                    .vars = estArgs
            }.InitialPopulation(popSize%)

            population.Parallel = True

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
                         Optional evolIterations% = Integer.MaxValue%,
                         Optional ByRef outPrint As List(Of outPrint) = Nothing,
                         Optional threshold# = 0.5,
                         Optional log10Fit As Boolean = True,
                         Optional ignores$() = Nothing,
                         Optional initOverrides As Dictionary(Of String, Double) = Nothing,
                         Optional estArgsBase As Dictionary(Of String, Double) = Nothing) As var()

            Dim vars$() = Model.GetParameters(GetType(T)).ToArray  ' 对于参数估算而言，y0初始值不需要变化了，使用实验观测值
            Dim fitness As New GAFFitness(GetType(T), observation, initOverrides) With {
                .log10Fitness = log10Fit,
                .Ignores = If(ignores Is Nothing, {}, ignores)
            }

            Call $"Ignores of {fitness.Ignores.GetJson}".__DEBUG_ECHO

            Return vars.__runInternal(
                popSize:=popSize,
                evolIterations:=evolIterations,
                fitness:=fitness,
                outPrint:=outPrint,
                threshold:=threshold,
                argsInit:=estArgsBase)
        End Function
    End Module
End Namespace
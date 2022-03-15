#Region "Microsoft.VisualBasic::42944117fde7a7baaef735a443411fc5, sciBASIC#\Data_science\MachineLearning\Bootstrapping\Darwinism\GAF\ODEs\Protocol.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 479
    '    Code Lines: 337
    ' Comment Lines: 88
    '   Blank Lines: 54
    '     File Size: 21.44 KB


    '     Enum MutateLevels
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Module Protocol
    ' 
    '         Function: __runInternal, Balance, CreateVector, (+4 Overloads) Fitting, GetRandomParameters
    '                   y0
    ' 
    '         Sub: Mutate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF.Helper
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Serialization.JSON
Imports DynamicsSystem = Microsoft.VisualBasic.Math.Calculus.ODEs

Namespace Darwinism.GAF.ODEs

    Public Enum MutateLevels As Integer
        Low = 1
        Medium = 2
        High = 3
        Ultra = 5
    End Enum

    ''' <summary>
    ''' 参数拟合的方法
    ''' </summary>
    Public Module Protocol

        ''' <summary>
        ''' Gets the first value as ``y0`` from the inputs samples
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        <Extension>
        Public Function y0(data As IEnumerable(Of NamedValue(Of Double()))) As Dictionary(Of String, Double)
            Return data.ToDictionary(Function(x) x.Name, Function(x) x.Value(Scan0))
        End Function

        ''' <summary>
        ''' Mutate a bit in an array.
        ''' </summary>
        ''' <param name="array#">The abstraction of a chromosome(parameter list).
        ''' (需要被拟合的参数列表，在这个函数里面会被修改一点产生突变)
        ''' </param>
        ''' <param name="rnd"></param>
        <Extension> Public Sub Mutate(ByRef array#(), rnd As Random, radicals#)
            Dim i% = rnd.Next(array.Length)  ' 得到需要被突变的位点在数组中的下标
            Dim n# = Math.Abs(array(i))      ' 得到元素值，由于负数取位数的时候回出错，所以这里取绝对值，因为只需要取位数
            Dim d# = If(rnd.NextDouble <= radicals, 1, 2)  ' radicals越大，则有越高的概率是发生很大的突变值的，反之会-1，即发生很小的突变
            Dim power# = Math.Log10(n#) - d#  ' 取位数
            Dim sign% =
                If(rnd.NextBoolean, 1, -1)

            n += sign * (rnd.NextDouble * 10 * (10 ^ power))

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
                                Optional log10Fit As Boolean = False,
                                Optional randomGenerator As IRandomSeeds = Nothing,
                                Optional mutateLevel As MutateLevels = MutateLevels.Low,
                                Optional print As Action(Of outPrint, var()) = Nothing,
                                Optional radicals# = 0.3,
                                Optional parallel As ParallelComputing(Of ParameterVector) = Nothing,
                                Optional weights As Dictionary(Of String, Double) = Nothing) As var()

            Dim vars$() = DynamicsSystem.GetParameters(model.GetType).ToArray

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
                base:=Nothing,
                randomGenerator:=randomGenerator,
                mutateLevel:=mutateLevel,
                print:=print,
                radicals:=radicals,
                parallel:=parallel,
                weights:=weights)
        End Function

        <Extension>
        Public Function Balance(vars$(), weights As Dictionary(Of String, Double)) As Dictionary(Of String, Double)
            Dim gaps As New List(Of String)

            For Each var In vars
                If Not weights.ContainsKey(var) Then
                    gaps.Add(var)
                End If
            Next

            If gaps.Count = 0 Then
                Return weights
            End If

            Dim splits = 1 - weights.Values.Sum

            If splits <= 0 Then
                ' 已经超过或者等于1了，则其他的都设置为零
                For Each var In gaps
                    Call weights.Add(var, 0R)
                Next
            Else
                splits /= gaps.Count

                For Each var In gaps
                    Call weights.Add(var, splits)
                Next
            End If

            Return weights
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
        ''' <param name="base"></param>
        ''' <param name="weights">Weights for variable fitness calcaulation</param>
        ''' <returns></returns>
        <Extension>
        Private Function __runInternal(vars$(), popSize%, threshold#, evolIterations%,
                                       fitness As GAFFitness,
                                       ByRef outPrint As List(Of outPrint),
                                       base As Dictionary(Of String, Double),
                                       randomGenerator As IRandomSeeds,
                                       mutateLevel As MutateLevels,
                                       print As Action(Of outPrint, var()),
                                       radicals#,
                                       parallel As ParallelComputing(Of ParameterVector),
                                       weights As Dictionary(Of String, Double)) As var()

            If Not weights Is Nothing Then
                fitness.weights = fitness _
                    .modelVariables _
                    .Where(Function(v) Array.IndexOf(fitness.Ignores, v) = -1) _
                    .ToArray _
                    .Balance(weights)
                Call $"Weights fitness average is {fitness.weights.GetJson}".__DEBUG_ECHO
            Else
                Call "Using normal fitness average calculation...".__DEBUG_ECHO
            End If

            If randomGenerator Is Nothing Then
                randomGenerator = Function() New Random
            End If
            If print Is Nothing Then
                print = Sub(x, v) Call x.ToString.__DEBUG_ECHO
            End If

            Dim population As Population(Of ParameterVector) =
                New ParameterVector(seeds:=randomGenerator) With {
                    .vars = vars.CreateVector(randomGenerator, base),
                    .MutationLevel = mutateLevel,
                    .radicals = radicals
            }.InitialPopulation(popSize%, parallel)

            Call $"Fitness using log10(x) is {If(fitness.log10Fitness, "enabled", "disabled")}".Warning

#If Not DEBUG Then
            population.Parallel = True
#End If
            Dim ga As New GeneticAlgorithm(Of ParameterVector)(
                population,
                fitness,
                randomGenerator)
            Dim out As New List(Of outPrint)
            '#If DEBUG Then
            '            Call ga.addIterationListener(
            '                New Dump With {
            '                    .a = fitness.a,
            '                    .b = fitness.b,
            '                    .n = fitness.n,
            '                    .model = fitness.Model,
            '                    .y0 = fitness.y0
            '                })
            '#End If
            Dim reporter As New EnvironmentDriver(Of ParameterVector)(ga) With {
                .Iterations = evolIterations,
                .Threshold = threshold
            }

            Call reporter _
                .AttachReporter(Sub(i, fit, environment)
                                    Dim output = EnvironmentDriver(Of ParameterVector).CreateReport(i, fit, environment)

                                    Call out.Add(output)
                                    Call print(output, ga.Best.vars.Select(Function(v) New var(v)))
                                End Sub) _
                .Train()

            outPrint = out
#If DEBUG Then
            Call Console.WriteLine("GAF fitting:")
            Call Console.WriteLine(ga.Best.vars.GetJson)
#End If
            Return ga.Best.vars
        End Function

        <Extension>
        Public Function CreateVector(vars$(), randomGenerator As IRandomSeeds, Optional base As Dictionary(Of String, Double) = Nothing) As var()
            Dim vector As var()

            If base.IsNullOrEmpty Then
                vector = vars.Select(
                    Function(x) New var With {
                        .Name = x,
                        .value = 0.5R
                    }).ToArray
            Else
                vector = LinqAPI.Exec(Of var) <=
 _
                    From x
                    In base
                    Where Array.IndexOf(vars, x.Key) > -1
                    Select New var With {
                        .Name = x.Key,
                        .value = x.Value
                    }

                Dim vData As Dictionary(Of var) = vector.ToDictionary

                For Each name$ In vars
                    If Not vData.ContainsKey(name$) Then
                        vData += New var With {
                            .Name = name,
                            .value = (2 ^ name.Length) * (100 * randomGenerator().NextDouble)
                        }
                    End If
                Next

                Return vData.Values.ToArray
            End If

            Return vector
        End Function

        ''' <summary>
        ''' 用于实际分析的GAF工具
        ''' </summary>
        ''' <param name="observation">用于进行拟合的目标真实的实验数据，模型计算所使用的y0初值从这里面来，这个数据对象只要求y属性具有实验数据就行了</param>
        ''' <param name="popSize%">
        ''' 更小的种群规模能够产生更快的进化速度，更大的种群规模能够产生更多的解集
        ''' </param>
        ''' <param name="evolIterations%"></param>
        ''' <param name="outPrint"></param>
        ''' <param name="threshold#"></param>
        ''' <param name="log10Fit">In the most of situation, there is no required of enable this feature.</param>
        ''' <param name="radicals">参数值介于[0-1]之间</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' ###### 2016-11-28
        ''' 一般情况下，<paramref name="log10Fit"/>会导致曲线失真，所以默认关闭这个参数
        ''' </remarks>
        <Extension>
        Public Function Fitting(Of T As MonteCarlo.Model)(
                         observation As ODEsOut,
                         Optional popSize% = 200%,
                         Optional evolIterations% = Integer.MaxValue%,
                         Optional ByRef outPrint As List(Of outPrint) = Nothing,
                         Optional threshold# = 0.5,
                         Optional log10Fit As Boolean = False,
                         Optional ignores$() = Nothing,
                         Optional initOverrides As Dictionary(Of String, Double) = Nothing,
                         Optional estArgsBase As Dictionary(Of String, Double) = Nothing,
                         Optional isRefModel As Boolean = False,
                         Optional randomGenerator As IRandomSeeds = Nothing,
                         Optional mutateLevel As MutateLevels = MutateLevels.Low,
                         Optional print As Action(Of outPrint, var()) = Nothing,
                         Optional radicals# = 0.3,
                         Optional parallel As ParallelComputing(Of ParameterVector) = Nothing,
                         Optional weights As Dictionary(Of String, Double) = Nothing) As var()

            Dim vars$() = Model.GetParameters(GetType(T)).ToArray  ' 对于参数估算而言，y0初始值不需要变化了，使用实验观测值
            Dim fitness As New GAFFitness(GetType(T), observation, initOverrides, isRefModel) With {
                .log10Fitness = log10Fit,
                .Ignores = If(ignores Is Nothing, {}, ignores)
            }

            Call $"Ignores of {fitness.Ignores.GetJson}".__DEBUG_ECHO
            Call $"Observation data length: {observation.x.Length}".__DEBUG_ECHO

            Return vars.__runInternal(
                popSize:=popSize,
                evolIterations:=evolIterations,
                fitness:=fitness,
                outPrint:=outPrint,
                threshold:=threshold,
                base:=estArgsBase,
                randomGenerator:=randomGenerator,
                mutateLevel:=mutateLevel,
                print:=print,
                radicals:=radicals,
                parallel:=parallel,
                weights:=weights)
        End Function

        <Extension>
        Public Function GetRandomParameters(model As Type, Optional range As DoubleRange = Nothing) As Dictionary(Of String, Double)
            Dim vars$() = MonteCarlo.Model.GetParameters(model).ToArray

            If range Is Nothing Then
                range = New DoubleRange(-10, 10)
            End If

            Dim out As New Dictionary(Of String, Double)
            Dim rand As New Random

            For Each v$ In vars$
                Call out.Add(v, rand.NextDouble(range))
            Next

            Return out
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
                         observation As IEnumerable(Of NamedCollection(Of Double)), x#(),
                         Optional popSize% = 100%,
                         Optional evolIterations% = Integer.MaxValue%,
                         Optional ByRef outPrint As List(Of outPrint) = Nothing,
                         Optional threshold# = 0.5,
                         Optional log10Fit As Boolean = False,
                         Optional ignores$() = Nothing,
                         Optional initOverrides As Dictionary(Of String, Double) = Nothing,
                         Optional estArgsBase As Dictionary(Of String, Double) = Nothing,
                         Optional isRefModel As Boolean = False,
                         Optional randomGenerator As IRandomSeeds = Nothing,
                         Optional mutateLevel As MutateLevels = MutateLevels.Low,
                         Optional print As Action(Of outPrint, var()) = Nothing,
                         Optional radicals# = 0.3,
                         Optional parallel As ParallelComputing(Of ParameterVector) = Nothing,
                         Optional weights As Dictionary(Of String, Double) = Nothing) As var()

            Return New ODEsOut With {
                .y = observation.ToDictionary,
                .x = x#
            }.Fitting(Of T)(popSize:=popSize,
                            estArgsBase:=estArgsBase,
                            evolIterations:=evolIterations,
                            ignores:=ignores,
                            initOverrides:=initOverrides,
                            isRefModel:=isRefModel,
                            log10Fit:=log10Fit,
                            outPrint:=outPrint,
                            randomGenerator:=randomGenerator,
                            threshold:=threshold,
                            mutateLevel:=mutateLevel,
                            print:=print,
                            radicals:=radicals,
                            parallel:=parallel,
                            weights:=weights)
        End Function

        ''' <summary>
        ''' 用于实际分析的GAF工具
        ''' </summary>
        ''' <param name="popSize%">
        ''' 更小的种群规模能够产生更快的进化速度，更大的种群规模能够产生更多的解集
        ''' </param>
        ''' <param name="evolIterations%"></param>
        ''' <param name="outPrint"></param>
        ''' <param name="threshold#"></param>
        ''' <param name="radicals">参数值介于[0-1]之间</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension>
        Public Function Fitting(Of T As MonteCarlo.Model)(
                              driver As Fitness(Of ParameterVector),
                              Optional popSize% = 200%,
                              Optional evolIterations% = Integer.MaxValue%,
                              Optional ByRef outPrint As List(Of outPrint) = Nothing,
                              Optional threshold# = 0.5,
                              Optional base As Dictionary(Of String, Double) = Nothing,
                              Optional randomGenerator As IRandomSeeds = Nothing,
                              Optional mutateLevel As MutateLevels = MutateLevels.Low,
                              Optional print As Action(Of outPrint, var()) = Nothing,
                              Optional radicals# = 0.3,
                              Optional parallel As ParallelComputing(Of ParameterVector) = Nothing) As var()

            Dim model As Type = GetType(T)
            Dim vars$() = MonteCarlo.Model.GetParameters(model).ToArray

            If randomGenerator Is Nothing Then
                randomGenerator = Function() New Random
            End If
            If print Is Nothing Then
                print = Sub(x, v) Call x.ToString.__DEBUG_ECHO
            End If

            Dim population As Population(Of ParameterVector) =
                New ParameterVector(seeds:=randomGenerator) With {
                    .vars = vars.CreateVector(randomGenerator, base),
                    .MutationLevel = mutateLevel,
                    .radicals = radicals
            }.InitialPopulation(popSize%, parallel)

#If Not DEBUG Then
            population.Parallel = True
#End If
            Dim ga As New GeneticAlgorithm(Of ParameterVector)(
                population,
                driver,
                randomGenerator)
            Dim out As New List(Of outPrint)
            Dim reporter As New EnvironmentDriver(Of ParameterVector)(ga) With {
                .Iterations = evolIterations,
                .Threshold = threshold
            }

            Call reporter _
                .AttachReporter(Sub(i, fit, envir)
                                    Dim output = EnvironmentDriver(Of ParameterVector).CreateReport(i, fit, envir)

                                    Call out.Add(output)
                                    Call print(output, ga.Best.vars.Select(Function(v) New var(v)))
                                End Sub) _
                .Train()

            outPrint = out
#If DEBUG Then
            Call Console.WriteLine("GAF fitting:")
            Call Console.WriteLine(ga.Best.vars.GetJson)
#End If
            Return ga.Best.vars
        End Function
    End Module
End Namespace

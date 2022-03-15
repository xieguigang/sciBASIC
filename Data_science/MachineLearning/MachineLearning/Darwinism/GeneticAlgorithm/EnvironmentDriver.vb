#Region "Microsoft.VisualBasic::4b42f9d82c258d6ae599c1b1bf3b9ae7, sciBASIC#\Data_science\MachineLearning\MachineLearning\Darwinism\GeneticAlgorithm\EnvironmentDriver.vb"

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

    '   Total Lines: 165
    '    Code Lines: 105
    ' Comment Lines: 35
    '   Blank Lines: 25
    '     File Size: 6.52 KB


    '     Class EnvironmentDriver
    ' 
    '         Properties: Iterations, Threshold
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CreateReport
    ' 
    '         Sub: reset, Terminate, Train
    ' 
    '     Structure outPrint
    ' 
    '         Properties: chromosome, fit, iter, MutationRate
    ' 
    '         Function: ToString
    ' 
    '         Sub: PrintTitle
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF.Helper
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models
Imports stdNum = System.Math

Namespace Darwinism.GAF

    ''' <summary>
    ''' 发生种群进化所需要的环境压力产生器
    ''' </summary>
    ''' <typeparam name="Chr"></typeparam>
    Public Class EnvironmentDriver(Of Chr As {Class, Chromosome(Of Chr)}) : Inherits IterationReporter(Of GeneticAlgorithm(Of Chr))

        Dim core As GeneticAlgorithm(Of Chr)
        Dim terminated As Boolean = False
        Dim takeBestSnapshot As Action(Of Chr, Double)

        ''' <summary>
        ''' 需要运行的总的迭代次数
        ''' </summary>
        ''' <returns></returns>
        Public Property Iterations As Integer
        Public Property Threshold As Double

        ''' <summary>
        ''' 创建一个新的环境压力驱动程序,用来驱动模型的进化学习
        ''' </summary>
        ''' <param name="ga"></param>
        Sub New(ga As GeneticAlgorithm(Of Chr), takeBestSnapshot As Action(Of Chr, Double), Optional iterations% = 500000)
            Me.core = ga
            Me.takeBestSnapshot = takeBestSnapshot
            Me.Iterations = iterations
        End Sub

        Public Overrides Sub Train(Optional parallel As Boolean = False)
            Dim errStatSize As Integer = 200
            Dim errors As New Queue(Of Double)(capacity:=errStatSize)
            Dim previousErrAverage As Double = Double.MaxValue

            ' 如果这个队列中的误差值全部都是一样的话
            ' 则将当前的种群销毁
            ' 将best突变一个位点后，自动重新初始化
            For i As Integer = 0 To errStatSize
                Call errors.Enqueue(Long.MaxValue)
            Next

            terminated = False

            For i As Integer = 0 To Iterations
                If terminated Then
                    Exit For
                Else
                    Call core.Evolve()
                End If

                With core.GetFitness(core.Best)
                    If Not reporter Is Nothing Then
                        Call reporter(i, .ByRef, core)
                    Else
                        Call .DoCall(core.Best.PipeOf(takeBestSnapshot))
                    End If

                    ' NaN的结果值与阈值相比较也是小于零的
                    ' 在这里跳过NaN值的测试
                    If Not .IsNaNImaginary AndAlso .CompareTo(Threshold) < 0 Then
                        Exit For
                    Else
                        Call errors.Enqueue(.ByRef)
                        Call errors.Dequeue()

                        Dim firstError# = errors.First

                        If stdNum.Abs(firstError - Threshold) > 0.01 AndAlso errors.All(Function(e) e = firstError) Then
                            Call .DoCall(AddressOf reset)

                            ' 如果在这里不替换一下的话
                            ' 会导致频繁出现重置的现象
                            For j As Integer = 0 To errStatSize / 2
                                Call errors.Enqueue(Long.MaxValue)
                                Call errors.Dequeue()
                            Next
                        End If
                    End If
                End With
            Next

            Call "Exit GA training loop due to the reason of reach iteration Upbound...".__DEBUG_ECHO
        End Sub

        Private Sub reset(fitness As Double)
            ' 因为已经很长时间没有变化误差了
            ' 所以最佳的个体肯定已经陷入了局部最优
            Dim bestSeed As Chr = core.Best
            Dim seed As Chr = bestSeed _
                .Mutate _
                .With(Sub(c) c.MutationRate = core.Best.MutationRate)
            ' do not add the local best result when reset the GA system
            ' so add base is set to false
            Dim newPop As Population(Of Chr) = core.Best.InitialPopulation(
                population:=New Population(Of Chr)(core.populationCreator(), core.population.Pcompute) With {
                    .capacitySize = core.population.capacitySize
                },
                addBase:=False
            )
            Dim newCore As New GeneticAlgorithm(Of Chr)(
                population:=newPop,
                fitnessFunc:=core.GetRawFitnessModel,
                replacementStrategy:=core.popStrategy.type,
                seeds:=core.seeds
            )

            core = newCore

            Call "GA module do RE-seeding as local optimal solution was found...".Warning
            Call takeBestSnapshot(bestSeed, fitness)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <example>
        ''' ' If fitness is satisfying - we can stop Genetic algorithm
        ''' 
        ''' If bestFit &lt;= Threshold Then
        '''     Call ga.Terminate()
        ''' End If
        ''' </example>
        Public Sub Terminate()
            Me.terminated = True
        End Sub

        Public Shared Function CreateReport(iteration%, fitness#, ga As GeneticAlgorithm(Of Chr)) As outPrint
            Dim best As Chr = ga.Best
            Dim bestFit As Double = ga.GetFitness(best)

            ' Listener prints best achieved solution
            Return New outPrint With {
                .iter = iteration,
                .fit = bestFit,
                .chromosome = best.ToString,
                .MutationRate = best.MutationRate
            }
        End Function
    End Class

    Public Structure outPrint

        Public Property iter%
        Public Property fit#
        Public Property chromosome$
        Public Property MutationRate#

        Public Shared Sub PrintTitle()
            ' just for pretty print
            Console.WriteLine($"{NameOf(outPrint.iter)}{vbTab}{NameOf(outPrint.fit)}{vbTab}{NameOf(outPrint.chromosome)}({NameOf(outPrint.MutationRate)})")
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"{iter}{vbTab}{fit}{vbTab}{chromosome}({MutationRate})"
        End Function
    End Structure
End Namespace

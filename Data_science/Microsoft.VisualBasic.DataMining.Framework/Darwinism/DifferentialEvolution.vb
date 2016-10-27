Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining
Imports Microsoft.VisualBasic.DataMining.Darwinism
Imports Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper.ListenerHelper
Imports Microsoft.VisualBasic.DataMining.Darwinism.Models
Imports Microsoft.VisualBasic.Language

Namespace Darwinism

    Public Interface IIndividual : Inherits Chromosome(Of IIndividual), ICloneable
        Function Yield(i%) As Double
        Sub Put(i%, value#)
    End Interface

    ''' <summary>
    ''' In evolutionary computation, differential evolution (DE) is a method that optimizes a problem by 
    ''' iteratively trying to improve a candidate solution with regard to a given measure of quality. 
    ''' Such methods are commonly known as metaheuristics as they make few or no assumptions about the 
    ''' problem being optimized and can search very large spaces of candidate solutions. However, 
    ''' metaheuristics such as DE do not guarantee an optimal solution is ever found.
    ''' 
    ''' DE Is used For multidimensional real-valued functions but does Not use the gradient Of the problem 
    ''' being optimized, which means DE does Not require For the optimization problem To be differentiable 
    ''' As Is required by classic optimization methods such As gradient descent And quasi-newton methods. 
    ''' DE can therefore also be used On optimization problems that are Not even continuous, are noisy, 
    ''' change over time, etc.[1]
    ''' 
    ''' DE optimizes a problem by maintaining a population Of candidate solutions And creating New candidate 
    ''' solutions by combining existing ones according To its simple formulae, And Then keeping whichever 
    ''' candidate solution has the best score Or fitness On the optimization problem at hand. In this way 
    ''' the optimization problem Is treated As a black box that merely provides a measure Of quality given 
    ''' a candidate solution And the gradient Is therefore Not needed.
    ''' 
    ''' DE Is originally due To Storn And Price.[2][3] Books have been published On theoretical And practical 
    ''' aspects Of Using DE In parallel computing, multiobjective optimization, constrained optimization, 
    ''' And the books also contain surveys of application areas.[4][5][6][7] Excellent surveys on the 
    ''' multi-faceted research aspects of DE can be found in journal articles Like.[8][9]
    ''' </summary>
    Public Module DifferentialEvolution

        Public Function RMS(a#(), b#()) As Double
            Dim sum#
            Dim n% = a.Length

            For i As Integer = 0 To n - 1
                sum += (a(i) - b(i)) ^ 2
            Next

            Return Math.Sqrt(sum)
        End Function

        Public Delegate Function [New](Of Individual As IIndividual)(seed As Random) As Individual

        ''' <summary>
        ''' Initialize population with individuals that have been initialized with uniform random noise
        ''' uniform noise means random value inside your search space
        ''' </summary>
        ''' <param name="__new"></param>
        ''' <param name="PopulationSize%"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function GetPopulation(Of Individual As IIndividual)(__new As [New](Of Individual), Optional PopulationSize% = 20) As List(Of Individual)
            Dim population As New List(Of Individual)
            Dim rand As New Random

            For i As Integer = 0 To PopulationSize - 1
                population += __new(seed:=rand)
            Next

            Return population
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="Individual"></typeparam>
        ''' <param name="target"></param>
        ''' <param name="[new]">How to creates a new <typeparamref name="Individual"/></param>
        ''' <param name="N%">dimensionality of problem, means how many variables problem has.</param>
        ''' <param name="threshold#"></param>
        ''' <param name="maxIterations%"></param>
        ''' <param name="F">differential weight [0,2]</param>
        ''' <param name="CR">crossover probability [0,1]</param>
        ''' <param name="PopulationSize%"></param>
        ''' <returns></returns>
        Public Function Evolution(Of Individual As IIndividual)(
                                         target As Func(Of Individual, Double),
                                          [new] As [New](Of Individual),
                                           N%,
                                     Optional F As Double = 1,
                                    Optional CR As Double = 0.5,
                                    Optional threshold# = 0.1,
                                    Optional maxIterations% = 500000,
                                    Optional PopulationSize% = 20,
                                    Optional iteratePrints As Action(Of outPrint) = Nothing) As Individual

            ' linked list that has our population inside
            Dim population As List(Of Individual) = [new].GetPopulation(PopulationSize)
            Dim bestFit# = Integer.MaxValue
            Dim fitnessFunction As Func(Of Individual, Double) = AddressOf New FitnessPool(Of Individual, Double)(target).Fitness
            Dim i As int = Scan0
            Dim random As New Random

            ' main loop of evolution.
            Do While (++i < maxIterations)
                For j As Integer = 0 To PopulationSize - 1
                    ' calculate New candidate solution

                    ' pick random point from population
                    Dim x = Math.Floor(random.NextDouble * (PopulationSize - 1))
                    Dim a, b, c As Integer

                    ' pick three different random points from population
                    Do While (a = x)
                        a = Math.Floor(random.NextDouble * (PopulationSize - 1))
                    Loop
                    Do While (b = x OrElse b = a)
                        b = Math.Floor(random.NextDouble * (PopulationSize - 1))
                    Loop
                    Do While (c = x OrElse c = a OrElse c = b)
                        c = Math.Floor(random.NextDouble * (PopulationSize - 1))
                    Loop

                    ' Pick a random index [0-Dimensionality]
                    Dim R = random.Next(N)

                    ' Compute the agent's new position
                    Dim original As Individual = population(x)
                    Dim candidate As Individual = DirectCast(original.Clone, Individual)

                    Dim individual1 As Individual = population(a)
                    Dim individual2 As Individual = population(b)
                    Dim individual3 As Individual = population(c)

                    ' if(i==R | i<CR)
                    ' candidate=a+f*(b-c)
                    ' else
                    ' candidate=x
                    If random.NextDouble < CR Then
                        Call candidate.Put(R, individual1.Yield(R) + F * (individual2.Yield(R) - individual3.Yield(R)))
                    End If ' else isn't needed because we cloned original to candidate

                    ' see if Is better than original, if so replace
                    Dim originalFitness# = fitnessFunction(original)
                    Dim candidateFitness# = fitnessFunction(candidate)

                    If (originalFitness > candidateFitness) Then
                        population.Remove(original)
                        population.Add(candidate)

                        If bestFit > candidateFitness Then
                            bestFit = candidateFitness

                            Dim out As New outPrint With {
                                .fit = bestFit,
                                .chromosome = candidate.ToString,
                                .iter = i
                            }
                            If Not iteratePrints Is Nothing Then
                                Call iteratePrints(out)
#If DEBUG Then
                                Call Console.WriteLine(out.ToString)
#End If
                            Else
                                Call Console.WriteLine(out.ToString)
                            End If
                        End If
                    End If

                    If bestFit <= threshold Then
                        Exit Do
                    End If
                Next
            Loop

            ' find best candidate solution
            Dim bestFitness As Individual = [new](random)
            i = 0
            Do While (++i < PopulationSize)
                Dim candidate As Individual = population(i.value - 1)
                If (fitnessFunction(bestFitness) > fitnessFunction(candidate)) Then
                    bestFitness = candidate
                End If
            Loop

            ' Returns your solution
            Return bestFitness
        End Function
    End Module
End Namespace
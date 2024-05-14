#Region "Microsoft.VisualBasic::28c656291bbf7a7e66b80961b1d61a2b, Data_science\Mathematica\Math\GeneticProgramming\evolution\Evolution.vb"

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

    '   Total Lines: 220
    '    Code Lines: 165
    ' Comment Lines: 16
    '   Blank Lines: 39
    '     File Size: 9.24 KB


    '     Class Evolution
    ' 
    '         Properties: ExpressionFactory
    ' 
    '         Function: breadChildren, evolve, evolvePolyFor, evolveTreeFor, tournamentSelection
    ' 
    '         Sub: breadNewPopulation, evaluatePopulation, initPopulation, mutate
    '         Enum EvolutionType
    ' 
    '             GA, GP
    ' 
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language.Java.Arrays
Imports Microsoft.VisualBasic.Math.Scripting
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.model
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.model.factory
Imports rndf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace evolution

    Public Class Evolution
        Private VERBOSE As Boolean = False

        Private factory As ExpressionFactory
        Private config As Configuration
        Private population As IList(Of Individual)
        Private dataTuples As IList(Of DataPoint)

        Public Overridable WriteOnly Property ExpressionFactory As ExpressionFactory
            Set(value As ExpressionFactory)
                SyncLock Me
                    factory = value
                End SyncLock
            End Set
        End Property

        Public Overridable Function evolvePolyFor(dataTuples As IEnumerable(Of DataPoint), configuration As GAConfiguration) As EvolutionResult
            SyncLock Me
                Me.dataTuples = New List(Of DataPoint)(dataTuples)
                config = configuration
                Return evolve(EvolutionType.GA)
            End SyncLock
        End Function

        Public Overridable Function evolveTreeFor(dataTuples As IEnumerable(Of DataPoint), configuration As GPConfiguration) As EvolutionResult
            SyncLock Me
                Me.dataTuples = New List(Of DataPoint)(dataTuples)
                config = configuration

                Return evolve(EvolutionType.GP)
            End SyncLock
        End Function

        Private Function evolve(type As EvolutionType) As EvolutionResult

            Dim start = CurrentUnixTimeMillis

            ' Genetic Algorithm and Genetic Programming
            GAPolynomial.Objective = config.objective.objective
            GPTree.Objective = config.objective.objective

            ' generate initial population
            population = New List(Of Individual)(config.populationSize)
            initPopulation(type, config.populationSize)
            evaluatePopulation()

            Dim max As Double = config.maxEpochs
            Dim threshold = config.fitnessThreshold

            Dim fitnessProgress As IList(Of Double) = New List(Of Double)()
            Dim timeProgress As IList(Of Long) = New List(Of Long)()

            ' main evolution loop
            Dim best = population(0)
            Dim epoch As Integer
            epoch = 0

            While epoch < max AndAlso best.Fitness > threshold
                Dim t = CurrentUnixTimeMillis

                breadNewPopulation(type)
                evaluatePopulation()
                best = population(0)

                fitnessProgress.Add(best.Fitness)
                timeProgress.Add(CurrentUnixTimeMillis - t)

                If VERBOSE Then
                    Console.Write("epoch={0:D} t={1:D} best={2:g}" & vbLf, epoch, CurrentUnixTimeMillis - t, best.Fitness)
                End If

                epoch += 1
            End While

            ' clean-up
            population.Clear()

            ' return the best found expression
            Return New EvolutionResult(best.Expression, best.Fitness, CurrentUnixTimeMillis - start, epoch, fitnessProgress, timeProgress)
        End Function

        Private Sub initPopulation(type As EvolutionType, size As Integer)
            Select Case type
                Case EvolutionType.GA
                    Dim order = CType(config, GAConfiguration).initPolyOrder
                    Dim from = CType(config, GAConfiguration).paramRangeFrom
                    Dim [to] = CType(config, GAConfiguration).paramRangeTo
                    For Each wrapper In factory.generatePolyExpressions(size, order, from, [to])
                        population.Add(New GAPolynomial(wrapper))
                    Next
                Case EvolutionType.GP
                    Dim depth = CType(config, GPConfiguration).initTreeDepth
                    For Each wrapper In factory.generateExpressions(size, depth)
                        population.Add(New GPTree(wrapper))
                    Next
            End Select
        End Sub

        Private Sub evaluatePopulation()
            For Each individual In population
                individual.computeFitness(dataTuples)
            Next
            population.Sort()
        End Sub

        Private Sub breadNewPopulation(type As EvolutionType)
            ' parent selection from the population
            Dim parents As IList(Of Individual) = tournamentSelection()
            ' recombination of parents to create new children
            Dim children = breadChildren(type, parents)

            ' combine parents and children to new population
            population.Clear()
            CType(population, List(Of Individual)).AddRange(parents)
            CType(population, List(Of Individual)).AddRange(children)
            ' apply mutation on new population (excluding the best individual)
            mutate(type, population.subList(1, population.Count))

            ' fill the rest of the population
            Dim rest = config.populationSize - population.Count
            If rest > 0 Then
                initPopulation(type, rest)
            End If
        End Sub

        Private Function tournamentSelection() As IList(Of Individual)
            Dim size = config.selectionSize
            Dim tournament = config.tournamentSize
            Dim selection As New List(Of Individual)(size)

            ' set the first-one as the best to ensure it'll survive
            Dim winner = population(0)
            For round = 0 To size - 1
                ' select the winner of one tournament
                For i = 0 To tournament - 1
                    Dim one = rndf.[Next](population)
                    If one.CompareTo(winner) < 0 Then
                        winner = one
                    End If
                Next
                selection.Add(winner)
                winner = Nothing
            Next

            Return selection
        End Function

        Private Function breadChildren(type As EvolutionType, parents As IList(Of Individual)) As IList(Of Individual)
            Select Case type
                Case EvolutionType.GA
                    Dim gaChildren As IList(Of Individual) = New List(Of Individual)(parents.Count)

                    ' copy all parents
                    For Each parent In parents
                        gaChildren.Add(New GAPolynomial(DirectCast(DirectCast(parent, GAPolynomial).Root.duplicate(), ExpressionWrapper)))
                    Next

                    ' crossover pairs of parents
                    Dim gaCrossover = CType(config, GAConfiguration).crossoverType
                    For i = 0 To gaChildren.Count - 1 Step 2
                        GAPolynomialUtils.crossover(gaCrossover, gaChildren(i), gaChildren(i + 1))
                    Next

                    Return gaChildren
                Case EvolutionType.GP
                    Dim gpChildren As New List(Of Individual)(parents.Count)

                    ' copy all parents
                    For Each parent As Individual In parents
                        gpChildren.Add(New GPTree(DirectCast(DirectCast(parent, GPTree).Root.duplicate(), ExpressionWrapper)))
                    Next

                    ' crossover pairs of parents
                    Dim gpCrossover = CType(config, GPConfiguration).crossoverType
                    For i = 0 To gpChildren.Count - 1 Step 2
                        GPTreeUtils.crossover(gpCrossover, gpChildren(i), gpChildren(i + 1))
                    Next

                    Return gpChildren
            End Select
            Return New List(Of Individual)()
        End Function

        Private Sub mutate(type As EvolutionType, individuals As IList(Of Individual))
            Dim p = config.mutationProbability
            Select Case type
                Case EvolutionType.GA
                    Dim gaMutation = CType(config, GAConfiguration).mutationType
                    Dim from = CType(config, GAConfiguration).paramRangeFrom
                    Dim [to] = CType(config, GAConfiguration).paramRangeTo
                    For Each individual In individuals
                        If rndf.NextDouble() < p Then
                            GAPolynomialUtils.mutation(gaMutation, individual, from, [to])
                        End If
                    Next
                Case EvolutionType.GP
                    Dim gpMutation = CType(config, GPConfiguration).mutationType
                    For Each individual As Individual In individuals
                        If rndf.NextDouble() < p Then
                            GPTreeUtils.mutation(gpMutation, DirectCast(individual, GPTree), factory)
                        End If
                    Next
            End Select
        End Sub

        Private Enum EvolutionType
            GA
            GP
        End Enum
    End Class

End Namespace

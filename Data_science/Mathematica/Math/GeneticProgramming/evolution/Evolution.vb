Imports cz.bia.ea.regression.model
Imports cz.bia.ea.regression.model.factory
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.Language.Java.Arrays
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace evolution

    Public Class Evolution
        Private VERBOSE As Boolean = False

        Private factory As ExpressionFactory
        Private config As Configuration
        Private population As IList(Of Individual)
        Private dataTuples As IList(Of Tuple)

        Public Overridable WriteOnly Property ExpressionFactory As ExpressionFactory
            Set(value As ExpressionFactory)
                SyncLock Me
                    factory = value
                End SyncLock
            End Set
        End Property

        Public Overridable Function evolvePolyFor(dataTuples As IList(Of Tuple), configuration As GAConfiguration) As Result
            SyncLock Me
                Me.dataTuples = dataTuples
                config = configuration


                Return evolve(EvolutionType.GA)
            End SyncLock
        End Function

        Public Overridable Function evolveTreeFor(dataTuples As IList(Of Tuple), configuration As GPConfiguration) As Result
            SyncLock Me
                Me.dataTuples = dataTuples
                config = configuration

                Return evolve(EvolutionType.GP)
            End SyncLock
        End Function

        Private Function evolve(type As EvolutionType) As Result

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
            Return New Result(best.Expression, best.Fitness, CurrentUnixTimeMillis - start, epoch, fitnessProgress, timeProgress)
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
            Dim selection As IList(Of Individual) = New List(Of Individual)(size)

            ' set the first-one as the best to ensure it'll survive
            Dim winner = population(0)
            For round = 0 To size - 1
                ' select the winner of one tournament
                For i = 0 To tournament - 1
                    Dim one = randf.[Next](population)
                    If one.CompareTo(winner) < 0 Then
                        winner = one
                    End If
                Next
                selection.Add(winner)
                winner = Nothing
            Next

            Return selection
        End Function

        Private Function breadChildren(Of T1)(type As EvolutionType, parents As IList(Of T1)) As IList(Of Individual)
            Select Case type
                Case EvolutionType.GA
                    Dim gaChildren As IList(Of GAPolynomial) = New List(Of GAPolynomial)(parents.Count)

                    ' copy all parents
                    For Each parent In CType(parents, IList(Of GAPolynomial))
                        gaChildren.Add(New GAPolynomial(CType(parent.Root.duplicate(), ExpressionWrapper)))
                    Next

                    ' crossover pairs of parents
                    Dim gaCrossover = CType(config, GAConfiguration).crossoverType
                    For i = 0 To gaChildren.Count - 1 Step 2
                        GAPolynomialUtils.crossover(gaCrossover, gaChildren(i), gaChildren(i + 1))
                    Next

                    Return CType(gaChildren, IList(Of Individual))
                Case EvolutionType.GP
                    Dim gpChildren As IList(Of GPTree) = New List(Of GPTree)(parents.Count)

                    ' copy all parents
                    For Each parent In CType(parents, IList(Of GPTree))
                        gpChildren.Add(New GPTree(CType(parent.Root.duplicate(), ExpressionWrapper)))
                    Next

                    ' crossover pairs of parents
                    Dim gpCrossover = CType(config, GPConfiguration).crossoverType
                    For i = 0 To gpChildren.Count - 1 Step 2
                        GPTreeUtils.crossover(gpCrossover, gpChildren(i), gpChildren(i + 1))
                    Next

                    Return CType(gpChildren, IList(Of Individual))
            End Select
            Return New List(Of Individual)()
        End Function

        Private Sub mutate(Of T1)(type As EvolutionType, individuals As IList(Of T1))
            Dim p = config.mutationProbability
            Select Case type
                Case EvolutionType.GA
                    Dim gaMutation = CType(config, GAConfiguration).mutationType
                    Dim from = CType(config, GAConfiguration).paramRangeFrom
                    Dim [to] = CType(config, GAConfiguration).paramRangeTo
                    For Each individual In CType(individuals, IList(Of GAPolynomial))
                        If randf.NextDouble() < p Then
                            GAPolynomialUtils.mutation(gaMutation, individual, from, [to])
                        End If
                    Next
                Case EvolutionType.GP
                    Dim gpMutation = CType(config, GPConfiguration).mutationType
                    For Each individual In CType(individuals, IList(Of GPTree))
                        If randf.NextDouble() < p Then
                            GPTreeUtils.mutation(gpMutation, individual, factory)
                        End If
                    Next
            End Select
        End Sub

        Private Enum EvolutionType
            GA
            GP
        End Enum

        Public Class Result
            Public ReadOnly result As Expression
            Public ReadOnly fitness As Double
            Public ReadOnly time As Long
            Public ReadOnly epochs As Integer
            Public ReadOnly fitnessProgress As IList(Of Double)
            Public ReadOnly timeProgress As IList(Of Long)

            Public Sub New(result As Expression, fitness As Double, time As Long, epochs As Integer, fitnessProgress As IList(Of Double), timeProgress As IList(Of Long))
                Me.result = result
                Me.fitness = fitness
                Me.time = time
                Me.epochs = epochs
                Me.fitnessProgress = fitnessProgress
                Me.timeProgress = timeProgress
            End Sub
        End Class

    End Class

End Namespace

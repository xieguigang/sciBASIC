
' definition of one individual in population
Public  Class Individual

    ' normally DifferentialEvolution uses floating point variables
    Public data1, data2 As Double
    ' but using integers Is possible too  
    Public data3%

    Public Overrides Function ToString() As String
        Return String.Join(",", data1, data2, data3)
    End Function

    Public Function Clone() As Individual
        Return New Individual With {
            .data1 = data1,
            .data2 = data2,
            .data3 = data3
        }
    End Function
End Class

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
Module DifferentialEvolution

    ' Variables
    ' linked list that has our population inside
    Dim population As New List(Of Individual)()
    ' New instance of Random number generator
    Dim random As New Random()
    Dim PopulationSize = 20

    ' differential weight [0,2]
    Dim F As Double = 1
    ' crossover probability [0,1]
    Dim CR As Double = 0.5
    ' dimensionality of problem, means how many variables problem has. this case 3 (data1,data2,data3)
    Dim N = 3

    Sub Main()
        Dim target As New Individual With {.data1 = 1, .data2 = 2, .data3 = 3}
        Dim tt = {target.data1, target.data2, target.data3}
        Dim result = DE(0.05, Function(x)
                                  Return RMS(tt, {x.data1, x.data2, x.data3})
                              End Function)

        RMS(tt, {result.data1, result.data2, result.data3})

        Console.WriteLine(result.ToString)
        Console.ReadKey()
    End Sub

    Public Function RMS(a#(), b#()) As Double
        Dim sum#
        Dim n% = a.Length

        For i As Integer = 0 To n - 1
            sum += (a(i) - b(i)) ^ 2
        Next

        Return Math.Sqrt(sum)
    End Function

    Public Function DE(threshold#, target As Func(Of Individual, Double)) As Individual
        ' Initialize population with individuals that have been initialized with uniform random noise
        ' uniform noise means random value inside your search space
        Dim i = 0
        Do While (i < PopulationSize)
            Dim Individual As New Individual()
            Individual.data1 = random.NextDouble * PopulationSize
            Individual.data2 = random.NextDouble * PopulationSize
            ' integers cant take floating point values And they need to be either rounded
            Individual.data3 = Math.Floor(random.NextDouble * PopulationSize)
            population.Add(Individual)
            i += 1
        Loop

        i = 0
        Dim j%
        ' main loop of evolution.

        Dim bestFit# = Integer.MaxValue
        Dim fitnessFunction As Func(Of Individual, Double) = AddressOf New fitnessCache() With {.cacl = target}.fitnessFunction

        Do While (Not bestFit <= threshold)
            i += 1
            j = 0
            Do While (j < PopulationSize)
                ' calculate New candidate solution

                ' pick random point from population
                Dim x = Math.Floor(random.NextDouble * (population.Count() - 1))
                Dim a, b, c As Integer

                ' pick three different random points from population
                Do While (a = x)
                    a = Math.Floor(random.NextDouble * (population.Count() - 1))
                Loop
                Do While (b = x OrElse b = a)
                    b = Math.Floor(random.NextDouble * (population.Count() - 1))
                Loop
                Do While (c = x OrElse c = a OrElse c = b)
                    c = Math.Floor(random.NextDouble * (population.Count() - 1))
                Loop

                ' Pick a random index [0-Dimensionality]
                Dim R = random.Next(N)

                ' Compute the agent's new position
                Dim original As Individual = population(x)
                Dim candidate As Individual = original.Clone()

                Dim individual1 As Individual = population(a)
                Dim individual2 As Individual = population(b)
                Dim individual3 As Individual = population(c)

                ' if(i==R | i<CR)
                ' candidate=a+f*(b-c)
                ' else
                ' candidate=x
                If (0 = R OrElse random.NextDouble < CR) Then
                    candidate.data1 = individual1.data1 + F * (individual2.data1 - individual3.data1)
                End If ' else isn't needed because we cloned original to candidate
                If (1 = R OrElse random.NextDouble < CR) Then
                    candidate.data2 = individual1.data2 + F * (individual2.data2 - individual3.data2)
                End If
                ' integer work same as floating points but they need to be rounded
                If (2 = R OrElse random.NextDouble < CR) Then
                    candidate.data3 = Math.Floor(individual1.data3 + F * (individual2.data3 - individual3.data3))
                End If

                ' see if Is better than original, if so replace
                Dim candidateFitness# = fitnessFunction(candidate)

                If (fitnessFunction(original) > candidateFitness AndAlso candidateFitness <= bestFit) Then
                    population.Remove(original)
                    population.Add(candidate)
                    bestFit = candidateFitness

                    Call Console.WriteLine(bestFit)
                End If

                j += 1
            Loop
        Loop

        ' find best candidate solution
        i = 0
        Dim bestFitness As New Individual()
        Do While (i < PopulationSize)
            Dim Individual As Individual = population(i)
            If (fitnessFunction(bestFitness) > fitnessFunction(Individual)) Then
                bestFitness = Individual
            End If
            i += 1
        Loop

        ' your solution
        Return bestFitness
    End Function

    Private Class fitnessCache

        Dim cache As New Dictionary(Of String, Double)

        Public cacl As Func(Of Individual, Double)

        ' This function tells how well given individual performs at given problem.
        Public Function fitnessFunction([in] As Individual) As Double
            Dim key$ = [in].ToString
            Dim fitness#

            If cache.ContainsKey(key$) Then
                fitness = cache(key$)
            Else
                fitness = cacl([in])
                cache.Add(key$, fitness)
            End If

            Return fitness
        End Function

    End Class
End Module

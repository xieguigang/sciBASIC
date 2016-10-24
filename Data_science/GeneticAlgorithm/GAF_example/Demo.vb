Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.DataMining.GAF
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class Demo

    Public Shared Sub Main(args As String())
        Dim population As Population(Of MyVector) = createInitialPopulation(5000)
        Dim fitness As Fitness(Of MyVector, Double) = New MyVectorFitness()
        Dim ga As New GeneticAlgorithm(Of MyVector, Double)(population, fitness)

        addListener(ga)
        ga.Evolve(5000)

        Pause()
    End Sub

    ''' <summary>
    ''' The simplest strategy for creating initial population <br/>
    ''' in real life it could be more complex
    ''' </summary>
    Private Shared Function createInitialPopulation(populationSize As Integer) As Population(Of MyVector)
        Dim population As New Population(Of MyVector)()
        Dim base As New MyVector()
        For i As Integer = 0 To populationSize - 1
            ' each member of initial population
            ' is mutated clone of base chromosome
            Dim chr As MyVector = base.Mutate()
            population.Add(chr)
        Next
        Return population
    End Function

    ''' <summary>
    ''' After each iteration Genetic algorithm notifies listener
    ''' </summary>
    Private Shared Sub addListener(ga As GeneticAlgorithm(Of MyVector, Double))
        ' just for pretty print
        Console.WriteLine(String.Format("{0}" & vbTab & "{1}" & vbTab & "{2}", "iter", "fit", "chromosome"))

        ' Lets add listener, which prints best chromosome after each iteration
        ga.addIterationListener(New IterartionListenerAnonymousInnerClassHelper())
    End Sub

    Private Class IterartionListenerAnonymousInnerClassHelper
        Implements IterartionListener(Of MyVector, Double)

        Private ReadOnly threshold As Double = 0.00001

        Public Sub Update(ga As GeneticAlgorithm(Of MyVector, Double)) Implements IterartionListener(Of MyVector, Double).Update

            Dim best As MyVector = ga.Best
            Dim bestFit As Double = ga.Fitness(best)
            Dim iteration As Integer = ga.Iteration

            ' Listener prints best achieved solution
            Console.WriteLine(String.Format("{0}" & vbTab & "{1}" & vbTab & "{2}", iteration, bestFit, best))

            ' If fitness is satisfying - we can stop Genetic algorithm
            If bestFit < Me.threshold Then
                ga.Terminate()
            End If
        End Sub
    End Class

    ''' <summary>
    ''' Chromosome, which represents vector of five integers
    ''' </summary>
    Public Class MyVector
        Implements Chromosome(Of MyVector), ICloneable

        Private Shared ReadOnly random As New Random()

        Private ReadOnly _vector As Integer() = New Integer(4) {}

        ''' <summary>
        ''' Returns clone of current chromosome, which is mutated a bit
        ''' </summary>
        Public Function Mutate() As MyVector Implements Chromosome(Of MyVector).Mutate
            Dim result As MyVector = Me.clone()

            ' just select random element of vector
            ' and increase or decrease it on small value
            Dim index As Integer = random.Next(Me.Vector.Length)
            Dim mutationValue As Integer = random.Next(3) - random.Next(3)
            result.Vector(index) += mutationValue

            Return result
        End Function

        ''' <summary>
        ''' Returns list of siblings <br/>
        ''' Siblings are actually new chromosomes, <br/>
        ''' created using any of crossover strategy
        ''' </summary>
        Public Function Crossover(other As MyVector) As IList(Of MyVector) Implements Chromosome(Of MyVector).Crossover
            Dim thisClone As MyVector = Me.clone()
            Dim otherClone As MyVector = other.clone()

            ' one point crossover
            Dim index As Integer = random.Next(Me.Vector.Length - 1)
            For i As Integer = index To Me.Vector.Length - 1
                Dim tmp As Integer = thisClone.Vector(i)
                thisClone.Vector(i) = otherClone.Vector(i)
                otherClone.Vector(i) = tmp
            Next

            Return {thisClone, otherClone}.ToList
        End Function

        Protected Friend Function clone() As Object Implements ICloneable.Clone
            Dim clone__ As New MyVector()
            Array.Copy(Me.Vector, 0, clone__.Vector, 0, Me.Vector.Length)
            Return clone__
        End Function

        Public Overridable ReadOnly Property Vector As Integer()
            Get
                Return Me._vector
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Me.Vector.GetJson
        End Function
    End Class

    ''' <summary>
    ''' Fitness function, which calculates difference between chromosomes vector
    ''' and target vector
    ''' </summary>
    Public Class MyVectorFitness
        Implements Fitness(Of MyVector, Double)

        Private ReadOnly target As Integer() = {10, 20, 30, 40, 50}

        Public Function Calculate(chromosome As MyVector) As Double Implements Fitness(Of MyVector, Double).Calculate
            Dim delta As Double = 0
            Dim v As Integer() = chromosome.Vector
            For i As Integer = 0 To 4
                delta += Me.sqr(v(i) - Me.target(i))
            Next
            Return delta
        End Function

        Private Function sqr(x As Double) As Double
            Return x * x
        End Function
    End Class
End Class

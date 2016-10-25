Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.DataMining.GAF
Imports Microsoft.VisualBasic.DataMining.GAF.Helper
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class Demo

    Public Shared Sub Main(args As String())
        Dim population As Population(Of MyVector) = New MyVector().InitialPopulation(5000)
        Dim fitness As Fitness(Of MyVector, Double) = New MyVectorFitness()
        Dim ga As New GeneticAlgorithm(Of MyVector, Double)(population, fitness)
        '   Dim out As New List(Of outPrint)

        ga.AddDefaultListener '(Sub(x) Call out.Add(x))
        ga.Evolve(5000)
        '   out.SaveTo("./outPrint.csv")

        Pause()
    End Sub

    ''' <summary>
    ''' Chromosome, which represents vector of five integers
    ''' </summary>
    Public Class MyVector : Implements Chromosome(Of MyVector), ICloneable

        Shared ReadOnly random As New Random()
        ReadOnly _vector As Integer() = New Integer(4) {}

        ''' <summary>
        ''' Returns clone of current chromosome, which is mutated a bit
        ''' </summary>
        Public Function Mutate() As MyVector Implements Chromosome(Of MyVector).Mutate
            Dim result As MyVector = Me.clone()
            Call result._vector.Mutate(random)
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
            Call random.Crossover(thisClone._vector, other._vector)
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
            Return Vector.JoinBy(",")
        End Function
    End Class

    ''' <summary>
    ''' Fitness function, which calculates difference between chromosomes vector
    ''' and target vector
    ''' </summary>
    Public Class MyVectorFitness
        Implements Fitness(Of MyVector, Double)

        ReadOnly target As Integer() = {10, 20, 30, 40, 50}

        Public Function Calculate(chromosome As MyVector) As Double Implements Fitness(Of MyVector, Double).Calculate
            Return FitnessHelper.Calculate(chromosome.Vector, target)
        End Function
    End Class
End Class

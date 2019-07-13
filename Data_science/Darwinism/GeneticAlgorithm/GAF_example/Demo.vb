#Region "Microsoft.VisualBasic::9c591217a593356b481506d2f083bf58, Data_science\Darwinism\GeneticAlgorithm\GAF_example\Demo.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Class Demo
    ' 
    '     Sub: Main
    '     Class MyVector
    ' 
    '         Properties: Vector
    ' 
    '         Function: clone, Crossover, Mutate, ToString
    ' 
    '     Class MyVectorFitness
    ' 
    '         Properties: Cacheable
    ' 
    '         Function: Calculate
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports GAF_example
Imports Microsoft.VisualBasic.Extensions
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF.Helper
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models

Public Class Demo

    Public Shared Sub Main(args As String())
        Dim population As Population(Of MyVector) = New MyVector().InitialPopulation(5000)
        Dim fitness As Fitness(Of MyVector) = New MyVectorFitness()
        Dim ga As New GeneticAlgorithm(Of MyVector)(population, fitness)
        Dim engine As New EnvironmentDriver(Of MyVector)(ga) With {
            .Iterations = 10000,
            .Threshold = 0.005
        }

        Call engine.AttachReporter(Sub(i, e, g) EnvironmentDriver(Of MyVector).CreateReport(i, e, g).ToString.__DEBUG_ECHO)
        Call engine.Train()

        Pause()
    End Sub

    ''' <summary>
    ''' Chromosome, which represents vector of five integers
    ''' </summary>
    Public Class MyVector : Implements Chromosome(Of MyVector), ICloneable

        Shared ReadOnly random As New Random()
        ReadOnly _vector As Double() = {50, 50, 50, 50, 50}

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
        Public Function Crossover(other As MyVector) As IEnumerable(Of MyVector) Implements Chromosome(Of MyVector).Crossover
            Dim thisClone As MyVector = Me.clone()
            Dim otherClone As MyVector = other.clone()
            Call random.Crossover(thisClone._vector, other._vector)
            Return {thisClone, otherClone}
        End Function

        Protected Friend Function clone() As Object Implements ICloneable.Clone
            Dim clone__ As New MyVector()
            Array.Copy(Me.Vector, 0, clone__.Vector, 0, Me.Vector.Length)
            Return clone__
        End Function

        Public Overridable ReadOnly Property Vector As Double()
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
        Implements Fitness(Of MyVector)

        ReadOnly target As Double() = {10, 20, 30, 40, 50}

        Public ReadOnly Property Cacheable As Boolean Implements Fitness(Of MyVector).Cacheable
            Get
                Return False
            End Get
        End Property

        Public Function Calculate(chromosome As MyVector) As Double Implements Fitness(Of MyVector).Calculate
            Return FitnessHelper.Calculate(chromosome.Vector, target)
        End Function
    End Class
End Class

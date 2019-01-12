Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF.Helper
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models

Namespace NeuralNetwork.Accelerator

    Public Module GAExtensions

        <Extension>
        Public Sub RunGAAccelerator(network As Network)
            Dim synapses = network.PopulateAllSynapses.ToArray
            Dim population As Population(Of WeightVector) = New WeightVector().InitialPopulation(5000)
            Dim fitness As Fitness(Of WeightVector) = New Fitness()
            Dim ga As New GeneticAlgorithm(Of WeightVector)(population, fitness)
            Dim engine As New EnvironmentDriver(Of WeightVector)(ga) With {
                .Iterations = 10000,
                .Threshold = 0.005
            }

            Call engine.AttachReporter(Sub(i, e, g) EnvironmentDriver(Of WeightVector).CreateReport(i, e, g).ToString.__DEBUG_ECHO)
            Call engine.Train()
        End Sub
    End Module

    Public Class WeightVector : Implements Chromosome(Of WeightVector), ICloneable

        Dim weights#()

        Shared ReadOnly random As New Random

        Public Function Crossover(another As WeightVector) As IEnumerable(Of WeightVector) Implements Chromosome(Of WeightVector).Crossover
            Dim thisClone As WeightVector = Me.Clone()
            Dim otherClone As WeightVector = another.Clone()
            Call random.Crossover(thisClone.weights, another.weights)
            Return {thisClone, otherClone}
        End Function

        Public Function Mutate() As WeightVector Implements Chromosome(Of WeightVector).Mutate
            Dim result As WeightVector = Me.Clone()
            Call result.weights.Mutate(random)
            Return result
        End Function

        Public Function Clone() As Object Implements ICloneable.Clone
            Dim weights#() = New Double(Me.weights.Length - 1) {}
            Call Array.Copy(Me.weights, Scan0, weights, Scan0, weights.Length)
            Return New WeightVector With {
                .weights = weights
            }
        End Function
    End Class

    Public Class Fitness : Implements Fitness(Of WeightVector)

        Public Function Calculate(chromosome As WeightVector) As Double Implements Fitness(Of WeightVector).Calculate
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
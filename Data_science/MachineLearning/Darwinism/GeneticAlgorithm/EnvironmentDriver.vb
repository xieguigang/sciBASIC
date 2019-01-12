Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models

Namespace Darwinism.GAF

    Public Class EnvironmentDriver(Of Chr As Chromosome(Of Chr)) : Inherits IterationReporter(Of GeneticAlgorithm(Of Chr))

        Dim core As GeneticAlgorithm(Of Chr)
        Dim terminated As Boolean = False

        ''' <summary>
        ''' 需要运行的总的迭代次数
        ''' </summary>
        ''' <returns></returns>
        Public Property Iterations As Integer
        Public Property Threshold As Double

        Sub New(ga As GeneticAlgorithm(Of Chr))
            Me.core = ga
        End Sub

        Public Overrides Sub Train(Optional parallel As Boolean = False)
            terminated = False

            For i As Integer = 0 To Iterations
                If terminated Then
                    Exit For
                Else
                    Call core.Evolve()
                End If

                If Not reporter Is Nothing Then
                    Call reporter(i, core.GetFitness(core.Best), core)
                End If
            Next
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
                .chromosome = best.ToString
            }
        End Function
    End Class

    Public Structure outPrint

        Public Property iter%
        Public Property fit#
        Public Property chromosome$

        Public Shared Sub PrintTitle()
            ' just for pretty print
            Console.WriteLine($"{NameOf(outPrint.iter)}{vbTab}{NameOf(outPrint.fit)}{vbTab}{NameOf(outPrint.chromosome)}")
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"{iter}{vbTab}{fit}{vbTab}{chromosome}"
        End Function
    End Structure
End Namespace
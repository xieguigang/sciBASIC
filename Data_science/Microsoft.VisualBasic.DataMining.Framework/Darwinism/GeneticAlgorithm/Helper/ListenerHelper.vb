Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.Darwinism.Models

Namespace Darwinism.GAF.Helper

    Public Module ListenerHelper

        Public Structure outPrint

            Public Property iter%
            Public Property fit#
            Public Property chromosome$

            Public Overrides Function ToString() As String
                Return $"{iter}{vbTab}{fit}{vbTab}{chromosome}"
            End Function
        End Structure

        ''' <summary>
        ''' After each iteration Genetic algorithm notifies listener
        ''' </summary>
        ''' 
        <Extension>
        Public Sub AddDefaultListener(Of T As Chromosome(Of T))(
                                  ByRef ga As GeneticAlgorithm(Of T, Double),
                               Optional print As Action(Of outPrint) = Nothing,
                               Optional threshold# = DefaultThreshold)

            If print Is Nothing Then
                print = Sub(out)
                            Call Console.WriteLine(out.ToString)
                        End Sub
                ' just for pretty print
                Console.WriteLine($"{NameOf(outPrint.iter)}{vbTab}{NameOf(outPrint.fit)}{vbTab}{NameOf(outPrint.chromosome)}")
            End If

            ' Lets add listener, which prints best chromosome after each iteration
            ga.addIterationListener(
                New IterartionListenerAnonymousInnerClassHelper(Of T) With {
                    .print = print,
                    .threshold = threshold
                })
        End Sub

        Const DefaultThreshold# = 0.00001

        Private Structure IterartionListenerAnonymousInnerClassHelper(Of T As Chromosome(Of T))
            Implements IterartionListener(Of T, Double)

            Public threshold As Double
            Public print As Action(Of outPrint)

            Public Sub Update(ga As GeneticAlgorithm(Of T, Double)) Implements IterartionListener(Of T, Double).Update
                Dim best As T = ga.Best
                Dim bestFit As Double = ga.Fitness(best)
                Dim iteration As Integer = ga.Iteration

                ' Listener prints best achieved solution
                Call print(
                    New outPrint With {
                        .iter = iteration,
                        .fit = bestFit,
                        .chromosome = best.ToString
                    })

                ' If fitness is satisfying - we can stop Genetic algorithm
                If bestFit <= threshold Then
                    Call ga.Terminate()
                End If
            End Sub
        End Structure
    End Module
End Namespace
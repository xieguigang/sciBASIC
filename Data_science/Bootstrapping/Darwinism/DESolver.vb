Imports Microsoft.VisualBasic.Data.Bootstrapping.GAF
Imports Microsoft.VisualBasic.DataMining.Darwinism.DifferentialEvolution
Imports Microsoft.VisualBasic.Mathematical.Calculus
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.DataMining.Darwinism
Imports Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper.ListenerHelper

Namespace Darwinism

    ''' <summary>
    ''' Differential Evolution estimates solver.
    ''' </summary>
    Public Module DESolver

        Public Function Fitting(Of T As MonteCarlo.Model)(
                         observation As ODEsOut,
                         Optional F As Double = 1,
                         Optional CR As Double = 0.5,
                         Optional threshold# = 0.1,
                         Optional maxIterations% = 500000,
                         Optional PopulationSize% = 20,
                         Optional ByRef iteratePrints As List(Of outPrint) = Nothing,
                         Optional initOverrides As Dictionary(Of String, Double) = Nothing) As var()

            Dim model As Type = GetType(T)
            Dim vars As String() = MonteCarlo.Model.GetParameters(model).ToArray
            Dim [new] As [New](Of ParameterVector) =
                Function(seed)
                    Dim out As New ParameterVector With {
                        .vars = vars _
                        .ToArray(Function(v) New var(v))
                    }

                    If seed Is Nothing Then
                        Return out
                    Else
                        For Each x In out.vars
                            Dim power# = (
                                If(seed.Next > 0.5, 1, -1) * seed.Next(vars.Length)
                            )
                            x.value = 100 ^ power
                        Next
                    End If

                    Return out
                End Function
            Dim fitness As New GAFFitness(model, observation, initOverrides)
            Dim iterates As New List(Of outPrint)
            Dim best = DifferentialEvolution.Evolution(
                AddressOf fitness.Calculate,
                [new],
                vars.Length,
                F, CR, threshold,
                maxIterations,
                PopulationSize,
                AddressOf iterates.Add)

            iteratePrints = iterates

            Return best.vars
        End Function
    End Module
End Namespace
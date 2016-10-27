Namespace Darwinism.Models

    Public Class FitnessPool(Of Individual, T As IComparable(Of T))

        Protected Friend ReadOnly cache As New Dictionary(Of String, T)
        Protected caclFitness As Func(Of Individual, T)

        Sub New(cacl As Func(Of Individual, T))
            caclFitness = cacl
        End Sub

        Sub New()
        End Sub

        ''' <summary>
        ''' This function tells how well given individual performs at given problem.
        ''' </summary>
        ''' <param name="[in]"></param>
        ''' <returns></returns>
        Public Function Fitness([in] As Individual) As T
            Dim key$ = [in].ToString
            Dim fit As T

            If cache.ContainsKey(key$) Then
                fit = cache(key$)
            Else
                fit = caclFitness([in])
                cache.Add(key$, fit)
            End If

            Return fit
        End Function
    End Class
End Namespace
Imports Microsoft.VisualBasic.DataMining.GAF
Imports Microsoft.VisualBasic.Mathematical.diffEq
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.DataMining.GAF.Helper

Namespace GAF

    ''' <summary>
    ''' Parameters that wait for bootstrapping estimates
    ''' </summary>
    Public Class ParameterVector
        Implements Chromosome(Of ParameterVector), ICloneable

        ReadOnly random As New Random(Now.Millisecond)

        Public Property vars As var()
        Public Property Vector As Double()
            Get
                Return vars _
                    .Select(Function(var) var.value) _
                    .ToArray
            End Get
            Set(value As Double())
                For i As Integer = 0 To vars.Length - 1
                    vars(i).value = value(i)
                Next
            End Set
        End Property

        Public Function Clone() As Object Implements ICloneable.Clone
            Return New ParameterVector With {
                .vars = vars _
                    .ToArray(Function(var) New var(var))
            }
        End Function

        Public Function Crossover(anotherChromosome As ParameterVector) As IList(Of ParameterVector) Implements Chromosome(Of ParameterVector).Crossover
            Dim thisClone As ParameterVector = Clone()
            Dim otherClone As ParameterVector = anotherChromosome.Clone
            Call random.Crossover(thisClone.Vector, otherClone.Vector)
            Return {thisClone, otherClone}.ToList
        End Function

        Public Function Mutate() As ParameterVector Implements Chromosome(Of ParameterVector).Mutate
            Dim m As ParameterVector = Clone()
            Call m.Vector.Mutate(random)
            Return m
        End Function
    End Class
End Namespace
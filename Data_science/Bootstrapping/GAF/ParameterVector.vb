Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo
Imports Microsoft.VisualBasic.DataMining.GAF
Imports Microsoft.VisualBasic.DataMining.GAF.Helper
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.diffEq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace GAF

    ''' <summary>
    ''' Parameters that wait for bootstrapping estimates
    ''' </summary>
    Public Class ParameterVector
        Implements Chromosome(Of ParameterVector), ICloneable

        ReadOnly random As New Random(Now.Millisecond)

        Public Property vars As VariableModel()
        Public Property Vector As Double()
            Get
                Return vars _
                    .Select(Function(var) {var.Min, var.Max}) _
                    .IteratesALL _
                    .ToArray
            End Get
            Set(value As Double())
                For Each var As SeqValue(Of Double()) In value _
                    .Split(parTokens:=2) _
                    .SeqIterator

                    Dim i% = var.i
                    vars(i).Min = var.obj(Scan0)
                    vars(i).Max = var.obj(1)
                Next
            End Set
        End Property

        Public Function Clone() As Object Implements ICloneable.Clone
            Return New ParameterVector With {
                .vars = vars _
                    .ToArray(Function(var) TryCast(var.Clone, VariableModel))
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

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace
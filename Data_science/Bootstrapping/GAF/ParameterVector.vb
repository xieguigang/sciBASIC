Imports System.Web.Script.Serialization
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

        ReadOnly random As New Random

        Public Property vars As var()

        <ScriptIgnore>
        Public Property Vector As Double()
            Get
                ' {var.Min, var.Max}).IteratesALL _
                Return vars _
                    .CopyVector(5) _
                    .IteratesALL _
                    .Select(Function(var) var.value) _
                    .ToArray
            End Get
            Set(value As Double())
                'For Each var As SeqValue(Of Double()) In value _
                '    .Split(parTokens:=2) _
                '    .SeqIterator

                '    Dim i% = var.i
                '    vars(i).Min = var.obj(Scan0)
                '    vars(i).Max = var.obj(1)
                'Next
                Dim mat = value.Split(vars.Length)
                Dim l% = mat(Scan0).Length

                For i As Integer = 0 To l - 1
                    Dim index% = i
                    vars(i).value = mat _
                        .Select(Function(v) v(index)) _
                        .Max + random.Next(vars.Length)
                Next
            End Set
        End Property

        Public Function Clone() As Object Implements ICloneable.Clone
            Return New ParameterVector With {
                .vars = vars _
                    .ToArray(Function(var) TryCast(var.Clone, var))
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
            Call m.Vector.Mutate(random)
            Call m.Vector.Mutate(random)
            Return m
        End Function

        Public Overrides Function ToString() As String
            Return vars _
                .Select(Function(x) x.value) _
                .JoinBy(",")
        End Function
    End Class
End Namespace
Imports System.Web.Script.Serialization
Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo
Imports Microsoft.VisualBasic.DataMining.GAF
Imports Microsoft.VisualBasic.DataMining.GAF.Helper
Imports Microsoft.VisualBasic.Language
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
        Public ReadOnly Property Vector As Double()
            Get
                ' {var.Min, var.Max}).IteratesALL _
                '_
                '               .CopyVector(5) _
                '                   .IteratesALL _
                Return vars _
                    .Select(Function(var) var.value) _
                    .ToArray
            End Get
        End Property

        Private Sub __setValues(value#())
            'Dim mat = value.Split(vars.Length)
            'Dim l% = mat(Scan0).Length

            'For i As Integer = 0 To l - 1
            '    Dim index% = i
            '    vars(i).value = mat _
            '        .Select(Function(v) v(index)) _
            '        .Max + New Random().Next(vars.Length)
            'Next

            For i As Integer = 0 To value.Length - 1
                vars(i).value = value(i)
            Next
        End Sub

        Public Function Clone() As Object Implements ICloneable.Clone
            Dim v As var() = LinqAPI.Exec(Of var) <=
 _
                From var As var
                In vars
                Select New var(var)

            Return New ParameterVector With {
                .vars = v
            }
        End Function

        Public Function Crossover(anotherChromosome As ParameterVector) As IList(Of ParameterVector) Implements Chromosome(Of ParameterVector).Crossover
            Dim thisClone As ParameterVector = DirectCast(Clone(), ParameterVector)
            Dim otherClone As ParameterVector = DirectCast(anotherChromosome.Clone, ParameterVector)
            Dim array1#() = thisClone.Vector
            Dim array2#() = otherClone.Vector

            random.Crossover(array1, array2)

            Dim copy1#() = New Double(array1.Length - 1) {}
            Dim copy2#() = New Double(array2.Length - 1) {}

            Call Array.ConstrainedCopy(array1, Scan0, copy1, Scan0, copy1.Length)
            Call Array.ConstrainedCopy(array2, Scan0, copy2, Scan0, copy2.Length)

            thisClone.__setValues(copy1)
            otherClone.__setValues(copy2)

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
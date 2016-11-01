Imports System.Web.Script.Serialization
Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo
Imports Microsoft.VisualBasic.DataMining.Darwinism
Imports Microsoft.VisualBasic.DataMining.Darwinism.GAF
Imports Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper
Imports Microsoft.VisualBasic.DataMining.Darwinism.Models
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.Calculus
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Darwinism.GAF

    ''' <summary>
    ''' Parameters that wait for bootstrapping estimates
    ''' </summary>
    Public Class ParameterVector
        Implements Chromosome(Of ParameterVector), ICloneable
        Implements IIndividual

        ReadOnly seeds As IRandomSeeds

        Public Sub New(seeds As IRandomSeeds)
            If seeds Is Nothing Then
                seeds = Function() New Random
            End If

            Me.seeds = seeds
        End Sub

        ''' <summary>
        ''' 只需要在这里调整参数就行了，y0初始值不需要
        ''' </summary>
        ''' <returns></returns>
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

        Public Sub Put(i As Int32, value As Double) Implements IIndividual.Put
            vars(i).value = value
        End Sub

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

        ''' <summary>
        ''' 按值复制
        ''' </summary>
        ''' <returns></returns>
        Public Function Clone() As Object Implements ICloneable.Clone
            Dim v As var() = LinqAPI.Exec(Of var) <=
 _
                From var As var
                In vars
                Select New var(var)

            Return New ParameterVector(seeds) With {
                .vars = v
            }
        End Function

        Public Function Crossover(anotherChromosome As IIndividual) As IList(Of IIndividual) Implements Chromosome(Of IIndividual).Crossover
            Return Crossover(anotherChromosome)
        End Function

        ''' <summary>
        ''' 结果是按值复制的
        ''' </summary>
        ''' <param name="anotherChromosome"></param>
        ''' <returns></returns>
        Public Function Crossover(anotherChromosome As ParameterVector) As IList(Of ParameterVector) Implements Chromosome(Of ParameterVector).Crossover
            Dim thisClone As ParameterVector = DirectCast(Clone(), ParameterVector)
            Dim otherClone As ParameterVector = DirectCast(anotherChromosome.Clone, ParameterVector)
            Dim array1#() = thisClone.Vector
            Dim array2#() = otherClone.Vector

            Call seeds() _
                .Crossover(array1, array2)
            thisClone.__setValues(array1)
            otherClone.__setValues(array2)

            Return {thisClone, otherClone}.ToList
        End Function

        ''' <summary>
        ''' 会按值复制
        ''' </summary>
        ''' <returns></returns>
        Public Function Mutate() As ParameterVector Implements Chromosome(Of ParameterVector).Mutate
            Dim m As ParameterVector = Clone()
            Dim random As Random = seeds()

            For i As Integer = 0 To 2
                Dim array#() = m.Vector

                Call array.Mutate(random)
                Call m.__setValues(array)
            Next

            Return m
        End Function

        Public Overrides Function ToString() As String
            Return vars _
                .Select(Function(x) x.Name & ":" & x.value) _
                .JoinBy(";")
        End Function

        Public Function Yield(i As Int32) As Double Implements IIndividual.Yield
            Return vars(i).value
        End Function

        Private Function Chromosome_Mutate() As IIndividual Implements Chromosome(Of IIndividual).Mutate
            Return Mutate()
        End Function
    End Class
End Namespace
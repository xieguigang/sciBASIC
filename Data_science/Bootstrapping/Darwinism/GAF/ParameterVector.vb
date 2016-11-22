#Region "Microsoft.VisualBasic::68303c8411ff480d02288debe582850f, ..\sciBASIC#\Data_science\Bootstrapping\Darwinism\GAF\ParameterVector.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

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
        ''' The function variable parameter that needs to fit, not includes the ``y0``.
        ''' (只需要在这里调整参数就行了，y0初始值不需要)
        ''' </summary>
        ''' <returns></returns>
        Public Property vars As var()

        Public Property MutationLevel As MutateLevels = MutateLevels.Low

        ''' <summary>
        ''' Transform as a vector for the mutation and crossover function.
        ''' </summary>
        ''' <returns></returns>
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
        ''' Clone and crossover and last assign the vector value.(结果是按值复制的)
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
        ''' Clone and mutation a bit and last assign the vector value.(会按值复制)
        ''' </summary>
        ''' <returns></returns>
        Public Function Mutate() As ParameterVector Implements Chromosome(Of ParameterVector).Mutate
            Dim m As ParameterVector = DirectCast(Clone(), ParameterVector)
            Dim random As Random = seeds()
            Dim i As Integer = MutationLevel

            Do While i > 0
                Dim array#() = m.Vector

                Call array.Mutate(random)
                Call m.__setValues(array)

                i -= 1
            Loop

            Return m
        End Function

        ''' <summary>
        ''' 这个函数生成的字符串是和<see cref="Parse"/>解析函数所使用的格式是相对应的
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return vars _
                .Select(Function(x) x.Name & ":" & x.value) _
                .JoinBy(";")
        End Function

        Public Shared Function Parse(s As String) As Dictionary(Of String, Double)
            Dim tks$() = s.Split(";"c)
            Dim vars As Dictionary(Of String, Double) = tks _
                .Select(Function(t) t.GetTagValue(":")) _
                .ToDictionary(Function(v) v.Name,
                              Function(v) v.Value.ParseDouble)
            Return vars
        End Function

        Public Function Yield(i As Int32) As Double Implements IIndividual.Yield
            Return vars(i).value
        End Function

        Private Function Chromosome_Mutate() As IIndividual Implements Chromosome(Of IIndividual).Mutate
            Return Mutate()
        End Function
    End Class
End Namespace

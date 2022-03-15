#Region "Microsoft.VisualBasic::6718d8aa0ba7c8de280c86133df1edf5, sciBASIC#\Data_science\MachineLearning\Bootstrapping\Darwinism\GAF\ODEs\ParameterVector.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 179
    '    Code Lines: 97
    ' Comment Lines: 54
    '   Blank Lines: 28
    '     File Size: 6.55 KB


    '     Class ParameterVector
    ' 
    '         Properties: MutationLevel, radicals, vars, Vector
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Chromosome_Mutate, Clone, (+2 Overloads) Crossover, Mutate, Parse
    '                   ToString, Yield
    ' 
    '         Sub: __setValues, Put
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Web.Script.Serialization
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MachineLearning.Darwinism
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF.Helper
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Calculus

Namespace Darwinism.GAF.ODEs

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
        ''' 突变的激进程度，假若这个值越高的话，会有越高的概率突变当前数位，反之较高的概率突变当前的-1数位
        ''' </summary>
        ''' <returns></returns>
        Public Property radicals As Double = 0.3

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
                Select New var(var)  ' 按值复制需要估算的参数数据对象

            Return New ParameterVector(seeds) With {
                .vars = v,
                .radicals = radicals,
                .MutationLevel = MutationLevel
            }
        End Function

        Public Function Crossover(anotherChromosome As IIndividual) As IEnumerable(Of IIndividual) Implements Chromosome(Of IIndividual).Crossover
            Return Crossover(TryCast(anotherChromosome, ParameterVector))
        End Function

        ''' <summary>
        ''' Clone and crossover and last assign the vector value.(结果是按值复制的)
        ''' </summary>
        ''' <param name="anotherChromosome"></param>
        ''' <returns></returns>
        Public Function Crossover(anotherChromosome As ParameterVector) As IEnumerable(Of ParameterVector) Implements Chromosome(Of ParameterVector).Crossover
            Dim thisClone As ParameterVector = DirectCast(Clone(), ParameterVector)
            Dim otheClone As ParameterVector = DirectCast(anotherChromosome.Clone, ParameterVector)

            Dim this#() = thisClone.Vector
            Dim othr#() = otheClone.Vector

            Call seeds().Crossover(this, othr)
            Call thisClone.__setValues(this)
            Call otheClone.__setValues(othr)

            Return {
                thisClone, otheClone
            }.AsList
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

                Call array.Mutate(random, radicals)
                Call m.__setValues(array)

                i -= 1
            Loop

            Return m
        End Function

        ''' <summary>
        ''' 这个函数生成的字符串是和<see cref="Parse"/>解析函数所使用的格式是相对应的
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' ###### 2016.12.2
        ''' 
        ''' ```
        ''' var=value,....
        ''' ```
        ''' 
        ''' 使用这种形式方便在R之中进行测试
        ''' </remarks>
        Public Overrides Function ToString() As String
            Return vars _
                .Select(Function(x) x.Name & "=" & x.value) _
                .JoinBy(",")
        End Function

        Public Shared Function Parse(s As String) As Dictionary(Of String, Double)
            Dim tks$() = s.Split(","c)
            Dim vars As Dictionary(Of String, Double) = tks _
                .Select(Function(t) t.GetTagValue("=")) _
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

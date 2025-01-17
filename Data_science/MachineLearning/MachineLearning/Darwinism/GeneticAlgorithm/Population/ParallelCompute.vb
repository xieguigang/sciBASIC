#Region "Microsoft.VisualBasic::fd24a441b0827ea5d587454a3929f120, Data_science\MachineLearning\MachineLearning\Darwinism\GeneticAlgorithm\Population\ParallelCompute.vb"

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

    '   Total Lines: 97
    '    Code Lines: 63 (64.95%)
    ' Comment Lines: 16 (16.49%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 18 (18.56%)
    '     File Size: 4.19 KB


    '     Class ParallelComputeFitness
    ' 
    ' 
    ' 
    '     Class ParallelPopulationCompute
    ' 
    '         Function: ComputeFitness
    '         Class ParallelTask
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Sub: Solve
    ' 
    ' 
    ' 
    '     Class ParallelDataSetCompute
    ' 
    '         Properties: verbose
    ' 
    '         Function: ComputeFitness, ComputeFitnessVerboseProgress
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.Tqdm
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models
Imports Microsoft.VisualBasic.Parallel

Namespace Darwinism.GAF.Population

    ''' <summary>
    ''' implements this interface for create custom parallel 
    ''' compute api function for run the genetic algorithm
    ''' </summary>
    ''' <typeparam name="chr"></typeparam>
    ''' <remarks>
    ''' 遗传算法的主要限速步骤是在fitness的计算之上
    ''' </remarks>
    Public MustInherit Class ParallelComputeFitness(Of chr As {Class, Chromosome(Of chr)})

        Public MustOverride Function ComputeFitness(comparator As FitnessPool(Of chr), source As PopulationCollection(Of chr)) As IEnumerable(Of NamedValue(Of Double))

    End Class

    Public Class ParallelPopulationCompute(Of chr As {Class, Chromosome(Of chr)}) : Inherits ParallelComputeFitness(Of chr)

        Public Overrides Function ComputeFitness(comparator As FitnessPool(Of chr), source As PopulationCollection(Of chr)) As IEnumerable(Of NamedValue(Of Double))
            Return DirectCast(New ParallelTask(source.GetCollection().ToArray, comparator).Run, ParallelTask).fitness
        End Function

        Private Class ParallelTask : Inherits VectorTask

            Public fitness As NamedValue(Of Double)()
            Public chrs As chr()
            Public env As FitnessPool(Of chr)

            Public Sub New(pop As chr(), comparator As FitnessPool(Of chr))
                MyBase.New(nsize:=pop.Length)

                fitness = Allocate(Of NamedValue(Of Double))(all:=True)
                chrs = pop
                env = comparator
            End Sub

            Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
                Dim c As chr

                For i As Integer = start To ends
                    c = chrs(i)
                    fitness(i) = New NamedValue(Of Double)(
                        name:=c.Identity,
                        value:=env.Fitness(c, parallel:=False)
                    )
                Next
            End Sub
        End Class
    End Class

    Public Class ParallelDataSetCompute(Of chr As {Class, Chromosome(Of chr)}) : Inherits ParallelComputeFitness(Of chr)

        Public Property verbose As Boolean = True

        ''' <summary>
        ''' the parallel is running in fitness calculation function, so we run sequential at here.
        ''' </summary>
        ''' <param name="comparator">
        ''' parallel computation between the dataset in this fitness calculation
        ''' </param>
        ''' <param name="source"></param>
        ''' <returns></returns>
        Public Overrides Function ComputeFitness(comparator As FitnessPool(Of chr), source As PopulationCollection(Of chr)) As IEnumerable(Of NamedValue(Of Double))
            If verbose Then
                Return ComputeFitnessVerboseProgress(comparator, source)
            Else
                Return From c As chr
                       In source.GetCollection()
                       Let fit As Double = comparator.Fitness(c, parallel:=True)
                       Let key As String = c.Identity
                       Select New NamedValue(Of Double) With {
                           .Name = key,
                           .Value = fit
                       }
            End If
        End Function

        Private Iterator Function ComputeFitnessVerboseProgress(comparator As FitnessPool(Of chr), source As PopulationCollection(Of chr)) As IEnumerable(Of NamedValue(Of Double))
            Dim pool As chr() = source.GetCollection.ToArray

            For Each c As chr In TqdmWrapper.Wrap(pool, wrap_console:=App.EnableTqdm)
                Dim fit As Double = comparator.Fitness(c, parallel:=True)
                Dim key As String = c.Identity

                Yield New NamedValue(Of Double) With {
                    .Name = key,
                    .Value = fit
                }
            Next
        End Function
    End Class
End Namespace

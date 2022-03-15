#Region "Microsoft.VisualBasic::1d97b6376c7e278240bcc2f54c8ded16, sciBASIC#\Data_science\MachineLearning\Bootstrapping\Bootstrapping_CLI\Class1.vb"

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

    '   Total Lines: 98
    '    Code Lines: 75
    ' Comment Lines: 4
    '   Blank Lines: 19
    '     File Size: 3.47 KB


    ' Class Class1
    ' 
    '     Sub: Main
    '     Class Individual
    ' 
    '         Function: Chromosome_Mutate, Clone, CreateOne, (+2 Overloads) Crossover, Mutate
    '                   ToString, Yield
    ' 
    '         Sub: Put
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.Darwinism
Imports Microsoft.VisualBasic.DataMining.Darwinism.Models

Public Class Class1
    Shared Sub Main()
        Dim target As New Individual With {.data1 = 1, .data2 = 2, .data3 = 3}
        Dim tt = {target.data1, target.data2, target.data3}
        Dim result = DifferentialEvolution.Evolution(Of Individual)(
            Function(x)
                Return RMS(tt, {x.data1, x.data2, x.data3})
            End Function, AddressOf Individual.CreateOne, 3,)

        RMS(tt, {result.data1, result.data2, result.data3})

        Console.WriteLine(result.ToString)
        Console.ReadKey()
    End Sub

    ' definition of one individual in population
    Public Class Individual
        Implements Chromosome(Of Individual)
        Implements IIndividual

        ' normally DifferentialEvolution uses floating point variables
        Public data1, data2 As Double
        ' but using integers Is possible too  
        Public data3%

        Public Overrides Function ToString() As String
            Return String.Join(",", data1, data2, data3)
        End Function

        Public Function Clone() As Object Implements ICloneable.Clone
            Return New Individual With {
                .data1 = data1,
                .data2 = data2,
                .data3 = data3
            }
        End Function

        Private Function Crossover(anotherChromosome As Individual) As IList(Of Individual) Implements Chromosome(Of Individual).Crossover

        End Function

        Private Function Mutate() As Individual Implements Chromosome(Of Individual).Mutate

        End Function

        Public Shared Function CreateOne(random As Random) As Individual
            Dim Individual As New Individual()

            If random Is Nothing Then
                Return Individual
            End If

            Individual.data1 = random.NextDouble * 100
            Individual.data2 = random.NextDouble * 100
            ' integers cant take floating point values And they need to be either rounded
            Individual.data3 = Math.Floor(random.NextDouble * 100)

            Return Individual
        End Function

        Public Function Yield(i As Int32) As Double Implements IIndividual.Yield
            Select Case i
                Case 0
                    Return data1
                Case 1
                    Return data2
                Case 2
                    Return data3
                Case Else
                    Throw New NotImplementedException
            End Select
        End Function

        Public Sub Put(i As Int32, value As Double) Implements IIndividual.Put
            Select Case i
                Case 0
                    data1 = value
                Case 1
                    data2 = value
                Case 2
                    data3 = value
                Case Else
                    Throw New NotImplementedException
            End Select
        End Sub

        Public Function Crossover(anotherChromosome As IIndividual) As IList(Of IIndividual) Implements Chromosome(Of IIndividual).Crossover
            Throw New NotImplementedException()
        End Function

        Private Function Chromosome_Mutate() As IIndividual Implements Chromosome(Of IIndividual).Mutate
            Throw New NotImplementedException()
        End Function
    End Class
End Class

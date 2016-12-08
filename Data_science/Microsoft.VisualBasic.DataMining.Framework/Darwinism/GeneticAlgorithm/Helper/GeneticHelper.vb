#Region "Microsoft.VisualBasic::f6f25a8d5536dbeb8807000a28e279a4, ..\sciBASIC#\Data_science\Microsoft.VisualBasic.DataMining.Framework\Darwinism\GeneticAlgorithm\Helper\GeneticHelper.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.Darwinism.Models

Namespace Darwinism.GAF.Helper

    Public Module GeneticHelper

        ''' <summary>
        ''' Returns clone of current chromosome, which is mutated a bit
        ''' </summary>
        ''' <param name="v#"></param>
        ''' <param name="random"></param>
        <Extension> Public Sub Mutate(ByRef v#(), random As Random)
            ' just select random element of vector
            ' and increase or decrease it on small value
            Dim index As Integer = random.Next(v.Length)
            Dim mutationValue# =
                random.Next(v.Length) - (random.NextDouble * v.Length)

            v(index) += mutationValue
        End Sub

        ''' <summary>
        ''' Returns clone of current chromosome, which is mutated a bit
        ''' </summary>
        ''' <param name="v%"></param>
        ''' <param name="random"></param>
        <Extension> Public Sub Mutate(ByRef v%(), random As Random)
            ' just select random element of vector
            ' and increase or decrease it on small value
            Dim index As Integer = random.Next(v.Length)
            Dim mutationValue# =
                random.Next(v.Length) -
                random.Next(v.Length)

            v(index) += mutationValue
        End Sub

        ''' <summary>
        ''' Returns list of siblings 
        ''' Siblings are actually new chromosomes, 
        ''' created using any of crossover strategy
        ''' </summary>
        ''' <param name="random"></param>
        ''' <param name="v1#"></param>
        ''' <param name="v2#"></param>
        <Extension>
        Public Sub Crossover(Of T)(random As Random, ByRef v1 As T(), ByRef v2 As T())
            Dim index As Integer = random.Next(v1.Length - 1)
            Dim tmp As T

            ' one point crossover
            For i As Integer = index To v1.Length - 1
                tmp = v1(i)
                v1(i) = v2(i)
                v2(i) = tmp
            Next
        End Sub

        ''' <summary>
        ''' The simplest strategy for creating initial population <br/>
        ''' in real life it could be more complex
        ''' </summary>
        <Extension>
        Public Function InitialPopulation(Of T As Chromosome(Of T))(base As T, populationSize As Integer, Optional parallel As ParallelComputing(Of T) = Nothing) As Population(Of T)
            Dim population As New Population(Of T)(parallel) With {
                .Parallel = True
            }

            For i As Integer = 0 To populationSize - 1
                ' each member of initial population
                ' is mutated clone of base chromosome
                Dim chr As T = base.Mutate()
                Call population.Add(chr)
            Next
            Return population
        End Function
    End Module
End Namespace

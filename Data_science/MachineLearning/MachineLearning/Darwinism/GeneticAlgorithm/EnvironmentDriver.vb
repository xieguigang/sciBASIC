#Region "Microsoft.VisualBasic::43ec4d5274d161f25d02c4befaded7c2, Data_science\MachineLearning\MachineLearning\Darwinism\GeneticAlgorithm\EnvironmentDriver.vb"

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

    '     Class EnvironmentDriver
    ' 
    '         Properties: Iterations, Threshold
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CreateReport
    ' 
    '         Sub: Terminate, Train
    ' 
    '     Structure outPrint
    ' 
    '         Properties: chromosome, fit, iter
    ' 
    '         Function: ToString
    ' 
    '         Sub: PrintTitle
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models

Namespace Darwinism.GAF

    Public Class EnvironmentDriver(Of Chr As Chromosome(Of Chr)) : Inherits IterationReporter(Of GeneticAlgorithm(Of Chr))

        Dim core As GeneticAlgorithm(Of Chr)
        Dim terminated As Boolean = False

        ''' <summary>
        ''' 需要运行的总的迭代次数
        ''' </summary>
        ''' <returns></returns>
        Public Property Iterations As Integer
        Public Property Threshold As Double

        Sub New(ga As GeneticAlgorithm(Of Chr))
            Me.core = ga
        End Sub

        Public Overrides Sub Train(Optional parallel As Boolean = False)
            terminated = False

            For i As Integer = 0 To Iterations
                If terminated Then
                    Exit For
                Else
                    Call core.Evolve()
                End If

                With core.GetFitness(core.Best)
                    If Not reporter Is Nothing Then
                        Call reporter(i, .ByRef, core)
                    End If

                    ' NaN的结果值与阈值相比较也是小于零的
                    ' 在这里跳过NaN值的测试
                    If Not .IsNaNImaginary AndAlso .CompareTo(Threshold) < 0 Then
                        Exit For
                    End If
                End With
            Next
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <example>
        ''' ' If fitness is satisfying - we can stop Genetic algorithm
        ''' 
        ''' If bestFit &lt;= Threshold Then
        '''     Call ga.Terminate()
        ''' End If
        ''' </example>
        Public Sub Terminate()
            Me.terminated = True
        End Sub

        Public Shared Function CreateReport(iteration%, fitness#, ga As GeneticAlgorithm(Of Chr)) As outPrint
            Dim best As Chr = ga.Best
            Dim bestFit As Double = ga.GetFitness(best)

            ' Listener prints best achieved solution
            Return New outPrint With {
                .iter = iteration,
                .fit = bestFit,
                .chromosome = best.ToString
            }
        End Function
    End Class

    Public Structure outPrint

        Public Property iter%
        Public Property fit#
        Public Property chromosome$

        Public Shared Sub PrintTitle()
            ' just for pretty print
            Console.WriteLine($"{NameOf(outPrint.iter)}{vbTab}{NameOf(outPrint.fit)}{vbTab}{NameOf(outPrint.chromosome)}")
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"{iter}{vbTab}{fit}{vbTab}{chromosome}"
        End Function
    End Structure
End Namespace

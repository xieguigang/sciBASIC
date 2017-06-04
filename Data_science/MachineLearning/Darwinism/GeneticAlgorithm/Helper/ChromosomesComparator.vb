#Region "Microsoft.VisualBasic::bb558d9ba07bc7944336b90b58945f76, ..\sciBASIC#\Data_science\MachineLearning\Darwinism\GeneticAlgorithm\Helper\ChromosomesComparator.vb"

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

Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models

Namespace Darwinism.GAF.Helper

    ''' <summary>
    ''' 缓存的Key是染色体的ToString的计算值
    ''' </summary>
    ''' <typeparam name="C"></typeparam>
    Friend Class ChromosomesComparator(Of C As Chromosome(Of C)) : Inherits FitnessPool(Of C, Double)
        Implements IComparer(Of C)

        Public Sub New(GA As GeneticAlgorithm(Of C))
            caclFitness = AddressOf GA.Fitness.Calculate
        End Sub

        Public Function compare(chr1 As C, chr2 As C) As Integer Implements IComparer(Of C).Compare
            Dim fit1 As Double = Fitness(chr1)
            Dim fit2 As Double = Fitness(chr2)
            Dim ret As Integer = fit1.CompareTo(fit2)
            Return ret
        End Function

        Public Sub clearCache()
            Call cache.Clear()
        End Sub
    End Class
End Namespace

#Region "Microsoft.VisualBasic::d8dd6a59c3b2a1e7dc1fa15d9ec356a7, sciBASIC#\Data_science\MachineLearning\Bootstrapping\Darwinism\GAF\Abstract\ParameterVector.vb"

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

    '   Total Lines: 45
    '    Code Lines: 33
    ' Comment Lines: 0
    '   Blank Lines: 12
    '     File Size: 1.87 KB


    '     Class ParameterVector
    ' 
    '         Function: ChromosomeMutate, Clone, (+2 Overloads) Crossover, Mutate, Yield
    ' 
    '         Sub: Put
    ' 
    '     Class FFFF
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MachineLearning.Darwinism
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models

Namespace Darwinism.GAF.Abstract

    Public Class ParameterVector(Of T As ParameterVector(Of T))
        Implements Chromosome(Of ParameterVector(Of T)), ICloneable
        Implements IIndividual

        Public Sub Put(i As Integer, value As Double) Implements IIndividual.Put
            Throw New NotImplementedException()
        End Sub

        Public Function Yield(i As Integer) As Double Implements IIndividual.Yield
            Throw New NotImplementedException()
        End Function

        Private Function Crossover(anotherChromosome As IIndividual) As IEnumerable(Of IIndividual) Implements Chromosome(Of IIndividual).Crossover
            Return Crossover(TryCast(anotherChromosome, ParameterVector(Of T)))
        End Function

        Public Function Crossover(anotherChromosome As ParameterVector(Of T)) As IEnumerable(Of ParameterVector(Of T)) Implements Chromosome(Of ParameterVector(Of T)).Crossover
            Throw New NotImplementedException()
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function Mutate() As IIndividual Implements Chromosome(Of IIndividual).Mutate
            Return ChromosomeMutate()
        End Function

        Public Overridable Function Clone() As Object Implements ICloneable.Clone
            Throw New NotImplementedException()
        End Function

        Public Overridable Function ChromosomeMutate() As ParameterVector(Of T) Implements Chromosome(Of ParameterVector(Of T)).Mutate
            Throw New NotImplementedException()
        End Function
    End Class

    Public Class FFFF : Inherits ParameterVector(Of FFFF)


    End Class
End Namespace

#Region "Microsoft.VisualBasic::388a92a5f99e384aecc63647b781bc41, Data_science\MachineLearning\MachineLearning\Darwinism\Models\FitnessPool.vb"

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

    '   Total Lines: 18
    '    Code Lines: 8 (44.44%)
    ' Comment Lines: 7 (38.89%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (16.67%)
    '     File Size: 672 B


    '     Class FitnessPool
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF

Namespace Darwinism.Models

    ''' <summary>
    ''' Compute fitness and cache the result data in this pool.
    ''' </summary>
    ''' <typeparam name="Individual"></typeparam>
    ''' <remarks>
    ''' this fitness calculation pool is works for the genetic algorithm module
    ''' </remarks>
    Public Class FitnessPool(Of Individual As {Class, Chromosome(Of Individual)}) : Inherits GeneralFitnessPool(Of Individual)

        Sub New(cacl As Fitness(Of Individual), capacity%)
            Call MyBase.New(cacl, capacity, Function(a) a.Identity)
        End Sub
    End Class
End Namespace

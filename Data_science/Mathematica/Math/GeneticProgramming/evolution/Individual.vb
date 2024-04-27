﻿#Region "Microsoft.VisualBasic::c84acc45e5a3ab409eec4879cc4f2d28, G:/GCModeller/src/runtime/sciBASIC#/Data_science/Mathematica/Math/GeneticProgramming//evolution/Individual.vb"

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

    '   Total Lines: 17
    '    Code Lines: 10
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 438 B


    '     Interface Individual
    ' 
    '         Properties: Expression, Fitness
    ' 
    '         Function: computeFitness
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Scripting
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.model

Namespace evolution

    Public Interface Individual
        Inherits IComparable(Of Individual)

        ReadOnly Property Expression As Expression

        ReadOnly Property Fitness As Double

        Function computeFitness(dataTuples As IList(Of DataPoint)) As Double

    End Interface

End Namespace


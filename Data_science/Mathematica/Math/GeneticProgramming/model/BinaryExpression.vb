﻿#Region "Microsoft.VisualBasic::85eb3e0b886e0cea6453d07d25b87dcf, G:/GCModeller/src/runtime/sciBASIC#/Data_science/Mathematica/Math/GeneticProgramming//model/BinaryExpression.vb"

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

    '   Total Lines: 15
    '    Code Lines: 9
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 366 B


    '     Interface BinaryExpression
    ' 
    '         Properties: LeftChild, RightChild
    ' 
    '         Function: swapLeftChild, swapRightChild
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace model

    Public Interface BinaryExpression
        Inherits Expression

        Property LeftChild As Expression
        Property RightChild As Expression

        Function swapLeftChild(newLeftChild As Expression) As Expression

        Function swapRightChild(newRightChild As Expression) As Expression

    End Interface

End Namespace


#Region "Microsoft.VisualBasic::197329628c618eca54dbe33eb5c78e3b, Data_science\Mathematica\Math\GeneticProgramming\evolution\measure\Objective.vb"

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

    '   Total Lines: 10
    '    Code Lines: 6 (60.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (40.00%)
    '     File Size: 249 B


    '     Interface Objective
    ' 
    '         Function: getError, getOverallError
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace evolution.measure
    Public Interface Objective

        Function getError(expected As Double, real As Double) As Double

        Function getOverallError(ParamArray errors As Double()) As Double

    End Interface

End Namespace

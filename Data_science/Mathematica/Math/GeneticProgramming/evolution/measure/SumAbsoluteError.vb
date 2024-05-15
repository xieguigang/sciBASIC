#Region "Microsoft.VisualBasic::6dbf6f96b49cfcbbe1a5faf2ef8c7615, Data_science\Mathematica\Math\GeneticProgramming\evolution\measure\SumAbsoluteError.vb"

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

    '   Total Lines: 21
    '    Code Lines: 16
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 642 B


    '     Class SumAbsoluteError
    ' 
    '         Function: getError, getOverallError
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace evolution.measure
    Public Class SumAbsoluteError
        Implements Objective

        Public Overridable Function getError(expected As Double, real As Double) As Double Implements Objective.getError
            Return std.Abs(expected - real)
        End Function

        Public Overridable Function getOverallError(ParamArray errors As Double()) As Double Implements Objective.getOverallError
            Dim sum = 0.0
            For Each [error] In errors
                sum += [error]
            Next
            Return sum
        End Function

    End Class

End Namespace

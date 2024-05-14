#Region "Microsoft.VisualBasic::30d5e29190684ad1b0baeb29bbc2561b, Data_science\Mathematica\Math\GeneticProgramming\evolution\measure\MeanAbsoluteError.vb"

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

    '   Total Lines: 11
    '    Code Lines: 8
    ' Comment Lines: 0
    '   Blank Lines: 3
    '     File Size: 313 B


    '     Class MeanAbsoluteError
    ' 
    '         Function: getOverallError
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace evolution.measure
    Public Class MeanAbsoluteError
        Inherits SumSquareError

        Public Overrides Function getOverallError(ParamArray errors As Double()) As Double
            Return MyBase.getOverallError(errors) / errors.Length
        End Function

    End Class

End Namespace

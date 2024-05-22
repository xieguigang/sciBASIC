#Region "Microsoft.VisualBasic::52913cff15d1ce17ad980303329d6e88, Data_science\MachineLearning\RestrictedBoltzmannMachine\math\functions\Sigmoid.vb"

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
    '    Code Lines: 11 (61.11%)
    ' Comment Lines: 3 (16.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (22.22%)
    '     File Size: 468 B


    '     Class Sigmoid
    ' 
    '         Function: apply, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace math.functions
    ''' <summary>
    ''' Created by kenny on 5/24/14.
    ''' </summary>
    Public Class Sigmoid
        Inherits DoubleFunction

        Public Overrides Function apply(x As Double) As Double
            Return 1.0 / (1.0 + System.Math.Exp(-x))
        End Function

        Public Overrides Function ToString() As String
            Return "sigmoid(x) = 1 / (1 + e^(-x))"
        End Function

    End Class

End Namespace

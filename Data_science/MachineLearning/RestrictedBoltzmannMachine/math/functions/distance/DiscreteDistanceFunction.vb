﻿#Region "Microsoft.VisualBasic::be31637638f0aa5cf521fd39e3489935, Data_science\MachineLearning\RestrictedBoltzmannMachine\math\functions\distance\DiscreteDistanceFunction.vb"

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

    '   Total Lines: 26
    '    Code Lines: 17 (65.38%)
    ' Comment Lines: 3 (11.54%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (23.08%)
    '     File Size: 778 B


    '     Class DiscreteDistanceFunction
    ' 
    '         Function: distance, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace math.functions.distance

    ''' <summary>
    ''' Created by kenny on 2/16/14.
    ''' </summary>
    Public Class DiscreteDistanceFunction
        Implements DistanceFunction

        Private Const DELTA As Double = 0.05

        Public Function distance(item1 As DenseMatrix, item2 As DenseMatrix) As Double Implements DistanceFunction.distance
            For i = 0 To item1.columns() - 1
                If System.Math.Abs(item1.get(0, i) - item2.get(0, i)) > DELTA Then
                    Return 1.0
                End If
            Next
            Return 0.0
        End Function

        Public Overrides Function ToString() As String
            Return "DiscreteDistanceFunction"
        End Function

    End Class

End Namespace

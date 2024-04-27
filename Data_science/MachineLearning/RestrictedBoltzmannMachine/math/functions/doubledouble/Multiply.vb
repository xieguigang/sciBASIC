﻿#Region "Microsoft.VisualBasic::edebf59f112ecee4892bd8ab66a2ff44, G:/GCModeller/src/runtime/sciBASIC#/Data_science/MachineLearning/RestrictedBoltzmannMachine//math/functions/doubledouble/Multiply.vb"

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

    '   Total Lines: 13
    '    Code Lines: 8
    ' Comment Lines: 3
    '   Blank Lines: 2
    '     File Size: 343 B


    '     Class Multiply
    ' 
    '         Function: apply
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace math.functions.doubledouble
    ''' <summary>
    ''' Created by kenny on 5/24/14.
    ''' </summary>
    Public Class Multiply
        Inherits DoubleDoubleFunction

        Public Overrides Function apply(v As Double, v2 As Double) As Double
            Return v * v2
        End Function
    End Class

End Namespace


﻿#Region "Microsoft.VisualBasic::03b355464a38dbbfedd60e4294de5a63, G:/GCModeller/src/runtime/sciBASIC#/Data_science/MachineLearning/RestrictedBoltzmannMachine//math/functions/RangedSigmoid.vb"

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

    '   Total Lines: 27
    '    Code Lines: 17
    ' Comment Lines: 3
    '   Blank Lines: 7
    '     File Size: 717 B


    '     Class RangedSigmoid
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: apply, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region


Namespace math.functions

    ''' <summary>
    ''' Created by kenny on 5/24/14.
    ''' </summary>
    Public Class RangedSigmoid
        Inherits DoubleFunction
        Private ReadOnly min As Double

        Private ReadOnly max As Double

        Public Sub New(min As Double, max As Double)
            Me.min = min
            Me.max = max
        End Sub

        Public Overrides Function apply(x As Double) As Double
            Return min + (max - min) / (1.0 + System.Math.Exp(-x))
        End Function

        Public Overrides Function ToString() As String
            Return "sigmoid(x) = min + ((max - min) / (1 + e^(-x)))"
        End Function
    End Class

End Namespace


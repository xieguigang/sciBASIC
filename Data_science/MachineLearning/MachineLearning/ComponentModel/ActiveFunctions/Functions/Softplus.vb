﻿#Region "Microsoft.VisualBasic::78c3917113ef656296ac73d2e93fe5a2, Data_science\MachineLearning\MachineLearning\ComponentModel\ActiveFunctions\Functions\Softplus.vb"

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

    '   Total Lines: 28
    '    Code Lines: 22 (78.57%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (21.43%)
    '     File Size: 840 B


    '     Class Softplus
    ' 
    '         Properties: Store
    ' 
    '         Function: [Function], Derivative, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports stdNum = System.Math

Namespace ComponentModel.Activations

    Public Class Softplus : Inherits IActivationFunction

        Public Overrides ReadOnly Property Store As ActiveFunction
            Get
                Return New ActiveFunction With {
                    .Arguments = {},
                    .name = NameOf(Softplus)
                }
            End Get
        End Property

        Public Overrides Function [Function](x As Double) As Double
            Return stdNum.Log(1 + stdNum.E ^ x)
        End Function

        Public Overrides Function ToString() As String
            Return Store.ToString
        End Function

        Protected Overrides Function Derivative(x As Double) As Double
            Return 1 / (1 + stdNum.E ^ (-x))
        End Function
    End Class
End Namespace

#Region "Microsoft.VisualBasic::8eaa030ce9f9ab63b93aaf98effe07a5, Data_science\MachineLearning\MachineLearning\ComponentModel\ActiveFunctions\Functions\Sinc.vb"

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

    '   Total Lines: 36
    '    Code Lines: 30
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 1.03 KB


    '     Class Sinc
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

    Public Class Sinc : Inherits IActivationFunction

        Public Overrides ReadOnly Property Store As ActiveFunction
            Get
                Return New ActiveFunction With {
                    .Arguments = {},
                    .name = NameOf(Sinc)
                }
            End Get
        End Property

        Public Overrides Function [Function](x As Double) As Double
            If x = 0R Then
                Return 1
            Else
                Return stdNum.Sin(x) / x
            End If
        End Function

        Public Overrides Function ToString() As String
            Return Store.ToString
        End Function

        Protected Overrides Function Derivative(x As Double) As Double
            If x = 0R Then
                Return 0
            Else
                Return stdNum.Cos(x) / x - stdNum.Sin(x) / (x ^ 2)
            End If
        End Function
    End Class
End Namespace

#Region "Microsoft.VisualBasic::6155a879353a888b4de56db4f6658549, ..\sciBASIC#\Data_science\Microsoft.VisualBasic.DataMining.Framework\NeuralNetwork\ActiveFunctions\Sigmoid.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Namespace NeuralNetwork.IFuncs

    ''' <summary>
    ''' https://github.com/trentsartain/Neural-Network/blob/master/NeuralNetwork/NeuralNetwork/Network/Sigmoid.cs
    ''' </summary>
    Public NotInheritable Class Sigmoid
        Implements IActivationFunction
        Implements ICloneable

        Public Function Derivative(x As Double) As Double Implements IActivationFunction.Derivative
            Return x * (1 - x)
        End Function

        Public Function Clone() As Object Implements ICloneable.Clone
            Return Me
        End Function

        Public Function [Function](x As Double) As Double Implements IActivationFunction.Function
            Return If(x < -45.0, 0.0, If(x > 45.0, 1.0, 1.0 / (1.0 + Math.Exp(-x))))
        End Function

        Public Function Derivative2(y As Double) As Double Implements IActivationFunction.Derivative2
            Return Derivative(y)
        End Function
    End Class
End Namespace

#Region "Microsoft.VisualBasic::56243c01bc66bfe7b1d456ab53f07120, Data_science\MachineLearning\MachineLearning\NeuralNetwork\ActiveFunctions\IActivationFunction.vb"

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

    '     Class IActivationFunction
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' AForge Neural Net Library
' AForge.NET framework
'
' Copyright © Andrew Kirillov, 2005-2008
' andrew.kirillov@gmail.com
'

Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure

Namespace NeuralNetwork.Activations

    ''' <summary>
    ''' Activation function interface.
    ''' </summary>
    ''' 
    ''' <remarks>All activation functions, which are supposed to be used with
    ''' neurons, which calculate their output as a function of weighted sum of
    ''' their inputs, should implement this interfaces.
    ''' </remarks>
    ''' 
    Public MustInherit Class IActivationFunction

        Public MustOverride ReadOnly Property Store As ActiveFunction

        ''' <summary>
        ''' Calculates function value.
        ''' </summary>
        '''
        ''' <param name="x">Function input value.</param>
        ''' 
        ''' <returns>Function output value, <i>f(x)</i>.</returns>
        '''
        ''' <remarks>The method calculates function value at point <paramref name="x"/>.</remarks>
        '''
        Public MustOverride Function [Function](x As Double) As Double

        ''' <summary>
        ''' Calculates function derivative.
        ''' </summary>
        ''' 
        ''' <param name="x">Function input value.</param>
        ''' 
        ''' <returns>Function derivative, <i>f'(x)</i>.</returns>
        ''' 
        ''' <remarks>The method calculates function derivative at point <paramref name="x"/>.</remarks>
        '''
        Public MustOverride Function Derivative(x As Double) As Double

        ''' <summary>
        ''' Calculates function derivative.
        ''' </summary>
        ''' 
        ''' <param name="y">Function output value - the value, which was obtained
        ''' with the help of "Function" method.</param>
        ''' 
        ''' <returns>Function derivative, <i>f'(x)</i>.</returns>
        ''' 
        ''' <remarks><para>The method calculates the same derivative value as the
        ''' <see cref="Derivative"/> method, but it takes not the input <b>x</b> value
        ''' itself, but the function value, which was calculated previously with
        ''' the help of "Function" method.</para>
        ''' 
        ''' <para><note>Some applications require as function value, as derivative value,
        ''' so they can save the amount of calculations using this method to calculate derivative.</note></para>
        ''' </remarks>
        ''' 
        Public MustOverride Function Derivative2(y As Double) As Double

        ''' <summary>
        ''' 必须要重写这个函数来将函数对象序列化为表达式字符串文本
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride Overrides Function ToString() As String

    End Class
End Namespace

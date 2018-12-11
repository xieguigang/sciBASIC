#Region "Microsoft.VisualBasic::86056c6605f1a230ad8f54621465e157, Data_science\DataMining\Microsoft.VisualBasic.DataMining.Framework\NeuralNetwork\ActiveFunctions\ThresholdFunction.vb"

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

'     Class ThresholdFunction
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: [Function], Clone, Derivative, Derivative2
' 
' 
' /********************************************************************************/

#End Region

' AForge Neural Net Library
' AForge.NET framework
' http://www.aforgenet.com/framework/
'
' Copyright © AForge.NET, 2007-2012
' contacts@aforgenet.com
'

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure

Namespace NeuralNetwork.Activations

    ''' <summary>
    ''' Threshold activation function.
    ''' </summary>
    '''
    ''' <remarks><para>The class represents threshold activation function with
    ''' the next expression:
    ''' <code lang="none">
    ''' f(x) = 1, if x >= 0, otherwise 0
    ''' </code>
    ''' </para>
    ''' 
    ''' <para>Output range of the function: <b>[0, 1]</b>.</para>
    ''' 
    ''' <para>Functions graph:</para>
    ''' <img src="img/neuro/threshold.bmp" width="242" height="172" />
    ''' </remarks>
    '''
    <Serializable>
    Public Class ThresholdFunction : Implements IActivationFunction

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ThresholdFunction"/> class.
        ''' </summary>
        Public Sub New()
        End Sub

        Public ReadOnly Property Store As ActiveFunction Implements IActivationFunction.Store
            Get
                Return New ActiveFunction With {
                    .Arguments = {},
                    .Name = NameOf(ThresholdFunction)
                }
            End Get
        End Property

        ''' <summary>
        ''' Calculates function value.
        ''' </summary>
        '''
        ''' <param name="x">Function input value.</param>
        ''' 
        ''' <returns>Function output value, <i>f(x)</i>.</returns>
        '''
        ''' <remarks>The method calculates function value at point <paramref name="x"/>.</remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function [Function](x As Double) As Double Implements IActivationFunction.[Function]
            Return If((x >= 0), 1, 0)
        End Function

        ''' <summary>
        ''' Calculates function derivative (not supported).
        ''' </summary>
        ''' 
        ''' <param name="x">Input value.</param>
        ''' 
        ''' <returns>Always returns 0.</returns>
        ''' 
        ''' <remarks><para><note>The method is not supported, because it is not possible to
        ''' calculate derivative of the function.</note></para></remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Derivative(x As Double) As Double Implements IActivationFunction.Derivative
            Return 0
        End Function

        ''' <summary>
        ''' Calculates function derivative (not supported).
        ''' </summary>
        ''' 
        ''' <param name="y">Input value.</param>
        ''' 
        ''' <returns>Always returns 0.</returns>
        ''' 
        ''' <remarks><para><note>The method is not supported, because it is not possible to
        ''' calculate derivative of the function.</note></para></remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Derivative2(y As Double) As Double Implements IActivationFunction.Derivative2
            Return 0
        End Function
    End Class
End Namespace

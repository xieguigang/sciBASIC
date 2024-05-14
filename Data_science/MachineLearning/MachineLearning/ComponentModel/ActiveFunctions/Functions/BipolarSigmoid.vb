#Region "Microsoft.VisualBasic::ac4159cf97bc059eed1ca0144d767aaf, Data_science\MachineLearning\MachineLearning\ComponentModel\ActiveFunctions\Functions\BipolarSigmoid.vb"

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

    '   Total Lines: 114
    '    Code Lines: 35
    ' Comment Lines: 69
    '   Blank Lines: 10
    '     File Size: 3.98 KB


    '     Class BipolarSigmoid
    ' 
    '         Properties: Alpha, Store
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: [Function], Derivative, ToString
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
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports stdNum = System.Math

Namespace ComponentModel.Activations

    ''' <summary>
    ''' Bipolar sigmoid activation function.
    ''' </summary>
    '''
    ''' <remarks><para>The class represents bipolar sigmoid activation function with
    ''' the next expression:
    ''' <code lang="none">
    '''                2
    ''' f(x) = ------------------ - 1
    '''        1 + exp(-alpha * x)
    '''
    '''           2 * alpha * exp(-alpha * x )
    ''' f'(x) = -------------------------------- = alpha * (1 - f(x)^2) / 2
    '''           (1 + exp(-alpha * x))^2
    ''' </code>
    ''' </para>
    ''' 
    ''' <para>Output range of the function: <b>[-1, 1]</b>.</para>
    ''' 
    ''' <para>Functions graph:</para>
    ''' <img src="img/neuro/sigmoid_bipolar.bmp" width="242" height="172" />
    ''' </remarks>
    ''' 
    <Serializable>
    Public Class BipolarSigmoid : Inherits IActivationFunction

        ''' <summary>
        ''' Sigmoid's alpha value.
        ''' </summary>
        '''
        ''' <remarks><para>The value determines steepness of the function. Increasing value of
        ''' this property changes sigmoid to look more like a threshold function. Decreasing
        ''' value of this property makes sigmoid to be very smooth (slowly growing from its
        ''' minimum value to its maximum value).</para>
        '''
        ''' <para>Default value is set to <b>2</b>.</para>
        ''' </remarks>
        ''' 
        Public Property Alpha As Double = 2.0R

        Public Overrides ReadOnly Property Store As ActiveFunction
            Get
                Return New ActiveFunction With {
                    .Arguments = {
                        New NamedValue With {.name = "alpha", .text = Alpha}
                    },
                    .name = NameOf(BipolarSigmoid)
                }
            End Get
        End Property

        ''' <summary>
        ''' Initializes a new instance of the <see cref="SigmoidFunction"/> class.
        ''' </summary>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="BipolarSigmoid"/> class.
        ''' </summary>
        ''' 
        ''' <param name="alpha">Sigmoid's alpha value.</param>
        ''' 
        Public Sub New(alpha As Double)
            Me.Alpha = alpha
        End Sub

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
        Public Overrides Function [Function](x As Double) As Double
            Return ((2 / (1 + stdNum.Exp(-_Alpha * x))) - 1)
        End Function

        ''' <summary>
        ''' Calculates function derivative.
        ''' </summary>
        ''' 
        ''' <param name="x">Function input value.</param>
        ''' 
        ''' <returns>Function derivative, <i>f'(x)</i>.</returns>
        ''' 
        ''' <remarks>The method calculates function derivative at point <paramref name="x"/>.</remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Protected Overrides Function Derivative(x As Double) As Double
            Return (_Alpha * (1 - x * x) / 2)
        End Function

        Public Overrides Function ToString() As String
            Return $"{NameOf(BipolarSigmoid)}(alpha:={Alpha})"
        End Function
    End Class
End Namespace

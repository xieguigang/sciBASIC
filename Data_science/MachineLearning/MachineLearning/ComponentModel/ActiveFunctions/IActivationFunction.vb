#Region "Microsoft.VisualBasic::494928c5002f30d1e58d4a23d88fe383, Data_science\MachineLearning\MachineLearning\ComponentModel\ActiveFunctions\IActivationFunction.vb"

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

    '   Total Lines: 130
    '    Code Lines: 40 (30.77%)
    ' Comment Lines: 76 (58.46%)
    '    - Xml Docs: 92.11%
    ' 
    '   Blank Lines: 14 (10.77%)
    '     File Size: 5.17 KB


    '     Class IActivationFunction
    ' 
    '         Properties: Truncate
    ' 
    '         Function: CalculateDerivative
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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace ComponentModel.Activations

    ''' <summary>
    ''' Activation function interface.
    ''' </summary>
    ''' <remarks>
    ''' All activation functions, which are supposed to be used with
    ''' neurons, which calculate their output as a function of weighted sum of
    ''' their inputs, should implement this interfaces.
    ''' </remarks>
    Public MustInherit Class IActivationFunction

        ''' <summary>
        ''' Gets the XML serializable data model of this activation function, so 
        ''' that the function object can be stored into a xml document.
        ''' </summary>
        ''' <returns>
        ''' A <see cref="ActiveFunction"/> data model which can be serialized 
        ''' into a xml document.
        ''' </returns>
        Public MustOverride ReadOnly Property Store As ActiveFunction

        ''' <summary>
        ''' 因为激活函数在求导之后,结果值可能会出现无穷大
        ''' 所以可以利用这个值来限制求导之后的结果最大值
        ''' </summary>
        ''' <returns>
        ''' The absolute value limitation of the derivative value; a value that 
        ''' is not a positive number means no truncation will be applied.
        ''' </returns>
        Public Property Truncate As Double = 10000

        ''' <summary>
        ''' Evaluate the activation function value at the specific point.
        ''' </summary>
        ''' <param name="x">The function input value.</param>
        ''' <returns>The function output value <i>f(x)</i>.</returns>
        Default Public ReadOnly Property Evaluate(x As Double) As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Me.Function(x)
            End Get
        End Property

        ''' <summary>
        ''' Evaluate the activation function value for each element in a given vector.
        ''' </summary>
        ''' <param name="a">A <see cref="Vector"/> of the function input values.</param>
        ''' <returns>
        ''' A new <see cref="Vector"/> in which each element is the function 
        ''' output value of the corresponding element in <paramref name="a"/>.
        ''' </returns>
        Default Public ReadOnly Property Evaluate(a As Vector) As Vector
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New Vector(From x As Double In a Select Me.Function(x))
            End Get
        End Property

        ''' <summary>
        ''' Calculates the derivative value of this activation function at the 
        ''' specific point, and the result will be truncated by the 
        ''' <see cref="Truncate"/> limitation.
        ''' </summary>
        ''' <param name="x">The function input value.</param>
        ''' <returns>
        ''' The derivative value <i>f'(x)</i>; the infinite value will be 
        ''' replaced by ``±100000`` and the ``NaN`` value will be replaced by ``1``.
        ''' </returns>
        Public Overridable Function CalculateDerivative(x As Double) As Double
            Dim val As Double

            If Truncate > 0 Then
                val = ValueTruncate(Derivative(x), Truncate)
            Else
                val = Derivative(x)
            End If

            If Double.IsPositiveInfinity(val) Then
                Return 100000
            ElseIf Double.IsNegativeInfinity(val) Then
                Return -100000
            ElseIf val.IsNaNImaginary Then
                Return 1
            Else
                Return val
            End If
        End Function

        ''' <summary>
        ''' Calculates function value.
        ''' </summary>
        ''' <param name="x">Function input value.</param>
        ''' <returns>Function output value, <i>f(x)</i>.</returns>
        ''' <remarks>
        ''' The method calculates function value at point <paramref name="x"/>.
        ''' </remarks>
        Public MustOverride Function [Function](x As Double) As Double

        ''' <summary>
        ''' Calculates function derivative.
        ''' </summary>
        ''' <param name="x">Function input value.</param>
        ''' <returns>Function derivative, <i>f'(x)</i>.</returns>
        ''' <remarks>
        ''' The method calculates function derivative at point <paramref name="x"/>.
        ''' </remarks>
        Protected MustOverride Function Derivative(x As Double) As Double

        ''' <summary>
        ''' 必须要重写这个函数来将函数对象序列化为表达式字符串文本
        ''' </summary>
        ''' <returns>
        ''' The text expression of this activation function, which can be parsed 
        ''' back by the <see cref="ActiveFunction.Parse"/> function.
        ''' </returns>
        Public MustOverride Overrides Function ToString() As String

    End Class
End Namespace

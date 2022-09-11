#Region "Microsoft.VisualBasic::0fdc7c254a6f971337bc13093261487e, sciBASIC#\Data_science\MachineLearning\MachineLearning\ComponentModel\ActiveFunctions\IActivationFunction.vb"

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

    '   Total Lines: 82
    '    Code Lines: 31
    ' Comment Lines: 39
    '   Blank Lines: 12
    '     File Size: 3.00 KB


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

        Public MustOverride ReadOnly Property Store As ActiveFunction

        ''' <summary>
        ''' 因为激活函数在求导之后,结果值可能会出现无穷大
        ''' 所以可以利用这个值来限制求导之后的结果最大值
        ''' </summary>
        ''' <returns></returns>
        Public Property Truncate As Double = 100

        Default Public ReadOnly Property Evaluate(x As Double) As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Me.Function(x)
            End Get
        End Property

        Default Public ReadOnly Property Evaluate(a As Vector) As Vector
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New Vector(From x As Double In a Select Me.Function(x))
            End Get
        End Property

        Public Overridable Function CalculateDerivative(x As Double) As Double
            If Truncate > 0 Then
                Return ValueTruncate(Derivative(x), Truncate)
            Else
                Return Derivative(x)
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
        ''' <returns></returns>
        Public MustOverride Overrides Function ToString() As String

    End Class
End Namespace

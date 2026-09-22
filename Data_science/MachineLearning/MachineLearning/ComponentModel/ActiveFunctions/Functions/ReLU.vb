#Region "Microsoft.VisualBasic::016d17695cea1f7bd6b908f83b440074, Data_science\MachineLearning\MachineLearning\ComponentModel\ActiveFunctions\Functions\ReLU.vb"

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

    '   Total Lines: 166
    '    Code Lines: 72 (43.37%)
    ' Comment Lines: 80 (48.19%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 14 (8.43%)
    '     File Size: 6.45 KB


    '     Class ReLU
    ' 
    '         Properties: Store
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: [Function], CalculateDerivative, Derivative, (+3 Overloads) ReLU, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace ComponentModel.Activations

    ''' <summary>
    ''' The rectified linear unit (ReLU) activation function: the input value 
    ''' which is less than the threshold will be clipped as the threshold value.
    ''' </summary>
    ''' <remarks>
    ''' ReLU is the default activation function of the modern neural network
    ''' models, as its derivative is cheap to calculate and it does not suffer 
    ''' from the vanishing gradient problem on the positive half-axis.
    ''' </remarks>
    Public Class ReLU : Inherits IActivationFunction

        ''' <summary>
        ''' Gets the XML serializable data model of this ReLU function.
        ''' </summary>
        ''' <returns>
        ''' A <see cref="ActiveFunction"/> data model which its function name is 
        ''' ``ReLU``, and the ``threshold`` value is stored as 
        ''' its only argument.
        ''' </returns>
        Public Overrides ReadOnly Property Store As ActiveFunction
            Get
                Return New ActiveFunction With {
                    .Arguments = {
                        New NamedValue With {
                            .name = "threshold",
                            .text = threshold
                        }
                    },
                    .name = NameOf(ReLU)
                }
            End Get
        End Property

        ''' <summary>
        ''' The clip threshold value, all of the input values which are less 
        ''' than this threshold will be clipped as this threshold value.
        ''' </summary>
        ReadOnly threshold# = 0

        ''' <summary>
        ''' Create a standard ReLU function with the default threshold value ``0``.
        ''' </summary>
        Sub New()
        End Sub

        ''' <summary>
        ''' Create a ReLU function with a specific clipping threshold value.
        ''' </summary>
        ''' <param name="threshold">The clipping threshold value.</param>
        Sub New(threshold As Double)
            Me.threshold = threshold
        End Sub

        ''' <summary>
        ''' Calculates the ReLU function value: the value which is less than the 
        ''' threshold will be clipped as the threshold value.
        ''' </summary>
        ''' <param name="x">The function input value.</param>
        ''' <returns>
        ''' The function output value; the output will be truncated by the 
        ''' <see cref="IActivationFunction.Truncate"/> limitation when the 
        ''' <paramref name="x"/> value is greater than the threshold.
        ''' </returns>
        Public Overrides Function [Function](x As Double) As Double
            If x < threshold Then
                Return threshold
            ElseIf Truncate > 0 Then
                Return ValueTruncate(x, Truncate)
            Else
                Return x
            End If
        End Function

        ''' <summary>
        ''' The standard ReLU function: the negative value will be clipped as zero.
        ''' </summary>
        ''' <param name="x">The function input value.</param>
        ''' <returns>
        ''' ``0`` when <paramref name="x"/> is a negative value, otherwise the 
        ''' <paramref name="x"/> value itself.
        ''' </returns>
        Public Shared Function ReLU(x As Double) As Double
            If x < 0 Then
                Return 0
            Else
                Return x
            End If
        End Function

        ''' <summary>
        ''' Apply the standard ReLU function on each element of the given vector.
        ''' </summary>
        ''' <param name="x">A <see cref="Vector"/> of the function input values.</param>
        ''' <returns>
        ''' The <paramref name="x"/> vector itself, in which all of the negative 
        ''' elements have been clipped as zero (the vector is modified in place).
        ''' </returns>
        Public Shared Function ReLU(x As Vector) As Vector
            x(x < 0.0) = Vector.Zero
            Return x
        End Function

        ''' <summary>
        ''' ReLU activator function will clip the negative value as zero
        ''' </summary>
        ''' <param name="x">
        ''' An array of the function input values, this array will be modified in place.
        ''' </param>
        ''' <returns>
        ''' The <paramref name="x"/> array itself, in which all of the negative 
        ''' elements have been clipped as zero.
        ''' </returns>
        Public Shared Function ReLU(x As Double()) As Double()
            For i As Integer = 0 To x.Length - 1
                If x(i) < 0 Then
                    x(i) = 0
                End If
            Next

            Return x
        End Function

        ''' <summary>
        ''' Calculates the derivative of this ReLU function: ``1`` when the input 
        ''' value is not less than the threshold, otherwise the threshold value.
        ''' </summary>
        ''' <param name="x">The function input value.</param>
        ''' <returns>The derivative value.</returns>
        Public Overrides Function CalculateDerivative(x As Double) As Double
            If x < threshold Then
                Return threshold
            Else
                Return 1
            End If
        End Function

        ''' <summary>
        ''' Calculates the derivative of this ReLU function: ``1`` when the input 
        ''' value is not less than the threshold, otherwise the threshold value.
        ''' </summary>
        ''' <param name="x">The function input value.</param>
        ''' <returns>The derivative value.</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Protected Overrides Function Derivative(x As Double) As Double
            If x < threshold Then
                Return threshold
            Else
                Return 1
            End If
        End Function

        ''' <summary>
        ''' Display this activation function as a text expression.
        ''' </summary>
        ''' <returns>A text expression in format like ``ReLU()``.</returns>
        Public Overrides Function ToString() As String
            Return $"{NameOf(ReLU)}()"
        End Function
    End Class
End Namespace

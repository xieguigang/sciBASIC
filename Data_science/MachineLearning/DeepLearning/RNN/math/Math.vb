#Region "Microsoft.VisualBasic::b25fe64a5b418ec83f75f2b81caddf23, Data_science\MachineLearning\DeepLearning\RNN\math\Math.vb"

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

    '   Total Lines: 56
    '    Code Lines: 31 (55.36%)
    ' Comment Lines: 15 (26.79%)
    '    - Xml Docs: 60.00%
    ' 
    '   Blank Lines: 10 (17.86%)
    '     File Size: 1.59 KB


    ' 	Class Math
    ' 
    ' 	    Properties: eps
    ' 
    ' 	    Function: (+2 Overloads) close, (+2 Overloads) softmax
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace RNN

	''' <summary>
	''' Math helper functions
	''' </summary>
	Public Class Math

		''' <summary>Default epsilon used by the approximate floating point comparisons.</summary>
		Public Const compareEpsilon As Double = 0.000001

		''' <summary>
		''' Compares two doubles using the default epsilon.
		''' </summary>
		''' <param name="a">First value.</param>
		''' <param name="b">Second value.</param>
		''' <returns><c>True</c> when the two values differ by at most <see cref="compareEpsilon"/>.</returns>
		<MethodImpl(MethodImplOptions.AggressiveInlining)>
		Public Shared Function close(a As Double, b As Double) As Boolean
			Return std.Abs(a - b) <= compareEpsilon
		End Function

		''' <summary>
		''' Compares two doubles using an explicit epsilon.
		''' </summary>
		''' <param name="a">First value.</param>
		''' <param name="b">Second value.</param>
		''' <param name="eps">The tolerance to use.</param>
		''' <returns><c>True</c> when the two values differ by at most <paramref name="eps"/>.</returns>
		<MethodImpl(MethodImplOptions.AggressiveInlining)>
		Public Shared Function close(a As Double, b As Double, eps As Double) As Boolean
			Return std.Abs(a - b) <= eps
		End Function

		''' <summary>Gets the default comparison epsilon.</summary>
		Public Shared ReadOnly Property eps() As Double
			<MethodImpl(MethodImplOptions.AggressiveInlining)>
			Get
				Return compareEpsilon
			End Get
		End Property

		' Useful Matrix functions 

		''' <summary>
		''' Applies the softmax function with temperature 1.0.
		''' </summary>
		''' <param name="yAtt">The input matrix, interpreted as a score vector.</param>
		''' <returns>The normalized probability matrix.</returns>
		Public Shared Function softmax(yAtt As Matrix) As Matrix
			Dim e_to_x As Matrix = (New Matrix(yAtt)).exp()
			e_to_x = e_to_x.div(e_to_x.sum())
			Return e_to_x
		End Function

		''' <summary>
		''' Applies the softmax function with the given temperature.
		''' </summary>
		''' <param name="yAtt">The input matrix, interpreted as a score vector.</param>
		''' <param name="temperature">The temperature; it must not be close to zero.</param>
		''' <returns>The normalized probability matrix.</returns>
		Public Shared Function softmax(yAtt As Matrix, temperature As Double) As Matrix
			Dim e_to_x As Matrix = (New Matrix(yAtt)).div(temperature).exp()
			e_to_x = e_to_x.div(e_to_x.sum())
			Return e_to_x
		End Function
	End Class

End Namespace

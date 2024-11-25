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

		Public Const compareEpsilon As Double = 0.000001

		''' <summary>
		''' Double epsilon compare 
		''' </summary>
		''' <param name="a"></param>
		''' <param name="b"></param>
		''' <returns></returns>
		''' 
		<MethodImpl(MethodImplOptions.AggressiveInlining)>
		Public Shared Function close(a As Double, b As Double) As Boolean
			Return std.Abs(a - b) <= compareEpsilon
		End Function

		<MethodImpl(MethodImplOptions.AggressiveInlining)>
		Public Shared Function close(a As Double, b As Double, eps As Double) As Boolean
			Return std.Abs(a - b) <= eps
		End Function

		' return the comparison epsilon
		Public Shared ReadOnly Property eps() As Double
			<MethodImpl(MethodImplOptions.AggressiveInlining)>
			Get
				Return compareEpsilon
			End Get
		End Property

		' Useful Matrix functions 

		' Applies the softmax function with temperature = 1.0
		Public Shared Function softmax(yAtt As Matrix) As Matrix
			Dim e_to_x As Matrix = (New Matrix(yAtt)).exp()
			e_to_x = e_to_x.div(e_to_x.sum())
			Return e_to_x
		End Function

		' Applies the softmax function with the given temperature.
		' Temperature can't be close to 0.
		Public Shared Function softmax(yAtt As Matrix, temperature As Double) As Matrix
			Dim e_to_x As Matrix = (New Matrix(yAtt)).div(temperature).exp()
			e_to_x = e_to_x.div(e_to_x.sum())
			Return e_to_x
		End Function
	End Class

End Namespace

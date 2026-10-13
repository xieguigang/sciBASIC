#Region "Microsoft.VisualBasic::2f998b0d6366104e807fea8874bd8df5, Data_science\MachineLearning\DeepLearning\RNN\math\Math.vb"

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

    '   Total Lines: 71
    '    Code Lines: 31 (43.66%)
    ' Comment Lines: 30 (42.25%)
    '    - Xml Docs: 96.67%
    ' 
    '   Blank Lines: 10 (14.08%)
    '     File Size: 2.58 KB


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
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
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
		Public Shared Function softmax(yAtt As Tensor) As Tensor
			Return SoftmaxCore(yAtt, 1.0)
		End Function

		''' <summary>
		''' Applies the softmax function with the given temperature.
		''' </summary>
		''' <param name="yAtt">The input matrix, interpreted as a score vector.</param>
		''' <param name="temperature">The temperature; it must not be close to zero.</param>
		''' <returns>The normalized probability matrix.</returns>
		Public Shared Function softmax(yAtt As Tensor, temperature As Double) As Tensor
			Return SoftmaxCore(yAtt, temperature)
		End Function

		''' <summary>
		''' softmax 的共同实现：<c>exp((x - max)/T) / Σ exp((x - max)/T)</c>
		''' </summary>
		''' <remarks>
		''' 相比旧实现（直接 exp 再除总和）多了两步，但都是必需的：
		'''   * **减最大值**：旧写法在 logits 量级较大时会溢出成 Infinity，
		'''     进而让整个概率向量变成 NaN；减最大值后最大项的 e⁰ = 1，
		'''     既不会溢出，也保证分母至少为 1。数值上两者完全等价（分子分母同乘常数）。
		'''   * **温度守卫**：temperature 趋近 0 时除法会放大到无穷，这里夹一个下限。
		''' </remarks>
		Private Shared Function SoftmaxCore(yAtt As Tensor, temperature As Double) As Tensor
			Dim t As Double = temperature

			If t <= 0.0 OrElse Double.IsNaN(t) Then t = 1.0

			Dim src = yAtt.Data
			Dim maxVal As Double = src(0)

			For Each v In src
				If v > maxVal Then maxVal = v
			Next

			Dim out(src.Length - 1) As Double
			Dim sumExp As Double = 0.0

			For i = 0 To src.Length - 1
				Dim e = std.Exp((src(i) - maxVal) / t)
				out(i) = e
				sumExp += e
			Next

			If sumExp <= 0.0 Then sumExp = 1.0

			For i = 0 To out.Length - 1
				out(i) = out(i) / sumExp
			Next

			Return Tensor.Wrap(out, yAtt.Shape)
		End Function
	End Class

End Namespace

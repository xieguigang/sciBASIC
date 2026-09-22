#Region "Microsoft.VisualBasic::4c8969a6d465d74a4b00377be84512c3, Data_science\MachineLearning\DeepLearning\Extensions.vb"

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
    '    Code Lines: 40 (71.43%)
    ' Comment Lines: 9 (16.07%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (12.50%)
    '     File Size: 1.56 KB


    ' Module Extensions
    ' 
    '     Function: LogSumExp, Sigmoid, Softmax
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

''' <summary>
''' Numerically stable activation and normalization helpers shared by the deep learning models.
''' </summary>
<HideModuleName> Public Module Extensions

    ''' <summary>
    ''' Computes the log-sum-exp of a vector in a numerically stable way.
    ''' </summary>
    ''' <param name="values">The input values.</param>
    ''' <returns>
    ''' <c>log(sum(exp(values)))</c>, computed after subtracting the maximum value so the exponentials cannot
    ''' overflow; returns <see cref="Double.NegativeInfinity"/> when the vector is empty or all values are
    ''' negative infinity.
    ''' </returns>
    Public Function LogSumExp(values As Double()) As Double
        Dim maxVal = Double.NegativeInfinity
        For Each v In values
            If v > maxVal Then maxVal = v
        Next

        If Double.IsNegativeInfinity(maxVal) Then Return Double.NegativeInfinity

        Dim sumExp As Double = 0
        For Each v In values
            sumExp += std.Exp(v - maxVal)
        Next
        Return maxVal + std.Log(sumExp)
    End Function

    ''' <summary>
    ''' Computes the softmax of a one dimensional vector of logits.
    ''' </summary>
    ''' <param name="logits">The unnormalized scores.</param>
    ''' <returns>
    ''' A probability vector of the same length that sums to one; the maximum logit is subtracted before
    ''' exponentiation for numerical stability.
    ''' </returns>
    Public Function Softmax(logits As Double()) As Double()
        Dim n = logits.Length
        Dim maxVal = Double.NegativeInfinity
        For Each v In logits
            If v > maxVal Then maxVal = v
        Next

        Dim result = New Double(n - 1) {}
        Dim sumExp As Double = 0
        For i = 0 To n - 1
            result(i) = std.Exp(logits(i) - maxVal)
            sumExp += result(i)
        Next
        For i = 0 To n - 1
            result(i) /= sumExp
        Next
        Return result
    End Function

    ''' <summary>
    ''' Computes the logistic sigmoid <c>1 / (1 + exp(-x))</c>.
    ''' </summary>
    ''' <param name="x">The input value.</param>
    ''' <returns>The sigmoid of <paramref name="x"/>, in the range <c>(0, 1)</c>.</returns>
    Public Function Sigmoid(x As Double) As Double
        If x >= 0 Then
            Return 1.0 / (1.0 + std.Exp(-x))
        Else
            Dim ex = std.Exp(x)
            Return ex / (1.0 + ex)
        End If
    End Function
End Module

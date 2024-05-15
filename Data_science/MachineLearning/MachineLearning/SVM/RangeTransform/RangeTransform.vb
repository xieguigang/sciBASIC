#Region "Microsoft.VisualBasic::0ed7376f24e64e8d336722c70f3c1d75, Data_science\MachineLearning\MachineLearning\SVM\RangeTransform\RangeTransform.vb"

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

    '   Total Lines: 150
    '    Code Lines: 74
    ' Comment Lines: 55
    '   Blank Lines: 21
    '     File Size: 6.35 KB


    '     Class RangeTransform
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: (+2 Overloads) Compute, (+2 Overloads) Transform
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' 
' * SVM.NET Library
' * Copyright (C) 2008 Matthew Johnson
' * 
' * This program is free software: you can redistribute it and/or modify
' * it under the terms of the GNU General Public License as published by
' * the Free Software Foundation, either version 3 of the License, or
' * (at your option) any later version.
' * 
' * This program is distributed in the hope that it will be useful,
' * but WITHOUT ANY WARRANTY; without even the implied warranty of
' * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' * GNU General Public License for more details.
' * 
' * You should have received a copy of the GNU General Public License
' * along with this program.  If not, see <http://www.gnu.org/licenses/>.

Imports stdNum = System.Math

Namespace SVM

    ''' <summary>
    ''' Class which encapsulates a range transformation.
    ''' </summary>
    Public Class RangeTransform : Implements IRangeTransform

        ''' <summary>
        ''' Default lower bound for scaling (-1).
        ''' </summary>
        Public Const DEFAULT_LOWER_BOUND As Integer = -1
        ''' <summary>
        ''' Default upper bound for scaling (1).
        ''' </summary>
        Public Const DEFAULT_UPPER_BOUND As Integer = 1

        Friend ReadOnly _inputStart As Double()
        Friend ReadOnly _inputScale As Double()
        Friend ReadOnly _outputStart As Double
        Friend ReadOnly _outputScale As Double
        Friend ReadOnly _length As Integer

        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="minValues">The minimum values in each dimension.</param>
        ''' <param name="maxValues">The maximum values in each dimension.</param>
        ''' <param name="lowerBound">The desired lower bound for all dimensions.</param>
        ''' <param name="upperBound">The desired upper bound for all dimensions.</param>
        Public Sub New(minValues As Double(), maxValues As Double(), lowerBound As Double, upperBound As Double)
            _length = minValues.Length
            If maxValues.Length <> _length Then Throw New Exception("Number of max and min values must be equal.")
            _inputStart = New Double(_length - 1) {}
            _inputScale = New Double(_length - 1) {}

            For i = 0 To _length - 1
                _inputStart(i) = minValues(i)
                _inputScale(i) = maxValues(i) - minValues(i)
            Next

            _outputStart = lowerBound
            _outputScale = upperBound - lowerBound
        End Sub

        Friend Sub New(inputStart As Double(), inputScale As Double(), outputStart As Double, outputScale As Double, length As Integer)
            _inputStart = inputStart
            _inputScale = inputScale
            _outputStart = outputStart
            _outputScale = outputScale
            _length = length
        End Sub

        ''' <summary>
        ''' Determines the Range transform for the provided problem.  Uses the default lower and upper bounds.
        ''' </summary>
        ''' <param name="prob">The Problem to analyze</param>
        ''' <returns>The Range transform for the problem</returns>
        Public Shared Function Compute(prob As Problem) As RangeTransform
            Return Compute(prob, DEFAULT_LOWER_BOUND, DEFAULT_UPPER_BOUND)
        End Function

        ''' <summary>
        ''' Determines the Range transform for the provided problem.
        ''' </summary>
        ''' <param name="prob">The Problem to analyze</param>
        ''' <param name="lowerBound">The lower bound for scaling</param>
        ''' <param name="upperBound">The upper bound for scaling</param>
        ''' <returns>The Range transform for the problem</returns>
        Public Shared Function Compute(prob As Problem, lowerBound As Double, upperBound As Double) As RangeTransform
            Dim minVals = New Double(prob.maxIndex - 1) {}
            Dim maxVals = New Double(prob.maxIndex - 1) {}

            For i = 0 To prob.maxIndex - 1
                minVals(i) = Double.MaxValue
                maxVals(i) = Double.MinValue
            Next

            For i = 0 To prob.count - 1

                For j = 0 To prob.X(i).Length - 1
                    Dim index = prob.X(i)(j).index - 1
                    Dim value = prob.X(i)(j).value
                    minVals(index) = stdNum.Min(minVals(index), value)
                    maxVals(index) = stdNum.Max(maxVals(index), value)
                Next
            Next

            For i = 0 To prob.maxIndex - 1

                If minVals(i) = Double.MaxValue OrElse maxVals(i) = Double.MinValue Then
                    minVals(i) = 0
                    maxVals(i) = 0
                End If
            Next

            Return New RangeTransform(minVals, maxVals, lowerBound, upperBound)
        End Function

        ''' <summary>
        ''' Transforms the input array based upon the values provided.
        ''' </summary>
        ''' <param name="input">The input array</param>
        ''' <returns>A scaled array</returns>
        Public Function Transform(input As Node()) As Node() Implements IRangeTransform.Transform
            Dim output = New Node(input.Length - 1) {}

            For i = 0 To output.Length - 1
                Dim index = input(i).index
                Dim value = input(i).value
                output(i) = New Node(index, Transform(value, index))
            Next

            Return output
        End Function

        ''' <summary>
        ''' Transforms this an input value using the scaling transform for the provided dimension.
        ''' </summary>
        ''' <param name="input">The input value to transform</param>
        ''' <param name="index">The dimension whose scaling transform should be used</param>
        ''' <returns>The scaled value</returns>
        Public Function Transform(input As Double, index As Integer) As Double Implements IRangeTransform.Transform
            index -= 1
            Dim tmp = input - _inputStart(index)
            If _inputScale(index) = 0 Then Return 0
            tmp /= _inputScale(index)
            tmp *= _outputScale
            Return tmp + _outputStart
        End Function
    End Class
End Namespace

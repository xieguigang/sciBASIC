#Region "Microsoft.VisualBasic::42ceee9bf84b03ee9c5a4b51c7b09c5a, Data_science\MachineLearning\MachineLearning\SVM\RangeTransform\GaussianTransform.vb"

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

    '   Total Lines: 116
    '    Code Lines: 56
    ' Comment Lines: 41
    '   Blank Lines: 19
    '     File Size: 4.36 KB


    '     Class GaussianTransform
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Compute, (+2 Overloads) Transform
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
    ''' A transform which learns the mean and variance of a sample set and uses these to transform new data
    ''' so that it has zero mean and unit variance.
    ''' </summary>
    Public Class GaussianTransform : Implements IRangeTransform

        Friend ReadOnly _means As Double()
        Friend ReadOnly _stddevs As Double()

        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="means">Means in each dimension</param>
        ''' <param name="stddevs">Standard deviation in each dimension</param>
        Public Sub New(means As Double(), stddevs As Double())
            _means = means
            _stddevs = stddevs
        End Sub

        ''' <summary>
        ''' Determines the Gaussian transform for the provided problem.
        ''' </summary>
        ''' <param name="prob">The Problem to analyze</param>
        ''' <returns>The Gaussian transform for the problem</returns>
        Public Shared Function Compute(prob As Problem) As GaussianTransform
            Dim counts = New Integer(prob.maxIndex - 1) {}
            Dim means = New Double(prob.maxIndex - 1) {}

            For Each sample In prob.X

                For i = 0 To sample.Length - 1
                    means(sample(i).index - 1) += sample(i).value
                    counts(sample(i).index - 1) += 1
                Next
            Next

            For i = 0 To prob.maxIndex - 1
                If counts(i) = 0 Then counts(i) = 2
                means(i) /= counts(i)
            Next

            Dim stddevs = New Double(prob.maxIndex - 1) {}

            For Each sample In prob.X

                For i = 0 To sample.Length - 1
                    Dim diff = sample(i).value - means(sample(i).index - 1)
                    stddevs(sample(i).index - 1) += diff * diff
                Next
            Next

            For i = 0 To prob.maxIndex - 1
                If stddevs(i) = 0 Then Continue For
                stddevs(i) /= counts(i) - 1
                stddevs(i) = stdNum.Sqrt(stddevs(i))
            Next

            Return New GaussianTransform(means, stddevs)
        End Function

#Region "IRangeTransform Members"

        ''' <summary>
        ''' Transform the input value using the transform stored for the provided index.
        ''' </summary>
        ''' <param name="input">Input value</param>
        ''' <param name="index">Index of the transform to use</param>
        ''' <returns>The transformed value</returns>
        Public Function Transform(input As Double, index As Integer) As Double Implements IRangeTransform.Transform
            index -= 1
            If _stddevs(index) = 0 Then Return 0
            Dim diff = input - _means(index)
            diff /= _stddevs(index)
            Return diff
        End Function
        ''' <summary>
        ''' Transforms the input array.
        ''' </summary>
        ''' <param name="input">The array to transform</param>
        ''' <returns>The transformed array</returns>
        Public Function Transform(input As Node()) As Node() Implements IRangeTransform.Transform
            Dim output = New Node(input.Length - 1) {}

            For i = 0 To output.Length - 1
                Dim index = input(i).index
                Dim value = input(i).value
                output(i) = New Node(index, Transform(value, index))
            Next

            Return output
        End Function

#End Region
    End Class
End Namespace

#Region "Microsoft.VisualBasic::e91ebeaa2d6bb270b917c8e60474c2bb, Data_science\MachineLearning\MachineLearning\SVM\Models\Model.vb"

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

    '   Total Lines: 128
    '    Code Lines: 52
    ' Comment Lines: 53
    '   Blank Lines: 23
    '     File Size: 4.52 KB


    '     Class Model
    ' 
    '         Properties: classLabels, dimensionNames, numberOfClasses, numberOfSVPerClass, pairwiseProbabilityA
    '                     pairwiseProbabilityB, parameter, rho, supportVectorCoefficients, supportVectorCount
    '                     supportVectorIndices, supportVectors, trainingSize
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Equals, GetHashCode
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

Namespace SVM

    ''' <summary>
    ''' Encapsulates an SVM Model.
    ''' </summary>
    Public Class Model

        ''' <summary>
        ''' Parameter object.
        ''' </summary>
        Public Property parameter As Parameter

        ''' <summary>
        ''' Number of classes in the model.
        ''' </summary>
        Public Property numberOfClasses As Integer

        ''' <summary>
        ''' Total number of support vectors.
        ''' </summary>
        Public Property supportVectorCount As Integer

        ''' <summary>
        ''' The support vectors.
        ''' </summary>
        Public Property supportVectors As Node()()

        ''' <summary>
        ''' The coefficients for the support vectors.
        ''' </summary>
        Public Property supportVectorCoefficients As Double()()

        ''' <summary>
        ''' Values in [1,...,num_training_data] to indicate SVs in the training set
        ''' </summary>
        Public Property supportVectorIndices As Integer()

        ''' <summary>
        ''' Constants in decision functions
        ''' </summary>
        Public Property rho As Double()

        ''' <summary>
        ''' First pairwise probability.
        ''' </summary>
        Public Property pairwiseProbabilityA As Double()

        ''' <summary>
        ''' Second pairwise probability.
        ''' </summary>
        Public Property pairwiseProbabilityB As Double()

        ' for classification only

        ''' <summary>
        ''' Class labels.
        ''' </summary>
        Public Property classLabels As Integer()

        ''' <summary>
        ''' Number of support vectors per class.
        ''' </summary>
        Public Property numberOfSVPerClass As Integer()
        Public Property dimensionNames As String()

        Public Property trainingSize As Integer

        Public Sub New()
        End Sub

        Public Overrides Function Equals(obj As Object) As Boolean
            Dim test As Model = TryCast(obj, Model)

            If test Is Nothing Then
                Return False
            End If

            Dim same = classLabels.IsEqual(test.classLabels)
            same = same AndAlso numberOfClasses = test.numberOfClasses
            same = same AndAlso numberOfSVPerClass.IsEqual(test.numberOfSVPerClass)

            If pairwiseProbabilityA IsNot Nothing Then
                same = same AndAlso pairwiseProbabilityA.IsEqual(test.pairwiseProbabilityA)
            End If
            If pairwiseProbabilityB IsNot Nothing Then
                same = same AndAlso pairwiseProbabilityB.IsEqual(test.pairwiseProbabilityB)
            End If

            same = same AndAlso parameter.Equals(test.parameter)
            same = same AndAlso rho.IsEqual(test.rho)
            same = same AndAlso supportVectorCoefficients.IsEqual(test.supportVectorCoefficients)
            same = same AndAlso supportVectorCount = test.supportVectorCount
            same = same AndAlso supportVectors.IsEqual(test.supportVectors)

            Return same
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return classLabels.ComputeHashcode() +
                numberOfClasses.GetHashCode() +
                numberOfSVPerClass.ComputeHashcode() +
                pairwiseProbabilityA.ComputeHashcode() +
                pairwiseProbabilityB.ComputeHashcode() +
                parameter.GetHashCode() +
                rho.ComputeHashcode() +
                supportVectorCoefficients.ComputeHashcode2() +
                supportVectorCount.GetHashCode() +
                supportVectors.ComputeHashcode2()
        End Function
    End Class
End Namespace

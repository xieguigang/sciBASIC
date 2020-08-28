#Region "Microsoft.VisualBasic::78b12b52f864ece014ba46c95a89c4bc, Data_science\MachineLearning\MachineLearning\SVM\Models\Model.vb"

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

    '     Class Model
    ' 
    '         Properties: ClassLabels, DimensionNames, NumberOfClasses, NumberOfSVPerClass, PairwiseProbabilityA
    '                     PairwiseProbabilityB, Parameter, Rho, SupportVectorCoefficients, SupportVectorCount
    '                     SupportVectorIndices, SupportVectors
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
    <Serializable> Public Class Model

        ''' <summary>
        ''' Parameter object.
        ''' </summary>
        Public Property Parameter As Parameter

        ''' <summary>
        ''' Number of classes in the model.
        ''' </summary>
        Public Property NumberOfClasses As Integer

        ''' <summary>
        ''' Total number of support vectors.
        ''' </summary>
        Public Property SupportVectorCount As Integer

        ''' <summary>
        ''' The support vectors.
        ''' </summary>
        Public Property SupportVectors As Node()()

        ''' <summary>
        ''' The coefficients for the support vectors.
        ''' </summary>
        Public Property SupportVectorCoefficients As Double()()

        ''' <summary>
        ''' Values in [1,...,num_training_data] to indicate SVs in the training set
        ''' </summary>
        Public Property SupportVectorIndices As Integer()

        ''' <summary>
        ''' Constants in decision functions
        ''' </summary>
        Public Property Rho As Double()

        ''' <summary>
        ''' First pairwise probability.
        ''' </summary>
        Public Property PairwiseProbabilityA As Double()

        ''' <summary>
        ''' Second pairwise probability.
        ''' </summary>
        Public Property PairwiseProbabilityB As Double()

        ' for classification only

        ''' <summary>
        ''' Class labels.
        ''' </summary>
        Public Property ClassLabels As Integer()

        ''' <summary>
        ''' Number of support vectors per class.
        ''' </summary>
        Public Property NumberOfSVPerClass As Integer()
        Public Property DimensionNames As String()

        Public Sub New()
        End Sub

        Public Overrides Function Equals(obj As Object) As Boolean
            Dim test As Model = TryCast(obj, Model)
            If test Is Nothing Then Return False
            Dim same = ClassLabels.IsEqual(test.ClassLabels)
            same = same AndAlso NumberOfClasses = test.NumberOfClasses
            same = same AndAlso NumberOfSVPerClass.IsEqual(test.NumberOfSVPerClass)
            If PairwiseProbabilityA IsNot Nothing Then same = same AndAlso PairwiseProbabilityA.IsEqual(test.PairwiseProbabilityA)
            If PairwiseProbabilityB IsNot Nothing Then same = same AndAlso PairwiseProbabilityB.IsEqual(test.PairwiseProbabilityB)
            same = same AndAlso Parameter.Equals(test.Parameter)
            same = same AndAlso Rho.IsEqual(test.Rho)
            same = same AndAlso SupportVectorCoefficients.IsEqual(test.SupportVectorCoefficients)
            same = same AndAlso SupportVectorCount = test.SupportVectorCount
            same = same AndAlso SupportVectors.IsEqual(test.SupportVectors)
            Return same
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return ClassLabels.ComputeHashcode() + NumberOfClasses.GetHashCode() + NumberOfSVPerClass.ComputeHashcode() + PairwiseProbabilityA.ComputeHashcode() + PairwiseProbabilityB.ComputeHashcode() + Parameter.GetHashCode() + Rho.ComputeHashcode() + SupportVectorCoefficients.ComputeHashcode2() + SupportVectorCount.GetHashCode() + SupportVectors.ComputeHashcode2()
        End Function
    End Class
End Namespace

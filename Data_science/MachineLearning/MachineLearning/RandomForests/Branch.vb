#Region "Microsoft.VisualBasic::c641c89934f670955c71d35f77c8d8a2, Data_science\MachineLearning\MachineLearning\RandomForests\Branch.vb"

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

    '   Total Lines: 142
    '    Code Lines: 52 (36.62%)
    ' Comment Lines: 81 (57.04%)
    '    - Xml Docs: 97.53%
    ' 
    '   Blank Lines: 9 (6.34%)
    '     File Size: 5.83 KB


    '     Class Branch
    ' 
    '         Function: getClass, getMean, getMissClass, getMSE
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace RandomForests

    ''' <summary>
    ''' One node (branch) of a random forest decision tree.
    ''' </summary>
    ''' <remarks>
    ''' Each branch holds the sample indices which are falling into it, the 
    ''' splitting feature with its splitting threshold, and the indices of its 
    ''' two child branches. A branch with the <see cref="status"/> value ``F`` 
    ''' is a final (leaf) branch.
    ''' </remarks>
    Public Class Branch

        ''' <summary>
        ''' the cached mean phenotype of the samples falling into this branch.
        ''' For a leaf branch it is exactly the leaf value used for prediction.
        ''' </summary>
        ''' <remarks>
        ''' ``mean`` is the mean value of the phenotype of the samples in this 
        ''' branch, and ``mean_snp`` is the splitting threshold value of the 
        ''' <see cref="Feature"/> of this branch.
        ''' </remarks>
        Public mean, mean_snp As Double

        ''' <summary>
        ''' The majority class value of the samples falling into this branch, 
        ''' which is evaluated by the <see cref="getClass"/> method.
        ''' </summary>
        Public class_val As Integer
        ''' <summary>
        ''' 'F' for final branch
        ''' </summary>
        Public status As String = " "
        ''' <summary>
        ''' the splitting feature index and the child branch indices.
        ''' <see cref="Child1"/> takes the samples whose feature value is
        ''' less than or equals to <see cref="mean_snp"/>, and
        ''' <see cref="Child2"/> takes the remaining samples.
        ''' </summary>
        Public Feature, Child1, Child2, Parent As Integer
        ''' <summary>
        ''' the sample indices (pointing to the training set) of this branch.
        ''' Its ``Count`` value is used as the node cover for TreeSHAP.
        ''' </summary>
        Public list As New List(Of Integer)()

        ''' <summary>
        ''' Evaluate the mean phenotype value of the samples falling into this branch, 
        ''' the result will be cached in the <see cref="mean"/> field.
        ''' </summary>
        ''' <param name="phen">
        ''' The phenotype (label) value of each sample in the whole training set, 
        ''' the <see cref="list"/> of this branch holds the indices into this array.
        ''' </param>
        ''' <returns>The mean phenotype value of the samples of this branch.</returns>
        Public Overridable Function getMean(phen As Double()) As Double

            Dim i = 0
            mean = 0.0R
            For i = 0 To list.Count - 1
                mean = mean + phen(list(i))
            Next
            mean = mean / list.Count
            Return mean
        End Function

        ''' <summary>
        ''' Evaluate the majority class value of the samples falling into this 
        ''' branch, the result will be cached in the <see cref="class_val"/> field.
        ''' </summary>
        ''' <param name="phen">
        ''' The class label value of each sample in the whole training set, the
        ''' <see cref="list"/> of this branch holds the indices into this array.
        ''' </param>
        ''' <returns>
        ''' The index of the class which owns the most samples in this branch, 
        ''' the value is one of ``0``, ``1`` and ``2``.
        ''' </returns>
        Public Overridable Function getClass(phen As Double()) As Integer
            Dim i = 0
            Dim temp = New Integer(2) {}
            For i = 0 To list.Count - 1
                temp(phen(list(i))) += 1
            Next
            If temp(0) > temp(1) And temp(0) > temp(2) Then
                class_val = 0
            ElseIf temp(1) > temp(2) Then
                class_val = 1
            Else
                class_val = 2
            End If
            Return class_val
        End Function

        ''' <summary>
        ''' Evaluate the sum of the squared error of the samples falling into 
        ''' this branch, relative to the <see cref="mean"/> value of this branch.
        ''' </summary>
        ''' <param name="phen">
        ''' The phenotype (label) value of each sample in the whole training set.
        ''' </param>
        ''' <returns>
        ''' The sum of the squared deviations, this value is used as the 
        ''' regression loss of this branch.
        ''' </returns>
        Public Overridable Function getMSE(phen As Double()) As Double
            Dim i = 0
            getMean(phen)
            Dim MSE = 0.0R
            For i = 0 To list.Count - 1
                MSE = MSE + (phen(list(i)) - mean) * (phen(list(i)) - mean)
            Next
            'MSE=MSE/list.size();
            Return MSE
        End Function

        ''' <summary>
        ''' Evaluate the number of the misclassified samples of this branch, 
        ''' relative to the <see cref="class_val"/> value of this branch.
        ''' </summary>
        ''' <param name="phen">
        ''' The class label value of each sample in the whole training set.
        ''' </param>
        ''' <returns>
        ''' The sum of the absolute deviations between the real class label and 
        ''' the <see cref="class_val"/> value, this value is used as the 
        ''' classification loss of this branch.
        ''' </returns>
        Public Overridable Function getMissClass(phen As Double()) As Double
            Dim i = 0
            getClass(phen)
            Dim MSE = 0.0R
            For i = 0 To list.Count - 1
                MSE = MSE + std.Abs(CInt(phen(list(i))) - class_val)
            Next
            'MSE=MSE/list.size();
            Return MSE
        End Function
    End Class
End Namespace

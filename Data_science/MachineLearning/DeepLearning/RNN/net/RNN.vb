#Region "Microsoft.VisualBasic::1a41d6018d0e6b07be6b40475e5d9fba, Data_science\MachineLearning\DeepLearning\RNN\net\RNN.vb"

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

    '   Total Lines: 27
    '    Code Lines: 10 (37.04%)
    ' Comment Lines: 12 (44.44%)
    '    - Xml Docs: 91.67%
    ' 
    '   Blank Lines: 5 (18.52%)
    '     File Size: 1.13 KB


    '     Class RNN
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace RNN

    ''' <summary>
    ''' A recurrent neural network.
    ''' </summary>
    <Serializable> Public MustInherit Class RNN
        Implements IntegerSampleable, Trainable

        Public MustOverride Function forwardBackward(ix As Integer(), iy As Integer()) As Double Implements Trainable.forwardBackward
		Public MustOverride Function sampleIndices(n As Integer, seed As Integer(), temp As Double, advance As Boolean) As Integer() Implements IntegerSampleable.sampleIndices
		Public MustOverride Function sampleIndices(n As Integer, seed As Integer(), temp As Double) As Integer() Implements IntegerSampleable.sampleIndices

        ' * Get ** 

        ''' <summary>
        ''' Returns true if the net was initialized.
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride ReadOnly Property Initialized As Boolean

        ''' <summary>
        ''' Returns the vocabulary size (max index + 1), if initialized.
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride ReadOnly Property VocabularySize As Integer
	End Class
End Namespace

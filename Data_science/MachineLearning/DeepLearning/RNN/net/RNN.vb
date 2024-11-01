#Region "Microsoft.VisualBasic::7bd4848016bcf0b101d2dbd8d09fc506, Data_science\MachineLearning\DeepLearning\RNN\net\RNN.vb"

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

    '   Total Lines: 19
    '    Code Lines: 11 (57.89%)
    ' Comment Lines: 4 (21.05%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (21.05%)
    '     File Size: 880 B


    ' 	Class RNN
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace RNN

	' A recurrent neural network.
	<Serializable>
	Public MustInherit Class RNN
		Implements IntegerSampleable, Trainable
		Public MustOverride Function forwardBackward(ix As Integer(), iy As Integer()) As Double Implements Trainable.forwardBackward
		Public MustOverride Function sampleIndices(n As Integer, seed As Integer(), temp As Double, advance As Boolean) As Integer() Implements IntegerSampleable.sampleIndices
		Public MustOverride Function sampleIndices(n As Integer, seed As Integer(), temp As Double) As Integer() Implements IntegerSampleable.sampleIndices

		' * Get ** 

		' Returns true if the net was initialized.
		Public MustOverride ReadOnly Property Initialized As Boolean

		' Returns the vocabulary size (max index + 1), if initialized.
		Public MustOverride ReadOnly Property VocabularySize As Integer
	End Class
End Namespace

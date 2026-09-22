#Region "Microsoft.VisualBasic::1e8a1295888127fe256f9726e8dd3388, Data_science\MachineLearning\DeepLearning\RNN\net\RNN.vb"

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

    '   Total Lines: 50
    '    Code Lines: 10 (20.00%)
    ' Comment Lines: 33 (66.00%)
    '    - Xml Docs: 96.97%
    ' 
    '   Blank Lines: 7 (14.00%)
    '     File Size: 2.39 KB


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

        ''' <summary>
        ''' Runs one forward and backward pass over a training sequence.
        ''' </summary>
        ''' <param name="ix">The input token indices.</param>
        ''' <param name="iy">The target token indices.</param>
        ''' <returns>The loss of this sequence.</returns>
        Public MustOverride Function forwardBackward(ix As Integer(), iy As Integer()) As Double Implements Trainable.forwardBackward

        ''' <summary>
        ''' Samples a sequence of indices from the network.
        ''' </summary>
        ''' <param name="n">Number of indices to sample.</param>
        ''' <param name="seed">The seed indices used to warm up the hidden state.</param>
        ''' <param name="temp">Sampling temperature in <c>(0.0, 1.0]</c>.</param>
        ''' <param name="advance">When <c>True</c> the hidden state is advanced while consuming the seed.</param>
        ''' <returns>The sampled indices.</returns>
		Public MustOverride Function sampleIndices(n As Integer, seed As Integer(), temp As Double, advance As Boolean) As Integer() Implements IntegerSampleable.sampleIndices

        ''' <summary>
        ''' Samples a sequence of indices from the network, advancing the hidden state.
        ''' </summary>
        ''' <param name="n">Number of indices to sample.</param>
        ''' <param name="seed">The seed indices used to warm up the hidden state.</param>
        ''' <param name="temp">Sampling temperature in <c>(0.0, 1.0]</c>.</param>
        ''' <returns>The sampled indices.</returns>
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

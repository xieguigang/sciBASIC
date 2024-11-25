#Region "Microsoft.VisualBasic::fe472601b9af1ab3255b76663dc8be10, Data_science\MachineLearning\DeepLearning\RNN\net\MultiLayerCharLevelRNN.vb"

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

    '   Total Lines: 119
    '    Code Lines: 48 (40.34%)
    ' Comment Lines: 55 (46.22%)
    '    - Xml Docs: 70.91%
    ' 
    '   Blank Lines: 16 (13.45%)
    '     File Size: 3.33 KB


    ' 	Class MultiLayerCharLevelRNN
    ' 
    ' 	    Properties: Alphabet, Initialized, VocabularySize
    ' 
    ' 	    Constructor: (+2 Overloads) Sub New
    ' 
    ' 	    Function: forwardBackward, (+2 Overloads) sampleIndices
    ' 
    ' 	    Sub: initialize, SetHiddenSize, SetLearningRate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace RNN

	''' <summary>
	''' Single layer character level RNN.
	''' </summary>
	<Serializable>
	Public Class MultiLayerCharLevelRNN : Inherits CharLevelRNN

		''' <summary>
		''' The alphabet for sampling.
		''' </summary>
		Protected Friend m_alphabet As Alphabet
		''' <summary>
		''' Basic network.
		''' </summary>
		Protected Friend internal As MultiLayerRNN

		''' <summary>
		''' Constructs without initialization.
		''' </summary>
		Public Sub New()
			internal = New MultiLayerRNN()
		End Sub

		' Constructs and initializes immediately.
		' Requires that alphabet != null.
		Public Sub New(alphabet As Alphabet)
			Me.New()
			initialize(alphabet)
		End Sub

		' * Hyperparameters ** 

		' Sets the hidden layer size. Network must be initialized again.
		Public Sub SetHiddenSize(value As Integer())
			internal.HiddenSize = value
		End Sub

		' Sets the learning rate.
		Public Sub SetLearningRate(value As Double)
			internal.LearningRate = value
		End Sub

		' * Initialize ** 

		' Initializes the net. alphabet != null.
		Public Overrides Sub initialize(alphabet As Alphabet)
			m_alphabet = alphabet
			internal.initialize(alphabet.size())
		End Sub

		' * Train ** 

		' 
		' 		    Performs a forward-backward pass for the given indices.
		' 	
		' 		    ix.length and iy.length lengths must match, can't be empty.
		' 		    All indices must be less than the vocabulary size.
		' 	
		' 		    Returns the cross-entropy loss.
		' 		
		Public Overrides Function forwardBackward(ix As Integer(), iy As Integer()) As Double
			Return internal.forwardBackward(ix, iy)
		End Function

		''' <summary>
		''' Samples n indices, sequence seed, advance the state.
		''' </summary>
		''' <param name="n"></param>
		''' <param name="seed"></param>
		''' <param name="temp"></param>
		''' <returns></returns>
		Public Overloads Overrides Function sampleIndices(n As Integer, seed As Integer(), temp As Double) As Integer()
			Return internal.sampleIndices(n, seed, temp)
		End Function

		''' <summary>
		''' Samples n indices, sequence seed, choose whether to advance the state.
		''' </summary>
		''' <param name="n"></param>
		''' <param name="seed"></param>
		''' <param name="temp"></param>
		''' <param name="advance"></param>
		''' <returns></returns>
		Public Overloads Overrides Function sampleIndices(n As Integer, seed As Integer(), temp As Double, advance As Boolean) As Integer()
			Return internal.sampleIndices(n, seed, temp, advance)
		End Function

		''' <summary>
		''' Returns the alphabet, if initialized.
		''' </summary>
		''' <returns></returns>
		Public Overrides ReadOnly Property Alphabet As Alphabet
			Get
				Return m_alphabet
			End Get
		End Property

		''' <summary>
		''' Returns true if the net was initialized.
		''' </summary>
		''' <returns></returns>
		Public Overrides ReadOnly Property Initialized As Boolean
			Get
				Return internal.Initialized
			End Get
		End Property

		''' <summary>
		''' Returns the vocabulary size (the alphabet size), if initialized.
		''' </summary>
		''' <returns></returns>
		Public Overrides ReadOnly Property VocabularySize As Integer
			Get
				Return internal.VocabularySize
			End Get
		End Property
	End Class
End Namespace

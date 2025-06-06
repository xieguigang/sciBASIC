﻿#Region "Microsoft.VisualBasic::220ddb3e6c94bbc468a99db615c4e0f6, Data_science\MachineLearning\DeepLearning\RNN\net\SingleLayerCharLevelRNN.vb"

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

    '   Total Lines: 101
    '    Code Lines: 49 (48.51%)
    ' Comment Lines: 35 (34.65%)
    '    - Xml Docs: 51.43%
    ' 
    '   Blank Lines: 17 (16.83%)
    '     File Size: 2.95 KB


    ' 	Class SingleLayerCharLevelRNN
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
	Public Class SingleLayerCharLevelRNN
		Inherits CharLevelRNN
		Protected Friend alphabetField As Alphabet ' The alphabet for sampling.

		Protected Friend internal As SingleLayerRNN ' Basic network.

		' Constructs without initialization.
		Public Sub New()
			internal = New SingleLayerRNN()
		End Sub

		' Constructs and initializes immediately. Requires that alphabet != null.
		Public Sub New(alphabet As Alphabet)
			Me.New()
			initialize(alphabet)
		End Sub

		' * Hyperparameters ** 

		''' <summary>
		''' Sets the hidden layer size. Network must be initialized again.
		''' </summary>
		''' <param name="value"></param>
		Public Sub SetHiddenSize(value As Integer)
			internal.HiddenSize = value
		End Sub

		' Sets the learning rate.
		Public Sub SetLearningRate(value As Double)
			internal.LearningRate = value
		End Sub

		''' <summary>
		''' Initializes the net. alphabet != null.
		''' </summary>
		''' <param name="alphabet"></param>
		Public Overrides Sub initialize(alphabet As Alphabet)
			alphabetField = alphabet
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

		' Samples n indices, sequence seed, choose whether to advance the state.
		Public Overloads Overrides Function sampleIndices(n As Integer, seed As Integer(), temp As Double, advance As Boolean) As Integer()
			Return internal.sampleIndices(n, seed, temp, advance)
		End Function

		' Returns the alphabet, if initialized.
		Public Overrides ReadOnly Property Alphabet As Alphabet
			Get
				Return alphabetField
			End Get
		End Property

		' Returns true if the net was initialized.
		Public Overrides ReadOnly Property Initialized As Boolean
			Get
				Return internal.Initialized
			End Get
		End Property

		' Returns the vocabulary size (the alphabet size), if initialized.
		Public Overrides ReadOnly Property VocabularySize As Integer
			Get
				Return internal.VocabularySize
			End Get
		End Property
	End Class


End Namespace

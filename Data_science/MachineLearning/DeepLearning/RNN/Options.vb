#Region "Microsoft.VisualBasic::5bfbd0f94c67c665da0f711658450fcc, Data_science\MachineLearning\DeepLearning\RNN\Options.vb"

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

    '   Total Lines: 49
    '    Code Lines: 17 (34.69%)
    ' Comment Lines: 10 (20.41%)
    '    - Xml Docs: 30.00%
    ' 
    '   Blank Lines: 22 (44.90%)
    '     File Size: 1.42 KB


    ' 	Class Options
    ' 
    ' 	    Properties: hiddenSize, inputFile, layers, learningRate, loopAroundTimes
    '                  printOptions, sampleEveryNSteps, samplingTemp, sequenceLength, snapshotEveryNSamples
    '                  trainingSampleLength, useSingleLayerNet
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO

Namespace RNN

	''' <summary>
	''' Application options.
	''' </summary>
	Public Class Options

		' * Model parameters ** 

		''' <summary>Size of a single RNN layer hidden state.</summary>
		Public Property hiddenSize As Integer = 100

		''' <summary>How many layers the network has.</summary>
		Public Property layers As Integer = 2


		' * Training parameters ** 

		''' <summary>How many steps the network is unrolled during training.</summary>
		Public Property sequenceLength As Integer = 50

		''' <summary>The network learning rate.</summary>
		Public Property learningRate As Double = 0.1


		' * Sampling parameters **

		''' <summary>
		''' Sampling temperature in <c>(0.0, 1.0]</c>. A lower temperature yields more conservative predictions.
		''' </summary>
		Public Property samplingTemp As Double = 1.0

		' * Other options ** 

		''' <summary>When <c>True</c> the options are printed at the start of the run.</summary>
		Public Property printOptions As Boolean = True

		''' <summary>Length of a sample drawn from the training data.</summary>
		Public Property trainingSampleLength As Integer = 400

		''' <summary>Take a network snapshot every N samples.</summary>
		Public Property snapshotEveryNSamples As Integer = 50

		''' <summary>How many times the training data is traversed.</summary>
		Public Property loopAroundTimes As Integer = 0


		''' <summary>Take a sample during training every N steps.</summary>
		Public Property sampleEveryNSteps As Integer = 100

		''' <summary>Path of the training data file.</summary>
		Public Property inputFile As String = "input.txt"

		''' <summary>When <c>True</c> the simple single layer network is used.</summary>
		Public Property useSingleLayerNet As Boolean = False

	End Class
End Namespace

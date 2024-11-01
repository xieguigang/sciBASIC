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

		Public Property hiddenSize As Integer = 100 ' Size of a single RNN layer hidden state.

		Public Property layers As Integer = 2 ' How many layers in a net?


		' * Training parameters ** 

		Public Property sequenceLength As Integer = 50 ' How many steps to unroll during training?

		Public Property learningRate As Double = 0.1 ' The network learning rate.


		' * Sampling parameters **

		' Sampling temperature (0.0, 1.0]. Lower
		' temperature means more conservative
		' predictions.
		Public Property samplingTemp As Double = 1.0

		' * Other options ** 

		Public Property printOptions As Boolean = True ' Print options at the start.

		Public Property trainingSampleLength As Integer = 400 ' Length of a sample during training.

		Public Property snapshotEveryNSamples As Integer = 50 ' Take a network's snapshot every N samples.

		Public Property loopAroundTimes As Integer = 0 ' Loop around the training data this many times.


		Public Property sampleEveryNSteps As Integer = 100 ' Take a sample during training every N steps.

		Public Property inputFile As String = "input.txt" ' The training data.

		Public Property useSingleLayerNet As Boolean = False ' Use the simple, single layer net.

	End Class
End Namespace

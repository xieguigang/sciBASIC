#Region "Microsoft.VisualBasic::26db482767ff0d8be7562148c8da19c7, Data_science\MachineLearning\DeepLearning\RNN\CharRNN.vb"

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

    '   Total Lines: 187
    '    Code Lines: 110 (58.82%)
    ' Comment Lines: 42 (22.46%)
    '    - Xml Docs: 35.71%
    ' 
    '   Blank Lines: 35 (18.72%)
    '     File Size: 7.64 KB


    '     Class CharRNN
    ' 
    '         Function: initialize, loadASnapshot
    ' 
    '         Sub: sample, saveASnapshot, train
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO

Namespace RNN

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/garstka/char-rnn-java
    ''' </remarks>
    Public Class CharRNN

        ''' <summary>
        ''' 01. Initialize a network for training.
        ''' </summary>
        ''' <param name="options"></param>
        ''' <returns></returns>
        Public Shared Function initialize(options As Options) As CharLevelRNN
            If options.useSingleLayerNet Then
                ' legacy network, single layer only
                Dim net As SingleLayerCharLevelRNN = New SingleLayerCharLevelRNN()
                net.SetHiddenSize(options.hiddenSize)
                net.SetLearningRate(options.learningRate)
                Return net ' Multi layer network.
            Else
                Dim net As MultiLayerCharLevelRNN = New MultiLayerCharLevelRNN()

                ' Use the same hidden size for all layers.
                Dim hiddenSize = options.hiddenSize
                Dim hidden = New Integer(options.layers - 1) {}
                For i = 0 To hidden.Length - 1
                    hidden(i) = hiddenSize
                Next
                net.SetHiddenSize(hidden)
                net.SetLearningRate(options.learningRate)
                Return net
            End If
        End Function

        ' Trains the network.
        Public Shared Sub train(options As Options, net As CharLevelRNN, snapshotName As String)
            ' Load the training set.

            Dim trainingSet = StringTrainingSet.fromFile(options.inputFile)

            Console.WriteLine("Data size: " & trainingSet.size().ToString() & ", vocabulary size: " & trainingSet.vocabularySize().ToString())

            ' Initialize the network and its trainer.

            If Not net.Initialized Then ' Only if not restored from a snapshot.
                net.initialize(trainingSet.Alphabet)
            End If

            Dim trainer As RNNTrainer = New RNNTrainer()
            trainer.SequenceLength = options.sequenceLength
            trainer.initialize(net, trainingSet)
            trainer.printDebug(True)

            ' For sampling during training, pick the temperature from options
            ' and the first character in the training set as seed.
            Dim seed = Convert.ToString(trainingSet.Data(0))
            Dim samplingTemperature = options.samplingTemp
            Dim sampleLength = options.trainingSampleLength


            Dim loopTimes = options.loopAroundTimes
            Dim sampleEveryNSteps = options.sampleEveryNSteps
            Dim snapshotEveryNSamples = options.snapshotEveryNSamples
            Dim nextSnapshotNumber = 1
            While True ' Go over the whole training set.

                Try

                    ' Train for some steps and sample a short string
                    ' for evaluation.
                    Dim batchCount = 0
                    While True

                        If System.Math.Min(Threading.Interlocked.Increment(batchCount), batchCount - 1) Mod snapshotEveryNSamples = 0 Then
                            Call saveASnapshot(snapshotName & "-" & System.Math.Min(Threading.Interlocked.Increment(nextSnapshotNumber), nextSnapshotNumber - 1).ToString(), net)
                        End If

                        Console.WriteLine("___________________")

                        trainer.train(sampleEveryNSteps)

                        Console.WriteLine(net.sampleString(sampleLength, seed, samplingTemperature, False))
                    End While
                Catch __unusedNoMoreTrainingDataException1__ As Exception
                    Console.WriteLine("Out of training data.")
                End Try

                Call saveASnapshot(snapshotName & "-" & System.Math.Min(Threading.Interlocked.Increment(nextSnapshotNumber), nextSnapshotNumber - 1).ToString(), net)

                If loopTimes <= 0 Then
                    Exit While
                End If

                Console.WriteLine("Looping around " & System.Math.Max(Threading.Interlocked.Decrement(loopTimes), loopTimes + 1).ToString() & "more time(s).")

                trainer.loopAround()
            End While
        End Sub

        ' Saves a network snapshot with this name to file.
        Public Shared Sub saveASnapshot(name As String, net As CharLevelRNN)
            If ReferenceEquals(name, Nothing) Then
                Throw New NullReferenceException("Network name can't be null.")
            End If

            If net Is Nothing Then
                Throw New NullReferenceException("Network can't be null.")
            End If

            ' Take a snapshot
            Try
                Using str As FileStream = New FileStream(name & ".snapshot", FileMode.Create, FileAccess.Write)
                    'ObjectOutputStream ostr = new ObjectOutputStream(str);
                    'ostr.writeObject(net);
                    'ostr.close();
                End Using
            Catch __unusedIOException1__ As IOException
                Console.WriteLine("Couldn't save a snapshot.")
                Return
            End Try
            Console.WriteLine("Saved as " & name & ".snapshot")
        End Sub

        ''' <summary>
        ''' 02. Loads a network snapshot with this name from file.
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Shared Function loadASnapshot(name As String) As CharLevelRNN
            If ReferenceEquals(name, Nothing) Then
                Throw New NullReferenceException("Name can't be null.")
            End If

            Dim net As CharLevelRNN = Nothing

            ' Load the snapshot
            Try
                Using str As FileStream = New FileStream(name & ".snapshot", FileMode.Open, FileAccess.Read)
                    'ObjectInputStream ostr = new ObjectInputStream(str);
                    'net = (CharLevelRNN) ostr.readObject();
                    'ostr.close();
                End Using
            Catch e As Exception When TypeOf e Is IOException
                Throw New IOException("Couldn't load the snapshot from file.", e)
            End Try
            Return net
        End Function


        ' 
        ' 		    Samples the net for n characters and prints the result.
        ' 		    Requirements:
        ' 		     - n >= 1,
        ' 		     - seed != null
        ' 		     - net != null, must be initialized
        ' 		     - temperature in (0.0,1.0]
        ' 		 
        Public Shared Sub sample(n As Integer, seed As String, temperature As Double, net As CharLevelRNN)
            If n < 1 Then
                Throw New ArgumentException("n must be at least 1")
            End If

            If net Is Nothing Then
                Throw New NullReferenceException("Network can't be null.")
            End If

            If ReferenceEquals(seed, Nothing) Then
                Throw New NullReferenceException("Seed can't be null.")
            End If

            If Not net.Initialized Then
                Throw New ArgumentException("Network must be initialized.")
            End If

            Try
                Console.WriteLine(net.sampleString(n, seed, temperature)) ' sample and advance
            Catch __unusedCharacterNotInAlphabetException1__ As Exception
                Console.WriteLine("Error: Character not in alphabet.")
            End Try
        End Sub
    End Class
End Namespace

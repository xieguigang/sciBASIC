#Region "Microsoft.VisualBasic::1e57e711bdabaac818f6e8f4b4288c2f, Data_science\DataMining\DataMining\SelfOrganizingMap.vb"

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

    '   Total Lines: 151
    '    Code Lines: 86 (56.95%)
    ' Comment Lines: 36 (23.84%)
    '    - Xml Docs: 66.67%
    ' 
    '   Blank Lines: 29 (19.21%)
    '     File Size: 5.30 KB


    ' Class SelfOrganizingMap
    ' 
    '     Properties: class_id, depth, numberOfNeurons
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: clusterId, embeddings, findNearestNeuron, train
    ' 
    '     Sub: shufflePixels, updateNeuronWeights
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.Tqdm
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Math.Correlations
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

''' <summary>
''' SOM: Self-Organizing Map
''' </summary>
Public Class SelfOrganizingMap

    Public ReadOnly Property numberOfNeurons As Integer

    ''' <summary>
    ''' data channel depth
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property depth As Integer

    ''' <summary>
    ''' weight matrix
    ''' </summary>
    Dim neuronWeights As Double()()
    Dim pixelsData As Double()()

    Public ReadOnly Property class_id As IReadOnlyCollection(Of Integer)

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="numberOfNeurons">number of the neurons</param>
    Public Sub New(numberOfNeurons As Integer, depth As Integer)
        Me.numberOfNeurons = numberOfNeurons
        Me.depth = depth

        neuronWeights = RectangularArray.Matrix(Of Double)(numberOfNeurons, depth)
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <returns></returns>
    Public Function embeddings() As Double()()
        Return neuronWeights.ToArray
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="pixels">dataset for run the training, data should be an rectangle array, 
    ''' with 2nd dimension size should be equals to <see cref="depth"/>.</param>
    Public Function train(ByRef pixels As Double()(),
                          Optional learningRate As Double = 0.9,
                          Optional alpha As Double = 0.01,
                          Optional epoch As Integer = 500) As SelfOrganizingMap

        Dim numberOfPixels = pixels.Length
        Dim numberOfFeatures = pixels(0).Length
        Dim globalMax As Double = Aggregate f In pixels Into Max(f.Max)
        Dim globalMin As Double = Aggregate f In pixels Into Min(f.Min)
        Dim delta As Double = globalMax - globalMin
        Dim initialLearningRate = learningRate
        Dim decaySteps As Integer = epoch / 5

        Const minLearningRate = 0.001

        pixelsData = RectangularArray.CopyOf(pixels)

        ' Initialize neuron weights randomly
        Dim w = RectangularArray.Matrix(Of Double)(numberOfNeurons, numberOfFeatures)
        For i As Integer = 0 To numberOfNeurons - 1
            For j As Integer = 0 To numberOfFeatures - 1
                ' Initialize with a random value within the data range
                w(i)(j) = globalMin + randf.NextDouble * delta
            Next
        Next

        ' SOM training algorithm
        For Each iteration As Integer In TqdmWrapper.Range(0, epoch)
            ' Randomly shuffle the pixels
            Call shufflePixels(pixels)

            ' Update neuron weights for each pixel
            For i As Integer = 0 To numberOfPixels - 1
                Dim pixel = pixels(i)
                Dim nearestNeuron = findNearestNeuron(pixel, w)

                Call updateNeuronWeights(pixel, w, nearestNeuron, learningRate)
            Next

            ' Decrease the learning rate
            ' Update the learning rate linearly
            learningRate = std.Max(minLearningRate, initialLearningRate - (initialLearningRate - minLearningRate) * (iteration / decaySteps))
        Next

        ' Set the neuron weights as representative colors
        neuronWeights = w
        _class_id = clusterId()

        Return Me
    End Function

    Private Sub shufflePixels(ByRef pixels As Double()())
        For i As Integer = pixels.Length - 1 To 1 Step -1
            Dim j = randf.Next(i + 1)
            Dim temp = pixels(i)
            pixels(i) = pixels(j)
            pixels(j) = temp
        Next
    End Sub

    Private Function findNearestNeuron(ByRef pixel As Double(), ByRef neuronWeightsMatrix As Double()()) As Integer
        Dim nearestNeuron = 0
        Dim minDistance = Double.MaxValue

        For i As Integer = 0 To numberOfNeurons - 1
            Dim distance = pixel.EuclideanDistance(neuronWeightsMatrix(i))

            If distance < minDistance Then
                minDistance = distance
                nearestNeuron = i
            End If
        Next

        ' returns the index
        Return nearestNeuron
    End Function

    Private Sub updateNeuronWeights(pixel As Double(), neuronWeightsMatrix As Double()(), nearestNeuron As Integer, learningRate As Double)
        Dim neuronWeights = neuronWeightsMatrix(nearestNeuron)

        For i As Integer = 0 To neuronWeights.Length - 1
            neuronWeights(i) += learningRate * (pixel(i) - neuronWeights(i))
        Next
    End Sub

    ''' <summary>
    ''' matrix data clustering
    ''' </summary>
    ''' <returns></returns>
    Private Function clusterId() As Integer()
        Dim numberOfPixels = pixelsData.Length
        Dim class_id = New Integer(numberOfPixels - 1) {}

        For i As Integer = 0 To numberOfPixels - 1
            class_id(i) = findNearestNeuron(pixelsData(i), neuronWeights)
        Next

        Return class_id
    End Function
End Class


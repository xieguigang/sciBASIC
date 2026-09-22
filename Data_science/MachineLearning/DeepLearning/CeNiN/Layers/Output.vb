#Region "Microsoft.VisualBasic::fff43df0d6ec8139482ddefc2f9b1294, Data_science\MachineLearning\DeepLearning\CeNiN\Layers\Output.vb"

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

    '   Total Lines: 74
    '    Code Lines: 38 (51.35%)
    ' Comment Lines: 24 (32.43%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 12 (16.22%)
    '     File Size: 3.48 KB


    '     Class Output
    ' 
    '         Properties: probabilities, sortedClasses, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: feedNext, getDecision, layerFeedNext, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Convolutional

    ''' <summary>
    ''' Terminal layer of a CeNiN network. It does not compute activations; it exposes the class labels
    ''' together with the probabilities produced by the preceding softmax layer.
    ''' </summary>
    Public Class Output : Inherits Layer

        Friend ReadOnly m_classes As String()

        ''' <summary>Class labels sorted by descending probability after a call to <see cref="getDecision"/>.</summary>
        Public ReadOnly Property sortedClasses As String()
        ''' <summary>Probabilities that correspond one to one with <see cref="sortedClasses"/>.</summary>
        Public ReadOnly Property probabilities As Single()

        ''' <summary>Gets the layer kind, always <see cref="CNN.LayerTypes.Output"/>.</summary>
        Public Overrides ReadOnly Property type As CNN.LayerTypes
            Get
                Return CNN.LayerTypes.Output
            End Get
        End Property

        ''' <summary>
        ''' Creates the output layer.
        ''' </summary>
        ''' <param name="inputTensorDims">The dimensions <c>[height, width, classCount]</c> of the incoming tensor.</param>
        ''' <param name="classes">The class labels in the order used by the network's training set.</param>
        Public Sub New(inputTensorDims As Integer(), classes As String())
            Call MyBase.New(inputTensorDims)

            m_classes = classes
            probabilities = New Single(inputTensorDims(2) - 1) {}
            sortedClasses = New String(inputTensorDims(2) - 1) {}
        End Sub

        ''' <summary>
        ''' Sorts the class probabilities in descending order and returns the most likely class label.
        ''' </summary>
        ''' <returns>The label of the class with the highest predicted probability.</returns>
        Public Function getDecision() As String
            If inputTensor.data IsNot Nothing Then
                Call Array.Copy(m_classes, sortedClasses, m_classes.Length)
                Call Array.ConstrainedCopy(inputTensor.data, 0, probabilities, 0, m_classes.Length)
                Call Array.Sort(probabilities, sortedClasses)
                Call Array.Reverse(probabilities)
                Call Array.Reverse(sortedClasses)

                Call disposeInputTensor()
            End If

            Return sortedClasses(0)
        End Function

        ''' <summary>
        ''' Always throws because the output layer is the end of the network and cannot feed another layer.
        ''' </summary>
        ''' <returns>This method never returns.</returns>
        Protected Overrides Function layerFeedNext() As Layer
            Throw New InvalidOperationException("the output layer cann't be feed to next layer!")
        End Function

        ''' <summary>Advances the output layer; this terminates the forward pass.</summary>
        ''' <returns>This method never returns.</returns>
        Public Overrides Function feedNext() As Layer
            Return layerFeedNext()
        End Function

        ''' <summary>Returns a short description of the class labels held by this layer.</summary>
        ''' <returns>A text that lists the class count and the first few class labels.</returns>
        Public Overrides Function ToString() As String
            Return $"{m_classes.Length} class tags: [{m_classes.Take(6).JoinBy("; ")}...]"
        End Function
    End Class
End Namespace

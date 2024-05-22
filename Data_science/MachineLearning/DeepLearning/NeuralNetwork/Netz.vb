#Region "Microsoft.VisualBasic::9de059e0c9219a5af0c76441a392924c, Data_science\MachineLearning\DeepLearning\NeuralNetwork\Netz.vb"

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

    '   Total Lines: 280
    '    Code Lines: 226 (80.71%)
    ' Comment Lines: 6 (2.14%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 48 (17.14%)
    '     File Size: 10.98 KB


    '     Class Netz
    ' 
    '         Properties: Bias, HiddenLayerCount, HiddenNeuronCount, InputNeuronCount, MaxOutputNeuronIndex
    '                     Neurons, Output, OutputNeuronCount, TotalError, Weights
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: predict
    ' 
    '         Sub: addInput, adjustWeightsAndBias2, calculateError2, run, train
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.Activations
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace NeuralNetwork

    ''' <summary>
    ''' Neural Network for regression analysis
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/brokkoli71/NeuralNetwork
    ''' </remarks>
    Public Class Netz : Inherits Model

        Public LERNRATE As Double = 0.01
        Private ReadOnly m_HIDDENLAYERCOUNT As Integer
        Private ReadOnly m_INPUTNEURONCOUNT As Integer
        Private ReadOnly m_HIDDENNEURONCOUNT As Integer
        Private ReadOnly m_OUTPUTNEURONCOUNT As Integer
        Private ReadOnly MAXNEURONCOUNT As Integer

        Private m_neurons As Double()()
        Private neuronsIsAlive As Boolean()()
        Private deltas As Double()()
        Private m_weights As Double()()()
        Private weightsIsAlive As Boolean()()()
        Private m_bias As Double()()
        Private biasIsAlive As Boolean()()

        Dim m_activate As Func(Of Double, Double) = AddressOf Sigmoid.doCall

        Public Sub New(inputNeurons As Integer, hiddenNeurons As Integer, hiddenLayers As Integer, outputNeurons As Integer, activate As Func(Of Double, Double))
            m_HIDDENLAYERCOUNT = hiddenLayers
            m_INPUTNEURONCOUNT = inputNeurons
            m_HIDDENNEURONCOUNT = hiddenNeurons
            m_OUTPUTNEURONCOUNT = outputNeurons
            m_activate = activate

            MAXNEURONCOUNT = If(m_HIDDENNEURONCOUNT > m_INPUTNEURONCOUNT, m_HIDDENNEURONCOUNT, m_INPUTNEURONCOUNT)

            m_neurons = RectangularArray.Matrix(Of Double)(m_HIDDENLAYERCOUNT + 2, MAXNEURONCOUNT) '[Layer][Neuron]
            neuronsIsAlive = RectangularArray.Matrix(Of Boolean)(m_HIDDENLAYERCOUNT + 2, MAXNEURONCOUNT) '[Layer][Neuron]
            deltas = RectangularArray.Matrix(Of Double)(m_HIDDENLAYERCOUNT + 2, MAXNEURONCOUNT)

            m_weights = RectangularArray.Cubic(Of Double)(m_HIDDENLAYERCOUNT + 1, MAXNEURONCOUNT, MAXNEURONCOUNT) '[FromHiddenLayer][FromHiddenNeuron][ToHiddenNeuron]
            weightsIsAlive = RectangularArray.Cubic(Of Boolean)(m_HIDDENLAYERCOUNT + 1, MAXNEURONCOUNT, MAXNEURONCOUNT) '[FromHiddenLayer][FromHiddenNeuron][ToHiddenNeuron]

            m_bias = RectangularArray.Matrix(Of Double)(m_HIDDENLAYERCOUNT + 2, MAXNEURONCOUNT) '[Layer][Neuron]
            biasIsAlive = RectangularArray.Matrix(Of Boolean)(m_HIDDENLAYERCOUNT + 2, MAXNEURONCOUNT) '[Neuron]

            For i = 0 To m_INPUTNEURONCOUNT - 1
                For j = 0 To m_HIDDENNEURONCOUNT - 1
                    m_weights(0)(i)(j) = (randf.NextDouble() * 2) - 1
                    weightsIsAlive(0)(i)(j) = True
                Next
            Next

            For i = 1 To m_HIDDENLAYERCOUNT - 1
                For j = 0 To m_HIDDENNEURONCOUNT - 1
                    For k = 0 To m_HIDDENNEURONCOUNT - 1
                        m_weights(i)(j)(k) = (randf.NextDouble() * 2) - 1
                        weightsIsAlive(i)(j)(k) = True
                    Next
                Next
            Next

            For i = 0 To m_HIDDENNEURONCOUNT - 1
                For j = 0 To m_OUTPUTNEURONCOUNT - 1
                    m_weights(m_HIDDENLAYERCOUNT)(i)(j) = (randf.NextDouble() * 2) - 1
                    weightsIsAlive(m_HIDDENLAYERCOUNT)(i)(j) = True
                Next
            Next

            For i = 1 To m_HIDDENLAYERCOUNT + 1 - 1
                For j = 0 To m_HIDDENNEURONCOUNT - 1
                    m_bias(i)(j) = (randf.NextDouble() * 2) - 1
                    biasIsAlive(i)(j) = True
                Next
            Next

            For i = 0 To m_OUTPUTNEURONCOUNT - 1
                m_bias(m_HIDDENLAYERCOUNT + 1)(i) = (randf.NextDouble() * 2) - 1
                biasIsAlive(m_HIDDENLAYERCOUNT + 1)(i) = True
            Next

            For i = 0 To m_INPUTNEURONCOUNT - 1
                neuronsIsAlive(0)(i) = True
            Next

            For i = 0 To m_HIDDENLAYERCOUNT - 1
                For j = 0 To m_HIDDENNEURONCOUNT - 1
                    neuronsIsAlive(i + 1)(j) = True
                Next
            Next
            For i = 0 To m_OUTPUTNEURONCOUNT - 1
                neuronsIsAlive(m_HIDDENLAYERCOUNT + 1)(i) = True
            Next
        End Sub

        Private Sub addInput(input As Double())
            m_neurons(0) = input
        End Sub

        Public Overridable Sub run(input As Double())
            Call addInput(input)

            For i = 1 To m_HIDDENLAYERCOUNT + 2 - 1
                For j = 0 To m_HIDDENNEURONCOUNT - 1
                    Dim sum As Double = 0

                    For k = 0 To If(i = 1, m_INPUTNEURONCOUNT, m_HIDDENNEURONCOUNT) - 1
                        sum += m_neurons(i - 1)(k) * m_weights(i - 1)(k)(j)
                    Next

                    m_neurons(i)(j) = m_activate(sum + m_bias(i)(j))
                Next
            Next
        End Sub

        Public Overridable ReadOnly Property Output As Double()
            Get

                Dim lOutput = New Double(m_OUTPUTNEURONCOUNT - 1) {}
                For i = 0 To lOutput.Length - 1
                    If neuronsIsAlive(m_HIDDENLAYERCOUNT + 1)(i) Then
                        lOutput(i) = m_neurons(m_HIDDENLAYERCOUNT + 1)(i)
                    End If
                Next
                Return lOutput
            End Get
        End Property

        Public Overridable Function predict(input As Double()) As Double()
            run(input)
            Return Output
        End Function

        Private Sub calculateError2(goodOutput As Double())
            For i = 0 To goodOutput.Length - 1
                deltas(m_HIDDENLAYERCOUNT + 1)(i) = m_neurons(m_HIDDENLAYERCOUNT + 1)(i) * (1 - m_neurons(m_HIDDENLAYERCOUNT + 1)(i))
                deltas(m_HIDDENLAYERCOUNT + 1)(i) *= m_neurons(m_HIDDENLAYERCOUNT + 1)(i) - goodOutput(i)
                deltas(m_HIDDENLAYERCOUNT + 1)(i) *= -1
            Next

            For i = m_HIDDENLAYERCOUNT To 1 Step -1
                For j = 0 To MAXNEURONCOUNT - 1

                    If neuronsIsAlive(i)(j) Then
                        deltas(i)(j) = 0
                        For k = 0 To MAXNEURONCOUNT - 1
                            deltas(i)(j) += deltas(i + 1)(k) * m_weights(i)(j)(k)
                        Next
                        deltas(i)(j) *= m_neurons(i)(j) * (1 - m_neurons(i)(j))
                    End If
                Next
            Next
        End Sub

        Private Sub adjustWeightsAndBias2()
            For i = 0 To m_HIDDENLAYERCOUNT + 1 - 1 'fromLayer
                For k = 0 To m_HIDDENNEURONCOUNT - 1 'toNeuron
                    For j = 0 To If(i = 0, m_INPUTNEURONCOUNT, m_HIDDENNEURONCOUNT) - 1 'fromNeuron
                        If weightsIsAlive(i)(j)(k) Then
                            m_weights(i)(j)(k) += LERNRATE * deltas(i + 1)(k) * m_neurons(i)(j)
                        End If
                    Next
                    If biasIsAlive(i + 1)(k) Then 'toNeuron
                        m_bias(i + 1)(k) += LERNRATE * deltas(i + 1)(k)
                    End If
                Next
            Next
        End Sub

        Public Overridable Sub train(input As Double(), goodOutput As Double())
            run(input)
            calculateError2(goodOutput)
            adjustWeightsAndBias2()
        End Sub

        Public Overridable ReadOnly Property TotalError As Double
            Get
                Dim [error] As Double = 0
                For i = 0 To m_OUTPUTNEURONCOUNT - 1
                    [error] += deltas(m_HIDDENLAYERCOUNT + 1)(i)
                Next
                Return [error] / m_OUTPUTNEURONCOUNT
            End Get
        End Property

        Public Overridable ReadOnly Property Weights As List(Of Double)
            Get
                Dim allWeights As List(Of Double) = New List(Of Double)()

                For i = 0 To m_weights.Length - 1
                    For j = 0 To m_weights(i).Length - 1
                        For k = 0 To m_weights(i)(j).Length - 1
                            If weightsIsAlive(i)(j)(k) Then
                                allWeights.Add(2 * m_activate(m_weights(i)(j)(k)) - 1)
                            End If
                        Next
                    Next
                Next

                Return allWeights
            End Get
        End Property
        Public Overridable ReadOnly Property Neurons As List(Of Double)
            Get
                Dim allNeurons As List(Of Double) = New List(Of Double)()

                For i = 0 To m_neurons.Length - 1
                    For j = 0 To m_neurons(i).Length - 1
                        If neuronsIsAlive(i)(j) Then
                            allNeurons.Add(m_neurons(i)(j))
                        End If
                    Next
                Next

                Return allNeurons
            End Get
        End Property

        Public Overridable ReadOnly Property Bias As List(Of Double)
            Get
                Dim allBias As List(Of Double) = New List(Of Double)()

                For i = 0 To m_bias.Length - 1
                    For j = 0 To m_bias(i).Length - 1
                        If biasIsAlive(i)(j) Then
                            allBias.Add(m_bias(i)(j))
                        End If
                    Next
                Next

                Return allBias
            End Get
        End Property


        Public Overridable ReadOnly Property HiddenLayerCount As Integer
            Get
                Return m_HIDDENLAYERCOUNT
            End Get
        End Property


        Public Overridable ReadOnly Property InputNeuronCount As Integer
            Get
                Return m_INPUTNEURONCOUNT
            End Get
        End Property


        Public Overridable ReadOnly Property HiddenNeuronCount As Integer
            Get
                Return m_HIDDENNEURONCOUNT
            End Get
        End Property


        Public Overridable ReadOnly Property OutputNeuronCount As Integer
            Get
                Return m_OUTPUTNEURONCOUNT
            End Get
        End Property
        Public Overridable ReadOnly Property MaxOutputNeuronIndex As Integer
            Get
                Dim output = m_neurons(m_HIDDENLAYERCOUNT + 1)
                Dim maximumOutput = 0
                For i = 1 To m_OUTPUTNEURONCOUNT - 1
                    If output(maximumOutput) < output(i) Then
                        maximumOutput = i
                    End If
                Next
                Return maximumOutput
            End Get
        End Property
    End Class

End Namespace

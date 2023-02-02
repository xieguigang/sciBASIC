Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports stdNum = System.Math

Namespace NeuralNetwork

    ''' <summary>
    ''' Neural Network for regression analysis
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/brokkoli71/NeuralNetwork
    ''' </remarks>
    Public Class Netz

        Public LERNRATE As Double = 0.01
        Private ReadOnly HIDDENLAYERCOUNTField As Integer
        Private ReadOnly INPUTNEURONCOUNTField As Integer
        Private ReadOnly HIDDENNEURONCOUNTField As Integer
        Private ReadOnly OUTPUTNEURONCOUNTField As Integer
        Private ReadOnly MAXNEURONCOUNT As Integer

        Private neuronsField As Double()()
        Private neuronsIsAlive As Boolean()()
        Private neuronsCost As Double()()
        Private deltas As Double()()


        Private weightsField As Double()()()
        Private weightsIsAlive As Boolean()()()


        Private biasField As Double()()
        Private biasIsAlive As Boolean()()

        Private outputError As Double()

        Public Sub New(inputNeurons As Integer, hiddenNeurons As Integer, hiddenLayers As Integer, outputNeurons As Integer)
            HIDDENLAYERCOUNTField = hiddenLayers
            INPUTNEURONCOUNTField = inputNeurons
            HIDDENNEURONCOUNTField = hiddenNeurons
            OUTPUTNEURONCOUNTField = outputNeurons
            MAXNEURONCOUNT = If(HIDDENNEURONCOUNTField > INPUTNEURONCOUNTField, HIDDENNEURONCOUNTField, INPUTNEURONCOUNTField)

            'JAVA TO C# CONVERTER CRACKED BY X-CRACKER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
            'ORIGINAL LINE: neurons = new double[HIDDENLAYERCOUNT+2][MAXNEURONCOUNT]; //[Layer][Neuron]
            neuronsField = RectangularArray.Matrix(Of Double)(HIDDENLAYERCOUNTField + 2, MAXNEURONCOUNT) '[Layer][Neuron]
            'JAVA TO C# CONVERTER CRACKED BY X-CRACKER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
            'ORIGINAL LINE: neuronsIsAlive = new bool[HIDDENLAYERCOUNT+2][MAXNEURONCOUNT]; //[Layer][Neuron]
            neuronsIsAlive = RectangularArray.Matrix(Of Boolean)(HIDDENLAYERCOUNTField + 2, MAXNEURONCOUNT) '[Layer][Neuron]
            'JAVA TO C# CONVERTER CRACKED BY X-CRACKER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
            'ORIGINAL LINE: neuronsCost = new double[HIDDENLAYERCOUNT+2][MAXNEURONCOUNT];
            neuronsCost = RectangularArray.Matrix(Of Double)(HIDDENLAYERCOUNTField + 2, MAXNEURONCOUNT)
            'JAVA TO C# CONVERTER CRACKED BY X-CRACKER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
            'ORIGINAL LINE: deltas = new double[HIDDENLAYERCOUNT+2][MAXNEURONCOUNT];
            deltas = RectangularArray.Matrix(Of Double)(HIDDENLAYERCOUNTField + 2, MAXNEURONCOUNT)


            'JAVA TO C# CONVERTER CRACKED BY X-CRACKER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
            'ORIGINAL LINE: weights = new double[HIDDENLAYERCOUNT+1][MAXNEURONCOUNT][MAXNEURONCOUNT]; //[FromHiddenLayer][FromHiddenNeuron][ToHiddenNeuron]
            weightsField = RectangularArray.Matrix(Of Double)(HIDDENLAYERCOUNTField + 1, MAXNEURONCOUNT, MAXNEURONCOUNT) '[FromHiddenLayer][FromHiddenNeuron][ToHiddenNeuron]
            'JAVA TO C# CONVERTER CRACKED BY X-CRACKER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
            'ORIGINAL LINE: weightsIsAlive = new bool[HIDDENLAYERCOUNT+1][MAXNEURONCOUNT][MAXNEURONCOUNT]; //[FromHiddenLayer][FromHiddenNeuron][ToHiddenNeuron]
            weightsIsAlive = RectangularArray.Matrix(Of Boolean)(HIDDENLAYERCOUNTField + 1, MAXNEURONCOUNT, MAXNEURONCOUNT) '[FromHiddenLayer][FromHiddenNeuron][ToHiddenNeuron]

            'JAVA TO C# CONVERTER CRACKED BY X-CRACKER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
            'ORIGINAL LINE: bias = new double[HIDDENLAYERCOUNT+2][MAXNEURONCOUNT]; //[Layer][Neuron]
            biasField = RectangularArray.Matrix(Of Double)(HIDDENLAYERCOUNTField + 2, MAXNEURONCOUNT) '[Layer][Neuron]
            'JAVA TO C# CONVERTER CRACKED BY X-CRACKER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
            'ORIGINAL LINE: biasIsAlive = new bool[HIDDENLAYERCOUNT+2][MAXNEURONCOUNT]; //[Neuron]
            biasIsAlive = RectangularArray.Matrix(Of Boolean)(HIDDENLAYERCOUNTField + 2, MAXNEURONCOUNT) '[Neuron]

            For i = 0 To INPUTNEURONCOUNTField - 1
                For j = 0 To HIDDENNEURONCOUNTField - 1
                    weightsField(0)(i)(j) = ((New Random()).NextDouble() * 2) - 1
                    weightsIsAlive(0)(i)(j) = True
                Next
            Next

            For i = 1 To HIDDENLAYERCOUNTField - 1
                For j = 0 To HIDDENNEURONCOUNTField - 1
                    For k = 0 To HIDDENNEURONCOUNTField - 1
                        weightsField(i)(j)(k) = ((New Random()).NextDouble() * 2) - 1
                        weightsIsAlive(i)(j)(k) = True
                    Next
                Next
            Next

            For i = 0 To HIDDENNEURONCOUNTField - 1
                For j = 0 To OUTPUTNEURONCOUNTField - 1
                    weightsField(HIDDENLAYERCOUNTField)(i)(j) = ((New Random()).NextDouble() * 2) - 1
                    weightsIsAlive(HIDDENLAYERCOUNTField)(i)(j) = True
                Next
            Next

            For i = 1 To HIDDENLAYERCOUNTField + 1 - 1
                For j = 0 To HIDDENNEURONCOUNTField - 1
                    biasField(i)(j) = ((New Random()).NextDouble() * 2) - 1
                    biasIsAlive(i)(j) = True
                Next
            Next

            For i = 0 To OUTPUTNEURONCOUNTField - 1
                biasField(HIDDENLAYERCOUNTField + 1)(i) = ((New Random()).NextDouble() * 2) - 1
                biasIsAlive(HIDDENLAYERCOUNTField + 1)(i) = True
            Next

            For i = 0 To INPUTNEURONCOUNTField - 1
                neuronsIsAlive(0)(i) = True
            Next

            For i = 0 To HIDDENLAYERCOUNTField - 1
                For j = 0 To HIDDENNEURONCOUNTField - 1
                    neuronsIsAlive(i + 1)(j) = True
                Next
            Next
            For i = 0 To OUTPUTNEURONCOUNTField - 1
                neuronsIsAlive(HIDDENLAYERCOUNTField + 1)(i) = True
            Next
        End Sub


        Private Sub addInput(input As Double())
            neuronsField(0) = input
        End Sub


        Public Overridable Sub run(input As Double())
            addInput(input)
            For i = 1 To HIDDENLAYERCOUNTField + 2 - 1
                For j = 0 To HIDDENNEURONCOUNTField - 1
                    Dim sum As Double = 0

                    For k = 0 To If(i = 1, INPUTNEURONCOUNTField, HIDDENNEURONCOUNTField) - 1
                        sum += neuronsField(i - 1)(k) * weightsField(i - 1)(k)(j)
                    Next

                    neuronsField(i)(j) = sigmoidValue(sum + biasField(i)(j))
                Next
            Next
        End Sub


        Public Overridable ReadOnly Property Output As Double()
            Get

                Dim lOutput = New Double(OUTPUTNEURONCOUNTField - 1) {}
                For i = 0 To lOutput.Length - 1
                    If neuronsIsAlive(HIDDENLAYERCOUNTField + 1)(i) Then
                        lOutput(i) = neuronsField(HIDDENLAYERCOUNTField + 1)(i)
                    End If
                Next
                Return lOutput
            End Get
        End Property

        Public Overridable Function getOutput(input As Double()) As Double()
            run(input)
            Return Output
        End Function



        Private Sub calculateError(goodOutput As Double())

            For i = 0 To goodOutput.Length - 1
                neuronsCost(HIDDENLAYERCOUNTField + 1)(i) = goodOutput(i) - neuronsField(HIDDENLAYERCOUNTField + 1)(i)
            Next
            For i = HIDDENLAYERCOUNTField To 1 Step -1
                For j = 0 To MAXNEURONCOUNT - 1
                    If neuronsIsAlive(i)(j) Then
                        neuronsCost(i)(j) = 0
                        For k = 0 To MAXNEURONCOUNT - 1
                            neuronsCost(i)(j) += neuronsCost(i + 1)(k) * weightsField(i)(j)(k)
                        Next
                        neuronsCost(i)(j) = neuronsCost(i)(j) ^ 2
                    End If
                Next
            Next
        End Sub

        Private Sub adjustWeightsAndBias()
            Dim neuronGradient As Double
            For i = 0 To HIDDENLAYERCOUNTField + 1 - 1 'fromLayer
                For k = 0 To HIDDENNEURONCOUNTField - 1 'toNeuron

                    neuronGradient = neuronsField(i + 1)(k) * (1 - neuronsField(i + 1)(k)) 'dsigmoid of toNeurons
                    neuronGradient *= neuronsCost(i + 1)(k) 'errors of toNeurons
                    neuronGradient *= LERNRATE

                    For j = 0 To If(i = 0, INPUTNEURONCOUNTField, HIDDENNEURONCOUNTField) - 1 'fromNeuron

                        If weightsIsAlive(i)(j)(k) Then
                            weightsField(i)(j)(k) += neuronGradient * neuronsField(i)(j) ' neuronGradient * neurons of 1st layer
                        End If
                    Next

                    If biasIsAlive(i + 1)(k) Then 'toNeuron
                        biasField(i + 1)(k) += neuronGradient
                    End If
                Next
            Next
        End Sub

        Private Sub calculateError2(goodOutput As Double())

            For i = 0 To goodOutput.Length - 1
                deltas(HIDDENLAYERCOUNTField + 1)(i) = neuronsField(HIDDENLAYERCOUNTField + 1)(i) * (1 - neuronsField(HIDDENLAYERCOUNTField + 1)(i))
                deltas(HIDDENLAYERCOUNTField + 1)(i) *= neuronsField(HIDDENLAYERCOUNTField + 1)(i) - goodOutput(i)
                deltas(HIDDENLAYERCOUNTField + 1)(i) *= -1
            Next

            For i = HIDDENLAYERCOUNTField To 1 Step -1
                For j = 0 To MAXNEURONCOUNT - 1

                    If neuronsIsAlive(i)(j) Then
                        deltas(i)(j) = 0
                        For k = 0 To MAXNEURONCOUNT - 1
                            deltas(i)(j) += deltas(i + 1)(k) * weightsField(i)(j)(k)
                        Next
                        deltas(i)(j) *= neuronsField(i)(j) * (1 - neuronsField(i)(j))
                    End If
                Next
            Next
        End Sub

        Private Sub adjustWeightsAndBias2()
            For i = 0 To HIDDENLAYERCOUNTField + 1 - 1 'fromLayer
                For k = 0 To HIDDENNEURONCOUNTField - 1 'toNeuron
                    For j = 0 To If(i = 0, INPUTNEURONCOUNTField, HIDDENNEURONCOUNTField) - 1 'fromNeuron
                        If weightsIsAlive(i)(j)(k) Then
                            weightsField(i)(j)(k) += LERNRATE * deltas(i + 1)(k) * neuronsField(i)(j)
                        End If
                    Next
                    If biasIsAlive(i + 1)(k) Then 'toNeuron
                        biasField(i + 1)(k) += LERNRATE * deltas(i + 1)(k)
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
                For i = 0 To OUTPUTNEURONCOUNTField - 1
                    [error] += deltas(HIDDENLAYERCOUNTField + 1)(i)
                Next
                Return [error] / OUTPUTNEURONCOUNTField
            End Get
        End Property

        Private Function sigmoidValue(d As Double) As Double
            Return 1 / (1 + stdNum.Exp(-d))
        End Function

        Public Overridable ReadOnly Property Weights As List(Of Double)
            Get
                Dim allWeights As List(Of Double) = New List(Of Double)()

                For i = 0 To weightsField.Length - 1
                    For j = 0 To weightsField(i).Length - 1
                        For k = 0 To weightsField(i)(j).Length - 1
                            If weightsIsAlive(i)(j)(k) Then
                                allWeights.Add(2 * sigmoidValue(weightsField(i)(j)(k)) - 1)
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

                For i = 0 To neuronsField.Length - 1
                    For j = 0 To neuronsField(i).Length - 1
                        If neuronsIsAlive(i)(j) Then
                            allNeurons.Add(neuronsField(i)(j))
                        End If
                    Next
                Next

                Return allNeurons
            End Get
        End Property

        Public Overridable ReadOnly Property Bias As List(Of Double)
            Get
                Dim allBias As List(Of Double) = New List(Of Double)()

                For i = 0 To biasField.Length - 1
                    For j = 0 To biasField(i).Length - 1
                        If biasIsAlive(i)(j) Then
                            allBias.Add(biasField(i)(j))
                        End If
                    Next
                Next

                Return allBias
            End Get
        End Property


        Public Overridable ReadOnly Property HiddenLayerCount As Integer
            Get
                Return HIDDENLAYERCOUNTField
            End Get
        End Property


        Public Overridable ReadOnly Property InputNeuronCount As Integer
            Get
                Return INPUTNEURONCOUNTField
            End Get
        End Property


        Public Overridable ReadOnly Property HiddenNeuronCount As Integer
            Get
                Return HIDDENNEURONCOUNTField
            End Get
        End Property


        Public Overridable ReadOnly Property OutputNeuronCount As Integer
            Get
                Return OUTPUTNEURONCOUNTField
            End Get
        End Property
        Public Overridable ReadOnly Property MaxOutputNeuronIndex As Integer
            Get
                Dim output = neuronsField(HIDDENLAYERCOUNTField + 1)
                Dim maximumOutput = 0
                For i = 1 To OUTPUTNEURONCOUNTField - 1
                    If output(maximumOutput) < output(i) Then
                        maximumOutput = i
                    End If
                Next
                Return maximumOutput
            End Get
        End Property
    End Class

End Namespace

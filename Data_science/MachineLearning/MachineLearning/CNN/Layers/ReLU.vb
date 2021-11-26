
Namespace Convolutional

    Friend Class ReLU : Inherits Layer

        Public Overrides ReadOnly Property type As LayerTypes
            Get
                Return LayerTypes.ReLU
            End Get
        End Property

        Public Sub New(inputTensorDims As Integer())
            Call MyBase.New(inputTensorDims)
        End Sub

        Protected Overrides Function layerFeedNext() As Layer
            Dim inputHeight = inputTensorDims(0)
            Dim inputWidth = inputTensorDims(1)
            Dim channelCount = inputTensorDims(2)
            Dim f As Single
            Dim inputInd = New Integer() {0, 0, 0}

            While inputInd(0) < inputHeight
                inputInd(1) = 0

                While inputInd(1) < inputWidth
                    inputInd(2) = 0

                    While inputInd(2) < channelCount
                        f = inputTensor(inputInd)
                        If f < 0 Then f = 0
                        writeNextLayerInput(inputInd, f)
                        inputInd(2) += 1
                    End While

                    inputInd(1) += 1
                End While

                inputInd(0) += 1
            End While

            disposeInputTensor()

            Return Me
        End Function
    End Class
End Namespace


Namespace Convolutional
    Friend Class ReLU : Inherits Layer

        Public Sub New(inputTensorDims As Integer())
            MyBase.New(inputTensorDims)
            type = "ReLU"
        End Sub

        Public Overrides Sub feedNext()
            outputTensorMemAlloc()
            Dim inputHeight = InputTensorDims(0)
            Dim inputWidth = InputTensorDims(1)
            Dim channelCount = InputTensorDims(2)
            Dim f As Single
            Dim inputInd = New Integer() {0, 0, 0}
            inputInd(0) = 0

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
        End Sub
    End Class
End Namespace

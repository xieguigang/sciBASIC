Imports System
Imports stdNum = System.Math

Namespace Convolutional
    Public Class Pool : Inherits Layer

        Public pool As Integer()
        Public stride As Integer()

        Public Sub New(inputTensorDims As Integer(), pad As Integer())
            MyBase.New(inputTensorDims, pad)
            type = "Pool"
            pool = New Integer(1) {}
            stride = New Integer(1) {}
        End Sub

        Public Overloads Sub setOutputDims()
            outputDims = New Integer(2) {
                CInt(stdNum.Floor(InputTensorDims(0) / stride(0))),
                CInt(stdNum.Floor(InputTensorDims(1) / stride(1))),
                InputTensorDims(2)
            }
        End Sub

        Public Overrides Sub feedNext()
            outputTensorMemAlloc()
            Dim inputHeight = InputTensorDims(0)
            Dim inputWidth = InputTensorDims(1)
            Dim channelCount = InputTensorDims(2)
            Dim poolHeight = pool(0)
            Dim poolWidth = pool(1)
            Dim inputInd = New Integer() {0, 0, 0}
            Dim outputInd = New Integer() {0, 0, 0}
            Dim max As Single
            inputInd(2) = 0

            While inputInd(2) < channelCount
                outputInd(2) = inputInd(2)
                Dim i = 0

                While i <= inputHeight - poolHeight
                    outputInd(0) = CInt(stdNum.Floor(i / stride(0)))
                    Dim j = 0

                    While j <= inputWidth - poolWidth
                        outputInd(1) = CInt(stdNum.Floor(j / stride(1)))
                        max = Single.MinValue
                        inputInd(0) = i

                        While inputInd(0) < i + poolHeight
                            inputInd(1) = j

                            While inputInd(1) < j + poolWidth
                                Dim f = inputTensor(inputInd)
                                If f > max Then max = f
                                inputInd(1) += 1
                            End While

                            inputInd(0) += 1
                        End While

                        writeNextLayerInput(outputInd, max)
                        j += stride(1)
                    End While

                    i += stride(0)
                End While

                inputInd(2) += 1
            End While

            disposeInputTensor()
        End Sub
    End Class
End Namespace

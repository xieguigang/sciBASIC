Imports System
Imports stdNum = System.Math

Namespace Convolutional
    Public Class SoftMax : Inherits Layer

        Public Sub New(inputTensorDims As Integer())
            MyBase.New(inputTensorDims)
            type = "SoftMax"
        End Sub

        Public Overrides Sub feedNext()
            outputTensorMemAlloc()
            Dim max = Single.MinValue

            For i = 0 To inputTensor.TotalLength - 1
                If inputTensor.memPtr(i) > max Then max = inputTensor.memPtr(i)
            Next

            Dim sum As Single = 0
            Dim nLMR = MyBase.nextLayer.inputTensor.memPtr

            For i = 0 To inputTensor.TotalLength - 1
                nLMR(i) = CSng(stdNum.Exp(inputTensor.memPtr(i) - max))
                sum += nLMR(i)
            Next

            For i = 0 To inputTensor.TotalLength - 1
                nLMR(i) /= sum
            Next

            disposeInputTensor()
        End Sub
    End Class
End Namespace

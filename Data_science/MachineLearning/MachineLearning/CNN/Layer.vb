
Namespace Convolutional
    Public MustInherit Class Layer

        Public type As String
        Private inputTensorDimsField As Integer()

        Public ReadOnly Property InputTensorDims As Integer()
            Get
                Return inputTensorDimsField
            End Get
        End Property

        Public paddedWriting As Boolean
        Public pad As Integer()
        Public inputTensor As Tensor = Nothing
        Public nextLayer As Layer

        Public Sub writeNextLayerInput(indexes As Integer(), value As Single)
            If nextLayer.paddedWriting Then
                Dim nInd As Integer() = CType(indexes.Clone(), Integer())
                nInd(0) += nextLayer.pad(0)
                nInd(1) += nextLayer.pad(2)
                nextLayer.inputTensor(nInd) = value
            Else
                nextLayer.inputTensor(indexes) = value
            End If
        End Sub

        Public Sub New(inputTensorDims As Integer())
            paddedWriting = False
            inputTensorDimsField = CType(inputTensorDims.Clone(), Integer())
        End Sub

        Public Sub New(inputTensorDims As Integer(), pad As Integer())
            Me.pad = CType(pad.Clone(), Integer())

            If pad(0) > 0 OrElse pad(2) > 0 Then
                paddedWriting = True
            Else
                paddedWriting = False
            End If

            inputTensorDimsField = CType(inputTensorDims.Clone(), Integer())
            inputTensorDimsField(0) += pad(0) + pad(1)
            inputTensorDimsField(1) += pad(2) + pad(3)
        End Sub

        Public outputDims As Integer()

        Public Sub setOutputDims()
            outputDims = CType(inputTensorDimsField.Clone(), Integer())
        End Sub

        Public MustOverride Sub feedNext()

        Public Sub inputTensorMemAlloc()
            inputTensor = New Tensor(inputTensorDimsField)
        End Sub

        Public Sub outputTensorMemAlloc()
            nextLayer.inputTensorMemAlloc()
        End Sub

        Public Sub disposeInputTensor()
            inputTensor.Dispose()
            inputTensor = Nothing
        End Sub

        Public Sub appendNext(nextLayer As Layer)
            Me.nextLayer = nextLayer
        End Sub
    End Class
End Namespace


Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Convolutional

    Public MustInherit Class Layer

        Public MustOverride ReadOnly Property type As LayerTypes

        Public ReadOnly Property inputTensorDims As Integer()

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
            _inputTensorDims = CType(inputTensorDims.Clone(), Integer())
        End Sub

        Public Sub New(inputTensorDims As Integer(), pad As Integer())
            Me.pad = CType(pad.Clone(), Integer())

            If pad(0) > 0 OrElse pad(2) > 0 Then
                paddedWriting = True
            Else
                paddedWriting = False
            End If

            _inputTensorDims = CType(inputTensorDims.Clone(), Integer())
            _inputTensorDims(0) += pad(0) + pad(1)
            _inputTensorDims(1) += pad(2) + pad(3)
        End Sub

        Public outputDims As Integer()

        Public Sub setOutputDims()
            outputDims = CType(_inputTensorDims.Clone(), Integer())
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns>
        ''' this function should be returns itself
        ''' </returns>
        Public MustOverride Function feedNext() As Layer

        Public Sub inputTensorMemAlloc()
            inputTensor = New Tensor(_inputTensorDims)
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

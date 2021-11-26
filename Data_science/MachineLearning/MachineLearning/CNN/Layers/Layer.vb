
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Convolutional

    Public MustInherit Class Layer : Implements IDisposable

        Public MustOverride ReadOnly Property type As LayerTypes

        Public ReadOnly Property inputTensorDims As Integer()

        Protected _paddedWriting As Boolean

        Public pad As Integer()
        Public inputTensor As Tensor = Nothing
        Public nextLayer As Layer

        Public Sub writeNextLayerInput(indexes As Integer(), value As Single)
            If nextLayer._paddedWriting Then
                Dim nInd As Integer() = CType(indexes.Clone(), Integer())
                nInd(0) += nextLayer.pad(0)
                nInd(1) += nextLayer.pad(2)
                nextLayer.inputTensor(nInd) = value
            Else
                nextLayer.inputTensor(indexes) = value
            End If
        End Sub

        Public Sub New(inputTensorDims As Integer())
            _paddedWriting = False
            _inputTensorDims = CType(inputTensorDims.Clone(), Integer())
        End Sub

        Public Sub New(inputTensorDims As Integer(), pad As Integer())
            Me.pad = CType(pad.Clone(), Integer())

            If pad(0) > 0 OrElse pad(2) > 0 Then
                _paddedWriting = True
            Else
                _paddedWriting = False
            End If

            _inputTensorDims = CType(inputTensorDims.Clone(), Integer())
            _inputTensorDims(0) += pad(0) + pad(1)
            _inputTensorDims(1) += pad(2) + pad(3)
        End Sub

        Public outputDims As Integer()
        Private disposedValue As Boolean

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub setOutputDims()
            outputDims = CType(_inputTensorDims.Clone(), Integer())
        End Sub

        ''' <summary>
        ''' <see cref="outputTensorMemAlloc"/> has been called in 
        ''' caller function <see cref="feedNext"/>.
        ''' </summary>
        ''' <returns>
        ''' this function should be returns itself
        ''' </returns>
        Protected MustOverride Function layerFeedNext() As Layer

        Public Overridable Function feedNext() As Layer
            Call outputTensorMemAlloc()
            Return layerFeedNext()
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub inputTensorMemAlloc()
            inputTensor = New Tensor(_inputTensorDims)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub outputTensorMemAlloc()
            nextLayer.inputTensorMemAlloc()
        End Sub

        ''' <summary>
        ''' release the tensor memory
        ''' </summary>
        Protected Sub disposeInputTensor()
            inputTensor.Dispose()
            inputTensor = Nothing
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub appendNext(nextLayer As Layer)
            Me.nextLayer = nextLayer
        End Sub

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)
                    If Not inputTensor Is Nothing Then
                        Call disposeInputTensor()
                    End If
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
                ' TODO: set large fields to null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
        ' Protected Overrides Sub Finalize()
        '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace

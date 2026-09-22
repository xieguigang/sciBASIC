#Region "Microsoft.VisualBasic::06268d1aa644d11bb13da8898f484cae, Data_science\MachineLearning\DeepLearning\CeNiN\Layers\Layer.vb"

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

    '   Total Lines: 179
    '    Code Lines: 81 (45.25%)
    ' Comment Lines: 74 (41.34%)
    '    - Xml Docs: 86.49%
    ' 
    '   Blank Lines: 24 (13.41%)
    '     File Size: 8.13 KB


    '     Class Layer
    ' 
    '         Properties: inputTensorDims
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: feedNext, ToString
    ' 
    '         Sub: appendNext, (+2 Overloads) Dispose, disposeInputTensor, inputTensorMemAlloc, outputTensorMemAlloc
    '              setOutputDims, writeNextLayerInput
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

Namespace Convolutional

    ''' <summary>
    ''' Base class of all layers in a CeNiN feed-forward network.
    ''' </summary>
    ''' <remarks>
    ''' Layers form a singly linked chain: every layer keeps a reference to the <see cref="nextLayer"/> it
    ''' feeds into, writes its activations directly into that layer's input tensor and then releases its
    ''' own input tensor, so the memory footprint of one forward pass stays bounded regardless of depth.
    ''' </remarks>
    Public MustInherit Class Layer : Implements IDisposable

        ''' <summary>Gets the kind of this layer, used for serialization and diagnostics.</summary>
        Public MustOverride ReadOnly Property type As CNN.LayerTypes

        ''' <summary>Gets the dimensions <c>[height, width, depth]</c> of this layer's input tensor.</summary>
        Public ReadOnly Property inputTensorDims As Integer()

        ''' <summary>
        ''' Indicates whether writes into the next layer's input tensor must be shifted by this layer's padding.
        ''' </summary>
        Protected _paddedWriting As Boolean

        ''' <summary>The padding <c>[top, bottom, left, right]</c> applied to the input borders.</summary>
        Public pad As Integer()
        ''' <summary>The input activation tensor of this layer.</summary>
        Public inputTensor As Tensor = Nothing
        ''' <summary>The layer that receives the output of this layer.</summary>
        Public nextLayer As Layer

        ''' <summary>
        ''' Writes a single activation value into the input tensor of the next layer.
        ''' </summary>
        ''' <param name="indexes">The <c>[height, width, depth]</c> coordinate of the value in this layer's output.</param>
        ''' <param name="value">The activation value to store.</param>
        ''' <remarks>
        ''' When the next layer expects padded input the coordinates are shifted by that layer's padding so
        ''' that the value lands at the correct padded position.
        ''' </remarks>
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

        ''' <summary>
        ''' Creates a layer without padding.
        ''' </summary>
        ''' <param name="inputTensorDims">The dimensions <c>[height, width, depth]</c> of the layer input.</param>
        Public Sub New(inputTensorDims As Integer())
            _paddedWriting = False
            _inputTensorDims = CType(inputTensorDims.Clone(), Integer())
        End Sub

        ''' <summary>
        ''' Creates a layer whose input tensor is enlarged by the given padding.
        ''' </summary>
        ''' <param name="inputTensorDims">The unpadded dimensions <c>[height, width, depth]</c> of the layer input.</param>
        ''' <param name="pad">The padding <c>[top, bottom, left, right]</c> added to the input borders.</param>
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

        ''' <summary>Dimensions <c>[height, width, depth]</c> of the tensor produced by this layer.</summary>
        Public outputDims As Integer()
        Private disposedValue As Boolean

        ''' <summary>
        ''' Initializes <see cref="outputDims"/> from the input dimensions; layers that change the spatial
        ''' shape override this method.
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub setOutputDims()
            outputDims = CType(_inputTensorDims.Clone(), Integer())
        End Sub

        ''' <summary>Returns the human readable name of this layer type.</summary>
        ''' <returns>The description of the layer's <see cref="type"/>.</returns>
        Public Overrides Function ToString() As String
            Return type.Description
        End Function

        ''' <summary>
        ''' <see cref="outputTensorMemAlloc"/> has been called in 
        ''' caller function <see cref="feedNext"/>.
        ''' </summary>
        ''' <returns>
        ''' this function should be returns itself
        ''' </returns>
        Protected MustOverride Function layerFeedNext() As Layer

        ''' <summary>
        ''' Allocates the next layer's input tensor and then runs the layer specific forward computation.
        ''' </summary>
        ''' <returns>This layer instance, allowing the caller to advance along the chain.</returns>
        Public Overridable Function feedNext() As Layer
            Call outputTensorMemAlloc()
            Return layerFeedNext()
        End Function

        ''' <summary>Allocates a fresh input tensor using this layer's input dimensions.</summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub inputTensorMemAlloc()
            inputTensor = New Tensor(_inputTensorDims)
        End Sub

        ''' <summary>Allocates the next layer's input tensor so this layer has a destination to write to.</summary>
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

        ''' <summary>
        ''' Links the given layer after this one, building the forward propagation chain.
        ''' </summary>
        ''' <param name="nextLayer">The layer that will consume this layer's output.</param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub appendNext(nextLayer As Layer)
            Me.nextLayer = nextLayer
        End Sub

        ''' <summary>Releases the resources held by this layer.</summary>
        ''' <param name="disposing"><c>True</c> to release both managed and unmanaged resources.</param>
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

        ''' <summary>Releases all resources held by this layer.</summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace

#Region "Microsoft.VisualBasic::a998160be066e84e039231d24524bf90, Data_science\MachineLearning\DeepLearning\CeNiN\Layers\Layer.vb"

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

    '   Total Lines: 125
    '    Code Lines: 81
    ' Comment Lines: 20
    '   Blank Lines: 24
    '     File Size: 4.45 KB


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
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Convolutional

    Public MustInherit Class Layer : Implements IDisposable

        Public MustOverride ReadOnly Property type As CNN.LayerTypes

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

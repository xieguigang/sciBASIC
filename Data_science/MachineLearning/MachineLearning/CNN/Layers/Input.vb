Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.MachineLearning.Convolutional.ImageProcessor

Namespace Convolutional

    Public Class Input : Inherits Layer

        Public inputSize As Integer()
        Public avgPixel As Single()

        Public Overrides ReadOnly Property type As LayerTypes
            Get
                Return LayerTypes.Input
            End Get
        End Property

        Public ReadOnly Property ResizedInputBmp As Bitmap

        Public Sub New(inputTensorDims As Integer())
            MyBase.New(New Integer() {0, 0, 0})

            inputSize = CType(inputTensorDims.Clone(), Integer())
            avgPixel = New Single(2) {}
        End Sub

        Public Overloads Sub setOutputDims()
            outputDims = CType(inputSize.Clone(), Integer())
        End Sub

        Public Function setInput(input As Bitmap, resizingMethod As ResizingMethod) As Input
            outputTensorMemAlloc()
            Dim iBitmap As Bitmap = CType(input.Clone(), Bitmap)
            _ResizedInputBmp = ImageProcessor.resizeBitmap(iBitmap, resizingMethod, inputSize)
            iBitmap.Dispose()

            Return Me
        End Function

        Public Overrides Function feedNext() As Layer
            Dim bmpData As BitmapData = _ResizedInputBmp.LockBits(New Rectangle(0, 0, inputSize(1), inputSize(0)), ImageLockMode.ReadOnly, _ResizedInputBmp.PixelFormat)
            Dim stride = bmpData.Stride
            Dim emptyBytesCount = stride - bmpData.Width * 3
            Dim rowLengthWithoutEB = stride - emptyBytesCount

            Dim dataPtr = bmpData.Scan0
            Dim byteCount = stride * bmpData.Height
            Dim i = 0
            Dim pControl = 0
            Dim b, g, r As Integer
            Dim ind = New Integer() {0, 0, 0}

            While i < byteCount
                b = Marshal.ReadByte(dataPtr)
                dataPtr += 1
                g = Marshal.ReadByte(dataPtr)
                dataPtr += 1
                r = Marshal.ReadByte(dataPtr)
                dataPtr += 1
                ind(2) = 0
                writeNextLayerInput(ind, r - avgPixel(0))
                ind(2) = 1
                writeNextLayerInput(ind, g - avgPixel(1))
                ind(2) = 2
                writeNextLayerInput(ind, b - avgPixel(2))
                ind(1) += 1
                pControl += 3
                i += 3

                If pControl = rowLengthWithoutEB Then
                    pControl = 0
                    dataPtr += emptyBytesCount
                    ind(1) = 0
                    ind(0) += 1
                End If
            End While

            _ResizedInputBmp.UnlockBits(bmpData)
            _ResizedInputBmp.Dispose()

            Return Me
        End Function
    End Class
End Namespace

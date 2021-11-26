Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.MachineLearning.Convolutional.ImageProcessor

Namespace Convolutional

    ''' <summary>
    ''' the layer for image inputs
    ''' </summary>
    Public Class Input : Inherits Layer

        Public inputSize As Integer()
        Public avgPixel As Single()

        Public Overrides ReadOnly Property type As LayerTypes
            Get
                Return LayerTypes.Input
            End Get
        End Property

        Public ReadOnly Property resizedInputBmp As Bitmap

        Public Sub New(inputTensorDims As Integer())
            MyBase.New(New Integer() {0, 0, 0})

            inputSize = CType(inputTensorDims.Clone(), Integer())
            avgPixel = New Single(2) {}
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Sub setOutputDims()
            outputDims = CType(inputSize.Clone(), Integer())
        End Sub

        Public Function setInput(input As Bitmap, resizingMethod As ResizingMethod) As Input
            Using iBitmap As Bitmap = CType(input.Clone(), Bitmap)
                outputTensorMemAlloc()
                _resizedInputBmp = ImageProcessor.resizeBitmap(iBitmap, resizingMethod, inputSize)
            End Using

            Return Me
        End Function

        ''' <summary>
        ''' do nothing in the image input layer
        ''' </summary>
        ''' <returns></returns>
        Protected Overrides Function layerFeedNext() As Layer
            Return Me
        End Function

        ''' <summary>
        ''' load test bitmap image data
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function feedNext() As Layer
            Dim fullImage As New Rectangle(0, 0, inputSize(1), inputSize(0))
            Dim bmpData As BitmapData = _resizedInputBmp.LockBits(fullImage, ImageLockMode.ReadOnly, _resizedInputBmp.PixelFormat)
            Dim stride As Integer = bmpData.Stride
            Dim emptyBytesCount As Integer = stride - bmpData.Width * 3
            Dim rowLengthWithoutEB As Integer = stride - emptyBytesCount
            Dim dataPtr As IntPtr = bmpData.Scan0
            Dim byteCount As Integer = stride * bmpData.Height
            Dim i As Integer = 0
            Dim pixel As Integer = 0
            Dim b, g, r As Integer
            Dim ind As Integer() = New Integer() {0, 0, 0}

            While i < byteCount
                b = Marshal.ReadByte(dataPtr)
                dataPtr += 1
                g = Marshal.ReadByte(dataPtr)
                dataPtr += 1
                r = Marshal.ReadByte(dataPtr)
                dataPtr += 1
                ind(2) = 0
                Call writeNextLayerInput(ind, r - avgPixel(0))
                ind(2) = 1
                Call writeNextLayerInput(ind, g - avgPixel(1))
                ind(2) = 2
                Call writeNextLayerInput(ind, b - avgPixel(2))
                ind(1) += 1
                pixel += 3
                i += 3

                If pixel = rowLengthWithoutEB Then
                    pixel = 0
                    dataPtr += emptyBytesCount
                    ind(1) = 0
                    ind(0) += 1
                End If
            End While

            Call _resizedInputBmp.UnlockBits(bmpData)
            Call _resizedInputBmp.Dispose()

            Return Me
        End Function
    End Class
End Namespace

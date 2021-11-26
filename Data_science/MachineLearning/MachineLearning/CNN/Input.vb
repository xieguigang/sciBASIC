Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices

Namespace Convolutional

    Public Class Input : Inherits Layer

        Public inputSize As Integer()
        Public avgPixel As Single()

        Public Enum ResizingMethod
            Stretch
            ZeroPad
        End Enum

        Private resizedInputBmpField As Bitmap

        Public ReadOnly Property ResizedInputBmp As Bitmap
            Get
                Return resizedInputBmpField
            End Get
        End Property

        Public Sub New(inputTensorDims As Integer())
            MyBase.New(New Integer() {0, 0, 0})
            type = "Input"
            inputSize = CType(inputTensorDims.Clone(), Integer())
            avgPixel = New Single(2) {}
        End Sub

        Public Overloads Sub setOutputDims()
            outputDims = CType(inputSize.Clone(), Integer())
        End Sub

        Private Function resizeBitmap(b As Bitmap, resizingMethod As ResizingMethod) As Bitmap
            Dim resizedBmp As Bitmap = New Bitmap(inputSize(1), inputSize(0), PixelFormat.Format24bppRgb)
            Dim gr = Graphics.FromImage(resizedBmp)

            If resizingMethod = ResizingMethod.Stretch Then
                gr.DrawImage(b, 0, 0, inputSize(1), inputSize(0))
            Else
                Dim inputAspRatio As Single = CSng(inputSize(0) / inputSize(1))
                Dim newHeight, newWidth As Integer
                Dim multiplier = CSng(b.Width) / b.Height

                If multiplier > inputAspRatio Then
                    multiplier = inputAspRatio / multiplier
                    newWidth = inputSize(1)
                    newHeight = CInt(newWidth * multiplier)
                Else
                    newHeight = inputSize(0)
                    newWidth = CInt(newHeight * multiplier)
                End If

                gr.DrawImage(b, (inputSize(1) - newWidth) / 2.0F, (inputSize(0) - newHeight) / 2.0F, newWidth, newHeight)
            End If

            gr.Dispose()
            Return resizedBmp
        End Function

        Public Function setInput(input As Bitmap, resizingMethod As ResizingMethod) As Input
            outputTensorMemAlloc()
            Dim iBitmap As Bitmap = CType(input.Clone(), Bitmap)
            resizedInputBmpField = resizeBitmap(iBitmap, resizingMethod)
            iBitmap.Dispose()

            Return Me
        End Function

        Public Overrides Sub feedNext()
            Dim bmpData As BitmapData = resizedInputBmpField.LockBits(New Rectangle(0, 0, inputSize(1), inputSize(0)), ImageLockMode.ReadOnly, resizedInputBmpField.PixelFormat)
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

            resizedInputBmpField.UnlockBits(bmpData)
            resizedInputBmpField.Dispose()
        End Sub
    End Class
End Namespace

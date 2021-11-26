Imports System.Drawing
Imports System.Drawing.Imaging

Namespace Convolutional

    Public Class ImageProcessor

        Public Enum ResizingMethod
            Stretch
            ZeroPad
        End Enum

        Private Shared Function Stretch(b As Bitmap, inputSize As Integer()) As Bitmap
            Dim resizedBmp As New Bitmap(inputSize(1), inputSize(0), PixelFormat.Format24bppRgb)

            Using gr As Graphics = Graphics.FromImage(resizedBmp)
                Call gr.DrawImage(b, 0, 0, inputSize(1), inputSize(0))
            End Using

            Return resizedBmp
        End Function

        Private Shared Function ZeroPad(b As Bitmap, inputSize As Integer())
            Dim resizedBmp As New Bitmap(inputSize(1), inputSize(0), PixelFormat.Format24bppRgb)

            Using gr As Graphics = Graphics.FromImage(resizedBmp)
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
            End Using

            Return resizedBmp
        End Function

        Friend Shared Function resizeBitmap(b As Bitmap, resizingMethod As ResizingMethod, inputSize As Integer()) As Bitmap
            If resizingMethod = ResizingMethod.Stretch Then
                Return Stretch(b, inputSize)
            Else
                Return ZeroPad(b, inputSize)
            End If
        End Function
    End Class
End Namespace
#Region "Microsoft.VisualBasic::815a2d06e1bb288c89857fb591c65d92, Data_science\MachineLearning\DeepLearning\CeNiN\Layers\Input.vb"

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

    '   Total Lines: 128
    '    Code Lines: 99 (77.34%)
    ' Comment Lines: 11 (8.59%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 18 (14.06%)
    '     File Size: 4.67 KB


    '     Class Input
    ' 
    '         Properties: resizedInputBmp, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: feedNext, layerFeedNext, setInput
    ' 
    '         Sub: setOutputDims
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.MachineLearning.Convolutional.ImageProcessor
Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.BitmapImage


#If NET48 Then
Imports Pen = System.Drawing.Pen
Imports Pens = System.Drawing.Pens
Imports Brush = System.Drawing.Brush
Imports Font = System.Drawing.Font
Imports Brushes = System.Drawing.Brushes
Imports SolidBrush = System.Drawing.SolidBrush
Imports DashStyle = System.Drawing.Drawing2D.DashStyle
Imports Image = System.Drawing.Image
Imports Bitmap = System.Drawing.Bitmap
Imports GraphicsPath = System.Drawing.Drawing2D.GraphicsPath
Imports FontStyle = System.Drawing.FontStyle
#Else
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports Pens = Microsoft.VisualBasic.Imaging.Pens
Imports Brush = Microsoft.VisualBasic.Imaging.Brush
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Brushes = Microsoft.VisualBasic.Imaging.Brushes
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports DashStyle = Microsoft.VisualBasic.Imaging.DashStyle
Imports Image = Microsoft.VisualBasic.Imaging.Image
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
Imports GraphicsPath = Microsoft.VisualBasic.Imaging.GraphicsPath
Imports FontStyle = Microsoft.VisualBasic.Imaging.FontStyle
#End If

Namespace Convolutional

    ''' <summary>
    ''' the layer for image inputs
    ''' </summary>
    Public Class Input : Inherits Layer

        ''' <summary>The required image size <c>[height, width, depth]</c> of this input layer.</summary>
        Public inputSize As Integer()
        ''' <summary>Per channel mean pixel values subtracted from the raw image before feeding it forward.</summary>
        Public avgPixel As Single()

        ''' <summary>Gets the layer kind, always <see cref="CNN.LayerTypes.Input"/>.</summary>
        Public Overrides ReadOnly Property type As CNN.LayerTypes
            Get
                Return CNN.LayerTypes.Input
            End Get
        End Property

        ''' <summary>Gets the source image after it has been resized to <see cref="inputSize"/>.</summary>
        Public ReadOnly Property resizedInputBmp As Bitmap

        ''' <summary>
        ''' Creates the image input layer for the given image size.
        ''' </summary>
        ''' <param name="inputTensorDims">The required image size <c>[height, width, depth]</c>.</param>
        Public Sub New(inputTensorDims As Integer())
            MyBase.New(New Integer() {0, 0, 0})

            inputSize = CType(inputTensorDims.Clone(), Integer())
            avgPixel = New Single(2) {}
        End Sub

        ''' <summary>Sets the output dimensions of this layer to the configured input image size.</summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Sub setOutputDims()
            outputDims = CType(inputSize.Clone(), Integer())
        End Sub

        ''' <summary>
        ''' Resizes the given bitmap to the network input size and prepares it as the source of the next
        ''' forward pass.
        ''' </summary>
        ''' <param name="input">The source image to classify.</param>
        ''' <param name="resizingMethod">The strategy used to fit the image into <see cref="inputSize"/>.</param>
        ''' <returns>This input layer, allowing the caller to continue the chain.</returns>
        Public Function setInput(input As Bitmap, resizingMethod As ResizingMethod) As Input
            Using iBitmap As Bitmap = CType(input.Clone(), Bitmap)
                outputTensorMemAlloc()
                _resizedInputBmp = ImageProcessor.resizeBitmap(iBitmap, resizingMethod, inputSize)
            End Using

            Return Me
        End Function

        ''' <summary>
        ''' Performs no computation; the input layer only holds the resized image.
        ''' </summary>
        ''' <returns>This layer instance.</returns>
        Protected Overrides Function layerFeedNext() As Layer
            Return Me
        End Function

        ''' <summary>
        ''' Loads the resized bitmap pixel data into the input tensor, subtracting the per channel mean values.
        ''' </summary>
        ''' <returns>This layer instance once the pixel data has been loaded.</returns>
        Public Overrides Function feedNext() As Layer
            Dim fullImage As New Rectangle(0, 0, inputSize(1), inputSize(0))
            Dim bmpData As BitmapBuffer = BitmapBuffer.FromBitmap(_resizedInputBmp)
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

            Call _resizedInputBmp.Dispose()

            Return Me
        End Function
    End Class
End Namespace

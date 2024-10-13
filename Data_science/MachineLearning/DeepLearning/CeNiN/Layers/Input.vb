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

        Public inputSize As Integer()
        Public avgPixel As Single()

        Public Overrides ReadOnly Property type As CNN.LayerTypes
            Get
                Return CNN.LayerTypes.Input
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

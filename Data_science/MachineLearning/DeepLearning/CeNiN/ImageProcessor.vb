#Region "Microsoft.VisualBasic::ed3ec5a89341044a5e5bde3ac01c923a, Data_science\MachineLearning\DeepLearning\CeNiN\ImageProcessor.vb"

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

    '   Total Lines: 78
    '    Code Lines: 49 (62.82%)
    ' Comment Lines: 14 (17.95%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 15 (19.23%)
    '     File Size: 3.06 KB


    '     Class ImageProcessor
    ' 
    ' 
    '         Enum ResizingMethod
    ' 
    '             Stretch, ZeroPad
    ' 
    ' 
    ' 
    '  
    ' 
    '     Function: resizeBitmap, Stretch, ZeroPad
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Driver

#If NET48 Then
Imports Bitmap = System.Drawing.Bitmap
Imports PixelFormat = System.Drawing.Imaging.PixelFormat
#Else
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
Imports PixelFormat = Microsoft.VisualBasic.Imaging.PixelFormat
#End If

Namespace Convolutional

    ''' <summary>
    ''' Provides the bitmap resizing strategies used to fit an arbitrary input image into the fixed
    ''' spatial size expected by the network's input layer.
    ''' </summary>
    Public Class ImageProcessor

        ''' <summary>
        ''' Specifies how a source bitmap is mapped onto the network input size.
        ''' </summary>
        Public Enum ResizingMethod

            ''' <summary>
            ''' Rescale the bitmap ignoring the original aspect ratio so that it exactly fills the input size.
            ''' </summary>
            Stretch

            ''' <summary>
            ''' Rescale the bitmap preserving the original aspect ratio and center it on a zero padded canvas
            ''' of the input size.
            ''' </summary>
            ZeroPad
        End Enum

        Private Shared Function Stretch(b As Bitmap, inputSize As Integer()) As Bitmap
            Dim resizedBmp As New Bitmap(inputSize(1), inputSize(0), PixelFormat.Format24bppRgb)

            Using g As IGraphics = DriverLoad.CreateGraphicsDevice(resizedBmp, driver:=Drivers.GDI)
                Call g.DrawImage(b, 0, 0, inputSize(1), inputSize(0))
            End Using

            Return resizedBmp
        End Function

        Private Shared Function ZeroPad(b As Bitmap, inputSize As Integer())
            Dim resizedBmp As New Bitmap(inputSize(1), inputSize(0), PixelFormat.Format24bppRgb)

            Using g As IGraphics = DriverLoad.CreateGraphicsDevice(resizedBmp, driver:=Drivers.GDI)
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

                g.DrawImage(b, (inputSize(1) - newWidth) / 2.0F, (inputSize(0) - newHeight) / 2.0F, newWidth, newHeight)
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

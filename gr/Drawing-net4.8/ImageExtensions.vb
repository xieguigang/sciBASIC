#Region "Microsoft.VisualBasic::8c89ee0e0f5c030b6c53313d8ae432c6, gr\Drawing-net4.8\ImageExtensions.vb"

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

    '   Total Lines: 40
    '    Code Lines: 24 (60.00%)
    ' Comment Lines: 11 (27.50%)
    '    - Xml Docs: 81.82%
    ' 
    '   Blank Lines: 5 (12.50%)
    '     File Size: 1.78 KB


    ' Module ImageExtensions
    ' 
    '     Sub: Adjust
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Imaging

Public Module ImageExtensions

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="originalImage"></param>
    ''' <param name="brightness"></param>
    ''' <param name="contrast"></param>
    ''' <param name="gamma"></param>
    ''' <remarks>
    ''' 1 means no changed
    ''' </remarks>
    Public Sub Adjust(ByRef originalImage As Bitmap, Optional brightness As Single = 1, Optional contrast As Single = 1, Optional gamma As Single = 1)
        Dim size As New Size(originalImage.Width, originalImage.Height)
        Dim adjustedImage As New Bitmap(size.Width, size.Height)
        Dim adjustedBrightness As Single = brightness - 1.0F
        ' create matrix that will brighten and contrast the image
        Dim ptsArray()() As Single = {
            New Single() {contrast, 0, 0, 0, 0}, ' scale red
            New Single() {0, contrast, 0, 0, 0}, ' scale green
            New Single() {0, 0, contrast, 0, 0}, ' scale blue
            New Single() {0, 0, 0, 1.0F, 0}, ' don't scale alpha
            New Single() {adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1}
        }

        Dim imageAttributes As New ImageAttributes()
        imageAttributes.ClearColorMatrix()
        imageAttributes.SetColorMatrix(New ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap)
        imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap)

        Using g As Graphics = Graphics.FromImage(adjustedImage)
            Call g.DrawImage(originalImage, New Rectangle(0, 0, size.Width, size.Height), 0, 0, size.Width, size.Height, GraphicsUnit.Pixel, imageAttributes)
        End Using

        originalImage = adjustedImage
    End Sub
End Module

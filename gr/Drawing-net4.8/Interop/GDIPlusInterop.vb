#Region "Microsoft.VisualBasic::b9b9ad573e388ec890898082869ac49b, gr\Drawing-net4.8\Interop\GDIPlusInterop.vb"

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

    '   Total Lines: 44
    '    Code Lines: 36 (81.82%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (18.18%)
    '     File Size: 1.95 KB


    ' Module GDIPlusInterop
    ' 
    '     Function: CastFontStyle, (+2 Overloads) CTypeFromGdiImage, (+3 Overloads) CTypeGdiImage
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Driver

Public Module GDIPlusInterop

    <Extension>
    Public Function CTypeGdiImage(gfx As GdiRasterGraphics) As System.Drawing.Image
        Return gfx.ImageResource.CTypeGdiImage
    End Function

    <Extension>
    Public Function CTypeGdiImage(image As Microsoft.VisualBasic.Imaging.Image) As System.Drawing.Image
        Return Interop.GDIPlusImage.CTypeGDIPlusImage(image)
    End Function

    <Extension>
    Public Function CTypeGdiImage(bitmap As Microsoft.VisualBasic.Imaging.Bitmap) As System.Drawing.Bitmap
        Return Interop.GDIPlusImage.CTypeGDIPlusImage(bitmap)
    End Function

    <Extension>
    Public Function CTypeFromGdiImage(image As System.Drawing.Image) As Microsoft.VisualBasic.Imaging.Image
        Return New Interop.GDIPlusImage(image)
    End Function

    <Extension>
    Public Function CTypeFromGdiImage(bitmap As System.Drawing.Bitmap) As Microsoft.VisualBasic.Imaging.Bitmap
        Return New Microsoft.VisualBasic.Imaging.Bitmap(bitmap.CreateBuffer)
    End Function

    <Extension>
    Public Function CastFontStyle(style As Microsoft.VisualBasic.Imaging.FontStyle) As System.Drawing.FontStyle
        Select Case style
            Case Microsoft.VisualBasic.Imaging.FontStyle.Regular : Return System.Drawing.FontStyle.Regular
            Case Microsoft.VisualBasic.Imaging.FontStyle.Bold : Return System.Drawing.FontStyle.Bold
            Case Microsoft.VisualBasic.Imaging.FontStyle.Italic : Return System.Drawing.FontStyle.Italic
            Case Microsoft.VisualBasic.Imaging.FontStyle.Underline : Return System.Drawing.FontStyle.Underline
            Case Microsoft.VisualBasic.Imaging.FontStyle.Strikeout : Return System.Drawing.FontStyle.Strikeout

            Case Else
                Throw New NotImplementedException
        End Select
    End Function
End Module

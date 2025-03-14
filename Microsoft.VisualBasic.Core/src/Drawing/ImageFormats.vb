#Region "Microsoft.VisualBasic::084d32e4d441911c934ff6c225faafb9, Microsoft.VisualBasic.Core\src\Drawing\ImageFormats.vb"

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

    '   Total Lines: 66
    '    Code Lines: 21 (31.82%)
    ' Comment Lines: 39 (59.09%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (9.09%)
    '     File Size: 1.92 KB


    '     Enum ImageFormats
    ' 
    '         Base64, Exif, MemoryBmp
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel

Namespace Imaging

    ''' <summary>
    ''' Specifies the file format of the image.
    ''' </summary>
    Public Enum ImageFormats As Integer

        Unknown = 0

        ''' <summary>
        ''' Gets the bitmap (BMP) image format.
        ''' </summary>
        <Description("bmp")> Bmp
        ''' <summary>
        ''' Gets the enhanced metafile (EMF) image format.
        ''' </summary>
        <Description("emf")> Emf
        ''' <summary>
        ''' Gets the Exchangeable Image File (Exif) format.
        ''' </summary>
        Exif
        ''' <summary>
        ''' Gets the Graphics Interchange Format (GIF) image format.
        ''' </summary>
        <Description("gif")> Gif
        ''' <summary>
        ''' Gets the Windows icon image format.
        ''' </summary>
        <Description("ico")> Icon
        ''' <summary>
        ''' Gets the Joint Photographic Experts Group (JPEG) image format.
        ''' </summary>
        <Description("jpg")> Jpeg
        ''' <summary>
        ''' Gets the format of a bitmap in memory.
        ''' </summary>
        MemoryBmp
        ''' <summary>
        ''' Gets the W3C Portable Network Graphics (PNG) image format.
        ''' </summary>
        <Description("png")> Png
        ''' <summary>
        ''' Gets the Tagged Image File Format (TIFF) image format.
        ''' </summary>
        <Description("tif")> Tiff
        ''' <summary>
        ''' Gets the Windows metafile (WMF) image format.
        ''' </summary>
        <Description("wmf")> Wmf
        ''' <summary>
        ''' Base64
        ''' </summary>
        Base64
        <Description("webp")> Webp

        <Description("svg")> Svg

        <Description("pdf")> Pdf
        ''' <summary>
        ''' postscript
        ''' </summary>
        <Description("ps")> PS
    End Enum
End Namespace

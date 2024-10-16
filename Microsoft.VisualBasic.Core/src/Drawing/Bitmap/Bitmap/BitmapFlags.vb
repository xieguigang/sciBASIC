#Region "Microsoft.VisualBasic::f42c1f4cbdd2f9f78269dd1307071ad5, Microsoft.VisualBasic.Core\src\Drawing\Bitmap\Bitmap\BitmapFlags.vb"

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

    '   Total Lines: 34
    '    Code Lines: 27 (79.41%)
    ' Comment Lines: 3 (8.82%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (11.76%)
    '     File Size: 754 B


    '     Enum BitsPerPixelEnum
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum BytesPerPixelEnum
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum CompressionMethod
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Imaging.BitmapImage.FileStream

    Public Enum BitsPerPixelEnum As Integer
        Monochrome = 1
        Four = 4
        Eight = 8
        RBG16 = 16
        RGB24 = 24
        RGBA32 = 32
    End Enum

    ''' <summary>
    ''' Number of bytes for specific Pixel format.
    ''' </summary>
    Public Enum BytesPerPixelEnum As Integer
        RBG16 = 2
        RGB24 = 3
        RGBA32 = 4
    End Enum

    Public Enum CompressionMethod As Integer
        BI_RGB = 0 ' none
        BI_RLE8 = 1
        BI_RLE4 = 2
        BI_BITFIELDS = 3
        BI_JPEG = 4
        BI_PNG = 5
        BI_ALPHABITFIELDS = 6
        BI_CMYK = 11
        BI_CMYKRLE8 = 12
        BI_CMYKRLE4 = 13
    End Enum

End Namespace

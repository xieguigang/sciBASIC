#Region "Microsoft.VisualBasic::ce14f4e9962b3fc006de85f8f7071939, Microsoft.VisualBasic.Core\src\Drawing\GDI+\Driver\Drivers.vb"

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

    '   Total Lines: 54
    '    Code Lines: 28 (51.85%)
    ' Comment Lines: 21 (38.89%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (9.26%)
    '     File Size: 1.73 KB


    '     Enum Drivers
    ' 
    '         GDI, PDF, PS, SVG, WMF
    ' 
    '  
    ' 
    ' 
    ' 
    '     Module FormatHelper
    ' 
    '         Function: ConvertFormat
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Imaging.Driver

    ''' <summary>
    ''' The imaging graphics engine list for <see cref="IGraphics"/>
    ''' </summary>
    Public Enum Drivers As Byte

        ''' <summary>
        ''' 与具体上下文相关的。当用户从命令行参数之中设置了环境变量之后，这个Default的含义与用户所设置的驱动程序类型一致，但是会被程序开发人员所设置的类型值所覆盖
        ''' </summary>
        [Default] = 0
        ''' <summary>
        ''' libgdi+ raster image model
        ''' </summary>
        GDI
        ''' <summary>
        ''' mime type: image/svg+xml
        ''' </summary>
        SVG
        ''' <summary>
        ''' application/postscript
        ''' </summary>
        PS
        ''' <summary>
        ''' Windows meta file
        ''' </summary>
        WMF
        ''' <summary>
        ''' application/pdf
        ''' </summary>
        PDF
        ''' <summary>
        ''' The PostScript file format is a page description language (PDL) developed by Adobe Systems, Inc. It was first 
        ''' introduced in 1982 and has since become a standard for printing and imaging in the graphics and publishing 
        ''' industries. PostScript is a powerful and flexible format used primarily for printing high-quality text and
        ''' graphics.
        ''' 
        ''' Here are some key aspects of the PostScript file format:
        ''' 
        ''' 1. **Language-Based Format**: PostScript is not just a file format but a full-fledged programming language. 
        '''    It includes a set of commands that describe how text, graphics, and images should be rendered on a page.
        ''' 2. **Device Independence**: One of the main advantages of PostScript is its device independence. This means
        '''    that a PostScript file can be created on one device and then printed or displayed on another device without 
        '''    needing to adjust the file for the specific characteristics of the output device.
        ''' 3. **Vector Graphics**: PostScript is particularly well-suited for vector graphics, which are defined by 
        '''    mathematical equations rather than pixels. This allows for crisp, high-resolution output at any size.
        ''' 4. **Encapsulated PostScript (EPS)**: A variant of PostScript, EPS is a file format that can be used to exchange 
        '''    graphics between different applications. EPS files can contain both vector and bitmap graphics and are often
        '''    used for logos, illustrations, and other graphics that need to be integrated into larger documents.
        ''' 5. **Font Handling**: PostScript introduced sophisticated font handling, including the ability to scale and rotate 
        '''    text. It also supports both Type 1 and Type 3 fonts, which are outline fonts that can be scaled to any size
        '''    without losing quality.
        ''' 6. **Complex Page Layouts**: PostScript is capable of describing very complex page layouts, including text in 
        '''    multiple columns, overlapping graphics, and nested objects.
        ''' 7. **Interpreted Language**: PostScript files are interpreted by a PostScript interpreter, which is typically 
        '''    built into a printer or a raster image processor (RIP). The interpreter reads the PostScript commands and
        '''    renders the page accordingly.
        ''' 8. **Compatibility**: While PostScript was once the dominant format for professional printing, its use has declined 
        '''    somewhat with the rise of PDF (Portable Document Format), also developed by Adobe. However, PostScript remains 
        '''    important in certain niches, and many printers and RIPs still support it.
        ''' 9. **File Extensions**: PostScript files commonly have the extensions .ps (for PostScript) or .eps (for Encapsulated PostScript).
        ''' 10. **Programming Features**: PostScript includes programming constructs such as variables, loops, and conditionals, 
        '''     which allow for a high degree of automation and customization in the creation of documents.
        '''     
        ''' Despite the rise of other formats like PDF, PostScript still has a place in the world of digital printing and publishing, 
        ''' particularly for applications where precise control over the output is required. It's also worth noting that many of the
        ''' concepts and technologies introduced with PostScript laid the groundwork for later developments in digital graphics and 
        ''' document management.
        ''' </summary>
        PostScript
    End Enum

    Public Module FormatHelper

        <Extension>
        Public Function ConvertFormat(format As ImageFormats) As Drivers
            Select Case format
                Case ImageFormats.Bmp, ImageFormats.Gif, ImageFormats.Jpeg, ImageFormats.Png, ImageFormats.Tiff
                    Return Drivers.GDI
                Case ImageFormats.Wmf
                    Return Drivers.WMF
                Case ImageFormats.Svg
                    Return Drivers.SVG
                Case ImageFormats.Pdf
                    Return Drivers.PDF
                Case Else
                    Return DriverLoad.DefaultGraphicsDevice
            End Select
        End Function
    End Module
End Namespace

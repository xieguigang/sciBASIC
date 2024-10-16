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

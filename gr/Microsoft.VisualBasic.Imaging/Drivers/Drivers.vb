﻿#Region "Microsoft.VisualBasic::3bc274bd8f02fc291527e37636b58a79, sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drivers\Drivers.vb"

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

    '   Total Lines: 33
    '    Code Lines: 12
    ' Comment Lines: 18
    '   Blank Lines: 3
    '     File Size: 1014 B


    '     Enum Drivers
    ' 
    '         [Default], GDI, PDF, PS, SVG
    '         WMF
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.PostScript
Imports Microsoft.VisualBasic.Imaging.SVG

Namespace Driver

    ''' <summary>
    ''' The imaging graphics engine list for <see cref="IGraphics"/>
    ''' </summary>
    Public Enum Drivers As Byte

        ''' <summary>
        ''' 与具体上下文相关的。当用户从命令行参数之中设置了环境变量之后，这个Default的含义与用户所设置的驱动程序类型一致，但是会被程序开发人员所设置的类型值所覆盖
        ''' </summary>
        [Default]
        ''' <summary>
        ''' <see cref="Graphics2D"/>
        ''' </summary>
        GDI
        ''' <summary>
        ''' <see cref="GraphicsSVG"/>, mime type: image/svg+xml
        ''' </summary>
        SVG
        ''' <summary>
        ''' <see cref="GraphicsPS"/>
        ''' </summary>
        PS
        ''' <summary>
        ''' Windows meta file: <see cref="Imaging.Wmf"/>
        ''' </summary>
        WMF
        PDF
    End Enum
End Namespace

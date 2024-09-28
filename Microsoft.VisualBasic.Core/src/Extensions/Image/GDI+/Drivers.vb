#Region "Microsoft.VisualBasic::598b582dd23ff10b42654dbcb2c0fcc7, gr\Microsoft.VisualBasic.Imaging\Drivers\Drivers.vb"

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
'    Code Lines: 12 (36.36%)
' Comment Lines: 18 (54.55%)
'    - Xml Docs: 100.00%
' 
'   Blank Lines: 3 (9.09%)
'     File Size: 1.04 KB


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

Namespace Imaging.Driver

    ''' <summary>
    ''' The imaging graphics engine list for <see cref="IGraphics"/>
    ''' </summary>
    Public Enum Drivers As Byte

        ''' <summary>
        ''' 与具体上下文相关的。当用户从命令行参数之中设置了环境变量之后，这个Default的含义与用户所设置的驱动程序类型一致，但是会被程序开发人员所设置的类型值所覆盖
        ''' </summary>
        [Default]
        ''' <summary>
        ''' libgdi+ raster image model
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
        ''' Windows meta file
        ''' </summary>
        WMF
        PDF
    End Enum
End Namespace

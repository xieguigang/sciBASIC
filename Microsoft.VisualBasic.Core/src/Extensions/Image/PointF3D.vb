#Region "Microsoft.VisualBasic::aeea519afd17da857810af769111f0a1, sciBASIC#\Microsoft.VisualBasic.Core\src\Extensions\Image\PointF3D.vb"

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

    '   Total Lines: 18
    '    Code Lines: 11
    ' Comment Lines: 3
    '   Blank Lines: 4
    '     File Size: 442 B


    '     Interface PointF3D
    ' 
    '         Properties: X, Y, Z
    ' 
    '     Interface Layout2D
    ' 
    '         Properties: X, Y
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Imaging

    ''' <summary>
    ''' 这个接口是为了实现Imaging模块的Point3D对象和数学函数模块的3D插值模块的兼容
    ''' </summary>
    Public Interface PointF3D : Inherits Layout2D
        Property Z As Double
    End Interface

    Public Interface Layout2D

        ''' <summary>
        ''' the x axis data
        ''' </summary>
        ''' <returns></returns>
        Property X As Double
        ''' <summary>
        ''' the y axis data
        ''' </summary>
        ''' <returns></returns>
        Property Y As Double

    End Interface

    Public Interface RasterPixel

        ''' <summary>
        ''' the x axis data
        ''' </summary>
        ''' <returns></returns>
        Property X As Integer
        ''' <summary>
        ''' the y axis data
        ''' </summary>
        ''' <returns></returns>
        Property Y As Integer

    End Interface
End Namespace

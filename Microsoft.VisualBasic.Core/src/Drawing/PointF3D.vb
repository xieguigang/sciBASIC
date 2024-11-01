#Region "Microsoft.VisualBasic::a53892c1bde09fad590c0526ebf9f4da, Microsoft.VisualBasic.Core\src\Drawing\PointF3D.vb"

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

    '   Total Lines: 118
    '    Code Lines: 48 (40.68%)
    ' Comment Lines: 47 (39.83%)
    '    - Xml Docs: 95.74%
    ' 
    '   Blank Lines: 23 (19.49%)
    '     File Size: 3.02 KB


    '     Interface PointF3D
    ' 
    '         Properties: Z
    ' 
    '     Interface IPoint3D
    ' 
    '         Properties: Z
    ' 
    '     Structure SpatialIndex3D
    ' 
    '         Properties: X, Y, Z
    ' 
    '     Interface Layout2D
    ' 
    '         Properties: X, Y
    ' 
    '     Interface RasterPixel
    ' 
    '         Properties: X, Y
    ' 
    '     Interface Pixel
    ' 
    '         Properties: Scale
    ' 
    '     Module PixelExtensions
    ' 
    '         Function: X, Y
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Imaging

    ''' <summary>
    ''' [x,y,z]
    ''' 
    ''' 这个接口是为了实现Imaging模块的Point3D对象和数学函数模块的3D插值模块的兼容
    ''' </summary>
    ''' <remarks>
    ''' 
    ''' </remarks>
    Public Interface PointF3D : Inherits Layout2D

        Property Z As Double

    End Interface

    Public Interface IPoint3D : Inherits RasterPixel

        Property Z As Integer

    End Interface

    ''' <summary>
    ''' a index point in 3d spatial geometry space
    ''' </summary>
    Public Structure SpatialIndex3D : Implements IPoint3D

        Public Property X As Integer Implements IPoint3D.X
        Public Property Y As Integer Implements IPoint3D.Y
        ''' <summary>
        ''' the z axis index data
        ''' </summary>
        ''' <returns></returns>
        Public Property Z As Integer Implements IPoint3D.Z

    End Structure

    ''' <summary>
    ''' a float 2d point
    ''' </summary>
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

    ''' <summary>
    ''' [x,y] tuple
    ''' </summary>
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

    ''' <summary>
    ''' a generic data model for HeatMap raster: [x,y,scale]
    ''' </summary>
    ''' <remarks>
    ''' the layout information comes from the base <see cref="RasterPixel"/> model
    ''' </remarks>
    Public Interface Pixel : Inherits RasterPixel

        ''' <summary>
        ''' the color scale data
        ''' </summary>
        ''' <returns></returns>
        Property Scale As Double

    End Interface

    <HideModuleName>
    Public Module PixelExtensions

        <Extension>
        Public Iterator Function X(Of T As RasterPixel)(shape As IEnumerable(Of T)) As IEnumerable(Of Integer)
            If shape Is Nothing Then
                Return
            Else
                For Each pi As T In shape
                    Yield pi.X
                Next
            End If
        End Function

        <Extension>
        Public Iterator Function Y(Of T As RasterPixel)(shape As IEnumerable(Of T)) As IEnumerable(Of Integer)
            If shape Is Nothing Then
                Return
            Else
                For Each pi As T In shape
                    Yield pi.Y
                Next
            End If
        End Function

    End Module
End Namespace

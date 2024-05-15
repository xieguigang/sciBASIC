#Region "Microsoft.VisualBasic::b312deb5af4068c1d40067226df27f54, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Math3D\Projection.vb"

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

    '   Total Lines: 110
    '    Code Lines: 68
    ' Comment Lines: 26
    '   Blank Lines: 16
    '     File Size: 4.17 KB


    '     Module Projection
    ' 
    '         Function: Center, PointXY, Projection, Rotate, (+3 Overloads) SpaceToGrid
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Math
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection

Namespace Drawing3D.Math3D

    ''' <summary>
    ''' 3D coordinate transformation tools.
    ''' </summary>
    Public Module Projection

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function Projection(points As IEnumerable(Of Point3D), camera As Camera) As PointF()
            Dim result As PointF() = camera _
                .Project(points) _
                .Select(Function(point)
                            Return point.PointXY(camera.screen)
                        End Function) _
                .ToArray
            Return result
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Rotate(polygon As IEnumerable(Of Point3D), camera As Camera) As IEnumerable(Of Point3D)
            Return camera.Rotate(polygon)
        End Function

        ''' <summary>
        ''' Gets the projection 2D point result from this readonly property
        ''' </summary>
        ''' <param name="rect"></param>
        ''' <returns></returns>
        <Extension>
        Public Function PointXY(p As Point3D, Optional rect As Size = Nothing) As PointF
            Dim x# = p.X, y# = p.Y

            If Single.IsPositiveInfinity(CSng(x)) Then
                x = rect.Width
            ElseIf Single.IsNegativeInfinity(CSng(x)) Then
                x = 0
            ElseIf Single.IsNaN(CSng(x)) Then
                x = rect.Width
            End If

            If Single.IsPositiveInfinity(CSng(y)) Then
                y = rect.Height
            ElseIf Single.IsNegativeInfinity(CSng(y)) Then
                y = 0
            ElseIf Single.IsNaN(CSng(y)) Then
                y = rect.Height
            End If

            Return New PointF(CSng(x), CSng(y))
        End Function

        ''' <summary>
        ''' 获取得到目标三维多边形的中心点
        ''' </summary>
        ''' <param name="model"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Center(model As IEnumerable(Of Point3D)) As Point3D
            Dim array As Point3D() = model.ToArray
            Dim x As Single = CSng(array.Select(Function(p) p.X).Sum / array.Length)
            Dim y As Single = CSng(array.Select(Function(p) p.Y).Sum / array.Length)
            Dim z As Single = CSng(array.Select(Function(p) p.Z).Sum / array.Length)

            Return New Point3D(x, y, z)
        End Function

        ''' <summary>
        ''' Transform point 3D into point 2D
        ''' </summary>
        ''' <param name="pt3D"></param>
        ''' <param name="xRotate"></param>
        ''' <returns></returns>
        <ExportAPI("SpaceToGrid")>
        <Extension> Public Function SpaceToGrid(pt3D As Point3D, xRotate As Single) As PointF
            Dim X As Single = CSng(Cos(xRotate) * pt3D.X + pt3D.Y)
            Dim Y As Single = CSng(Sin(xRotate) * pt3D.X - pt3D.Z)

            Return New PointF(X, Y)
        End Function

        <ExportAPI("SpaceToGrid")>
        Public Function SpaceToGrid(px As Single, py As Single, pz As Single, xRotate As Single) As PointF
            Dim X As Single = CSng(Cos(xRotate) * px + py)
            Dim Y As Single = CSng(Sin(xRotate) * px - pz)

            Return New PointF(X, Y)
        End Function

        ''' <summary>
        ''' Project of the 3D point to 2D point
        ''' </summary>
        ''' <param name="pt3D"></param>
        ''' <param name="xRotate"></param>
        ''' <param name="offset"></param>
        ''' <returns></returns>
        <ExportAPI("SpaceToGrid")>
        <Extension> Public Function SpaceToGrid(pt3D As Point3D, xRotate As Single, offset As Point) As PointF
            Dim X As Single = CSng(Cos(xRotate) * pt3D.X + pt3D.Y + offset.X)
            Dim Y As Single = CSng(Sin(xRotate) * pt3D.X - pt3D.Z + offset.Y)

            Return New PointF(X, Y)
        End Function
    End Module
End Namespace

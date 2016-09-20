#Region "Microsoft.VisualBasic::c39bccbee1bc58bafcf76287bb546339, ..\visualbasic_App\Datavisualization\Microsoft.VisualBasic.Imaging\Drawing3D\Transformation.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Drawing3D

    ''' <summary>
    ''' 3D coordinate transformation tools.
    ''' </summary>
    <PackageNamespace("Coordinate.Transformation",
                      Category:=APICategories.UtilityTools,
                      Publisher:="xie.guigang@gmail.com",
                      Description:="3D coordinate transformation tools.")>
    Public Module Transformation

        <Extension>
        Public Function Center(model As IEnumerable(Of Point3D)) As Point3D
            Dim array As Point3D() = model.ToArray
            Dim x As Double = array.Select(Function(p) p.X).Sum / array.Length
            Dim y As Double = array.Select(Function(p) p.Y).Sum / array.Length
            Dim z As Double = array.Select(Function(p) p.Z).Sum / array.Length

            Return New Point3D(x, y, z)
        End Function

        ''' <summary>
        ''' Transform point 3D into point 2D
        ''' </summary>
        ''' <param name="pt3D"></param>
        ''' <param name="xRotate"></param>
        ''' <returns></returns>
        <ExportAPI("SpaceToGrid")>
        <Extension> Public Function SpaceToGrid(pt3D As Point3D, xRotate As Double) As Point
            Dim X As Double = Math.Cos(xRotate) * pt3D.X + pt3D.Y
            Dim Y As Double = Math.Sin(xRotate) * pt3D.X - pt3D.Z

            Return New Point(X, Y)
        End Function

        <ExportAPI("SpaceToGrid")>
        Public Function SpaceToGrid(px As Single, py As Single, pz As Single, xRotate As Double) As Point
            Dim X As Double = Math.Cos(xRotate) * px + py
            Dim Y As Double = Math.Sin(xRotate) * px - pz

            Return New Point(X, Y)
        End Function

        ''' <summary>
        ''' Project of the 3D point to 2D point
        ''' </summary>
        ''' <param name="pt3D"></param>
        ''' <param name="xRotate"></param>
        ''' <param name="offset"></param>
        ''' <returns></returns>
        <ExportAPI("SpaceToGrid")>
        <Extension> Public Function SpaceToGrid(pt3D As Point3D, xRotate As Double, offset As Point) As Point
            Dim X As Double = Math.Cos(xRotate) * pt3D.X + pt3D.Y + offset.X
            Dim Y As Double = Math.Sin(xRotate) * pt3D.X - pt3D.Z + offset.Y

            Return New Point(X, Y)
        End Function
    End Module
End Namespace

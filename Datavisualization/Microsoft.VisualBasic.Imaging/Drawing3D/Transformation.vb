#Region "718b1147fadd006a09a4aa5c5bec8270, ..\Microsoft.VisualBasic.Imaging\Drawing3D\Transformation.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

        <ExportAPI("SpaceToGrid")>
        <Extension> Public Function SpaceToGrid(pt3D As Point3D, xRotate As Double, offset As Point) As Point
            Dim X As Double = Math.Cos(xRotate) * pt3D.X + pt3D.Y + offset.X
            Dim Y As Double = Math.Sin(xRotate) * pt3D.X - pt3D.Z + offset.Y

            Return New Point(X, Y)
        End Function
    End Module
End Namespace

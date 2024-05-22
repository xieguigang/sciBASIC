#Region "Microsoft.VisualBasic::0745b53686cc8414f09a56defc1c915c, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Math3D\Transformation.vb"

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

    '   Total Lines: 155
    '    Code Lines: 106 (68.39%)
    ' Comment Lines: 30 (19.35%)
    '    - Xml Docs: 90.00%
    ' 
    '   Blank Lines: 19 (12.26%)
    '     File Size: 5.83 KB


    '     Module Transformation
    ' 
    '         Properties: ORIGIN
    ' 
    '         Function: Centra, Distance, Offsets, OffSets, RotateX
    '                   RotateY, RotateZ, (+3 Overloads) Scale, Translate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports sys = System.Math

Namespace Drawing3D.Math3D

    Public Module Transformation

        ''' <summary>
        ''' Gets the centra location of the target model which is consist with a set of surface
        ''' </summary>
        ''' <param name="surfaces"></param>
        ''' <returns></returns>
        <Extension> Public Function Centra(surfaces As IEnumerable(Of Surface)) As Point3D
            Dim vertices = surfaces.Select(Function(s) s.vertices).ToVector
            Dim x = vertices.Select(Function(p3D) p3D.X).Average
            Dim y = vertices.Select(Function(p3D) p3D.Y).Average
            Dim z = vertices.Select(Function(p3D) p3D.Z).Average

            Return New Point3D(x, y, z)
        End Function

        <Extension>
        Public Function Offsets(offset As Point3D, model As IEnumerable(Of Surface)) As IEnumerable(Of Surface)
            Dim out As New List(Of Surface)

            For Each surface As Surface In model
                out += New Surface With {
                    .brush = surface.brush,
                    .vertices = surface _
                        .vertices _
                        .Select(Function(p3D) p3D - offset).ToArray
                }
            Next

            Return out
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function OffSets(polygon As IEnumerable(Of Point3D), offset As Point3D) As Point3D()
            Return polygon.Select(Function(point) point - offset).ToArray
        End Function

        ''' <summary>
        ''' 三维坐标系的原点``(0, 0, 0)``
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ORIGIN As New Point3D(0, 0, 0)

#Region "3D rotation around a specific point"

        ''' <summary>
        ''' Rotate about origin on the X axis
        ''' </summary>
        ''' 
        <Extension>
        Public Function RotateX([Me] As Point3D, origin As Point3D, angle As Double) As Point3D
            Dim pY As Double = [Me].Y - origin.Y
            Dim pZ As Double = [Me].Z - origin.Z
            Dim cos As Double = sys.Cos(angle)
            Dim sin As Double = sys.Sin(angle)
            Dim z As Double = pZ * cos - pY * sin
            Dim y As Double = pZ * sin + pY * cos
            pZ = z
            pY = y
            Return New Point3D([Me].X, pY + origin.Y, pZ + origin.Z)
        End Function

        ''' <summary>
        ''' Rotate about origin on the Y axis
        ''' </summary>
        ''' 
        <Extension>
        Public Function RotateY([Me] As Point3D, origin As Point3D, angle As Double) As Point3D
            Dim pX As Double = [Me].X - origin.X
            Dim pZ As Double = [Me].Z - origin.Z
            Dim cos As Double = sys.Cos(angle)
            Dim sin As Double = sys.Sin(angle)
            Dim x As Double = pX * cos - pZ * sin
            Dim z As Double = pX * sin + pZ * cos
            pX = x
            pZ = z
            Return New Point3D(pX + origin.X, [Me].Y, pZ + origin.Z)
        End Function

        ''' <summary>
        ''' Rotate about origin on the Y axis
        ''' </summary>
        ''' 
        <Extension>
        Public Function RotateZ([Me] As Point3D, origin As Point3D, angle As Double) As Point3D
            Dim pX As Double = [Me].X - origin.X
            Dim pY As Double = [Me].Y - origin.Y
            Dim cos As Double = sys.Cos(angle)
            Dim sin As Double = sys.Sin(angle)
            Dim x As Double = pX * cos - pY * sin
            Dim y As Double = pX * sin + pY * cos
            pX = x
            pY = y
            Return New Point3D(pX + origin.X, pY + origin.Y, [Me].Z)
        End Function
#End Region

        ''' <summary>
        ''' Translate a point from a given dx, dy, and dz
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Translate(base As Point3D, dx#, dy#, dz#) As Point3D
            With base
                Return New Point3D(.X + dx, .Y + dy, .Z + dz)
            End With
        End Function

        ''' <summary>
        ''' Scale a point about a given origin
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Scale(base As Point3D, origin As Point3D, dx#, dy#, dz#) As Point3D
            With base
                Return New Point3D(
                    x:=(.X - origin.X) * dx + origin.X,
                    y:=(.Y - origin.Y) * dy + origin.Y,
                    z:=(.Z - origin.Z) * dz + origin.Z
                )
            End With
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Scale(base As Point3D, origin As Point3D, dx As Double) As Point3D
            Return base.Scale(origin, dx, dx, dx)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Scale(base As Point3D, origin As Point3D, dx As Double, dy As Double) As Point3D
            Return base.Scale(origin, dx, dy, 1)
        End Function

        ''' <summary>
        ''' Distance between two points
        ''' </summary>
        <Extension> Public Function Distance(p1 As Point3D, p2 As Point3D) As Double
            Dim dx As Double = p2.X - p1.X
            Dim dy As Double = p2.Y - p1.Y
            Dim dz As Double = p2.Z - p1.Z

            Return sys.Sqrt(dx * dx + dy * dy + dz * dz)
        End Function
    End Module
End Namespace

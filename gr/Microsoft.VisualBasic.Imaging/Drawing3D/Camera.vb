#Region "Microsoft.VisualBasic::9d5040d3eb82c21b15cf2b5c8f661a6f, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Camera.vb"

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
    ' along with this program. If not see <http://www.gnu.org/licenses/>.

    ' /********************************************************************************/

    '     Class Camera
    ' 
    '         Constructor: (+4 Overloads) Sub New
    ' 
    '         Properties: AmbientStrength, AngleX, AngleY, AngleZ, FieldOfView
    '                     LightColor, LightDirection, Offset, Screen, ViewDistance
    ' 
    '         Function: Lighting, (+2 Overloads) Project, (+4 Overloads) Rotate
    '                   (+2 Overloads) RotateX, (+2 Overloads) RotateY
    '                   (+2 Overloads) RotateZ, ToString
    ' 
    '         Sub: Draw
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports std = System.Math

Namespace Drawing3D

    Public Class Camera

        ''' <summary>
        ''' the view distance from the user view to target object
        ''' </summary>
        Public Property ViewDistance!
        Public Property AngleX!
        Public Property AngleY!
        Public Property AngleZ!
        Public Property FieldOfView! = 256.0!
        Public Property Screen As Size
        ''' <summary>
        ''' Using for the project result 
        ''' </summary>
        Public Property Offset As PointF

        ''' <summary>
        ''' Light direction (unit vector pointing toward the light source).
        ''' </summary>
        Public Property LightDirection As Point3D
        ''' <summary>
        ''' Ambient term in [0,1] used by the lighting model.
        ''' </summary>
        Public Property AmbientStrength As Double
        ''' <summary>
        ''' Light color, default using <see cref="Color.White"/>.
        ''' </summary>
        Public Property LightColor As Color

        Public Sub New()
            Dim lightPosition As New Point3D(2, -1, 3)

            Me.LightDirection = lightPosition.Normalize()
            Me.AmbientStrength = 0.2
            Me.LightColor = Color.FromArgb(255, 255, 255)
        End Sub

        Sub New(gfx As IGraphics, viewAngle As Point3D, Optional viewDistance As Single = 100)
            Call Me.New(viewAngle)

            Me.ViewDistance = viewDistance
            Me.Screen = gfx.Size
        End Sub

        Sub New(canvas As GraphicsRegion, viewAngle As Point3D, Optional viewDistance As Single = 100)
            Call Me.New(viewAngle)

            Me.ViewDistance = viewDistance
            Me.Screen = canvas.Size
        End Sub

        Sub New(viewAngle As Point3D)
            Call Me.New()

            AngleX = viewAngle.X
            AngleY = viewAngle.Y
            AngleZ = viewAngle.Z
        End Sub

#Region "Rotation"

        ''' <summary>
        ''' Apply the three camera-axis rotations to a single point, computing the
        ''' trigonometric values only once for the whole transformation.
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function RotatePoint(p As Point3D, cosX As Double, sinX As Double, cosY As Double, sinY As Double, cosZ As Double, sinZ As Double) As Point3D
            ' X axis
            Dim y1 = p.Y * cosX - p.Z * sinX
            Dim z1 = p.Y * sinX + p.Z * cosX
            ' Y axis
            Dim x2 = p.X * cosY + z1 * sinY
            Dim z2 = -p.X * sinY + z1 * cosY
            ' Z axis
            Dim x3 = x2 * cosZ - y1 * sinZ
            Dim y3 = x2 * sinZ + y1 * cosZ

            Return New Point3D(x3, y3, z2)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Rotate(v As Vector3D) As Vector3D
            Return v.RotateX(AngleX).RotateY(AngleY).RotateZ(AngleZ)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Rotate(pt As Point3D) As Point3D
            Dim radX = AngleX * std.PI / 180, radY = AngleY * std.PI / 180, radZ = AngleZ * std.PI / 180

            Return RotatePoint(pt,
                std.Cos(radX), std.Sin(radX),
                std.Cos(radY), std.Sin(radY),
                std.Cos(radZ), std.Sin(radZ))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function RotateX(pt As Point3D) As Point3D
            Return pt.RotateX(AngleX)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function RotateY(pt As Point3D) As Point3D
            Return pt.RotateY(AngleY)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function RotateZ(pt As Point3D) As Point3D
            Return pt.RotateZ(AngleZ)
        End Function

        ''' <summary>
        ''' Batch rotation of a point set. Trigonometric values are computed once
        ''' for the whole set instead of per point.
        ''' </summary>
        Public Iterator Function Rotate(pts As IEnumerable(Of Point3D)) As IEnumerable(Of Point3D)
            Dim radX = AngleX * std.PI / 180, radY = AngleY * std.PI / 180, radZ = AngleZ * std.PI / 180
            Dim cosX = std.Cos(radX), sinX = std.Sin(radX)
            Dim cosY = std.Cos(radY), sinY = std.Sin(radY)
            Dim cosZ = std.Cos(radZ), sinZ = std.Sin(radZ)

            For Each pt As Point3D In pts
                Yield RotatePoint(pt, cosX, sinX, cosY, sinY, cosZ, sinZ)
            Next
        End Function

        Public Iterator Function RotateX(pts As IEnumerable(Of Point3D)) As IEnumerable(Of Point3D)
            For Each pt As Point3D In pts
                Yield pt.RotateX(AngleX)
            Next
        End Function

        Public Iterator Function RotateY(pts As IEnumerable(Of Point3D)) As IEnumerable(Of Point3D)
            For Each pt As Point3D In pts
                Yield pt.RotateY(AngleY)
            Next
        End Function

        Public Iterator Function RotateZ(pts As IEnumerable(Of Point3D)) As IEnumerable(Of Point3D)
            For Each pt As Point3D In pts
                Yield pt.RotateZ(AngleZ)
            Next
        End Function

#End Region

#Region "3D -> 2D Project"

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Project(pt As Point3D) As Point3D
            Return pt.Project(Screen.Width, Screen.Height, FieldOfView, ViewDistance, Offset)
        End Function

        Public Iterator Function Project(pts As IEnumerable(Of Point3D)) As IEnumerable(Of Point3D)
            For Each pt As Point3D In pts
                Yield pt.Project(Screen.Width, Screen.Height, FieldOfView, ViewDistance, Offset)
            Next
        End Function

#End Region

        Public Iterator Function Rotate(model As IEnumerable(Of Surface)) As IEnumerable(Of Surface)
            For Each f As Surface In model
                Yield New Surface With {
                    .brush = f.brush,
                    .vertices = Rotate(f.vertices).ToArray
                }
            Next
        End Function

#If WINDOWS Then
        Public Sub Draw(ByRef canvas As Graphics, surface As IEnumerable(Of Surface), Optional drawPath As Boolean = False)
            Dim faces As New List(Of Surface)

            With Me
                For Each f As Surface In surface
                    faces.Add(New Surface With {
                        .brush = f.brush,
                        .vertices = Rotate(f.vertices).ToArray
                    })
                Next
            End With

            Call PainterAlgorithm.SurfacePainter(canvas, Me, faces, drawPath)
        End Sub
#End If

        ''' <summary>
        ''' Compute the lit color of a surface using the camera's light setup.
        ''' Falls back to the surface's base color when the face is degenerate.
        ''' </summary>
        Public Function Lighting(surface As Surface) As Color
            Dim baseColor As Color

            If TypeOf surface.brush Is SolidBrush Then
                baseColor = DirectCast(surface.brush, SolidBrush).Color
            Else
                baseColor = Color.Black
            End If

            Return surface.vertices _
                .ComputeLighting(LightDirection, baseColor, AmbientStrength, LightColor)
        End Function

        ''' <summary>
        ''' debug view
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Dim debug As New StringBuilder

            Call debug.AppendLine($"Rotation vector:  x={AngleX}, y={AngleY}, z={AngleZ}")
            Call debug.AppendLine($"View distance:    {ViewDistance}")
            Call debug.AppendLine($"FOV:              {FieldOfView}")
            Call debug.AppendLine($"Screen size:      {Screen.Width}px X {Screen.Height}px")
            Call debug.AppendLine($"Light color:      {LightColor.ToHtmlColor}")
            Call debug.AppendLine($"Light direction:  x={LightDirection.X}, y={LightDirection.Y}, z={LightDirection.Z}")
            Call debug.AppendLine($"Ambient:          {AmbientStrength}")

            Return debug.ToString
        End Function
    End Class
End Namespace

#Region "Microsoft.VisualBasic::7d90f16bfd7a9769af471bac74f00dd6, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Camera.vb"

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

    '   Total Lines: 181
    '    Code Lines: 130
    ' Comment Lines: 19
    '   Blank Lines: 32
    '     File Size: 6.32 KB


    '     Class Camera
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: Lighting, (+2 Overloads) Project, (+4 Overloads) Rotate, (+2 Overloads) RotateX, (+2 Overloads) RotateY
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
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Device
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Drawing3D

    Public Class Camera

        ''' <summary>
        ''' the view distance from the user view to target object
        ''' </summary>
        Public viewDistance!
        Public angleX!, angleY!, angleZ!
        Public fov! = 256.0!
        Public screen As Size
        ''' <summary>
        ''' Using for the project result 
        ''' </summary>
        Public offset As PointF

        ''' <summary>
        ''' Light
        ''' </summary>
        Public lightAngle As Point3D
        ''' <summary>
        ''' Light
        ''' </summary>
        Public colorDifference As Double
        ''' <summary>
        ''' Light, default using <see cref="Color.White"/> as the light color
        ''' </summary>
        Public lightColor As Color

        Public Sub New()
            Dim lightPosition As New Point3D(2, -1, 3)

            Me.lightAngle = lightPosition.Normalize()
            Me.colorDifference = 0.2
            Me.lightColor = Color.FromArgb(255, 255, 255)
        End Sub

        Sub New(viewAngle As Point3D)
            Call Me.New()

            angleX = viewAngle.X
            angleY = viewAngle.Y
            angleZ = viewAngle.Z
        End Sub

#Region "Rotation"

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Rotate(v As Vector3D) As Vector3D
            Return v.RotateX(angleX).RotateY(angleY).RotateZ(angleZ)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Rotate(pt As Point3D) As Point3D
            Return pt.RotateX(angleX).RotateY(angleY).RotateZ(angleZ)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function RotateX(pt As Point3D) As Point3D
            Return pt.RotateX(angleX)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function RotateY(pt As Point3D) As Point3D
            Return pt.RotateY(angleY)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function RotateZ(pt As Point3D) As Point3D
            Return pt.RotateZ(angleZ)
        End Function

        Public Iterator Function Rotate(pts As IEnumerable(Of Point3D)) As IEnumerable(Of Point3D)
            For Each pt As Point3D In pts
                Yield pt _
                    .RotateX(angleX) _
                    .RotateY(angleY) _
                    .RotateZ(angleZ)
            Next
        End Function

        Public Iterator Function RotateX(pts As IEnumerable(Of Point3D)) As IEnumerable(Of Point3D)
            For Each pt As Point3D In pts
                Yield pt.RotateX(angleX)
            Next
        End Function

        Public Iterator Function RotateY(pts As IEnumerable(Of Point3D)) As IEnumerable(Of Point3D)
            For Each pt As Point3D In pts
                Yield pt.RotateY(angleY)
            Next
        End Function

        Public Iterator Function RotateZ(pts As IEnumerable(Of Point3D)) As IEnumerable(Of Point3D)
            For Each pt As Point3D In pts
                Yield pt.RotateZ(angleZ)
            Next
        End Function
#End Region

#Region "3D -> 2D Project"

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Project(pt As Point3D) As Point3D
            Return pt.Project(screen.Width, screen.Height, fov, viewDistance, offset)
        End Function

        Public Iterator Function Project(pts As IEnumerable(Of Point3D)) As IEnumerable(Of Point3D)
            For Each pt As Point3D In pts
                Yield pt.Project(screen.Width, screen.Height, fov, viewDistance, offset)
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

        Public Sub Draw(ByRef canvas As Graphics, surface As IEnumerable(Of Surface), Optional drawPath As Boolean = False)
            Dim faces As New List(Of Surface)

            With Me
                For Each f As Surface In surface
                    faces += New Surface With {
                        .brush = f.brush,
                        .vertices = Rotate(f.vertices).ToArray
                    }
                Next
            End With

            Call canvas.SurfacePainter(Me, faces, drawPath)
        End Sub

        Public Function Lighting(surface As Surface) As Color
            Dim color As Color = DirectCast(surface.brush, SolidBrush).Color
            Try
                color = surface _
                    .vertices _
                    .Lighting(lightAngle,
                              color,
                              colorDifference,
                              lightColor)
            Catch ex As Exception

            End Try

            Return color
        End Function

        ''' <summary>
        ''' debug view
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Dim debug As New StringBuilder

            Call debug.AppendLine($"Rotation vector:  x={angleX}, y={angleY}, z={angleZ}")
            Call debug.AppendLine($"View distance:    {viewDistance}")
            Call debug.AppendLine($"FOV:              {fov}")
            Call debug.AppendLine($"Screen size:      {screen.Width}px X {screen.Height}px")
            Call debug.AppendLine($"Light color:      {lightColor.ToHtmlColor}")
            Call debug.AppendLine($"Light angle:      x={lightAngle.X}, y={lightAngle.Y}, z={lightAngle.Z}")

            Return debug.ToString
        End Function
    End Class
End Namespace

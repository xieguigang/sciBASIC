#Region "Microsoft.VisualBasic::17ed7caa05bc738589c1ffad8db742ec, gr\ModelViewer\FormAbout.vb"

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

    '   Total Lines: 44
    '    Code Lines: 37 (84.09%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (15.91%)
    '     File Size: 1.60 KB


    ' Class FormAbout
    ' 
    '     Sub: FormAbout_Load, RenderPanel1_Paint, Timer1_Tick
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models
Imports Microsoft.VisualBasic.Imaging.Landscape.Data

Public Class FormAbout

    Dim cubeModel As New Cube(1)
    Dim WithEvents renderer As New SceneRenderer()

    Private Sub FormAbout_Load(sender As Object, e As EventArgs) Handles Me.Load
        renderer.Camera.Screen = canvas.ClientSize
        renderer.Camera.LightColor = Color.White
        renderer.Camera.AmbientStrength = 0
        renderer.BackgroundColor = canvas.BackColor
        renderer.LoadModel(cubeModel.faces.Select(Function(a) New Landscape.Data.Surface() With {
            .paint = DirectCast(a.brush, SolidBrush).Color.ToHtmlColor,
            .vertices = a.vertices.Select(Function(p) New Vertex(p)).ToArray
        }))
        renderer.Camera.FieldOfView = 0
        renderer.ShowGround = False
        renderer.FitView()
        canvas.Invalidate()

        Timer1.Interval = 15
        Timer1.Enabled = True
    End Sub

    Private Sub RenderPanel1_Paint(sender As Object, e As PaintEventArgs) Handles canvas.Paint
        If renderer Is Nothing Then Return
        If renderer.HasData Then
            renderer.Draw(e.Graphics, canvas.ClientSize)
        Else
            e.Graphics.Clear(renderer.BackgroundColor)
        End If
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        renderer.Camera.AngleX += 3
        renderer.Camera.AngleY += 2
        renderer.Camera.AngleZ += 1

        canvas.Invalidate()
    End Sub
End Class

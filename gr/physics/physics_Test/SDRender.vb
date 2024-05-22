#Region "Microsoft.VisualBasic::6e7c802893b9d7d02445e93e0e359dc0, gr\physics\physics_Test\SDRender.vb"

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

    '   Total Lines: 47
    '    Code Lines: 37 (78.72%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (21.28%)
    '     File Size: 1.68 KB


    '     Module SDRender
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: RenderField
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Physics.Boids

Namespace Boids.Viewer
    Public Module SDRender

        Dim colors As Color()
        Dim n As Integer = 30

        Sub New()
            colors = Designer.GetColors(ScalerPalette.Typhoon.Description, n)
        End Sub

        Public Function RenderField(field As Field) As Bitmap
            Dim bmp As Bitmap = New Bitmap(CInt(field.Width), CInt(field.Height))
            Dim max As Double = field.MaxSpeed

            Using gfx = Graphics.FromImage(bmp)
                gfx.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                gfx.Clear(Color.Black) ' (ColorTranslator.FromHtml("#003366"))

                Dim len As Integer = field.Entity.Count

                For i = 0 To len - 1
                    Dim boid As Boid = field(i)

                    If i < field.PredatorCount Then
                        RenderShape.RenderBoid(gfx, boid.x, boid.y, boid.GetAngle, Color.White)
                    Else
                        Dim lv As Integer = ((boid.GetSpeed / max) * n) - 1

                        If lv < 0 Then
                            lv = 0
                        ElseIf lv >= colors.Length Then
                            lv = colors.Length - 1
                        End If

                        RenderShape.RenderBoid(gfx, boid.x, boid.y, boid.GetAngle, colors(lv))
                    End If
                Next
            End Using
            Return bmp
        End Function
    End Module
End Namespace

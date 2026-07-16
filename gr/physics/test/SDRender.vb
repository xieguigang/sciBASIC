#Region "Microsoft.VisualBasic::4f1fc988606e86f9d6c77cc65996528b, gr\physics\test\SDRender.vb"

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

    '   Total Lines: 49
    '    Code Lines: 38 (77.55%)
    ' Comment Lines: 1 (2.04%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (20.41%)
    '     File Size: 1.80 KB


    '     Module SDRender
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: RenderField
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Drawing
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Physics.Boids
Imports Microsoft.VisualBasic.Imaging

Namespace Boids.Viewer
    Public Module SDRender

        Dim colors As Color()
        Dim n As Integer = 30

        Sub New()
            colors = Designer.GetColors(ScalerPalette.viridis.Description, n)
        End Sub

        Public Function RenderField(field As Field) As System.Drawing.Bitmap
            Dim bmp As New System.Drawing.Bitmap(CInt(field.Width), CInt(field.Height))
            Dim max As Double = field.MaxSpeed

            Using gfx As IGraphics = Graphics2D.Open(bmp)
                ' gfx.SmoothingMode = SmoothingMode.AntiAlias
                gfx.Clear(Color.Black) ' (ColorTranslator.FromHtml("#003366"))

                Dim len As Integer = field.Entity.Count

                For i = 0 To len - 1
                    Dim boid As Boid = field(i)

                    If i < field.PredatorCount Then
                        RenderShape.RenderBoid(gfx, boid.x, boid.y, boid.Xvel, boid.Yvel, Color.White)
                    Else
                        Dim lv As Integer = ((boid.GetSpeed / max) * n) - 1

                        If lv < 0 Then
                            lv = 0
                        ElseIf lv >= colors.Length Then
                            lv = colors.Length - 1
                        End If

                        RenderShape.RenderBoid(gfx, boid.x, boid.y, boid.Xvel, boid.Yvel, colors(lv))
                    End If
                Next
            End Using
            Return bmp
        End Function
    End Module
End Namespace

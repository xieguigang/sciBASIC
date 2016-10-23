#Region "Microsoft.VisualBasic::0724f552225d2d38458c3c63f679ceb6, ..\visualbasic_App\Data_science\Mathematical\Plots\3D\Scatter.vb"

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
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing2D

Namespace Plot3D

    Public Module Scatter

        Public Function Plot(func As Func(Of Double, Double, Double),
                             x As DoubleRange,
                             y As DoubleRange,
                             camera As Camera,
                             Optional xsteps! = 0.1,
                             Optional ysteps! = 0.1,
                             Optional font As Font = Nothing,
                             Optional bg$ = "white") As Bitmap

            Dim data As Point3D() = Evaluate(func, x, y, xsteps, ysteps)

            Return GraphicsPlots(
                camera.screen, New Size(5, 5), bg,
                Sub(ByRef g, region)
                    Call AxisDraw.DrawAxis(g, data, camera, font)

                    With camera

                        For Each pt As Point3D In data
                            pt = .Project(.Rotate(pt))
                            Call g.FillPie(Brushes.Red, New Rectangle(pt.PointXY(camera.screen), New Size(5, 5)), 0, 360)
                        Next
                    End With
                End Sub)
        End Function
    End Module
End Namespace

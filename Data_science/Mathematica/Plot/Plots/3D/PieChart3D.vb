#Region "Microsoft.VisualBasic::3eb6b2d2070d225311e7e0df7cbf46c9, ..\sciBASIC#\Data_science\Mathematica\Plot\Plots\3D\PieChart3D.vb"

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
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models.Isometric.Shapes
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math

Namespace Plot3D

    Public Module PieChart3D

        <Extension>
        Public Function Plot3D(data As IEnumerable(Of NamedValue(Of Integer)), camera As Camera, Optional schema$ = "Paired:c12") As GraphicsData
            Return data.Fractions(schema).Plot3D(camera)
        End Function

        <Extension>
        Public Function Plot3D(data As IEnumerable(Of Fractions), camera As Camera, Optional valueLabel As ValueLabels = ValueLabels.Percentage) As GraphicsData
            Dim start As New float
            Dim sweep As New float
            Dim alpha As Double, pt As PointF
            Dim centra As Point3D = camera.screen.GetCenter
            Dim r! = 2.0!
            Dim label$
            Dim pie As Pie
            Dim pieChart As New List(Of Surface)

            For Each x As Fractions In data
                pie = New Pie(centra, r, (start = ((+start) + (sweep = CSng(360 * x.Percentage)))) - sweep.value, sweep, 20, 1)
                pieChart += pie.Model3D(x.Color)
                alpha = (+start) - (+sweep / 2)
                pt = (r / 1.5).ToPoint(alpha)  ' 在这里r/1.5是因为这些百分比的值的标签需要显示在pie的内部
                pt = New PointF(pt.X + centra.X, pt.Y + centra.Y)
                label = x.GetValueLabel(valueLabel)
            Next

            Dim plot3DInternal =
                Sub(ByRef g As IGraphics, region As GraphicsRegion)
                    With pieChart.Centra
                        Dim matrix As New Matrix(.Offsets(pieChart))
                        Dim vector = camera.Rotate(matrix.Matrix)
                        Dim model2D = matrix _
                            .TranslateBuffer(camera, vector, True) _
                            .AsList

                        Call g.BufferPainting(model2D,)
                    End With
                End Sub

            Return g.GraphicsPlots(
                camera.screen, g.DefaultPadding,
                "white",
                plot3DInternal)
        End Function
    End Module
End Namespace

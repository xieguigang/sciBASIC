#Region "Microsoft.VisualBasic::e86402302c95ee792934a5e4dcd7b6d3, ..\sciBASIC#\Data_science\Mathematica\Plot\Plots\3D\g\Grid.vb"

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
Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D.Device
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace Plot3D.Model

    Public Module GridBottom

        Public Function Grid(xrange As DoubleRange, yrange As DoubleRange, steps As (X!, Y!), Z#, Optional strokeCSS$ = Stroke.AxisGridStroke) As Line()
            Dim gridData As New List(Of Line)
            Dim a, b As Point3D
            Dim pen As Pen = Stroke.TryParse(strokeCSS).GDIObject

            For X As Double = xrange.Min To xrange.Max Step steps.X
                a = New Point3D With {.X = X, .Y = yrange.Min, .Z = Z}
                b = New Point3D With {.X = X, .Y = yrange.Max, .Z = Z}
                gridData += New Line(a, b) With {
                    .Stroke = pen
                }
            Next

            For Y As Double = yrange.Min To yrange.Max Step steps.Y
                a = New Point3D With {.X = xrange.Min, .Y = Y, .Z = Z}
                b = New Point3D With {.X = xrange.Max, .Y = Y, .Z = Z}
                gridData += New Line(a, b) With {
                    .Stroke = pen
                }
            Next

            Return gridData
        End Function
    End Module
End Namespace

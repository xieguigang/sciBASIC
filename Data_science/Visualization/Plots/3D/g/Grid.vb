#Region "Microsoft.VisualBasic::283323255f104320a20f0501b64dd834, Data_science\Visualization\Plots\3D\g\Grid.vb"

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

'     Module GridBottom
' 
'         Function: Grid
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D.Device
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Html.CSS

Namespace Plot3D.Model

    Public Module Grids

        Public Iterator Function Grid1(xrange As DoubleRange, yrange As DoubleRange, steps As (X!, Y!), Z#, Optional strokeCSS$ = Stroke.AxisGridStroke) As IEnumerable(Of Element3D)
            Dim gridData As New List(Of Line)
            Dim a, b As Point3D
            Dim pen As Pen = Stroke.TryParse(strokeCSS).GDIObject

            For X As Double = xrange.Min To xrange.Max Step steps.X
                a = New Point3D With {.X = X, .Y = yrange.Min, .Z = Z}
                b = New Point3D With {.X = X, .Y = yrange.Max, .Z = Z}

                Yield New Line(a, b) With {
                    .Stroke = pen
                }
            Next

            For Y As Double = yrange.Min To yrange.Max Step steps.Y
                a = New Point3D With {.X = xrange.Min, .Y = Y, .Z = Z}
                b = New Point3D With {.X = xrange.Max, .Y = Y, .Z = Z}

                Yield New Line(a, b) With {
                    .Stroke = pen
                }
            Next
        End Function

        Public Iterator Function Grid2(xrange As DoubleRange, zrange As DoubleRange, steps As (X!, Z!), Y#, Optional strokeCSS$ = Stroke.AxisGridStroke) As IEnumerable(Of Element3D)
            Dim gridData As New List(Of Line)
            Dim a, b As Point3D
            Dim pen As Pen = Stroke.TryParse(strokeCSS).GDIObject

            For X As Double = xrange.Min To xrange.Max Step steps.X
                a = New Point3D With {.X = X, .Z = zrange.Min, .Y = Y}
                b = New Point3D With {.X = X, .Z = zrange.Max, .Y = Y}

                Yield New Line(a, b) With {
                    .Stroke = pen
                }
            Next

            For z As Double = zrange.Min To zrange.Max Step steps.Z
                a = New Point3D With {.X = xrange.Min, .Y = Y, .Z = z}
                b = New Point3D With {.X = xrange.Max, .Y = Y, .Z = z}

                Yield New Line(a, b) With {
                    .Stroke = pen
                }
            Next
        End Function

        Public Iterator Function Grid3(yrange As DoubleRange, zrange As DoubleRange, steps As (Y!, Z!), X#, Optional strokeCSS$ = Stroke.AxisGridStroke) As IEnumerable(Of Element3D)
            Dim a, b As Point3D
            Dim pen As Pen = Stroke.TryParse(strokeCSS).GDIObject

            For z As Double = zrange.Min To zrange.Max Step steps.Z
                a = New Point3D With {.X = X, .Y = yrange.Min, .Z = z}
                b = New Point3D With {.X = X, .Y = yrange.Max, .Z = z}

                Yield New Line(a, b) With {
                    .Stroke = pen
                }
            Next

            For Y As Double = yrange.Min To yrange.Max Step steps.Y
                a = New Point3D With {.Z = zrange.Min, .X = X, .Y = Y}
                b = New Point3D With {.Z = zrange.Max, .X = X, .Y = Y}

                Yield New Line(a, b) With {
                    .Stroke = pen
                }
            Next
        End Function
    End Module
End Namespace

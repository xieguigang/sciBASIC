#Region "Microsoft.VisualBasic::a7ba88e540f3d1f79bd091c3068759d4, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\d3js\interpolate\rgb.vb"

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
Imports Microsoft.VisualBasic.Imaging.d3.color

Namespace d3.interpolate

    Public Module rgb

        Public Function interpolateRgb(a As Drawing.Color, b As Drawing.Color) As Func(Of Byte, String)
            Dim ar = a.R,
                ag = a.G,
                ab = a.B,
                br = b.R - ar,
                bg = b.G - ag,
                bb = b.B - ab

            Return Function(t As Byte) _
                       $"#{d3_rgb_hex(Math.Round(ar + br * t))}{d3_rgb_hex(Math.Round(ag + bg * t))}{d3_rgb_hex(Math.Round(ab + bb * t))}"
        End Function
    End Module
End Namespace

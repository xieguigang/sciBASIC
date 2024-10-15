#Region "Microsoft.VisualBasic::68397ce361c70fd984f3a930cff75325, gr\Microsoft.VisualBasic.Imaging\test\ShadowsTest.vb"

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

'   Total Lines: 66
'    Code Lines: 30 (45.45%)
' Comment Lines: 21 (31.82%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 15 (22.73%)
'     File Size: 2.87 KB


' Module ShadowsTest
' 
'     Sub: Main1
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.Drawing
Imports Microsoft.VisualBasic.Imaging

Module ShadowsTest

    Sub Main1()

        Using Graphics As Graphics2D = New Size(500, 500).CreateGDIDevice(Color.White)

            Dim _path As New GraphicsPath

            _path.AddEllipse(New RectangleF(20, 30, 460, 460))
            '_path.AddLine(New PointF(20, 20), New PointF(300, 300))
            '_path.AddLine(New PointF(300, 300), New PointF(200, 450))
            '_path.AddLine(New PointF(200, 450), New PointF(20, 20))
            _path.CloseAllFigures()

            ' this Is where we create the shadow effect, so we will use a 
            ' pathgradientbursh And assign our GraphicsPath that we created of a 
            ' Rounded Rectangle
            Using _Brush As New PathGradientBrush(_path)

                ' set the wrapmode so that the colors will layer themselves
                ' from the outer edge in
                _Brush.WrapMode = WrapMode.Clamp

                ' Create a color blend to manage our colors And positions And
                ' since we need 3 colors set the default length to 3
                Dim _ColorBlend As New ColorBlend(3)

                ' here Is the important part of the shadow making process, remember
                ' the clamp mode on the colorblend object layers the colors from
                ' the outside to the center so we want our transparent color first
                ' followed by the actual shadow color. Set the shadow color to a 
                ' slightly transparent DimGray, I find that it works best.|
                _ColorBlend.Colors = {
                    Color.Transparent,
                    Color.FromArgb(120, Color.DimGray),
                    Color.FromArgb(150, Color.DimGray),
                    Color.FromArgb(200, Color.DimGray)
                }

                ' our color blend will control the distance of each color layer
                ' we want to set our transparent color to 0 indicating that the 
                ' transparent color should be the outer most color drawn, then
                ' our Dimgray color at about 10% of the distance from the edge
                _ColorBlend.Positions = {0F, 0.125, 0.5F, 1.0F}

                ' assign the color blend to the pathgradientbrush
                _Brush.InterpolationColors = _ColorBlend

                ' fill the shadow with our pathgradientbrush
                Graphics.FillPath(_Brush, _path)

                _path = New GraphicsPath
                _path.AddEllipse(New RectangleF(0, 0, 450, 450))
                _path.CloseAllFigures()

                Graphics.FillPath(Brushes.Red, _path)

                Graphics.Save("./shadows.png", ImageFormats.Png)
            End Using
        End Using
    End Sub
End Module

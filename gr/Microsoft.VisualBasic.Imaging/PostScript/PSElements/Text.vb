#Region "Microsoft.VisualBasic::d98189869b81cc2c092923fa4428a379, gr\Microsoft.VisualBasic.Imaging\PostScript\PSElements\Text.vb"

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

    '   Total Lines: 52
    '    Code Lines: 41 (78.85%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (21.15%)
    '     File Size: 1.86 KB


    '     Class Text
    ' 
    '         Properties: font, location, rotation, text
    ' 
    '         Function: GetSize, GetXy, ScaleTo, ToString
    ' 
    '         Sub: Paint, WriteAscii
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render

Namespace PostScript.Elements

    Public Class Text : Inherits PSElement

        Public Property text As String
        Public Property font As CSSFont
        Public Property rotation As Single
        Public Property location As PointF

        Friend Overrides Sub WriteAscii(ps As Writer)
            ps.font(font)
            ps.color(font.color.TranslateColor)
            ps.text(text, location.X, location.Y)
        End Sub

        Friend Overrides Sub Paint(g As IGraphics)
            Dim css As CSSEnvirnment = g.LoadEnvironment
            Dim font As Font = css.GetFont(Me.font)
            Dim br As New SolidBrush(Me.font.color.TranslateColor)

            Call g.DrawString(text, font, br, location.X, location.Y, rotation)
        End Sub

        Public Overrides Function ToString() As String
            Return $"({location.X.ToString("F1")},{location.Y.ToString("F1")}) text({text}) [{font.color}]"
        End Function

        Friend Overrides Function ScaleTo(scaleX As d3js.scale.LinearScale, scaleY As d3js.scale.LinearScale) As PSElement
            Return New Text With {
                .font = font,
                .location = New PointF(scaleX(location.X), scaleY(location.Y)),
                .rotation = rotation,
                .text = text,
                .comment = comment
            }
        End Function

        Friend Overrides Function GetXy() As PointF
            Return location
        End Function

        Friend Overrides Function GetSize() As SizeF
            Return DriverLoad.MeasureTextSize(text, New CSSEnvirnment(1000, 1000).GetFont(font))
        End Function
    End Class

End Namespace

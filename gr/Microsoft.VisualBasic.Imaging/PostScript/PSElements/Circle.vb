#Region "Microsoft.VisualBasic::c271c0d41a6bea00b3895381f8333ad0, gr\Microsoft.VisualBasic.Imaging\PostScript\PSElements\Circle.vb"

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
    '    Code Lines: 34 (77.27%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (22.73%)
    '     File Size: 1.52 KB


    '     Class Circle
    ' 
    '         Properties: center, stroke
    ' 
    '         Function: ScaleTo
    ' 
    '         Sub: Paint, WriteAscii
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render

Namespace PostScript.Elements

    Public Class Circle : Inherits PSElement(Of Shapes.Circle)

        Public Property stroke As Stroke

        Public ReadOnly Property center As PointF
            Get
                Dim r As Single = shape.Radius
                Dim x = r + shape.Location.X
                Dim y = r + shape.Location.Y

                Return New PointF(x, y)
            End Get
        End Property

        Friend Overrides Sub WriteAscii(ps As Writer)
            Throw New NotImplementedException()
        End Sub

        Friend Overrides Sub Paint(g As IGraphics)
            Call g.DrawCircle(center, shape.Radius, shape.fill.GetBrush)

            If Not stroke Is Nothing Then
                Call g.DrawCircle(center, shape.Radius, g.LoadEnvironment.GetPen(stroke), fill:=False)
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return $"({center.X.ToString("F1")},{center.Y.ToString("F1")}) circle(radius={shape.Radius.ToString("F2")}) [{shape.fill}]"
        End Function

        Friend Overrides Function ScaleTo(scaleX As d3js.scale.LinearScale, scaleY As d3js.scale.LinearScale) As PSElement
            Dim O As Shapes.Circle = shape
            Dim moveTo As New PointF(scaleX(O.Location.X), scaleY(O.Location.Y))

            Return New Circle With {
                .stroke = stroke,
                .shape = New Shapes.Circle(moveTo, O.Radius * 2, O.fill.TranslateColor),
                .comment = comment
            }
        End Function

        Friend Overrides Function GetXy() As PointF
            Return shape.Location
        End Function

        Friend Overrides Function GetSize() As SizeF
            Return shape.Size
        End Function
    End Class
End Namespace

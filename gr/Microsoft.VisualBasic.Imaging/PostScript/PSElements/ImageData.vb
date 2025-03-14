#Region "Microsoft.VisualBasic::2c1255deca4463ed10ecf3ff11082323, gr\Microsoft.VisualBasic.Imaging\PostScript\PSElements\ImageData.vb"

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

    '   Total Lines: 37
    '    Code Lines: 27 (72.97%)
    ' Comment Lines: 4 (10.81%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (16.22%)
    '     File Size: 1.38 KB


    '     Class ImageData
    ' 
    '         Properties: image, location, scale, size
    ' 
    '         Function: ScaleTo
    ' 
    '         Sub: Paint, WriteAscii
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Net.Http

Namespace PostScript.Elements

    Public Class ImageData : Inherits PSElement

        ''' <summary>
        ''' image data is encoded as base64 data uri
        ''' </summary>
        ''' <returns></returns>
        Public Property image As DataURI
        Public Property size As Size
        Public Property scale As SizeF
        Public Property location As PointF

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Overrides Sub WriteAscii(ps As Writer)
            Call ps.image(image, location.X, location.Y, size.Width, size.Height, scale.Width, scale.Height)
        End Sub

        Friend Overrides Sub Paint(g As IGraphics)
            Call g.DrawImage(DriverLoad.LoadFromStream(image.ToStream), location.X, location.Y, size.Width, size.Height)
        End Sub

        Friend Overrides Function ScaleTo(scaleX As d3js.scale.LinearScale, scaleY As d3js.scale.LinearScale) As PSElement
            Return New ImageData With {
                .image = image,
                .location = New PointF(scaleX(location.X), scaleY(location.Y)),
                .scale = scale,
                .size = size
            }
        End Function

        Friend Overrides Function GetXy() As PointF
            Return location
        End Function
    End Class
End Namespace

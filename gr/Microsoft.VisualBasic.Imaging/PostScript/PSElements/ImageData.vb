#Region "Microsoft.VisualBasic::340ff9bbe8d4bfbd8770bec267f212aa, gr\Microsoft.VisualBasic.Imaging\PostScript\PSElements\ImageData.vb"

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

    '   Total Lines: 27
    '    Code Lines: 18 (66.67%)
    ' Comment Lines: 4 (14.81%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (18.52%)
    '     File Size: 881 B


    '     Class ImageData
    ' 
    '         Properties: image, location, scale, size
    ' 
    '         Sub: Paint, WriteAscii
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
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
            Throw New NotImplementedException()
        End Sub
    End Class
End Namespace

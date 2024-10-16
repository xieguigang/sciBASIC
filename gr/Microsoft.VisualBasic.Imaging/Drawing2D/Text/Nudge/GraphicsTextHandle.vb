#Region "Microsoft.VisualBasic::7b216585bf52c2ba6c09d7933bcb91ba, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Text\Nudge\GraphicsTextHandle.vb"

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

    '   Total Lines: 36
    '    Code Lines: 24 (66.67%)
    ' Comment Lines: 4 (11.11%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (22.22%)
    '     File Size: 1.09 KB


    '     Class GraphicsTextHandle
    ' 
    '         Properties: canvas, texts
    ' 
    '         Function: get_figheight, get_figwidth, get_xlim, get_ylim
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.d3js.Layout
Imports Microsoft.VisualBasic.MIME.Html.CSS

Namespace Drawing2D.Text.Nudge

    Public Class GraphicsTextHandle

        Public Property texts As Label()

        ''' <summary>
        ''' the internal plot region
        ''' </summary>
        ''' <returns></returns>
        Public Property canvas As GraphicsRegion

        Public Function get_xlim(css As CSSEnvirnment) As Double()
            With canvas.PlotRegion(css)
                Return { .X, .Right}
            End With
        End Function

        Public Function get_ylim(css As CSSEnvirnment) As Double()
            With canvas.PlotRegion(css)
                Return { .Top, .Bottom}
            End With
        End Function

        Friend Function get_figheight(css As CSSEnvirnment) As Double
            Return canvas.PlotRegion(css).Height
        End Function

        Friend Function get_figwidth(css As CSSEnvirnment) As Double
            Return canvas.PlotRegion(css).Width
        End Function
    End Class
End Namespace

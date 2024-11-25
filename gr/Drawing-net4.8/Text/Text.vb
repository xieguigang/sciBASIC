#Region "Microsoft.VisualBasic::1a1b9b51ee1a4bea62677860eccb1f88, gr\Drawing-net4.8\Text\Text.vb"

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

    '   Total Lines: 75
    '    Code Lines: 49 (65.33%)
    ' Comment Lines: 18 (24.00%)
    '    - Xml Docs: 94.44%
    ' 
    '   Blank Lines: 8 (10.67%)
    '     File Size: 2.56 KB


    '     Module TextRender
    ' 
    '         Function: DrawHtmlText
    ' 
    '         Sub: RenderHTML
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Drawing.Imaging.BitmapImage
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports Microsoft.VisualBasic.Scripting.Expressions
Imports Image = System.Drawing.Image

Namespace Drawing2D.Text

    ''' <summary>
    ''' 基于HTML语法的字符串的绘制描述信息的解析
    ''' </summary>
    Public Module TextRender

        ReadOnly HTMLtemplate$ = (
            <html>
                <head>
                    <style type="text/css">
                    body {
                        $font
                    }
                    </style>
                </head>
                <body>
                    $text
                </body>
            </html>).ToString

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="text$"></param>
        ''' <param name="CSS_style$"></param>
        ''' <param name="location">默认是 ``(0, 0)``</param>
        ''' <param name="maxWidth%"></param>
        <Extension>
        Public Sub RenderHTML(ByRef g As Graphics, text$, CSS_style$, Optional location As PointF = Nothing, Optional maxWidth% = 1024)
            Dim table As New Dictionary(Of String, String) From {
                {"font", CSS_style},
                {"text", text}
            }
            Dim html$ = HTMLtemplate _
                .Interpolate(table, nullAsEmpty:=True)

#If NET48 Then
            Call HtmlRenderer.Render(
                g, html,
                location, maxWidth
            )
#Else
            Throw New NotImplementedException
#End If
        End Sub

        ''' <summary>
        ''' Rendering the html text as gdi+ image
        ''' </summary>
        ''' <param name="label$">HTML</param>
        ''' <param name="cssFont$">For html ``&lt;p>...&lt;/p>`` css style</param>
        ''' <param name="maxSize$"></param>
        ''' <returns></returns>
        Public Function DrawHtmlText(label$, cssFont$, Optional maxSize$ = "1600,600") As Image
            Using g As Graphics2D = New Size(1600, 600).CreateGDIDevice(Color.Transparent)
                Dim out As Image

                TextRender.RenderHTML(g.Graphics, label, cssFont,, maxWidth:=g.Width)
                out = g.GetImageResource
                out = out.CorpBlank(blankColor:=Color.Transparent)

                Return out
            End Using
        End Function
    End Module
End Namespace

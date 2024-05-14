#Region "Microsoft.VisualBasic::e2f4ef919d6e2046e3176eaef0c08040, gr\Microsoft.VisualBasic.Imaging\test\HtmlRenderTest.vb"

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

    '   Total Lines: 31
    '    Code Lines: 22
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 1.08 KB


    ' Module HtmlRenderTest
    ' 
    '     Sub: Main1
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text
Imports Microsoft.VisualBasic.MIME.Html.Render

Public Module HtmlRenderTest

    ReadOnly testHTML$ =
        (<div style='font-style: normal; font-size: 14; font-family: Microsoft YaHei;' attr2="99999999 + dd">
             <span style="color:red;">Hello</span><span style="color:blue;">world!</span> 
            2<sup>333333</sup> + X<sub>i</sub> = <span style="font-size: 36;">6666666</span>
         </div>).ToString

    Sub Main1()

        Dim content = TextAPI.TryParse(testHTML).ToArray

        Using g As Graphics2D = New Size(1500, 400).CreateGDIDevice
            Call g.RenderHTML(content, New Point(20, 20))

            Dim size = g.MeasureSize(content).ToSize
            Dim rect As New Rectangle With {.Location = New Point(20, 20), .Size = size}

            Call g.DrawRectangle(Pens.Green, rect)

            Call g.Save("./test.png", ImageFormats.Png)
        End Using

        Pause()
    End Sub
End Module

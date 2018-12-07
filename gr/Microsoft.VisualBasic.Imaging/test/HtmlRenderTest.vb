Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text
Imports Microsoft.VisualBasic.MIME.Markup.HTML

Public Module HtmlRenderTest

    ReadOnly testHTML$ =
        (<div style='font-style: normal; font-size: 14; font-family: Microsoft YaHei;' attr2="99999999 + dd">
             <span style="color:red;">Hello</span><span style="color:blue;">world!</span> 
            2<sup>333333</sup> + X<sub>i</sub> = <span style="font-size: 36;">6666666</span>
         </div>).ToString

    Sub Main()

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

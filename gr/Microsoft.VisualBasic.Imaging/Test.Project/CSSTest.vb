Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.MIME.Markup.HTML.Render

Module CSSTest
    Sub Main()
        Dim html$ = "<html>
<style>

#font {

font-size: 55px;
color: red;
}

</style>

<div id=""font"">12345</div>

</html>"


        Using g = New Size(100, 100).CreateGDIDevice
            Call g.Graphics.Render(html, New Point(0, 0), 10)
        End Using
    End Sub
End Module

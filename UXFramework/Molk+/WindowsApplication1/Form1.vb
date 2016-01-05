Public Class Form1
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        '  Dim doc = Microsoft.VisualBasic.DocumentFormat.HTML.HtmlDocument.Load("G:\Microsoft.VisualBasic_Framework\UXFramework\Molk+\WindowsApplication1\bin\Debug\testDialog.htm")
        ' doc = Microsoft.VisualBasic.DocumentFormat.HTML.HtmlDocument.Load("G:\Microsoft.VisualBasic_Framework\UXFramework\Molk+\WindowsApplication1\bin\Debug\Microsoft.VisualBasic.DocumentFormat.HTML.xml")

        HtmlUserControl1.Control = New MolkPlusTheme.HtmlUserControl.HtmlControl With {.HTML = "<a href=""/invoke1"">button1 test</a>"}
        HtmlUserControl1.Control.AddHandler("/invoke1", Sub() Call MsgBox("2342342342"))
    End Sub


End Class

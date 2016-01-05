Imports Microsoft.VisualBasic.MolkPlusTheme.ControlComponent

Public Class Form1

    Dim Caption As MolkPlusTheme.Windows.Forms.Controls.Caption

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Caption = New MolkPlusTheme.Windows.Forms.Controls.Caption
        Controls.Add(Caption)

        Caption.Text = "Test Form"
        Caption.SubCaption = "Form1"
        Caption.Icon = Me.Icon.ToBitmap
        Caption.Update()

        Call MolkPlusTheme.Visualise.Elements.MultipleTabPage.MolkPlusTheme.SetupResource(MultipleTabpagePanel1)

        Call MultipleTabpagePanel1.AddTabPage("XXXXX", New Panel With {.BackColor = Color.AntiqueWhite}, TabCloseEventHandle:=Nothing)
        Call MultipleTabpagePanel1.AddTabPage("ABCDEFG", New Panel With {.BackColor = Color.NavajoWhite}, TabCloseEventHandle:=Nothing)
        Call MultipleTabpagePanel1.AddTabPage("XIEGUIGANG", New Panel With {.BackColor = Color.DarkSeaGreen}, TabCloseEventHandle:=Nothing)
        Call MultipleTabpagePanel1.AddTabPage("POPO", New Panel With {.BackColor = Color.HotPink}, TabCloseEventHandle:=Nothing)
        Call MultipleTabpagePanel1.AddTabPage("{{{{{{", New Panel With {.BackColor = Color.Maroon}, TabCloseEventHandle:=Nothing)
        Call MultipleTabpagePanel1.AddTabPage("1+1=2", New Panel With {.BackColor = Color.GhostWhite}, TabCloseEventHandle:=Nothing)
        Call MultipleTabpagePanel1.AddTabPage("000", New Panel With {.BackColor = Color.CadetBlue}, TabCloseEventHandle:=Nothing)

        TabControl1.TabLabelWidth = 130

        Call TabControl1.AddTabPage("TEST", New Panel With {.BackColor = Color.DarkSeaGreen})
        Call TabControl1.AddTabPage("NULL", New Panel With {.BackColor = Color.DarkGoldenrod}, )
    End Sub

    Private Sub Form1_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        MultipleTabpagePanel1.Size = Me.Size
    End Sub
    Private Sub MultipleTabpagePanel1_Click(sender As Object, e As EventArgs) Handles MultipleTabpagePanel1.Click

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Call Me.InputBoxShowDialog(Sub(s As String)
                                       MsgBox(s)
                                   End Sub, New Point)
    End Sub
End Class

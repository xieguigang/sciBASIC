#Region "Microsoft.VisualBasic::582837a7585bc86e80e6cabba68edd13, ..\visualbasic_App\UXFramework\Molk+\TestProject\Form1.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

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

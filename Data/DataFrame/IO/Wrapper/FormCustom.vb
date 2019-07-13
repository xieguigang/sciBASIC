#Region "Microsoft.VisualBasic::68ac2d775258e6df3c2de6a138b27666, Data\DataFrame\IO\Wrapper\FormCustom.vb"

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

    ' Class FormCustom
    ' 
    '     Sub: Button1_Click, Button2_Click, ComboBox1_SelectedIndexChanged, EditTitle, FormCustom_FormClosed
    '          FormCustom_Load, LinkLabel1_LinkClicked, LinkLabel2_LinkClicked
    ' 
    ' /********************************************************************************/

#End Region

Friend Class FormCustom

    Public Form As CsvChartDevice

    Private Sub FormCustom_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        Form.ShownDialog = False
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Call EditTitle()

        Close()
    End Sub

#Region "Edit methods"
    Private Sub EditTitle()
        If Not String.IsNullOrEmpty(TextBox1.Text) Then
            Dim T = Form._chart.Titles("Title1")
            T.Text = TextBox1.Text
        End If
    End Sub

#End Region

    Private Sub Button2_Click(sender As Object, e As EventArgs)
        Close()
    End Sub

    Private Sub FormCustom_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        For Each Column In Form.Data.SchemaOridinal
            If Column.Value = 0 Then
                Continue For
            End If
            ComboBox1.Items.Add(Column.Key)
        Next
        ComboBox1.SelectedIndex = 0
        TextBox1.Text = Form._chart.Titles("Title1").Text
    End Sub

    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        If ColorDialog1.ShowDialog = System.Windows.Forms.DialogResult.OK Then
            Form._chart.Series(index:=ComboBox1.SelectedIndex).Color = ColorDialog1.Color
        End If
    End Sub

    Private Sub LinkLabel2_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel2.LinkClicked
        Dim n As Double = Val(TextBox2.Text)

        If n >= 0 Then
            Form._chart.Series(index:=ComboBox1.SelectedIndex).BorderWidth = CInt(n)
        End If
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        TextBox2.Text = CStr(Form._chart.Series(ComboBox1.SelectedIndex).BorderWidth)
    End Sub
End Class

#Region "Microsoft.VisualBasic::be6bc3664c09ed7c03c3a3c97d9d65a8, tutorials\Marked\FormMarkWeb.vb"

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

    '   Total Lines: 30
    '    Code Lines: 23 (76.67%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (23.33%)
    '     File Size: 1.18 KB


    ' Class FormMarkWeb
    ' 
    '     Sub: TabControl1_SelectedIndexChanged, ToolStripButton1_Click
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MIME.text.markdown

Public Class FormMarkWeb

    ReadOnly render As New MarkdownRender

    Private Sub ToolStripButton1_Click(sender As Object, e As EventArgs) Handles ToolStripButton1.Click
        Using file As New OpenFileDialog With {.Filter = "Markdown 文档(*.md)|*.md"}
            If file.ShowDialog = DialogResult.OK Then
                Dim md As String = file.FileName.ReadAllText
                Dim html As String = render.Transform(md)

                Call TextBox1.Clear()
                Call TextBox1.AppendText(md)

                Call WebViewLoader.NavigateToLargeString(WebView21, html)
            End If
        End Using
    End Sub

    Private Async Sub FormMarkWeb_Load(sender As Object, e As EventArgs) Handles Me.Load
        Await WebViewLoader.Init(WebView21)
    End Sub

    Private Sub TabControl1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles TabControl1.SelectedIndexChanged
        If TabControl1.SelectedTab Is TabPage1 Then
            Call WebViewLoader.NavigateToLargeString(WebView21, render.Transform(TextBox1.Text))
        End If
    End Sub
End Class

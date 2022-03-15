#Region "Microsoft.VisualBasic::3417689633639e994118b879a9e629ac, sciBASIC#\vs_solutions\installer\Installer\FormFinish.vb"

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

    '   Total Lines: 21
    '    Code Lines: 16
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 737.00 B


    ' Class FormFinish
    ' 
    '     Sub: ButtonNext_Click, FormFinish_Load, LinkLabel1_LinkClicked, LinkLabel2_LinkClicked
    ' 
    ' /********************************************************************************/

#End Region

Public Class FormFinish

    Protected Overrides Sub ButtonNext_Click(sender As Object, e As EventArgs)
        Close()
    End Sub

    Private Sub FormFinish_Load(sender As Object, e As EventArgs) Handles Me.Load
        ButtonNext.Text = "Close"
        LabelTitle.Text = "Success!"

        Highlight(Label3)
    End Sub

    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        Process.Start("https://github.com/xieguigang/sciBASIC")
    End Sub

    Private Sub LinkLabel2_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel2.LinkClicked
        Process.Start("http://sciBASIC.NET")
    End Sub
End Class

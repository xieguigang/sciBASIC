#Region "Microsoft.VisualBasic::0c52acd218f4ee1a5a48771e975ada4a, vs_solutions\installer\Installer\FormWelcome.vb"

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

    ' Class FormWelcome
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: ButtonNext_Click
    ' 
    ' /********************************************************************************/

#End Region

Public Class FormWelcome

    Sub New()

        ' 此调用是设计器所必需的。
        InitializeComponent()

        ' 在 InitializeComponent() 调用之后添加任何初始化。
        LabelTitle.Text = "Install sciBASIC#"

        Highlight(Label1)
    End Sub

    Protected Overrides Sub ButtonNext_Click(sender As Object, e As EventArgs)
        Call Microsoft.VisualBasic.Parallel.RunTask(AddressOf New FormProgress() With {.Location = Location}.ShowDialog)
        Me.Close()
    End Sub
End Class

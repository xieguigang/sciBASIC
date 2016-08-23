#Region "Microsoft.VisualBasic::20c1ac6be79d04c203b305cf08d780ba, ..\visualbasic_App\UXFramework\Molk+\TestProject\Form6.vb"

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

Public Class Form6
    Private Sub Form6_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Controls.Remove(Me.CheckedListBox1)
        XpPanel2.Control = Me.CheckedListBox1
    End Sub

    Private Sub JumpNavigator1_Load(sender As Object, e As EventArgs) Handles JumpNavigator1.Load
        JumpNavigator1.IndexList = (From i As Integer In 20.Sequence Select CStr(i)).ToArray


    End Sub

    Private Sub JumpNavigator1_StartNavigation(Index As String) Handles JumpNavigator1.StartNavigation
        MsgBox(Index)
    End Sub
End Class

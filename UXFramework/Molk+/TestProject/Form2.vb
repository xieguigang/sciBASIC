#Region "Microsoft.VisualBasic::2136ef6aa83abebdc48dfc2ec44ddf1c, ..\visualbasic_App\UXFramework\Molk+\TestProject\Form2.vb"

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

Public Class Form2

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        MsgBox(23)
    End Sub

    Public Shared Sub Main()
        Call (New Form2).ShowDialog()
    End Sub

    Private Sub PreviewHandle1_Load(sender As Object, e As EventArgs) Handles PreviewHandle1.Load
        ' PreviewHandle1.BackgroundImage = Image.FromFile("E:\SIYU_DNAuction\Message-Notification-UI-Box-PSD.jpg")
    End Sub

    Private Sub Ping1_Load(sender As Object, e As EventArgs) Handles Ping1.Load
        '  Ping1.IPAddress = "112.74.64.178"
        Ping1.HostName = "codeproject.com"
    End Sub

    Dim i As Integer = 1

    Private Sub Ping1_MouseClick(sender As Object, e As MouseEventArgs) Handles Ping1.MouseClick
        Call Ping1.Updates(i, 0)
        i += 1
        If i > 5 Then
            i = 1
        End If
    End Sub
End Class

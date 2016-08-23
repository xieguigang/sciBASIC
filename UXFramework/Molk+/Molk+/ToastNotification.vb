#Region "Microsoft.VisualBasic::4b9ba96096be027c3f5a11b6b373fbf4, ..\visualbasic_App\UXFramework\Molk+\Molk+\ToastNotification.vb"

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

Public Class ToastNotification

    ''' <summary>
    ''' 通过这个对象来显示打开的动画
    ''' </summary>
    ''' <returns></returns>
    Public Property InvokeDisplay As Action(Of ToastNotification)
    ''' <summary>
    ''' 通过这个对象来显示关闭的动画
    ''' </summary>
    ''' <returns></returns>
    Public Property InvokeClose As Action(Of ToastNotification)

    ''' <summary>
    ''' 显示一行提示消息
    ''' </summary>
    ''' <param name="Message"></param>
    Public Sub ShowMessage(Message As String)
        Label1.Text = Message

        If Not InvokeDisplay Is Nothing Then
            Call _InvokeDisplay(Me)
        Else
            Me.Visible = True
            BringToFront()
        End If
    End Sub

    Private Sub CloseButton_Click(sender As Object, e As EventArgs) Handles CloseButton.Click
        If Not InvokeClose Is Nothing Then
            Call _InvokeClose(Me)
        Else
            Visible = False
        End If
    End Sub

    Private Sub ToastNotification_Load(sender As Object, e As EventArgs) Handles Me.Load
        On Error Resume Next

        AddHandler Parent.Resize, AddressOf ParentResize

        CloseButton.Size = New Size(12, 12)
        CloseButton.Location = New Point(Width - CloseButton.Width - 10, (Height - CloseButton.Height) / 2.5)
        CloseButton.BringToFront()

        Visible = False
    End Sub

    Private Sub ParentResize()
        Size = New Size(Parent.Width, Height)
    End Sub

    Private Sub ToastNotification_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        Try
            CloseButton.Location = New Point(Width - CloseButton.Width - 10, (Height - CloseButton.Height) / 2.5)
            Location = New Point(0, Parent.Height - Height)
        Catch ex As Exception

        End Try
    End Sub
End Class

#Region "Microsoft.VisualBasic::fd90245704c381faba903c58438c96de, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\UserInputControl.vb"

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

Friend Class UserInputControl

    Dim _InternalHookDevice As Control
    Dim _ThemeColor As Color

    Public Property ThemeColor As Color
        Get
            Return _ThemeColor
        End Get
        Set(value As Color)
            _ThemeColor = value
            btnOK.BackColor = value
        End Set
    End Property

    Public Event InputComplete(Inputs As String)

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        Call Unload()
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        TextBox1.Text = ""
        Call Unload()
    End Sub

    Public Shared Sub Input(Hook As Control, InputHandle As Action(Of String), Location As Point, Optional Width As Integer = -1, Optional [Default] As String = "")
        Dim InputBox As New UserInputControl With {.ThemeColor = Color.AliceBlue, ._InternalHookDevice = Hook}

        Hook.Controls.Add(InputBox)
        InputBox.Location = Location
        InputBox.TextBox1.Text = [Default]
        InputBox.BringToFront()
        If Width > 0 Then
            InputBox.Size = New Size(Width, InputBox.Height)
        End If

        AddHandler InputBox.InputComplete, Sub(s As String) Call InputHandle(s)
    End Sub

    Private Sub Unload()
        Dim InputData As String = TextBox1.Text

        RaiseEvent InputComplete(InputData)

        Call _InternalHookDevice.Controls.Remove(Me)
        Call Me.Dispose()
    End Sub

    Private Sub UserInputControl_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        btnCancel.Location = New Point(Width - btnCancel.Width, 0)
        btnOK.Location = New Point(btnCancel.Location.X - 5 - btnOK.Width, 0)
        TextBox1.Location = New Point
        TextBox1.Size = New Size(Width - btnCancel.Width - btnOK.Width - 10, Height)
    End Sub
End Class

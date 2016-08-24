#Region "Microsoft.VisualBasic::2ee22b7527b36cd31381259fc6ca7009, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\WebBrowser.vb"

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

Imports System.ComponentModel

Namespace Windows.Forms.Controls

    Public Class WebBrowser

        Private Sub WebBrowser1_DocumentCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs) Handles WebBrowser1.DocumentCompleted
            TextBox1.Text = WebBrowser1.Url.ToString
        End Sub

        Private Sub Browser_Load(sender As Object, e As EventArgs) Handles Me.Load
            Call WebBrowser1.Navigate(get_HomePage)
        End Sub

        Protected MustOverride Function get_HomePage() As String

        Private Sub TextBox1_KeyDown(sender As Object, e As KeyEventArgs) Handles TextBox1.KeyDown
            If e.KeyCode = System.Windows.Forms.Keys.Enter Then
                Call WebBrowser1.Navigate(TextBox1.Text)
            End If
        End Sub

        Public Sub GotoHomePage()
            Call WebBrowser1.Navigate(get_HomePage)
        End Sub
    End Class
End Namespace

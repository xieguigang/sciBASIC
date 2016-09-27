#Region "Microsoft.VisualBasic::269b5aed828d8fff949517ff00b88ec1, ..\visualbasic_App\gr\Microsoft.VisualBasic.Imaging\Test.Project\Form1.vb"

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

Public Class Form1

    Dim WithEvents ccc As New CubeTest

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Call Me.Controls.Add(ccc)
        ccc.Dock = Windows.Forms.DockStyle.Fill
        Call ccc.Run()
    End Sub
End Class

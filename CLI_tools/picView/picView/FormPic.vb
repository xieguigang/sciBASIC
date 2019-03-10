#Region "Microsoft.VisualBasic::c00fa63613dbd737aa38935f60c478d0, CLI_tools\picView\picView\FormPic.vb"

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

    ' Class FormPic
    ' 
    '     Sub: FormPic_Load, PictureBox1_Click, UpdateImage
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Parallel.Tasks

Public Class FormPic

    Dim timer As New UpdateThread(1000, AddressOf UpdateImage)
    Dim file$

    Public Sub UpdateImage()
        Static fl&

        Dim len& = file.FileLength

        If len > 0 AndAlso len <> fl Then
            fl = len
            Dim img As Image = GDIPlusExtensions.LoadImage(file)
            PictureBox1.BackgroundImage = img
        End If
    End Sub

    Private Sub PictureBox1_Click(sender As Object, e As EventArgs) Handles PictureBox1.Click

    End Sub

    Private Sub FormPic_Load(sender As Object, e As EventArgs) Handles Me.Load
        file = App.Command
        Call timer.Start()
    End Sub
End Class

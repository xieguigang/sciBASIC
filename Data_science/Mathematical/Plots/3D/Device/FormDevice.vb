#Region "Microsoft.VisualBasic::9662ed47fc9ab4aba8020cea707a1eb4, ..\sciBASIC#\Data_science\Mathematical\Plots\3D\Device\FormDevice.vb"

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

Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.Imaging

Namespace Plot3D.Device

    Public Class FormDevice

        Protected Friend WithEvents canvas As Canvas

        Private Sub FormDevice_Load(sender As Object, e As EventArgs) Handles Me.Load
            If canvas Is Nothing Then
                canvas = New Canvas With {
                    .Dock = DockStyle.Fill
                }
            End If

            Call Controls.Add(canvas)
        End Sub

        Private Sub SavePlotToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SavePlotToolStripMenuItem.Click
            Using file As New SaveFileDialog With {
                .Filter = "PNG image(*.png)|*.png"
            }
                If file.ShowDialog = DialogResult.OK Then
                    Call canvas.BackgroundImage _
                        .SaveAs(file.FileName)
                End If
            End Using
        End Sub
    End Class
End Namespace

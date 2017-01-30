#Region "Microsoft.VisualBasic::33c678e00fcdef005fd752032b78542b, ..\sciBASIC#\gr\3DEngineTest\3DEngineTest\FormTest.vb"

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

Imports Microsoft.VisualBasic.Parallel

Public Class FormTest

    Dim WithEvents canvas As New CubeModel With {
        .Dock = DockStyle.Fill
    }

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Call Controls.Add(canvas)
        Call canvas.Run()
    End Sub

    Private Sub FormTest_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        Call RunTask(AddressOf New FormLandscape().ShowDialog)
    End Sub
End Class

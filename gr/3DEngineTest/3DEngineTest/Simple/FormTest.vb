#Region "Microsoft.VisualBasic::7c93c9ac254582e9dffc7862469be655, gr\3DEngineTest\3DEngineTest\Simple\FormTest.vb"

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

    ' Class FormTest
    ' 
    '     Sub: Form1_Load, FormTest_Closed
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.ChartPlots.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Parallel

Public Class FormTest

    Dim WithEvents canvas As New CubeModel With {
        .Dock = DockStyle.Fill
    }

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'Dim plat As New ColorPalette With {
        '    .Dock = DockStyle.Fill
        '}

        'Controls.Add(plat)

        Call Controls.Add(canvas)
        Call canvas.Run()
    End Sub

    Private Sub FormTest_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        ' Call RunTask(AddressOf New FormLandscape().ShowDialog)
    End Sub
End Class

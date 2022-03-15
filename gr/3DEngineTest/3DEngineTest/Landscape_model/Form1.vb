#Region "Microsoft.VisualBasic::317a9685350d4b7b9b7c120cb99ddc52, sciBASIC#\gr\3DEngineTest\3DEngineTest\Landscape_model\Form1.vb"

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


    ' Code Statistics:

    '   Total Lines: 17
    '    Code Lines: 12
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 512.00 B


    ' Class Form1
    ' 
    '     Sub: colors_SelectColor, Form1_Load
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.ChartPlots.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors

Public Class Form1

    Dim WithEvents colors As New ColorPalette With {.Dock = DockStyle.Fill}

    Public setColor As Action(Of Color)

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        Controls.Add(colors)
    End Sub

    Private Sub colors_SelectColor(c As Color) Handles colors.SelectColor
        Call setColor(c)
    End Sub
End Class

#Region "Microsoft.VisualBasic::153135150c90ae52fb855372928bce72, ..\sciBASIC#\Data_science\Mathematica\Math\MathApp\Form1.vb"

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

Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D.Device

Public Class Form1

    Dim WithEvents canvas As Canvas

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        canvas = New Canvas
        canvas.Dock = System.Windows.Forms.DockStyle.Fill
        Controls.Add(canvas)
    End Sub
End Class

#Region "Microsoft.VisualBasic::06c7574d3cd53920f317b43465ce1d60, sciBASIC#\vs_solutions\installer\Installer\FormTemplate.vb"

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

    '   Total Lines: 13
    '    Code Lines: 9
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 394.00 B


    ' Class FormTemplate
    ' 
    '     Sub: ButtonNext_Click, Highlight
    ' 
    ' /********************************************************************************/

#End Region

Public Class FormTemplate

    Public Shared ReadOnly TemplateColor As Color = Color.FromArgb(0, 99, 177)

    Protected Overridable Sub ButtonNext_Click(sender As Object, e As EventArgs) Handles ButtonNext.Click

    End Sub

    Public Sub Highlight(label As Label)
        label.BackColor = Color.FromArgb(34, 118, 173)
        label.ForeColor = Color.White
    End Sub
End Class

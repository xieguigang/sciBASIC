#Region "Microsoft.VisualBasic::2f88aef9468756a72c9dc77403784ffa, sciBASIC#\Data_science\Mathematica\Math\ODEsViewer\FormLoadModel.vb"

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

    '   Total Lines: 33
    '    Code Lines: 25
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 935.00 B


    ' Class FormLoadModel
    ' 
    '     Properties: DllFile, Model
    ' 
    '     Sub: Button1_Click, FormLoadModel_Load
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.Bootstrapping

Public Class FormLoadModel

    Public Property DllFile As String
    Public ReadOnly Property Model As Type

    Dim models As Type()

    Private Sub FormLoadModel_Load(sender As Object, e As EventArgs) Handles Me.Load
        models = MonteCarlo.DllParser(DllFile).ToArray

        For Each m As Type In models
            Call ListBox1.Items.Add(m.FullName)
        Next
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If ListBox1.SelectedIndex = -1 Then
            Return
        End If

        For Each m As Type In models
            If Scripting.ToString(ListBox1.SelectedItem) = m.FullName Then
                _Model = m
                Exit For
            End If
        Next

        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub
End Class

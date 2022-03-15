#Region "Microsoft.VisualBasic::cb13fe52e3d5431420790b5c48d4d372, sciBASIC#\gr\network-visualization\test\FormCanvas.vb"

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

    '   Total Lines: 70
    '    Code Lines: 55
    ' Comment Lines: 0
    '   Blank Lines: 15
    '     File Size: 2.78 KB


    ' Class FormCanvas
    ' 
    '     Sub: AutoRotateToolStripMenuItem_Click, DToolStripMenuItem_Click, Form1_Load, FormCanvas_Closed, RefreshParametersToolStripMenuItem_Click
    '          SaveAsSVGToolStripMenuItem_Click, ShowLabelsToolStripMenuItem_Click, TrackBar1_Scroll
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.Data.visualize.Network.Canvas
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Cytoscape
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.SpringForce

Public Class FormCanvas

    Dim canvas As New Canvas With {
        .Dock = DockStyle.Fill
    }

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Call Me.Controls.Add(canvas)

        canvas.Graph(True) = CytoscapeTableLoader.CytoscapeExportAsGraph(
            App.HOME & "\Resources\xcb-main-Edges.csv",
            App.HOME & "\Resources\xcb-main-Nodes.csv")

        TrackBar1.Minimum = 0
        TrackBar1.Maximum = Math.PI * 2 * 1000

        DToolStripMenuItem.Checked = True
        ShowLabelsToolStripMenuItem.Checked = False
        AutoRotateToolStripMenuItem.Checked = True
    End Sub

    Private Sub SaveAsSVGToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SaveAsSVGToolStripMenuItem.Click
        Using file As New SaveFileDialog With {.Filter = "*.svg|*.svg"}
            Call canvas.Stop()

            If file.ShowDialog = DialogResult.OK Then
                Call canvas.WriteLayout()
                Call canvas.Graph.ToSVG(New Size(1920, 1200),, DToolStripMenuItem.Checked).SaveAsXml(file.FileName)
            End If

            Call canvas.Run()
        End Using
    End Sub

    Private Sub RefreshParametersToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RefreshParametersToolStripMenuItem.Click
        Dim value As ForceDirectedArgs = Parameters.Load
        Call canvas.SetFDGParams(value)
    End Sub

    Private Sub DToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DToolStripMenuItem.Click
        If DToolStripMenuItem.Checked Then
            canvas.Graph(True) = canvas.Graph
        Else
            canvas.Graph(False) = canvas.Graph
        End If
    End Sub

    Private Sub TrackBar1_Scroll(sender As Object, e As EventArgs) Handles TrackBar1.Scroll
        canvas.ViewDistance = TrackBar1.Value
    End Sub

    Private Sub AutoRotateToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AutoRotateToolStripMenuItem.Click
        canvas.AutoRotate = AutoRotateToolStripMenuItem.Checked
    End Sub

    Private Sub ShowLabelsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ShowLabelsToolStripMenuItem.Click
        canvas.ShowLabel = ShowLabelsToolStripMenuItem.Checked
    End Sub

    Private Sub FormCanvas_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        Call App.Exit(0)
    End Sub
End Class

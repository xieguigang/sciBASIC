#Region "Microsoft.VisualBasic::7eddfa3520104f968e2221e311f526b1, ..\VisualBasic_AppFramework\Datavisualization\Datavisualization.Network\Test\FormCanvas.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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
Imports Microsoft.VisualBasic.DataVisualization.Network.Canvas
Imports Microsoft.VisualBasic.DataVisualization.Network.FileStream

Public Class FormCanvas

    Dim canvas As New Canvas With {
        .Dock = DockStyle.Fill
    }

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Call Me.Controls.Add(canvas)

        canvas.Graph(True) = CytoscapeExportAsGraph(
            App.HOME & "\Resources\xcb-main-Edges.csv",
            App.HOME & "\Resources\xcb-main-Nodes.csv")

        TrackBar1.Minimum = 0
        TrackBar1.Maximum = Math.PI * 2 * 1000

        DToolStripMenuItem.Checked = True
    End Sub

    Private Sub SaveAsSVGToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SaveAsSVGToolStripMenuItem.Click
        Using file As New SaveFileDialog With {.Filter = "*.svg|*.svg"}
            Call canvas.Stop()

            If file.ShowDialog = DialogResult.OK Then
                Call canvas.WriteLayout()
                Call canvas.Graph.ToSVG(New Size(1920, 1200)).SaveAsXml(file.FileName)
            End If

            Call canvas.Run()
        End Using
    End Sub

    Private Sub RefreshParametersToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RefreshParametersToolStripMenuItem.Click
        Dim value As ForceDirectedArgs = Config.Load
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
        canvas.SetRotate(TrackBar1.Value / 1000)
    End Sub
End Class

Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.DataVisualization.Network.Canvas
Imports Microsoft.VisualBasic.DataVisualization.Network.FileStream

Public Class Form1

    Dim canvas As New Canvas With {
        .Dock = DockStyle.Fill
    }

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Call Me.Controls.Add(canvas)

        canvas.Graph(True) = CytoscapeExportAsGraph(
            App.HOME & "\Resources\xcb-main-Edges.csv",
            App.HOME & "\Resources\xcb-main-Nodes.csv")
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
End Class

Imports System.Windows.Forms
Imports Microsoft.VisualBasic.DataVisualization.Network.Canvas
Imports Microsoft.VisualBasic.DataVisualization.Network.FileStream

Public Class Form1
    Dim canvas As New Canvas
    Sub New()

        Call Module1.test()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        Me.Controls.Add(canvas)
        canvas.Dock = DockStyle.Fill
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'canvas.Graph = CytoscapeExportAsGraph(
        '    App.HOME & "\Resources\Edges.csv",
        '    App.HOME & "\Resources\Nodes.csv")
        'Try
        canvas.Graph = CytoscapeExportAsGraph(
         "F:\VisualBasic_AppFramework\Datavisualization\Datavisualization.Network\net_test\xcb\Edges.csv",
         "F:\VisualBasic_AppFramework\Datavisualization\Datavisualization.Network\net_test\xcb\Nodes.csv")
        '  Catch ex As Exception
        '   Call App.LogException(ex)
        '   End Try
    End Sub
End Class

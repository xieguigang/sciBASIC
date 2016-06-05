Imports System.Windows.Forms
Imports Microsoft.VisualBasic.DataVisualization.Network.Canvas
Imports Microsoft.VisualBasic.DataVisualization.Network.FileStream

Public Class Form1
    Dim car As New Canvas
    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        Me.Controls.Add(car)
        car.Dock = DockStyle.Fill
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        car.Graph = CytoscapeExportAsGraph(
            "F:\VisualBasic_AppFramework\Datavisualization\Datavisualization.Network\net_test\Edges.csv",
            "F:\VisualBasic_AppFramework\Datavisualization\Datavisualization.Network\net_test\Nodes.csv")
    End Sub
End Class

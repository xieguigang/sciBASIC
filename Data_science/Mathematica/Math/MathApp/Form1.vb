#Region "Microsoft.VisualBasic::153135150c90ae52fb855372928bce72, Data_science\Mathematica\Math\MathApp\Form1.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Class Form1
    ' 
    '     Sub: Form1_Load
    ' 
    ' /********************************************************************************/

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

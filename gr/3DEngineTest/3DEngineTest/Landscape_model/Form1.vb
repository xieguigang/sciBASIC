#Region "Microsoft.VisualBasic::e932e7632410c08c1e4809d7e38863fe, gr\3DEngineTest\3DEngineTest\Landscape_model\Form1.vb"

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
    '     Sub: colors_SelectColor, Form1_Load
    ' 
    ' /********************************************************************************/

#End Region

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

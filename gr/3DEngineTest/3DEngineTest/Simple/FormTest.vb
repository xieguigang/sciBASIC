#Region "Microsoft.VisualBasic::5a18efd5aee3a62b8bdbf54541afcda8, gr\3DEngineTest\3DEngineTest\Simple\FormTest.vb"

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

    ' Class FormTest
    ' 
    '     Sub: Form1_Load, FormTest_Closed
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Parallel

Public Class FormTest

    Dim WithEvents canvas As New CubeModel With {
        .Dock = DockStyle.Fill
    }

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim plat As New ColorPalette With {
            .Dock = DockStyle.Fill
        }

        Controls.Add(plat)

        Call Controls.Add(canvas)
        Call canvas.Run()
    End Sub

    Private Sub FormTest_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        Call RunTask(AddressOf New FormLandscape().ShowDialog)
    End Sub
End Class

#Region "Microsoft.VisualBasic::e7dbe11251c9a071d384a0aed9c611e4, Data_science\Visualization\Plots\3D\Device\FormDevice.vb"

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

    '     Class FormDevice
    ' 
    '         Sub: FormDevice_Load, SavePlotToolStripMenuItem_Click
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.Imaging

Namespace Plot3D.Device

    Public Class FormDevice

        Protected Friend WithEvents canvas As Canvas

        Private Sub FormDevice_Load(sender As Object, e As EventArgs) Handles Me.Load
            If canvas Is Nothing Then
                canvas = New Canvas With {
                    .Dock = DockStyle.Fill
                }
            End If

            Call Controls.Add(canvas)
        End Sub

        Private Sub SavePlotToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SavePlotToolStripMenuItem.Click
            Using file As New SaveFileDialog With {
                .Filter = "PNG image(*.png)|*.png"
            }
                If file.ShowDialog = DialogResult.OK Then
                    Call canvas.BackgroundImage _
                        .SaveAs(file.FileName)
                End If
            End Using
        End Sub
    End Class
End Namespace

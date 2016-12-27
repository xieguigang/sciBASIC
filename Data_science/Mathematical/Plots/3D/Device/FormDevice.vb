Imports System.Windows.Forms

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
    End Class
End Namespace
Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.ComponentModel
Imports ColorsPalette = Microsoft.VisualBasic.Language.List(Of Microsoft.VisualBasic.ComponentModel.Map(Of System.Drawing.Rectangle, System.Drawing.Color))


Namespace Drawing2D.Colors

    Public Class ColorPalette

        Dim level1Colors As New ColorsPalette
        Dim level2Colors As New ColorsPalette

        Dim n = 20
        Dim half!

        Private Sub ColorPalette_Load(sender As Object, e As EventArgs) Handles Me.Load
            Dim dw! = Width / n
            Dim dh! = Height / 2 - 10
            Dim colors As Color() = {}

            For i As Integer = 0 To n - 1
                level1Colors += New Map(Of Rectangle, Color) With {
                    .Key = New Rectangle With {
                        .X = i * dw,
                        .Y = 10,
                        .Width = dw,
                        .Height = dh
                    },
                    .Maps = colors(i)
                }
            Next

            half = Height / 2
        End Sub

        Private Sub ColorPalette_MouseClick(sender As Object, e As MouseEventArgs) Handles Me.MouseClick
            If e.Location.Y > half Then
                ' 选二级颜色
            Else
                ' 设置一级颜色

            End If
        End Sub

        Private Sub ColorPalette_Resize(sender As Object, e As EventArgs) Handles Me.Resize
            half = Height / 2
        End Sub
    End Class
End Namespace
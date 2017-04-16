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

            Call Me.Invalidate()
        End Sub

        Private Sub ColorPalette_MouseClick(sender As Object, e As MouseEventArgs) Handles Me.MouseClick
            If e.Location.Y > half Then
                ' 选二级颜色
            Else
                ' 设置一级颜色

            End If

            ' 进行界面刷新
            Call Me.Invalidate()
        End Sub

        Private Sub ColorPalette_Resize(sender As Object, e As EventArgs) Handles Me.Resize
            half = Height / 2
            Call Me.Invalidate()
        End Sub

        ''' <summary>
        ''' 大小或者选择了新颜色之后需要重新生成模型
        ''' </summary>
        Private Sub __colorsPaletteModels()

        End Sub

        ''' <summary>
        ''' 大小改变了之后或者选择了新的颜色之后在这里进行界面重绘
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Private Sub ColorPalette_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint

        End Sub
    End Class
End Namespace
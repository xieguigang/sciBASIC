Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports ColorsPalette = Microsoft.VisualBasic.ComponentModel.Map(Of System.Drawing.Rectangle, System.Drawing.Color)

Namespace Drawing2D.Colors

    Public Class ColorPalette

        Dim level1Colors As ColorsPalette()
        Dim level2Colors As ColorsPalette()
        Dim half!
        Dim current% = 0
        Dim index As SeqValue(Of IntRange)()

        Public Event SelectColor(c As Color)

        Private Sub ColorPalette_Load(sender As Object, e As EventArgs) Handles Me.Load
            Call __colorsPaletteModels()
            Call Me.Invalidate()
        End Sub

        Private Sub ColorPalette_MouseClick(sender As Object, e As MouseEventArgs) Handles Me.MouseClick
            Dim ly = e.Location.Y
            Dim i As Integer = index _
                .Where(Function(x) (+x).IsInside(e.X)) _
                .Select(Function(x) x.i) _
                .DefaultFirst(-1)
#If DEBUG Then
            ParentForm.Text = $"pointer={e.Location.GetJson}, index={i}"
#End If
            If i = -1 OrElse ly < 10 OrElse (ly > half AndAlso ly < half + 10) Then
                Return
            End If

            If ly > half + 10 Then
                ' 选二级颜色
                RaiseEvent SelectColor(level2Colors(i).Maps)
            ElseIf ly > 10 Then
                ' 设置一级颜色
                current = i
                ' 进行界面刷新
                Call ColorPaletteRInit()
            Else
            End If
        End Sub

        Private Sub ColorPaletteRInit() Handles Me.Resize
            Call __colorsPaletteModels()
            Call Me.Invalidate()
        End Sub

        ''' <summary>
        ''' 大小或者选择了新颜色之后需要重新生成模型
        ''' </summary>
        Private Sub __colorsPaletteModels()
            Dim n = Designer.TSF.Length
            Dim dw! = Width / n
            Dim dh! = Height / 2 - 10
            Dim y = 10
            Dim colors As Color() = Designer.TSF
            Dim getColors =
                Function() As ColorsPalette()
                    Dim out As New List(Of ColorsPalette)

                    For i As Integer = 0 To n - 1
                        out += New Map(Of Rectangle, Color) With {
                            .Key = New Rectangle With {
                                .X = i * dw,
                                .Y = y,
                                .Width = dw,
                                .Height = dh
                            },
                            .Maps = colors(i)
                        }
                    Next

                    Return out
                End Function

            level1Colors = getColors()
            half = Height / 2

            Dim c As Color()

            If current = 0 Then
                c = {
                    colors.Last,
                    colors(0),
                    colors(1)
                }
            ElseIf current = colors.Length - 1 Then
                c = {
                    colors(colors.Length - 2),
                    colors.Last,
                    colors(0)
                }
            Else
                c = {
                    colors(current - 1),
                    colors(current),
                    colors(current + 1)
                }
            End If

            colors = Designer.CubicSpline(c, n)
            y = half + 10
            level2Colors = getColors()
            index = level2Colors _
                .SeqIterator _
                .Select(Function(i)
                            Dim r = +i
                            Return New SeqValue(Of IntRange) With {
                                .i = i,
                                .value = New IntRange(r.Key.Left, r.Key.Right)
                            }
                        End Function) _
                .ToArray
        End Sub

        ''' <summary>
        ''' 大小改变了之后或者选择了新的颜色之后在这里进行界面重绘
        ''' </summary>
        ''' <param name="e"></param>
        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            Dim g As Graphics = e.Graphics

            For Each block As Map(Of Rectangle, Color) In level1Colors.JoinIterates(level2Colors)
                Dim b As New SolidBrush(block.Maps)
                Call g.FillRectangle(b, block.Key)
            Next
        End Sub
    End Class
End Namespace
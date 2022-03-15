#Region "Microsoft.VisualBasic::af521f6975d7c21269ce0e2f77787740, sciBASIC#\Data_science\Visualization\Canvas3D\ColorPalette.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 153
    '    Code Lines: 119
    ' Comment Lines: 14
    '   Blank Lines: 20
    '     File Size: 5.46 KB


    '     Class ColorPalette
    ' 
    '         Sub: __colorsPaletteModels, ColorPalette_Load, ColorPalette_MouseClick, ColorPaletteRInit, OnPaint
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
#If debug Then
Imports Microsoft.VisualBasic.Serialization.JSON
#End If
Imports ColorsPalette = Microsoft.VisualBasic.ComponentModel.Map(Of System.Drawing.Rectangle, System.Drawing.Color)

Namespace Drawing2D.Colors

    Public Class ColorPalette

        Dim level1Colors As ColorsPalette()
        Dim level2Colors As ColorsPalette()
        Dim half!
        Dim current1% = 0
        Dim current2% = 0
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
                current2 = i
                RaiseEvent SelectColor(level2Colors(i).Maps)
            ElseIf ly > 10 Then
                ' 设置一级颜色
                current1 = i
            Else
                Return
            End If

            ' 进行界面刷新
            Call ColorPaletteRInit()
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

            If current1 = 0 Then
                c = {
                    colors.Last,
                    colors(0),
                    colors(1)
                }
            ElseIf current1 = colors.Length - 1 Then
                c = {
                    colors(colors.Length - 2),
                    colors.Last,
                    colors(0)
                }
            Else
                c = {
                    colors(current1 - 1),
                    colors(current1),
                    colors(current1 + 1)
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
            Dim p As New Pen(Color.White, 1.5)
            Dim size As New Size(Width / level1Colors.Length, 10)

            For Each block As Map(Of Rectangle, Color) In level1Colors.JoinIterates(level2Colors)
                Dim b As New SolidBrush(block.Maps)
                Call g.FillRectangle(b, block.Key)
            Next

            ' Dim rect As New Rectangle(New Point(level1Colors(current1).Key.X, 0), size)
            'Call g.FillRectangle(Brushes.Black, rect)
            Call g.DrawRectangle(p, level1Colors(current1).Key)

            'rect = New Rectangle(New Point(level2Colors(current2).Key.X, half), size)
            ' Call g.FillRectangle(Brushes.Black, rect)
            Call g.DrawRectangle(p, level2Colors(current2).Key)
        End Sub
    End Class
End Namespace

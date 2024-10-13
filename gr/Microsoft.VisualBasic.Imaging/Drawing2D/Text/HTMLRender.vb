#Region "Microsoft.VisualBasic::7c02bf10c0a3590d07d96e8db9bdfaff, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Text\HTMLRender.vb"

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

    '   Total Lines: 179
    '    Code Lines: 134 (74.86%)
    ' Comment Lines: 21 (11.73%)
    '    - Xml Docs: 66.67%
    ' 
    '   Blank Lines: 24 (13.41%)
    '     File Size: 6.79 KB


    '     Module HTMLRender
    ' 
    '         Function: drawNormal, drawSub, drawSup, MeasureSize
    ' 
    '         Sub: (+2 Overloads) DrawHtmlString, RenderHTML
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
'Imports Microsoft.VisualBasic.Drawing.Drawing2D.Text
Imports Microsoft.VisualBasic.MIME.Html.Render

Namespace Drawing2D.Text

    ''' <summary>
    ''' 进行简单的HTML片段的渲染
    ''' </summary>
    Public Module HTMLRender

        <Extension>
        Public Sub DrawHtmlString(g As IGraphics, text$, baseFontStyle As Font, defaultColor As Color, location As Point)
            Dim tokens As TextString() = TextAPI _
                .TryParse(text, baseFontStyle, defaultColor) _
                .ToArray

            Call g.RenderHTML(tokens, New PointF(location.X, location.Y))
        End Sub

        <Extension>
        Public Sub DrawHtmlString(g As IGraphics, text$, baseFontStyle As Font, defaultColor As Color, location As PointF)
            Dim tokens As TextString() = TextAPI _
                .TryParse(text, baseFontStyle, defaultColor) _
                .ToArray

            Call g.RenderHTML(tokens, location)
        End Sub

        ''' <summary>
        ''' 估算出文本的绘制区域大小
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="html"></param>
        ''' <returns></returns>
        <Extension>
        Public Function MeasureSize(g As IGraphics, html As TextString()) As SizeF
            Dim size As SizeF
            Dim dW%, dh%

            For Each fragment As TextString In html
                Select Case fragment.weight
                    Case TextString.WeightStyles.sub, TextString.WeightStyles.sup
                        dW = g.MeasureString(fragment, fragment.GetWeightedFont).Width
                        size = New SizeF With {
                            .Height = size.Height,
                            .Width = size.Width + dW
                        }
                    Case Else
                        Dim lines$() = fragment.text.LineTokens
                        Dim n% = lines.Length
                        Dim font As Font = fragment.font

                        ' n等于零,则不变
                        If n = 1 Then
                            With g.MeasureString(lines(Scan0), font)
                                dW = .Width
                                dh = .Height
                            End With

                            size = New Size With {
                                .Width = size.Width + dW,
                                .Height = {size.Height, dh}.Max
                            }
                        ElseIf n > 1 Then
                            dh = g.MeasureString(lines(Scan0), font).Height
                            dW = lines _
                                .Select(Function(l)
                                            Return g.MeasureString(l, font).Width
                                        End Function) _
                                .Max
                            size = New SizeF With {
                                .Width = {dW, size.Width}.Max,
                                .Height = size.Height + dh * n
                            }
                        End If
                End Select
            Next

            Return size
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="html"></param>
        ''' <param name="topleft">程序会以这个位置为原点进行布局的计算操作</param>
        <Extension>
        Public Sub RenderHTML(g As IGraphics, html As TextString(), Optional topleft As PointF = Nothing)
            Dim size As SizeF

            For Each fragment As TextString In html

                ' 已经在这里处理了topleft的更新了
                Select Case fragment.weight
                    Case TextString.WeightStyles.sub
                        size = g.drawSub(fragment, topleft)
                    Case TextString.WeightStyles.sup
                        size = g.drawSup(fragment, topleft)
                    Case Else
                        size = g.drawNormal(fragment, topleft)
                End Select
            Next
        End Sub

        <Extension>
        Private Function drawNormal(g As IGraphics, str As TextString, ByRef topleft As PointF) As SizeF
            Dim font As Font = str.font
            Dim color As New SolidBrush(str.color.TranslateColor)
            Dim size As SizeF
            Dim hasLines As Integer = 0
            Dim offsetY%
            Dim p As PointF

            For Each line As String In str.text.LineTokens
                size = g.MeasureString(line, font)
                p = New PointF With {
                    .X = topleft.X,
                    .Y = topleft.Y + offsetY
                }
                g.DrawString(line, font, color, p)
                hasLines += 1
                offsetY += size.Height
            Next

            If hasLines = 1 Then
                ' 没有换行, 则
                ' X前进字符串的宽度
                ' Y不变
                topleft = New PointF With {
                    .X = topleft.X + size.Width,
                    .Y = topleft.Y
                }
            End If

            Return g.MeasureString(str, font)
        End Function

        <Extension>
        Private Function drawSub(g As IGraphics, text As TextString, ByRef topleft As PointF) As SizeF
            Dim font As Font = text.GetWeightedFont
            Dim size As SizeF = g.MeasureString(text.text, font)
            Dim color As New SolidBrush(text.color.TranslateColor)

            g.DrawString(
                text, font, color, New PointF With {
                    .X = topleft.X,
                    .Y = topleft.Y + size.Height
            })
            topleft = New PointF With {
                .X = topleft.X + size.Width,
                .Y = topleft.Y
            }

            Return size
        End Function

        <Extension>
        Private Function drawSup(g As IGraphics, text As TextString, ByRef topleft As PointF) As SizeF
            Dim font As Font = text.GetWeightedFont
            Dim size As SizeF = g.MeasureString(text.text, font)
            Dim color As New SolidBrush(text.color.TranslateColor)

            g.DrawString(
                text, font, color, New PointF With {
                    .X = topleft.X,
                    .Y = topleft.Y
            })
            topleft = New PointF With {
                .X = topleft.X + size.Width,
                .Y = topleft.Y
            }

            Return size
        End Function
    End Module
End Namespace

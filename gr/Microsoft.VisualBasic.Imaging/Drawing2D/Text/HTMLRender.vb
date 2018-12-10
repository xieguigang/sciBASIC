Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.Markup.HTML

Namespace Drawing2D.Text

    ''' <summary>
    ''' 进行简单的HTML片段的渲染
    ''' </summary>
    Public Module HTMLRender

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
        Public Sub RenderHTML(g As IGraphics, html As TextString(), Optional topleft As Point = Nothing)
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
        Private Function drawNormal(g As IGraphics, str As TextString, ByRef topleft As Point) As SizeF
            Dim font As Font = str.font
            Dim color As New SolidBrush(str.color.TranslateColor)
            Dim size As SizeF
            Dim hasLines As Integer = 0
            Dim offsetY%
            Dim p As Point

            For Each line As String In str.text.LineTokens
                size = g.MeasureString(line, font)
                p = New Point With {
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
                topleft = New Point With {
                    .X = topleft.X + size.Width,
                    .Y = topleft.Y
                }
            End If

            Return g.MeasureString(str, font)
        End Function

        <Extension>
        Private Function drawSub(g As IGraphics, text As TextString, ByRef topleft As Point) As SizeF
            Dim font As Font = text.GetWeightedFont
            Dim size As SizeF = g.MeasureString(text.text, font)
            Dim color As New SolidBrush(text.color.TranslateColor)

            g.DrawString(
                text, font, color, New Point With {
                    .X = topleft.X,
                    .Y = topleft.Y + size.Height
            })
            topleft = New Point With {
                .X = topleft.X + size.Width,
                .Y = topleft.Y
            }

            Return size
        End Function

        <Extension>
        Private Function drawSup(g As IGraphics, text As TextString, ByRef topleft As Point) As SizeF
            Dim font As Font = text.GetWeightedFont
            Dim size As SizeF = g.MeasureString(text.text, font)
            Dim color As New SolidBrush(text.color.TranslateColor)

            g.DrawString(
                text, font, color, New Point With {
                    .X = topleft.X,
                    .Y = topleft.Y
            })
            topleft = New Point With {
                .X = topleft.X + size.Width,
                .Y = topleft.Y
            }

            Return size
        End Function
    End Module
End Namespace
Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace Drawing2D.Vector.Text

    Public Class FontMetrics

        Public ReadOnly Property Font As Font
        ''' <summary>
        ''' The default gdi+ graphics context
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Graphics As Graphics
        ''' <summary>
        ''' 在当前的字体条件下面的，使用默认的<see cref="Graphics"/>上下文的文本行高
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Height As Single

        Sub New(font As Font, g As Graphics)
            Me.Font = font
            Height = g.MeasureString("1", font).Height
        End Sub

        Sub New(font As CSSFont, g As Graphics)
            Me.New(font.GDIObject, g)
        End Sub

        ''' <summary>
        ''' Using another graphics context
        ''' </summary>
        ''' <param name="s"></param>
        ''' <param name="g"></param>
        ''' <returns></returns>
        Public Function GetStringBounds(s As String, g As Graphics) As RectangleF
            Return New RectangleF(New Point, g.MeasureString(s, Font))
        End Function

        Public Function GetStringBounds(s As String) As RectangleF
            Return GetStringBounds(s, Graphics)
        End Function

        Public Shared Narrowing Operator CType(f As FontMetrics) As Font
            Return f.Font
        End Operator
    End Class

    Public Module Extensions

        <Extension>
        Public Function FontMetrics(g As Graphics2D) As FontMetrics
            Return New FontMetrics(g.Font, g.Graphics)
        End Function
    End Module
End Namespace
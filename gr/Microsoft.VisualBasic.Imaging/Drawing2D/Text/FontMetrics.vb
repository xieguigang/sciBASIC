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

        Public ReadOnly Property Height As Single

        Sub New(font As Font, g As Graphics)
            Me.Font = font
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
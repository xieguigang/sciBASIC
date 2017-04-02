Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace Drawing2D.Vector.Text

    Public Class FontMetrics

        Public ReadOnly Property Font As Font
        Public ReadOnly Property Graphics As Graphics

        Public ReadOnly Property Height As Single

        Sub New(font As Font)
            Me.Font = font
        End Sub

        Sub New(font As CSSFont)
            Me.New(font.GDIObject)
        End Sub

        Public Function GetStringBounds(s As String, g As Graphics) As RectangleF
            Throw New NotImplementedException()
        End Function

        Public Shared Narrowing Operator CType(f As FontMetrics) As Font
            Return f.Font
        End Operator
    End Class

    Public Module Extensions

        <Extension>
        Public Function FontMetrics(g As Graphics) As FontMetrics

        End Function
    End Module
End Namespace
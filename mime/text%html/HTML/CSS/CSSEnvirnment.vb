Imports System.Drawing

Namespace HTML.CSS

    Public Class CSSEnvirnment

        Public ReadOnly Property baseFont As Font
        Public ReadOnly Property baseLine As Pen

        Sub New(basefont As Font, baseline As Pen)
            Me.baseFont = basefont
            Me.baseLine = baseline
        End Sub

        Public Function GetFontByScale(em As Single) As Font
            Dim newSize As Single = em * baseFont.Size
            Dim newFont As New Font(baseFont.FontFamily, newSize, baseFont.Style)

            Return newFont
        End Function

    End Class
End Namespace
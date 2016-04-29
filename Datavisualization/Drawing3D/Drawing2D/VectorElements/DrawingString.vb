Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

Namespace Drawing2D.VectorElements

    Public Class DrawingString : Inherits VectorObject

        Public Property Text As String
        Public Property Pen As Brush
        Public Property Font As Font

        Sub New(locat As Point, size As Size)
            Call MyBase.New(locat, size)
        End Sub

        Sub New(rect As Rectangle)
            Call MyBase.New(rect)
        End Sub

        Public Overrides Function ToString() As String
            Return Text
        End Function

        Public Overrides Sub Draw(gdi As GDIPlusDeviceHandle)
            Call gdi.DrawString(Text, Font, Pen, New RectangleF(RECT.X, RECT.Y, RECT.Width, RECT.Height))
        End Sub
    End Class
End Namespace
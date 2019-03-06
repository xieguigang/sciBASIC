Imports RectangleF = Microsoft.VisualBasic.Imaging.LayoutModel.Rectangle2D

Namespace Layouts.Cola

    Public Class Rectangle2D : Inherits RectangleF

        Public Property space_left As Double

        Sub New(x1#, x2#, y1#, y2#)
            Call MyBase.New(x1, x2, y1, y2)
        End Sub

        Sub New()
            Call MyBase.New
        End Sub
    End Class
End Namespace
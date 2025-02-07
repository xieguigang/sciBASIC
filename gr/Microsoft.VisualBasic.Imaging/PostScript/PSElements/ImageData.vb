Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Net.Http

Namespace PostScript.Elements

    Public Class ImageData : Inherits PSElement

        ''' <summary>
        ''' image data is encoded as base64 data uri
        ''' </summary>
        ''' <returns></returns>
        Public Property image As DataURI
        Public Property size As Size
        Public Property scale As SizeF
        Public Property location As PointF

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Overrides Sub WriteAscii(ps As Writer)
            Call ps.image(image, location.X, location.Y, size.Width, size.Height, scale.Width, scale.Height)
        End Sub

        Friend Overrides Sub Paint(g As IGraphics)
            Throw New NotImplementedException()
        End Sub
    End Class
End Namespace
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Shapes
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class Legend

    Public Property style As LegendStyles
    Public Property title As String
    Public Property color As String
    ''' <summary>
    ''' CSS expression, which can be parsing by <see cref="CSSFont"/> 
    ''' </summary>
    ''' <returns></returns>
    Public Property fontstyle As String

    Public Function GetFont() As Font
        Return CSSFont.TryParse(fontstyle).GDIObject
    End Function

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class

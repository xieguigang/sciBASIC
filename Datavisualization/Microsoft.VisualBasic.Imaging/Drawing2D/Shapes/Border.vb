Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Drawing2D.Vector.Shapes

    ''' <summary>
    ''' 边对象
    ''' </summary>
    Public Class Border

        Public Property width As Single
        Public Property color As Color
        Public Property style As DashStyle

        Public Function GetPen() As Pen
            Return New Pen(color, width) With {
                .DashStyle = style
            }
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace
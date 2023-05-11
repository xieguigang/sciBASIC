
Imports System.Drawing
Imports System.Xml.Serialization

Namespace SVG.XML

    ''' <summary>
    ''' 矩形对象
    ''' </summary>
    Public Class rect : Inherits node

        <XmlAttribute> Public Property height As String
        <XmlAttribute> Public Property width As String
        <XmlAttribute> Public Property y As String
        <XmlAttribute> Public Property x As String

#Region "圆角矩形"
        Public Property rx As String
        Public Property ry As String
#End Region

        Sub New()
        End Sub

        Sub New(rect As RectangleF)
            With Me
                .width = rect.Width
                .height = rect.Height
                .x = rect.X
                .y = rect.Y
            End With
        End Sub

        Sub New(rect As Rectangle)
            With Me
                .width = rect.Width
                .height = rect.Height
                .x = rect.X
                .y = rect.Y
            End With
        End Sub

        Public Shared Operator +(rect As rect, offset As PointF) As rect
            rect = DirectCast(rect.MemberwiseClone(), rect)
            rect.x += offset.X
            rect.y += offset.Y
            Return rect
        End Operator
    End Class

End Namespace
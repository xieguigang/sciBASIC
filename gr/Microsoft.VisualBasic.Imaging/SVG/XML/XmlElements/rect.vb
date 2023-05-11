
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

        ''' <summary>
        ''' Contructs a rectangle with the specified width and height.
        ''' </summary>
        ''' <param name="width">rectangle width</param>
        ''' <param name="height">rectangle height</param>
        Public Sub New(width As Double, height As Double)
            Me.width = width
            Me.height = height
        End Sub

        ''' <summary>
        ''' Contructs a rectangle with the specified width and height at the given position.
        ''' </summary>
        ''' <param name="x">left top corner X-coordinate</param>
        ''' <param name="y">left top corner Y-coordinate</param>
        ''' <param name="width">rectangle width</param>
        ''' <param name="height">rectangle height</param>
        Public Sub New(x As Double, y As Double, width As Double, height As Double)
            Me.x = x
            Me.y = y
            Me.width = width
            Me.height = height
        End Sub

        Public Shared Operator +(rect As rect, offset As PointF) As rect
            rect = DirectCast(rect.MemberwiseClone(), rect)
            rect.x += offset.X
            rect.y += offset.Y
            Return rect
        End Operator
    End Class

End Namespace
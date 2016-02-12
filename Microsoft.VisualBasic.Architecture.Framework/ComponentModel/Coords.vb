Imports System.Drawing
Imports System.Xml.Serialization

Namespace ComponentModel

    Public Class Coords

        <XmlAttribute("x")> Public Property X As Integer
        <XmlAttribute("y")> Public Property Y As Integer

        Sub New()
        End Sub

        Sub New(pt As Point)
            Call Me.New(pt.X, pt.Y)
        End Sub

        Sub New(x As Integer, y As Integer)
            Me.X = x
            Me.Y = y
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{X}, {Y}]"
        End Function

        Public Shared Widening Operator CType(pt As Point) As Coords
            Return New Coords With {.X = pt.X, .Y = pt.Y}
        End Operator

        Public Shared Narrowing Operator CType(x As Coords) As Point
            Return New Point(x.X, x.Y)
        End Operator

        Public Shared Widening Operator CType(pt As Integer()) As Coords
            Return New Coords(pt.FirstOrDefault, pt.LastOrDefault)
        End Operator
    End Class
End Namespace
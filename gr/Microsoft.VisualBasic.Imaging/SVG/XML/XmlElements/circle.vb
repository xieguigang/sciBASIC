Imports System.Drawing
Imports System.Xml.Serialization

Namespace SVG.XML

    Public Class circle : Inherits node

        <XmlAttribute> Public Property cy As Single
        <XmlAttribute> Public Property cx As Single
        <XmlAttribute> Public Property r As Single

        Public Property title As title


        Public Shared Operator +(c As circle, offset As PointF) As circle
            c = DirectCast(c.MemberwiseClone, circle)
            c.cx += offset.X
            c.cy += offset.Y
            Return c
        End Operator
    End Class
End Namespace
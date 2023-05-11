
Imports System.Drawing
Imports System.Xml.Serialization

Namespace SVG.XML

    ''' <summary>
    ''' 一个线段对象
    ''' </summary>
    Public Class line : Inherits node

        <XmlAttribute> Public Property y2 As Single
        <XmlAttribute> Public Property x2 As Single
        <XmlAttribute> Public Property y1 As Single
        <XmlAttribute> Public Property x1 As Single

        Public Shared Operator +(line As line, offset As PointF) As line
            line = DirectCast(line.MemberwiseClone, line)

            With line
                .x1 += offset.X
                .x2 += offset.X
                .y1 += offset.Y
                .y2 += offset.Y

                Return line
            End With
        End Operator
    End Class

End Namespace

Namespace SVG.XML

    Public Class polyline : Inherits node

        <XmlAttribute> Public Property points As String()
        <XmlAttribute("marker-end")>
        Public Property markerEnd As String

        Public Overrides Function ToString() As String
            Return points.JoinBy(" ")
        End Function

        Public Shared Operator +(line As polyline, offset As PointF) As polyline
            ' Throw New NotImplementedException
            Return Nothing
        End Operator
    End Class
End Namespace
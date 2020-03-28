Imports System.Xml.Serialization

<XmlType("math", Namespace:="http://www.w3.org/1998/Math/MathML")>
Public Class Math

    <XmlElement("apply")>
    Public Property applyValue As Apply
    Public Property lambda As lambda

End Class

Public Class Apply : Inherits symbols

    Public Property divide As mathOperator
    Public Property times As mathOperator
    Public Property plus As mathOperator

    Public Property cn As constant

    <XmlElement("apply")>
    Public Property apply As Apply()

    Public ReadOnly Property [operator] As String
        Get
            If Not divide Is Nothing Then
                Return "/"
            ElseIf Not times Is Nothing Then
                Return "*"
            ElseIf Not plus Is Nothing Then
                Return "+"
            Else
                Return "-"
            End If
        End Get
    End Property

    Public Overrides Function ToString() As String
        If ci.IsNullOrEmpty Then
            Return [operator] & $"( {apply.JoinBy(" ")} )"
        ElseIf ci.Length = 1 Then
            Return $"{[operator]} {ci(Scan0)}"
        ElseIf ci.Length = 2 Then
            Return $"({ci(0)} {[operator]} {ci(1)})"
        Else
            Return "invalid"
        End If
    End Function

End Class

Public Class mathOperator
End Class

Public Class constant

    <XmlAttribute>
    Public Property type As String
    <XmlText>
    Public Property value As String

    Public Overrides Function ToString() As String
        Return $"({type}) {value}"
    End Function

End Class

Public Class lambda

    <XmlElement("bvar")>
    Public Property bvar As symbols()
    Public Property apply As Apply
End Class

Public Class symbols

    <XmlElement("ci")>
    Public Property ci As String()
End Class
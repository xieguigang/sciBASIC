Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Map

    Public Class Style
        <XmlAttribute> Public Property id As String
        Public Property IconStyle As IconStyle
        Public Property LabelStyle As LabelStyle

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Class StyleMap
        <XmlAttribute> Public Property id As String
        <XmlElement("Pair")> Public Property Pairs As Pair()

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Class Pair
        Public Property key As String
        Public Property styleUrl As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Class LabelStyle
        Public Property scale As Double

        Public Overrides Function ToString() As String
            Return scale
        End Function
    End Class

    Public Class IconStyle
        Public Property color As String
        Public Property scale As Double
        Public Property Icon As Link
        Public Property hotSpot As hotSpot

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Class hotSpot
        <XmlAttribute> Public Property x As Double
        <XmlAttribute> Public Property y As Double
        <XmlAttribute> Public Property xunits As String
        <XmlAttribute> Public Property yunits As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Class Link

        Public Property href As String

        Public Overrides Function ToString() As String
            Return href
        End Function
    End Class
End Namespace
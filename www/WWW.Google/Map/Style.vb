#Region "Microsoft.VisualBasic::d19aea302410f605b59a8c09be4b64ec, www\WWW.Google\Map\Style.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class Style
    ' 
    '         Properties: IconStyle, id, LabelStyle
    ' 
    '         Function: ToString
    ' 
    '     Class StyleMap
    ' 
    '         Properties: id, Pairs
    ' 
    '         Function: ToString
    ' 
    '     Class Pair
    ' 
    '         Properties: key, styleUrl
    ' 
    '         Function: ToString
    ' 
    '     Class LabelStyle
    ' 
    '         Properties: scale
    ' 
    '         Function: ToString
    ' 
    '     Class IconStyle
    ' 
    '         Properties: color, hotSpot, Icon, scale
    ' 
    '         Function: ToString
    ' 
    '     Class hotSpot
    ' 
    '         Properties: x, xunits, y, yunits
    ' 
    '         Function: ToString
    ' 
    '     Class Link
    ' 
    '         Properties: href
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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

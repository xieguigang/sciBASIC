Imports System.Xml.Serialization

Namespace HTML.CSS

    Public Interface CSSLayer

        ''' <summary>
        ''' Drawing order, if this index value is greater, then it will be draw on the top most.
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute("z-index")>
        Property zIndex As Integer

    End Interface
End Namespace